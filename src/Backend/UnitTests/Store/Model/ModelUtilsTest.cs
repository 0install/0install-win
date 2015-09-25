using System;
using System.IO;
using FluentAssertions;
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
            ModelUtils.ContainsTemplateVariables("").Should().BeFalse();
            ModelUtils.ContainsTemplateVariables("x").Should().BeFalse();
            ModelUtils.ContainsTemplateVariables("}{").Should().BeFalse();
            ModelUtils.ContainsTemplateVariables("{}").Should().BeTrue();
            ModelUtils.ContainsTemplateVariables("{var}").Should().BeTrue();
            ModelUtils.ContainsTemplateVariables("x{var}x").Should().BeTrue();
        }

        [Test]
        public void TestGetAbsolutePath()
        {
            string absolutePath = WindowsUtils.IsWindows ? @"C:\local\subdir\file" : "/local/subdir/file";

            string result = ModelUtils.GetAbsolutePath("subdir/file", new FeedUri(WindowsUtils.IsWindows ? @"C:\local\" : "/local/"));
            Path.IsPathRooted(result).Should().BeTrue();
            result.Should().Be(absolutePath);

            ModelUtils.GetAbsolutePath(absolutePath)
                .Should().Be(absolutePath, because: "Should ignore source if path is already absolute.");
        }

        [Test]
        public void TestGetAbsolutePathException()
        {
            Assert.Throws<UriFormatException>(() => ModelUtils.GetAbsolutePath("subdir/file"));
            Assert.Throws<UriFormatException>(() => ModelUtils.GetAbsolutePath("subdir/file", new FeedUri("http://remote/")));
        }

        [Test]
        public void TestGetAbsoluteHref()
        {
            Uri absoluteHref = WindowsUtils.IsWindows ? new Uri("file:///C:/local/subdir/file") : new Uri("file:///local/subdir/file");

            var result = ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), new FeedUri(WindowsUtils.IsWindows ? @"C:\local\feed.xml" : "/local/feed.xml"));
            result.IsAbsoluteUri.Should().BeTrue();
            result.Should().Be(absoluteHref);

            ModelUtils.GetAbsoluteHref(absoluteHref)
                .Should().Be(absoluteHref, because: "Should ignore source if href is already absolute.");
        }

        [Test]
        public void TestGetAbsoluteHrefException()
        {
            Assert.Throws<UriFormatException>(() => ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative)));
            Assert.Throws<UriFormatException>(() => ModelUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), new FeedUri("http://remote/")));
        }
    }
}
