/*
 * Copyright 2010-2015 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    /// <summary>
    /// Contains extension methods for <see cref="CapabilityModel"/> <see cref="BindingList{T}"/>s.
    /// </summary>
    public static class CapabilityModelExtensions
    {
        /// <summary>
        /// Sets all <see cref="CapabilityModel.Use"/> values within a list/model to a specific value.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="model">A model represeting the underlying <see cref="Store.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        /// <param name="value">The value to set.</param>
        public static void SetAllUse<T>([NotNull, ItemNotNull] this BindingList<T> model, bool value)
            where T : CapabilityModel
        {
            #region Sanity checks
            if (model == null) throw new ArgumentNullException("model");
            #endregion

            foreach (var element in model.Except(element => element.Capability.ExplicitOnly))
                element.Use = value;
            model.ResetBindings();
        }
    }
}
