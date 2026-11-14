using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class TransactionIO
{
    static UIVariables vars;
    static string generalPath;
    static string specificPath;
    static StreamReader fileOutput;
    static StreamWriter fileInput;
    public static void Initialize(UIVariables scriptableObject)
    {
        vars = scriptableObject;
        generalPath = Path.Combine(Application.persistentDataPath,"Records");
        UpdateIOPaths();
    }
    public static void UpdateIOPaths()
    {
        fileInput?.Close();    
        fileOutput?.Close();

        Directory.CreateDirectory(generalPath);
        specificPath = Path.Combine(generalPath,vars.businessesList[vars.businessIndex] + ".csv");
        using (new FileStream(specificPath, FileMode.OpenOrCreate, FileAccess.Write)) { }
        fileInput = new StreamWriter(specificPath, append: true);
    }

    public static bool InputTransaction()
    {
        StringBuilder newTrans = new StringBuilder();

        if(!TestVarFormats()) {
            Debug.LogError("Test variable formats check failed.");
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
            return false;
        }

        try
        {
            //Debug.Log("Trans String: " + newTrans.ToString());
            fileInput.WriteLine(newTrans.ToString());
            fileInput.Flush();
            Debug.Log("Successfully added Transaction");
            return true;
        } 
        catch (Exception)
        {   
            Debug.LogError("StreamWriter could not write transaction to file");
            return false;
        }
    }

    public static List<string> GetFileContents()
    {
        fileOutput = new StreamReader(specificPath);    

        List<string> fileContents = new List<string>();
        string line = fileOutput.ReadLine();
        while(line != null)
        {
            fileContents.Add(line);
            line = fileOutput.ReadLine();
        }

        fileOutput.Close();
        return fileContents;
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
        var formats = new[] { "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "dd/MM/yyyy" };
        DateTime dt;
        if (!DateTime.TryParseExact(vars.date, formats, null, System.Globalization.DateTimeStyles.None, out dt))
        {
            Debug.LogError("Date is not in the correct format");
            return false;
        }

        return true;
    }
}
