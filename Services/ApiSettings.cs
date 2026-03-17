using Microsoft.Maui.Storage;

namespace TravelGuideApp.Services
{
    /// <summary>
    /// Cấu hình kết nối API.
    /// ClientSecret KHÔNG được hardcode ở đây — phải được set vào SecureStorage lần đầu khởi động
    /// hoặc thông qua một provisioning flow an toàn.
    /// </summary>
    public static class ApiSettings
    {
        // For Android emulator use 10.0.2.2 to access host machine.
        // For real device, change to your machine's LAN IP or domain.
        public const string BaseUrl = "http://10.0.2.2:5014";

        public const string ClientId = "travelguide-app";

        // Key dùng để lưu/đọc secret từ SecureStorage
        private const string SecretStorageKey = "api_client_secret";

        // Fallback chỉ cho dev/emulator — PHẢI đặt secret thật qua SetClientSecretAsync trước khi release
        private const string DevFallbackSecret = "dev-secret-change-me-in-production";

        /// <summary>
        /// Đọc ClientSecret từ SecureStorage (ưu tiên) hoặc fallback dev.
        /// Gọi hàm này async khi cần secret để call API.
        /// </summary>
        public static async System.Threading.Tasks.Task<string> GetClientSecretAsync()
        {
            var stored = await SecureStorage.Default.GetAsync(SecretStorageKey);
            return string.IsNullOrWhiteSpace(stored) ? DevFallbackSecret : stored;
        }

        /// <summary>
        /// Lưu ClientSecret vào SecureStorage (gọi một lần khi cài app / provisioning).
        /// </summary>
        public static async System.Threading.Tasks.Task SetClientSecretAsync(string secret)
        {
            await SecureStorage.Default.SetAsync(SecretStorageKey, secret);
        }

        /// <summary>
        /// Xóa ClientSecret khỏi SecureStorage (dùng khi logout hoặc reset app).
        /// </summary>
        public static void RemoveClientSecret()
        {
            SecureStorage.Default.Remove(SecretStorageKey);
        }
    }
}
