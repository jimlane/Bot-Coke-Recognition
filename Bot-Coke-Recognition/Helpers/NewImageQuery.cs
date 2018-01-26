using System;
using Microsoft.Bot.Builder.FormFlow;

namespace Bot_Coke_Recognition.Helpers
{
    [Serializable]
    public class NewImageQuery
    {
        [Prompt("Would you like to add this image to the library? {&}")]
        public string YesNo { get; set; }

        [Prompt("OK, tell me what kind of {&} this is ")]
        public string beverage { get; set; }
    }
}