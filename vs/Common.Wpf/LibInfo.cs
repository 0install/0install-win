using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Wpf
{
    public class LibInfo : Model
    {

        private String iconUrl = "";
        public String IconUrl
        {
            get
            {
                return this.iconUrl;
            }
            set
            {
                this.iconUrl = value;
                this.NotifyPropertyChanged("IconUrl");
            }
        }

        private List<LibInfo> dependencies = new List<LibInfo>();

        public List<LibInfo> Dependencies
        {
            get
            {
                return this.dependencies;
            }
            set
            {
                this.dependencies = value;
                this.NotifyPropertyChanged("Dependencies");
            }
        }

        private String name = "";
        public String Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        private String version = "";
        public String Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
                this.NotifyPropertyChanged("Version");
            }
        }

        private String publisher = "";
        public String Publisher
        {
            get
            {
                return this.publisher;
            }
            set
            {
                this.publisher = value;
                this.NotifyPropertyChanged("Publisher");
            }
        }

        private String size = "";
        public String Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
                this.NotifyPropertyChanged("Size");
            }
        }

            public LibInfo(String name, String version, String publisher, String size, String iconUrl)
        {
            this.Name = name;
            this.Version = version;
            this.Publisher = publisher;
            this.Size = size;
            this.IconUrl = iconUrl;
        }
    }
}
