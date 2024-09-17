using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace API.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureBlobStorage")["ConnectionString"];
            var containerName = configuration.GetSection("AzureBlobStorage")["ContainerName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(
                    nameof(connectionString),
                    "Azure Blob Storage connection string is not configured."
                );
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(
                    nameof(containerName),
                    "Azure Blob Storage container name is not configured."
                );
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName;

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
