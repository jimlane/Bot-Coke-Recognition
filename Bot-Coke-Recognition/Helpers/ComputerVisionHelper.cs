using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Bot_Coke_Recognition.Helpers
{
    public class ComputerVisionHelper
    {
        public static async Task<List<string>> AnalyzeImage(Stream attachment)
        {
            AnalysisResult analysisResult;
            var features = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Description };
            var visionClient = new VisionServiceClient("ebbecef60400431492feb902f22a28fe", "https://eastus2.api.cognitive.microsoft.com/vision/v1.0");
            List<string> imageTags = new List<string>();

            try
            {
                analysisResult = await visionClient.AnalyzeImageAsync(attachment, features);
                foreach (var item in analysisResult.Tags)
                {
                    imageTags.Add(item.Name.ToString());
                }
                foreach (var item in analysisResult.Description.Tags)
                {
                    imageTags.Add(item.ToString());
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                throw;
            }
            return imageTags;
        }
    }
}