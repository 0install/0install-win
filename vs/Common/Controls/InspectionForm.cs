using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// Contains a single property grid for inspecting and manipulating the properties of an arbitrary object.
    /// </summary>
    public sealed partial class InspectionForm : Form
    {
        public InspectionForm(object value)
        {
            InitializeComponent();

            propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Displays a property grid for manipulating the properties of an object.
        /// </summary>
        /// <param name="value">The object to be inspected.</param>
        //[LuaGlobal(Description = "Displays a property grid for manipulating the properties of an object.")]
        public static void Inspect(object value)
        {
            new InspectionForm(value).Show();
        }
    }
}
