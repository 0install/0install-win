Imports NanoByte.Common.Tasks
Imports ZeroInstall.Services
Imports ZeroInstall.Store
Imports ZeroInstall.Store.Model

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Run(New Requirements() With {.InterfaceUri = New FeedUri(args(0))})
    End Sub

    Private Sub Run(requirements As Requirements)
        Dim services = New ServiceLocator(New CliTaskHandler())
        With services
            Dim selections = .Solver.Solve(requirements)
            Dim missing = .SelectionsManager.GetUncachedImplementations(selections)
            .Fetcher.Fetch(missing)
            .Executor.Start(selections)
        End With
    End Sub
End Module