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
    public class NonBeverageDialog : LuisDialog<object>
    {
        [LuisIntent("Non-Beverage")]
        public async Task NonBeverageIntent(IDialogContext context,
            IAwaitable<IMessageActivity> activity, LuisResult result)
 
        {
            StringBuilder sb = new StringBuilder();
            Activity a = (Activity)context.Activity;
            foreach (var item in a.Properties)
            {
                sb.Append(item.Value + ", ");
            }
            await context.PostAsync("Sorry, but that doesn't look like a beverage.");
            await context.PostAsync("I'm not exactly sure what it is, but here are some of the things I've determined about this picture:");
            await context.PostAsync(sb.ToString());
            context.Done("");
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context,
        IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Done("");
        }

    }
}