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
            this.Icon = ResHelper.GetImage("ZeroInstall.Wpf", "Icon.ico");
        }


        void RaiseUnecoverableException_Click(object sender, RoutedEventArgs e)
        {
            throw new NullReferenceException("Null Reference Exception raised on primary UI thread.");
        }

        private void StartSecondaryWorkerThreadButton_Click(object sender, RoutedEventArgs e)
        {
            // Creates and starts a secondary thread in a single threaded apartment (STA)
            var thread = new Thread(this.MethodRunningOnSecondaryWorkerThread);
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
                int secondaryWorkerThreadId = Thread.CurrentThread.ManagedThreadId;
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
