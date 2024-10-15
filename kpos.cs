using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

struct GeneticData
{
    public string Protein;    // Название белка
    public string Organism;   // Название организма
    public string AminoAcids; // Цепочка аминокислот
}

class GeneticSearch
{
    static List<GeneticData> proteins = new List<GeneticData>(); // Список белков
    static int commandCounter = 1; // Счётчик команд

    static void Main()
    {
        // Чтение данных о белках
        ReadSequences("sequences.txt");

        // Создание выходного файла
        using (StreamWriter output = new StreamWriter("genedata.txt"))
        {
            output.WriteLine("Ваше Имя"); // Ваше имя
            output.WriteLine("Генетический поиск");

            // Чтение и выполнение команд
            ProcessCommands("commands.txt", output);
        }
    }

    static void ReadSequences(string filename)
    {
        foreach (string line in File.ReadLines(filename))
        {
            var parts = line.Split('\t');
            proteins.Add(new GeneticData
            {
                Protein = parts[0],
                Organism = parts[1],
                AminoAcids = RLDecoding(parts[2])
            });
        }
    }

    static void ProcessCommands(string filename, StreamWriter output)
    {
        foreach (string line in File.ReadLines(filename))
        {
            var parts = line.Split('\t');
            string command = parts[0];
            output.WriteLine($"---\n{commandCounter:D3}");

            switch (command)
            {
                case "search":
                    Search(parts[1], output);
                    break;
                case "diff":
                    Diff(parts[1], parts[2], output);
                    break;
                case "mode":
                    Mode(parts[1], output);
                    break;
            }

            commandCounter++;
        }
    }

    static void Search(string sequence, StreamWriter output)
    {
        bool found = false;
        foreach (var protein in proteins)
        {
            if (protein.AminoAcids.Contains(sequence))
            {
                output.WriteLine($"{protein.Organism}\t{protein.Protein}");
                found = true;
            }
        }
        if (!found)
        {
            output.WriteLine("NOT FOUND");
        }
    }

    static void Diff(string protein1, string protein2, StreamWriter output)
    {
        var p1 = proteins.FirstOrDefault(p => p.Protein == protein1);
        var p2 = proteins.FirstOrDefault(p => p.Protein == protein2);

        if (p1.Protein == null || p2.Protein == null)
        {
            output.Write("MISSING:");
            if (p1.Protein == null) output.Write($" {protein1}");
            if (p2.Protein == null) output.Write($" {protein2}");
            output.WriteLine();
        }
        else
        {
            int differences = CalculateDifference(p1.AminoAcids, p2.AminoAcids);
            output.WriteLine($"amino-acids difference: {differences}");
        }
    }

    static int CalculateDifference(string seq1, string seq2)
    {
        int minLength = Math.Min(seq1.Length, seq2.Length);
        int diff = Math.Abs(seq1.Length - seq2.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (seq1[i] != seq2[i]) diff++;
        }
        return diff;
    }

    static void Mode(string proteinName, StreamWriter output)
    {
        var protein = proteins.FirstOrDefault(p => p.Protein == proteinName);

        if (protein.Protein == null)
        {
            output.WriteLine($"amino-acid occurs: MISSING: {proteinName}");
        }
        else
        {
            var mostCommon = protein.AminoAcids
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .First();

            output.WriteLine($"amino-acid occurs: {mostCommon.Key} {mostCommon.Count()}");
        }
    }

    static string RLEncoding(string aminoAcids)
    {
        StringBuilder encoded = new StringBuilder();
        int count = 1;

        for (int i = 1; i <= aminoAcids.Length; i++)
        {
            if (i < aminoAcids.Length && aminoAcids[i] == aminoAcids[i - 1])
            {
                count++;
            }
            else
            {
                if (count > 2)
                    encoded.Append(count);
                encoded.Append(aminoAcids[i - 1]);
                count = 1;
            }
        }
        return encoded.ToString();
    }

    static string RLDecoding(string encoded)
    {
        StringBuilder decoded = new StringBuilder();
        int i = 0;

        while (i < encoded.Length)
        {
            if (char.IsDigit(encoded[i]))
            {
                int count = encoded[i] - '0';
                decoded.Append(new string(encoded[i + 1], count));
                i += 2;
            }
            else
            {
                decoded.Append(encoded[i]);
                i++;
            }
        }
        return decoded.ToString();
    }
}
