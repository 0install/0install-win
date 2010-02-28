using System;
using System.Reflection;
using System.Windows.Forms;

namespace ZeroInstall
{
    internal partial class ConfigForm : Form
    {
        public ConfigForm(Settings settings)
        {
            InitializeComponent();

            foreach (FieldInfo field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                listBox.Items.Add(field.GetValue(settings));
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = listBox.SelectedItem;
        }
    }
}
