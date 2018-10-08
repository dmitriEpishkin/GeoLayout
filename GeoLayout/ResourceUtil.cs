using System;
using System.Reflection;
using System.Windows;

namespace GeoLayout {
    public static class ResourceUtil {
        /// <summary>
        /// Get resource dictionary by path in currect (calling) assembly
        /// </summary>
        /// <param name="relativePath">path to resource dictionary</param>
        /// <returns></returns>
        public static ResourceDictionary GetRelativeResourceDictionary(string relativePath) {
            return GetRelativeResourceDictionary(Assembly.GetCallingAssembly(), relativePath);
        }

        /// <summary>
        /// Get resource dictionary by path
        /// </summary>
        /// <param name="assembly">Assembly witch contain resource dictionary</param>
        /// <param name="relativePath">Path to resource dictionary</param>
        /// <returns></returns>
        public static ResourceDictionary GetRelativeResourceDictionary(Assembly assembly, string relativePath) {
            var resourceDictionary = new ResourceDictionary();
            var assemblyName = assembly.FullName;
            var resourcePath = relativePath.TrimStart('/', '\\');
            resourceDictionary.Source = new Uri(string.Format(@"pack://application:,,,/{0};component/{1}", assemblyName, resourcePath), UriKind.Absolute);
            return resourceDictionary;
        }
    }
}
