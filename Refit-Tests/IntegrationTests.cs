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
        Task<Hazard> Create(MultiPartData<Hazard> hazard, [AliasAs("imageFile")] Stream image);

        //[Multipart]
        //[Post("/hazards")]
        //Task<Hazard> Create(string siteId, string name, [AliasAs("imageFile")] Stream image);


        [Get("/account/status")]
        Task<HttpResponseMessage> GetAccoutStatus();

        [Post("/hazards/ack")]
        Task<HttpResponseMessage> Update(MultiPartData<IdItem> item);
    }

    public class AuthHandler : DelegatingHandler
    {
        private readonly string _apikey;
        private readonly string _userkey;

        public AuthHandler(string apikey, string userkey) : base(new HttpClientHandler())
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
                HttpMessageHandlerFactory = () => new AuthHandler(apiKey, userKey)
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

            try
            {
               // var r = await api.GetAccoutStatus();
               var r = await api.Create(MultiPartData<Hazard>.Create(hazard),null);
             //  var r = await api.Create("8", "hello", null);

                //var r = await api.Update(MultiPartData<IdItem>.Create(new IdItem()
                //{
                //    Id = 555
                //}));

                //var t = await r.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
