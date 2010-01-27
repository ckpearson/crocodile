using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xInvoke
{
    public sealed class ContractGivenNotInterfaceException : Exception
    {
        public ContractGivenNotInterfaceException() : base("The contract supplied is not an interface") { }
    }
}
