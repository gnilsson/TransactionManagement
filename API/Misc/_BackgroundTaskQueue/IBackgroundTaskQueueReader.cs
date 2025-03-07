#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Misc;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IBackgroundTaskQueueReader<T>
{
    ValueTask<T> DequeueAsync(CancellationToken cancellationToken);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
}
