using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Wpf
{
    public class StepController
    {
        public List<IStep> Steps { get; set; }
        public int Index { get; private set; }

        public StepController()
        {
            this.Steps = new List<IStep>();
            this.Index = -1;
        }

        public void Start()
        {
            while (this.Index > 0)
            {
                this.Previous();
            }
            this.Next();
        }

        public void Next()
        {
            this.Index++;
            this.Steps[this.Index].Main();
        }

        public void Previous()
        {
            this.Steps[this.Index].Rollback();
            this.Index--;
            this.Steps[this.Index].Main();
        }
    }

    public interface IStep
    {
        void Main();
        void Rollback();
    }

    public class Step : IStep
    {
        private Action ActMain { get; set; }
        private Action ActRollback { get; set; }

        public Step(Action main)
        {
            this.ActMain = main;
        }

        public Step(Action main, Action rollback)
            : this(main)
        {
            this.ActRollback = rollback;
        }

        public virtual void Main()
        {
            this.ActMain();
        }
        public virtual void Rollback()
        {
            this.ActRollback();
        }
    }
}
