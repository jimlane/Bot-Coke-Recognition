using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace Bot_Coke_Recognition.Dialogs
{
    [Serializable]
    public class NewImageDialog : IDialog<object>
    {
        private string origChan;
        private string origID;
        public Task StartAsync(IDialogContext context)
        {
            this.origChan = context.Activity.ChannelId;
            this.origID = context.Activity.Id;
            context.PostAsync("Do you want to add this image to the library?");
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result as Activity;
            switch (message.Text.ToLower())
            {
                case "yes":
                    await context.PostAsync("OK, tell me what beverage this is");
                    context.Wait(GetResponse);
                    break;
                default:
                    break;
            }
        }

        public async Task GetResponse(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            IMessageActivity message = await result;
            await context.Forward(new LuisImageDialog(this.origChan, this.origID) as IDialog<object>, this.ResumeAfterLuisImageDialog, message, context.CancellationToken);
        }

        private async Task ResumeAfterLuisImageDialog(IDialogContext context, IAwaitable<object> result)
        {
            await CleanupStorage(context.Activity.Id);
            context.Done("");
        }

        private async Task CleanupStorage(string IDToClean)
        {
            //TODO: get rid of any storage entries made during this conversation
            
        }

    }
}