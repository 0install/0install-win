/*
 * Copyright 2010 Dennis Keil
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

namespace Common.Wpf
{
    public class StepController
    {
        public List<IStep> Steps { get; set; }
        public int Index { get; private set; }

        public StepController()
        {
            Steps = new List<IStep>();
            Index = -1;
        }

        public void Start()
        {
            while (Index > 0)
            {
                Previous();
            }
            Next();
        }

        public void Next()
        {
            Index++;
            Steps[Index].Main();
        }

        public void Previous()
        {
            Steps[Index].Rollback();
            Index--;
            Steps[Index].Main();
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
            ActMain = main;
        }

        public Step(Action main, Action rollback)
            : this(main)
        {
            ActRollback = rollback;
        }

        public virtual void Main()
        {
            ActMain();
        }
        public virtual void Rollback()
        {
            ActRollback();
        }
    }
}
