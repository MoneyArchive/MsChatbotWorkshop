using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application6.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Chronic;

    using Microsoft.ProjectOxford.Emotion;
    using Microsoft.ProjectOxford.Emotion.Contract;
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Vision;

    using Newtonsoft.Json;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> argument)
        {
            var message = await argument as Activity;
            await context.PostAsync("收到訊息");


            if (message.Attachments?.Count > 0 && message.Attachments.First().ContentType.StartsWith("image"))
            {
                await context.PostAsync("開始辨識，請稍候..");
                var url = message.Attachments.First().ContentUrl;
                var reply = context.MakeMessage();
                // todo change method
                reply.Text = await Emotion(url);
                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync("Hi there! Upload picutre please.");
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task<string> Vision(string url)
        {
            VisionServiceClient client = new VisionServiceClient("809703eae3894ddf9e2e9b9d33b6cab1");
            var result = await client.AnalyzeImageAsync(url, new VisualFeature[] { VisualFeature.Description });
            return result.Description.Captions.First().Text;
        }

        private async Task<string> Face(string url)
        {
            FaceServiceClient client = new FaceServiceClient("e2b7e3cb70614c198312769804e1c0fd");
            var result = await client.DetectAsync(url, true, false, new FaceAttributeType[] { FaceAttributeType.Age, FaceAttributeType.Gender });
            return $"男性: {result.Count(x => x.FaceAttributes.Gender == "male")}, 女性: {result.Count(x => x.FaceAttributes.Gender == "female")}"; ;
        }

        private async Task<string> Emotion(string url)
        {
            EmotionServiceClient client = new EmotionServiceClient("44ce1b4254eb4ec69d50d4cf421c78fb");
            var result = await client.RecognizeAsync(url);
            string msg = "";
            foreach (Emotion emotion in result)
            {
                msg += $"{JsonConvert.SerializeObject(emotion.Scores)}\r\n";
            }
            return msg;
        }
    }
}