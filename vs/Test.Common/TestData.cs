using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace Common
{
    /// <summary>
    /// Utility class that provides access to the test data contained in the
    /// assembly as resources.
    /// </summary>
    public static class TestData
    {
        private static readonly Assembly testDataAssembly = Assembly.GetAssembly(typeof(TestData));

        public static Stream GetSdlArchiveStream()
        {
            return GetTestDataResourceStreamByName("sdlArchive.zip");
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
