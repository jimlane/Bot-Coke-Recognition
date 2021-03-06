﻿using System;
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
        string ConvID;

        public async Task StartAsync(IDialogContext context)
        {
            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - StartAsync: " + e.Message.ToString());
                throw;
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
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
                    context.Wait(this.MessageReceivedAsync);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - MessageReceivedAsync: " + e.Message.ToString());
                throw;
            }
        }

        private async Task ProcessAttachments(Activity thisActivity)
        {
            try
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - ProcessAttachments: " + e.Message.ToString());
                throw;
            }
        }

        private bool ThisIsABeveragePicture(List<string> imageTags)
        {
            try
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - ThisIsABeveragePicture: " + e.Message.ToString());
                throw;
            }
        }

        private bool ThisIsAKnownPicture()
        {
            try
            {
                //Determine if we've seen this beverage image before
                theseResults = CustomVisionHelper.PredictImage(thisImage2);
                if (theseResults.Positives.Count > 1)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - ThisIsAKnownPicture: " + e.Message.ToString());
                throw;
            }
        }

        private async Task ResumeAfterImageDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception e)
            {
                await context.PostAsync("Error - ResumeAfterImageDialog: " + e.Message.ToString());
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private bool hasValidAttachment(Activity thisActivity)
        {
            try
            {
                if (thisActivity.Attachments.Count > 0)
                {
                    //we have to jump through these hoops for Teams which always attaches an attachment indicating the channel
                    if (thisActivity.Attachments[0].Content != null)
                    {
                        if (thisActivity.Attachments[0].Content.ToString().Contains("skype"))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - hasValidAttachment: " + e.Message.ToString());
                throw;
            }
        }

        private async Task<Stream> getAttachment(Activity thisActivity)
        {
            try
            {
                using (var HttpCli = new HttpClient())
                {
                    try
                    {
                        // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                        if ((thisActivity.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase)) || (thisActivity.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var token = await new MicrosoftAppCredentials().GetTokenAsync();
                            HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        //Get the attached image
                        //HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await new MicrosoftAppCredentials().GetTokenAsync());
                        //HttpCli.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - getAttachment: " + e.Message.ToString());
                throw;
            }
        }
        private IMessageActivity GatherImageTags(Activity currentActivity, List<string> theseTags)
        {
            try
            {
                foreach (var item in theseTags)
                {
                    currentActivity.Properties.Add(new JProperty(currentActivity.Properties.Count.ToString(), item));
                }
                return currentActivity;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error - GatherImageTags: " + e.Message.ToString());
                throw;
            }
        }

    }
}