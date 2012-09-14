/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Windows.Forms;
using Common.Tasks;
using NUnit.Framework;

namespace Common.Controls
{
    /// <summary>
    /// Contains test methods for <see cref="TrackingLabel"/>.
    /// </summary>
    [TestFixture]
    public class TrackingLabelTest
    {
        private MockTask _task;
        private TrackingLabel _label;

        [SetUp]
        public void SetUp()
        {
            _task = new MockTask();
            try
            {
                _label = new TrackingLabel();
            }
            catch (TypeInitializationException ex)
            {
                // Don't fail on headless systems
                throw new InconclusiveException("Cannot test GUI on headless systems", ex);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_label != null) _label.Dispose();
        }

        [Test]
        public void TestNormal()
        {
            _label.CreateControl();
            _label.Task = _task;
            Application.DoEvents();
            Assert.AreEqual(TaskState.Ready, _label.CurrentState);

            _task.Start();
            _task.MockStateData();
            Application.DoEvents();
            Assert.AreEqual("64 / 128", _label.Text);

            _task.MockStateComplete();
            Application.DoEvents();
            Assert.AreEqual(TaskState.Complete, _label.CurrentState);
        }

        [Test]
        public void TestAlreadyStarted()
        {
            _task.Start();
            _task.MockStateData();

            _label.CreateControl();
            _label.Task = _task;
            Application.DoEvents();
            Assert.AreEqual("64 / 128", _label.Text);

            _task.MockStateComplete();
            Application.DoEvents();
            Assert.AreEqual(TaskState.Complete, _label.CurrentState);
        }

        [Test]
        public void TestAlreadyComplete()
        {
            _task.Start();
            _task.MockStateData();
            _task.MockStateComplete();

            _label.CreateControl();
            _label.Task = _task;
            Application.DoEvents();
            Assert.AreEqual(TaskState.Complete, _label.CurrentState);
        }
    }
}
