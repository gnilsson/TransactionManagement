#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Features;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class ResponseCachingTeeStream : Stream
{
    private readonly Stream _responseStream;
    private readonly Stream _cacheStream;

    public ResponseCachingTeeStream(Stream responseStream, Stream cacheStream)
    {
        _responseStream = responseStream;
        _cacheStream = cacheStream;
    }

    public override bool CanRead => _cacheStream.CanRead;
    public override bool CanSeek => _cacheStream.CanSeek;
    public override bool CanWrite => _responseStream.CanWrite && _cacheStream.CanWrite;
    public override long Length => _cacheStream.Length;

    public override long Position
    {
        get => _cacheStream.Position;
        set => _cacheStream.Position = value;
    }

    public override void Flush()
    {
        _responseStream.Flush();
        _cacheStream.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            _responseStream.FlushAsync(cancellationToken),
            _cacheStream.FlushAsync(cancellationToken));
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _cacheStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _cacheStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _responseStream.SetLength(value);
        _cacheStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _responseStream.Write(buffer, offset, count);
        _cacheStream.Write(buffer, offset, count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _responseStream.WriteAsync(buffer, cancellationToken).AsTask(),
            _cacheStream.WriteAsync(buffer, cancellationToken).AsTask());
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await _cacheStream.ReadAsync(buffer, cancellationToken);
    }
}
