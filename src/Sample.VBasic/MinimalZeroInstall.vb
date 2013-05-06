Imports ZeroInstall.Model
Imports ZeroInstall.Injector

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Run(
            New Resolver(New CliHandler()),
            New Requirements() With {.InterfaceID = args(0)})
    End Sub

    Private Sub Run(resolver As Resolver, requirements As Requirements)
        Dim selections = Resolver.Solver.Solve(requirements)
        Dim missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
        resolver.Fetcher.Fetch(missing)
        Call New Executor(selections, resolver.Store).Start()
    End Sub
End Module