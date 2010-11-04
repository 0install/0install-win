using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Wpf
{
    public class ResHelper
    {
        public static ImageSource GetImage(String assembly, String path)
        {
            return new BitmapImage(new Uri(String.Format("pack://application:,,,/{0};component/{1}", assembly, path)));
        }
        
    }
}
