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
using System.Threading;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class TestHost : IHost, HostControl
    {
        private readonly ILogWriter _log = HostLogger.Get<TestHost>();
        private readonly IServiceHandle _serviceHandle;
        private readonly HostSettings _settings;

        public TestHost(HostSettings settings, IHostEnvironment environment, IServiceHandle serviceHandle)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            _settings = settings;
            _serviceHandle = serviceHandle;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }

        public TopshelfExitCode Run()
        {
            var exitCode = TopshelfExitCode.AbnormalExit;
            try
            {
                exitCode = TopshelfExitCode.ServiceControlRequestFailed;

                _log.InfoFormat("The {0} service is being started.", _settings.ServiceName);
                _serviceHandle.Start(this);
                _log.InfoFormat("The {0} service was started.", _settings.ServiceName);

                Thread.Sleep(100);

                exitCode = TopshelfExitCode.ServiceControlRequestFailed;

                _log.InfoFormat("The {0} service is being stopped.", _settings.ServiceName);
                _serviceHandle.Stop(this);
                _log.InfoFormat("The {0} service was stopped.", _settings.ServiceName);

                exitCode = TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("The service threw an exception during testing.", ex);
            }
            finally
            {
                _serviceHandle.Dispose();
            }

            return exitCode;
        }

        void HostControl.Stop() => _log.Info("Service Stop requested, exiting.");

        void HostControl.Stop(TopshelfExitCode exitCode) => _log.Info($"Service Stop requested with exit code {exitCode}, exiting.");
    }
}
