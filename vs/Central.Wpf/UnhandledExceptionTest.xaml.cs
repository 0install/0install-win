/*
 * Copyright 2010 Dennis Keil
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using Common.Wpf;

namespace ZeroInstall.Central.Wpf
{
    /// <summary>
    /// Interaktionslogik für UnhandledExceptionTest.xaml
    /// </summary>
    public partial class UnhandledExceptionTest : Window
    {
        public UnhandledExceptionTest()
        {
            InitializeComponent();
            Icon = ResHelper.GetImage("0install-wpf", "Icon.ico");
        }


        void RaiseUnecoverableException_Click(object sender, RoutedEventArgs e)
        {
            throw new NullReferenceException("Null Reference Exception raised on primary UI thread.");
        }

        private void StartSecondaryWorkerThreadButton_Click(object sender, RoutedEventArgs e)
        {
            // Creates and starts a secondary thread in a single threaded apartment (STA)
            var thread = new Thread(MethodRunningOnSecondaryWorkerThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void MethodRunningOnSecondaryWorkerThread()
        {
            try
            {
                WorkerMethod();
            }
            catch (Exception ex)
            {
                // Dispatch the exception back to the main UI thread. Then, reraise
                // the exception on the main UI thread and handle it from the handler 
                // the Application object's DispatcherUnhandledException event.
                //int secondaryWorkerThreadId = Thread.CurrentThread.ManagedThreadId;
                Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        (DispatcherOperationCallback)(arg =>
                        {
                            // THIS CODE RUNS BACK ON THE MAIN UI THREAD
                            throw ex;
                        }),
                        null);

                // NOTE - Application execution will only continue from this point
                //        onwards if the exception was handled on the main UI thread.
                //        by Application.DispatcherUnhandledException
            }
        }

        private void WorkerMethod()
        {
            // This method would do real processing on the secondary worker thread.
            // For the purposes of this sample, it throws an index out of range exception
            string msg = string.Format(
                    "Index out of range exception raised on secondary worker thread {0}.",
                    Dispatcher.CurrentDispatcher.Thread.ManagedThreadId);
            throw new IndexOutOfRangeException(msg);
        }
    }
}
