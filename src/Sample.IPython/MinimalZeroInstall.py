#import clr
#clr.AddReferenceToFile("ZeroInstall.Model.dll", "ZeroInstall.Services.dll")

import sys
from ZeroInstall.Model import Requirements
from ZeroInstall.Services import ServiceLocator, CliHandler

requirements = Requirements(InterfaceID = sys.argv[1])

services = ServiceLocator(CliHandler())
selections = services.Solver.Solve(requirements)
missing = services.SelectionsManager.GetUncachedImplementations(selections)
services.Fetcher.Fetch(missing)
services.Executor.Start(selections)
