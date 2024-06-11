using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GatePass.Models
{
    public class MyModel
    {
        public static string AccessToken { get; internal set; }
        public string id_token { get; set; }
        public string code { get; set; }
        public string state { get; set; }

        public string access_token { get; set; }
        public string token_type { get; set;
        }
    }


    
}