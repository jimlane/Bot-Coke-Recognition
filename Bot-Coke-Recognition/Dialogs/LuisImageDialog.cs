using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Bot_Coke_Recognition.Helpers;


namespace Bot_Coke_Recognition.Dialogs
{
    [LuisModel("c1928865-1f14-407f-b75c-2f43ea7ad190", "0ff1c954c30844ffb894631c6e26ed7b")]
    [Serializable]
    public class LuisImageDialog : LuisDialog<object>
    {
        private string origChan;
        private string origID;
        private Stream fileImage;

        public LuisImageDialog(string ChanID, string convID)
        {
            this.origID = convID;
            this.origChan = ChanID;
        }

        [LuisIntent("Can-Of-Coke")]
        public async Task CokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Coke");
        }

        [LuisIntent("Can-Of-CokeZero")]
        public async Task CokeZeroCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Coke Zero");
        }

        [LuisIntent("Can-Of-Diet-Coke")]
        public async Task DietCokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Diet Coke");
        }

        [LuisIntent("Bottle-Of-Coke")]
        public async Task CokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            //retrieve the attachment from blob storage
            RetrieveAttachment();
            //add this image to the Custom Vision repository and retrain the project
            if (VisionHelper.addImage(fileImage, (List<string>)result.Entities))
            {
                await context.PostAsync("Added your image of a bottle of Coke");
            }
            else
            {
                await context.PostAsync("Something went wrong - try again later");
            }
            context.Call(new BeverageDialog() as IDialog<object>, this.ResumeAfterImageAddition);
        }

        [LuisIntent("Bottle-Of-Diet-Coke")]
        public async Task DietCokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Diet Coke");
        }

        [LuisIntent("Bottle-Of-CokeZero")]
        public async Task CokeZeroBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Coke Zeror");
        }

        [LuisIntent("Can-Of-Sprite")]
        public async Task SpriteCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a can of Sprite");
        }

        [LuisIntent("Bottle-Of-Sprite")]
        public async Task SpriteBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Added your image of a bottle of Sprite");
        }

        [LuisIntent("Casual-Chat")]
        public async Task CasualChatIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("I don't understand your input - resetting");
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("I don't understand your input - resetting");
        }
        private async Task ResumeAfterImageAddition(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }

        private bool RetrieveAttachment()
        {
            try
            {
                //retrieve the attachment's URI from table storage
                TableUtility thisTable = new TableUtility();
                CloudTable myTable = thisTable.TableClient.GetTableReference("BotImageCache");
                TableQuery<ImageCache> query = new TableQuery<ImageCache>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, this.origID));
                ImageCache myCache = new ImageCache();
                foreach (ImageCache item in myTable.ExecuteQuery(query))
                {
                    myCache.location = item.location;
                }

                //retrieve actual attachment from blob storage
                BlobUtility thisBlob = new BlobUtility();
                fileImage = thisBlob.GetBlob(myCache.location);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }

            return true;
        }

    }
}