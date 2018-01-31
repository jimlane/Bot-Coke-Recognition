using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Beverage_Bot.Helpers;


namespace Beverage_Bot.Dialogs
{
    [Serializable]
    public class NewImageDialog : IDialog<object>
    {
        private const string AddImage = "Add Image";
        private const string DontAddImage = "Do Not Add Image";
        private string attachURL;
        public async Task StartAsync(IDialogContext context)
        {
            Activity a = (Activity)context.Activity;
            if (a.Attachments.Count > 0)
            {
                attachURL = a.Attachments[0].ContentUrl;
            }
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { AddImage, DontAddImage }, "This appears to be a beverage container, but I haven't seen it before. Would you like to add this image to the collection?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case AddImage:
                        Activity activity = (Activity)context.Activity;
                        activity.Text = context.UserData.GetValue<string>("Intent1");
                        activity.Attachments.Add(new Attachment());
                        activity.Attachments[0].ContentUrl = attachURL;
                        await context.Forward(new LuisImageDialog(), this.ResumeAfterAddImageDialog, activity, CancellationToken.None);
                        context.Done("");
                        break;
                    case DontAddImage:
                        await context.PostAsync("OK, thanks for sharing your picture!");
                        context.Done("Exiting");
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterAddImageDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
                //context.Done("");
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}