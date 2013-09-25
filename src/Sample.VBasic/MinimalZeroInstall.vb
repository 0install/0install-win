Imports ZeroInstall.Injector
Imports ZeroInstall.Model
Imports ZeroInstall.Backend

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Dim resolver = New Resolver(New CliHandler())
        Dim requirements = New Requirements() With {.InterfaceID = args(0)}

        Dim selections = Resolver.Solver.Solve(requirements)
        Dim missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
        resolver.Fetcher.Fetch(missing)
        Call New Executor(selections, resolver.Store).Start()
    End Sub
End Module