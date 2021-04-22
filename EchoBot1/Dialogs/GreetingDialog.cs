using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using EchoBot1.Models;
using EchoBot1.Services;


namespace EchoBot1.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _botStateService;
        #endregion
        public GreetingDialog(string dialogId, StateService stateService) : base(dialogId)
        {

            _botStateService = stateService ?? throw new System.ArgumentNullException(nameof(StateService));
            InitializeWaterfallDialog();
        }
        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";

        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")
                    }, cancellationToken);

                    
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                userProfile.Name = (string)stepContext.Result;
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can i help you today?", userProfile.Name)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        } 

    }
}
