using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using WebApplication2;

namespace controller
{
    public class ApiClient
    {
        private readonly string _resourceName;
        private readonly string _baseUrl;
        private readonly RestSharpJsonNetSerializer _remoteServicesSerializer;

        public ApiClient(string baseUrl, string resourceName,
            RestSharpJsonNetSerializer remoteServicesSerializer)
        {
            _baseUrl = baseUrl;
            _resourceName = resourceName;
            _remoteServicesSerializer = remoteServicesSerializer;
        }

        public string BaseUrl
        {
            get { return _baseUrl; }
        }


        public async Task<T> Exec<T>(string req)
        {
            var request = CreateRestRequest(req);
            return await ExecuteAsync<T>(request);
        }

        protected async Task<TResponse> ExecuteAsync<TResponse>(IRestRequest request)
        {
            var client = CreateRestClient();
            var response = await client.ExecuteTaskAsync(request);
            if (response.ErrorException != null)
                throw response.ErrorException;
            return HandleResponse<TResponse>(request, response, client);
        }

        private TResponse HandleResponse<TResponse>(IRestRequest request, IRestResponse response, RestClient client)
        {
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Status code {response.StatusCode} returned", new Exception(response.Content));
            }

            if (typeof(Stream) == typeof(TResponse))
            {
                var data = client.DownloadData(request);
                var stream = new MemoryStream(data, false);
                return (TResponse)(object)stream;
            }

            var settings = new JsonSerializerSettings();
            settings.ApplyDefaultDataServicesSerializationSettings();

            return JsonConvert.DeserializeObject<TResponse>(response.Content, settings);
        }


        private RestClient CreateRestClient()
        {
            var uri = new Uri(_baseUrl);
            var relativeUri = _resourceName;
            var client = new RestClient(new Uri(uri, relativeUri))
            {
                //Authenticator = new NtlmAuthenticator()
            };
            return client;
        }

        protected IRestRequest CreateRestRequest(string resource = "", Method method = Method.GET)
        {
            var request = new RestRequest(resource, method)
            {
                JsonSerializer = _remoteServicesSerializer
            };

            return request;
        }
    }
}