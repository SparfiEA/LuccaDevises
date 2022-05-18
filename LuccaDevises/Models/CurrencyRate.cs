using System;
using System.Collections.Generic;
using System.Text;

namespace LuccaDevises.Models
{
    public class CurrencyRate
    {
        // From : devise de départ
        public string From { get; set; }
        // To : devise d'arrivée
        public string To { get; set; }
        // Rate : taux de conversion
        public double Rate { get; set; }

    }
}
