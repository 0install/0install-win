/*
 * Copyright 2006-2013 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods for image files.
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// Guesses the <see cref="ImageFormat"/> of an image on base of its extension.
        /// </summary>
        /// <param name="path">Path to an image file.</param>
        /// <returns>The guessed <see cref="ImageFormat"/> or <see langword="null"/> if the format could not be guessed.</returns>
        public static ImageFormat GuessImageFormat(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            string extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension)) return null;
            if (StringUtils.EqualsIgnoreCase(extension, ".bmp")) return ImageFormat.Bmp;
            if (StringUtils.EqualsIgnoreCase(extension, ".emf")) return ImageFormat.Emf;
            if (StringUtils.EqualsIgnoreCase(extension, ".gif")) return ImageFormat.Gif;
            if (StringUtils.EqualsIgnoreCase(extension, ".ico")) return ImageFormat.Icon;
            if (StringUtils.EqualsIgnoreCase(extension, ".jpg")) return ImageFormat.Jpeg;
            if (StringUtils.EqualsIgnoreCase(extension, ".png")) return ImageFormat.Png;
            if (StringUtils.EqualsIgnoreCase(extension, ".tif")) return ImageFormat.Tiff;
            if (StringUtils.EqualsIgnoreCase(extension, ".wmf")) return ImageFormat.Wmf;
            return null;
        }

        /// <summary>
        /// Determines the <see cref="ImageFormat"/> of an image.
        /// </summary>
        /// <param name="path">Path to an image file.</param>
        /// <returns>The <see cref="ImageFormat"/>.</returns>
        /// <exception cref="OutOfMemoryException">Thrown if the file does not have a valid image format or GDI+ does not support the pixel format of the file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <exception cref="ArgumentException">hrown if <paramref name="path"/> is a <see cref="Uri"/>.</exception>
        /// <seealso cref="Image.FromFile(string)"/>
        public static ImageFormat GetImageFormat(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var image = Image.FromFile(path))
                return image.RawFormat;
        }
    }
}
