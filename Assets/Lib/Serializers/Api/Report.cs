using System;

namespace Serializers.Api.Report
{
    [Serializable]
    public class CreateRequest
    {
        public CreateRequest(string room_id, int reason, string notes, string user_id)
        {
            this.reason = reason;
            this.notes = notes;
            this.user_id = user_id;
            this.room_id = room_id;
        }

        public int reason;
        public string notes;
        public string user_id;
        public string room_id;
    }

    [Serializable]
    public class Response
    {
        public string status { get; set; }
        public string error { get; set; }
    }
}