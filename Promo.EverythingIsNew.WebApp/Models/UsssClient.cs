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
        private readonly HttpClient _httpClient;

        public UssApiClient(DelegatingHandler delegatingHandler)
        {
            HttpMessageHandler handler = new MainUssHandler(delegatingHandler);
            _httpClient = new HttpClient(handler);
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(UssApiMethod method)
        {
            var handler = method.GetResponseHandler();
            using (var request = method.CreateRequest())
            {
                Task<HttpResponseMessage> responseTask;
                responseTask = _httpClient.SendAsync(request);
                return await handler.HandleResponse(responseTask, request.RequestUri.OriginalString);
            }
        }

        public async Task<HttpResponseMessage> ProcessStreamAsync(UssApiMethod method)
        {
            var handler = method.GetResponseHandler();
            using (var request = method.CreateRequest())
            {
                return await _httpClient.SendAsync(request);
            }
        }
    }


    public class MainUssHandler : DelegatingHandler
    {
        public MainUssHandler(DelegatingHandler delegatingHandler = null)
        {
            HttpMessageHandler inner = new HttpClientHandler()
            {
                UseCookies = false
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }

    public class UssApiMethod
    {
        public string BaseUri { get; protected set; }
        public string MethodName { get; protected set; }
        public HttpMethod HttpMethod { get; protected set; }
        public NameValueCollection QueryStringParams { get; set; }
        public HttpContent Body { get; set; }
        public string StringContent { get; set; }

        public UssApiMethod(string methodName, HttpMethod httpMethod, string baseUri = "api")
        {
            BaseUri = baseUri;
            MethodName = methodName;
            HttpMethod = httpMethod;
            Body = null;
        }

        protected virtual string GetBaseUriForMethod()
        {
            return BaseUri + "/" + "/" + MethodName;
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
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            

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
        public abstract Task<HttpResponseMessage> HandleResponse(Task<HttpResponseMessage> responseTask, string uri);
    }

    public class EmptyResponseHandler : ResponseHandler
    {

        public override bool ShouldHandleResponse(HttpResponseMessage response)
        {
            return false;
        }

        public async override Task<HttpResponseMessage> HandleResponse(Task<HttpResponseMessage> responseTask, string uri)
        {
            HttpResponseMessage response = null;
            string content = null;

            try
            {
                response = await responseTask;
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!content.Contains("\"code\":" + ((int)UssStatusCode.SUCCESS).ToString())) //Йа костыль!
                {
                    var json = JsonConvert.DeserializeObject<dynamic>(content);
                    if (json.meta.code != UssStatusCode.SUCCESS)
                    {
                        throw new UssException((UssStatusCode)json.meta.code, (string)json.meta.message);
                    }
                }

            }
            catch (Exception e)
            {
                
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




    public class CreateSsoRequestMethod : GetDataMethod<Int64>
    {
        public string UssLogin { get; set; }
        public string LinkedAccountLogin { get; set; }
        public string Nickname { get; set; }
        public bool IsInvite { get; set; }

        public CreateSsoRequestMethod(string ussLogin, string linkedAccountLogin, bool isInvite, string nickName = null)
            : base("sso/linkage/request/creation")
        {
            HttpMethod = HttpMethod.Post;
            UssLogin = ussLogin;
            LinkedAccountLogin = linkedAccountLogin;
            Nickname = nickName ?? "";
            IsInvite = isInvite;
        }

        protected override string GetStringBodyContent()
        {
            return JsonConvert.SerializeObject(new
            {
                requestType = IsInvite ? 1 : 0,
                linkedAccountLogin = LinkedAccountLogin,
                login = UssLogin,
                nickName = Nickname
            });
        }

        public override async Task<Int64> ParseResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<dynamic>(content).requestId;
        }
    }


    public class GetDataMethod<T> : UssApiMethod
    {
        public GetDataMethod(string methodName) : base(methodName, HttpMethod.Get)
        {
        }

        public virtual async Task<T> ParseResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

    }
}