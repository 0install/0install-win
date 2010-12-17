using System.ComponentModel;

namespace Common.Wpf
{
    /// <summary>
    /// Derive from this class if your class should implement "INotifyPropertyChanged".
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Empty constructor
        public Model()
        {}
        #endregion
    }
}
