using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Beverage_Bot.Helpers;
using Newtonsoft.Json.Linq;

namespace Beverage_Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        List<string> ImageTags = new List<string>();
        PredictionResults theseResults = new PredictionResults();
        Stream thisImage1;
        Stream thisImage2;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (hasValidAttachment(activity))
            {
                await ProcessAttachments((Activity)context.Activity);
                if (ThisIsABeveragePicture(ImageTags))
                {
                    if (ThisIsAKnownPicture())
                    {
                        // This is an image we've trained on before
                        activity.Text = theseResults.Positives[0].ToString() + " " + theseResults.Positives[1].ToString();
                        await context.Forward(new BeverageDialog(), this.ResumeAfterImageDialog, activity, CancellationToken.None);
                    }
                    else
                    {
                        // This is probably a new beverage image
                        context.UserData.SetValue<string>("Intent1", theseResults.Maybes[0].ToString());
                        await context.Forward(new NewImageDialog(), this.ResumeAfterImageDialog, activity, CancellationToken.None);
                    }
                }
                else
                {
                    // Not sure what this image is, but we'll describe what we see to the user
                    activity.Text = "non beverage";
                    await context.Forward(new NonBeverageDialog(), this.ResumeAfterImageDialog, this.GatherImageTags(activity, ImageTags), CancellationToken.None);
                }
            }
            else
            {
                await context.PostAsync("Hi, I'm the Beverage Recognition bot. Send me a picture!");
                context.Done("");
            }
        }

        private async Task ProcessAttachments(Activity thisActivity)
        {
            if (thisActivity.Attachments.Count > 0)
            {
                //Retrieve the attachment
                thisImage1 = await getAttachment(thisActivity);
                thisImage2 = await getAttachment(thisActivity);
                //Determine what kind of picture this is
                ImageTags = await ComputerVisionHelper.AnalyzeImage(thisImage1);
            }
        }

        private bool ThisIsABeveragePicture(List<string> imageTags)
        {
            foreach (var item in imageTags)
            {
                if (item.IndexOf("beverage") >= 0 ||
                    item.IndexOf("can") >= 0 ||
                    item.IndexOf("bottle") >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ThisIsAKnownPicture()
        {
            //Determine if we've seen this beverage image before
            theseResults = CustomVisionHelper.PredictImage(thisImage2);
            if (theseResults.Positives.Count > 1)
            {
                return true;
            }
            return false;
        }

        private async Task ResumeAfterImageDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
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

        private bool hasValidAttachment(Activity thisActivity)
        {
            if (thisActivity.Attachments.Count > 0)
            {
                if (thisActivity.Attachments[0].ContentUrl.Contains("skype"))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<Stream> getAttachment(Activity thisActivity)
        {
            using (var HttpCli = new HttpClient())
            {
                try
                {
                    //Get the attached image
                    HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await new MicrosoftAppCredentials().GetTokenAsync());
                    HttpCli.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    var file1 = await HttpCli.GetAsync(thisActivity.Attachments[0].ContentUrl);
                    return await file1.Content.ReadAsStreamAsync();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                    throw;
                }
            }
        }
        private IMessageActivity GatherImageTags(Activity currentActivity, List<string> theseTags)
        {
            foreach (var item in theseTags)
            {
                currentActivity.Properties.Add(new JProperty(currentActivity.Properties.Count.ToString(), item));
            }
            return currentActivity;
        }

    }
}