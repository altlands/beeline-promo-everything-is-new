using AltLanDS.Common.Events;
using AutoMapper.Internal;
using EventSourceProxy;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{


    public class UssApiClient
    {
        public UssClientConfig Config { get; set; }
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpLongClient;


        public UssApiClient()
            : this(null, null)
        {
        }

        public UssApiClient(DelegatingHandler delegatingHandler, UssClientConfig config = null)
        {
            Config = config ?? UssClientConfig.Current;
            HttpMessageHandler handler = new MainUssHandler(Config, delegatingHandler);

            _httpClient = new HttpClient(handler) { BaseAddress = Config.EndpointAddress, Timeout = Config.HttpClientTimeout };
            _httpLongClient = new HttpClient(handler) { BaseAddress = Config.EndpointAddress, Timeout = Config.HttpClientLongTimeout };
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(UssApiMethod method)
        {
            var handler = method.GetResponseHandler();
            var operationId = Guid.NewGuid();

            Stopwatch time = Stopwatch.StartNew();

            using (var request = method.CreateRequest())
            {
                if (method.IsPasswordSendingMethod())
                {
                    UssEvents.Log.CallingUss(request.RequestUri.OriginalString, operationId);
                }
                else
                    UssEvents.Log.CallingUss(request.RequestUri.OriginalString, operationId, method.StringContent);
                string cookieLog = null;
                IEnumerable<string> cookies;
                if (request.Headers.TryGetValues("Cookie", out cookies))
                    cookieLog = string.Join(", ", cookies);

                UssEvents.Log.CallingUss(request.RequestUri.OriginalString, operationId, method.StringContent, cookieLog);

                Task<HttpResponseMessage> responseTask;

                if (method.IsLongRunning)
                {
                    responseTask = _httpLongClient.SendAsync(request);
                }
                else
                    responseTask = _httpClient.SendAsync(request);

                return await handler.HandleResponse(responseTask, request.RequestUri.OriginalString, operationId, time);
            }
        }

        public async Task<HttpResponseMessage> ProcessStreamAsync(UssApiMethod method)
        {
            var handler = method.GetResponseHandler();
            using (var request = method.CreateRequest())
            {
                var operationId = Guid.NewGuid();

                if (method.IsPasswordSendingMethod())
                {
                    UssEvents.Log.CallingUss(request.RequestUri.OriginalString, operationId);
                }
                else
                    UssEvents.Log.CallingUss(request.RequestUri.OriginalString, operationId, method.StringContent);
                return await _httpClient.SendAsync(request);
            }
        }
    }




    public class UssClientConfig
    {
        private static readonly Lazy<UssClientConfig> _current = new Lazy<UssClientConfig>(
            () =>
            {
                var configuration = (IDictionary)ConfigurationManager.GetSection("uss");
                var current = new UssClientConfig()
                {
                    EndpointAddress = new Uri(configuration["endpointAddress"].ToString()),
                    HttpClientTimeout = TimeSpan.Parse(configuration["invokeTimeout"].ToString()),
                    HttpClientLongTimeout = configuration["invokeLongTimeout"] == null ? TimeSpan.Parse(configuration["invokeTimeout"].ToString()) : TimeSpan.Parse(configuration["invokeLongTimeout"].ToString()),
                    Token = configuration["token"].ToString(),
                    Signature = configuration["signature"].ToString(),
                    ClientType = configuration["clientType"] == null ? null : configuration["clientType"].ToString()
                };
                return current;
            });

        public static UssClientConfig Current
        {
            get
            {
                return _current.Value;
            }
        }

        public Uri EndpointAddress { get; set; }
        public TimeSpan HttpClientTimeout { get; set; }
        public TimeSpan HttpClientLongTimeout { get; set; }
        public String Token { get; set; }
        public String Signature { get; set; }
        public String ClientType { get; set; }
    }

    public class MainUssHandler : DelegatingHandler
    {
        private readonly UssClientConfig _config;
        private readonly string _token;
        private readonly string _tokenParamName;
        private readonly bool _useHeaderInsteadOfCookie;

        public MainUssHandler(UssClientConfig config, DelegatingHandler delegatingHandler = null)
        {
            _config = config;

            HttpMessageHandler inner = new HttpClientHandler()
            {
                UseCookies = false
            };

            if (delegatingHandler != null)
            {
                if (delegatingHandler.InnerHandler != null) throw new ArgumentException("delegatingHandler has non-null InnerHandler", "delegatingHandler.InnerHandler");
                delegatingHandler.InnerHandler = inner;
                inner = delegatingHandler;
            }

            inner = new UssAuthHandler(_config.Token)
            {
                InnerHandler = inner
            };

            this.InnerHandler = inner;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {

            if (_config.ClientType != null && !request.Headers.Any(h => h.Key.Equals("Client-Type")))
            {
                request.Headers.Add("Client-Type", _config.ClientType);
            }

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }

    public class UssApiMethod
    {
        private readonly IUssHashFunction _hashFunction;
        public string Version { get; protected set; }
        public string BaseUri { get; protected set; }
        public string MethodName { get; protected set; }
        public HttpMethod HttpMethod { get; protected set; }
        public NameValueCollection QueryStringParams { get; set; }
        public HttpContent Body { get; set; }
        public string StringContent { get; set; }
        public bool IsLongRunning { get; set; }


        public UssApiMethod(string methodName, HttpMethod httpMethod, IUssHashFunction hashFunction = null, string version = "v2.0", string baseUri = "api")
        {
            //_hashFunction = hashFunction ?? UssHashFunction.StandartHashFunction;
            BaseUri = baseUri;
            Version = version;
            MethodName = methodName;
            HttpMethod = httpMethod;

            IsLongRunning = false;
            //QueryStringParams = new QueryStringParser(string.Empty);
            Body = null;
        }

        protected virtual string GetBaseUriForMethod()
        {
            return BaseUri + "/" + Version + "/" + MethodName;
        }

        protected virtual string ComputeHash(params string[] args)
        {
            return _hashFunction.ComputeHash(String.Concat(args));
        }

        protected virtual NameValueCollection GetQueryParams()
        {
            return null;
        }
        protected virtual string GetStringBodyContent()
        {
            return null;
        }

        protected virtual HttpContent GetBodyContent()
        {
            return Body;
        }

        protected virtual string GetHashForRequest()
        {
            return null;
        }

        private string GetHashForRequestInternal(NameValueCollection queryStringParams)
        {
            var paramsToHash = new NameValueCollection(queryStringParams);
            paramsToHash.Remove("client");
            paramsToHash.Remove("hash");
            var stringToHash = string.Join("", paramsToHash.AllKeys.Select(key => ProceedStringForHash(paramsToHash[key])));
            return _hashFunction.ComputeHash(stringToHash);
        }

        private string ProceedStringForHash(string str)
        {

            return str;
        }

        public virtual ResponseHandler GetResponseHandler()
        {
            return new EmptyResponseHandler();
        }
        private string _queryString;

        public virtual string GetQueryString()
        {
            if (_queryString == null)
            {
                var requsetUri = GetBaseUriForMethod();
                var queryString = GetQueryParams();
                if (QueryStringParams.HasKeys()) queryString.Add(QueryStringParams);

                var hash = GetHashForRequest() ?? GetHashForRequestInternal(queryString);
                if (!String.IsNullOrEmpty(hash)) queryString["hash"] = hash;

                if (queryString.HasKeys()) requsetUri = requsetUri + "?" + queryString;
                _queryString = requsetUri;
            }

            return _queryString;
        }

        public virtual HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(HttpMethod, GetQueryString());

            request.Content = GetBodyContent();
            if (request.Content == null)
            {
                StringContent = GetStringBodyContent();
                if (StringContent != null)
                    request.Content = new StringContent(StringContent, Encoding.UTF8, "application/json");
                else
                    request.Content = Body;
            }

            return request;
        }


        static UssApiMethod()
        {
            var settings = new JsonSerializerSettings();


            settings.DateParseHandling = DateParseHandling.DateTimeOffset;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            JsonConvert.DefaultSettings = (() => settings);
        }
    }



    public static class Extensions
    {
        public static bool IsPasswordSendingMethod(this object method)
        {
            return false;
        }
    }

    public static class UssEvents
    {
        private static readonly Lazy<IUssEvents> _log = new Lazy<IUssEvents>(
           () =>
           {
               TraceParameterProvider.Default.For<IUssEvents>().AddActivityIdContext();
               return EventSourceImplementer.GetEventSourceAs<IUssEvents>();
           });

        public static IUssEvents Log
        {
            get
            {
                return _log.Value;
            }
        }

        public static EventSource LogEventSource
        {
            get
            {
                return _log.Value as EventSource;
            }
        }
    }

    public class UssAuthHandler : DelegatingHandler
    {
        private readonly string _token;
        private readonly string _tokenParamName;
        private readonly bool _useHeaderInsteadOfCookie;

        public UssAuthHandler(string token, string tokenParamName = "token", bool useHeaderInsteadOfCookie = false)
        {
            _token = token;
            _tokenParamName = tokenParamName;
            _useHeaderInsteadOfCookie = useHeaderInsteadOfCookie;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_useHeaderInsteadOfCookie)
            {
                if (!request.Headers.Contains(_tokenParamName)) request.Headers.Add(_tokenParamName, _token);
            }
            else
            {
                if (request.Headers.Contains("Cookie"))
                {
                    var existingCookies = request.Headers.First(h => h.Key == "Cookie").Value.ToList();

                    if (!existingCookies.Any(c => c.StartsWith(_tokenParamName + "=", StringComparison.CurrentCultureIgnoreCase)))
                        existingCookies.Add(String.Format("{0}={1}", _tokenParamName, _token));

                    existingCookies.RemoveAll(c => c == (_tokenParamName + "="));  //remove empty token

                    request.Headers.Remove("Cookie");
                    request.Headers.Add("Cookie", string.Join(";", existingCookies));
                }
                else
                    request.Headers.Add("Cookie", String.Format("{0}={1}", _tokenParamName, _token));
            }

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }

    public interface IUssHashFunction
    {
        string ComputeHash(params string[] args);
    }

    public abstract class ResponseHandler
    {
        public abstract bool ShouldHandleResponse(HttpResponseMessage response);
        public abstract Task<HttpResponseMessage> HandleResponse(Task<HttpResponseMessage> responseTask, string uri, Guid operationId, Stopwatch time);
    }

    public class EmptyResponseHandler : ResponseHandler
    {

        public override bool ShouldHandleResponse(HttpResponseMessage response)
        {
            return false;
        }

        public async override Task<HttpResponseMessage> HandleResponse(Task<HttpResponseMessage> responseTask, string uri, Guid operationId, Stopwatch time)
        {
            HttpResponseMessage response = null;
            string content = null;

            try
            {
                response = await responseTask;
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                time.Stop();
                if (!content.Contains("\"code\":" + ((int)UssStatusCode.SUCCESS).ToString())) //Йа костыль!
                {
                    var json = JsonConvert.DeserializeObject<dynamic>(content);
                    if (json.meta.code != UssStatusCode.SUCCESS)
                    {
                        throw new UssException((UssStatusCode)json.meta.code, (string)json.meta.message);
                    }
                }

                UssEvents.Log.UssResponse(uri, operationId, time.Elapsed, content, (int)response.StatusCode);
            }
            catch (Exception e)
            {
                time.Stop();
                
                throw;
            }
            return response;
        }
    }

    public interface IUssEvents
    {
        [Event(1, Level = EventLevel.Informational, Message = "Starting", Task = UssTasks.UsssApi)]
        void Starting();

        [Event(500, Level = EventLevel.Informational, Message = "Calling USSAPI method {0}", Task = UssTasks.UsssApi)]
        void CallingUss(string method, Guid operationId, string body = null, string cookies = null);
        [Event(501, Level = EventLevel.Informational, Message = "Response USSAPI method {0}", Task = UssTasks.UsssApi)]
        void UssResponse(string uri, Guid operationId, TimeSpan processingTimespan, string content, int statusCode);
        [Event(502, Level = EventLevel.Error, Message = "USSAPI logical exception in method {0}", Task = UssTasks.UsssApi)]
        void UssLogicalException(string method, Guid operationId, TimeSpan processingTimespan, string content, Exception e);
        [Event(503, Level = EventLevel.Error, Message = "USSAPI fatal exception in method {0}", Task = UssTasks.UsssApi)]
        void UssFatalException(string method, Guid operationId, TimeSpan processingTimespan, string content, Exception e);
    }

    public static class UssTasks
    {
        public const EventTask UsssApi = (EventTask)21;
    }

    public class UssException : Exception
    {
        public UssException(UssStatusCode code, string message, HttpResponseMessage ussResponse = null) : base(message)
        {
            Code = code;
            UssResponse = ussResponse;
        }
        public UssStatusCode Code { get; set; }
        public HttpResponseMessage UssResponse { get; set; }
    }

    public enum UssStatusCode : int
    {
        SUCCESS = 20000,
        INVALID_QUERY_PARAM = 20001,
        AUTH_ERROR = 20002,
        TOKEN_NOT_FOUND = 20003,
        TOKEN_EXPIRED = 20004,
        CTN_NOT_FOUND = 20005,
        FORBIDDEN = 20006,
    };
}