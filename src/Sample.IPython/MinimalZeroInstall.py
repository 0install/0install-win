#import clr
#clr.AddReferenceToFile("ZeroInstall.Services.dll", "ZeroInstall.Model.dll")

import sys
from ZeroInstall import ServiceLocator, CliHandler
from ZeroInstall.Model import Requirements
from ZeroInstall.Injector import Executor

requirements = Requirements(InterfaceID = sys.argv[1]) # sys.argv[0]

locator = ServiceLocator(CliHandler())
selections = locator.Solver.Solve(requirements)
missing = locator.SelectionsManager.GetUncachedImplementations(selections)
locator.Fetcher.Fetch(missing)
Executor(selections, locator.Store).Start()
