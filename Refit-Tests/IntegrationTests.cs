﻿using System;
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

        public FileInfo ImageFile { get; set; }
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
        Task<Hazard> Create(MultipartData<Hazard> hazard);

        //[Multipart]
        //[Post("/hazards")]
        //Task<Hazard> Create(string siteId, string name, [AliasAs("imageFile")] Stream image);


        [Get("/account/status")]
        Task<HttpResponseMessage> GetAccoutStatus();

        [Post("/hazards/ack")]
        Task<HttpResponseMessage> Update(MultipartData<IdItem> item);
    }

    public class BatchUser
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public interface IBatchApi
    {

        [Post("/users/login")]
        Task<BatchUser> Login(string userId, string password);

        [Get("/company/current")]
        Task<Company> GetCompany();
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
            //const string apiKey = "CF5252EB-4537-4460-A1F6-6D9BF0DBDBFA";
            const string apiKey = "CF5252EB-4537-4460-A1F6-6D9BF0DBDBFA";
            const string userKey = "0872d545-0639-4847-9dda-bc0e31bd871a";
            var url = "https://dev.bluechilli.com/safetycompass/api";
            //const string userKey = "096035ac-535b-4d10-aa4f-ad6bceead7d4";
           // var url = "https://local.bluechilli.com/safetycompass/api";


            var settings = new RefitSettings()
            {
                JsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter(), new IsoDateTimeConverter() },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                },
                HttpMessageHandlerFactory = () => new AuthHandler(apiKey, userKey)
            };

           var api = RestService.For<IHazardApi>(url, settings);

            var hazard = new Hazard()
            {
                siteId = 4,
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
                },
                ImageFile = new FileInfo(@"C:\Temp\icon.png")

            };

            var r = await api.Create(MultipartData<Hazard>.Create(hazard));
            Assert.NotNull(r);

        }

        [Fact()]
        public async Task MultipartBatchRequestShouldSucceed()
        {
            const string apiKey = "D2FC4BB2-6E9A-4204-9075-013B7C748159";
            const string userKey = "c7334a86-e4ba-454b-b7fa-c0a337a0a21d";

            var url = "https://benchon-dev.azurewebsites.net/api/v1";

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

            var builder = BatchRequestBuilder.For<IBlastMeApi>();
            var rq = builder.AddRequest(api => api.GetNumberSentOfOffers(2))
                .AddRequest(api => api.GetBusiness(2))
                .Build("/$batch");

            var client = RestService.For<IBlastMeApi>(url, settings);

            var r = await client.BatchAsync(rq);
            Assert.NotNull(r);

        }
    }
}
