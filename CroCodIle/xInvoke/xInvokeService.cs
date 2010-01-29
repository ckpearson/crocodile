using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheCodeKing.Net.Messaging;
using System.Xml.Linq;

namespace xInvoke
{
    /// <summary>
    /// Allows for remote calling using a defined contract
    /// </summary>
    /// <typeparam name="T">The contract (Interface) to use</typeparam>
    public sealed class xInvokeService<T>
    {
        private xInvokeContract _contract = null;
        private T _instance = default(T);
        private string _serviceName = string.Empty;

        private IXDListener _listen = null;
        private IXDBroadcast _sender = null;

        /// <summary>
        /// Instantiates the service (Local machine only)
        /// </summary>
        /// <param name="Instance">The instance of the contract to be invoked</param>
        /// <param name="ServiceName">The name of the service</param>
        public xInvokeService(T Instance, string ServiceName)
        {
            _create(Instance, ServiceName, false);
        }

        /// <summary>
        /// Instantiates the service (with the option to propagate to over the network - will only work if the client originally broadcast over the network)
        /// </summary>
        /// <param name="Instance">The instance of the contract to be invoked</param>
        /// <param name="ServiceName">The name of the service</param>
        /// <param name="Network">Whether to allow the service to be called over the network</param>
        public xInvokeService(T Instance, string ServiceName, bool Network)
        {
            _create(Instance, ServiceName, Network);
        }

        private void _create(T Instance, string ServiceName, bool Network)
        {
            if (Instance == null)
            {
                throw new ArgumentNullException("Instance");
            }
            else { _contract = new xInvokeContract(typeof(T)); }
            if (Instance.GetType().GetInterface(_contract.Type.FullName) == null)
            {
                throw new Exception("Instance is not of the contract");
            }
            if (String.IsNullOrEmpty(ServiceName))
            {
                throw new ArgumentOutOfRangeException("ServiceName");
            }

            
            _instance = Instance;
            _serviceName = ServiceName;

            _listen = XDListener.CreateListener(XDTransportMode.IOStream);
            _sender = XDBroadcast.CreateBroadcast(XDTransportMode.IOStream, Network);
            _listen.MessageReceived += new XDListener.XDMessageHandler(_listen_MessageReceived);
        }

        

        ~xInvokeService()
        {
            //Stop();
        }

        private void _listen_MessageReceived(object sender, XDMessageEventArgs e)
        {
            XDocument xd = XDocument.Parse(e.DataGram.Message);

            string method = xd.Root.Attribute("Method").Value;

            IEnumerable<object> p = from par in xd.Descendants("Param")
                                    select par.Attribute("Value").Value.Deserialise();

            List<Type> _ts = new List<Type>();
            foreach (object para in p)
                _ts.Add(para.GetType());

            object r = _contract.Type.GetMethod(method, _ts.ToArray()).Invoke(_instance,
                p.ToArray());

            if (_contract.Type.GetMethod(method, _ts.ToArray()).ReturnType != Type.GetType("System.Void"))
            {
                _sender.SendToChannel("xInvoke_" + _serviceName + "_CLIENT",
                    r.Serialise());
            }
        }

        /// <summary>
        /// Starts the service
        /// </summary>
        public void Begin()
        {
            _listen.RegisterChannel("xInvoke_" + _serviceName);
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        public void Stop()
        {
            _listen.UnRegisterChannel("xInvoke_" + _serviceName);
        }

    }
}
