namespace Bot_Application5
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml.Linq;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Location;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    using Newtonsoft.Json;

    [LuisModel("340fed8f-e53f-4442-82e8-d4a92d78e0f2", "88c8e901f4374305a5f664f9a3ce4cb4")]
    [Serializable]
    public class UbikeLuisDialog : LuisDialog<object>
    {
        private string entity;

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string msg = string.Empty;
            Random random = new Random(DateTime.Now.Millisecond);
            var tmp = random.Next() % 4;
            switch (tmp)
            {
                case 0:
                    msg = "抱歉，我沒聽懂你說的話";
                    break;
                case 1:
                    msg = "實在聽不懂你在說甚麼阿...";
                    break;
                case 2:
                    msg = "抱歉，我不知道你想要告訴我什麼";
                    break;
                case 3:
                    msg = "我不太懂你想要表達的是什麼";
                    break;
            }

            await context.PostAsync(msg);
            context.Wait(MessageReceived);
        }

        [LuisIntent("髒話")]
        public async Task Dirty(IDialogContext context, LuisResult result)
        {
            string msg = string.Empty;
            Random random = new Random(DateTime.Now.Millisecond);
            var tmp = random.Next() % 4;
            switch (tmp)
            {
                case 0:
                    msg = "唉，任何人工智慧都敵不過閣下您呀";
                    break;
                case 1:
                    msg = "我的老天鵝阿，您在自我介紹嗎";
                    break;
                case 2:
                    msg = "不好意思，讓您賤笑了";
                    break;
                case 3:
                    msg = "不要說你自己呀，我都不忍目睹了";
                    break;
            }

            await context.PostAsync(msg);
            context.Wait(MessageReceived);
        }

        [LuisIntent("問候")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            string msg = string.Empty;
            Random random = new Random(DateTime.Now.Millisecond);
            var tmp = random.Next() % 3;
            switch (tmp)
            {
                case 0:
                    msg = "Whats' up";
                    break;
                case 1:
                    msg = "很高興見到你";
                    break;
                case 2:
                    msg = "您好";
                    break;
            }

            await context.PostAsync(msg);
            context.Wait(MessageReceived);
        }

        [LuisIntent("詢問")]
        public async Task Where(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            var apiKey = "As5j3FKfU0PQ1mGpartrpxN_qwggIehOx1oj-mI7_3hv17WpaQIAwK6jPWo5AG5q";
            var prompt = "告訴我你在哪裡";
            var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

            var locationDialog = new LocationDialog(apiKey, message.ChannelId, prompt, options);
            context.Call(locationDialog, ResumeAfterLocationDialogAsync);
        }

        public async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            var lat = place.Geo.Latitude;
            var lng = place.Geo.Longitude;

            string imgUrl =
                $"http://maps.google.com/maps/api/staticmap?center={lat},{lng}&zoom=16&size=400x400&sensor=false&format=png32&maptype=roadmap&markers={lat},{lng}";
            IMessageActivity activity = context.MakeMessage();
            activity.Attachments = new List<Attachment>();
            ThumbnailCard plCard2 = new ThumbnailCard()
            {
                Title = "您目前所在位置",
                Images = new List<CardImage>() { new CardImage(url: imgUrl) },
            };
            activity.Attachments.Add(plCard2.ToAttachment());
            await context.PostAsync(activity);
            await context.PostAsync("正在搜尋離您最近的ubike站點");

            // send navi
            #region find near station

            var openJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/YouBikeTP.json"));
            List<Models.UbikeStation> stations = JsonConvert.DeserializeObject<List<Models.UbikeStation>>(openJson);
            var coord = new GeoCoordinate(lat, lng);
            var nearest = stations.Select(x => new GeoCoordinate(double.Parse(x.lat), double.Parse(x.lng), int.Parse(x.sno)))
                                   .OrderBy(x => x.GetDistanceTo(coord))
                                   .First();

            var station = stations.First(x => x.sno == nearest.Altitude.ToString().PadLeft(4, '0'));

            #endregion

            var destImg = $"http://maps.google.com/maps/api/staticmap?zoom=16&size=600x400&sensor=false&path={lat},{lng}|{station.lat},{station.lng}&markers={lat},{lng}|{station.lat},{station.lng}";
            // var destImg = "http://maps.google.com/maps/api/staticmap?center={lat},{lng}&zoom=16&size=400x400&sensor=false&format=png32&maptype=roadmap&markers={lat},{lng}";
            IMessageActivity replyNavi = context.MakeMessage();
            replyNavi.Type = "message";
            replyNavi.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: destImg));
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"http://maps.google.com/maps?daddr={station.lat},{station.lng}&amp;ll=",
                Type = "openUrl",
                Title = "前往"
            };
            cardButtons.Add(plButton);

            ThumbnailCard plCard = new ThumbnailCard()
            {
                Title = $"最近站點為：{station.sna}",
                Subtitle = "是否要前往導航?",
                Images = cardImages,
                Buttons = cardButtons
            };

            replyNavi.Attachments.Add(plCard.ToAttachment());

            await
                context.PostAsync(replyNavi);
            context.Wait(MessageReceived);
        }

        [LuisIntent("前往")]
        public async Task Goto(IDialogContext context, LuisResult result)
        {
            var tmp = result.Entities.FirstOrDefault()?.Entity;
            await context.PostAsync($"你想要前往...{tmp}");

            var requestUri =
                $"http://maps.googleapis.com/maps/api/geocode/xml?address={Uri.EscapeDataString(tmp)}&sensor=false";

            var request = WebRequest.Create(requestUri);
            var response = request.GetResponse();
            var xdoc = XDocument.Load(response.GetResponseStream());

            var result2 = xdoc.Element("GeocodeResponse").Element("result");
            var locationElement = result2.Element("geometry").Element("location");
            var lat = double.Parse(locationElement.Element("lat").Value);
            var lng = double.Parse(locationElement.Element("lng").Value);

            string imgUrl =
                $"http://maps.google.com/maps/api/staticmap?center={lat},{lng}&zoom=16&size=400x400&sensor=false&format=png32&maptype=roadmap&markers={lat},{lng}";

            IMessageActivity replyNavi = context.MakeMessage();
            replyNavi.Type = "message";
            replyNavi.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: imgUrl));

            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"http://maps.google.com/maps?daddr={lat},{lng}&amp;ll=",
                Type = "openUrl",
                Title = "前往"
            };
            cardButtons.Add(plButton);

            ThumbnailCard plCard = new ThumbnailCard()
            {
                Title = tmp,
                Subtitle = "是否要前往導航?",
                Images = cardImages,
                Buttons = cardButtons
            };
            replyNavi.Attachments.Add(plCard.ToAttachment());
            await
                context.PostAsync(replyNavi);
            context.Wait(MessageReceived);
        }
    }
}