using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Configuration.Install
{
    public class InstallContext
    {
        private readonly StringDictionary parameters;

        public InstallContext() : this(null, null)
        {
        }

        public InstallContext(string logFilePath, string[] commandLine)
        {
            parameters = InstallContext.ParseCommandLine(commandLine);
            if (Parameters["logfile"] == null && logFilePath != null)
            {
                Parameters["logfile"] = logFilePath;
            }
        }

        public StringDictionary Parameters => parameters;

        public bool IsParameterTrue(string paramName)
        {
            var text = Parameters[paramName.ToLower(CultureInfo.InvariantCulture)];
            if (text == null)
            {
                return false;
            }

            if (string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(text, "yes", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(text, "1", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return "".Equals(text);
            }
            return true;
        }

        public void LogMessage(string message)
        {
            try
            {
                LogMessageHelper(message);
            }
            catch (Exception)
            {
                try
                {
                    Parameters["logfile"] = Path.Combine(Path.GetTempPath(), Path.GetFileName(Parameters["logfile"]));
                    LogMessageHelper(message);
                }
                catch (Exception)
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

        internal void LogMessageHelper(string message)
        {
            StreamWriter streamWriter = null;
            try
            {
                if (!string.IsNullOrEmpty(Parameters["logfile"]))
                {
                    streamWriter = new StreamWriter(Parameters["logfile"], true, Encoding.UTF8);
                    streamWriter.WriteLine(message);
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }

        protected static StringDictionary ParseCommandLine(string[] args)
        {
            var stringDictionary = new StringDictionary();
            if (args == null)
            {
                return stringDictionary;
            }
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                {
                    args[i] = args[i].Substring(1);
                }
                var num = args[i].IndexOf('=');
                if (num < 0)
                {
                    stringDictionary[args[i].ToLower(CultureInfo.InvariantCulture)] = "";
                }
                else
                {
                    stringDictionary[args[i].Substring(0, num).ToLower(CultureInfo.InvariantCulture)] = args[i].Substring(num + 1);
                }
            }
            return stringDictionary;
        }
    }
}
