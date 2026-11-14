using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIVariables vars;
    //global variables
    private VisualElement _root;
    private Label _busSelectedText;
    private ListView _transactionList;
    private Button _openPopupButton;
    private Button _year_L;
    private Button _year_R;
    private IntegerField _yearField;
    private Tab _summaryTab;
    private ListView _summaryList;
    private Button _exportButton;
    private DropdownField _businessDropdown;
    private Button _addBusinessButton;
    private List<Button> _addTransButtons;
    private List<Button> _cancelButtons;
    private List<Button> editTransButtons;


    //Runs on startup
    public void Start()
    {
        TransactionIO.Initialize(vars);
        ResetScriptableObject();
        SetVisualElements();
        RegisterUICallbacks();
        UpdateBusinessList();
    }

    private void SetVisualElements()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _busSelectedText = _root.Q<Label>("BusinessText");
        _transactionList = _root.Q<ListView>("TransactionList");
        _openPopupButton = _root.Q<Button>("OpenPopupButton");
        _year_L = _root.Q<Button>("YearButton_L");
        _year_R = _root.Q<Button>("YearButton_R");
        _yearField = _root.Q<IntegerField>("YearIntField");
        _summaryTab = _root.Q<Tab>("SummaryScreen");
        _summaryList = _root.Q<ListView>("SummaryListView");
        _businessDropdown = _root.Q<DropdownField>("BusinessDropdown");
        _exportButton = _root.Q<Button>("ExportButton");
        _addBusinessButton = _root.Q<Button>("AddBusinessButton");

        _addTransButtons = _root.Query<Button>("AddTransactionButton").ToList();
        _cancelButtons = _root.Query<Button>("CancelButton").ToList();

        //Edit transaction buttons need to be set after the list is constructed
    }

    private void RegisterUICallbacks()
    {
        _year_L.clicked += () => ChangeYear(-1);
        _year_L.clicked += () => UpdateSummary(); //If event order is non-ordered, register updateSummary() to a yearField value changed event.
        _year_R.clicked += () => ChangeYear(1);
        _year_R.clicked += () => UpdateSummary();

        _openPopupButton.clicked += () => OpenPopup();

        _summaryTab.selected += (Tab tab) => UpdateSummary();
        _exportButton.clicked += () => ExportSummary();

        _businessDropdown.RegisterValueChangedCallback(evt => UpdateBusiness());
        _addBusinessButton.clicked += () => AddBusiness();

        foreach(Button b in _addTransButtons)
        {
            b.clicked += () => AddTransaction();
        }
        foreach(Button b in _cancelButtons)
        {
            b.clicked += () => CancelTransaction();
        }

        //Edit transaction buttons need to be registered need to be set after the list is contructed
    }

    private void ResetScriptableObject()
    {
        vars.vendor = null;
        vars.date = null;
        vars.amount = 0;
        vars.category = 0;
        vars.notes = null;
    }
    private void ChangeYear(int amount)
    {
        Debug.Log("change year event triggered");
        _yearField.value += amount;
    }

    private void OpenPopup()
    {
        Debug.Log("open popup event triggered");
    }

    private void UpdateSummary()
    {
        Debug.Log("update summary event triggered");
    }

    private void ExportSummary()
    {
        Debug.Log("export summary event triggered");
    }

    private void UpdateBusinessList()
    {
        Debug.Log("update business list event triggered");
    }

    private void UpdateBusiness()
    {
        Debug.Log("update business event triggered");
    }

    private void AddBusiness()
    {
        Debug.Log("add business event triggered");
    }

    private void AddTransaction()
    {
        Debug.Log("add transaction event triggered");
        bool success = TransactionIO.InputTransaction();
        if(success) ResetScriptableObject();
        else
        {
            //create error message, not part of MVP
        }
    }

    private void CancelTransaction()
    {
        Debug.Log("cancel transaction event triggered");
        ResetScriptableObject();
        //Close popup window
    }

    private void EditTransaction()
    {
        Debug.Log("edit transaction event triggered");
    }
}
