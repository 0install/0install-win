using System.Drawing;

namespace Common
{
    /// <summary>
    /// An object that can be highlighted in list representations.
    /// </summary>
    public interface IHighlightable
    {
        /// <summary>
        /// The color to highlight this object with in list representations. <see cref="Color.Empty"/> for no highlighting.
        /// </summary>
        Color HighlightColor { get; }
    }
}
