namespace API.Misc;

public sealed class TeeStream : Stream
{
    private readonly Stream _stream1;
    private readonly Stream _stream2;

    public TeeStream(Stream stream1, Stream stream2)
    {
        _stream1 = stream1;
        _stream2 = stream2;
    }

    public override bool CanRead => _stream2.CanRead;
    public override bool CanSeek => _stream2.CanSeek;
    public override bool CanWrite => _stream1.CanWrite && _stream2.CanWrite;
    public override long Length => _stream2.Length;

    public override long Position
    {
        get => _stream2.Position;
        set => _stream2.Position = value;
    }

    public override void Flush()
    {
        _stream1.Flush();
        _stream2.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            _stream1.FlushAsync(cancellationToken),
            _stream2.FlushAsync(cancellationToken));
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream2.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream2.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream1.SetLength(value);
        _stream2.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream1.Write(buffer, offset, count);
        _stream2.Write(buffer, offset, count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _stream1.WriteAsync(buffer, cancellationToken).AsTask(),
            _stream2.WriteAsync(buffer, cancellationToken).AsTask());
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await _stream2.ReadAsync(buffer, cancellationToken);
    }
}
