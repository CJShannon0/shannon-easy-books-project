using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public static class TransactionIO
{
    static UIVariables vars;
    static VisualTreeAsset template;
    static string generalPath;
    static string specificPath;
    static StreamReader fileOutput;
    static StreamWriter fileInput;
    public static void Initialize(UIVariables scriptableObject, VisualTreeAsset temp)
    {
        vars = scriptableObject;
        generalPath = Path.Combine(Application.persistentDataPath,"Records");
        template = temp;
        UpdateIOPaths();
    }
    public static void UpdateIOPaths()
    {
        fileInput?.Close();    
        fileOutput?.Close();

        Directory.CreateDirectory(generalPath);
        specificPath = Path.Combine(generalPath,vars.businessesList[vars.businessIndex] + ".csv");
        using (new FileStream(specificPath, FileMode.OpenOrCreate, FileAccess.Write)) { }    
    }

    public static bool InputTransaction()
    {
        fileInput = new StreamWriter(specificPath, append: true);
        StringBuilder newTrans = new StringBuilder();

        if(!TestVarFormats()) {
            Debug.LogError("Test variable formats check failed.");
            fileInput.Close();
            return false;
        }
        try
        {        
            newTrans.Append(vars.date + ",");
            newTrans.Append(vars.vendor + ",");
            newTrans.Append(vars.category.ToString() + ",");
            newTrans.Append(vars.amount.ToString() + ",");
            if(vars.notes != null) //notes is allowed to be empty
                newTrans.Append(vars.notes);
            else 
                newTrans.Append(" ");
        } 
        catch (Exception)
        {
            Debug.LogError("StreamWriter failed to parse inputs.");
            fileInput.Close();
            return false;
        }

        try
        {
            //Debug.Log("Trans String: " + newTrans.ToString());
            fileInput.WriteLine(newTrans.ToString());
            fileInput.Flush();
            fileInput.Close();
            Debug.Log("Successfully added Transaction");
            return true;
        } 
        catch (Exception)
        {   
            Debug.LogError("StreamWriter could not write transaction to file");
            fileInput.Close();
            return false;
        }
    }

   public static bool UpdateTransactionList(ListView listView)
    {
        var records = new List<string[]>();
        listView.itemsSource = records;
        listView.makeItem = () => template.CloneTree();
        listView.bindItem = (record, index) =>
        {
            string[] fields = records[index];

            Label vendor = record.Q<Label>("VendorText");
            Label date = record.Q<Label>("DateText");
            Label category = record.Q<Label>("CategoryText");
            Label notes = record.Q<Label>("NotesText");
            FloatField amount = record.Q<FloatField>("AmountField");

            date.text = fields[0];
            vendor.text = fields[1];
            category.text = Categories.categories[int.Parse(fields[2])];
            amount.value = float.Parse(fields[3]);
            notes.text = fields[4];
        };
        listView.Rebuild();

        fileOutput = new StreamReader(specificPath);    

        try
        {
            var line = fileOutput.ReadLine();
            while(line != null)
            {
                records.Add(line.Split(",", 5));
                line = fileOutput.ReadLine();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("StreamReader failed to update transaction list." + e);
            fileOutput.Close();
            return false;   
        }

        fileOutput.Close();
        listView.Rebuild();
        return true;
    }

    private static bool TestVarFormats()
    {
        //test int/float variables
        if(vars.amount < 0 ||
            vars.category < 0)
        {
            Debug.LogError("StreamWriter failed to parse amount or category.");
            return false;
        }

        //test string vars for null
        if(vars.vendor == null ||
            vars.date == null)
        {
            Debug.LogError("StreamWriter failed to parse vendor or date.");
            return false;
        }

        //parse date for correct format
        var formats = new[] { "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy", "MMddyyyy"};
        DateTime dt;
        if (!DateTime.TryParseExact(vars.date, formats, null, System.Globalization.DateTimeStyles.None, out dt))
        {
            Debug.LogError("Date is not in the correct format");
            return false;
        }

        return true;
    }
}
