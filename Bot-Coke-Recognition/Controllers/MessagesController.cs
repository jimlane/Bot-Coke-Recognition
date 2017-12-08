using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals;
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
                                HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await new MicrosoftAppCredentials().GetTokenAsync());
                                HttpCli.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                                var file = await HttpCli.GetAsync(activity.Attachments[0].ContentUrl);

                                //StateClient stateClient = new StateClient(new MicrosoftAppCredentials("2405d9a4-cc5c-4da6-96c8-31d65beade23", "xqdRXRT76{yqinECL511)#|"));
                                //BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                                //userData.SetProperty<string>("imageTags", await VisionHelper.getTags(await file.Content.ReadAsStreamAsync()));

                                activity.Text = await VisionHelper.getTags(await file.Content.ReadAsStreamAsync());
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                                throw;
                            }
                        }
                    }
                    // chain responses to the the Luis Response Dialog
                    var TypingReply = activity.CreateReply();
                    TypingReply.Type = ActivityTypes.Typing;
                    await cli.Conversations.ReplyToActivityAsync(TypingReply);
                    await Conversation.SendAsync(activity,
                        () => { return Chain.From(() => new LuisResponseDialog() as IDialog<object>); });

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