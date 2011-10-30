using System.ComponentModel;

namespace ZeroInstall.Commands.WinForms.AccessPointModels
{
    internal abstract class AccessPointModel
    {
        #region Variables
        /// <summary>
        /// Stores whether the <see cref="CapabilityModel.Capability" /> was already used or not.
        /// </summary>
        private readonly bool _wasUsed;

        /// <summary>
        /// Indicates whether the <see cref="CapabilityModel.Capability" /> shall be used or not.
        /// </summary>
        // ReSharper disable MemberCanBePrivate.Global
        public bool Use { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="Use" /> of the <see cref="CapabilityModel.Capability" /> has been changed.
        /// </summary>
        [Browsable(false)]
        public bool Changed { get { return _wasUsed != Use; } }
        #endregion

        protected AccessPointModel(bool used)
        {
            _wasUsed = Use = used;
        }
    }
}