using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace Bot_Coke_Recognition.Dialogs
{
    [LuisModel("c1928865-1f14-407f-b75c-2f43ea7ad190", "0ff1c954c30844ffb894631c6e26ed7b")]
    [Serializable]
    public class LuisResponseDialog : LuisDialog<object>
    {
        [LuisIntent("Can-Of-Coke")]
        public async Task CokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Coke");
        }

        [LuisIntent("Can-Of-CokeZero")]
        public async Task CokeZeroCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Coke Zero");
        }

        [LuisIntent("Can-Of-Diet-Coke")]
        public async Task DietCokeCanIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("That looks like a can of Diet Coke");
        }

        [LuisIntent("Casual-Chat")]
        public async Task CasualChatIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Hello - send me a picture!");
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Sorry, I can't determine what that is");
            context.Wait(MessageReceived);
        }

    }
}