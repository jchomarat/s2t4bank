using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S2T4Bank.Model
{
    public class Duration
    {
        public static string TimexDurationToFrench(TimexProperty timex)
        {
            if (timex.Years != null)
            {
                if (timex.Years == 1)
                    return $"{timex.Years} an";
                return $"{timex.Years} ans";
            }

            if (timex.Months != null)
            {
                return $"{timex.Months} mois";
            }

            if (timex.Weeks != null)
            {
                if (timex.Weeks == 1)
                    return $"{timex.Weeks} semaine";
                return $"{timex.Weeks} semaines";
            }

            if (timex.Days != null)
            {
                if (timex.Days == 1)
                    return $"{timex.Days} jour";
                return $"{timex.Days} jours";
            }

            if (timex.Hours != null)
            {
                if (timex.Hours == 1)
                    return $"{timex.Hours} heure";
                return $"{timex.Hours} heures";
            }

            if (timex.Minutes != null)
            {
                if (timex.Minutes == 1)
                    return $"{timex.Minutes} minute";
                return $"{timex.Minutes} minutes";
            }

            if (timex.Seconds != null)
            {
                if (timex.Seconds == 1)
                    return $"{timex.Seconds} seconde";
                return $"{timex.Seconds} secondes";
            }

            return string.Empty;
        }
    }
}
