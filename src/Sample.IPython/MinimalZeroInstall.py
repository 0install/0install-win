#import clr
#clr.AddReferenceToFile("ZeroInstall.Model.dll", "ZeroInstall.Injector.dll")

from ZeroInstall.Model import Requirements
from ZeroInstall.Injector import Resolver, CliHandler, Executor

def run(resolver, requirements):
    selections = resolver.Solver.Solve(requirements)
    missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
    resolver.Fetcher.Fetch(missing)
    Executor(selections, resolver.Store).Start()

import sys
run(
    Resolver(CliHandler()),
    Requirements(InterfaceID = sys.argv[0]))