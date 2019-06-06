using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace S2T4Bank
{
    public class Localization
    {
        private string _Locale = CultureInfo.DefaultThreadCurrentUICulture.ToString();
        public string Locale
        {
            get => _Locale;
            set => SetLanguage(value);
        }

        private void SetLanguage(string locale)
        {           
            if ((_Locale != locale) && (!String.IsNullOrEmpty(locale)))
            {
                _Locale = locale;

            }
            CultureInfo.CurrentUICulture = new CultureInfo(_Locale);
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
        }
    }
}
