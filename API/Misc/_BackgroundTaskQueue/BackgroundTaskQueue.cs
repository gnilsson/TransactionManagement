using System.Threading.Channels;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Misc;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class BackgroundTaskQueue<T> : IBackgroundTaskQueueReader<T>, IBackgroundTaskQueueWriter<T>
{
    private readonly Channel<T> _channel;

    public BackgroundTaskQueue()
    {
        _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });
    }

    public BackgroundTaskQueue(UnboundedChannelOptions options)
    {
        _channel = Channel.CreateUnbounded<T>(options);
    }

    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.WaitToReadAsync(cancellationToken);
    }

    public ValueTask QueueAsync(T item, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(item, cancellationToken);
    }
}
