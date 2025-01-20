using System.Reflection;

namespace Core.Helpers
{
    public class ApplicationHelper
    {
        public static class ApplicationVersion
        {
            public static string GetAssemblyVersion()
            {
                return GetAssemblyVersionInternal(Assembly.GetCallingAssembly);
            }

            // Internal method to allow dependency injection for testing
            internal static string GetAssemblyVersionInternal(Func<Assembly> getAssembly)
            {
                var buildTag = System.Environment.GetEnvironmentVariable("BUILD_TAG");

                return string.IsNullOrWhiteSpace(buildTag) ? getAssembly()?.GetName().FullName : buildTag;
            }
        }
    }
}