namespace Bot_Application5
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    using Newtonsoft.Json;

    [Serializable]
    public class UbikeDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var openJson = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/YouBikeTP.json"));
            List<Models.UbikeStation> stations = JsonConvert.DeserializeObject<List<Models.UbikeStation>>(openJson);

            var data = stations.FirstOrDefault(x => x.sna.Contains(activity.Text));
            if (data == null)
            {
                await context.PostAsync($"搜尋不到相關ubike站點資訊");
            }
            else
            {
                await context.PostAsync(JsonConvert.SerializeObject(data));
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}