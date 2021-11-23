using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Configuration.Install
{
    public class InstallContext
    {
        public InstallContext() : this(null, null)
        {
        }

        public InstallContext(string? logFilePath, IList<string>? commandLine)
        {
            Parameters = ParseCommandLine(commandLine);
            if (Parameters["logfile"] == null && logFilePath != null)
            {
                Parameters["logfile"] = logFilePath;
            }
        }

        public StringDictionary Parameters { get; }

        public bool IsParameterTrue(string paramName)
        {
            var text = Parameters[paramName.ToLower(CultureInfo.InvariantCulture)];
            if (text == null)
            {
                return false;
            }

            if (!string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(text, "1", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(text);
            }

            return true;
        }

        public void LogMessage(string? message)
        {
            try
            {
                LogMessageHelper(message);
            }
            catch
            {
                try
                {
                    Parameters["logfile"] = Path.Combine(Path.GetTempPath(), Path.GetFileName(Parameters["logfile"]) ?? string.Empty);
                    LogMessageHelper(message);
                }
                catch
                {
                    Parameters["logfile"] = null;
                }
            }

            if (!IsParameterTrue("LogToConsole") && Parameters["logtoconsole"] != null)
            {
                return;
            }

            Console.WriteLine(message);
        }

        private static StringDictionary ParseCommandLine(IList<string>? args)
        {
            var options = new StringDictionary();
            if (args == null)
            {
                return options;
            }

            for (var i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                    args[i].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    args[i] = args[i][1..];
                }

                var equalsPos = args[i].IndexOf('=', StringComparison.OrdinalIgnoreCase);
                if (equalsPos < 0)
                {
                    options[args[i].ToLower(CultureInfo.InvariantCulture)] = string.Empty;
                }
                else
                {
                    options[args[i][..equalsPos].ToLower(CultureInfo.InvariantCulture)] = args[i][(equalsPos + 1)..];
                }
            }

            return options;
        }

        private void LogMessageHelper(string? message)
        {
            StreamWriter? log = null;
            try
            {
                var parameter = Parameters["logfile"];
                if (!string.IsNullOrEmpty(parameter))
                {
                    log = new StreamWriter(parameter, true, Encoding.UTF8);
                    log.WriteLine(message);
                }
            }
            finally
            {
                log?.Close();
                log?.Dispose();
            }
        }
    }
}
