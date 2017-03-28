using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;

namespace Refit
{
    public interface IBatchable
    {
        
    }
    public interface IAsyncBatchable : IBatchable
    {
        Task<IBatchResponse> BatchAsync(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IObservableBatchable : IBatchable
    {
        IObservable<IBatchResponse> Batch(IBatchRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }


    #region BatchRequest

#if PORTABLE
     public static class BatchRequestBuilder
    {
        public static IBatchRequestBuilder<T> For<T>(RefitSettings settings = null) where T : IBatchable
        {
           throw new NotImplementedException("You've somehow included the PCL version of Refit in your app. You need to use the platform-specific version!");
        }
    }
#endif

    public class BatchParameterInfo
    {
        public Type ResponseType { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

  
    public interface IBatchRequestBuilder<T> where T : IBatchable
    {
        IBatchRequestBuilder<T> AddHeader(KeyValuePair<string , string> header);
        IBatchRequestBuilder<T> AddRequest(string methodName, params object[] paramters);
        IBatchRequestBuilder<T> AddRequest<TRes>(Expression<Func<T, TRes>> expression);
        IBatchRequestBuilder<T> AddRequestWithLabel<TRes>(string label , Expression<Func<T, TRes>> expression);
        IBatchRequestBuilder<T> AddRequestWithLabel(string label, string methodName,  params object[] paramters);
        IBatchRequest<T> Build(string path = "/batch");
    }

    public interface IBatchRequest
    {
        string Path { get; }
        Func<HttpClient, CancellationToken, Task<IBatchResponse>> RestResultTaskFunc { get;  }
        Func<HttpClient, CancellationToken, IObservable<IBatchResponse>> RestResultRxFunc { get; }
    }

    public interface IBatchRequest<T> : IBatchRequest where T : IBatchable
    {
        
    }

    #endregion BatchReqeust

    #region BatchResponse

    public interface IBatchResponse
    {
        IEnumerable<RestResult<TResult>> GetResults<TResult>(string methodName);
        RestResult<TResult> GetResult<TResult>(int index);
        RestResult<TResult> GetResult<TResult>(string methodName, string label);
        RestResult GetResult(string methodName, string label = null);

        int Count { get; }
    }

    struct Unit
    {
        public bool HasValue => false;
        public static Unit Default => new Unit();
    }

    public class RestResult
    {
        public int Index { get; set; }
        public string Label { get; internal set; }
        public string Method { get; internal set; }
        public bool IsSuccessful { get; internal set; }
        public Exception Exception { get; internal set; }

        internal object Val { get; set; }
    }

    public class RestResult<T> : RestResult
    {
        public T Value => (T) Val;


        public void MatchException<TException>(Action<TException> onMatch, Action<Exception> onNotMatch) where TException : Exception
        {
            var ex = Exception as TException;

            if (ex != null)
            {
                onMatch(ex);
            }
            else
            {
                onNotMatch(this.Exception);
            }
        }

        public static RestResult<T> AsSuccess(string method, T resp, string label = null)
        {
            return new RestResult<T>()
            {
                Label = label,
                Method = method,
                Val = resp,
                IsSuccessful = true
            };
        }

        public static RestResult<T> AsError(string method, Exception ex, string label = null)
        {
            return new RestResult<T>()
            {
                Label = label,
                Method = method,
                Exception = ex,
                IsSuccessful = false
            };
        }
    }
    #endregion

}
