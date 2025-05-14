using System.Collections.Generic;

namespace Models
{
    public class User
    {
        public string id;
        public int level;
        public int coins;
        public int tokens;
        public string username;
        public string email;
        public string provider;
        public string avatar;
        public List<string> avatars;
        public string inserted_at;
        public string updated_at;
    }
}