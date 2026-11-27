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
    private static readonly string[] DateFormats =
    {
        "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy", "MMddyyyy"
    };

    static int listLength = 0;

    public static void Initialize(UIVariables scriptableObject, VisualTreeAsset temp)
    {
        vars = scriptableObject;
        generalPath = Path.Combine(Application.persistentDataPath,"Records");
        template = temp;
        UpdateIOPaths();
        listLength = GetNumEntries();
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
            listLength += 1;
            newTrans.Append(listLength + ",");  
            newTrans.Append(
                DateTime.ParseExact(vars.date, DateFormats, null, System.Globalization.DateTimeStyles.None)
                    .ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture) + ",");
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
            IntegerField id = record.Q<IntegerField>("TransactionID");

    
            id.value = int.Parse(fields[0]);
            date.text = fields[1];
            vendor.text = fields[2];
            category.text = Categories.categories[int.Parse(fields[3])];
            amount.value = float.Parse(fields[4]);
            notes.text = fields[5];
        };
        listView.Rebuild();

        fileOutput = new StreamReader(specificPath);    

        try
        {
            var line = fileOutput.ReadLine();
            while(line != null)
            {
                records.Add(line.Split(",", 6));
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

    public static bool DeleteTransaction(int id)
    {
        List<string> records = GetFileContents();

        listLength = 0;
        
        //StreamWriter is used to clear the contents of the file for rewriting.
        StreamWriter newFileInput = new StreamWriter(specificPath,false);
        newFileInput.Close();

        //Have to move all records after the deleted one up one line to save memory.
        foreach(string record in records)
        {
            string[] fields = record.Split(",");
            if(int.Parse(fields[0]) != id)
            {
                //I could probably overload InputTransaction() to make code more readable.
                vars.date = fields[1];
                vars.vendor = fields[2];
                vars.category = int.Parse(fields[3]);
                vars.amount = float.Parse(fields[4]);
                vars.notes = fields[5];
                InputTransaction();
            }
        }

        vars.date = null;
        vars.vendor = null;
        vars.category = 0;
        vars.amount = 0;
        vars.notes = null;

        return true;
    }

    public static bool EditTransaction(int id)
    {
        List<string> records = GetFileContents();

        listLength = 0;
        
        //StreamWriter is used to clear the contents of the file for rewriting.
        StreamWriter newFileInput = new StreamWriter(specificPath,false);
        newFileInput.Close();

        foreach(string record in records)
        {
            string[] fields = record.Split(",");
            if(int.Parse(fields[0]) != id)
            {
                StreamWriter edit = new StreamWriter(specificPath);
                edit.WriteLine(record);
                edit.Close();
            }
            else
            {
                listLength = int.Parse(fields[0]) - 1;
                InputTransaction();
            }
        }

        vars.date = null;
        vars.vendor = null;
        vars.category = 0;
        vars.amount = 0;
        vars.notes = null;
        listLength = records.Count;

        return true;
    }

    public static bool TestVarFormats()
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
        DateTime dt;
        if (!DateTime.TryParseExact(vars.date, DateFormats, null, System.Globalization.DateTimeStyles.None, out dt))
        {
            Debug.LogError("Date is not in the correct format");
            return false;
        }

        return true;
    }

    public static List<string> GetFileContents()
    {
        List<string> contents = new List<string>();

        StreamReader fileOutput = new StreamReader(specificPath);

        string line = fileOutput.ReadLine();
        while(line != null)
        {
            contents.Add(line);
            line = fileOutput.ReadLine();
        }

        fileOutput.Close();
        return contents;
    }

    private static int GetNumEntries()
    {
        return GetFileContents().Count;
    }

    public static bool SetTempVariablesForEditing(int id)
    {
        List<string> records = GetFileContents();
        id--;
        string[] fields = records[id].Split(",");

        vars.idSelected = int.Parse(fields[0]);
        vars.date = fields[1];
        vars.vendor = fields[2];
        vars.category = int.Parse(fields[3]);
        vars.amount = int.Parse(fields[4]);
        vars.notes = fields[5];

        return true;
    }
}
