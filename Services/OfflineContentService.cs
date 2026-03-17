using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TravelGuideApp.Services
{
    public class OfflineContentService
    {
        private readonly HttpClient _httpClient;

        public OfflineContentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> DownloadToCacheAsync(string url, string subfolder)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var filePath = GetCachePath(url, subfolder);
            if (File.Exists(filePath))
                return filePath;

            var bytes = await _httpClient.GetByteArrayAsync(url);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await File.WriteAllBytesAsync(filePath, bytes);
            return filePath;
        }

        public string GetCachePath(string url, string subfolder)
        {
            var fileName = BuildFileName(url);
            var root = Path.Combine(FileSystem.Current.AppDataDirectory, "offline", subfolder);
            return Path.Combine(root, fileName);
        }

        public bool TryGetCachedPath(string url, string subfolder, out string filePath)
        {
            filePath = GetCachePath(url, subfolder);
            return File.Exists(filePath);
        }

        private static string BuildFileName(string url)
        {
            var ext = Path.GetExtension(url);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".bin";

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
            var name = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return $"{name}{ext}";
        }
    }
}
