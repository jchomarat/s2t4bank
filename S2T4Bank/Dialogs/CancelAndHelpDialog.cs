// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace S2T4Bank.Dialogs
{
    public class CancelAndHelpDialog : ComponentDialog
    {
        protected readonly Localization Localization;

        public CancelAndHelpDialog(string id, Localization localization)
            : base(id)
        {
            Localization = localization;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "help":
                    case "?":
                        await innerDc.Context.SendActivityAsync(Resources.BotLanguage.CancelHelp, cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "annuler":
                    case "arrêter":
                    case "arreter":
                    case "quitter":
                    case "sortir":
                    case "stop":
                    case "quit":
                    case "exit":
                        await innerDc.Context.SendActivityAsync(Resources.BotLanguage.CancelCancel, cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                    case "language":
                        await innerDc.Context.SendActivityAsync($"Current language is: {Localization.Locale}", cancellationToken: cancellationToken);
                        return null;
                }
                // Check the language
                if (text.StartsWith("language"))
                {
                    CultureInfo.CurrentUICulture = new CultureInfo(text.Split(" ")[1]);
                    innerDc.Context.Activity.Locale = CultureInfo.CurrentUICulture.ToString();
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
                    Localization.Locale = innerDc.Context.Activity.Locale;
                }
            }

            return null;
        }
    }
}
