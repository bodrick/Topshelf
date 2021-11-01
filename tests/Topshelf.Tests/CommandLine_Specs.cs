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
using NUnit.Framework;
using Topshelf.Configuration;
using Topshelf.Exceptions;
using Topshelf.Hosts;
using Topshelf.Runtime;
// ReSharper disable InconsistentNaming

namespace Topshelf.Tests
{
    [TestFixture]
    public class Passing_install
    {
        [Test]
        public void Extensible_the_command_line_should_be_yes()
        {
            var isSuperfly = false;

            HostFactory.New(x =>
            {
                x.Service<MyService>();

                x.AddCommandLineSwitch("superfly", v => isSuperfly = v);

                x.ApplyCommandLine("--superfly");
            });

            Assert.That(isSuperfly, Is.True);
        }

        [Test]
        public void Extensible_the_command_line_should_be_yet_again()
        {
            string? volumeLevel = null;

            HostFactory.New(x =>
            {
                x.Service<MyService>();

                x.AddCommandLineDefinition("volumeLevel", v => volumeLevel = v);

                x.ApplyCommandLine("-volumeLevel:11");
            });

            Assert.That(volumeLevel, Is.EqualTo("11"));
        }

        [Test]
        public void Need_to_handle_crazy_special_characters_in_argument()
        {
            string? password = null;

            HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.AddCommandLineDefinition("password", v => password = v);
                x.ApplyCommandLine("-password \"abc123=:,.<>/?;!@#$%^&*()-+\"");
            });

            Assert.That(password, Is.EqualTo("abc123=:,.<>/?;!@#$%^&*()-+"));
        }

        [Test]
        public void Need_to_handle_crazy_special_characters_in_argument_no_quotes()
        {
            string? password = null;

            HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.AddCommandLineDefinition("password", v => password = v);
                x.ApplyCommandLine("-password abc123=:,.<>/?;!@#$%^&*()-+");
            });

            Assert.That(password, Is.EqualTo("abc123=:,.<>/?;!@#$%^&*()-+"));
        }

        [Test]
        public void Need_to_handle_crazy_special_characters_in_arguments()
        {
            string? password = null;

            HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.AddCommandLineDefinition("password", v => password = v);
                x.ApplyCommandLine("-password:abc123!@#=$%^&*()-+");
            });

            Assert.That(password, Is.EqualTo("abc123!@#=$%^&*()-+"));
        }

        [Test]
        public void Should_create_a_start_host()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("start");
            });

            Assert.That(host, Is.InstanceOf<StartHost>());
        }

        [Test]
        public void Should_create_a_stop_host()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("stop");
            });

            Assert.That(host, Is.InstanceOf<StopHost>());
        }

        [Test]
        public void Should_create_an_install_host()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
        }

        [Test]
        public void Should_create_an_install_host_to_set_disabled()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --disabled");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.StartMode, Is.EqualTo(HostStartMode.Disabled));
        }

        [Test]
        public void Should_create_an_install_host_to_start_automatically()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --autostart");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.StartMode, Is.EqualTo(HostStartMode.Automatic));
        }

        [Test]
        public void Should_create_an_install_host_to_start_delayed()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --delayed");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.StartMode, Is.EqualTo(HostStartMode.AutomaticDelayed));
        }

        [Test]
        public void Should_create_an_install_host_to_start_manually()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --manual");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.StartMode, Is.EqualTo(HostStartMode.Manual));
        }

        [Test]
        public void Should_create_an_install_host_to_start_manually_without_being_case_sensitive()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("InstAll --ManuAl");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.StartMode, Is.EqualTo(HostStartMode.Manual));
        }

        [Test]
        public void Should_create_an_install_host_with_description()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -description \"Joe is good\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Description, Is.EqualTo("Joe is good"));
        }

        [Test]
        public void Should_create_an_install_host_with_display_name()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.DisplayName, Is.EqualTo("Joe"));
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_and_instance_name()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe\" -instance \"42\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.DisplayName, Is.EqualTo("Joe (Instance: 42)"));
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_and_instance_name_no_quotes()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname Joe -instance 42");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.DisplayName, Is.EqualTo("Joe (Instance: 42)"));
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_with_instance_name()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe (Instance: 42)\" -instance \"42\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.DisplayName, Is.EqualTo("Joe (Instance: 42)"));
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_with_instance_name_no_quotes()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe (Instance: 42)\" -instance 42");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.DisplayName, Is.EqualTo("Joe (Instance: 42)"));
        }

        [Test]
        public void Should_create_an_install_host_with_service_name()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -servicename \"Joe\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Name, Is.EqualTo("Joe"));
            Assert.That(installHost.Settings.ServiceName, Is.EqualTo("Joe"));
        }

        [Test]
        public void Should_create_an_install_host_with_service_name_and_instance_name()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -servicename \"Joe\" -instance \"42\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Name, Is.EqualTo("Joe"));
            Assert.That(installHost.Settings.InstanceName, Is.EqualTo("42"));
            Assert.That(installHost.Settings.ServiceName, Is.EqualTo("Joe$42"));
        }

        [Test]
        public void Should_create_an_install_host_with_service_name_and_instance_name_no_quotes()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -servicename Joe -instance 42");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Name, Is.EqualTo("Joe"));
            Assert.That(installHost.Settings.InstanceName, Is.EqualTo("42"));
            Assert.That(installHost.Settings.ServiceName, Is.EqualTo("Joe$42"));
        }

        [Test]
        public void Should_create_an_install_host_with_service_name_no_quotes()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -servicename Joe");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Name, Is.EqualTo("Joe"));
            Assert.That(installHost.Settings.ServiceName, Is.EqualTo("Joe"));
        }

        [Test]
        public void Should_create_an_install_host_without_being_case_sensitive()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("Install");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
        }

        [Test]
        public void Should_create_an_uninstall_host()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("uninstall");
            });

            Assert.That(host, Is.InstanceOf<UninstallHost>());
        }

        [Test]
        public void Should_create_and_install_host_with_service_name_containing_space()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -servicename \"Joe's Service\" -instance \"42\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.Settings.Name, Is.EqualTo("Joe's Service"));
            Assert.That(installHost.Settings.InstanceName, Is.EqualTo("42"));
            Assert.That(installHost.Settings.ServiceName, Is.EqualTo("Joe's Service$42"));
        }

        [Test]
        public void Should_require_password_option_when_specifying_username() =>
            Assert.Throws<HostConfigurationException>(() =>
                HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install -username \"Joe\"");
                }));

        [Test]
        public void Should_throw_an_exception_on_an_invalid_command_line()
        {
            var exception = Assert.Throws<HostConfigurationException>(() => HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("explode");
            }));

            Assert.That(exception.Message.Contains("explode"), Is.True);
        }

        [Test]
        public void Will_allow_blank_password_when_specifying_username()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -username \"Joe\" -password \"\"");
            });

            Assert.That(host, Is.InstanceOf<InstallHost>());
            var installHost = (InstallHost)host;
            Assert.That(installHost.InstallSettings.Credentials.Username, Is.EqualTo("Joe"));
            Assert.That(installHost.InstallSettings.Credentials.Password, Is.EqualTo(""));
        }

        private class MyService : IServiceControl
        {
            public bool Continue(IHostControl hostControl) => throw new NotImplementedException();

            public bool Pause(IHostControl hostControl) => throw new NotImplementedException();

            public bool Start(IHostControl hostControl) => throw new NotImplementedException();

            public bool Stop(IHostControl hostControl) => throw new NotImplementedException();
        }
    }
}
