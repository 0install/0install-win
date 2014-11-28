using System;
using System.IO;
using NanoByte.Common.Native;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="ModelUtils"/>.
    /// </summary>
    [TestFixture]
    public class ModelUtilsTest
    {
        [Test]
        public void TestContainsTemplateVariables()
        {
            Assert.IsFalse(ModelUtils.ContainsTemplateVariables(""));
            Assert.IsFalse(ModelUtils.ContainsTemplateVariables("x"));
            Assert.IsFalse(ModelUtils.ContainsTemplateVariables("}{"));
            Assert.IsTrue(ModelUtils.ContainsTemplateVariables("{}"));
            Assert.IsTrue(ModelUtils.ContainsTemplateVariables("{var}"));
            Assert.IsTrue(ModelUtils.ContainsTemplateVariables("x{var}x"));
        }

        [Test]
        public void TestGetAbsolutePath()
        {
            string absolutePath = WindowsUtils.IsWindows ? @"C:\local\subdir\file" : "/local/subdir/file";

            string result = ModelUtils.GetAbsolutePath("subdir/file", new FeedUri(WindowsUtils.IsWindows ? @"C:\local\" : "/local/"));
            Assert.IsTrue(Path.IsPathRooted(result));
            Assert.AreEqual(absolutePath, result);

            Assert.AreEqual(absolutePath, ModelUtils.GetAbsolutePath(absolutePath, null), "Should ignore source if path is already absolute.");
        }

        [Test]
        public void TestGetAbsolutePathException()
        {
            Assert.Throws<IOException>(() => ModelUtils.GetAbsolutePath("subdir/file", null));
            Assert.Throws<IOException>(() => ModelUtils.GetAbsolutePath("subdir/file", new FeedUri("http://remote/")));
        }

        [Test]
        public void TestGetAbsoluteHref()
        {
            Uri absoluteHref = WindowsUtils.IsWindows ? new Uri("file:///C:/local/subdir/file") : new Uri("file:///local/subdir/file");

            var result = ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), new FeedUri(WindowsUtils.IsWindows ? @"C:\local\feed.xml" : "/local/feed.xml"));
            Assert.IsTrue(result.IsAbsoluteUri);
            Assert.AreEqual(absoluteHref, result);

            Assert.AreEqual(absoluteHref, ModelUtils.GetAbsoluteHref(absoluteHref, null), "Should ignore source if href is already absolute.");
        }

        [Test]
        public void TestGetAbsoluteHrefException()
        {
            Assert.Throws<IOException>(() => ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), null));
            Assert.Throws<IOException>(() => ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), new FeedUri("http://remote/")));
        }
    }
}
