Imports ZeroInstall.Backend
Imports ZeroInstall.Model
Imports ZeroInstall.Injector

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Run(New Requirements() With {.InterfaceID = args(0)})
    End Sub

    Private Sub Run(requirements As Requirements)
        Dim locator = New ServiceLocator(New CliHandler())
        With locator
            Dim selections = .Solver.Solve(requirements)
            Dim missing = .SelectionsManager.GetUncachedImplementations(selections)
            .Fetcher.Fetch(missing)
            Call New Executor(selections, .Store).Start()
        End With
    End Sub
End Module