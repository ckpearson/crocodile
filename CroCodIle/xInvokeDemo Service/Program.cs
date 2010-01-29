using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xInvoke;

namespace xInvokeDemo_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            TestService ts = new TestService();
            xInvokeService<ITestService> service = new xInvokeService<ITestService>(ts, "TestService", true);
            service.Begin();

            Console.WriteLine("Service Started, ENTER to exit");

            Console.WriteLine();
            Console.ReadLine();
            service.Stop();
        }
    }

    internal interface ITestService
    {
        void sendMessage(string message);
    }

    internal class TestService : ITestService
    {

        public void sendMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - Message Received\n\r\t" + message);
        }
    }
}
