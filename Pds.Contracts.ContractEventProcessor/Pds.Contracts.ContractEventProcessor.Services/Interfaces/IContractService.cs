using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Interfaces
{
    /// <summary>
    /// Example service.
    /// </summary>
    public interface IContractService
    {
        /// <summary>
        /// Process event message.
        /// </summary>
        /// <param name="contractEvent">The contract event.</param>
        /// <returns>
        /// The hello string.
        /// </returns>
        Task<string> ProcessMessage(string contractEvent);
    }
}