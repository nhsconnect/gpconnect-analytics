using System.Reflection;

namespace gpconnect_analytics.Helpers
{
    public class ApplicationHelper
    {
        public static class ApplicationVersion
        {
            public static string GetAssemblyVersion()
            {
                string buildTag = System.Environment.GetEnvironmentVariable("BUILD_TAG");

                if (string.IsNullOrWhiteSpace(buildTag))
                    return Assembly.GetCallingAssembly()?.GetName().FullName;

                return buildTag;
            }
        }
    }
}
