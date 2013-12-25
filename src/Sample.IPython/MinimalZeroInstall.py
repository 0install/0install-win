#import clr
#clr.AddReferenceToFile("ZeroInstall.Backend.dll", "ZeroInstall.Model.dll", "ZeroInstall.Injector.dll")

import sys
from ZeroInstall.Backend import ServiceLocator, CliHandler
from ZeroInstall.Model import Requirements
from ZeroInstall.Injector import Executor

requirements = Requirements(InterfaceID = sys.argv[0]) # sys.argv[1]

locator = ServiceLocator(CliHandler())
selections = locator.Solver.Solve(requirements)
missing = locator.SelectionsManager.GetUncachedImplementations(selections)
locator.Fetcher.Fetch(missing)
Executor(selections, locator.Store).Start()
