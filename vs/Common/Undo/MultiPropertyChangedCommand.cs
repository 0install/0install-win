using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Undo
{
    /// <summary>
    /// An undo command that handles multiple changed properties - usually used with a <see cref="PropertyGrid"/>
    /// </summary>
    public class MultiPropertyChangedCommand : PreExecutedCommand
    {
        #region Variables
        private readonly object[] _targets;
        private readonly PropertyDescriptor _property;
        private readonly object[] _oldValues;
        private readonly object _newValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the command after the properties were first changed
        /// </summary>
        /// <param name="targets">The objects the property belongs to</param>
        /// <param name="property">The property that was changed</param>
        /// <param name="oldValues">The property's old values</param>
        /// <param name="newValue">The property's current value</param>
        public MultiPropertyChangedCommand(object[] targets, PropertyDescriptor property, object[] oldValues, object newValue)
        {
            #region Sanity checks
            if (targets == null) throw new ArgumentNullException("targets");
            if (targets.Length != oldValues.Length) throw new ArgumentException(Resources.TargetsOldValuesLength, "targets");
            if (property == null) throw new ArgumentNullException("property");
            if (oldValues == null) throw new ArgumentNullException("oldValues");
            #endregion

            _targets = targets;
            _property = property;
            _oldValues = oldValues;
            _newValue = newValue;
        }

        /// <summary>
        /// Initializes the command after the property was first changed
        /// </summary>
        /// <param name="targets">The objects the <see cref="PropertyGrid.SelectedObject"/> is target at</param>
        /// <param name="e">The event data from the <see cref="PropertyGrid.PropertyValueChanged"/></param>
        /// <param name="oldValues">The property's old values</param>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "This is simply a comfort wrapper for extracting values from the event arguments")]
        public MultiPropertyChangedCommand(object[] targets, PropertyValueChangedEventArgs e, object[] oldValues)
            : this(targets, e.ChangedItem.PropertyDescriptor, oldValues, e.ChangedItem.Value)
        {}
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Set the changed property value again
        /// </summary>
        protected override void OnRedo()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                // Use refelction to get the specific property for each object and set the new value everywhere
                _targets[i].GetType().GetProperty(_property.Name).SetValue(_targets[i], _newValue, null);
            }
        }

        /// <summary>
        /// Restore the original property values
        /// </summary>
        protected override void OnUndo()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                // Use reflection to get the specific property for each object and set the corresponding old value for each
                _targets[i].GetType().GetProperty(_property.Name).SetValue(_targets[i], _oldValues[i], null);
            }
        }
        #endregion
    }
}
