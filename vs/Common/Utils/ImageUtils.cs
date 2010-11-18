using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Common.Utils
{
    public class ImageUtils
    {
        /// <summary>
        /// Guesses the <see cref="ImageFormat"/> of an image on base of its extension.
        /// </summary>
        /// <param name="path">Path to the image.</param>
        /// <returns>The guessed <see cref="ImageFormat"/> or null, if the format could not be guessed.</returns>
        public static ImageFormat GuessImageFormat(string path)
        {
            #region sanity checks
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            #endregion

            string extension = Path.GetExtension(path);

            if (extension == string.Empty)
                return null;

            if (StringUtils.Compare(extension, ".bmp"))
                return ImageFormat.Bmp;
            if (StringUtils.Compare(extension, ".emf"))
                return ImageFormat.Emf;
            if (StringUtils.Compare(extension, ".gif"))
                return ImageFormat.Gif;
            if (StringUtils.Compare(extension, ".ico"))
                return ImageFormat.Icon;
            if (StringUtils.Compare(extension, ".jpg"))
                return ImageFormat.Jpeg;
            if (StringUtils.Compare(extension, ".png"))
                return ImageFormat.Png;
            if (StringUtils.Compare(extension, ".tif"))
                return ImageFormat.Tiff;
            if (StringUtils.Compare(extension, ".wmf"))
                return ImageFormat.Wmf;

            return null;
        }

        /// <summary>
        /// Returns the <see cref="ImageFormat"/> of an image.
        /// </summary>
        /// <param name="path">Path to an image.</param>
        /// <returns>The <see cref="ImageFormat"/>.</returns>
        /// <exception cref="OutOfMemoryException">The file does not have a valid image format.
        ///-or-
        ///GDI+ does not support the pixel format of the file.</exception>
        /// <exception cref="FileNotFoundException">The specified file does not exist.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a <see cref="Uri"/>.</exception>
        /// <seealso cref="Image.FromFile(string)"/>
        public static ImageFormat GetImageFormat(string path)
        {
            #region sanity checks

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            #endregion

            using (var image = Image.FromFile(path))
            {
                return image.RawFormat;
            }
        }
    }
}