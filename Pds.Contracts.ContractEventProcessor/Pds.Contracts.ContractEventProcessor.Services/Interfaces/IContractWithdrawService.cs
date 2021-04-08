using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Interfaces
{
    /// <summary>
    /// Contract withdraw Service.
    /// </summary>
    public interface IContractWithdrawService
    {
        /// <summary>
        /// It will approve the contrack by calling the contract data api.
        /// </summary>
        /// <param name="contractEvent">ContractEvent.</param>
        /// <param name="contract">Data model contract.</param>
        /// <returns>Returns True if the contract approved else false.</returns>
        public Task<bool> WithdrawAsync(ContractEvent contractEvent, Contract contract);
    }
}