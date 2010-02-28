using System;
using System.IO;

namespace Common.Helpers
{
    /// <summary>
    /// Provides generic helper methods for <see cref="Stream"/>s
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Copies the content of one stream to another in buffer-sized steps
        /// </summary>
        /// <param name="source">The source stream to copy from</param>
        /// <param name="destination">The destination stream to copy to</param>
        /// <param name="bufferSize">The size of the buffer to use for copying in bytes</param>
        public static void Copy(Stream source, Stream destination, long bufferSize)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            var buffer = new byte[bufferSize];
            int read;

            if (source.CanSeek) source.Position = 0;

            do
            {
                read = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, read);
            } while (read != 0);

            if (destination.CanSeek) destination.Position = 0;
        }

        /// <summary>
        /// Copies the content of one stream to another in one go
        /// </summary>
        /// <param name="source">The source stream to copy from</param>
        /// <param name="destination">The destination stream to copy to</param>
        public static void Copy(Stream source, Stream destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            Copy(source, destination, source.Length == 0 ? source.Position : source.Length);
        }
    }
}