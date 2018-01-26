using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Coke_Recognition.Helpers
{
    [Serializable]
    public class PredictionResults
    {
        public PredictionResults ()
        {
            this.Positives = new List<string>();
            this.Maybes = new List<string>();
            this.Negatives = new List<string>();
        }
        public List<string> Positives { get; set; }
        public List<string> Maybes { get; set; }
        public List<string> Negatives { get; set; }

    }
}