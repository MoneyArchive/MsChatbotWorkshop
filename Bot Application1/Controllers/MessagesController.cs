using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    using System;
    using System.Linq;

    using Autofac;

    using Microsoft.Bot.Builder.Dialogs.Internals;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                var reply = HandleSystemMessage(activity);
                if (reply != null)
                {
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                IConversationUpdateActivity conversationupdate = message;
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    ConnectorClient client = new ConnectorClient(new Uri(message.ServiceUrl));
                    if (conversationupdate.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in conversationupdate.MembersAdded.Where(x=>x.Name!="Bot"))
                        {
                            if (newMember.Id != message.Recipient.Id)
                            {
                                reply.Text = $"Welcome {newMember.Name}! ";
                            }
                            else
                            {
                                reply.Text = $"Welcome {message.From.Name}";
                            }
                            return reply;
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}