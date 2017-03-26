using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Refit.Tests
{
   
    public class RequestBuilderForBatchTests
    {
        private const string baseURL = "http://www.api.com/";
        private const string batchURL = "http://www.api.com/batch";

        [Fact]
        public async Task ShouldBuildMultipartMixedContent_WhenBatchRequested()
        {
            var batchRequestBuilder = BatchRequestBuilder.For<ITestApi>();
            var request = batchRequestBuilder
                    .AddRequest(proxy => proxy.Hello("Hello"))
                    .AddRequest(nameof(ITestApi.Hello), "Dope")
                    .Build() as BatchRequest;

            var factory = request.RestResultTaskFunc;
            var testHttpMessageHandler = new TestHttpMessageHandler
            {
                ContentFactory = () =>
                {
                    var mulitpartContent = new MultipartContent("mixed", $"batch_{Guid.NewGuid().ToString()}");
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Dope") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Hello") }));
                    return mulitpartContent;
                }
            };
            var client = new HttpClient(testHttpMessageHandler) { BaseAddress = new Uri(baseURL) };
            var r = RestService.For<ITestApi>(client);
            var response = await (Task<IBatchResponse>)factory(client, CancellationToken.None);
            Assert.Equal(HttpMethod.Post, testHttpMessageHandler.RequestMessage.Method);
            Assert.Equal(batchURL, testHttpMessageHandler.RequestMessage.RequestUri.AbsoluteUri);
            Assert.IsType<MultipartContent>(testHttpMessageHandler.RequestMessage.Content);
            Assert.Equal("multipart/mixed", (testHttpMessageHandler.RequestMessage.Content as MultipartContent).Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task ShouldUsePathInTheBatchRequest_WhenBuildingRequest()
        {
            var batchRequestBuilder = BatchRequestBuilder.For<ITestApi>();
            var request = batchRequestBuilder
                    .AddRequest(proxy => proxy.Hello("Hello"))
                    .AddRequest(nameof(ITestApi.Hello), "Dope")
                    .Build() as BatchRequest;


            var factory = request.RestResultTaskFunc;

            var testHttpMessageHandler = new TestHttpMessageHandler
            {
                ContentFactory = () =>
                {
                    var mulitpartContent = new MultipartContent("mixed", $"batch_{Guid.NewGuid().ToString()}");
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Dope") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Hello") }));
                    return mulitpartContent;
                }
            };

            var client = new HttpClient(testHttpMessageHandler) { BaseAddress = new Uri(baseURL) };
            var response = await (Task<IBatchResponse>)factory(client, CancellationToken.None);
            Assert.Equal(batchURL, testHttpMessageHandler.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ShouldContainsResponses_WhenRequestReturns200()
        {
            var batchRequestBuilder = BatchRequestBuilder.For<ITestApi>();
            var request = batchRequestBuilder
                    .AddRequest(proxy => proxy.Hello("Hello"))
                    .AddRequest(nameof(ITestApi.Hello), "Dope")
                    .AddRequest(api => api.GetHello("id"))
                    .Build() as BatchRequest;


            var factory = request.RestResultTaskFunc;

            var testHttpMessageHandler = new TestHttpMessageHandler
            {
                ContentFactory = () =>
                {
                    var mulitpartContent = new MultipartContent("mixed", $"batch_{Guid.NewGuid().ToString()}");
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Dope") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Hello") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK)));
                    return mulitpartContent;
                }
            };
            var client = new HttpClient(testHttpMessageHandler) { BaseAddress = new Uri(baseURL) };
            var response = await (Task<IBatchResponse>)factory(client, CancellationToken.None);
            Assert.Equal(3, response.Count);
        }

        [Fact]
        public async Task ShouldContainsRequestMessagesInTheMultiPartContent()
        {
            var batchRequestBuilder = BatchRequestBuilder.For<ITestApi>();
            var request = batchRequestBuilder
                    .AddRequest(proxy => proxy.Hello("Hello"))
                    .AddRequest(nameof(ITestApi.Hello), "Dope")
                    .AddRequest(api => api.GetHello("id"))
                    .AddRequestWithLabel("GetHelloFailure", api => api.GetHello("2"))
                    .Build() as BatchRequest;


            var factory = request.RestResultTaskFunc;

            var testHttpMessageHandler = new TestHttpMessageHandler
            {
                ContentFactory = () =>
                {
                    var mulitpartContent = new MultipartContent("mixed", $"batch_{Guid.NewGuid().ToString()}");
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Dope") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Hello") }));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.OK)));
                    mulitpartContent.Add(new HttpMessageContent(new HttpResponseMessage(HttpStatusCode.BadRequest)));
                    return mulitpartContent;
                }
            };
            var client = new HttpClient(testHttpMessageHandler) { BaseAddress = new Uri(baseURL) };
            var response = await (Task<IBatchResponse>)factory(client, CancellationToken.None);
            Assert.Equal(4, response.Count);
            var r = response.GetResult<string>(0);
            Assert.Equal("Dope", r.Value);
            Assert.Equal(true, r.IsSuccessful);
            var r1 = response.GetResult(nameof(ITestApi.GetHello));
            Assert.Equal(true, r.IsSuccessful);
            var r2 = response.GetResult(nameof(ITestApi.GetHello), "GetHelloFailure");
            Assert.NotNull(r2);
            Assert.Equal(false, r2.IsSuccessful);

        }

        [Fact]
        public void ShouldAddFullPathFromRelativeUrl()
        {
            var uri = new Uri(baseURL);
            var uri1 ="/batch".TrimStart('/');

            Assert.Equal(batchURL, new Uri(uri, uri1).AbsoluteUri);
        }
    }
}
