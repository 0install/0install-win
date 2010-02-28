using System;
using Common.Properties;

namespace Common.Undo
{
    /// <summary>
    /// An undo command that automatically tracks when <see cref="Execute"/> and <see cref="Undo"/> can be called
    /// </summary>
    public abstract class SimpleCommand : IUndoCommand
    {
        #region Variables
        private bool _actionPerformed;
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Performs the desired action
        /// </summary>
        public void Execute()
        {
            // We cannot perform the action repeatedly in a row
            if (_actionPerformed) throw new InvalidOperationException(Resources.RedoNotAvailable);

            OnExecute();

            // Ready for undo, don't redo
            _actionPerformed = true;
        }

        /// <summary>
        /// Hook to perform the desired action
        /// </summary>
        protected abstract void OnExecute();
        #endregion

        #region Undo
        /// <summary>
        /// Undoes the changes made by <see cref="Execute"/>
        /// </summary>
        public virtual void Undo()
        {
            // If the action has not been performed yet, we cannnot undo it
            if (!_actionPerformed) throw new InvalidOperationException(Resources.UndoNotAvailable);

            OnUndo();

            // As if the action had never happened
            _actionPerformed = false;
        }

        /// <summary>
        /// Hook to undo the changes made by <see cref="OnExecute"/>
        /// </summary>
        protected abstract void OnUndo();
        #endregion
    }
}
