﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
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

        [JsonProperty("imageFile")]
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

        [Get("/account/status")]
        Task<HttpResponseMessage> GetAccoutStatus();

        [Post("/hazards/ack")]
        Task<HttpResponseMessage> Update(MultipartData<IdItem> item);

        [Multipart]
        [Post("/hazards")]
        Task<Hazard> Create1(Hazard hazard, FileInfo imageFile);
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class BatchUser
    {
        [JsonProperty("accountGUID")]
        public string AccountGUID { get; set; }

        [JsonProperty("isProfileComplete")]
        public bool IsProfileComplete { get; set; }

        [JsonProperty("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("ageGroup")]
        public string AgeGroup { get; set; }
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

    public class ProfileParameter
    {
        public string UserKey { get; set; }
    }
    public interface IBatchApi : IAsyncBatchable, IObservableBatchable
    {

        [Post("/v1/account/login")]
        Task<BatchUser> Login([Body()] Login login);

        [Get("/v1/account/profile")]
        Task<BatchUser> GetProfile(ProfileParameter parameter);
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
        [Fact(/*Skip = "Please test locally"*/)]
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
                name = "BlueChilli",
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
                ImageFile = new FileInfo(@"C:\Temp\messenger-hover.png")
            };

            try
            {
                var r = await api.Create(MultipartData<Hazard>.Create(hazard));
                Assert.NotNull(r);

            }
            catch (ApiException e)
            {
              
                throw new ApplicationException(e.Content);
            }

        }

        [Fact(Skip = "Please test locally")]
        public async Task ShouldSuccessfullyCallHazardApiAndUpload()
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
                name = "BlueChilli",
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

            try
            {
                var r = await api.Create1(hazard,  new FileInfo(@"C:\Temp\messenger-hover.png"));
                Assert.NotNull(r);

            }
            catch (ApiException e)
            {
              
                throw new ApplicationException(e.Content);
            }

        }

        [Fact(/*Skip = "Please test locally"*/)]
        public async Task MultipartBatchAsyncRequestShouldSucceed()
        {
            const string apiKey = "D2FC4BB2-6E9A-4204-9075-013B7C748159";
            const string userKey = "61e63b10-8054-4cea-8842-689a1b9f687e";

            var url = "https://dev.bluechilli.com/blastme/api";

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
            var req = builder
                    .AddRequest(api => api.Login(new Login()
                    {
                        Email = "max@bluechilli.com",
                        Password = "123456"
                    }))
                    .AddRequest(api => api.GetProfile(new ProfileParameter()
                    {
                            UserKey = userKey
                    }))
                    .Build("/$batch/sequential");

            var client = RestService.For<IBatchApi>(url, settings);

            var r = await client.BatchAsync(req);
            Assert.NotNull(r);
            var c = r.GetResults<BatchUser>(nameof(IBatchApi.GetProfile)).FirstOrDefault();
            Assert.NotNull(c);
            Assert.Equal("John", c.Value.FirstName);

        }

        [Fact(/*Skip = "Please test locally"*/)]
        public async Task MultipartBatchObservableRequestShouldSucceed()
        {
            const string apiKey = "D2FC4BB2-6E9A-4204-9075-013B7C748159";
            const string userKey = "61e63b10-8054-4cea-8842-689a1b9f687e";

            var url = "https://dev.bluechilli.com/blastme/api";

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
            var req = builder
                    .AddRequest(api => api.Login(new Login()
                    {
                        Email = "max@bluechilli.com",
                        Password = "123456"
                    }))
                    .AddRequest(api => api.GetProfile(new ProfileParameter()
                    {
                            UserKey = userKey
                    }))
                    .Build("/$batch/sequential");

            var client = RestService.For<IBatchApi>(url, settings);

            var r = await client.Batch(req);
            Assert.NotNull(r);
            var c = r.GetResults<BatchUser>(nameof(IBatchApi.GetProfile)).FirstOrDefault();
            Assert.NotNull(c);
            Assert.Equal("John", c.Value.FirstName);

        }
    }
}
