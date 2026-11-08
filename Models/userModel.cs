using Microsoft.AspNetCore.Mvc;

namespace Stackup.Models
{
    public class userModel
    {
        public int UID { get; set; }
        public string ?Username { get; set; }
        public string ?Password { get; set; }
        public string ?Desc { get; set; }

        public string ?Picture { get; set; }


    }
}
