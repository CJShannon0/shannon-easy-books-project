using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIVariables", menuName = "Scriptable Objects/UIVariables")]
public class UIVariables : ScriptableObject
{
    //Logic Variables
    List<string> businessesList;
    int businessSelected;
    int yearSelected;

    //Transaction temp variables
    string vendor;
    string date;
    string amount;
    string category;
    string notes;
}
