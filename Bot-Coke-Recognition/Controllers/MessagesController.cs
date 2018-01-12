using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Bot.Connector;
using Bot_Coke_Recognition.Dialogs;
using Bot_Coke_Recognition.Helpers;
using Newtonsoft.Json.Linq;

namespace Bot_Coke_Recognition
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var cli = new ConnectorClient(new Uri(activity.ServiceUrl));
            PredictionResults theseResults = new PredictionResults();
            Activity TypingReply = null;
            List<string> ImageTags = new List<string>();
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    if (activity.Attachments.Count > 0)
                    {
                        using (var HttpCli = new HttpClient())
                        {
                            try
                            {
                                //Get the attached image
                                HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await new MicrosoftAppCredentials().GetTokenAsync());
                                HttpCli.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                                var file1 = await HttpCli.GetAsync(activity.Attachments[0].ContentUrl);
                                var thisImage1 = await file1.Content.ReadAsStreamAsync();
                                var file2 = await HttpCli.GetAsync(activity.Attachments[0].ContentUrl);
                                var thisImage2 = await file2.Content.ReadAsStreamAsync();

                                //Begin reply
                                TypingReply = activity.CreateReply();
                                TypingReply.Type = ActivityTypes.Typing;
                                await cli.Conversations.ReplyToActivityAsync(TypingReply);

                                //Determine what kind of picture this is
                                ImageTags = await ComputerVisionHelper.AnalyzeImage(thisImage1);
                                if (ThisIsABeveragePicture(ImageTags))
                                {
                                    //Determine if we've seen this beverage image before
                                    theseResults = CustomVisionHelper.PredictImage(thisImage2);
                                    if (theseResults.Positives.Count == 2)
                                    {
                                        // We've seen this image before
                                        activity.Text = theseResults.Positives[0].ToString() + " " + theseResults.Positives[1].ToString();
                                        await Conversation.SendAsync(activity,
                                            () => { return Chain.From(() => new BeverageDialog() as IDialog<object>); });
                                    }
                                    else if (theseResults.Maybes.Count > 0)
                                    {
                                        // This is probably a new beverage image
                                        activity.Text = theseResults.Maybes[0].ToString();
                                        
                                        await Conversation.SendAsync(GatherImageTags(activity, ImageTags), () => new NewImageDialog());
                                        //await Conversation.SendAsync(GatherImageTags(activity, ImageTags),
                                        //    () => { return Chain.From(() => new NewImageDialog() as IDialog<object>); });
                                    }
                                    else
                                    {
                                        // chain responses to the the Luis NonBeverage Dialog
                                        await Conversation.SendAsync(GatherImageTags(activity, ImageTags),
                                            () => { return Chain.From(() => new NonBeverageDialog() as IDialog<object>); });
                                    }
                                }
                                else
                                {
                                    // chain responses to the the LUIS NonBeverage Dialog
                                    activity.Text = "non beverage";
                                    await Conversation.SendAsync(GatherImageTags(activity, ImageTags),
                                        () => { return Chain.From(() => new NonBeverageDialog() as IDialog<object>); });
                                }

                                //TODO: Figure out why the "None" tag doesn't trigger the LUIS None intent
                                if (activity.Text == "None")
                                {
                                    activity.Text = "asdfadsfadsfdsaf";
                                }
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                                throw;
                            }
                        }
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    if (activity.MembersAdded.Count > 0)
                    {
                        foreach (var newMember in activity.MembersAdded)
                        {
                            if (newMember.Id != activity.Recipient.Id)
                            {
                                // send initial greeting
                                var initialGreeting = activity.CreateReply();
                                initialGreeting.Text = "I'm the Beverage Recognition bot. Send me a picture!";
                                await cli.Conversations.ReplyToActivityAsync(initialGreeting);
                            }
                        }
                    }
                    break;
                default:
                    //HandleSystemMessage(activity);
                    break;
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
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

        private IMessageActivity GatherImageTags(Activity currentActivity, List<string> theseTags)
        {
            foreach (var item in theseTags)
            {
                currentActivity.Properties.Add(new JProperty(currentActivity.Properties.Count.ToString(), item));
            }
            return currentActivity;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}