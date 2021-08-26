using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyParfait
{
    public static class DialogHelper
    {
        public static async Task<bool> ShowYesNo(this IDialogCoordinator dialog, object context, string message)
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
            };
            var result = await dialog.ShowMessageAsync(
                context,
                "JellyParfait",
                message,
                MessageDialogStyle.AffirmativeAndNegative,
                settings
            );
            return result == MessageDialogResult.Affirmative;
        }
    }
}
