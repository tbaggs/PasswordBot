using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordNotificationBot
{
    public class PasswordEvent
    {
        public string UserID { get; set; }
        public string PasswordExpirationDate { get; set; }
    }

    public class PasswordEventNotification
    {
        [JsonProperty("PasswordEventNotification")]
        public PasswordEvent PasswordEvent { get; set; }
    }
}
