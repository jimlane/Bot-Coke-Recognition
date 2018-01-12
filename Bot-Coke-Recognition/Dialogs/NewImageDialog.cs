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
        //private string origChan;
        //private string origID;
        private string attachURL;
        public async Task StartAsync(IDialogContext context)
        {
            //this.origChan = context.Activity.ChannelId;
            //this.origID = context.Activity.Id;
            await context.PostAsync("This looks like a Coca-Cola beverage, but I haven't seen this image before.");
            await context.PostAsync("Would you like me to add this image to the library?");
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            Activity a = (Activity)context.Activity;
            if (a.Attachments.Count > 0)
            {
                attachURL = a.Attachments[0].ContentUrl;
            }
            var message = await result;
            if (message.Text.ToLower() == "yes")
            {
                await context.PostAsync("OK, tell me what beverage this is");
                context.Wait(GetResponse);
            }
            else
            {
                context.Wait(this.MessageReceivedAsync);
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