using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xunit;
using Refit;
using RichardSzalay.MockHttp;

namespace Refit.Tests
{
    public class TestObject
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class Item
    {
        public List<int> Ids { get; set; }
    }
    public interface IRunscopeApi
    {
        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadStream([AttachmentName("test.pdf")] Stream stream);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadBytes([AttachmentName("test.pdf")] byte[] bytes);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadString(string someString);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadFileInfo(IEnumerable<FileInfo> fileInfos, FileInfo anotherFile);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadFileInfoWithUrlEncodedBodyData( MultiPartData<Item> item, [AliasAs("fileInfo")] IEnumerable<FileInfo> fileInfo);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadFileInfoWithJsonObject(MultiPartData<TestObject> testObject, FileInfo anotherFile);

        [Post("/")]
        Task<HttpResponseMessage> SendMessage(string message);

        [Get("/")]
        Task<HttpResponseMessage> GetMessage();

    }


    public class MultipartTests
    {
        // To test: sign up for a Runscope account (it's free, despite them implying that's its only good for 30 days)
        // and then insert your bucket URI here in order to run tests and verify success via the Runscope UI
        const string runscopeUri = "https://8df7aa5t6xhz.runscope.net/";

        // [Fact(Skip = "Set runscopeUri field to your Runscope key in order to test this function.")]
        [Fact]
        public async Task MultipartUploadShouldWorkWithStream()
        {
            using (var stream = GetTestFileStream("Test Files/Test.pdf")) {
                var fixture = RestService.For<IRunscopeApi>(runscopeUri);
                var result = await fixture.UploadStream(stream);

                Assert.True(result.IsSuccessStatusCode);
            }
        }

        //[Fact(Skip = "Set runscopeUri field to your Runscope key in order to test this function.")]
        [Fact]
        public async Task MultipartUploadShouldWorkWithByteArray()
        {
            using (var stream = GetTestFileStream("Test Files/Test.pdf"))
            using (var reader = new BinaryReader(stream)) {
                var bytes = reader.ReadBytes((int)stream.Length);

                var fixture = RestService.For<IRunscopeApi>(runscopeUri);
                var result = await fixture.UploadBytes(bytes);

                Assert.True(result.IsSuccessStatusCode);
            }
        }

        // [Fact(Skip = "Set runscopeUri field to your Runscope key in order to test this function.")]
        [Fact]
        public async Task MultipartUploadShouldWorkWithFileInfo()
        {
            var fileName = Path.GetTempFileName();

            try {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName)) {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();

                    var fixture = RestService.For<IRunscopeApi>(runscopeUri);
                    var result = await fixture.UploadFileInfo(new [] { new FileInfo(fileName), new FileInfo(fileName) }, new FileInfo(fileName));

                    Assert.True(result.IsSuccessStatusCode);
                }
            } finally {
                File.Delete(fileName);
            }
        }

        //  [Fact(Skip = "Set runscopeUri field to your Runscope key in order to test this function.")]
        [Fact]
        public async Task MultipartUploadShouldWorkWithString()
        {
            const string text = "This is random text";

            var fixture = RestService.For<IRunscopeApi>(runscopeUri);
            var result = await fixture.UploadString(text);

            Assert.True(result.IsSuccessStatusCode);
        }

        [Fact()]
        public async Task MultipartUploadShouldWorkWithBodyData()
        {
            var fileName = Path.GetTempFileName();
            var settings = new RefitSettings
            {
                JsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new SnakeCasePropertyNamesContractResolver() }
            };

            try
            {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();
                    var fixture = RestService.For<IRunscopeApi>(runscopeUri, settings);

                    var result = await fixture.UploadFileInfoWithUrlEncodedBodyData(MultiPartData<Item>.Create(new Item() { Ids = new List<int>() { 1 } }),  new[] { new FileInfo(fileName), new FileInfo(fileName) });
                    Assert.True(result.IsSuccessStatusCode);
                   
                }
            }
            finally
            {
                File.Delete(fileName);
            }
           
        }

        [Fact()]
        public async Task MultipartUploadShouldWorkWithJsonData()
        {
            var fileName = Path.GetTempFileName();


            var settings = new RefitSettings()
            {
                JsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter(), new IsoDateTimeConverter() },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                }
            };


            try
            {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();
                    var testObj = new TestObject()
                    {
                        Name = "Hello"
                    };

                    var fixture = RestService.For<IRunscopeApi>(runscopeUri, settings);
                    var result = await fixture.UploadFileInfoWithJsonObject(MultiPartData<TestObject>.Create(testObj), new FileInfo(fileName));
                    Assert.True(result.IsSuccessStatusCode);
                   
                }
            }
            finally
            {
                File.Delete(fileName);
            }

        }

      
        private static Stream GetTestFileStream(string relativeFilePath)
        {
            const char namespaceSeparator = '.';

            // get calling assembly
#if !NETCOREAPP1_1
            var assembly = Assembly.GetCallingAssembly();
#else
            var assembly = Assembly.GetEntryAssembly();
#endif

            // compute resource name suffix
            var relativeName = "." + relativeFilePath
                .Replace('\\', namespaceSeparator)
                .Replace('/', namespaceSeparator)
                .Replace(' ', '_');

            // get resource stream
            var fullName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(relativeName, StringComparison.InvariantCulture));
            if (fullName == null) {
                throw new Exception(string.Format("Unable to find resource for path \"{0}\". Resource with name ending on \"{1}\" was not found in assembly.", relativeFilePath, relativeName));
            }

            var stream = assembly.GetManifestResourceStream(fullName);
            if (stream == null) {
                throw new Exception(string.Format("Unable to find resource for path \"{0}\". Resource named \"{1}\" was not found in assembly.", relativeFilePath, fullName));
            }

            return stream;
        }
    }
}
