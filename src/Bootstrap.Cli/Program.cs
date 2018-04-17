// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Tasks;

namespace ZeroInstall
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            ProgramUtils.Init();

            using (var handler = new CliTaskHandler())
                return (int)ProgramUtils.Run(args, handler, gui: false);
        }
    }
}
