using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xInvoke;

namespace xInvokeDemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TestServiceClient client = new TestServiceClient();
            Console.WriteLine("Client running");
            bool close = false;
            do
            {
                Console.Write("Message? (Q) Quits: ");
                string msg = Console.ReadLine();
                if (msg.ToLower().Trim() == "q")
                {
                    close = true;
                }
                if (!close)
                {
                    client.sendMessage(msg);
                }
            } while (close == false);
        }
    }

    internal interface ITestService
    {
        void sendMessage(string message);
    }

    internal class TestServiceClient : ITestService
    {
        private xInvokeClient<ITestService> _client = null;

        public TestServiceClient()
        {
            _client = new xInvokeClient<ITestService>("TestService", true);
            _client.Begin();
        }

        public void sendMessage(string message)
        {
            _client.DoCall(new object[] { message });
        }
    }
}
