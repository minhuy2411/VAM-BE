using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace VAM.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bucket;

        public FirebaseStorageService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _bucket = configuration["Firebase:Bucket"] ?? throw new ArgumentNullException("Firebase:Bucket configuration is missing");
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // Create a unique filename to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var objectPath = $"{folderName}/{uniqueFileName}";

            // URL-encode the object path for the query parameter and request URL
            var encodedObjectPath = Uri.EscapeDataString(objectPath);
            var uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucket}/o?name={encodedObjectPath}";

            using var stream = file.OpenReadStream();
            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            
            // Set content type from original file
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload file to Firebase: {response.StatusCode} - {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(responseBody);
            
            // Extract download token. Firebase Storage API returns metadata containing 'downloadTokens'
            string? downloadToken = null;
            if (jsonDocument.RootElement.TryGetProperty("downloadTokens", out var tokenProp))
            {
                downloadToken = tokenProp.GetString();
            }

            // If token is missing, sometimes it is in the metadata object nested inside
            if (string.IsNullOrEmpty(downloadToken) && jsonDocument.RootElement.TryGetProperty("metadata", out var metadataProp))
            {
                if (metadataProp.TryGetProperty("firebaseStorageDownloadTokens", out var fbTokenProp))
                {
                    downloadToken = fbTokenProp.GetString();
                }
            }

            // Construct public URL
            // Format: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{encodedObjectPath}?alt=media&token={downloadToken}
            var publicUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucket}/o/{encodedObjectPath}?alt=media";
            if (!string.IsNullOrEmpty(downloadToken))
            {
                publicUrl += $"&token={downloadToken}";
            }

            return publicUrl;
        }
    }
}
