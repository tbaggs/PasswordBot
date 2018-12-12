﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;

namespace PasswordNotificationBot
{
    public class PasswordBot : IBot
    {
        public const string _welcomeText = @"Hello!  I'm Pass, the password reset notification bot.  You can
                                            use me to get notifications when your password is about to expire.
                                            To get started just type 'Notify Me'";

        private readonly PasswordNotificationState _notificationState;
        private readonly IStatePropertyAccessor<PasswordNotfications> _passwordNotificationsPropertyAccessor;
        private readonly StateBotAccessors _accessors;
        private readonly DialogSet _dialogs;

        public PasswordBot(StateBotAccessors accessors, PasswordNotificationState notificationsState, EndpointService endpointService)
        {
            _notificationState = notificationsState ?? throw new ArgumentNullException(nameof(notificationsState));
            _passwordNotificationsPropertyAccessor =
                notificationsState.CreateProperty<PasswordNotfications>(nameof(PasswordNotfications));

            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            AppId = string.IsNullOrWhiteSpace(endpointService.AppId) ? "1" : endpointService.AppId;

            _dialogs = new DialogSet(accessors.ConversationDialogStateAccessor);
            _dialogs.Add(new TextPrompt("EmailAddress"));
        }

        private string AppId { get; }


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.ChannelId == "emulator" || turnContext.Activity.ChannelId =="directline")
            {

            }


            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                // Handle non-message activities.
                await OnSystemActivityAsync(turnContext);
            }
            else
            {
                PasswordNotfications notifications = await _passwordNotificationsPropertyAccessor.GetAsync(turnContext, () => new PasswordNotfications());

                // Get the user's text input for the message.
                var text = turnContext.Activity.Text.Trim().ToLowerInvariant();

                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                switch (text)
                {
                    case "notify":
                    case "notify me":
                        
                        // If the DialogTurnStatus is Empty we should start a new dialog.
                        if (results.Status == DialogTurnStatus.Empty)
                        {
                            // A prompt dialog can be started directly on the DialogContext. The prompt text is given in the PromptOptions.
                            await dialogContext.PromptAsync(
                                "EmailAddress",
                                new PromptOptions { Prompt = MessageFactory.Text("Please enter your email.") },
                                cancellationToken);
                        }

                        break;

                    case "show":

                        // Display information for all jobs in the log.
                        if (notifications.Count > 0)
                        {
                            await turnContext.SendActivityAsync($"There are {notifications.Count} notifications registered in the system");
                        }
                        else
                        {
                            await turnContext.SendActivityAsync("The notifications collection is empty.");
                        }

                        break;

                    default:
                        if (results.Status == DialogTurnStatus.Complete)
                        {
                            // Check for a result.
                            if (results.Result != null)
                            {
                                string userId = turnContext.Activity.Text;

                                // Create a new notification for the user.
                                PasswordNotfications.NotificationData notification = CreateNotification(turnContext, notifications, userId);

                                // Set the new property
                                await _passwordNotificationsPropertyAccessor.SetAsync(turnContext, notifications);

                                // Now save it into the Notification State
                                await _notificationState.SaveChangesAsync(turnContext);

                                await turnContext.SendActivityAsync(
                                    $"Created notification for {results.Result}. We'll notify you when a password expiration is about to occur.");
                            }
                        }
                        else
                        {
                            await turnContext.SendActivityAsync("I didn't understand your input. Try again.");
                        }

                        break;
                }

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }

        private PasswordNotfications.NotificationData CreateNotification(ITurnContext turnContext, PasswordNotfications notifications, string userId)
        {
            PasswordNotfications.NotificationData notification = new PasswordNotfications.NotificationData
            {
                UserID = userId,
                CreatedTimeStamp = DateTime.Now,
                Conversation = turnContext.Activity.GetConversationReference(),
            };

            notifications[notification.UserID] = notification;

            return notification;
        }

        private async Task OnSystemActivityAsync(ITurnContext turnContext)
        {
            // On a job completed event, mark the job as complete and notify the user.
            if (turnContext.Activity.Type is ActivityTypes.Event)
            {
            //    var jobLog = await _jobLogPropertyAccessor.GetAsync(turnContext, () => new JobLog());
            //    var activity = turnContext.Activity.AsEventActivity();
            //    if (activity.Name == JobCompleteEventName
            //        && activity.Value is long timestamp
            //        && jobLog.ContainsKey(timestamp)
            //        && !jobLog[timestamp].Completed)
            //    {
            //        await CompleteJobAsync(turnContext.Adapter, AppId, jobLog[timestamp]);
            //    }
            }
            else if (turnContext.Activity.Type is ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext);
                }
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(_welcomeText);
                }
            }
        }
    }
}
