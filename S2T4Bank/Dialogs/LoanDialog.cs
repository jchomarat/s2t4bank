using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.NumberWithUnit;
using S2T4Bank.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Recognizers.Text.DataTypes.TimexExpression.Constants;

namespace S2T4Bank.Dialogs
{
    public class LoanDialog : CancelAndHelpDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

        public LoanDialog(IConfiguration configuration, ILogger logger, Localization localization) : base(nameof(Loan), localization)
        {
            Configuration = configuration;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AmountStepAsync,
                DurationStepAsync,
                InsuranceStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            stepContext.Context.Activity.Locale = Localization.Locale;
            var loan = (Loan)stepContext.Options;

            loan.Assurance = (string)stepContext.Result;

            if (!Enum.IsDefined(typeof(Insurance.Type), loan.Assurance))
            {
                var res = await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken, false);
                loan.Assurance = res.Assurance;
                if (loan.Assurance == null)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(Resources.BotLanguage.LoanNotUnderstood), cancellationToken);
                    return await stepContext.MoveAsync(stepContext, -1, loan.Duration, cancellationToken);
                }

            }

            return await stepContext.EndDialogAsync(loan, cancellationToken);
        }


        private async Task<DialogTurnResult> InsuranceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            var loan = (Loan)stepContext.Options;

            loan.Duration = (string)stepContext.Result;
            if (IsAmbiguous(loan.Duration))
            {
                var res = DateTimeRecognizer.RecognizeDateTime(loan.Duration, Culture.French);
                var dur = res.Where(x => x.TypeName == "datetimeV2.duration").FirstOrDefault();
                if (dur != null)
                {
                    var duration = ConvertResolutionResult(dur);
                    loan.Duration = duration["timex"];
                }
                else
                {
                    // Check if it does contain "1 an"
                    if (loan.Duration.ToLower().Contains("1 an"))
                        loan.Duration = "P1Y";
                }
            }

            if (IsAmbiguous(loan.Duration))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Resources.BotLanguage.LoanNotUnderstood), cancellationToken);
                return await stepContext.MoveAsync(stepContext, -1, loan.Ammount, cancellationToken);
            }

            if (loan.Assurance == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Resources.BotLanguage.LoanInsuranceStep) }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(loan.Duration, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> DurationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            var loan = (Loan)stepContext.Options;

            loan.Ammount = (string)stepContext.Result;
            if (loan.Currency == null)
            {
                var ret = ExtractNumberCurrency(loan.Ammount);
                loan.Ammount = ret.Item1;
                loan.Currency = ret.Item2;
            }
            if (!IsValidNumberCurrency(loan.Ammount, loan.Currency))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Resources.BotLanguage.LoanNotUnderstood), cancellationToken);
                return await stepContext.MoveAsync(stepContext, -1, loan, cancellationToken);
            }

            if ((loan.Duration == null) || IsAmbiguous(loan.Duration))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Resources.BotLanguage.LoanDurationStep) }, cancellationToken);
            }
            return await stepContext.NextAsync(loan.Duration, cancellationToken);
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Localization.Locale = stepContext.Context.Activity.Locale;
            var loan = (Loan)stepContext.Options;

            if (loan.Ammount == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(Resources.BotLanguage.LoanLoanStep) }, cancellationToken);
            }
            return await stepContext.NextAsync(loan.Ammount, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            if (timex == null) return true;
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(TimexTypes.Duration);
        }

        private static Dictionary<string, string> ConvertResolutionResult(ModelResult modelResult)
        {
            return (modelResult.Resolution["values"] as List<Dictionary<string, string>>)[0];
        }

        private static bool IsValidNumberCurrency(string ammount, string currency)
        {
            if ((String.IsNullOrEmpty(currency)) || (String.IsNullOrEmpty(ammount)))
                return false;
            if (NumberWithUnitRecognizer.RecognizeCurrency(ammount, Culture.French).Where(x => x.TypeName == "currency").FirstOrDefault() != null)
            {
                // var ret = res.Resolution["value"];
                return true;
            }
            else
            {
                if (NumberRecognizer.RecognizeNumber(ammount, Culture.French).Where(x => x.TypeName == "number").FirstOrDefault() != null)
                    return true;
            }

            return false;
        }

        private Tuple<string, string> ExtractNumberCurrency(string ammount)
        {
            var res = NumberWithUnitRecognizer.RecognizeCurrency(ammount, Culture.French).Where(x => x.TypeName == "currency").FirstOrDefault();
            if (res != null)
            {
                var amm = (string)res.Resolution["value"];
                var cur = (string)res.Resolution["unit"];
                return new Tuple<string, string>(amm, cur);
            }
            return new Tuple<string, string>(null, null);
        }
    }
}
