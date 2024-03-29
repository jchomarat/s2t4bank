// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace S2T4Bank.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, Localization localization)
            : base(conversationState, userState, dialog, logger, localization)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment(Localization.Locale);
                    var response = CreateResponse(turnContext.Activity, welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }

        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            Localization.Locale = turnContext.Activity.Locale;
            return base.OnTurnAsync(turnContext, cancellationToken);
        }

        // Create an attachment message response.
        private Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment(string locale)
        {

            // combine path for cross platform support
            string[] paths = { ".", "Cards", $"welcomeCard-{locale}.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}

