using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraBot.Media;
using MoreLinq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CameraBot.Telegram
{
    internal static class MessageHelpers
    {
        public static InlineKeyboardMarkup GetReplyMarkup(Node node, int keyboardWidth = 3)
        {
            IEnumerable<IEnumerable<InlineKeyboardButton>> GetNavigationRows(Node node)
            {
                foreach (IEnumerable<Node> row in node.Children.Batch(keyboardWidth))
                {
                    yield return row.Select(node => InlineKeyboardButton.WithCallbackData(node.Name, node.Id));
                }

                if (node.Parent != null)
                {
                    yield return new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Back", node.Parent.Id)
                    };
                }
            }

            IEnumerable<IEnumerable<InlineKeyboardButton>> keyboard = GetNavigationRows(node);
            var markup = new InlineKeyboardMarkup(keyboard);

            return markup;
        }

        internal static string FeedbackPromptMarkdown => "To leave your feedback, reply to this message";

        internal static string SnapshotGreetingMarkdown => "Select a camera";

        public static string GetCaptionMarkdown(Snapshot snapshot, int hoursOffset, string dateTimeFormat)
        {
            var builder = new StringBuilder();

            DateTime taken = snapshot.TakenUtc.AddHours(hoursOffset);

            if (!snapshot.Node.IsRootChild())
            {
                builder.AppendFormat("*{0}.* ", snapshot.Node.Parent.Name);
            }

            builder.AppendFormat("*{0}.* ", snapshot.Node.Name);
            builder.AppendFormat($"{{0:{dateTimeFormat}}}", taken);

            if (!string.IsNullOrWhiteSpace(snapshot.Node.Url))
            {
                builder.AppendFormat("\n[Watch live]({0})", snapshot.Node.Url);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.Node.Website))
            {
                builder.AppendFormat("\n[{0}]({0})", snapshot.Node.Website);
            }

            return builder.ToString();
        }

        public static string GetFailureMarkdown(Snapshot snapshot)
        {
            var result = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(snapshot.Node.SnapshotUrl))
            {
                result.Append("I failed to get the snapshot you wanted. Try again. ");
            }
            else
            {
                result.Append("I do not know how to get a snapshot from this camera. ");

                if (!string.IsNullOrWhiteSpace(snapshot.Node.Url))
                {
                    result.Append("Use \"Watch live\" to open the live stream.");
                }
                else if (!string.IsNullOrWhiteSpace(snapshot.Node.Website))
                {
                    result.Append("You may try using the website. ");
                }
            }

            if (!string.IsNullOrWhiteSpace(snapshot.Node.Url))
            {
                result.AppendFormat("\n[Watch live]({0})", snapshot.Node.Url);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.Node.Website))
            {
                result.AppendFormat("\n[{0}]({0})", snapshot.Node.Website);
            }

            return result.ToString();
        }

        internal static string GetFeedbackMarkdown(Message message, string label)
        {
            var result = new StringBuilder();

            result.Append("*Feedback received!*").AppendLine().AppendLine();

            if (!string.IsNullOrEmpty(label))
            {
                result.AppendFormat("_label: {0}_", label).AppendLine();
            }

            result.AppendFormat("_from: @{0}_", message.From.Username).AppendLine().AppendLine();
            result.Append(message.Text);

            return result.ToString();
        }

        internal static string BadRequestMessage => "Sorry, I don't know that command.";
    }
}