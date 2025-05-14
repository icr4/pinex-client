using System;
using System.Collections.Generic;

namespace Serializers.Api.Ranking
{
    [Serializable]
    public class ListResponse
    {
        public string status { get; set; }
        public string error { get; set; }
        public bool more { get; set; }
        public List<Models.Friend> rankings { get; set; }
    }
}