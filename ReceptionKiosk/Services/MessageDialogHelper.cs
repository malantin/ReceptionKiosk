using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ReceptionKiosk.Services
{
    public class MessageDialogHelper
    {
        /// <summary>
        /// Helper for MessageDialog confirmation
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <param name="actionYes">Click on Yes</param>
        /// <param name="actionNo">Click on No</param>
        /// <returns></returns>
        public static async Task ConfirmDialogAsync(string title, string message, Func<Task> actionYesAsync, Func<Task> actionNoAsync)
        {
            var dialog = new MessageDialog(message, title);
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            var dialogResult = await dialog.ShowAsync();

            if ((int)dialogResult.Id == 0)
                await actionYesAsync();
            else
                await actionNoAsync();
        }

        /// <summary>
        /// Helper for MessageDialog
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public static async Task MessageDialogAsync(string title, string message)
        {
            await (new MessageDialog(message, title)).ShowAsync();
        }

        /// <summary>
        /// Helper for MessageDialog
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public static async Task MessageDialogAsync(string message)
        {
            await (new MessageDialog(message)).ShowAsync();
        }
    }
}
