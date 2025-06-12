using System;
using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Requests;
using Assets.Scripts.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class RequestModule : MonoBehaviour
    {
        public static RequestModule Singleton { get; private set; }

        private readonly string apiUrl = ConfigModule.Get("API_URL");

        void Awake()
        {
            Singleton = this;
        }

        public void GetRequest<T>(string endPoint, string apiKey = null, Action<T> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            StartCoroutine(MakeRequest(endPoint, RequestMethod.GET, apiKey, null, onSuccess, onFail));
        }

        public void GetRequest(string endPoint, string apiKey = null, Action<string> onFail = null)
        {
            StartCoroutine(MakeRequest<BaseResponse>(endPoint, RequestMethod.GET, apiKey, null, null, onFail));
        }

        public void PostRequest<T>(string endPoint, string apiKey = null, BaseRequest body = null, Action<BaseResponse> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            StartCoroutine(MakeRequest(endPoint, RequestMethod.POST, apiKey, body, onSuccess, onFail));
        }

        public void PostRequest(string endPoint, string apiKey = null, BaseRequest body = null, Action<string> onFail = null)
        {
            StartCoroutine(MakeRequest<BaseResponse>(endPoint, RequestMethod.POST, apiKey, body, null, onFail));
        }

        public void PutRequest<T>(string endPoint, string apiKey = null, BaseRequest body = null, Action<BaseResponse> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            StartCoroutine(MakeRequest(endPoint, RequestMethod.PUT, apiKey, body, onSuccess, onFail));
        }

        public void PutRequest(string endPoint, string apiKey = null, BaseRequest body = null, Action<string> onFail = null)
        {
            StartCoroutine(MakeRequest<BaseResponse>(endPoint, RequestMethod.PUT, apiKey, body, null, onFail));
        }

        public void PatchRequest<T>(string endPoint, string apiKey = null, BaseRequest body = null, Action<BaseResponse> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            StartCoroutine(MakeRequest(endPoint, RequestMethod.PATCH, apiKey, body, onSuccess, onFail));
        }

        public void PatchRequest(string endPoint, string apiKey = null, BaseRequest body = null, Action<string> onFail = null)
        {
            StartCoroutine(MakeRequest<BaseResponse>(endPoint, RequestMethod.PATCH, apiKey, body, null, onFail));
        }

        public void DeleteRequest<T>(string endPoint, string apiKey = null, Action<BaseResponse> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            StartCoroutine(MakeRequest(endPoint, RequestMethod.DELETE, apiKey, null, onSuccess, onFail));
        }

        public void DeleteRequest(string endPoint, string apiKey = null, Action<string> onFail = null)
        {
            StartCoroutine(MakeRequest<BaseResponse>(endPoint, RequestMethod.DELETE, apiKey, null, null, onFail));
        }

        private UnityWebRequest GetWebRequest(string endPoint, RequestMethod method, BaseRequest body = null)
        {
            string url = $"{apiUrl.TrimEnd('/')}/{endPoint.TrimStart('/')}";

            return method switch
            {
                RequestMethod.POST or RequestMethod.PUT or RequestMethod.PATCH => UnityWebRequest.Put(url, body.ToJson()),
                RequestMethod.DELETE => UnityWebRequest.Delete(url),
                _ => UnityWebRequest.Get(url),
            };
        }

        private IEnumerator MakeRequest<T>(string endPoint, RequestMethod method, string apiKey = null, BaseRequest body = null, Action<T> onSuccess = null, Action<string> onFail = null) where T : BaseResponse
        {
            using UnityWebRequest request = GetWebRequest(endPoint, method, body);

            if (method == RequestMethod.POST)
                request.method = UnityWebRequest.kHttpVerbPOST;
            else
                request.method = method.ToString();

            if (apiKey != null)
                request.SetRequestHeader("Authorization", apiKey);

            if (method == RequestMethod.POST || method == RequestMethod.PUT || method == RequestMethod.PATCH)
                request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke(request.error);
                yield break;
            }

            if (onSuccess == null)
                yield break;

            T response = BaseResponse.CreateFromJSON<T>(request.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
    }
}