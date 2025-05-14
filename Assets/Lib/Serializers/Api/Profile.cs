using Models;
using System;
using System.Collections.Generic;

namespace Serializers.Api.Profile
{
    [Serializable]
    public class UpdateUsernameRequest
    {
        public UpdateUsernameRequest(string username)
        {
            this.user = new() {
                {"username", username}
            };
        }
        public Dictionary<string, string> user { get; set; }
    }

    [Serializable]
    public class UpdateAvatarRequest
    {
        public UpdateAvatarRequest(string avatar)
        {
            this.user = new() {
                {"avatar", avatar}
            };
        }
        public Dictionary<string, string> user { get; set; }
    }

    [Serializable]
    public class ShowResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public User user { get; set; }
    }

    [Serializable]
    public class UpdateResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public User user { get; set; }
    }
    
}