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
using Topshelf.Configuration.CommandLineParser;
using Topshelf.Configuration.Options;

namespace Topshelf.Configuration.HostConfigurators
{
    internal static class CommandLineParserOptions
    {
        internal static void AddTopshelfOptions(ICommandLineElementParser<IOption> x) =>
            x.Add((from arg in x.Argument("command")
                   from cmd in x.Argument()
                   where int.TryParse(cmd.Id, out var n)
                   select (IOption)new CommandOption(cmd.Id))
                .Or(from arg in x.Argument("help")
                    select (IOption)new HelpOption())
                .Or(from arg in x.Argument("run")
                    select (IOption)new RunOption())
                .Or(from help in x.Switch("help")
                    select (IOption)new HelpOption())
                .Or(from systemHelp in x.Switch("systemonly")
                    select (IOption)new SystemOnlyHelpOption())
                .Or(from name in x.Definition("servicename")
                    select (IOption)new ServiceNameOption(name.Value))
                .Or(from desc in x.Definition("description")
                    select (IOption)new ServiceDescriptionOption(desc.Value))
                .Or(from disp in x.Definition("displayname")
                    select (IOption)new DisplayNameOption(disp.Value))
                .Or(from instance in x.Definition("instance")
                    select (IOption)new InstanceOption(instance.Value))
                .Or(from arg in x.Argument("stop")
                    select (IOption)new StopOption())
                .Or(from arg in x.Argument("start")
                    select (IOption)new StartOption())
                .Or(from arg in x.Argument("install")
                    select (IOption)new InstallOption())
                .Or(from arg in x.Argument("uninstall")
                    select (IOption)new UninstallOption())
                .Or(from arg in x.Switch("sudo")
                    select (IOption)new SudoOption())
                .Or(from username in x.Definition("username")
                    from password in x.Definition("password")
                    select (IOption)new ServiceAccountOption(username.Value, password.Value))
                .Or(from autostart in x.Switch("autostart")
                    select (IOption)new AutostartOption())
                .Or(from manual in x.Switch("manual")
                    select (IOption)new ManualStartOption())
                .Or(from disabled in x.Switch("disabled")
                    select (IOption)new DisabledOption())
                .Or(from delayed in x.Switch("delayed")
                    select (IOption)new DelayedOption())
                .Or(from interactive in x.Switch("interactive")
                    select (IOption)new InteractiveOption())
                .Or(from autostart in x.Switch("localsystem")
                    select (IOption)new LocalSystemOption())
                .Or(from autostart in x.Switch("localservice")
                    select (IOption)new LocalServiceOption())
                .Or(from autostart in x.Switch("networkservice")
                    select (IOption)new NetworkServiceOption())
            );

        internal static void AddUnknownOptions(ICommandLineElementParser<IOption> x) =>
            x.Add((from unknown in x.Definition()
                   select (IOption)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Switch()
                    select (IOption)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Argument()
                    select (IOption)new UnknownOption(unknown.ToString())));
    }
}
