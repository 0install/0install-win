using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZeroInstall.Central.Wpf
{
	/// <summary>
	/// Interaction logic for AddApplication.xaml
	/// </summary>
	public partial class AddApplication : Window
	{
		public AddApplication()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

		private void Window_Drop(object sender, System.Windows.DragEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}
	}
}