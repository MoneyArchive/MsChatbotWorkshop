using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Http;

namespace Bot_Application2.Dialogs
{
    using System.Collections.Generic;
    using System.Web;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            switch (activity?.Text)
            {
                case "test":
                    var reply = context.MakeMessage();
                    reply.Type = "message";
                    reply.Attachments = new List<Attachment>();
                    List<CardAction> cardButtons = CreateButtons();

                    var card = new HeroCard() { Title = "請輸入測試選項", Buttons = cardButtons };
                    reply.Attachments.Add(card.ToAttachment());

                    // return our reply to the user
                    await context.PostAsync(reply);
                    break;
                case "1":
                    #region Thumbnail Card 範例

                    var reply1 = context.MakeMessage();
                    reply1.Type = "message";
                    reply1.Attachments = new List<Attachment>();
                    var btn1 = new CardAction()
                    {
                        Type = "openUrl",
                        Title = "前往活動網站",
                        Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/"
                    };
                    Uri imgUrl = new Uri(HttpContext.Current.Request.Url, VirtualPathUtility.ToAbsolute("~/Images/2017AI-APP-Contest.png"));
                    var card1 = new ThumbnailCard()
                    {
                        Title = "AI 智能服務應用大賽",
                        Subtitle = "AI + ChatBot 與 AI + Office 365兩大主題等你來挑戰！",
                        Images = new List<CardImage>() { new CardImage(url: imgUrl.ToString()) },
                        Buttons = new List<CardAction>() { btn1 }
                    };
                    reply1.Attachments.Add(card1.ToAttachment());

                    // return our reply to the user
                    await context.PostAsync(reply1);

                    #endregion
                    break;
                case "2":
                    #region Receipt Card 範例

                    var reply2 = context.MakeMessage();
                    reply2.Type = "message";
                    reply2.Attachments = new List<Attachment>();
                    var card2 = new ReceiptCard()
                    {
                        Facts =
                            new List<Fact>
                                {
                                    // Reference https://developers.facebook.com/docs/messenger-platform/send-api-reference/receipt-template
                                    new Fact(
                                        "Order Number",
                                        DateTime.Now.ToString("yyyyMMddHHmmss")),
                                    new Fact(
                                        "Payment Method",
                                        "VISA 5555-****-****-5555"),
                                    //new Fact(
                                    //    "Currency",
                                    //    "TWD")
                                },
                        Items =
                            new List<ReceiptItem>
                                {
                                    new ReceiptItem(
                                        "頻寬費用",
                                        price: "$ 38.45",
                                        quantity: "368",
                                        image:
                                        new CardImage(
                                            url:
                                            "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                                    new ReceiptItem(
                                        "App Service",
                                        price: "$ 45.00",
                                        quantity: "720",
                                        image:
                                        new CardImage(
                                            url:
                                            "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                                },
                        Tax = "$ 7.50",
                        Total = "$ 90.95",
                        Buttons =
                            new List<CardAction>
                                {
                                    new CardAction(
                                        ActionTypes.OpenUrl,
                                        "更多資訊",
                                        "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                                        "https://azure.microsoft.com/zh-tw/pricing/")
                                }
                    };
                    reply2.Attachments.Add(card2.ToAttachment());

                    // return our reply to the user
                    await context.PostAsync(reply2);

                    #endregion
                    break;
                case "3":
                    #region Carousel Thumbnail Card 範例

                    var reply3 = context.MakeMessage();

                    reply3.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    Uri imgUrl3 = new Uri(HttpContext.Current.Request.Url, VirtualPathUtility.ToAbsolute("~/Images/2017AI-APP-Contest.png"));
                    var btn31 = new CardAction()
                    {
                        Type = "openUrl",
                        Title = "前往活動網站",
                        Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/"
                    };
                    var hero31 = new HeroCard()
                    {
                        Title = "AI 智能服務應用大賽",
                        Subtitle = "AI + ChatBot 與 AI + Office 365兩大主題等你來挑戰！",
                        Text = "目前 Facebook Messenger、WhatsApp 和 LINE 的使用者加起來已超過 27 億人，而手機使用者每天有超過 90% 以上的時間都花在使用聊天通訊軟體及平台，越來越多使用者開始減少手機內的 App 數量，只留下少數黏著度高的應用程式，例如聊天通訊 App。所以當您要打造一個新的商業應用時，應該嘗試連結使用者最多的聊天通訊平台，創造更高的效益！",
                        Images = new List<CardImage>() { new CardImage(url: imgUrl3.ToString()) },
                        Buttons = new List<CardAction>() { btn31 }
                    };

                    var btn32 = new CardAction()
                    {
                        Type = "openUrl",
                        Title = "前往報名",
                        Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/"
                    };
                    var thumbnail32 = new HeroCard()
                    {
                        Title = "填寫報名資料",
                        Subtitle = "AI + ChatBot 與 AI + Office 365兩大主題等你來挑戰！",
                        Images = new List<CardImage>() { new CardImage(url: imgUrl3.ToString()) },
                        Buttons = new List<CardAction>() { btn32 }
                    };

                    reply3.Attachments = new List<Attachment>() { hero31.ToAttachment(), thumbnail32.ToAttachment() };

                    await context.PostAsync(reply3);

                    #endregion
                    break;
                default:
                    await context.PostAsync("輸入'test'來顯示列表！");
                    break;
            }

            context.Wait(MessageReceivedAsync);
        }

        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Azure Storage",
                    "Offload the heavy lifting of data center management",
                    "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
                    new CardImage(url: "https://docs.microsoft.com/en-us/azure/storage/media/storage-introduction/storage-concepts.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
                GetThumbnailCard(
                    "DocumentDB",
                    "Blazing fast, planet-scale NoSQL",
                    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
                    new CardImage(url: "https://docs.microsoft.com/en-us/azure/documentdb/media/documentdb-introduction/json-database-resources1.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
                GetHeroCard(
                    "Azure Functions",
                    "Process events with a serverless code architecture",
                    "An event-based serverless compute experience to accelerate your development. It can scale based on demand and you pay only for the resources you consume.",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-5daae9212bb433ad0510fbfbff44121ac7c759adc284d7a43d60dbbf2358a07a/images/page/services/functions/01-develop.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
                GetThumbnailCard(
                    "Cognitive Services",
                    "Build powerful intelligence into your applications to enable natural and contextual interactions",
                    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-68b530dac63f0ccae8466a2610289af04bdc67ee0bfbc2d5e526b8efd10af05a/images/page/services/cognitive-services/cognitive-services.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/cognitive-services/")),
            };
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private static List<CardAction> CreateButtons()
        {
            // Reference https://docs.botframework.com/en-us/csharp/builder/sdkreference/attachments.html#ButtonandCardActions

            // Create 5 CardAction buttons 
            // and return to the calling method 
            List<CardAction> cardButtons = new List<CardAction>();
            var btn1 = new CardAction()
            {
                Type = "imBack",
                Title = "Thumbnail Card 範例",
                Value = "1"
            };
            cardButtons.Add(btn1);

            var btn2 = new CardAction()
            {
                Type = "imBack",
                Title = "Receipt Card 範例",
                Value = "2"
            };
            cardButtons.Add(btn2);

            var btn3 = new CardAction()
            {
                Type = "imBack",
                Title = "Carousel Thumbnail Card 範例",
                Value = "3"
            };
            cardButtons.Add(btn3);

            var btn4 = new CardAction()
            {
                Type = "imBack",
                Title = "會自動換頁範例",
                Value = "4"
            };
            cardButtons.Add(btn4);

            return cardButtons;
        }
    }
}