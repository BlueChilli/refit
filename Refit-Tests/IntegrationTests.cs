using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using Xunit;

namespace Refit.Tests
{
    public class Attachment
    {

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("contentPath")]
        public string contentPath { get; set; }

        [JsonProperty("attachmentType")]
        public string attachmentType { get; set; }
    }

    public class Hazard
    {

        
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("latitude")]
        public double latitude { get; set; }

        [JsonProperty("longitude")]
        public double longitude { get; set; }

        [JsonProperty("altitude")]
        public double altitude { get; set; }

        [JsonProperty("siteId")]
        public int siteId { get; set; }

        [JsonProperty("hazardTypeId")]
        public int hazardTypeId { get; set; }

        [JsonProperty("notes")]
        public string notes { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment> attachments { get; set; }

    }

    public class IdItem
    {
        public int Id { get; set; }
    }
    public interface IHazardApi
    {
        [Multipart]
        [Headers("Accept: */*")]
        [Post("/hazards")]
        Task<Hazard> Create(MultipartData<Hazard> hazard, [AliasAs("imageFile")] FileInfo imageFile);

        //[Multipart]
        //[Post("/hazards")]
        //Task<Hazard> Create(string siteId, string name, [AliasAs("imageFile")] Stream image);


        [Get("/account/status")]
        Task<HttpResponseMessage> GetAccoutStatus();

        [Post("/hazards/ack")]
        Task<HttpResponseMessage> Update(MultipartData<IdItem> item);
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class BatchUser
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PagedList<T>
    {
        public PagedList()
        {
            Data = new List<T>();
        }

        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public List<T> Data { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public interface IBatchApi : IAsyncBatchable, IObservableBatchable
    {

        [Post("/v1/users/login")]
        Task<BatchUser> Login([Body()] Login login);

        [Get("/v1/company/current")]
        Task<Company> GetCompany();

        [Get("/v1/users/profile")]
        Task<BatchUser> GetProfile();
    }
    public class AuthHandler : DelegatingHandler
    {
        private readonly string _apikey;
        private readonly string _userkey;

        public AuthHandler(string apikey, string userkey, CookieContainer container = null) : base(new HttpClientHandler() { CookieContainer = container })
        {
            this._apikey = apikey;
            this._userkey = userkey;
        }
       
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("ApiKey", _apikey);
            request.Headers.Add("UserKey", _userkey);

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class IntegrationTests
    {
        [Fact]
        public async Task ShouldSuccessfullyCallHazardApi()
        {
            const string apiKey = "CF5252EB-4537-4460-A1F6-6D9BF0DBDBFA";
            const string userKey = "0872d545-0639-4847-9dda-bc0e31bd871a";

            var url = "https://dev.bluechilli.com/safetycompass/api";


            var settings = new RefitSettings()
            {
                JsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter(), new IsoDateTimeConverter() },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                },
                HttpMessageHandlerFactory = () => new AuthHandler(apiKey, userKey, new CookieContainer())
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            var api = RestService.For<IHazardApi>(url, settings);

            var hazard = new Hazard()
            {
                siteId = 8,
                name = "BlueChilli7",
                latitude = -33.8716715,
                longitude = 151.20616129999996,
                altitude = 24,
                hazardTypeId = 1,
                notes = "Lorem ipsum dolor sit amet, ",
                attachments = new List<Attachment>()
                {
                    new Attachment()
                    {
                        name = "Attach1",
                        attachmentType = "VideoLink",
                        contentPath = "https://www.youtube.com/2945kdf49fk"
                       
                    }
                }

            };

            var r = await api.Create(MultipartData<Hazard>.Create(hazard), new FileInfo(@"C:\Temp\messenger-hover.png"));
            Assert.NotNull(r);

        }

        [Fact()]
        public async Task MultipartBatchRequestShouldSucceed()
        {
            const string apiKey = "F5311DE2-6F54-443E-8FC6-863AE944CE4A";
            const string userKey = "c7334a86-e4ba-454b-b7fa-c0a337a0a21d";

            var url = "https://benchon-dev.azurewebsites.net/api";

            var container = new CookieContainer();
            var settings = new RefitSettings()
            {
                JsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter(), new IsoDateTimeConverter() },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                },
                HttpMessageHandlerFactory = () => new AuthHandler(apiKey, userKey, container)
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            var builder = BatchRequestBuilder.For<IBatchApi>();
            var req = builder.AddRequest(api => api.GetProfile())
                    .AddRequest(api => api.GetCompany())
                    .Build("/$batch/nonsequential");

            var client = RestService.For<IBatchApi>(url, settings);

            try
            {
                var user = await client.Login(new Login()
                {
                    Email = "max@bluechilli.com",
                    Password = "123456"
                });
                var r = await client.BatchAsync(req);
                Assert.NotNull(r);
                var c = r.GetResults<Company>(nameof(IBatchApi.GetCompany)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
            //Assert.NotNull(r.GetResult<Company>(nameof(IBatchApi.GetCompany)));

        }
    }
}
