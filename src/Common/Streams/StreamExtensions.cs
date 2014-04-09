/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;
using System.Text;
using NanoByte.Common.Tasks;

namespace NanoByte.Common.Streams
{
    /// <summary>
    /// Provides extension methods for <see cref="Stream"/>s.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Copies the content of one stream to another.
        /// </summary>
        /// <param name="source">The source stream to copy from.</param>
        /// <param name="destination">The destination stream to copy to.</param>
        /// <param name="bufferSize">The size of the buffer to use for copying in bytes.</param>
        /// <param name="cancellationToken">Used to signal when the user wishes to cancel the task execution.</param>
        /// <remarks>Will try to <see cref="Stream.Seek"/> to the start of <paramref name="source"/>.</remarks>
        public static void CopyTo(this Stream source, Stream destination, long bufferSize = 4096, CancellationToken cancellationToken = default(CancellationToken))
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            #endregion

            var buffer = new byte[bufferSize];
            int read;

            if (source.CanSeek) source.Position = 0;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                read = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, read);
            } while (read != 0);

            if (destination.CanSeek) destination.Position = 0;
        }

        /// <summary>
        /// Writes the entire content of a stream to a file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path of the file to write.</param>
        public static void WriteTo(this Stream stream, string path)
        {
            using (var fileStream = File.Create(path))
                stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Compares two streams for bit-wise equality.
        /// </summary>
        /// <remarks>Will try to <see cref="Stream.Seek"/> to the start of both streams.</remarks>
        public static bool ContentEquals(this Stream stream1, Stream stream2)
        {
            #region Sanity checks
            if (stream1 == null) throw new ArgumentNullException("stream1");
            if (stream2 == null) throw new ArgumentNullException("stream2");
            #endregion

            if (stream1.CanSeek) stream1.Position = 0;
            if (stream2.CanSeek) stream2.Position = 0;

            while (true)
            {
                int byte1 = stream1.ReadByte();
                int byte2 = stream2.ReadByte();
                if (byte1 != byte2) return false;
                else if (byte1 == -1) return true;
            }
        }

        /// <summary>
        /// Creates a new <see cref="MemoryStream"/> and fills it with UTF-8 encoded string data.
        /// </summary>
        /// <param name="data">The data to fill the stream with.</param>
        /// <returns>A filled stream with the position set to zero.</returns>
        public static MemoryStream ToStream(this string data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            var stream = new MemoryStream(byteArray);
            return stream;
        }

        /// <summary>
        /// Reads the entire content of a stream as UTF-8 encoded string data (will seek from zero to end).
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A entire content of the stream.</returns>
        public static string ReadToString(this Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            if (stream.CanSeek) stream.Position = 0;
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Reads the entire content of a stream to a byte array (will seek from zero to end).
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A entire content of the stream.</returns>
        public static byte[] ReadToArray(this Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
