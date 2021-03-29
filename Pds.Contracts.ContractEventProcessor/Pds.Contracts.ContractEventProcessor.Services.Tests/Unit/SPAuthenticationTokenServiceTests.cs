using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Services.Configurations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.SharePointClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class SPAuthenticationTokenServiceTests
    {
        private readonly ILogger<ISharePointClientService> _mockLogger
          = Mock.Of<ILogger<ISharePointClientService>>(MockBehavior.Strict);

        private readonly IOptions<SPClientServiceConfiguration> _mockSPClientServiceConfiguration
            = Mock.Of<IOptions<SPClientServiceConfiguration>>(MockBehavior.Strict);

        private readonly HttpClient _mockHttpClient
            = Mock.Of<HttpClient>(MockBehavior.Strict);


        [TestMethod]
        public async Task AcquireSPTokenAsync_ExpectedResult()
        {
            //Arrange
            SetMockSetup_AcquireSPTokenAsync();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""id"": 101, ""access_token"" : ""asdsdfgsdfg"" }"),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);

            var spAuthenticationTokenService = new SPAuthenticationTokenService(httpClient, _mockSPClientServiceConfiguration, _mockLogger);

            //Act
            var retrievedPosts = await spAuthenticationTokenService.AcquireSPTokenAsync();

            //Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
               ItExpr.IsAny<CancellationToken>());
            Mock.Get(_mockSPClientServiceConfiguration).VerifyAll();
            Mock.Get(_mockLogger).VerifyAll();
        }

        [TestMethod]
        public void AcquireSPTokenAsync_ExpectedSPTokenAcquisitionFailureException()
        {
            //Arrange
            SetMockSetup_AcquireSPTokenAsync();
            SetMockSetup_Logger(LogLevel.Error);

            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""id"": 101, ""sdfg"" : ""asdsdfgsdfg"" }"),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);

            var spAuthenticationTokenService = new SPAuthenticationTokenService(httpClient, _mockSPClientServiceConfiguration, _mockLogger);

            //Act
            Func<Task> act = async () => await spAuthenticationTokenService.AcquireSPTokenAsync();

            //Assert
            act.Should().Throw<SPTokenAcquisitionFailureException>();
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
               ItExpr.IsAny<CancellationToken>());
            Mock.Get(_mockSPClientServiceConfiguration).VerifyAll();
            Mock.Get(_mockLogger).VerifyAll();
        }


        private void SetMockSetup_AcquireSPTokenAsync()
        {
            // new StringContent(It.IsAny<string>(), Encoding.UTF8, "application/x-www-form-urlencoded")
            var spClientServiceConfiguration = GetSPClientServiceConfiguration();
            Mock.Get(_mockSPClientServiceConfiguration)
               .Setup(x => x.Value)
               .Returns(spClientServiceConfiguration)
               .Verifiable();

            SetMockSetup_Logger(LogLevel.Information);
        }

        private void SetMockSetup_Logger(LogLevel logLevel)
        {
            Mock.Get(_mockLogger)
            .Setup(logger => logger.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }


        private SPClientServiceConfiguration GetSPClientServiceConfiguration()
        {
            return new SPClientServiceConfiguration()
            {
                ApiBaseAddress = "https://testgovuk.sharepoint.com",
                AppUri = "00000034563456345600000000000/testgovuk.sharepoint.com",
                Authority = "https://accounts.accesscontrol.windows.net/",
                ClientId = "fasdfasdfasdfasdfasdf",
                ClientSecret = "asdfasdfasdfasdfasdf",
                PublicationFolderSuffix = "Outputfolder",
                RelativeSiteURL = "/sites/pdstest",
                Resource = "0000003456345600000000000",
                TenantId = "3423wertwet234542345"
            };
        }
    }
}
