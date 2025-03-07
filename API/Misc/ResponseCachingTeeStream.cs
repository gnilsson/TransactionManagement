namespace API.Misc;

public sealed class ResponseCachingTeeStream : Stream
{
    private readonly Stream _responseStream;
    private readonly Stream _cacheStream;
    private readonly bool _isCompleteAndStreaming;

    public ResponseCachingTeeStream(Stream responseStream, Stream cacheStream, bool isCompleteAndStreaming)
    {
        _responseStream = responseStream;
        _cacheStream = cacheStream;
        _isCompleteAndStreaming = isCompleteAndStreaming;
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
        // note:
        // I have no idea if this makes sense
        // it is one weird way I found to trick the json serializer to use utf8jsonwriter in the response stream
        if (_isCompleteAndStreaming)
        {
            Task.WaitAll(
                _responseStream.FlushAsync(),
                _cacheStream.FlushAsync());
            return;
        }

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
        // note:
        // I have no idea if this makes sense
        if (_isCompleteAndStreaming)
        {
            Task.WaitAll(
                _responseStream.WriteAsync(buffer, offset, count),
                _cacheStream.WriteAsync(buffer, offset, count));
            return;
        }
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
