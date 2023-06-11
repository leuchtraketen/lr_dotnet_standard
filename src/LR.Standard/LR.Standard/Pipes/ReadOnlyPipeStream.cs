using System;
using System.IO;
using System.Threading;

namespace LR.Standard.Pipes;

internal sealed class ReadOnlyPipeStream : Stream
{
    private readonly IReadOnlyPipe _pipe;
    private readonly Action? _check_for_exceptions;

    public ReadOnlyPipeStream(IReadOnlyPipe pipe, Action? check_for_exceptions)
    {
        this._pipe = pipe;
        this._check_for_exceptions = check_for_exceptions;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotImplementedException();

    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int i = 0;
        while (i < count)
        {
            this._pipe.Read(out byte b, out bool content_available, out bool end_reached);
            if (end_reached) break;
            if (!content_available) Thread.Sleep(5);
            buffer[offset + i] = (byte)b;
            i++;
        }

        this._check_for_exceptions?.Invoke();
        return i;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
