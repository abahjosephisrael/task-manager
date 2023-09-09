using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TaskManager.Application.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
        }
        public Response(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }
        [JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("error")]
        public List<string> Errors { get; set; }
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
