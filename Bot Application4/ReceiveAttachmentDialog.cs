namespace ReceiveAttachmentBot
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Configuration;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Location;
    using Microsoft.Bot.Connector;
    using Microsoft.ProjectOxford.Vision;

    using Newtonsoft.Json;

    [Serializable]
    internal class ReceiveAttachmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> argument)
        {
            var message = await argument as Activity;

            if (message.Text == "location")
            {
                await context.PostAsync("開始詢問地點，請稍後..");
                var apiKey = "As5j3FKfU0PQ1mGpartrpxN_qwggIehOx1oj-mI7_3hv17WpaQIAwK6jPWo5AG5q";
                var prompt = "告訴我你在哪裡";
                var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

                var locationDialog = new LocationDialog(apiKey, message.ChannelId, prompt, options);
                context.Call(locationDialog, ResumeAfterLocationDialogAsync);
                return;
            }
            else if (message.Attachments?.Count > 0 && message.Attachments.First().ContentType.StartsWith("image"))
            {
                await context.PostAsync("收到圖片，開始辨識，請稍後..");
                VisionServiceClient client = new VisionServiceClient("809703eae3894ddf9e2e9b9d33b6cab1");
                var url = message.Attachments.First().ContentUrl;
                var result = await client.AnalyzeImageAsync(url, new VisualFeature[] { VisualFeature.Description });
                var reply = context.MakeMessage();
                reply.Text = result.Description.Captions.First().Text;
                await context.PostAsync(reply);
                await context.PostAsync("complete vision");
            }
            else
            {
                await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
            }

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            if (place != null)
            {
                var address = place.GetPostalAddress();
                var formatteAddress = string.Join(", ", new[]
                {
                        address.StreetAddress,
                        address.Locality,
                        address.Region,
                        address.PostalCode,
                        address.Country
                    }.Where(x => !string.IsNullOrEmpty(x)));

                await context.PostAsync($"您的地址為: {formatteAddress}，{place.Geo.Latitude}, {place.Geo.Longitude}");
            }

            context.Done<string>(null);
        }
    }
}