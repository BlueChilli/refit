using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Refit
{
    #region BatchRequest

    public static class BatchRequestBuilder
    {
        public static IBatchRequestBuilder<T> For<T>(RefitSettings settings = null) where T : IBatchable
        {
            if (!typeof(T).IsAssignableTo(typeof(IAsyncBatchable)) && !typeof(T).IsAssignableTo(typeof(IObservableBatchable)))
            {
                throw new ArgumentException("typedinterface must implements either IAsyncBatchable or IObservableBatchable");
            }

            return new BatchRequestBuilder<T>(settings);
        }
    }

    class BatchRequestBuilder<T> : IBatchRequestBuilder<T> where T : IBatchable
    {
        private readonly RefitSettings _settings;
        private readonly IRequestBuilder _builder;
        private readonly List<RequestInfo> _requests;
        private readonly Dictionary<string, string> _headers;
       
        internal BatchRequestBuilder(RefitSettings settings = null)
        {
            _settings = settings;
            _builder = RequestBuilder.ForType<T>(settings);
            _requests = new List<RequestInfo>();
            _headers = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// add a request to batch
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public IBatchRequestBuilder<T> AddRequest(string methodName, params object[] paramters)
        {
            return AddRequestWithLabel(null, methodName, paramters);
        }

        public IBatchRequestBuilder<T> AddRequest<TRes>(Expression<Func<T, TRes>> expression)
        {
            return AddRequestWithLabel<TRes>(null, expression);
        }

        public IBatchRequest<T> Build(string path = "/batch")
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var r = new BatchRequest<T>(path, _requests.ToList(), _settings, _headers);
            r.RestResultTaskFunc = _builder.BuildRestResultTaskFuncForBatch(r);
            r.RestResultRxFunc = _builder.BuildRestResultRxFuncForBatch(r);
            Cleanup();
            return r;
        }

        private void Cleanup()
        {
            this._requests.Clear();
            this._headers.Clear();
        }

        public IBatchRequestBuilder<T> AddRequestWithLabel<TRes>(string label, Expression<Func<T, TRes>> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            var parameterList = new List<object>();
            var methodName = body.Method.Name;

            foreach (var argument in body.Arguments)
            {
                var value = ExtractParameterValue(argument);
                parameterList.Add(value);
            }

            this.AddRequestWithLabel(label, methodName, parameterList.ToArray());

            return this;
        }

        public IBatchRequestBuilder<T> AddRequestWithLabel(string label, string methodName, params object[] paramters)
        {

            if (methodName == nameof(IAsyncBatchable.BatchAsync) || methodName == nameof(IObservableBatchable.Batch))
            {
                return this;
            }

            var requestFunc = _builder.BuildRequestFuncForMethod(methodName);

            if (!String.IsNullOrWhiteSpace(label) && _requests.Any(m => m.Label == label))
            {
                throw new ArgumentException($"{nameof(label)} must be unique within the requests");
            }

            _requests.Add(new RequestInfo()
            {
                Label = label,
                Method = methodName,
                ParameterList = paramters,
                RequestMessageFactory = requestFunc
            });

            return this;
        }

        public IBatchRequestBuilder<T> AddHeader(KeyValuePair<string, string> header)
        {

            if (!_headers.ContainsKey(header.Key))
            {
                _headers.Add(header.Key, header.Value);
            }
            else
            {
                _headers[header.Key] = header.Value;
            }

            return this;
        }

        private static object ExtractParameterValue(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                return ((ConstantExpression)expression).Value;
            }

            if (expression is MemberExpression)
            {
                return GetValue((MemberExpression)expression);
            }

            if (expression is UnaryExpression)
            {
                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                return GetValue((MemberExpression)((UnaryExpression)expression).Operand);
            }

            return  Expression.Lambda(expression).Compile().DynamicInvoke();
         
        }


        private static object GetValue(MemberExpression exp)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
#if WINDOWS_APP || NETSTANDARD1_3
                        .GetTypeInfo().GetDeclaredField(exp.Member.Name)
#else
                        .GetField(exp.Member.Name)
#endif
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }

            if (exp.Expression is MemberExpression)
            {
                return GetValue((MemberExpression)exp.Expression);
            }

            return Expression.Lambda(exp).Compile().DynamicInvoke(); ;
        }

       
    }

    class RequestInfo
    {
        public string Label { get; set; }
        public string Method { get; set; }
        public object[] ParameterList { get; set; }
        public Func<HttpClient, object[], HttpRequestMessage> RequestMessageFactory { get; set; }
    }

    internal abstract class BatchRequest : IBatchRequest
    {
        protected readonly List<RequestInfo> _requests;
        private readonly RefitSettings _refitSetting;
        private readonly Dictionary<string, string> _headers;
        public Func<HttpClient, CancellationToken, Task<IBatchResponse>> RestResultTaskFunc { get; internal set; }
        public Func<HttpClient, CancellationToken, IObservable<IBatchResponse>> RestResultRxFunc { get; internal set;}

        internal BatchRequest(string path, List<RequestInfo> requests, RefitSettings refitSetting, Dictionary<string, string> headers)
        {
            _requests = requests;
           _refitSetting = refitSetting;
            this._headers = headers;
            Path = path;

        }


        internal List<RequestInfo> Requests => _requests;
        internal RefitSettings RefitSettings => _refitSetting;
        internal IDictionary<string, string> Headers => _headers;

        internal BatchResponse CreateBatchResponse()
        {
            return new BatchResponse(this.Requests.Select(m => m.Method).Distinct());
        }

        public string Path { get; }
    }


    class BatchRequest<T> : BatchRequest, IBatchRequest<T> where T : IBatchable
    {
        internal BatchRequest(string path, List<RequestInfo> requests, RefitSettings refitSettings, Dictionary<string, string> headers) : base(path, requests, refitSettings, headers)
        {
           
        }

        
    }

    #endregion BatchReqeust

    #region BatchResponse

   
    internal class BatchResponse : IBatchResponse
    {
        private readonly IEnumerable<string> methods;
        private readonly List<RestResult> _responses;

        internal BatchResponse(IEnumerable<string> methods)
        {
            this.methods = methods;
            _responses = new List<RestResult>();
        }

    
        internal BatchResponse AddResponse(RestResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!methods.Contains(result.Method))
            {
                throw new ArgumentException($"Method {result.Method} does not exists");
            }

            if (result.Exception == null && result.Val == null)
            {
                throw new ArgumentNullException(nameof(result.Val));
            }


            _responses.Add(result);

            return this;
        }

      
        public IEnumerable<RestResult<TResult>> GetResult<TResult>(string methodName)
        {
            if (!_responses.Any(m => m.Method == methodName))
            {
                return new List<RestResult<TResult>>() { RestResult<TResult>.AsError(methodName, new ArgumentException("Method could not be found")) };
            }
         
            var responseInfo = _responses.FirstOrDefault(m => m.Method == methodName);

            if (!(responseInfo.Val is TResult))
            {
                return new List<RestResult<TResult>>() { RestResult<TResult>.AsError(methodName, new ArgumentException($"Expected type was {typeof(TResult).Name} but was {responseInfo.Val.GetType().Name}")) };
            }

            var r = _responses.Where(m => m.Method == methodName).Select(m => (RestResult<TResult>)m).ToList();

            return r;

        }

        public RestResult<TResult> GetResult<TResult>(int index)
        {
            if (index > _responses.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            var responseInfo = _responses[index];
            if (!(responseInfo.Val is TResult))
            {
                return RestResult<TResult>.AsError(responseInfo.Method, new ArgumentException($"Expected type was {typeof(TResult).Name} but was {responseInfo.Val.GetType().Name}"));
            }


            return (RestResult<TResult>)_responses[index];
        }

        public RestResult<TResult> GetResult<TResult>(string methodName, string label)
        {
            var r = GetResult<TResult>(methodName);
            return !(r.Any(m => m.Label == label)) ? RestResult<TResult>.AsError(methodName, new ArgumentException($"Could not find the result with {label}")) : r.FirstOrDefault(m => m.Label == label);
        }

        public int Count => _responses.Count;
        public RestResult GetResult(string methodName, string label = null)
        {
            if (!_responses.Any(m => m.Method == methodName))
            {
                return RestResult<Unit>.AsError(methodName, new ArgumentException("Method could not be found"));
            }

            if (!String.IsNullOrWhiteSpace(label))
            {
                return _responses.FirstOrDefault(m => m.Method == methodName &&  m.Label == label);
            }

            return _responses.FirstOrDefault(m => m.Method == methodName);

        }
    }


    #endregion
}
