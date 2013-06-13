#import clr
#clr.AddReferenceToFile("ZeroInstall.Backend.dll", "ZeroInstall.Model.dll", "ZeroInstall.Injector.dll")

import sys
from ZeroInstall.Backend import Resolver, CliHandler
from ZeroInstall.Model import Requirements
from ZeroInstall.Injector import Executor

resolver = Resolver(CliHandler())
requirements = Requirements(InterfaceID = sys.argv[0]) # sys.argv[1]

selections = resolver.Solver.Solve(requirements)
missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
resolver.Fetcher.Fetch(missing)
Executor(selections, resolver.Store).Start()
