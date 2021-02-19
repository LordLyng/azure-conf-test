using System.Data;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace AzureConfTest
{
    public static class OfflineCacheFactory
    {
        public static OfflineFileCache GetOfflineFileCache(string path = null)
        {
            if (path == null)
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDomain.CurrentDomain.FriendlyName, "configCache.json");

            var folder = Path.GetDirectoryName(path);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            Console.WriteLine($"Offline file cahce created at \"{path}\"");

            return new OfflineFileCache(new OfflineFileCacheOptions() { Path = path });
        }
    }
}