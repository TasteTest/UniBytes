using System.IO;
using System.Threading;
using System.Threading.Tasks;
using backend.Common;

namespace backend.Services;

public interface IBlobStorageService
{
    Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, CancellationToken ct = default);
    Task<Result> DeleteImageAsync(string blobUrl, CancellationToken ct = default);
}