using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Refit
{
    partial class RequestBuilderImplementation
    {
        private const string RequestIdHeaderKey = "RequestId";

        public Func<HttpClient, CancellationToken, Task<IBatchResponse>> BuildRestResultTaskFuncForBatch(IBatchRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!(request is BatchRequest batchRequestData))
            {
                throw new ArgumentNullException(nameof(request));
            }

            var methods = batchRequestData.Requests.Select(m => m.Method).ToList();
            var headers = batchRequestData.Headers;
            var requests = batchRequestData.Requests.ToList();
            var path = request.Path.TrimStartSlash();
            var refitSettings = batchRequestData.RefitSettings ?? this.settings;
            var responses = batchRequestData.CreateBatchResponse();
            var taskFuncs = new List<MulticastDelegate>();
            var restMethods = new Dictionary<RequestInfo, RestMethodInfo>();

            foreach (var req in requests)
            {
                var restMethod = FindMatchingRestMethodInfo(req.Method, req.ParameterList?.Select(m => m.GetType()).ToArray(), null);
                restMethods.Add(req, restMethod);

                if (restMethod.ReturnType == typeof(Task))
                {
                    var taskFunc = GetVoidResponseResolver();
                    taskFuncs.Add(taskFunc);
                }
                else
                {
                    var taskFuncMi = GetType().GetMethod("GetResponseResolver", BindingFlags.NonPublic | BindingFlags.Instance);
                    var taskFunc = (MulticastDelegate)taskFuncMi.MakeGenericMethod(restMethod.SerializedReturnType).Invoke(this, new object[] { });
                    taskFuncs.Add(taskFunc);
                }

            }

            return async (client, cancellationToken) =>
            {
                var batchMultipartContent = new MultipartContent("mixed", "----GreatBatchBoundary");
                var baseUrl = client.BaseAddress.AbsoluteUri.TrimEndSlash() + "/";
                var absolutePath = client.BaseAddress.AbsolutePath;
                var basePath = absolutePath == "/" ? client.BaseAddress.AbsoluteUri : client.BaseAddress.AbsoluteUri.Replace(absolutePath, "");

                foreach (var r in requests)
                {
                    var requestMessage = r.RequestMessageFactory(client, r.ParameterList);
                    var requestPath = requestMessage.RequestUri.OriginalString.TrimStartSlash();
                    requestMessage.RequestUri = new Uri(new Uri(basePath), requestPath);
                    requestMessage.Headers.Add(RequestIdHeaderKey, r.RequestId);
                    batchMultipartContent.Add(new HttpMessageContent(requestMessage));
                }

                var rq = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), path).AbsoluteUri) { Content = batchMultipartContent };

                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        rq.Headers.Add(header.Key, header.Value);
                    }    
                }

                var resp = await client.SendAsync(rq, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    throw await ApiException.Create(rq, HttpMethod.Post, resp, refitSettings).ConfigureAwait(false);
                }

                var contents = await resp.Content.ReadAsMultipartAsync(cancellationToken).ConfigureAwait(false);

                var items = new List<RestResult>();
                for (var j = 0; j < contents.Contents.Count; j++)
                {
                    var res = await contents.Contents[j].ReadAsHttpResponseMessageAsync(cancellationToken).ConfigureAwait(false);
                    if (res.Headers.Contains(RequestIdHeaderKey))
                    {
                        var requestId = res.Headers.GetValues(RequestIdHeaderKey).FirstOrDefault();
                        var req = requests.FirstOrDefault(m => m.RequestId == requestId);
                        var restMethod = restMethods[req];
                        var indexId = requests.IndexOf(req);
                        var taskFunc = taskFuncs[indexId];
                        var result = await ((Task<RestResult>)taskFunc.DynamicInvoke(rq, res, restMethod, req.Label)).ConfigureAwait(false);
                        result.Index = indexId;
                        items.Add(result);
                    }
                    else
                    {
                        // this should never happens
                        throw new ArgumentException($"RequestId is not in the response header. Sorry can not parse it !!!! Please check that you are sending requestId in request Header.");
                    }

                }

                var restResults = items.OrderBy(m => m.Index).ToList();

                foreach (var o in restResults)
                {
                    responses.AddResponse(o);
                }

                return responses;
            };

        }

        public Func<HttpClient, CancellationToken, IObservable<IBatchResponse>> BuildRestResultRxFuncForBatch(IBatchRequest request)
        {
           var taskFunc = BuildRestResultTaskFuncForBatch(request);

            return (client, cancellationToken) => {
                return new TaskToObservable<IBatchResponse>(ct => {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ct);
                    return taskFunc(client, cts.Token);
                });
            };
        }

        private Func<HttpRequestMessage, HttpResponseMessage, RestMethodInfo, string, Task<RestResult>> GetVoidResponseResolver()
        {
            return ResolveResponseTo;
        }

        private Func<HttpRequestMessage, HttpResponseMessage, RestMethodInfo, string, Task<RestResult>> GetResponseResolver<T>()
        {
            return ResolveResponseTo<T>;
        }

         private async Task<RestResult> ResolveResponseTo(HttpRequestMessage req, HttpResponseMessage resp, RestMethodInfo restMethod, string label = null)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var ex = await ApiException.Create(req, restMethod.HttpMethod, resp, restMethod.RefitSettings).ConfigureAwait(false);
                return RestResult<Unit>.AsError(restMethod.Name, ex, label);
            }

            return RestResult<Unit>.AsSuccess(restMethod.Name, Unit.Default, label);
        }
        private async Task<RestResult> ResolveResponseTo<T>(HttpRequestMessage req, HttpResponseMessage resp, RestMethodInfo restMethod, string label = null)
        {
            try
            {
               
                if (restMethod.SerializedReturnType == typeof(HttpResponseMessage))
                {
                    // NB: This double-casting manual-boxing hate crime is the only way to make 
                    // this work without a 'class' generic constraint. It could blow up at runtime 
                    // and would be A Bad Idea if we hadn't already vetted the return type.
                    return RestResult<T>.AsSuccess(restMethod.Name, (T)(object)resp, label);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var ex = await ApiException.Create(req, restMethod.HttpMethod, resp, restMethod.RefitSettings).ConfigureAwait(false);
                    return RestResult<T>.AsError(restMethod.Name, ex, label);
                }

                if (restMethod.SerializedReturnType == typeof(HttpContent))
                {
                    return RestResult<T>.AsSuccess(restMethod.Name, (T)(object)resp.Content, label);
                }

                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (restMethod.SerializedReturnType == typeof(string))
                {
                    return RestResult<T>.AsSuccess(restMethod.Name, (T)(object)content, label);
                }

                return RestResult<T>.AsSuccess(restMethod.Name, JsonConvert.DeserializeObject<T>(content, settings.JsonSerializerSettings), label);

            }
            catch (Exception e)
            {
                return RestResult<T>.AsError(restMethod.Name, e);
            }
           
        }

    }
}
