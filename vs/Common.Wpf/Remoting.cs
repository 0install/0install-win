using System;

namespace Common.Wpf
{
    public interface IRemoteCom
    {
        string DoWork(string message);
    }

    public class RemoteServerObject : System.MarshalByRefObject, IRemoteCom
    {
        public RemoteServerObject()
        {
            Console.WriteLine("Remote Object Activated.");
        }

        public string DoWork(string message)
        {
            return ("From Server: " + message);
        }
    }
}
