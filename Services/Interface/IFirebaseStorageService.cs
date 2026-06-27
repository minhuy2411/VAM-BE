using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace VAM.Services
{
    public interface IFirebaseStorageService
    {
        /// <summary>
        /// Uploads a file to Firebase Storage and returns the public download URL.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="folderName">The folder path in the bucket (e.g. "certificates").</param>
        /// <returns>The public URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }
}
