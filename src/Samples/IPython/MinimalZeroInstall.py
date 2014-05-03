#import clr
#clr.AddReferenceToFile("ZeroInstall.Services.dll", "ZeroInstall.Store.dll")

import sys
from ZeroInstall.Services import ServiceLocator
from ZeroInstall.Store import CliServiceHandler
from ZeroInstall.Store.Model import Requirements

requirements = Requirements(InterfaceID = sys.argv[1])

services = ServiceLocator(CliServiceHandler())
selections = services.Solver.Solve(requirements)
missing = services.SelectionsManager.GetUncachedImplementations(selections)
services.Fetcher.Fetch(missing)
services.Executor.Start(selections)
