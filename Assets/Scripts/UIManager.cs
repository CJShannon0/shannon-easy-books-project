using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
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
        SetVisualElements();
        RegisterUICallbacks();
        UpdateBusinessList();
    }

    private void SetVisualElements()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _busSelectedText = _root.Q<Label>(name:"BusinessText");
        _transactionList = _root.Q<ListView>(name:"TransactionList");
        _openPopupButton = _root.Q<Button>(name:"OpenPopupButton");
        _year_L = _root.Q<Button>(name:"YearButton_L");
        _year_R = _root.Q<Button>(name:"YearButton_R");
        _yearField = _root.Q<IntegerField>(name:"YearIntField");
        _summaryTab = _root.Q<Tab>(name:"SummaryScreen");
        _summaryList = _root.Q<ListView>(name:"SummaryListView");
        _businessDropdown = _root.Q<DropdownField>(name:"BusinessDropdown");
        _exportButton = _root.Q<Button>(name:"ExportButton");
        _addBusinessButton = _root.Q<Button>(name:"AddBusinessButton");

        _addTransButtons = _root.Query<Button>(name:"AddTransactionButton").ToList();
        _cancelButtons = _root.Query<Button>(name:"CancelButton").ToList();

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


    private void ChangeYear(int amount)
    {
        
    }

    private void OpenPopup()
    {
        
    }

    private void UpdateSummary()
    {
        
    }

    private void ExportSummary()
    {
        
    }

    private void UpdateBusinessList()
    {
        
    }

    private void UpdateBusiness()
    {
        
    }

    private void AddBusiness()
    {
        
    }

    private void AddTransaction()
    {
        
    }

    private void CancelTransaction()
    {
        
    }

    private void EditTransaction()
    {
        
    }
}
