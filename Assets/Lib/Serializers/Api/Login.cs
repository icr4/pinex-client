using Models;
using System;
using Constants;

namespace Serializers.Api.Login
{
    [Serializable]
    public class LoginRequest
    {
        public LoginRequest(string token, string provider, string username)
        {
            this.token = token;
            this.provider = provider;
            this.version = ClientConstants.VERSION;
            this.username = username;
        }
        public string version { get; set; }
        public string token { get; set; }
        public string provider { get; set; }

        public string username { get; set; }
    }

    [Serializable]
    public class LoginVerifyRequest
    {
        public LoginVerifyRequest(string auth_token)
        {
            this.auth_token = auth_token;
            this.version = ClientConstants.VERSION;
        }
        public string auth_token { get; set; }
        public string version { get; set; }
    }

    [Serializable]
    public class LoginResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public string auth_token { get; set; }
        public string reason { get; set; }
        public string expires_at { get; set; }
        public User user { get; set; }
    }
}