using System.Reflection;
using System.IO;

namespace Common.Compression
{
    /// <summary>
    /// Utility class that provides access to the test data contained in the
    /// assembly as resources.
    /// </summary>
    public static class TestData
    {
        private static readonly Assembly testDataAssembly = Assembly.GetAssembly(typeof(TestData));

        public static Stream GetSdlZipArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.zip");
        }

        public static Stream GetSdlTarArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.tar");
        }

        public static Stream GetSdlTarGzArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.tar.gz");
        }

        public static Stream GetSdlTarBz2ArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.tar.bz2");
        }

        public static Stream GetSdlTarLzmaArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.tar.lzma");
        }

        public static Stream GetSdlDllStream()
        {
            return GetTestDataResourceStreamByName("SDL.dll");
        }

        public static Stream GetSdlReadmeStream()
        {
            return GetTestDataResourceStreamByName("README-SDL.txt");
        }

        private static Stream GetTestDataResourceStreamByName(string name)
        {
            return testDataAssembly.GetManifestResourceStream(typeof(TestData), name);
        }
    }
}
