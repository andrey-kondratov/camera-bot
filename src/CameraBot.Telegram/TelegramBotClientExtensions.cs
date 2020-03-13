using System.Threading;
using System.Threading.Tasks;
using CameraBot.Media;
using CameraBot.Messaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace CameraBot.Telegram
{
    internal static class TelegramBotClientExtensions
    {
        public static Task<Message> GreetSnapshot(this TelegramBotClient client, Message message, Node root,
            CancellationToken cancellationToken = default)
        {
            string text = MessageHelpers.SnapshotGreetingMarkdown;
            IReplyMarkup replyMarkup = MessageHelpers.GetReplyMarkup(root);

            return client.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Markdown,
                replyMarkup: replyMarkup, cancellationToken: cancellationToken);
        }

        public static Task<Message> PromptFeedback(this TelegramBotClient client, Message message,
            CancellationToken cancellationToken = default)
        {
            string text = MessageHelpers.FeedbackPromptMarkdown;

            return client.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> Navigate(this TelegramBotClient client, Node node, ISnapshotRequest request,
            string alert = null, CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;
            long chatId = query.Message.Chat.Id;
            int requestMessageId = query.Message.MessageId;

            // show alert or silently acknowledge
            bool showAlert = !string.IsNullOrWhiteSpace(alert);
            string text = showAlert ? alert : null;
            await client.AnswerCallbackQueryAsync(query.Id, text: text, showAlert: showAlert,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // update navigation controls in the current message
            InlineKeyboardMarkup replyMarkup = MessageHelpers.GetReplyMarkup(node);
            return await client.EditMessageReplyMarkupAsync(chatId, requestMessageId, replyMarkup, cancellationToken)
                .ConfigureAwait(false);
        }

        public static Task<Message> SendSnapshotFailure(this TelegramBotClient client, Snapshot snapshot,
            ISnapshotRequest request, Node nodeToNavigate, CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;

            // send a new text message 
            string markdown = MessageHelpers.GetFailureMarkdown(snapshot);
            InlineKeyboardMarkup replyMarkup = MessageHelpers.GetReplyMarkup(nodeToNavigate);

            return client.SendTextMessageAsync(query.Message.Chat.Id, markdown, ParseMode.Markdown,
                replyMarkup: replyMarkup, cancellationToken: cancellationToken);
        }

        public static Task<Message> SendSnapshotSuccess(this TelegramBotClient client, Snapshot snapshot,
            ISnapshotRequest request, Node nodeToNavigate, TelegramOptions options, CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;

            // post the snapshot and navigation controls in a new message
            var photo = new InputOnlineFile(snapshot.Stream);
            string caption = MessageHelpers.GetCaptionMarkdown(snapshot, options.HoursOffset, options.DateTimeFormat);
            IReplyMarkup replyMarkup = MessageHelpers.GetReplyMarkup(snapshot.Node.Parent);

            return client.SendPhotoAsync(query.Message.Chat.Id, photo, caption, ParseMode.Markdown,
                replyMarkup: replyMarkup, cancellationToken: cancellationToken);
        }

        public static Task ClearLastKeyboard(this TelegramBotClient client, ISnapshotRequest request,
            CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;

            return client.ClearLastKeyboard(query, cancellationToken);
        }

        public static async Task ClearLastKeyboard(this TelegramBotClient client, CallbackQuery query,
            CancellationToken cancellationToken = default)
        {
            // quietly answer the query
            await client.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await client.ClearLastKeyboard(query.Message, cancellationToken).ConfigureAwait(false);
        }

        public static Task ClearLastKeyboard(this TelegramBotClient client, Message message,
            CancellationToken cancellationToken = default)
        {
            long chatId = message.Chat.Id;
            int requestMessageId = message.MessageId;

            // remove navigation controls from the current message
            return client.EditMessageReplyMarkupAsync(chatId, requestMessageId, InlineKeyboardMarkup.Empty(),
                cancellationToken);
        }

        public static Task<Message> SendBadRequest(this TelegramBotClient client, Message message,
            CancellationToken cancellationToken = default)
        {
            string text = MessageHelpers.BadRequestMessage;

            return client.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove(),
                replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
        }
    }
}