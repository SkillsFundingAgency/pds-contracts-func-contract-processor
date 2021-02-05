using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// Manage workflow state.
    /// </summary>
    /// <seealso cref="Pds.Contracts.ContractEventProcessor.Services.Interfaces.IWorkflowStateManager" />
    public class WorkflowStateManager : IWorkflowStateManager
    {
        /// <inheritdoc/>
        public async Task SetWorkflowStateAsync(IMessageSession messageSession, SessionWorkflowState state)
        {
            byte[] bytes = null;
            if (state != null)
            {
                var json = JsonConvert.SerializeObject(state);
                bytes = Encoding.UTF8.GetBytes(json);
            }

            await messageSession.SetStateAsync(bytes);
        }

        /// <inheritdoc/>
        public async Task<SessionWorkflowState> GetWorkflowStateAsync(IMessageSession messageSession)
        {
            byte[] bytes = await messageSession.GetStateAsync();

            var state = new SessionWorkflowState();
            if (bytes != null)
            {
                var json = Encoding.UTF8.GetString(bytes);
                state = JsonConvert.DeserializeObject<SessionWorkflowState>(json);
            }

            return state;
        }

        /// <inheritdoc/>
        public async Task<SessionWorkflowState> ResetWorkflowStateAsync(IMessageSession messageSession)
        {
            await messageSession.SetStateAsync(null);
            return new SessionWorkflowState();
        }
    }
}