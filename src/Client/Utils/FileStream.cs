using System;
using System.IO;

using Godot;

using EnsureThat;

namespace GodotUtils.IO
{
    /// <summary>
    /// Implementation of a Stream to read/write from an underlying Godot File instance.
    /// From RR|Away on the Godot csharp Discord    
    /// </summary>
    class FileStream : Stream
    {
        /// <summary>
        /// The underlying Godot.File we are using for IO.
        /// </summary>
        private Godot.File file;

        private bool canRead;
        private bool canWrite;

        public FileStream(Godot.File file)
        {
            EnsureArg.IsNotNull(file);

            EnsureArg.IsTrue(file.IsOpen(), nameof(file), opts => opts.WithMessage("File must be open"));

            this.file = file;

            CheckErrorState();

            // TODO: Godot exposes no way to determine how the underlying file was opened (read or write).
            //	For now everything is true, and we rely on the user being sane. Proposal open at:
            //	https://github.com/godotengine/godot-proposals/issues/2106
            canRead = true;
            canWrite = true;
        }

        /// <inheritdoc/>
        public override bool CanRead => canRead;

        /// <inheritdoc/>
        public override bool CanWrite => canWrite;

        /// <inheritdoc/>
        public override bool CanSeek => true;   // All Godot File's support seeking

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                CheckFileState();
                return (long)file.GetLen();
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                CheckFileState();
                return (long)file.GetPosition();
            }
            set
            {
                Seek(0, SeekOrigin.Begin);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // TODO: This has a bind in Godot 4.0. Can implement then. For now we just silently ignore
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureArg.IsNotNull(buffer);
            EnsureArg.IsGte(offset, 0);
            EnsureArg.IsGte(count, 0);

            EnsureArg.IsGte(buffer.Length - offset, count, nameof(count),
                opts => opts.WithMessage("Offset + count ends past end of buffer"));

            CheckFileState();

            // Use File methods directly rather than properties we implement here in FileStream (e.g. Position).
            //	It's a DRY violation, but saves needlessly calling into Godot to (re)check file state. 
            var remaining = (int)(file.GetLen() - file.GetPosition());

            var length = Math.Min(count, remaining);
            var data = file.GetBuffer(length);

            CheckErrorState();

            Array.Copy(data, 0, buffer, offset, data.Length);

            return data.Length;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureArg.IsNotNull(buffer);
            EnsureArg.IsGte(offset, 0);
            EnsureArg.IsGte(count, 0);

            EnsureArg.IsGte(buffer.Length, offset + count, nameof(count),
                opts => opts.WithMessage("Offset + count ends past end of buffer"));

            CheckFileState();

            // If we don't use the whole buffer we need to make a subset copy before sending to Godot.
            //	It's ugly to copy the array, but its quicker than calling into Godot 1 byte at time.
            byte[] data = buffer;
            if (offset != 0 || buffer.Length > count)
            {
                data = new byte[count];
                Array.Copy(buffer, offset, data, 0, count);
            }
            file.StoreBuffer(data);

            CheckErrorState();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckFileState();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    file.Seek((int)offset);
                    break;

                case SeekOrigin.Current:
                    file.Seek((long)file.GetPosition() + (int)offset);
                    break;

                case SeekOrigin.End:
                    file.SeekEnd((int)offset);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            CheckErrorState();

            return (long)file.GetPosition();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            // No equivalent in Godot.File
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks the state of the underlying Godot.File object to make sure we can still operate.
        /// </summary>
        private void CheckFileState()
        {
            if (!file.IsOpen())
            {
                // Must now be closed. We know it *was* open as we checked it in ctor
                throw new InvalidOperationException("Underlying Godot File has been closed");
            }
        }

        /// <summary>
        /// Checks the file to see if any errors have been rasied.
        /// </summary>
        private void CheckErrorState() => file.GetError().ThrowOnError();
    }
}