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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.CommandLineParser;
using Topshelf.Configuration.Configurators;
using Topshelf.Configuration.Options;
using Topshelf.Logging;
using Topshelf.Runtime;
using Topshelf.Runtime.Windows;

namespace Topshelf.Configuration.HostConfigurators
{
    public class HostConfigurator : IHostConfigurator, IConfigurator
    {
        private readonly IList<ICommandLineConfigurator> _commandLineOptionConfigurators;
        private readonly IList<IHostBuilderConfigurator> _configurators;
        private readonly WindowsHostSettings _settings;
        private bool _commandLineApplied;
        private EnvironmentBuilderFactory _environmentBuilderFactory;
        private HostBuilderFactory _hostBuilderFactory;
        private ServiceBuilderFactory? _serviceBuilderFactory;

        public HostConfigurator()
        {
            _configurators = new List<IHostBuilderConfigurator>();
            _commandLineOptionConfigurators = new List<ICommandLineConfigurator>();
            _settings = new WindowsHostSettings();
            _environmentBuilderFactory = DefaultEnvironmentBuilderFactory;
            _hostBuilderFactory = DefaultHostBuilderFactory;
        }

        public UnhandledExceptionPolicyCode UnhandledExceptionPolicy
        {
            get => _settings.UnhandledExceptionPolicy;
            set => _settings.UnhandledExceptionPolicy = value;
        }

        public void AddCommandLineDefinition(string name, Action<string> callback)
        {
            var configurator = new CommandLineDefinitionConfigurator(name, callback);

            _commandLineOptionConfigurators.Add(configurator);
        }

        public void AddCommandLineSwitch(string name, Action<bool> callback)
        {
            var configurator = new CommandLineSwitchConfigurator(name, callback);
            _commandLineOptionConfigurators.Add(configurator);
        }

        public void AddConfigurator(IHostBuilderConfigurator configurator) => _configurators.Add(configurator);

        public void AddServiceArgument(string name, string value) => _settings.ServiceArguments[name] = value;

        public void ApplyCommandLine()
        {
            if (_commandLineApplied)
            {
                return;
            }

            var options = CommandLine.Parse<IOption>(ConfigureCommandLineParser);
            ApplyCommandLineOptions(options);
        }

        public void ApplyCommandLine(string commandLine)
        {
            var options = CommandLine.Parse<IOption>(ConfigureCommandLineParser, commandLine);
            ApplyCommandLineOptions(options);
            _commandLineApplied = true;
        }

        public IHost CreateHost()
        {
            var type = typeof(HostFactory);
            HostLogger.Get<HostConfigurator>()
                .InfoFormat(CultureInfo.CurrentCulture, "{0} v{1}, {2} ({3})", type.Namespace ?? string.Empty,
                    type.Assembly.GetName().Version?.ToString() ?? string.Empty,
                    RuntimeInformation.FrameworkDescription, Environment.Version);

            var environmentBuilder = _environmentBuilderFactory(this);

            var environment = environmentBuilder.Build();

            var builder = _hostBuilderFactory(environment, _settings);
            builder = _configurators.Aggregate(builder, (current, configurator) => configurator.Configure(current));

            try
            {
                var serviceBuilder = _serviceBuilderFactory!(_settings);
                return builder.Build(serviceBuilder);
            }
            //Intercept exceptions from serviceBuilder, TopShelf handling is in HostFactory
            catch (Exception ex)
            {
                builder.Settings.ExceptionCallback?.Invoke(ex);
                throw;
            }
        }

        public void EnableHandleCtrlBreak() => _settings.CanHandleCtrlBreak = true;

        public void EnablePauseAndContinue() => _settings.CanPauseAndContinue = true;

        public void EnablePowerEvents() => _settings.CanHandlePowerEvent = true;

        public void EnableSessionChanged() => _settings.CanSessionChanged = true;

        public void EnableShutdown() => _settings.CanShutdown = true;

        public void OnException(Action<Exception> callback) => _settings.ExceptionCallback = callback;

        public void SetCanStop(bool canStop) => _settings.CanStop = canStop;

        public void SetDescription(string description) => _settings.Description = description;

        public void SetDisplayName(string name) => _settings.DisplayName = name;

        public void SetInstanceName(string instanceName) => _settings.InstanceName = instanceName;

        public void SetServiceName(string name) => _settings.Name = name;

        public void SetStartTimeout(TimeSpan startTimeOut) => _settings.StartTimeOut = startTimeOut;

        public void SetStopTimeout(TimeSpan stopTimeOut) => _settings.StopTimeOut = stopTimeOut;

        public void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory) =>
            _environmentBuilderFactory = environmentBuilderFactory;

        public void UseHostBuilder(HostBuilderFactory hostBuilderFactory) => _hostBuilderFactory = hostBuilderFactory;

        public void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory) => _serviceBuilderFactory = serviceBuilderFactory;

        public IEnumerable<IValidateResult> Validate()
        {
            if (_serviceBuilderFactory == null)
            {
                yield return this.Failure("ServiceBuilderFactory", "must not be null");
            }

            if (string.IsNullOrEmpty(_settings.DisplayName) && string.IsNullOrEmpty(_settings.Name))
            {
                yield return this.Failure("DisplayName", "must be specified and not empty");
            }

            if (string.IsNullOrEmpty(_settings.Name))
            {
                yield return this.Failure("Name", "must be specified and not empty");
            }
            else
            {
                var disallowed = new[] { '\t', '\r', '\n', '\\', '/' };
                if (_settings.Name.IndexOfAny(disallowed) >= 0)
                {
                    yield return this.Failure("Name", "must not contain whitespace, '/', or '\\' characters");
                }
            }

            foreach (var result in _configurators.SelectMany(x => x.Validate()))
            {
                yield return result;
            }

            yield return this.Success("Name", _settings.Name);

            if (!string.Equals(_settings.Name, _settings.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                yield return this.Success("DisplayName", _settings.DisplayName);
            }

            if (!string.Equals(_settings.Name, _settings.Description, StringComparison.OrdinalIgnoreCase))
            {
                yield return this.Success("Description", _settings.Description);
            }

            if (!string.IsNullOrEmpty(_settings.InstanceName))
            {
                yield return this.Success("InstanceName", _settings.InstanceName);
            }

            yield return this.Success("ServiceName", _settings.ServiceName);
        }

        private static IEnvironmentBuilder DefaultEnvironmentBuilderFactory(IHostConfigurator configurator) =>
            new WindowsHostEnvironmentBuilder(configurator);

        private static IHostBuilder DefaultHostBuilderFactory(IHostEnvironment environment, IHostSettings settings) =>
            new RunBuilder(environment, settings);

        private void ApplyCommandLineOptions(IEnumerable<IOption> options)
        {
            foreach (var option in options)
            {
                option.ApplyTo(this);
            }
        }

        private void ConfigureCommandLineParser(ICommandLineElementParser<IOption> parser)
        {
            CommandLineParserOptions.AddTopshelfOptions(parser);

            foreach (var optionConfigurator in _commandLineOptionConfigurators)
            {
                optionConfigurator.Configure(parser);
            }

            CommandLineParserOptions.AddUnknownOptions(parser);
        }
    }
}
