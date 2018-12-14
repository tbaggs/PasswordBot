using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace PasswordNotificationBot
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private bool _isProduction = false;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IStorage dataStore;

            var environment = Configuration.GetSection("environment")?.Value == "production" ? "production" : "development";
            _isProduction = environment == "production";

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            if (!File.Exists(botFilePath))
            {
                throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
            }

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(@".\PasswordBot.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));


            // The Memory Storage used here is for local bot debugging only. When the bot
            // is restarted, everything stored in memory will be gone.
            if (!_isProduction)
            {
                dataStore = new MemoryStorage();
            }
            else
            {
                // //Storage configuration name or ID from the .bot file.
                const string StorageConfigurationId = "blob";
                var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);

                if (!(blobConfig is BlobStorageService blobStorageConfig))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
                }

                //Default container name.
                const string DefaultBotContainer = "passwordnotificationbot";
                var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
                dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);
            }

            // Create PasswordNotificationState object.
            // The Password Notification State object is where we persist anything at the notification-scope.
            // Note: It's independent of any user or conversation.
            PasswordNotificationState notificationsState = new PasswordNotificationState(dataStore);

            // Make it available to our bot
            services.AddSingleton(sp => notificationsState);

            ConversationState conversationState = new ConversationState(dataStore);
            UserState userState = new UserState(dataStore);
            DialogState conversationDialogState = new DialogState();

            services.AddBot<PasswordBot>(options =>
            {
                // Retrieve current endpoint.
                var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
                }

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
                options.ChannelProvider = new ConfigurationChannelProvider(Configuration);

                // Creates a logger for the application to use.
                ILogger logger = _loggerFactory.CreateLogger<PasswordBot>();

                // Catches any errors that occur during a conversation turn and logs them.
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });

            services.AddSingleton<StateBotAccessors>(sp =>
            {
                return new StateBotAccessors(conversationDialogState, conversationState, userState)
                {
                    // The dialogs will need a state store accessor. Creating it here once (on-demand) allows the dependency injection
                    // to hand it to our IBot class that is create per-request.
                    ConversationDataAccessor = conversationState.CreateProperty<ConversationData>(StateBotAccessors.ConversationDataName),
                    UserProfileAccessor = userState.CreateProperty<UserProfile>(StateBotAccessors.UserProfileName),
                    ConversationDialogStateAccessor = conversationState.CreateProperty<DialogState>(StateBotAccessors.ConversationDialogName),
                };
            });

            services.AddSingleton(sp =>
            {
                var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
                }

                return (EndpointService)service;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
