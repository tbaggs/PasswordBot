using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordNotificationBot
{
    public class PasswordNotificationState : BotState
    {
        /// <summary>The key used to cache the state information in the turn context.</summary>
        private const string StorageKey = "PasswordBot.PasswordNotificationState";

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordNotificationState"/> class.</summary>
        /// <param name="storage">The storage provider to use.</param>
        public PasswordNotificationState(IStorage storage) : base(storage, StorageKey)
        {
        }

        /// <summary>Gets the storage key for caching state information.</summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext) => StorageKey;
    }
}
