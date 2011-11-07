using System.Globalization;
using Common;
using ZeroInstall.Model;

namespace ZeroInstall.Commands.WinForms
{
    partial class IntegrateAppForm
    {
        private static EntryPoint GenerateEntryPointFromFeed(Feed feed)
        {
            var point = new EntryPoint { Command = Command.NameRun };
            point.Descriptions.AddAll(feed.Descriptions);
            point.Icons.AddAll(feed.Icons);
            point.NeedsTerminal = feed.NeedsTerminal;
            point.Summaries.AddAll(feed.Summaries);

            return point;
        }

        private class EntryPointWrapper : ToStringWrapper<EntryPoint>
        {
            public EntryPointWrapper(EntryPoint entryPoint)
                : base(entryPoint, () => entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture))
            { }
        }
    }
}
