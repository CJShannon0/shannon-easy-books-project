using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIVariables", menuName = "Scriptable Objects/UIVariables")]
public class UIVariables : ScriptableObject
{
    //Logic Variables
    public List<string> businessesList;
    public string businessSelected;
    public int yearSelected;

    //Transaction temp variables
    public string vendor;
    public string date;
    public float amount;
    public int category;
    public string notes;
}
