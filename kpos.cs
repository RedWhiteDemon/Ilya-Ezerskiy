using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenPro
{
    public struct GeneData
    {
        public string protein; // название белка
        public string organism; // название организма
        public string amino_acids; // цепочка аминокислот
    }

    class Program
    {
        static List<GeneData> data = new List<GeneData>();

        static void Main(string[] args)
        {
            
            RGeneData("sequences.0.txt");

            
            RHComm("commands.0.txt");
        }

        static void RGeneData(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] fragments = line.Split('\t');
                    if (fragments.Length == 3)
                    {
                        GeneData protein = new GeneData
                        {
                            protein = fragments[0],
                            organism = fragments[1],
                            amino_acids = fragments[2]
                        };
                        data.Add(protein);
                    }
                }
            }
        }

        static void RHComm(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                int operationNumber = 1;
                using (StreamWriter writer = new StreamWriter("genedata.0.txt"))
                {
                    writer.WriteLine("Езерский Илья");
                    writer.WriteLine("Генетический поиск");

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] command = line.Split('\t');
                        string op = command[0].Trim();
                        string result = "";

                        switch (op)
                        {
                            case "search":
                                result = HSearch(command[1]);
                                break;
                            case "diff":
                                result = HDiff(command[1], command[2]);
                                break;
                            case "mode":
                                result = HMode(command[1]);
                                break;
                        }

                        writer.WriteLine($"{operationNumber.ToString("D3")}   {op}   {command.Length > 1 ? command[1] : ""}");
                        writer.WriteLine(result);
                        writer.WriteLine("================================================");

                        operationNumber++;
                    }
                }
            }
        }

        static string HSearch(string amino_acid)
        {
            string decoded = RLD(amino_acid);
            var results = data.Where(d => d.amino_acids.Contains(decoded)).ToList();
            
            if (results.Count == 0)
                return "NOT FOUND";

            return string.Join(Environment.NewLine, results.Select(d => $"{d.organism}    {d.protein}"));
        }

        static string HDiff(string protein1Name, string protein2Name)
        {
            var protein1 = data.FirstOrDefault(d => d.protein == protein1Name);
            var protein2 = data.FirstOrDefault(d => d.protein == protein2Name);

            if (protein1.protein == null || protein2.protein == null)
            {
                var missing = new List<string>();
                if (protein1.protein == null) missing.Add(protein1Name);
                if (protein2.protein == null) missing.Add(protein2Name);

                return $"MISSING: {string.Join(", ", missing)}";
            }

            string seq1 = RLD(protein1.amino_acids);
            string seq2 = RLD(protein2.amino_acids);

            int length = Math.Max(seq1.Length, seq2.Length);
            int diffCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (i >= seq1.Length || i >= seq2.Length || seq1[i] != seq2[i])
                {
                    diffCount++;
                }
            }

            return $"amino-acids difference: {diffCount}";
        }

        static string HMode(string proteinName)
        {
            var protein = data.FirstOrDefault(d => d.protein == proteinName);

            if (protein.protein == null)
                return $"MISSING: {proteinName}";

            string sequence = RLD(protein.amino_acids);
            var counts = new Dictionary<char, int>();

            foreach (char amino in sequence)
            {
                if (counts.ContainsKey(amino))
                    counts[amino]++;
                else
                    counts[amino] = 1;
            }

            var maxCount = counts.Values.Max();
            var mostFrequent = counts.Where(kv => kv.Value == maxCount)
                                     .OrderBy(kv => kv.Key)
                                     .First();

            return $"amino-acid occurs: {mostFrequent.Key} {mostFrequent.Value}";
        }

        static string RLD(string encoded)
        {
            var decoded = new StringBuilder();
            for (int i = 0; i < encoded.Length; i++)
            {
                if (char.IsDigit(encoded[i]))
                {
                    int count = encoded[i] - '0';
                    char amino = encoded[i + 1];
                    decoded.Append(new string(amino, count));
                    i++; 
                }
                else
                {
                    decoded.Append(encoded[i]);
                }
            }
            return decoded.ToString();
        }
    }
}
