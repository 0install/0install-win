#import clr
#clr.AddReferenceToFile("ZeroInstall.Model.dll", "ZeroInstall.Injector.dll")

from ZeroInstall.Model import Architecture, Requirements
from ZeroInstall.Injector import Policy, CliHandler, Executor
from ZeroInstall.Injector.Solver import SelectionsUtils

def run(requirements):
    policy = Policy.CreateDefault(CliHandler())
    selections = policy.Solve(requirements)
    missing = SelectionsUtils.GetUncachedImplementations(selections, policy)
    policy.Fetch(missing)
    Executor(selections, policy.Fetcher.Store).Start()

import sys
run(Requirements(InterfaceID = sys.argv[0]))