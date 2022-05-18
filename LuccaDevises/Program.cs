using LuccaDevises.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LuccaDevises
{
    class Program
    {
        public static List<string> lstRes { get; set; }
        public static List<string> lstPrevious { get; set; }
        public static List<string> lstNext { get; set; }
        public static List<CurrencyRate> listCurrencyRate { get; set; }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {

                var fileExists = File.Exists(args[0]);

                if (fileExists)
                {
                    var file = File.ReadAllText(args[0]);
                    Console.WriteLine(file.ToString());
                    try
                    {
                        ManageFile(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception", ex);
                        throw;
                    }
              
                }
                else
                {
                    Console.WriteLine("Aucun fichier trouvé pour le chemin saisi.");
                }
            }
            else
            {
                Console.WriteLine("Aucun argument n'a été saisi.");
            }
        }


        private static string ManageFile(string file)
        {
            var counter = 0;
            var lines = file.Split(Environment.NewLine);
            var from = "";
            var montant = 0.00;
            var to = "";

            lstRes = new List<string>();
            lstPrevious = new List<string>();
            lstNext = new List<string>();
            listCurrencyRate = new List<CurrencyRate>();


            foreach (string line in lines)
            {
                switch (counter)
                {
                    case 0:
                        var subs = line.Split(';');
                        from = subs[0];
                        montant = int.Parse(subs[1]);
                        to = subs[2];
                        break;
                    case 1:
                        break;
                    default :
                        var subsCurr = line.Split(';');
                        var currencyRate = new CurrencyRate()
                        {
                            From = subsCurr[0],
                            To = subsCurr[1],
                            Rate = double.Parse(subsCurr[2].Replace('.', ','))
                        };

                        listCurrencyRate.Add(currencyRate);
                        break;

                }
                counter++;
            }

            GetCurrency(to, from);

            var listResultReversed = lstRes;
            // puisque nous sommes parti de la devise voulue, nous remettons la liste dans le bonne ordre (départ vers arrivée)
            listResultReversed.Reverse();
            var finalResult = montant;

            var i = 0;
            var j = 1;

            // nous divisons par 2 la liste car les conversion de devises vont par 2 (ex: EUR vers CHF)
            for (var cpt = 0; cpt < listResultReversed.Count / 2; cpt++)
            {
                finalResult = CalculerRate(listResultReversed[i], listResultReversed[j], finalResult);

                i = i + 2;
                j = j + 2;
            }
            Console.WriteLine(Math.Round(finalResult, MidpointRounding.AwayFromZero));
            Console.ReadKey();
            return "ok";
        }

        // Dans cette méthode nous partons de la devise voulue (to) afin d'essayer de récupérer dans la liste un taux de change qui aurait notre devise voulu.
        private static void GetCurrency(string to, string fromCurrency)
        {
            var previousCurrency = "";

            foreach (var element in listCurrencyRate)
            {
                if (element.To == to)
                {
                    // Si nous arrivons à récupérer une devise alors nous vérifions que la liste des dernières devises ne contient pas déjà le from de notre objet CurrencyRate
                    // nous avons déjà ainsi une paire de devise avec la précédente devise (previousCurrency) qui devient ainsi la devise de départ de la paire.
                    if (!lstPrevious.Contains(element.From))
                    {
                        previousCurrency = element.From;
                        lstPrevious.Add(element.From);
                    }

                    if (!lstNext.Contains(to))
                    {
                        lstNext.Add(to);
                    }
                }
            }

            if (string.IsNullOrEmpty(previousCurrency))
            {
                foreach (var element in listCurrencyRate)
                {
                    // puisque nous avons plus haut récupéré la devise de départ de la paire, nous essayons de récupérer ici la devise d'arrivée parmi la liste de données entrée.
                    // previousCurrency prend ainsi la valeur d'arrivée de la paire de devise et on l'ajoute dans la liste des devises d'arrivées (lstNext)
                    if (element.From == to)
                    {
                        if (!lstNext.Contains(element.To))
                        {
                            previousCurrency = element.To;
                            lstNext.Add(previousCurrency);
                        }
                    }
                }
            }

            // nous ajoutons dans la liste des résultats la devise d'arrivée et la devise précédente qui va avec par rapport au jeu de données
            lstRes.Add(to);
            lstRes.Add(previousCurrency);

            if (previousCurrency != fromCurrency)
            {
                GetCurrency(previousCurrency, fromCurrency);
            }
        }

        private static double CalculerRate(string currency1, string currency2, double montant)
        {
            var result = 0.00;

            foreach (var element in listCurrencyRate)
            {
                if (element.From == currency1 && element.To == currency2)
                {
                    result = montant * element.Rate;
                }
                // ici nous gérons le cas où le taux serait inversé.
                if (element.To == currency1 && element.From == currency2)
                {
                    result = montant * Math.Round((1 / element.Rate),4);
                }
            }

            return Math.Round(result,4);
        }

    }
}
