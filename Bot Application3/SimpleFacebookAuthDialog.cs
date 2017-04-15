using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.SimpleFacebookAuthBot
{
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// This Dialog implements the OAuth login flow for Facebook. 
    /// You can read more about Facebook's login flow here: https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow
    /// </summary>
    [Serializable]
    public class SimpleFacebookAuthDialog : IDialog<string>
    {

        /// <summary>
        /// OAuth callback registered for Facebook app.
        /// <see cref="Controllers.OAuthCallbackController"/> implementats the callback.
        /// </summary>
        /// <remarks>
        /// Make sure to replace this with the appropriate website url registered for your Facebook app.
        /// </remarks>
        public static readonly Uri FacebookOauthCallback = new Uri(HttpContext.Current.Request.Url, "/api/OAuthCallback");

        /// <summary>
        /// The key that is used to keep the AccessToken in <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PrivateConversationData"/>
        /// </summary>
        public static readonly string AuthTokenKey = "AuthToken";

        /// <summary>
        /// The pending message that is written to the <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PrivateConversationData"/>
        /// when bot is waiting for the response from the callback
        /// </summary>
        public readonly ResumptionCookie ResumptionCookie;

        /// <summary>
        /// Constructs an instance of the SimpleFacebookAuthDialog
        /// </summary>
        /// <param name="msg"></param>
        public SimpleFacebookAuthDialog(IMessageActivity msg)
        {
            ResumptionCookie = new ResumptionCookie(msg);
        }

        /// <summary>
        /// The chain of dialogs that implements the login/logout process for the bot
        /// </summary>
        public static readonly IDialog<string> dialog = Chain
            .PostToChain()
            .Switch(
                new Case<IMessageActivity, IDialog<string>>((msg) =>
                {
                    var regex = new Regex("^login", RegexOptions.IgnoreCase);
                    return regex.IsMatch(msg.Text);
                }, (ctx, msg) =>
                {
                    // User wants to login, send the message to Facebook Auth Dialog
                    return Chain.ContinueWith(new SimpleFacebookAuthDialog(msg),
                                async (context, res) =>
                       {
                           // The Facebook Auth Dialog completed successfully and returend the access token in its results
                           var token = await res;
                           var valid = await FacebookHelpers.ValidateAccessToken(token);
                           var name = await FacebookHelpers.GetFacebookProfileName(token);
                           context.UserData.SetValue("name", name);
                           return Chain.Return($"你登入的名字為: {name}");
                       });
                }),
                new Case<IMessageActivity, IDialog<string>>((msg) =>
                {
                    var regex = new Regex("^logout", RegexOptions.IgnoreCase);
                    return regex.IsMatch(msg.Text);
                }, (ctx, msg) =>
                {
                    // Clearing user related data upon logout
                    ctx.PrivateConversationData.RemoveValue(AuthTokenKey);
                    ctx.UserData.RemoveValue("name");
                    return Chain.Return($"你已被登出!");
                }),
                new DefaultCase<IMessageActivity, IDialog<string>>((ctx, msg) =>
                {
                    string token;
                    string name = string.Empty;
                    if (ctx.PrivateConversationData.TryGetValue(AuthTokenKey, out token) && ctx.UserData.TryGetValue("name", out name))
                    {
                        var validationTask = FacebookHelpers.ValidateAccessToken(token);
                        validationTask.Wait();
                        if (validationTask.IsCompleted && validationTask.Result)
                        {
                            return Chain.Return($"你登入的名字為: {name}");
                        }
                        else
                        {
                            return Chain.Return($"Your Token has expired! Say \"login\" to log you back in!");
                        }
                    }
                    else
                    {
                        return Chain.Return("輸入 \"login\" 來進行臉書登入!");
                    }
                })
            ).Unwrap().PostToUser();


        public async Task StartAsync(IDialogContext context)
        {
            await LogIn(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            if (msg.Text.StartsWith("token:"))
            {
                // Dialog is resumed by the OAuth callback and access token
                // is encoded in the message.Text
                var token = msg.Text.Remove(0, "token:".Length);
                context.PrivateConversationData.SetValue(AuthTokenKey, token);
                context.Done(token);
            }
            else
            {
                await LogIn(context);
            }
        }

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                context.PrivateConversationData.SetValue("persistedCookie", ResumptionCookie);

                // sending the sigin card with Facebook login url
                var reply = context.MakeMessage();
                var fbLoginUrl = FacebookHelpers.GetFacebookLoginURL(ResumptionCookie, FacebookOauthCallback.ToString());
                reply.Text = "請選擇以下登入方式";

                reply.Attachments.Add(SigninCard.Create("使用第三方進行登入",
                                                        "用臉書登入",
                                                        fbLoginUrl
                                                        ).ToAttachment());
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done(token);
            }
        }
    }
}