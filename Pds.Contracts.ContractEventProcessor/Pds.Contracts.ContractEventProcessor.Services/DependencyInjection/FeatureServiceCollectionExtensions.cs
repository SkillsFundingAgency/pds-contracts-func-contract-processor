using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.Data.Api.Client.Registrations;

namespace Pds.Contracts.ContractEventProcessor.Services.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class FeatureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for the current feature to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the feature's services to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> provider.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddFeatureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IContractEventSessionManager, ContractEventSessionManager>();
            services.AddSingleton<IWorkflowStateManager, WorkflowStateManager>();
            services.AddSingleton<IContractService, ContractService>();

            var policyRegistry = services.AddPolicyRegistry();
            services.AddContractsDataApiClient(configuration, policyRegistry);

            return services;
        }
    }
}