// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.ServiceProcess;
using NanoByte.Common.Native;
using ZeroInstall.Store.Service;

// Encode installation path into mutex name to allow instance detection during updates
string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
if (AppMutex.Probe(mutexName + "-update")) return 999;

// NOTE: Do not block updater from starting because it will automatically stop service

ServiceBase.Run(new ServiceBase[] {new StoreService()});
return 0;
