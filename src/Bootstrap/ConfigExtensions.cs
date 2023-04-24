// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

public static class ConfigExtensions
{
    public static void ReadFromBootstrapConfig(this Config config)
        => config.ReadFrom(BootstrapConfig.Instance.IniData);
}
