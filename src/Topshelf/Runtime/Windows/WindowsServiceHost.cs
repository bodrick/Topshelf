// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Exceptions;
using Topshelf.Logging;

namespace Topshelf.Runtime.Windows
{
    public class WindowsServiceHost : ServiceBase, IHost, IHostControl
    {
        private static readonly ILogWriter Log = HostLogger.Get<WindowsServiceHost>();
        private readonly IHostConfigurator _configurator;
        private readonly IHostEnvironment _environment;
        private readonly IServiceHandle _serviceHandle;
        private readonly IHostSettings _settings;
        private int _deadThread;
        private bool _disposed;
        private Exception? _unhandledException;

        public WindowsServiceHost(IHostEnvironment environment, IHostSettings settings, IServiceHandle serviceHandle, IHostConfigurator configurator)
        {
            _settings = settings;
            _serviceHandle = serviceHandle;
            _environment = environment;
            _configurator = configurator;

            CanStop = settings.CanStop;
            CanPauseAndContinue = settings.CanPauseAndContinue;
            CanShutdown = settings.CanShutdown;
            CanHandlePowerEvent = settings.CanHandlePowerEvent;
            CanHandleSessionChangeEvent = settings.CanSessionChanged;
            ServiceName = _settings.ServiceName;
        }

        void IHostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            Log.DebugFormat(CultureInfo.CurrentCulture, "Requesting additional time: {0}", timeRemaining);
            RequestAdditionalTime((int)timeRemaining.TotalMilliseconds);
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            ExitCode = (int)TopshelfExitCode.Ok;

            Log.Info("Starting as a Windows service");

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                var message = $"The {_settings.ServiceName} service has not been installed yet. Please run '{Assembly.GetEntryAssembly()?.GetName()} install'.";
                Log.Fatal(message);

                ExitCode = (int)TopshelfExitCode.ServiceNotInstalled;
                throw new TopshelfException(message);
            }

            Log.Debug("[Topshelf] Starting up as a windows service application");

            Run(this);

            return (TopshelfExitCode)Enum.ToObject(typeof(TopshelfExitCode), ExitCode);
        }

        void IHostControl.Stop() => InternalStop();

        void IHostControl.Stop(TopshelfExitCode exitCode) => InternalStop(exitCode);

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _serviceHandle.Dispose();
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override void OnContinue()
        {
            try
            {
                Log.Info("[Topshelf] Resuming service");

                if (!_serviceHandle.Continue(this))
                {
                    throw new TopshelfException("The service did not continue successfully (returned false).");
                }

                Log.Info("[Topshelf] Resumed");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did not resume successfully", ex);
                throw;
            }
        }

        protected override void OnCustomCommand(int command)
        {
            try
            {
                Log.InfoFormat(CultureInfo.CurrentCulture, "[Topshelf] Custom command {0} received", command);

                _serviceHandle.CustomCommand(this, command);

                Log.InfoFormat(CultureInfo.CurrentCulture, "[Topshelf] Custom command {0} processed", command);
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Error("Unhandled exception during custom command processing detected", ex);
            }
        }

        protected override void OnPause()
        {
            try
            {
                Log.Info("[Topshelf] Pausing service");

                if (!_serviceHandle.Pause(this))
                {
                    throw new TopshelfException("The service did not pause successfully (returned false).");
                }

                Log.Info("[Topshelf] Paused");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did not pause gracefully", ex);
                throw;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            try
            {
                Log.Info("[Topshelf] Power event raised");

                var arguments = new WindowsPowerEventArguments(powerStatus);

                var result = _serviceHandle.PowerEvent(this, arguments);

                Log.Info("[Topshelf] Power event handled");

                return result;
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did handle the Power event correctly", ex);
                ExitCode = (int)TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            try
            {
                Log.Info("[Topshelf] Service session changed");

                var arguments = new WindowsSessionChangedArguments(changeDescription);

                _serviceHandle.SessionChanged(this, arguments);

                Log.Info("[Topshelf] Service session changed handled");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The did not handle Service session change correctly", ex);
                ExitCode = (int)TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnShutdown()
        {
            try
            {
                Log.Info("[Topshelf] Service is being shutdown");

                _serviceHandle.Shutdown(this);

                Log.Info("[Topshelf] Stopped");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int)TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Info("[Topshelf] Starting");

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                Log.DebugFormat(CultureInfo.CurrentCulture, "[Topshelf] Current Directory: {0}", Directory.GetCurrentDirectory());

                Log.DebugFormat(CultureInfo.CurrentCulture, "[Topshelf] Arguments: {0}", string.Join(",", args));

                var startArgs = string.Join(" ", args);
                _configurator.ApplyCommandLine(startArgs);

                if (!_serviceHandle.Start(this))
                {
                    throw new TopshelfException("The service did not start successfully (returned false).");
                }

                Log.Info("[Topshelf] Started");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did not start successfully", ex);

                ExitCode = (int)TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                Log.Info("[Topshelf] Stopping");

                if (!_serviceHandle.Stop(this))
                {
                    throw new TopshelfException("The service did not stop successfully (return false).");
                }

                Log.Info("[Topshelf] Stopped");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                Log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int)TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }

            if (_unhandledException != null)
            {
                ExitCode = (int)TopshelfExitCode.AbnormalExit;
                Log.Info("[Topshelf] Unhandled exception detected, rethrowing to cause application to restart.");
                throw new InvalidOperationException("An unhandled exception was detected", _unhandledException);
            }
        }

        private void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _settings.ExceptionCallback?.Invoke((Exception)e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.TakeNoAction)
            {
                return;
            }

            Log.Fatal("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.LogErrorOnly)
            {
                return;
            }

            HostLogger.Shutdown();

            ExitCode = (int)TopshelfExitCode.AbnormalExit;
            _unhandledException = (Exception)e.ExceptionObject;

            Stop();

            // it isn't likely that a TPL thread should land here, but if it does let's no block it
            if (Task.CurrentId.HasValue)
            {
                return;
            }

            var deadThreadId = Interlocked.Increment(ref _deadThread);
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId;
            while (true)
            {
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        private void InternalStop(TopshelfExitCode? exitCode = null)

        {
            if (CanStop)
            {
                Log.Debug("Stop requested by hosted service");
                if (exitCode.HasValue)
                {
                    ExitCode = (int)exitCode.Value;
                }

                Stop();
            }
            else
            {
                Log.Debug("Stop requested by hosted service, but service cannot be stopped at this time");
                throw new ServiceControlException("The service cannot be stopped at this time");
            }
        }

        private sealed class WindowsPowerEventArguments : IPowerEventArguments
        {
            public WindowsPowerEventArguments(PowerBroadcastStatus powerStatus) => EventCode = (PowerEventCode)Enum.ToObject(typeof(PowerEventCode), (int)powerStatus);

            public PowerEventCode EventCode { get; }
        }
        private sealed class WindowsSessionChangedArguments : ISessionChangedArguments
        {
            public WindowsSessionChangedArguments(SessionChangeDescription changeDescription)
            {
                ReasonCode = (SessionChangeReasonCode)Enum.ToObject(typeof(SessionChangeReasonCode), (int)changeDescription.Reason);
                SessionId = changeDescription.SessionId;
            }

            public SessionChangeReasonCode ReasonCode { get; }

            public int SessionId { get; }
        }
    }
}
