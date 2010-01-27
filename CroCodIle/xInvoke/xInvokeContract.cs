using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace xInvoke
{
    /// <summary>
    /// Defines the contract used for invocation
    /// </summary>
    public sealed class xInvokeContract
    {
        private Type _contract = null;
        private string _definition = string.Empty;

        public xInvokeContract(Type Contract)
        {
            if (Contract == null)
            {
                throw new ArgumentNullException("Contract");
            }
            else
            {
                if (Contract.IsInterface == false)
                {
                    throw new ContractGivenNotInterfaceException();
                }
                else
                {
                    _contract = Contract;
                }
            }
        }

        /// <summary>
        /// The Type (The Interface) that the contract uses
        /// </summary>
        public Type Type
        {
            get { return _contract; }
        }

        /// <summary>
        /// Returns the XML definition of the contract
        /// </summary>
        /// <returns>The XML definition</returns>
        public string GetContractDefinition()
        {
            if (_definition == string.Empty)
            {
                XElement xml = new XElement("xInvokeContract",
                    new XAttribute("Name", _contract.Name),
                    new XElement("Methods",
                    from m in _contract.GetMethods()
                    select new XElement("Method",
                        new XAttribute("Name", m.Name),
                        new XAttribute("ReturnType", m.ReturnType.FullName),
                        new XElement("Params",
                            from p in m.GetParameters()
                            select new XElement("Param",
                                new XAttribute("Name", p.Name),
                                new XAttribute("Type", p.ParameterType.FullName))))),
                                new XElement("Properties",
                                    from p in _contract.GetProperties()
                                        select new XElement("Property",
                                            new XAttribute("Name",p.Name),
                                            new XAttribute("Type",p.PropertyType),
                                            new XAttribute("Get",p.GetGetMethod() != null ? true : false),
                                            new XAttribute("Set",p.GetSetMethod() != null ? true : false))));

                _definition = xml.ToString(SaveOptions.DisableFormatting);
            }

            return _definition;
        }

        /// <summary>
        /// Crude method to determine if two contracts match
        /// </summary>
        /// <param name="Contract1">The first contract</param>
        /// <param name="Contract2">The second contract</param>
        /// <returns>Whether the contracts match (comparison made on the XML)</returns>
        public static bool ContractsMatch(xInvokeContract Contract1, xInvokeContract Contract2)
        {
            if (Contract1 == null | Contract2 == null)
            {
                throw new ArgumentNullException("Contract(s) can't be null");
            }

            return Contract1.GetContractDefinition() == Contract2.GetContractDefinition();
        }
    }
}
