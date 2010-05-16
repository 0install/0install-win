/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace ZeroInstall.Store.Interface
{
    /// <summary>
    /// Provides access to the interface icon cache.
    /// </summary>
    public static class Icon
    {
        //public static Image GetIcon(Model.Icon icon)
        //{
        //    throw new NotImplementedException();

        //    Image image;

        //    var fileRequest = WebRequest.Create(icon.Location);
        //    var fileReponse = fileRequest.GetResponse();

        //    try
        //    {
        //        Stream stream = fileReponse.GetResponseStream();
        //        image = Image.FromStream(stream);
        //    }
        //    catch (ArgumentException)
        //    {
        //        lblIconUrlError.Text = "URL does not describe an image";
        //        return;
        //    }

        //    switch (icon.MimeType)
        //    {
        //        case "image/png":
        //            if (!image.RawFormat.Equals(ImageFormat.Png)) throw new Exception();
        //            break;
        //        case "image/vnd-microsoft-icon":
        //            if (!image.RawFormat.Equals(ImageFormat.Icon)) throw new Exception();
        //            break;
        //    }
        //}
    }
}
