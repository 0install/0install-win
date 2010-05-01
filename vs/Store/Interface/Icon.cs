using System;
using System.Drawing;
using System.Net;

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
