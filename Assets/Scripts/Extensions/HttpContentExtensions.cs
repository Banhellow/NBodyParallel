using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine;

namespace Extensions
{
    public static class HttpContentExtensions
    {
        public static T GetContentData<T>(this HttpContent content)
        {
            var responseBody =  content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(responseBody);
        }
    }
}