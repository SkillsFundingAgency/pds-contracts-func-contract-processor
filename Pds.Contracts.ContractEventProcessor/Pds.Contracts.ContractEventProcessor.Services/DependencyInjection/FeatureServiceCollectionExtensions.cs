using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.ContractEventProcessor.Services.Configurations;
using Pds.Contracts.ContractEventProcessor.Services.DocumentServices;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.SharePointClient;
using Pds.Contracts.Data.Api.Client.Registrations;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.ApiClient.Services;

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
            services.AddScoped<IContractEventSessionManager, ContractEventSessionManager>();
            services.AddSingleton<IWorkflowStateManager, WorkflowStateManager>();
            services.AddScoped<IContractService, ContractService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<IContractApprovalService, ContractApprovalService>();

            var policyRegistry = services.AddPolicyRegistry();
            services.AddContractsDataApiClient(configuration, policyRegistry);
            services.Configure<SPClientServiceConfiguration>(options => configuration.GetSection(nameof(SPClientServiceConfiguration)).Bind(options));
            services.AddSharePointClientContext();
            services.AddScoped<IContractCreationService, ContractCreationService>();
            services.AddScoped<ISharePointClientService, SharePointClientService>();
            services.AddSingleton<IContractProcessorService, ContractProcessorService>();
            services.AddAsposeLicense();
            services.AddSingleton<IDocumentManagementService, AsposeDocumentManagementService>();

            return services;
        }
    }
}