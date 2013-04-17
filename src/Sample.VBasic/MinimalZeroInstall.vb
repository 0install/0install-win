Imports ZeroInstall.Model
Imports ZeroInstall.Injector
Imports ZeroInstall.Injector.Solver

Module MinimalZeroInstall
    Sub Main(ByVal args As String())
      Run(New Requirements() With {.InterfaceID = args(0)})
    End Sub

    Private Sub Run(requirements As Requirements)
        Dim myPolicy = Policy.CreateDefault(New CliHandler())
        Dim selections = myPolicy.Solve(requirements)
        Dim missing = selections.GetUncachedImplementations(myPolicy)
        myPolicy.Fetch(missing)
        Call New Executor(selections, myPolicy.Fetcher.Store).Start()
    End Sub
End Module