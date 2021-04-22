using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using EchoBot1.Services;
using System.Text.RegularExpressions;
using System.Threading;
namespace EchoBot1.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion
        public MainDialog(StateService stateService) : base(nameof(MainDialog))
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            InitializeWaterfallDialog();
        }
            private void InitializeWaterfallDialog()
            {
                var waterfallSteps = new WaterfallStep[]
                {
                   InitialStepAsync,
                   FinalStepAsync
                };
                AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
                AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _stateService));
                AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));
                InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
            }
            private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {

                if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "Hi").Success)

                {
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);

                }


            }
            private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {


                return await stepContext.EndDialogAsync(null, cancellationToken);

            }
        }
    }


