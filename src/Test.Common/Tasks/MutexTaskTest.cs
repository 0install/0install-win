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
using System.Threading;
using NUnit.Framework;

namespace Common.Tasks
{
    /// <summary>
    /// Contains test methods for <see cref="MutexTask"/>.
    /// </summary>
    [TestFixture]
    public class MutexTaskTest
    {
        [Test(Description = "Ensures the task waits until the mutex becomes free.")]
        public void TestWait()
        {
            using (var mutex = new Mutex())
            {
                mutex.WaitOne();

                var task = new MutexTask("Test task", mutex);
                task.Start();
                Assert.AreEqual(TaskState.Started, task.State);

                mutex.ReleaseMutex();
                task.Join();
                Assert.AreEqual(TaskState.Complete, task.State);
            }
        }

        [Test(Description = "Starts waiting for an everblocking mutex using Start() and stops again right away using Cancel().")]
        public void TestCancelAsync()
        {
            using (var mutex = new Mutex())
            {
                mutex.WaitOne();

                var task = new MutexTask("Test task", mutex);
                task.Start();
                task.Cancel();

                Assert.AreEqual(TaskState.Ready, task.State);
            }
        }

        [Test(Description = "Starts waiting for an everblocking mutex using RunSync() and stops again right away using Cancel().")]
        public void TestCancelSync()
        {
            using (var mutex = new Mutex())
            {
                mutex.WaitOne();

                // Monitor for a cancellation exception
                var task = new MutexTask("Test task", mutex);
                bool exceptionThrown = false;
                var waitThread = new Thread(delegate()
                {
                    try
                    {
                        task.RunSync(null);
                    }
                    catch (OperationCanceledException)
                    {
                        exceptionThrown = true;
                    }
                });

                // Start and then cancel the download
                waitThread.Start();
                Thread.Sleep(100);
                task.Cancel();
                waitThread.Join();

                Assert.IsTrue(exceptionThrown, task.State.ToString());
            }
        }
    }
}
