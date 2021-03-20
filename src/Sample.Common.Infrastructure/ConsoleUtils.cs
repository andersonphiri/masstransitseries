using System;
using System.Reflection;

namespace Sample.Common.Infrastructure
{
    public static class ConsoleUtils
    {
        public static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            return $"{assembly.Name} V{assembly.Version}";
        }
    }
}
