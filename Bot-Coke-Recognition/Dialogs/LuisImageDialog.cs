using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Http;
using System.Net.Http.Headers;
using Beverage_Bot.Helpers;


namespace Beverage_Bot.Dialogs
{
    [LuisModel("c1928865-1f14-407f-b75c-2f43ea7ad190", "0ff1c954c30844ffb894631c6e26ed7b")]
    [Serializable]
    public class LuisImageDialog : LuisDialog<object>
    {
        private string origChan;
        private string origID;
        private static Stream fileImage;

        [LuisIntent("Can-Of-Coke")]
        public async Task CokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            try
            {
                Activity a = (Activity)context.Activity;
                //add this image to the Custom Vision repository and retrain the project
                List<string> myList = new List<string>();
                myList.Add("can");
                myList.Add("coke");
                if (CustomVisionHelper.addImage(await RetrieveAttachment(a.Attachments[0].ContentUrl), myList))
                {
                    await context.PostAsync("Added your image of a can of Coke");
                }
                else
                {
                    await context.PostAsync("Something went wrong - try again later");
                }

                context.Done("");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }
        }

        [LuisIntent("Can-Of-CokeZero")]
        public async Task CokeZeroCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Coke Zero");
            context.Done("");
        }

        [LuisIntent("Can-Of-Diet-Coke")]
        public async Task DietCokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Diet Coke");
            context.Done("");
        }

        [LuisIntent("Bottle-Of-Coke")]
        public async Task CokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            try
            {
                Activity a = (Activity)context.Activity;
                //add this image to the Custom Vision repository and retrain the project
                List<string> myList = new List<string>();
                myList.Add("can");
                myList.Add("bottle");
                if (CustomVisionHelper.addImage(await RetrieveAttachment(a.Attachments[0].ContentUrl), myList))
                {
                    await context.PostAsync("Added your image of a bottle of Coke");
                }
                else
                {
                    await context.PostAsync("Something went wrong - try again later");
                }

                context.Done("");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }
        }

        [LuisIntent("Bottle-Of-Diet-Coke")]
        public async Task DietCokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Diet Coke");
            context.Done("");
        }

        [LuisIntent("Bottle-Of-CokeZero")]
        public async Task CokeZeroBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Coke Zeror");
            context.Done("");
        }

        [LuisIntent("Can-Of-Sprite")]
        public async Task SpriteCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Sprite");
            context.Done("");
        }

        [LuisIntent("Bottle-Of-Sprite")]
        public async Task SpriteBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Sprite");
            context.Done("");
        }

        [LuisIntent("Casual-Chat")]
        public async Task CasualChatIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("LuisImageDialog.cs - I don't understand your input - resetting");
            context.Done("");
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("LuisImageDialog.cs - I don't understand your input - resetting");
            context.Done("");
        }
        private async Task ResumeAfterImageAddition(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }

        private async Task<Stream> RetrieveAttachment(string thisURL)
        {
            try
            {
                using (var HttpCli = new HttpClient())
                {
                    //Get the attached image
                    HttpCli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await new MicrosoftAppCredentials().GetTokenAsync());
                    HttpCli.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    var file1 = await HttpCli.GetAsync(thisURL);
                    return await file1.Content.ReadAsStreamAsync();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }
        }

    }
}