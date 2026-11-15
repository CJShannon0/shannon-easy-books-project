using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class SummaryCalculator
{
    public static VisualTreeAsset template;

    public static void Initialize(VisualTreeAsset temp)
    {
        template = temp;
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

    public static float[] CalculateTotal(List<string> document)
    {
        float[] runningTotals = new float[Categories.categories.Length];

        for(int i = 0; i < runningTotals.Length; i++)
        {
            runningTotals[i] = 0;
        }

        foreach(string entry in document)
        {
            string[] fields = entry.Split(",", 5);

            //[2] = category, [3] = amount
            runningTotals[int.Parse(fields[2])] += float.Parse(fields[3]);
        }

        return runningTotals;
    }
}
