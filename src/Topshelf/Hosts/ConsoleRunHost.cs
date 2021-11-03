// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Topshelf.Exceptions;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class ConsoleRunHost : IHost, IHostControl, IDisposable
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogWriter _log = HostLogger.Get<ConsoleRunHost>();
        private readonly IServiceHandle _serviceHandle;
        private readonly IHostSettings _settings;
        private int _deadThread;
        private bool _disposed;
        private ManualResetEvent? _exit;

        private TopshelfExitCode _exitCode;

        private volatile bool _hasCancelled;

        public ConsoleRunHost(IHostSettings settings, IHostEnvironment environment, IServiceHandle serviceHandle)
        {
            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;

            if (settings.CanSessionChanged)
            {
                SystemEvents.SessionSwitch += OnSessionChanged;
            }

            if (settings.CanHandlePowerEvent)
            {
                SystemEvents.PowerModeChanged += OnPowerModeChanged;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IHostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _environment.IsServiceInstalled(_settings.ServiceName) && !_environment.IsServiceStopped(_settings.ServiceName))
            {
                _log.ErrorFormat(CultureInfo.CurrentCulture,
                    "The {0} service is running and must be stopped before running via the console",
                    _settings.ServiceName);

                return TopshelfExitCode.ServiceAlreadyRunning;
            }

            var started = false;
            try
            {
                _log.Debug("Starting up as a console application");

                _exit = new ManualResetEvent(false);
                _exitCode = TopshelfExitCode.Ok;
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IntPtr.Zero != GetConsoleWindow())
                {
                    try
                    {
                        // It is common to run console applications in windowless mode, this prevents
                        // the process from crashing when attempting to set the title.
                        Console.Title = _settings.DisplayName;
                    }
                    catch (Exception e) when (e is IOException or PlatformNotSupportedException)
                    {
                        _log.Info("It was not possible to set the console window title. See the inner exception for details.", e);
                    }
                }
                Console.CancelKeyPress += HandleCancelKeyPress;

                if (!_serviceHandle.Start(this))
                {
                    throw new TopshelfException("The service failed to start (return false).");
                }

                started = true;

                _log.InfoFormat(CultureInfo.CurrentCulture, "The {0} service is now running, press Control+C to exit.", _settings.ServiceName);

                _exit?.WaitOne();
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("An exception occurred", ex);

                return TopshelfExitCode.AbnormalExit;
            }
            finally
            {
                if (started)
                {
                    StopService();
                }

                _exit?.Close();
                _exit?.Dispose();

                HostLogger.Shutdown();
            }

            return _exitCode;
        }

        void IHostControl.Stop()
        {
            _log.Info("Service Stop requested, exiting.");
            _exit?.Set();
        }

        void IHostControl.Stop(TopshelfExitCode exitCode)
        {
            _log.Info($"Service Stop requested with exit code {exitCode}, exiting.");
            _exitCode = exitCode;
            _exit?.Set();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _serviceHandle.Dispose();
                _exit?.Close();
                _exit?.Dispose();
            }

            _disposed = true;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _settings.ExceptionCallback?.Invoke((Exception)e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.TakeNoAction)
            {
                return;
            }

            _log.Fatal("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.LogErrorOnly)
            {
                return;
            }

            HostLogger.Shutdown();

            if (e.IsTerminating)
            {
                _exitCode = TopshelfExitCode.AbnormalExit;
                _exit?.Set();

                // it isn't likely that a TPL thread should land here, but if it does let's no block it
                if (Task.CurrentId.HasValue)
                {
                    return;
                }

                // this is evil, but perhaps a good thing to let us clean up properly.
                var deadThreadId = Interlocked.Increment(ref _deadThread);
                Thread.CurrentThread.IsBackground = true;

                // Only set name if thread does not already have one.
                Thread.CurrentThread.Name ??= "Unhandled Exception " + deadThreadId;

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            }
        }

        private void HandleCancelKeyPress(object? sender, ConsoleCancelEventArgs consoleCancelEventArgs)
        {
            if (!_settings.CanHandleCtrlBreak && consoleCancelEventArgs.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                _log.Error("Control+Break detected, terminating service (not cleanly, use Control+C to exit cleanly)");
                return;
            }

            consoleCancelEventArgs.Cancel = true;

            if (_hasCancelled)
            {
                return;
            }

            _log.InfoFormat(CultureInfo.CurrentCulture, "Control+{0} detected, attempting to stop service.", consoleCancelEventArgs.SpecialKey == ConsoleSpecialKey.ControlBreak ? "Break" : "C");
            if (_serviceHandle.Stop(this))
            {
                _hasCancelled = true;
                _exit?.Set();
            }
            else
            {
                _hasCancelled = false;
                _log.Error("The service is not in a state where it can be stopped.");
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            var arguments = new ConsolePowerEventArguments(e.Mode);

            _serviceHandle.PowerEvent(this, arguments);
        }

        private void OnSessionChanged(object sender, SessionSwitchEventArgs e)
        {
            var arguments = new ConsoleSessionChangedArguments(e.Reason);

            _serviceHandle.SessionChanged(this, arguments);
        }

        private void StopService()
        {
            try
            {
                if (!_hasCancelled)
                {
                    _log.InfoFormat(CultureInfo.CurrentCulture, "Stopping the {0} service", _settings.ServiceName);

                    if (!_serviceHandle.Stop(this))
                    {
                        throw new TopshelfException("The service failed to stop (returned false).");
                    }
                }
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("The service did not shut down gracefully", ex);
                _exitCode = TopshelfExitCode.ServiceControlRequestFailed;
            }
            finally
            {
                _serviceHandle.Dispose();

                _log.InfoFormat(CultureInfo.CurrentCulture, "The {0} service has stopped.", _settings.ServiceName);
            }
        }

        private sealed class ConsolePowerEventArguments : IPowerEventArguments
        {
            public ConsolePowerEventArguments(PowerModes powerMode) => EventCode = powerMode switch
            {
                PowerModes.Resume => PowerEventCode.ResumeAutomatic,
                PowerModes.StatusChange => PowerEventCode.PowerStatusChange,
                PowerModes.Suspend => PowerEventCode.Suspend,
                _ => throw new ArgumentOutOfRangeException(nameof(powerMode), powerMode, null),
            };

            public PowerEventCode EventCode { get; }
        }
        private sealed class ConsoleSessionChangedArguments : ISessionChangedArguments
        {
            public ConsoleSessionChangedArguments(SessionSwitchReason reason)
            {
                ReasonCode = (SessionChangeReasonCode)Enum.ToObject(typeof(SessionChangeReasonCode), (int)reason);
                SessionId = Process.GetCurrentProcess().SessionId;
            }

            public SessionChangeReasonCode ReasonCode { get; }

            public int SessionId { get; }
        }
    }
}
