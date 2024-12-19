using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.EABroker
{
    public enum eEARunningStates
    {
        /// <summary>
        /// EABroker is not start, or other error state
        /// </summary>
        NotAvailable,
        /// <summary>
        /// EABroker is wait for command or usage
        /// </summary>
        Waiting,
        /// <summary>
        /// EABroker is occupied by the Edit Procedure 
        /// </summary>
        Edit, 
        /// <summary>
        /// EABroker is moving/arranging window
        /// </summary>
        Arrange, 
        /// <summary>
        /// EABroker is serving EasyMemory'c command
        /// </summary>
        EasyMemory
    }
}
