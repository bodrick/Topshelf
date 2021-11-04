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
                   where int.TryParse(cmd.Id, out _)
                   select (IOption)new CommandOption(cmd.Id))
                .Or(x.Argument("help").Select(_ => (IOption)new HelpOption()))
                .Or(x.Argument("run").Select(_ => (IOption)new RunOption()))
                .Or(x.Switch("help").Select(_ => (IOption)new HelpOption()))
                .Or(x.Switch("systemonly").Select(_ => (IOption)new SystemOnlyHelpOption()))
                .Or(x.Definition("servicename").Select(name => (IOption)new ServiceNameOption(name.Value)))
                .Or(x.Definition("description").Select(desc => (IOption)new ServiceDescriptionOption(desc.Value)))
                .Or(x.Definition("displayname").Select(disp => (IOption)new DisplayNameOption(disp.Value)))
                .Or(x.Definition("instance").Select(instance => (IOption)new InstanceOption(instance.Value)))
                .Or(x.Argument("stop").Select(_ => (IOption)new StopOption()))
                .Or(x.Argument("start").Select(_ => (IOption)new StartOption()))
                .Or(x.Argument("install").Select(_ => (IOption)new InstallOption()))
                .Or(x.Argument("uninstall").Select(_ => (IOption)new UninstallOption()))
                .Or(x.Switch("sudo").Select(_ => (IOption)new SudoOption()))
                .Or(from username in x.Definition("username")
                    from password in x.Definition("password")
                    select (IOption)new ServiceAccountOption(username.Value, password.Value))
                .Or(x.Switch("autostart").Select(_ => (IOption)new AutostartOption()))
                .Or(x.Switch("manual").Select(_ => (IOption)new ManualStartOption()))
                .Or(x.Switch("disabled").Select(_ => (IOption)new DisabledOption()))
                .Or(x.Switch("delayed").Select(_ => (IOption)new DelayedOption()))
                .Or(x.Switch("interactive").Select(_ => (IOption)new InteractiveOption()))
                .Or(x.Switch("localsystem").Select(_ => (IOption)new LocalSystemOption()))
                .Or(x.Switch("localservice").Select(_ => (IOption)new LocalServiceOption()))
                .Or(x.Switch("networkservice").Select(_ => (IOption)new NetworkServiceOption()))
            );

        internal static void AddUnknownOptions(ICommandLineElementParser<IOption> x) =>
            x.Add(
                x.Definition().Select(unknown => (IOption)new UnknownOption(unknown.ToString() ?? string.Empty))
                    .Or(x.Switch().Select(unknown => (IOption)new UnknownOption(unknown.ToString() ?? string.Empty)))
                    .Or(x.Argument().Select(unknown => (IOption)new UnknownOption(unknown.ToString() ?? string.Empty))));
    }
}
