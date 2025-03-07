#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Misc;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IBackgroundTaskQueueWriter<T>
{
    ValueTask QueueAsync(T item, CancellationToken cancellationToken = default);
}
