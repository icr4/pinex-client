using System;
using System.Collections.Generic;

namespace Serializers.Api.Friendship
{
    [Serializable]
    public class CreateRequest
    {
        public CreateRequest(string friend_id) {
            this.friend_id = friend_id;
        }

        public string friend_id;
    }

    [Serializable]
    public class ConfirmRequest
    {
        public ConfirmRequest(string ref_id) {
            this.ref_id = ref_id;
        }

        public string ref_id;
    }

    [Serializable]
    public class ListResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public List<Models.Friendship> friendships { get; set; }
    }

    [Serializable]
    public class SearchResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public List<Models.Friend> users { get; set; }
    }
}