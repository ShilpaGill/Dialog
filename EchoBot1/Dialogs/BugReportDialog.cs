﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using EchoBot1.Models;
using EchoBot1.Services;
using System.Text.RegularExpressions;

namespace EchoBot1.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion
        public BugReportDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            InitializeWaterfallDialog();
        }
        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync,
            CallbackTimeStepAsync,
            PhoneNumberStepAsync,
            BugStepAsync,
            SummaryStepAsync
            };
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.mainFlow", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));
            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";


        }
        #region Waterfall Steps
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a description for your report")
                }, cancellationToken);


        }
        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = (string)stepContext.Result;
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a callback time"),
                    RetryPrompt = MessageFactory.Text("The value entered must be between the hour of 9 am and 5 pm."),
                }, cancellationToken);


        }
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["callbackTime"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a phone number that we can call you back at"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid phone number"),
                }, cancellationToken);


        }
        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = (string)stepContext.Result;
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the type of bug"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Performance", "Usability", "Serious Bug", "Other" }),
                }, cancellationToken);


        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;
            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Description = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = (string)stepContext.Values["PhoneNumber"];
            userProfile.Bug = (string)stepContext.Values["bug"];

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is summary of your bug report:"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Description: {0}", userProfile.Description)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Callback Time: {0}", userProfile.CallbackTime.ToString())), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Phone Number: {0}", userProfile.PhoneNumber)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Bug: {0}", userProfile.Bug)), cancellationToken);

            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion
        #region Validators
        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            if (promptContext.Recognized.Succeeded)
            {
                var resolution = promptContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9, 0, 0);
                TimeSpan end = new TimeSpan(17, 0, 0);
                if ((selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end))
                {
                    valid = true;
                }
            }
            return Task.FromResult(valid);
        }
        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{4}$").Success;
            }
            return Task.FromResult(valid);

        }


        #endregion
    }
}



