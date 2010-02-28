namespace Common.Undo
{
    /// <summary>
    /// A executable command with an undo function
    /// </summary>
    public interface IUndoCommand
    {
        /// <summary>
        /// Performs the desired action
        /// </summary>
        void Execute();

        /// <summary>
        /// Undoes changes made by <see cref="Execute"/>
        /// </summary>
        void Undo();
    }
}