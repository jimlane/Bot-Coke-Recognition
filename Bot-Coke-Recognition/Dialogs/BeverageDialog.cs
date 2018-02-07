using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace Beverage_Bot.Dialogs
{
    [LuisModel("c1928865-1f14-407f-b75c-2f43ea7ad190", "0ff1c954c30844ffb894631c6e26ed7b")]
    [Serializable]
    public class BeverageDialog : LuisDialog<object>
    {
        [LuisIntent("Non-Beverage")]
        public async Task NonBeverageIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Why?");
            context.Done<object>(null);
        }

        [LuisIntent("Can-Of-Coke")]
        public async Task CokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Coke");
            context.Done<object>(null);
        }

        [LuisIntent("Can-Of-CokeZero")]
        public async Task CokeZeroCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Coke Zero");
            context.Done<object>(null);
        }

        [LuisIntent("Can-Of-Diet-Coke")]
        public async Task DietCokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Diet Coke");
            context.Done<object>(null);
        }

        [LuisIntent("Bottle-Of-Coke")]
        public async Task CokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a bottle of Coke");
            context.Done<object>(null);
        }

        [LuisIntent("Bottle-Of-Diet-Coke")]
        public async Task DietCokeBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a bottle of Diet Coke");
            context.Done<object>(null);
        }

        [LuisIntent("Bottle-Of-CokeZero")]
        public async Task CokeZeroBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a bottle of Coke Zero");
            context.Done<object>(null);
        }

        [LuisIntent("Can-Of-Sprite")]
        public async Task SpriteCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Sprite");
            context.Done<object>(null);
        }

        [LuisIntent("Bottle-Of-Sprite")]
        public async Task SpriteBottleIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a bottle of Sprite");
            context.Done<object>(null);
        }

        [LuisIntent("Casual-Chat")]
        public async Task CasualChatIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Hello - send me a picture!");
            context.Done<object>(null);
        }

        private async Task ResumeAfterNewImageDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ImageResult = await result;
            await context.PostAsync("Image added successfully!");
            context.Done<object>(null);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("BeverageDialog.cs - something went wrong!");
            context.Done<object>(null);
        }

        [LuisIntent("")]
        public async Task UnknownIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("BeverageDialog.cs - something went wrong!");
            context.Done<object>(null);
        }
    }
}