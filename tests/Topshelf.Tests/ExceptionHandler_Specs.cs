// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using NUnit.Framework;
using Topshelf.Configuration;
// ReSharper disable InconsistentNaming

namespace Topshelf.Tests
{
    [TestFixture]
    public class Exception_callback
    {
        private const string StartExceptionMessage = "Throw on Start Requested";
        private const string StopExceptionMessage = "Throw on Stop Requested";

        [Test]
        public void Should_be_called_when_exception_thrown_in_construct_using()
        {
            var sawException = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service<ExceptionThrowingService>(s =>
                {
                    s.ConstructUsing(() => throw new Exception($"Unable to resolve {nameof(ExceptionThrowingService)} from DI container"));
                    s.WhenStarted((es, hc) => true);
                    s.WhenStopped((es, hc) => true);
                });

                x.OnException(_ => sawException = true);
            });

            Assert.That(sawException, Is.True);
            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.AbnormalExit));
        }

        [Test]
        public void Should_be_called_when_exception_thrown_in_constructor()
        {
            var sawException = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service<ServiceThrowingExceptionInConstructor>();

                x.OnException(_ => sawException = true);
            });

            Assert.That(sawException, Is.True);
            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.AbnormalExit));
        }

        [Test]
        public void Should_be_called_when_exception_thrown_in_Start_method()
        {
            var sawExceptionInStart = false;
            var sawExceptionInStop = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(true, false));

                x.OnException(ex =>
                {
                    if (ex.Message == StartExceptionMessage)
                    {
                        sawExceptionInStart = true;
                    }

                    if (ex.Message == StopExceptionMessage)
                    {
                        sawExceptionInStop = true;
                    }
                });
            });

            Assert.That(sawExceptionInStart, Is.True);
            Assert.That(sawExceptionInStop, Is.False);
            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.ServiceControlRequestFailed));
        }

        [Test]
        public void Should_be_called_when_exception_thrown_in_Stop_method()
        {
            var sawExceptionInStart = false;
            var sawExceptionInStop = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, true));

                x.OnException(ex =>
                {
                    if (ex.Message == StartExceptionMessage)
                    {
                        sawExceptionInStart = true;
                    }

                    if (ex.Message == StopExceptionMessage)
                    {
                        sawExceptionInStop = true;
                    }
                });
            });

            Assert.That(sawExceptionInStart, Is.False);
            Assert.That(sawExceptionInStop, Is.True);
            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.ServiceControlRequestFailed));
        }

        [Test]
        public void Should_not_be_called_when_no_exceptions_thrown()
        {
            var sawException = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, false));

                x.OnException(ex => sawException = true);
            });

            Assert.That(sawException, Is.False);
            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.Ok));
        }

        [Test]
        public void Should_not_prevent_default_action_when_not_set()
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(true, false));
            });

            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.ServiceControlRequestFailed));

            exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, true));
            });

            Assert.That(exitCode, Is.EqualTo(TopshelfExitCode.ServiceControlRequestFailed));
        }

        /// <summary>
        /// A simple service that can be configured to throw exceptions while starting or stopping.
        /// </summary>
        private class ExceptionThrowingService : IServiceControl
        {
            private readonly bool _throwOnStart;
            private readonly bool _throwOnStop;

            public ExceptionThrowingService(bool throwOnStart, bool throwOnStop)
            {
                _throwOnStart = throwOnStart;
                _throwOnStop = throwOnStop;
            }

            public bool Start(IHostControl hostControl)
            {
                if (_throwOnStart)
                {
                    throw new InvalidOperationException(StartExceptionMessage);
                }

                return true;
            }

            public bool Stop(IHostControl hostControl)
            {
                if (_throwOnStop)
                {
                    throw new InvalidOperationException(StopExceptionMessage);
                }

                return true;
            }
        }

        private class ServiceThrowingExceptionInConstructor : IServiceControl
        {
            public ServiceThrowingExceptionInConstructor() => throw new Exception("Exception from constructor");

            public bool Start(IHostControl hostControl) => true;

            public bool Stop(IHostControl hostControl) => true;
        }
    }
}
