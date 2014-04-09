/*
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

using System;
using System.Diagnostics.CodeAnalysis;

namespace NanoByte.Common.Controls
{

    #region Enumerations
    /// <summary>
    /// Mask indicating which fields in <see cref="TouchEventArgs"/> are valid.
    /// </summary>
    /// <seealso cref="TouchEventArgs.Mask"/>
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Justification = "The keyword mask implies flag usage without the need for a plural form")]
    [Flags]
    public enum TouchEventMask
    {
        /// <summary>TOUCHINPUTMASKF_TIMEFROMSYSTEM</summary>
        Time = 0x0001,

        /// <summary>TOUCHINPUTMASKF_EXTRAINFO</summary>
        ExtraInfo = 0x0002,

        /// <summary>TOUCHINPUTMASKF_CONTACTAREA</summary>
        ContactArea = 0x0004
    }
    #endregion

    /// <summary>
    /// Event information about a touch event.
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        /// <summary>
        /// Touch X client coordinate in pixels.
        /// </summary>
        public int LocationX { get; set; }

        /// <summary>
        /// Touch Y client coordinate in pixels.
        /// </summary>
        public int LocationY { get; set; }

        /// <summary>
        /// X size of the contact area in pixels.
        /// </summary>
        public int ContactX { get; set; }

        /// <summary>
        /// X size of the contact area in pixels.
        /// </summary>
        public int ContactY { get; set; }

        /// <summary>
        /// Contact ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Mask indicating which fields in the structure are valid.
        /// </summary>
        public TouchEventMask Mask { get; set; }

        /// <summary>
        /// Touch event time.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Indicates that this structure corresponds to a primary contact point.
        /// </summary>
        public bool Primary;

        /// <summary>
        /// The touch event came from the user's palm.
        /// </summary>
        public bool Palm;

        /// <summary>
        /// The user is hovering above the touch screen.
        /// </summary>
        public bool InRange;

        /// <summary>
        /// This input was not coalesced.
        /// </summary>
        public bool NoCoalesce;
    }
}
