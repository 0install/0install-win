Imports NanoByte.Common.Tasks
Imports ZeroInstall.Services
Imports ZeroInstall.Store.Model

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
        Dim services = New ServiceLocator(New CliTaskHandler())
        With services
            Dim selections = .Solver.Solve(New Requirements(args(0)))
            Dim missing = .SelectionsManager.GetUncachedImplementations(selections)
            .Fetcher.Fetch(missing)
            .Executor.Start(selections)
        End With
    End Sub
End Module