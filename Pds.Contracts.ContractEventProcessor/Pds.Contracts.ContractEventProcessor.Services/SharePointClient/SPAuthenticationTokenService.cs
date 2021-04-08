using Microsoft.Extensions.Options;
using Pds.Contracts.ContractEventProcessor.Services.Configurations;
using Pds.Contracts.ContractEventProcessor.Services.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.SharePointClient
{
    /// <summary>
    /// The SharePoint authentication token serivce.
    /// </summary>
    public class SPAuthenticationTokenService : ISPAuthenticationTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly SPClientServiceConfiguration _spConfig;
        private readonly IContractEventProcessorLogger<ISharePointClientService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPAuthenticationTokenService"/> class.
        /// </summary>
        /// <param name="httpClient">The Http client.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        /// <param name="spClientServiceConfiguration">The SharePoint Client Service configuration.</param>
        public SPAuthenticationTokenService(
            HttpClient httpClient,
            IOptions<SPClientServiceConfiguration> spClientServiceConfiguration,
            IContractEventProcessorLogger<ISharePointClientService> logger)
        {
            _httpClient = httpClient;
            _spConfig = spClientServiceConfiguration.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> AcquireSPTokenAsync()
        {
            Uri site = new Uri($"{_spConfig.ApiBaseAddress}{_spConfig.RelativeSiteURL}");

            var body = "grant_type=client_credentials" +
                $"&resource={_spConfig.Resource}/{site.DnsSafeHost}@{_spConfig.TenantId}" +
                $"&client_id={_spConfig.ClientId}@{_spConfig.TenantId}" +
                $"&client_secret={_spConfig.ClientSecret}";

            using (var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                _logger.LogInformation($"[{nameof(AcquireSPTokenAsync)}] - Attempting to get token from the SharePoint account access control. Site: {site.AbsoluteUri}");

                var result = await _httpClient.PostAsync($"{_spConfig.Authority}{_spConfig.TenantId}/tokens/OAuth/2", stringContent)
                    .ContinueWith((response) =>
                    {
                        return response.Result.Content.ReadAsStringAsync().Result;
                    })
                    .ConfigureAwait(false);

                if (result == null)
                {
                    throw new SPTokenAcquisitionFailureException("Access token cannot be acquired for the SharePoint AAD Auth.");
                }

                var tokenResult = JsonSerializer.Deserialize<JsonElement>(result);
                string token = string.Empty;

                try
                {
                    token = tokenResult.GetProperty("access_token").GetString();
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogError(ex, $"[{nameof(AcquireSPTokenAsync)}] - Access token cannot be acquired for the SharePoint AAD Auth. Site: {site.AbsoluteUri}");
                    throw new SPTokenAcquisitionFailureException("Access token cannot be acquired for the SharePoint AAD Auth.");
                }

                if (string.IsNullOrEmpty(token))
                {
                    throw new SPTokenAcquisitionFailureException("Access token cannot be acquired for the SharePoint AAD Auth.");
                }

                _logger.LogInformation($"[{nameof(AcquireSPTokenAsync)}] - Successfully acquired the SharePoint token from the SharePoint account access control. Site: {site.AbsoluteUri}");
                return token;
            }
        }
    }
}