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
        private string attachURL;
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
            Activity a = (Activity)context.Activity;
            if (a.Attachments.Count > 0)
            {
                attachURL = a.Attachments[0].ContentUrl;
            }
            //var message = await result as Activity;
            switch (a.Text.ToLower())
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
            Activity a = (Activity)context.Activity;
            a.Attachments.Insert(0, new Attachment());
            a.Attachments[0].ContentUrl = attachURL;
            await context.Forward(new LuisImageDialog() as IDialog<object>, this.ResumeAfterLuisImageDialog, a, context.CancellationToken);
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