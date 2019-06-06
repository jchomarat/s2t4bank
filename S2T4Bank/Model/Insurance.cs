using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace S2T4Bank.Model
{
    public struct Insurance
    {
        public enum Type
        {
            none,
            di,
            itt,
            ipt,
            gpe
        }

        public static string[] FriendlyName =
        {
            "aucune assurance",
            "décès et invalidité",
            "incapacité temporaire totale de travail",
            "invalidité permanente totale",
            "garantie perte d'emploi"
        };

        public static string GetFriendlyName(string id)
        {
            if (Enum.IsDefined(typeof(Type), id))
            {
                return FriendlyName[(int)Enum.Parse(typeof(Type), id)];
            }
            return "";
        }
    }
}
