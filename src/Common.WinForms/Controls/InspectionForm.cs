﻿/*
 * Copyright 2006-2014 Bastian Eicher
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

using System.Windows.Forms;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Contains a single property grid for inspecting and manipulating the properties of an arbitrary object.
    /// </summary>
    public sealed partial class InspectionForm : Form
    {
        ///<summary>
        /// Creates a new inspection form.
        ///</summary>
        ///<param name="value">The object to be inspected.</param>
        public InspectionForm(object value)
        {
            InitializeComponent();

            propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Displays a property grid for manipulating the properties of an object.
        /// </summary>
        /// <param name="value">The object to be inspected.</param>
        public static void Inspect(object value)
        {
            new InspectionForm(value).Show();
        }
    }
}
