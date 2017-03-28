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
    public class TestApiObject
    {
        public string Name { get; set; }
        public string SomeText { get; set; }
    }
    public interface ITestApi : IAsyncBatchable
    {
        [Multipart]
        [Post("/")]
        Task UploadFileInfo2();

        [Get("/hello")]
        Task<string> Hello(string message);

        [Get("/{id}")]
        Task GetHello(string id);

        [Get("/sometest")]
        Task<TestApiObject> SomeTest(TestObject obj);

        [Post("/sometest")]
        Task SomeTestPost(TestObject obj);
    }

    public interface ITestApi2 : IBatchable
    {
        [Post("/")]
        Task<string> Hello2(string message);
    }

    public interface ITestApi3
    {
        [Post("/")]
        Task<string> ChangeTask([Body] MultipartData<TestObject> testObject);

    }

    public class BatchRequestTests
    {

        [Fact]
        public void For_ShouldThrow_TDoesNotImplementsEitherIAsyncBatchableOrIObservableBatchable()
        {
            Assert.Throws<ArgumentException>(() => BatchRequestBuilder.For<ITestApi2>());
        }


        [Fact]
        public void AddRequest_ShouldThrow_WhenMethodDoesNotExists()
        {
           var builder = BatchRequestBuilder.For<ITestApi>();
            Assert.Throws<ArgumentException>(() => builder.AddRequest("Test", paramters: null));
        }


        [Fact]
        public void AddRequest_CanAdd_Request()
        {
            var testObj = new TestObject() { Name = "hello", Id = 20 };
            var builder = BatchRequestBuilder.For<ITestApi>();
            var r = builder
                    .AddRequest(nameof(ITestApi.UploadFileInfo2), null)
                    .AddRequest(api => api.Hello("Hello"))
                    .AddRequest(api => api.Hello("hello2"))
                    .AddRequest(api => api.SomeTest(testObj))
                    .AddRequest(api => api.SomeTestPost(new TestObject() { Name = "hello", Id = 20 }))
                    .Build() as BatchRequest;

            Assert.Equal(5, r.Requests.Count);
            Assert.IsType<TestObject>(r.Requests[4].ParameterList[0]);
        }

        [Fact]
        public void AddRequestWithLabel_LabelShouldBeUnique_IfLabelIsGiven()
        {
            var builder = BatchRequestBuilder.For<ITestApi>();

            Assert.Throws<ArgumentException>(() => builder
                    .AddRequestWithLabel("Helo123", api => api.Hello("Hello"))
                    .AddRequestWithLabel("Helo123", api => api.Hello("hello2"))
                    .Build());
        }


    }
}
