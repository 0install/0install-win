// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Drawing;

namespace ZeroInstall.Central.WinForms.Properties
{
    /// <summary>
    /// Static preload of commonly used app-related resources.
    /// </summary>
    internal static class AppResources
    {
        public static readonly ScalableImage
            CandidateImage = new(ImageResources.AppCandidate),
            AddedImage = new(ImageResources.AppAdded),
            IntegratedImage = new (ImageResources.AppIntegrated);

        public static readonly string
            CandidateText = Resources.MyAppsAdd,
            AddedText = Resources.MyAppsAdded,
            IntegratedText = Resources.MyAppsAddedAndIntegrate;

        public static readonly string
            RunText = Resources.Run,
            RunWithOptionsText = Resources.RunWithOptions,
            UpdateText = Resources.Update,
            IntegrateText = Resources.Integrate,
            ModifyText = Resources.ModifyIntegration,
            RemoveText = Resources.Remove,
            Working = Resources.Working;
    }
}
