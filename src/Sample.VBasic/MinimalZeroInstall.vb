Imports ZeroInstall.Model
Imports ZeroInstall.Services

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Run(New Requirements() With {.InterfaceID = args(0)})
    End Sub

    Private Sub Run(requirements As Requirements)
        Dim services = New ServiceLocator(New CliHandler())
        With services
            Dim selections = .Solver.Solve(requirements)
            Dim missing = .SelectionsManager.GetUncachedImplementations(selections)
            .Fetcher.Fetch(missing)
            .Executor.Start(selections)
        End With
    End Sub
End Module