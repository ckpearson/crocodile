using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheCodeKing.Net.Messaging;
using System.Diagnostics;
using System.Xml.Linq;
using System.Reflection;

namespace xInvoke
{
    /// <summary>
    /// Allows an application to call methods on a service with the same contract
    /// </summary>
    /// <typeparam name="T">The contract (Interface) to use</typeparam>
    public sealed class xInvokeClient<T>
    {
        private xInvokeContract _contract = null;
        private string _serviceName = string.Empty;

        private IXDBroadcast _sender = null;
        private IXDListener _listener = null;

        /// <summary>
        /// Instantiates the instance
        /// </summary>
        /// <param name="ServiceName">The name of the service to call to</param>
        public xInvokeClient(string ServiceName)
        {
            _contract = new xInvokeContract(typeof(T));
            if (String.IsNullOrEmpty(ServiceName))
            {
                throw new ArgumentOutOfRangeException("ServiceName");
            }


            _serviceName = ServiceName;

            _sender = XDBroadcast.CreateBroadcast(XDTransportMode.IOStream);
            _listener = XDListener.CreateListener(XDTransportMode.IOStream);

            _listener.MessageReceived += new XDListener.XDMessageHandler(_listener_MessageReceived);
        }

        ~xInvokeClient()
        {
            //Stop();
        }

        private void _listener_MessageReceived(object sender, XDMessageEventArgs e)
        {
            CurrentReturnValue = e.DataGram.Message.Deserialise();
            CurrentCallHasReturnValue = true;
        }

        /// <summary>
        /// Calls the current contract method on the remote service
        /// </summary>
        /// <param name="parameters">The parameters</param>
        public void DoCall(object[] parameters)
        {
            _intCall(parameters);
        }

        /// <summary>
        /// Calls the current contract method on the remote service
        /// </summary>
        public void DoCall()
        {
            _intCall(new object[] { });
        }

        /// <summary>
        /// Calls the current contract method on the remote service
        /// </summary>
        /// <typeparam name="K">The Type of the return value</typeparam>
        /// <returns>The return value</returns>
        public K DoCall<K>()
        {
            _intCall(new object[] { });
            if (CurrentCallWillHaveReturnValue)
            {
                while (CurrentCallHasReturnValue == false) { }
                return (K)CurrentReturnValue;
            }
            else { return default(K); }
        }


        /// <summary>
        /// Calls the current contract method on the remote service
        /// </summary>
        /// <typeparam name="K">The Type of the return value</typeparam>
        /// <param name="parameters">The parameters</param>
        /// <returns>The return value</returns>
        public K DoCall<K>(object[] parameters)
        {
            _intCall(parameters);
            if (CurrentCallWillHaveReturnValue)
            {
                while (CurrentCallHasReturnValue == false) { }
                return (K)CurrentReturnValue;
            }
            else { return default(K); }
        }

        private void _intCall(object[] parameters)
        {
            
            MethodBase mb = new StackTrace().GetFrame(2).GetMethod();

            XElement xml = new XElement("xInvokeCall",
                new XAttribute("Method", mb.Name),
                new XElement("Params",
                    from p in parameters
                    select new XElement("Param",
                        new XAttribute("Value",
                            p.Serialise()))));

            CurrentCallHasReturnValue = false;
            CurrentReturnValue = null;

            _sender.SendToChannel("xInvoke_" + _serviceName, xml.ToString(SaveOptions.DisableFormatting));

            List<Type> _ts = new List<Type>();
            foreach (object p in parameters)
            {
                _ts.Add(p.GetType());
            }

            MethodInfo mi = _contract.Type.GetMethod(mb.Name, _ts.ToArray());

            CurrentCallWillHaveReturnValue = mi.ReturnType != Type.GetType("System.Void");

        }

        /// <summary>
        /// Whether the call will return a value
        /// </summary>
        public bool CurrentCallWillHaveReturnValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the call has received the return value
        /// </summary>
        public bool CurrentCallHasReturnValue
        {
            get;
            private set;
        }

        /// <summary>
        /// The return value from the call
        /// </summary>
        public object CurrentReturnValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts the client
        /// </summary>
        public void Begin()
        {
            _listener.RegisterChannel("xInvoke_" + _serviceName + "_CLIENT");
        }

        /// <summary>
        /// Stops the client
        /// </summary>
        public void Stop()
        {
            _listener.UnRegisterChannel("xInvoke_" + _serviceName + "_CLIENT");
        }
    }
}
