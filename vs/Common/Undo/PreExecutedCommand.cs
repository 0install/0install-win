namespace Common.Undo
{
    /// <summary>
    /// An undo command that does nothing on the first <see cref="IUndoCommand.Execute"/> call, because the action was already performed beforehand
    /// </summary>
    public abstract class PreExecutedCommand : FirstExecuteCommand
    {
        /// <summary>
        /// Do nothing on first execute
        /// </summary>
        protected override sealed void OnFirstExecute()
        {}
    }
}
