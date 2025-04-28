using System.Reflection;
using log4net;
using log4net.Config;

namespace EFCore.Tests
{
    /// <summary>
    /// Inits logging with log4net library
    /// <param name="internalDebugging">log library internal debugging switch to investigate log setup issues</param>
    /// </summary>
    public class Logging
    {
        private static bool s_initialized = false;
        
        public static void Init(bool internalDebugging = false)
        {
            if (s_initialized)
                return;
            log4net.Util.LogLog.InternalDebugging = internalDebugging;
            GlobalContext.Properties["framework"] = "net6.0";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("app.config"));
            s_initialized = true;
        }
    }
}

