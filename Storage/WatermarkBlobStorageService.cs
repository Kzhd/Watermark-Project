using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Storage
{
    public class WatermarkBlobStorageService : IBlobStorageService
    {
        private const string blobStorageConnectionString = "Storage:ConnectionString";
        private const string period = ".";
        private readonly IConfiguration _configuration;
        private readonly string storageConnectionString;
        private readonly CloudBlobClient _client;

        public WatermarkBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            storageConnectionString = this._configuration[blobStorageConnectionString];
            _client = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
        }

        public async Task UploadAsync(string containerid, Stream fileStream, string fileId, string fileExtension,
                    int blobAccessExpirationDays, CancellationToken token)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (fileExtension == null)
            {
                throw new ArgumentNullException(nameof(fileExtension));
            }

            if (blobAccessExpirationDays <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blobAccessExpirationDays));
            }

            CloudBlobContainer container = _client.GetContainerReference(containerid.ToString());

            await container.CreateIfNotExistsAsync(token).ConfigureAwait(false);

            string blobRelativePath = Path.GetFileName(string.Concat(fileId.ToString(), period, fileExtension));

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobRelativePath);
            blockBlob.Properties.ContentType = fileExtension;
            await blockBlob.UploadFromStreamAsync(fileStream, token).ConfigureAwait(false);
        }

        public async Task<Stream> DownloadAsync(string containerid, string fileId, string fileExtension, CancellationToken token = default)
        {
            if (fileExtension == null)
            {
                throw new ArgumentNullException(nameof(fileExtension));
            }

            CloudBlobContainer container = _client.GetContainerReference(containerid.ToString());

            if (!await container.ExistsAsync(token).ConfigureAwait(false))
            {
                throw new Exception($"Not Found - Container with ID# {containerid}");
            }

            var blockBlob = container.GetBlockBlobReference(string.Concat(fileId, period, fileExtension));

            if (!await blockBlob.ExistsAsync(token).ConfigureAwait(false))
            {
                throw new Exception($"Not Found - Blob with ID# {fileId} in Container# {containerid}");
            }

            MemoryStream stream = new MemoryStream();

            await blockBlob.DownloadToStreamAsync(stream, token).ConfigureAwait(false);

            return stream;
        }

        public async Task DeleteAsync(string containerid, string fileId, string fileExtension, CancellationToken token = default)
        {
            if(fileExtension == null)
            {
                throw new ArgumentNullException(nameof(fileExtension));
            }

            CloudBlobContainer container = _client.GetContainerReference(containerid.ToString());

            if(!await container.ExistsAsync(token).ConfigureAwait(false))
            {
                throw new Exception($"Not Found - Container with ID# {containerid}");
            }

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(string.Concat(fileId.ToString(),period,fileExtension));

            if(!await blockBlob.ExistsAsync(token).ConfigureAwait(false))
            {
                throw new Exception($"Not Found - BLob with ID# {fileId} in Container# {containerid}");
            }

            await blockBlob.DeleteIfExistsAsync(token).ConfigureAwait(false);
        }
    }
}
