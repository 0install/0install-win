/*
 * Copyright 2006-2011 Bastian Eicher
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
using NUnit.Framework;

namespace Common.Controls
{
    /// <summary>
    /// Contains test methods for <see cref="TrackingProgressBar"/>.
    /// </summary>
    [TestFixture]
    public class TrackingProgressBarTest
    {
        private MockTask _task;
        private TrackingProgressBar _progressBar;

        [SetUp]
        public void SetUp()
        {
            _task = new MockTask();
            try { _progressBar = new TrackingProgressBar {Task = _task}; }
            catch (TypeInitializationException ex)
            {
                // Don't fail on Server systems
                throw new InconclusiveException("Unable to create GUI", ex);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_progressBar != null) _progressBar.Dispose();
        }

        [Test]
        public void TestNormal()
        {
            _progressBar.CreateControl();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(0, _progressBar.Value);

            _task.Start();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Marquee, _progressBar.Style);

            _task.MockStateData();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(50, _progressBar.Value);

            _task.MockStateComplete();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(100, _progressBar.Value);
        }

        [Test]
        public void TestAlreadyStarted()
        {
            _task.Start();
            _task.MockStateData();

            _progressBar.CreateControl();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(50, _progressBar.Value);

            _task.MockStateComplete();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(100, _progressBar.Value);
        }

        [Test]
        public void TestAlreadyComplete()
        {
            _task.Start();
            _task.MockStateData();
            _task.MockStateComplete();

            _progressBar.CreateControl();
            Application.DoEvents();
            Assert.AreEqual(ProgressBarStyle.Continuous, _progressBar.Style);
            Assert.AreEqual(100, _progressBar.Value);
        }
    }
}
