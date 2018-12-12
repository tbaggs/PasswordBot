using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordNotificationBot
{
    public class StateBotAccessors
    {
        public StateBotAccessors(DialogState conversationDialogState, ConversationState conversationState, UserState userState)
        {
            ConversationDialogState = conversationDialogState ?? throw new ArgumentNullException(nameof(conversationDialogState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public static string UserProfileName { get; } = "UserProfile";

        public static string ConversationDataName { get; } = "ConversationData";

        public static string ConversationDialogName { get; } = "ConversationDialog";

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> ConversationDialogStateAccessor { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public DialogState ConversationDialogState { get;  }
}

    public class UserProfile
    {
        public string EMail { get; set; }
        public string Name { get; set; }
    }
    public class ConversationData
    {
        // The time-stamp of the most recent incoming message.
        public string Timestamp { get; set; }

        // The ID of the user's channel.
        public string ChannelId { get; set; }

        // Track whether we have already asked the user's name
        public bool ConfiguredNotification{ get; set; } = false;
    }
}
