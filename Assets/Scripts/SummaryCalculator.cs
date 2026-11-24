using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class SummaryCalculator
{
    public static UIVariables vars;
    public static VisualTreeAsset template;
    private static string generalPath;
    private static string specificPath;

    public static void Initialize(UIVariables so, VisualTreeAsset temp)
    {
        vars = so;
        template = temp;
        generalPath = Path.Combine(Application.persistentDataPath,"exports");
    }
    public static bool UpdateSummaryList(ListView listView)
    {
        float[] records = CalculateTotal(TransactionIO.GetFileContents());
        listView.itemsSource = records;
        listView.makeItem = () => template.CloneTree();
        listView.bindItem = (record, index) =>
        {
            FloatField amount = record.Q<FloatField>("FloatField");
            amount.label = Categories.categories[index];
            amount.value = records[index];
        };
        listView.Rebuild();
        return true;
    }

    public static void ExportSummary()
    {
        Directory.CreateDirectory(generalPath);
        specificPath = Path.Combine(generalPath,vars.businessesList[vars.businessIndex] + "-" + vars.yearSelected + ".txt");

        StreamWriter writer = new StreamWriter(specificPath);
        float[] totals = CalculateTotal(TransactionIO.GetFileContents());

        writer.WriteLine("Transaction Summary");
        writer.WriteLine("Business: \t" + vars.businessSelected);
        writer.WriteLine("Year: \t" + vars.yearSelected);
        writer.WriteLine();

        int categoryWidth = 0;
        int amountWidth = 0;
        string[] amountTexts = new string[totals.Length];
        for(int i = 0; i < totals.Length; i++)
        {
            categoryWidth = Math.Max(categoryWidth, Categories.categories[i].Length);
            amountTexts[i] = totals[i].ToString();
            amountWidth = Math.Max(amountWidth, amountTexts[i].Length);
        }

        for(int i = 0; i < totals.Length; i++)
        {
            writer.WriteLine(Categories.categories[i].PadRight(categoryWidth + 2) + amountTexts[i].PadLeft(amountWidth));
        }
        writer.Close();

#if UNITY_STANDALONE || UNITY_EDITOR
        Application.OpenURL(new Uri(generalPath + Path.DirectorySeparatorChar).AbsoluteUri);
#endif
    }

    public static float[] CalculateTotal(List<string> document)
    {
        float[] runningTotals = new float[Categories.categories.Length];

        for(int i = 0; i < runningTotals.Length; i++)
        {
            runningTotals[i] = 0;
        }

        foreach(string entry in document)
        {
            string[] fields = entry.Split(",");

            //[1] = date, [3] = category, [4] = amount
            int year = int.Parse(fields[1].Split("/",3)[2]);
            //Debug.LogWarning(year);
            if(year == vars.yearSelected)
                runningTotals[int.Parse(fields[3])] += float.Parse(fields[4]);
        }

        return runningTotals;
    }
}
