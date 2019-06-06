// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using S2T4Bank.Model;

namespace S2T4Bank
{
    public static class LuisHelper
    {
        public static async Task<Loan> ExecuteLuisQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken, bool checkIntent = true)
        {
            var loan = new Loan();

            try
            {
                // Create the LUIS settings from configuration.
                var luisApplication = new LuisApplication(
                    configuration[$"LuisAppId-{turnContext.Activity.Locale}"],
                    configuration[$"LuisAPIKey-{turnContext.Activity.Locale}"],
                    "https://" + configuration[$"LuisAPIHostName-{turnContext.Activity.Locale}"]
                );

                var recognizer = new LuisRecognizer(luisApplication);

                // The actual call to LUIS
                var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);

                var (intent, score) = recognizerResult.GetTopScoringIntent();
                if ((intent == "emprunt") || (!checkIntent))
                {
                    // We need to get the result from the LUIS JSON which at every level returns an array.
                    loan.Ammount = recognizerResult.Entities["money"]?.FirstOrDefault()?["number"]?.ToString();
                    loan.Currency = recognizerResult.Entities["money"]?.FirstOrDefault()?["units"]?.ToString();
                    loan.Assurance = recognizerResult.Entities["insurance"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();

                    // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                    // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                    var test = JsonConvert.DeserializeObject<LuisInsurance>(recognizerResult.Entities.ToString());
                    loan.Duration = test?.Datetime.Where(x => x.Type == "duration").FirstOrDefault()?.Timex?[0].ToString();
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"LUIS Exception: {e.Message} Check your LUIS configuration.");
            }

            return loan;
        }
    }
}
