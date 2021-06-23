using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contract to Upload Download and Delete a file from Blob Storage
///</summary>
public interface IBlobStorageService
{
    Task UploadAsync(string containerid, Stream fileStream, string fileId, string fileExtension,
                    int blobAccessExpirationDays, CancellationToken token);
    Task DownloadAsync(string containerid, string fileId, string fileExtension, CancellationToken token);
    Task DeleteAsync(string containerid, string fileId, string fileExtension, CancellationToken token);
}