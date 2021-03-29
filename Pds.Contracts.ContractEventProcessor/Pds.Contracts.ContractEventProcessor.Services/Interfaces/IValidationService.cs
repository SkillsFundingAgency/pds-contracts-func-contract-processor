using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Interfaces
{
    /// <summary>
    /// Contract validation service interface.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Check And find the contratct event type.
        /// </summary>
        /// <param name="contractEvent">contractEvent.</param>
        /// <returns>Returns contract event type.</returns>
        public ContractEventType GetContractEventType(ContractEvent contractEvent);
    }
}
