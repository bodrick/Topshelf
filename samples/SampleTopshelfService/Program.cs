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
using Serilog;
using Topshelf;
using Topshelf.Configuration;
using Topshelf.Serilog;

namespace SampleTopshelfService
{
    internal class Program
    {
        private static int Main() => (int)HostFactory.Run(x =>
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            x.UseSerilog();

            x.UseAssemblyInfoForServiceInfo();

            var throwOnStart = false;
            var throwOnStop = false;
            var throwUnhandled = false;

            x.Service(_ => new SampleService(throwOnStart, throwOnStop, throwUnhandled), s =>
            {
                s.BeforeStartingService(_ => Console.WriteLine("BeforeStart"));
                s.BeforeStoppingService(_ => Console.WriteLine("BeforeStop"));
            });

            x.SetStartTimeout(TimeSpan.FromSeconds(10));
            x.SetStopTimeout(TimeSpan.FromSeconds(10));

            x.EnableServiceRecovery(r =>
            {
                r.RestartService(3)
                    .RunProgram(7, "ping google.com")
                    .RestartComputer(5, "message");

                r.OnCrashOnly();
                r.SetResetPeriod(2);
            });

            x.AddCommandLineSwitch("throwonstart", v => throwOnStart = v);
            x.AddCommandLineSwitch("throwonstop", v => throwOnStop = v);
            x.AddCommandLineSwitch("throwunhandled", v => throwUnhandled = v);

            x.OnException(exception => Console.WriteLine("Exception thrown - " + exception.Message));
        });

        private void SansInterface() => HostFactory.New(x =>
        {
            // can define services without the interface dependency as well, this is just for
            // show and not used in this sample.
            x.Service<SampleSansInterfaceService>(s =>
            {
                s.ConstructUsing(() => new SampleSansInterfaceService());
                s.WhenStarted(v => v.Start());
                s.WhenStopped(v => v.Stop());
            });
        });
    }
}
