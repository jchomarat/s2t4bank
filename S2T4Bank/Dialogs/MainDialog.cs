// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using S2T4Bank.Model;

namespace S2T4Bank.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;
        protected readonly Localization Localization;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, Localization localization)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;
            Localization = localization;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new LoanDialog(configuration, logger, localization));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckLanguageAsync,
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckLanguageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            
            if (stepContext.Context.Activity.Text?.ToLower() == "language")
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Resources.BotLanguage.MainLanguage) }, cancellationToken);
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;

            var res = (string)stepContext.Result;
            if (!String.IsNullOrEmpty(res))
            {
                Localization.Locale = res;
            }

            if (string.IsNullOrEmpty(Configuration[$"LuisAppId-{Localization.Locale}"])
                || string.IsNullOrEmpty(Configuration[$"LuisAPIKey-{Localization.Locale}"])
                || string.IsNullOrEmpty(Configuration[$"LuisAPIHostName-{Localization.Locale}"]))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(Resources.BotLanguage.MainLuisError), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Resources.BotLanguage.MainIntroStep) }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            stepContext.Context.Activity.Locale = Localization.Locale;
            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var loan = stepContext.Result != null
                    ?
                await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken)
                    :
                new Loan();

            // In this sample we only have a single Intent we are concerned with. However, typically a scenario
            // will have multiple different Intents each corresponding to starting a different child Dialog.

            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            return await stepContext.BeginDialogAsync(nameof(Loan), loan, cancellationToken);

            //return await stepContext.NextAsync(bookingDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            if (stepContext.Result != null)
            {
                var result = (Loan)stepContext.Result;

                // Now we have all details, display the results
                var nicedate = Duration.TimexDurationToFrench(new TimexProperty(result.Duration));
                var msg = String.Format(Resources.BotLanguage.MainFinalStep, result.Ammount, result.Currency, nicedate, Insurance.GetFriendlyName(result.Assurance));
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                msg = JsonConvert.SerializeObject(result);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Resources.BotLanguage.MainFinalStepThanks), cancellationToken);
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
