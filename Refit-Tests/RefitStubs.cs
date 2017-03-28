﻿// <auto-generated />
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Refit;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nustache;
using Nustache.Core;
using Refit.Generator;
using System.Reflection;
using SomeType = CollisionA.SomeType;
using CollisionB;
using System.Reactive.Linq;
using RichardSzalay.MockHttp;

/* ******** Hey You! *********
 *
 * This is a generated file, and gets rewritten every time you build the
 * project. If you want to edit it, you need to edit the mustache template
 * in the Refit package */

#pragma warning disable
namespace RefitInternalGenerated
{
    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    sealed class PreserveAttribute : Attribute
    {

        //
        // Fields
        //
        public bool AllMembers;

        public bool Conditional;
    }
}
#pragma warning restore

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIGitHubApi : IGitHubApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIGitHubApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<User> GetUser(string userName)
        {
            var arguments = new object[] { userName };
            return (Task<User>) methodImpls["GetUser"](Client, arguments);
        }

        public virtual IObservable<User> GetUserObservable(string userName)
        {
            var arguments = new object[] { userName };
            return (IObservable<User>) methodImpls["GetUserObservable"](Client, arguments);
        }

        public virtual IObservable<User> GetUserCamelCase(string userName)
        {
            var arguments = new object[] { userName };
            return (IObservable<User>) methodImpls["GetUserCamelCase"](Client, arguments);
        }

        public virtual Task<List<User>> GetOrgMembers(string orgName)
        {
            var arguments = new object[] { orgName };
            return (Task<List<User>>) methodImpls["GetOrgMembers"](Client, arguments);
        }

        public virtual Task<UserSearchResult> FindUsers(string q)
        {
            var arguments = new object[] { q };
            return (Task<UserSearchResult>) methodImpls["FindUsers"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> GetIndex()
        {
            var arguments = new object[] {  };
            return (Task<HttpResponseMessage>) methodImpls["GetIndex"](Client, arguments);
        }

        public virtual IObservable<string> GetIndexObservable()
        {
            var arguments = new object[] {  };
            return (IObservable<string>) methodImpls["GetIndexObservable"](Client, arguments);
        }

        public virtual Task NothingToSeeHere()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["NothingToSeeHere"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIGitHubApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIHazardApi : IHazardApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIHazardApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<Hazard> Create(MultiPartData<Hazard> hazard)
        {
            var arguments = new object[] { hazard };
            return (Task<Hazard>) methodImpls["Create"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> GetAccoutStatus()
        {
            var arguments = new object[] {  };
            return (Task<HttpResponseMessage>) methodImpls["GetAccoutStatus"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> Update(MultiPartData<IdItem> item)
        {
            var arguments = new object[] { item };
            return (Task<HttpResponseMessage>) methodImpls["Update"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIHazardApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIBlastMeApi : IBlastMeApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIBlastMeApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual IObservable<ProfileDto> GetProfile(string userKey)
        {
            var arguments = new object[] { userKey };
            return (IObservable<ProfileDto>) methodImpls["GetProfile"](Client, arguments);
        }

        public virtual IObservable<BusinessDto> GetBusiness(int businessId)
        {
            var arguments = new object[] { businessId };
            return (IObservable<BusinessDto>) methodImpls["GetBusiness"](Client, arguments);
        }

        public virtual Task<int> GetNumberSentOfOffers(int status)
        {
            var arguments = new object[] { status };
            return (Task<int>) methodImpls["GetNumberSentOfOffers"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIBlastMeApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIAmARefitInterfaceButNobodyUsesMe : IAmARefitInterfaceButNobodyUsesMe
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIAmARefitInterfaceButNobodyUsesMe(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task RefitMethod()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["RefitMethod"](Client, arguments);
        }

        public virtual Task AnotherRefitMethod()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["AnotherRefitMethod"](Client, arguments);
        }

        public virtual Task NoConstantsAllowed()
        {
            throw new NotImplementedException("Either this method has no Refit HTTP method attribute or you've used something other than a string literal for the 'path' argument.");
        }

        public virtual Task SpacesShouldntBreakMe()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["SpacesShouldntBreakMe"](Client, arguments);
        }

        public virtual Task ReservedWordsForParameterNames(int @int,string @string,float @long)
        {
            var arguments = new object[] { @int,@string,@long };
            return (Task) methodImpls["ReservedWordsForParameterNames"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIAmARefitInterfaceButNobodyUsesMe 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIBoringCrudApi<T, TKey> : IBoringCrudApi<T, TKey>
        where T : class
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIBoringCrudApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<T> Create(T paylod)
        {
            var arguments = new object[] { paylod };
            return (Task<T>) methodImpls["Create"](Client, arguments);
        }

        public virtual Task<List<T>> ReadAll()
        {
            var arguments = new object[] {  };
            return (Task<List<T>>) methodImpls["ReadAll"](Client, arguments);
        }

        public virtual Task<T> ReadOne(TKey key)
        {
            var arguments = new object[] { key };
            return (Task<T>) methodImpls["ReadOne"](Client, arguments);
        }

        public virtual Task Update(TKey key,T payload)
        {
            var arguments = new object[] { key,payload };
            return (Task) methodImpls["Update"](Client, arguments);
        }

        public virtual Task Delete(TKey key)
        {
            var arguments = new object[] { key };
            return (Task) methodImpls["Delete"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIBoringCrudApi<T, TKey> 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedITestApi : ITestApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedITestApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task UploadFileInfo2()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["UploadFileInfo2"](Client, arguments);
        }

        public virtual Task<string> Hello(string message)
        {
            var arguments = new object[] { message };
            return (Task<string>) methodImpls["Hello"](Client, arguments);
        }

        public virtual Task GetHello(string id)
        {
            var arguments = new object[] { id };
            return (Task) methodImpls["GetHello"](Client, arguments);
        }

        public virtual Task<TestApiObject> SomeTest(TestObject obj)
        {
            var arguments = new object[] { obj };
            return (Task<TestApiObject>) methodImpls["SomeTest"](Client, arguments);
        }

        public virtual Task SomeTestPost(TestObject obj)
        {
            var arguments = new object[] { obj };
            return (Task) methodImpls["SomeTestPost"](Client, arguments);
        }



    }

    public partial class AutoGeneratedITestApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedITestApi2 : ITestApi2
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedITestApi2(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<string> Hello2(string message)
        {
            var arguments = new object[] { message };
            return (Task<string>) methodImpls["Hello2"](Client, arguments);
        }



    }

    public partial class AutoGeneratedITestApi2 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedITestApi3 : ITestApi3
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedITestApi3(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<string> ChangeTask(MultiPartData<TestObject> testObject)
        {
            var arguments = new object[] { testObject };
            return (Task<string>) methodImpls["ChangeTask"](Client, arguments);
        }



    }

    public partial class AutoGeneratedITestApi3 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedINamespaceCollisionApi : INamespaceCollisionApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedINamespaceCollisionApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<SomeType> SomeRequest()
        {
            var arguments = new object[] {  };
            return (Task<SomeType>) methodImpls["SomeRequest"](Client, arguments);
        }



    }

    public partial class AutoGeneratedINamespaceCollisionApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedINpmJs : INpmJs
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedINpmJs(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<RootObject> GetCongruence()
        {
            var arguments = new object[] {  };
            return (Task<RootObject>) methodImpls["GetCongruence"](Client, arguments);
        }



    }

    public partial class AutoGeneratedINpmJs 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIRequestBin : IRequestBin
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIRequestBin(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task Post()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["Post"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIRequestBin 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIAmHalfRefit : IAmHalfRefit
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIAmHalfRefit(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task Post()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["Post"](Client, arguments);
        }

        public virtual Task Get()
        {
            throw new NotImplementedException("Either this method has no Refit HTTP method attribute or you've used something other than a string literal for the 'path' argument.");
        }



    }

    public partial class AutoGeneratedIAmHalfRefit 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIHttpBinApi<TResponse, TParam, THeader> : IHttpBinApi<TResponse, TParam, THeader>
        where TResponse : class
        where THeader : struct
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIHttpBinApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<TResponse> Get(TParam param,THeader header)
        {
            var arguments = new object[] { param,header };
            return (Task<TResponse>) methodImpls["Get"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIHttpBinApi<TResponse, TParam, THeader> 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIBrokenWebApi : IBrokenWebApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIBrokenWebApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<bool> PostAValue(string derp)
        {
            var arguments = new object[] { derp };
            return (Task<bool>) methodImpls["PostAValue"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIBrokenWebApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIHttpContentApi : IHttpContentApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIHttpContentApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<HttpContent> PostFileUpload(HttpContent content)
        {
            var arguments = new object[] { content };
            return (Task<HttpContent>) methodImpls["PostFileUpload"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIHttpContentApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIUseOverloadedMethods : IUseOverloadedMethods
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIUseOverloadedMethods(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task Get()
        {
            var arguments = new object[] {  };
            return (Task) methodImpls["Get"](Client, arguments);
        }

        public virtual Task Get(int id)
        {
            var arguments = new object[] { id };
            return (Task) methodImpls["Get"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIUseOverloadedMethods 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}

namespace Refit.Tests
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIRunscopeApi : IRunscopeApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;
    
        public AutoGeneratedIRunscopeApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<HttpResponseMessage> UploadStream(Stream stream)
        {
            var arguments = new object[] { stream };
            return (Task<HttpResponseMessage>) methodImpls["UploadStream"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> UploadBytes(byte[] bytes)
        {
            var arguments = new object[] { bytes };
            return (Task<HttpResponseMessage>) methodImpls["UploadBytes"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> UploadString(string someString)
        {
            var arguments = new object[] { someString };
            return (Task<HttpResponseMessage>) methodImpls["UploadString"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> UploadFileInfo(IEnumerable<FileInfo> fileInfos,FileInfo anotherFile)
        {
            var arguments = new object[] { fileInfos,anotherFile };
            return (Task<HttpResponseMessage>) methodImpls["UploadFileInfo"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> UploadFileInfoWithUrlEncodedBodyData(MultiPartData<Item> item,IEnumerable<FileInfo> fileInfo)
        {
            var arguments = new object[] { item,fileInfo };
            return (Task<HttpResponseMessage>) methodImpls["UploadFileInfoWithUrlEncodedBodyData"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> UploadFileInfoWithJsonObject(MultiPartData<TestObject> testObject,FileInfo anotherFile)
        {
            var arguments = new object[] { testObject,anotherFile };
            return (Task<HttpResponseMessage>) methodImpls["UploadFileInfoWithJsonObject"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> SendMessage(string message)
        {
            var arguments = new object[] { message };
            return (Task<HttpResponseMessage>) methodImpls["SendMessage"](Client, arguments);
        }

        public virtual Task<HttpResponseMessage> GetMessage()
        {
            var arguments = new object[] {  };
            return (Task<HttpResponseMessage>) methodImpls["GetMessage"](Client, arguments);
        }



    }

    public partial class AutoGeneratedIRunscopeApi 
    {
        
        public Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = request.RestResultTaskFunc;
            return method(Client, cancellationToken);
        }

        public IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method =  request.RestResultRxFunc;
            return method(Client, cancellationToken);
        }
    
    }
                                                                                                                                                                    
}
