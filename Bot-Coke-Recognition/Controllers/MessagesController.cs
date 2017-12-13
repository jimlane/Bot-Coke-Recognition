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
                                var file = await HttpCli.GetAsync(activity.Attachments[0].ContentUrl);
                                var thisImage = await file.Content.ReadAsStreamAsync();

                                //Cache the image in case its needed later for training
                                CacheAttachment(thisImage, activity);

                                //Determine what kind of picture this is
                                activity.Text = VisionHelper.getTags(thisImage);
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
                    else
                    {
                        var thisasdf = "asdfasdf";
                    }
                    // chain responses to the the Luis Response Dialog
                    var TypingReply = activity.CreateReply();
                    TypingReply.Type = ActivityTypes.Typing;
                    await cli.Conversations.ReplyToActivityAsync(TypingReply);
                    await Conversation.SendAsync(activity,
                        () => { return Chain.From(() => new BeverageDialog() as IDialog<object>); });

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
                    HandleSystemMessage(activity);
                    break;
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private bool CacheAttachment(dynamic thisAttachment, Activity thisActivity)
        {
            try
            {
                //send attachment to blob storage
                BlobUtility thisBlob = new BlobUtility();
                thisBlob.PutBlob(thisAttachment.ToString(), thisActivity.Id, "application/octet-stream", new BlobContainerPermissions());

                //cache the attachment's URI in table storage
                TableUtility thisTable = new TableUtility();
                CloudTable myTable = thisTable.TableClient.GetTableReference("BotImageCache");
                myTable.CreateIfNotExists();
                ImageCache myCache = new ImageCache(thisActivity.ChannelId, thisActivity.Id);
                myCache.location = thisBlob.BlobContainer.StorageUri.PrimaryUri.ToString();
                //thisTable.Insert(myTable, myCache);
                TableOperation insertOperation = TableOperation.Insert(myCache);
                myTable.Execute(insertOperation);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }

            return true;
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