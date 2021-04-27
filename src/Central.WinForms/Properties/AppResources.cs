// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Drawing;
using SharedResources = ZeroInstall.Central.Properties.Resources;

namespace ZeroInstall.Central.WinForms.Properties
{
    /// <summary>
    /// Static preload of commonly used app-related resources.
    /// </summary>
    internal static class AppResources
    {
        public static readonly Bitmap
            CandidateImage = Resources.AppCandidate,
            AddedImage = Resources.AppAdded,
            IntegratedImage = Resources.AppIntegrated;

        public static readonly string
            CandidateText = SharedResources.MyAppsAdd,
            AddedText = SharedResources.Integrate,
            IntegratedText = SharedResources.MyAppsAddedAndIntegrate;

        public static readonly string
            RunText = SharedResources.Run,
            RunWithOptionsText = SharedResources.RunWithOptions,
            UpdateText = SharedResources.Update,
            IntegrateText = SharedResources.Integrate,
            ModifyText = SharedResources.ModifyIntegration,
            RemoveText = SharedResources.Remove,
            Working = SharedResources.Working;
    }
}
