using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneticsProject
{
    public struct GeneticData
    {
        public string protein; // название белка
        public string organism; // название организма
        public string amino_acids; // цепочка аминокислот 
    }

    class Program
    {
        static List<GeneticData> data = new List<GeneticData>();

        // Метод для чтения данных о белках из файла
        static void ReadGeneticData(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] fragments = line.Split('\t');
                    GeneticData protein;
                    protein.protein = fragments[0];
                    protein.organism = fragments[1];
                    protein.amino_acids = fragments[2];
                    data.Add(protein);
                }
            }
        }

        // Метод для выполнения команд
        static void ReadHandleCommands(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                int counter = 0;
                using (StreamWriter writer = new StreamWriter("genedata.txt"))
                {
                    writer.WriteLine("Your Name"); // Замените на ваше имя
                    writer.WriteLine("Генетический поиск");

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        counter++;
                        string[] command = line.Split('\t');

                        // Обработка команды search
                        if (command[0].Equals("search"))
                        {
                            writer.WriteLine($"{counter:D3}   search   {command[1]}");
                            int index = Search(command[1]);
                            if (index != -1)
                                writer.WriteLine($"{data[index].organism}    {data[index].protein}");
                            else
                                writer.WriteLine("NOT FOUND");
                            writer.WriteLine("================================================");
                        }

                        // Обработка команды diff
                        if (command[0].Equals("diff"))
                        {
                            writer.WriteLine($"{counter:D3}   diff   {command[1]}   {command[2]}");
                            int diffCount = Diff(command[1], command[2]);
                            writer.WriteLine($"amino-acids difference: {diffCount}");
                            writer.WriteLine("================================================");
                        }

                        // Обработка команды mode
                        if (command[0].Equals("mode"))
                        {
                            writer.WriteLine($"{counter:D3}   mode   {command[1]}");
                            string mostFrequent = Mode(command[1]);
                            writer.WriteLine(mostFrequent);
                            writer.WriteLine("================================================");
                        }
                    }
                }
            }
        }

        // Метод для поиска белка по последовательности аминокислот
        static int Search(string amino_acid)
        {
            string decoded = RLDecoding(amino_acid);
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].amino_acids.Contains(decoded))
                    return i;
            }
            return -1;
        }

        // Метод для вычисления различий между двумя белками
        static int Diff(string protein1Name, string protein2Name)
        {
            var protein1 = data.FirstOrDefault(p => p.protein.Equals(protein1Name));
            var protein2 = data.FirstOrDefault(p => p.protein.Equals(protein2Name));

            if (string.IsNullOrEmpty(protein1.protein) || string.IsNullOrEmpty(protein2.protein))
            {
                var missing = new List<string>();
                if (string.IsNullOrEmpty(protein1.protein)) missing.Add(protein1Name);
                if (string.IsNullOrEmpty(protein2.protein)) missing.Add(protein2Name);
                return -1; // Если один из белков не найден, вернуть -1
            }

            int diffCount = 0;
            string amino1 = protein1.amino_acids;
            string amino2 = protein2.amino_acids;

            int length = Math.Max(amino1.Length, amino2.Length);
            for (int i = 0; i < length; i++)
            {
                if (i >= amino1.Length || i >= amino2.Length || amino1[i] != amino2[i])
                {
                    diffCount++;
                }
            }
            return diffCount;
        }

        // Метод для определения наиболее часто встречающейся аминокислоты
        static string Mode(string proteinName)
        {
            var protein = data.FirstOrDefault(p => p.protein.Equals(proteinName));

            if (string.IsNullOrEmpty(protein.protein))
            {
                return $"MISSING: {proteinName}";
            }

            var aminoCounts = new Dictionary<char, int>();
            foreach (var amino in protein.amino_acids)
            {
                if (aminoCounts.ContainsKey(amino))
                {
                    aminoCounts[amino]++;
                }
                else
                {
                    aminoCounts[amino] = 1;
                }
            }

            var mostFrequent = aminoCounts.OrderByDescending(pair => pair.Value)
                                           .ThenBy(pair => pair.Key)
                                           .First();

            return $"amino-acid occurs: {mostFrequent.Key} {mostFrequent.Value}";
        }

        // Метод для кодирования аминокислот с использованием RLE
        static string RLEncoding(string amino_acids)
        {
            StringBuilder encoded = new StringBuilder();
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                int count = 1;
                while (i < amino_acids.Length - 1 && amino_acids[i + 1] == ch)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded.Append(count).Append(ch);
                else if (count == 1) encoded.Append(ch);
                else if (count == 2) encoded.Append(ch).Append(ch);
            }
            return encoded.ToString();
        }

        // Метод для декодирования аминокислот с использованием RLE
        static string RLDecoding(string amino_acids)
        {
            StringBuilder decoded = new StringBuilder();
            for (int i = 0; i < amino_acids.Length; i++)
            {
                if (char.IsDigit(amino_acids[i]))
                {
                    char letter = amino_acids[i + 1];
                    int count = amino_acids[i] - '0';
                    decoded.Append(new string(letter, count)); // Добавить символ count раз
                    i++; // Пропустить символ
                }
                else
                {
                    decoded.Append(amino_acids[i]);
                }
            }
            return decoded.ToString();
        }

        // Точка входа в программу
        static void Main(string[] args)
        {
            ReadGeneticData("sequences.txt");
            ReadHandleCommands("commands.txt");
        }
    }
}
