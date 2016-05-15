import clr
clr.AddReferenceToFile("NanoByte.Common.dll", "ZeroInstall.Services.dll", "ZeroInstall.Store.dll")

import sys
from NanoByte.Common.Tasks import CliTaskHandler
from ZeroInstall.Services import ServiceLocator
from ZeroInstall.Store import FeedUri
from ZeroInstall.Store.Model import Requirements

services = ServiceLocator(CliTaskHandler())

selections = services.Solver.Solve(Requirements(sys.argv[1]))
missing = services.SelectionsManager.GetUncachedImplementations(selections)
services.Fetcher.Fetch(missing)
services.Executor.Start(selections)
