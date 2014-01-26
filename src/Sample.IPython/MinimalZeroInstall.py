#import clr
#clr.AddReferenceToFile("ZeroInstall.Services.dll", "ZeroInstall.Model.dll")

import sys
from ZeroInstall import ServiceLocator, CliHandler
from ZeroInstall.Model import Requirements
from ZeroInstall.Injector import Executor

requirements = Requirements(InterfaceID = sys.argv[1]) # sys.argv[0]

services = ServiceLocator(CliHandler())
selections = services.Solver.Solve(requirements)
missing = services.SelectionsManager.GetUncachedImplementations(selections)
services.Fetcher.Fetch(missing)
services.Executor.Start(selections)
