/*
 * Copyright 2010 Dennis Keil
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

using System.Collections.Generic;

namespace Common.Wpf
{
    public class LibInfo : Model
    {
        private string _iconUrl = "";
        public string IconUrl
        {
            get
            {
                return _iconUrl;
            }
            set
            {
                _iconUrl = value;
                NotifyPropertyChanged("IconUrl");
            }
        }

        private List<LibInfo> _dependencies = new List<LibInfo>();
        public List<LibInfo> Dependencies
        {
            get
            {
                return _dependencies;
            }
            set
            {
                _dependencies = value;
                NotifyPropertyChanged("Dependencies");
            }
        }

        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string version = "";
        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
                NotifyPropertyChanged("Version");
            }
        }

        private string _publisher = "";
        public string Publisher
        {
            get
            {
                return _publisher;
            }
            set
            {
                _publisher = value;
                NotifyPropertyChanged("Publisher");
            }
        }

        private string _size = "";
        public string Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                NotifyPropertyChanged("Size");
            }
        }

        public LibInfo(string name, string version, string publisher, string size, string iconUrl)
        {
            Name = name;
            Version = version;
            Publisher = publisher;
            Size = size;
            IconUrl = iconUrl;
        }
    }
}
