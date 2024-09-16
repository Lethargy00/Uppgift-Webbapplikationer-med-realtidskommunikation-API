using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Azure Blob Storage connection string is not configured.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = "nartuligablobcontainer";

            if (string.IsNullOrEmpty(_containerName))
            {
                throw new ArgumentNullException(nameof(_containerName), "Blob container name is not configured.");
            }

            // Ensure the container exists or create it if not
            EnsureContainerExistsAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureContainerExistsAsync()
        {
            Console.WriteLine("EnsureContainerExistsAsync" + _containerName);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            try
            {
                await containerClient.GetPropertiesAsync(); // Check if the container exists
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                await containerClient.CreateAsync(PublicAccessType.Blob); // Create the container if it doesn't exist
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadBlobAsync(string blobName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var downloadInfo = await blobClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }
    }
}