using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordNotificationBot
{
    public class PasswordNotfications : Dictionary<string, PasswordNotfications.NotificationData>
    {
        public class NotificationData
        {
            public DateTime CreatedTimeStamp { get; set; } = DateTime.Now;

            /// <summary>
            /// Gets or sets the user identity which created the request for password expiration notifications.
            /// </summary>
            /// <value>
            /// Email/identity string of the user to that wishes to be notified 
            /// </value>
            public string UserID { get; set; }

            /// <summary>
            /// Gets or sets the conversation reference to which to send status updates.
            /// </summary>
            /// <value>
            /// The conversation reference to which to send status updates.
            /// </value>
            public ConversationReference Conversation { get; set; }
        }
    }

}
