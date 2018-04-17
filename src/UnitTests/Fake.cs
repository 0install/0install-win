// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall
{
    public static class Fake
    {
        public static readonly FeedUri Feed1Uri = new FeedUri("http://0install.de/feeds/test/test1.xml");
        public static readonly FeedUri Feed2Uri = new FeedUri("http://0install.de/feeds/test/test2.xml");
        public static readonly FeedUri SubFeed1Uri = new FeedUri("http://0install.de/feeds/test/sub1.xml");
        public static readonly FeedUri SubFeed2Uri = new FeedUri("http://0install.de/feeds/test/sub2.xml");
        public static readonly FeedUri SubFeed3Uri = new FeedUri("http://0install.de/feeds/test/sub3.xml");

        public static Feed Feed => new Feed
        {
            Uri = Feed1Uri,
            Name = "MyApp",
            Homepage = new Uri("http://0install.de/"),
            Summaries = {"Summary"},
            Elements =
            {
                new Implementation
                {
                    ID = "id1",
                    ManifestDigest = new ManifestDigest(sha256: "123"),
                    Version = new ImplementationVersion("1.0")
                }
            }
        };

        public static Selections Selections => new Selections
        {
            InterfaceUri = Feed1Uri,
            Command = Command.NameRun,
            Implementations =
            {
                new ImplementationSelection
                {
                    InterfaceUri = Feed1Uri,
                    FromFeed = SubFeed1Uri,
                    ID = "id1",
                    ManifestDigest = new ManifestDigest(sha256: "123"),
                    Version = new ImplementationVersion("1.0")
                },
                new ImplementationSelection
                {
                    InterfaceUri = Feed2Uri,
                    FromFeed = SubFeed2Uri,
                    ID = "id2",
                    ManifestDigest = new ManifestDigest(sha256: "abc"),
                    Version = new ImplementationVersion("1.0")
                }
            }
        };
    }
}
