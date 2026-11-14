using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public static class TransactionIO
{
    static UIVariables vars;
    static string generalPath;
    static StreamReader fileOutput;
    static StreamWriter fileInput;
    public static void Initialize(string path)
    {
        generalPath = Path.Combine(Application.persistentDataPath,"Records");
        UpdateIOPaths();
    }
    public static void UpdateIOPaths()
    {
        fileInput?.Close();    
        fileOutput?.Close();

        string specificPath = Path.Combine(generalPath,vars.businessesList[vars.businessIndex] + ".csv");
        fileInput = new StreamWriter(specificPath);
        fileOutput = new StreamReader(specificPath);
    }

    public static bool InputTransaction()
    {
        StringBuilder newTrans = new StringBuilder();

        //check for incomplete form
        if(vars.date == null ||
            vars.vendor == null ||
            vars.category == -1 ||
            vars.amount <= 0 || //amount can also not be negative
            vars.notes == null)
        {
            return false;
        }

        newTrans.Append(vars.date + ",");
        newTrans.Append(vars.vendor + ",");
        newTrans.Append(vars.category + ",");
        newTrans.Append(vars.amount + ",");
        newTrans.Append(vars.notes + ",");

        try
        {
            fileInput.WriteLine(newTrans);
            return true;
        } catch (Exception e)
        {   
            Debug.LogError("StreamWriter could not write transaction to file");
            return false;
        }
    }

    public static List<string> GetFileContents()
    {
        List<string> fileContents = new List<string>();
        string line = fileOutput.ReadLine();
        while(line != null)
        {
            fileContents.Add(line);
        }

        UpdateIOPaths();
        return fileContents;
    }
}
