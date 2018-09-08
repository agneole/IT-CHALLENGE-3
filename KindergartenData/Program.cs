using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace KindergartenData
{
    public class Program
    {
        private static readonly string _path = @"https://raw.githubusercontent.com/vilnius/darzeliai/master/data/Darzeliu%20galimu%20priimti%20ir%20lankantys%20vaikai2018.csv";
        private static readonly string _fileName = @"test.csv";
        private static readonly char _separator = ';';

        public static void Main(string[] args)
        {
            DownloadFile(_path, _fileName);

            var kindergartens = GetKindergartens(_fileName);

            //IŠVESTI Į EKRANĄ DIDŽIAUSIĄ IR MAŽIAUSIĄ STULPELIO „CHILDS_COUNT“ REIKŠMĘ:
            var maxChildsCount = kindergartens.Max(t => t.ChildsCount);
            var minChildsCount = kindergartens.Min(t => t.ChildsCount);
            Console.WriteLine($"DIDŽIAUSIA - {maxChildsCount} IR MAŽIAUSIA - {minChildsCount} STULPELIO \"CHILDS_COUNT\" REIKŠMĖ");

            //SURASTI REIKALINGAS EILUTES IR SUFORMUOTI ŽODĮ (pavyzdžiui:AIT_1,5-3_LIET):
            var data = GetFormatedData(kindergartens, maxChildsCount);
            data.AddRange(GetFormatedData(kindergartens, minChildsCount));
            WriteDataToFile(data, "task1.txt");

            //SURASKITE, KURIOS KALBOS DARŽELIAI TURI DAUGIAUSIAI LAISVŲ VIETŲ PROCENTAIS:
            var percent = GetPercentOfFreeSpacesGroupedByLanguage(kindergartens);
            WriteDataToFile(percent, "task2.txt");

            //IŠRINKTI VISUS DARŽELIUS, KURIUOSE YRA NUO 2 IKI 4 LAISVŲ VIETŲ. SUGRUPUOTI GAUTUS DARŽELIUS PAGAL PAVADINIMĄ IR IŠRŪŠIUOTI NUO Z IKI A.
            var answer = GetKindergartensWhereFreeSpaceIsBetween2And4(kindergartens);
            WriteDataToFile(answer, "task3.txt");

            Console.ReadLine();
        }

        private static void DownloadFile(string path, string fileName)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(path, fileName);
            }
        }

        private static List<KindergartenDto> GetKindergartens(string fileName)
        {
            var kindergartens = new List<KindergartenDto>();
            var lines = File.ReadAllLines(fileName);

            for (int i = 1; i < lines.Length; i++)
            {
                var entries = lines[i].Split(_separator);

                var newKindergarten = new KindergartenDto()
                {
                    DarzId = int.Parse(entries[0]),
                    SchoolName = entries[1].Trim('"'),
                    TypeId = int.Parse(entries[2]),
                    TypeLabel = entries[3].Trim('"'),
                    LanId = int.Parse(entries[4]),
                    LineLabel = entries[5],
                    ChildsCount = int.Parse(entries[6]),
                    FreeSpace = int.Parse(entries[7])
                };

                kindergartens.Add(newKindergarten);
            }

            return kindergartens;
        }

        private static List<string> GetFormatedData(List<KindergartenDto> kindergartens, int childsCount)
        {
            var data = new List<string>();

            foreach (var kindergarten in kindergartens.Where(x => x.ChildsCount == childsCount))
            {
                var schoolName = kindergarten.SchoolName.ToUpper();
                var schoolLanguage = kindergarten.LineLabel.ToUpper();
                var preffix = new String(schoolName.Where(Char.IsLetter).ToArray()).Substring(0, 3);
                var splitedString = kindergarten.TypeLabel.Split(' ');
                string number = GetItemsWithNumber(splitedString);
                var suffix = new String(schoolLanguage.Where(Char.IsLetter).ToArray()).Substring(0, 4);
                var word = $"{preffix}_{number}_{suffix}";
                data.Add(word);
            }

            return data;
        }

        private static string GetPercentOfFreeSpacesGroupedByLanguage(List<KindergartenDto> kindergartens)
        {
            var groupedList = kindergartens
             .GroupBy(u => u.LineLabel)
             .Select(grp => grp.ToList()).ToList(); //TODO get rid of this line

            var languages = new List<string>();
            var percents = new List<double>();

            foreach (var group in groupedList)
            {
                var childsCount = group.Sum(x => x.ChildsCount);
                var freeSpace = group.Sum(x => x.FreeSpace);
                percents.Add(Math.Round((double)(freeSpace * 100) / (childsCount + freeSpace), 2));
                languages.Add(group.First().LineLabel);
            }

            var maxPercent = percents.Max();
            var lang = languages[percents.IndexOf(maxPercent)];
            return $"{lang} {maxPercent}%";
        }

        private static List<string> GetKindergartensWhereFreeSpaceIsBetween2And4(List<KindergartenDto> kindergartens)
        {
            var groupedList = kindergartens
                 .GroupBy(u => u.SchoolName)
                 .Select(grp => grp.ToList()).ToList(); //TODO get rid of this line

            var names = new List<string>();

            foreach (var group in groupedList)
            {
                var sum = group.Sum(x => x.FreeSpace);
                if (sum >= 2 && sum <= 4)
                {
                    names.Add(group.First().SchoolName);
                }
            }

            names = names.OrderByDescending(x => x).ToList();
            return names;
        }

        private static void WriteDataToFile(string data, string textFileName)
        {
            WriteDataToFile(new List<string> { data }, textFileName);
        }

        private static void WriteDataToFile(List<string> data, string textFileName)
        {
            using (StreamWriter writer = new StreamWriter(textFileName))
            {
                foreach (var item in data)
                {
                    writer.WriteLine(item);
                }
            }
        }

        private static string GetItemsWithNumber(string[] array)
        {
            var numbersList = new List<string>();

            foreach (var item in array)
            {
                if (item.Any(char.IsDigit))
                {
                    numbersList.Add(item);
                }
            }

            return string.Join("-", numbersList);
        }

    }
}
