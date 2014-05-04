#import clr
#clr.AddReferenceToFile("ZeroInstall.Services.dll", "ZeroInstall.Store.dll")

import sys
from NanoByte.Common.Tasks import CliTaskHandler
from ZeroInstall.Services import ServiceLocator
from ZeroInstall.Store.Model import Requirements

requirements = Requirements(InterfaceID = sys.argv[1])

services = ServiceLocator(CliTaskHandler())
selections = services.Solver.Solve(requirements)
missing = services.SelectionsManager.GetUncachedImplementations(selections)
services.Fetcher.Fetch(missing)
services.Executor.Start(selections)
