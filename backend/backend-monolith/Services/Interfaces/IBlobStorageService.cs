using System.IO;
using System.Threading;
using System.Threading.Tasks;
using backend_monolith.Common;

namespace backend_monolith.Services;

public interface IBlobStorageService
{
    Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, CancellationToken ct = default);
    Task<Result> DeleteImageAsync(string blobUrl, CancellationToken ct = default);
}