using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Refit;

namespace Refit.Tests
{
    public class BatchResponseTests
    {
        [Fact]
        public void AddResponse_ShouldThrow_WhenMethodDoesNotExists()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                    .AddRequest(nameof(ITestApi.UploadFileInfo2), null)
                    .AddRequest(nameof(ITestApi.Hello), "hello")
                    .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            Assert.Throws<ArgumentException>(() => rs.AddResponse(RestResult<string>.AsSuccess("test", "")));
        }

        [Fact]
        public void AddResponse_ShoudThrow_WhenResponseIsNull()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                      .AddRequest(nameof(ITestApi.UploadFileInfo2), null)
                      .AddRequest(nameof(ITestApi.Hello), "hello")
                      .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            Assert.Throws<ArgumentNullException>(() => rs.AddResponse(null));
        }


        [Fact]
        public void AddResponse_ShouldThrow_WhenResultIsNullButDoesNotHaveException()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder.AddRequest(nameof(ITestApi.UploadFileInfo2),  null)
                    .AddRequest(nameof(ITestApi.Hello), "hello")
                    .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            Assert.Throws<ArgumentNullException>(() => rs.AddResponse(RestResult<string>.AsError("Hello", null)));

        }


        [Fact]
        public void AddResponse_LabelShouldBeUnique()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder.AddRequest(nameof(ITestApi.UploadFileInfo2), null)
                    .AddRequest(nameof(ITestApi.Hello), "hello")
                    .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            Assert.Throws<ArgumentNullException>(() => rs.AddResponse(RestResult<string>.AsError("Hello", null)));

        }

        [Fact]
        public async Task GetResult_ShouldReturnFailedResult_WhenTResultDoesNotMatchTypeOfResult()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                        .AddRequest(nameof(ITestApi.UploadFileInfo2),  null)
                        .AddRequest(nameof(ITestApi.Hello), "hello")
                        .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            rs.AddResponse(RestResult<int>.AsSuccess(nameof(ITestApi.UploadFileInfo2), 10));
            var re = rs.GetResult<string>(nameof(ITestApi.Hello));
            var result = re.FirstOrDefault();
            Assert.Equal(1, re.Count());
            Assert.Equal(false, result.IsSuccessful);
            var r = await Assert.ThrowsAsync<ArgumentException>(() => { throw result.Exception; });
        }


        [Fact]
        public async Task GetResult_ShouldReturnFailedResult_WhenResultDoesNotExitsForThatMethod()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                .AddRequest(nameof(ITestApi.UploadFileInfo2),  null)
                .AddRequest(nameof(ITestApi.Hello), "hello")
                .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            var re = rs.GetResult<string>("Hello");
            var result = re.FirstOrDefault();
            Assert.Equal(false, result.IsSuccessful);
            var r = await Assert.ThrowsAsync<ArgumentException>(() => { throw result.Exception; });
        }

        [Fact]
        public void GetResult_ShouldThrow_WhenIndexIsOutOfRange()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                .AddRequest(nameof(ITestApi.UploadFileInfo2), null)
                .AddRequest(nameof(ITestApi.Hello), "hello")
                .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            rs.AddResponse(RestResult<string>.AsSuccess(nameof(ITestApi.Hello), "Dope"));
            Assert.Throws<ArgumentOutOfRangeException>(() => rs.GetResult<string>(4));
        }

        [Fact]
        public void GetResult_ShouldReturnFailedResultOfGivenIndex_IfTypeDoesNotMatch()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                .AddRequest(nameof(ITestApi.UploadFileInfo2),  null)
                .AddRequest(nameof(ITestApi.Hello), "hello")
                .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            rs.AddResponse(RestResult<string>.AsSuccess(nameof(ITestApi.Hello), "Dope"));
            var re = rs.GetResult<int>(0);
            Assert.NotNull(re);
            Assert.Equal(false, re.IsSuccessful);
            Assert.IsType<ArgumentException>(re.Exception);
        }

        [Fact]
        public void GetResult_ShouldReturnResultOfGivenIndex_IfMatch()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();
            var rq = builder
                .AddRequest(nameof(ITestApi.UploadFileInfo2),  null)
                .AddRequest(nameof(ITestApi.Hello), "hello")
                .Build() as BatchRequest;

            var rs = rq.CreateBatchResponse();
            rs.AddResponse(RestResult<string>.AsSuccess(nameof(ITestApi.Hello), "Dope"));
            Assert.Equal(1, rs.Count);
            var re = rs.GetResult<string>(0);
            Assert.NotNull(re);
            Assert.Equal(true, re.IsSuccessful);
            Assert.Equal("Dope", re.Value);
        }


    }
}
