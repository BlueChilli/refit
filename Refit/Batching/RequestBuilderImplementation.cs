using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Refit
{
    partial class RequestBuilderImplementation
    {
        public Func<HttpClient, CancellationToken, Task<IBatchResponse>> BuildRestResultTaskFuncForBatch(IBatchRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var batchRequestData = request as BatchRequest;

            if (batchRequestData == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var methods = batchRequestData.Requests.Select(m => m.Method).ToList();
            var headers = batchRequestData.Headers;
            var restMethods = this.interfaceHttpMethods.Where(m => methods.Contains(m.Key)).ToDictionary(a => a.Key, a => a.Value);
            var requests = batchRequestData.Requests.ToList();
            var path = request.Path.TrimStartSlash();
            var refitSettings = batchRequestData.RefitSettings ?? this.settings;
            var responses = batchRequestData.CreateBatchResponse();
            var taskFuncs = new List<MulticastDelegate>();

            foreach (var req in requests)
            {
                var restMethod = restMethods[req.Method];

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
                var batchMultipartContent = new MultipartContent("mixed", $"----batch_{Guid.NewGuid().ToString()}");
                var baseUrl = client.BaseAddress.TrimEndSlash() + "/";

                foreach (var r in requests)
                {
                    var requestMessage = r.RequestMessageFactory(client, r.ParameterList);
                    var requestPath = requestMessage.RequestUri.OriginalString.TrimStartSlash();
                    requestMessage.RequestUri = new Uri(new Uri(baseUrl), requestPath);
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
                    throw await ApiException.Create(rq.RequestUri, HttpMethod.Post, resp, refitSettings).ConfigureAwait(false);
                }

                var contents = await resp.Content.ReadAsMultipartAsync(cancellationToken).ConfigureAwait(false);

                var i = 0;
                foreach (var req in requests)
                {
                    var restMethod = restMethods[req.Method];
                    var res = await contents.Contents[i].ReadAsHttpResponseMessageAsync(cancellationToken).ConfigureAwait(false);
                    var taskFunc = taskFuncs[i];
                    var result = await ((Task<RestResult>)taskFunc.DynamicInvoke(rq.RequestUri, res, restMethod, req.Label)).ConfigureAwait(false);
                    responses.AddResponse(result);
                    i++;
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

        private Func<Uri, HttpResponseMessage, RestMethodInfo, string, Task<RestResult>> GetVoidResponseResolver()
        {
            return ResolveResponseTo;
        }

        private Func<Uri, HttpResponseMessage, RestMethodInfo, string, Task<RestResult>> GetResponseResolver<T>()
        {
            return ResolveResponseTo<T>;
        }
    }
}
