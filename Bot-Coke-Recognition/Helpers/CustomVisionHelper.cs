using System;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Bot_Coke_Recognition.Helpers
{
    [Serializable]
    public class CustomVisionHelper
    {
        static Guid defaultIterationId = new Guid("58de3af5-6330-433d-a230-8a537435372f");
        static Guid projectId = new Guid("ef2c19e0-a82d-4366-a9cb-6f5e7360ccfc");
        public static PredictionResults PredictImage(Stream input)
        {
            PredictionResults theseResults = new PredictionResults();
            using (input)
            {
                try
                {
                    PredictionEndpointCredentials credentials = new PredictionEndpointCredentials("ba8fbc38188a44f5a3ceee53ff439817");
                    PredictionEndpoint cli = new PredictionEndpoint(credentials);
                    var result = cli.PredictImage(projectId, input);
                    foreach (var p in result.Predictions)
                    {
                        var probability = p.Probability;
                        if (probability > 1)
                            probability /= 100;
                        if (probability > 0.92)
                        {
                            theseResults.Positives.Add(p.Tag);
                        }
                        else if (probability > 0.5)
                        {
                            theseResults.Maybes.Add(p.Tag);
                        }else
                        {
                            theseResults.Negatives.Add(p.Tag);
                        }
                    }
                    return theseResults;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                    throw;
                }
            }
        }

        public static bool addImage(Stream image, List<string> tags)
        {
            try
            {
                TrainingApiCredentials trainingCredentials = new TrainingApiCredentials("1f790af625894f61be692809f81ff828");
                TrainingApi cli = new TrainingApi(trainingCredentials);
                cli.CreateImagesFromData(projectId, image, new List<string>());
                cli.TrainProject(projectId);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                return false;
            }
        }

        public static bool retrainProject()
        {
            return true;
        }

    }
}