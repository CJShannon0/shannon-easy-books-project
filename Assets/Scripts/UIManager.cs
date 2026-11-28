using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIVariables vars;
    public VisualTreeAsset transRecord;
    public VisualTreeAsset summEntry;
    //global variables
    private VisualElement _root;
    private Label _busSelectedText;

    private ListView _transactionList;
    private Tab _transListTab;

    private VisualElement _transPopup;
    private Button _openAddTransPopupButton;
    private Button _closeAddTransPopupButton;

    private VisualElement _editPopup;
    private Button _editTransButton;
    private Button _closeEditTransPopupButton;

    private Button _year_L;
    private Button _year_R;
    private IntegerField _yearField;
    private Tab _summaryTab;
    private ListView _summaryList;
    private Button _exportButton;

    private DropdownField _businessDropdown;
    private TextField _addBusinessField;
    private Button _addBusinessButton;
    private List<Button> _addTransButtons;



    //Runs on startup
    public void Start()
    {
        TransactionIO.Initialize(vars,transRecord);
        SummaryCalculator.Initialize(vars,summEntry);
        ResetScriptableObject();
        SetVisualElements();
        RegisterUICallbacks();
        UpdateBusinessList();
    }

    private void SetVisualElements()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _busSelectedText = _root.Q<Label>("BusinessText");
        
        _transListTab = _root.Q<Tab>("TransactionsScreen");
        _transactionList = _root.Q<ListView>("TransactionList");
        _transactionList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        
        _transPopup = _root.Q<VisualElement>("TransactionPopup");
        _openAddTransPopupButton = _root.Q<Button>("OpenAddTransactionPopupButton");
        _closeAddTransPopupButton = _root.Q<Button>("CloseAddTransactionPopupButton");
        
        _editPopup = _root.Q<VisualElement>("EditTransactionPopup");
        _editTransButton = _root.Q<Button>("EditTransactionButton");
        _closeEditTransPopupButton = _root.Q<Button>("CloseEditTransactionPopupButton");

        _year_L = _root.Q<Button>("YearButton_L");
        _year_R = _root.Q<Button>("YearButton_R");
        _yearField = _root.Q<IntegerField>("YearIntField");
        _summaryTab = _root.Q<Tab>("SummaryScreen");
        _summaryList = _root.Q<ListView>("SummaryListView");
       
        _exportButton = _root.Q<Button>("ExportButton");

        _addBusinessField = _root.Q<TextField>("AddBusinessField");
        _addBusinessButton = _root.Q<Button>("AddBusinessButton");
        _businessDropdown = _root.Q<DropdownField>("BusinessDropdown");

        _addTransButtons = _root.Query<Button>("AddTransactionButton").ToList();

        //Edit transaction buttons need to be set after the list is constructed
    }

    private void RegisterUICallbacks()
    {
        _year_L.clicked += () => ChangeYear(-1);        
        _year_R.clicked += () => ChangeYear(1);
        //_yearField.RegisterValueChangedCallback(evt => UpdateSummary());
        //This event triggers before the scriptable object is updated so I can't use it.

        _transListTab.selected += (Tab tab) => UpdateTransList();
        _openAddTransPopupButton.clicked += () => OpenPopup(_transPopup);
        _closeAddTransPopupButton.clicked += () => ClosePopup(_transPopup);

        _editTransButton.clicked += () => EditTransaction();
        _closeEditTransPopupButton.clicked += () => ClosePopup(_editPopup);
        _closeEditTransPopupButton.clicked += () => ResetScriptableObject();

        _summaryTab.selected += (Tab tab) => UpdateSummary();
        _exportButton.clicked += () => ExportSummary();

        _businessDropdown.RegisterValueChangedCallback(evt => UpdateBusiness());
        _addBusinessButton.clicked += () => AddBusiness();

        foreach(Button b in _addTransButtons)
        {
            b.clicked += () => AddTransaction();
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
        vars.yearSelected += amount;
        UpdateSummary();
    }

    private void UpdateTransList()
    {
        Debug.Log("update transaction list event triggered");
        TransactionIO.UpdateTransactionList(_transactionList, SetTransactionRecordEvents);
    }
    private void OpenPopup(VisualElement panel)
    {
        Debug.Log("open popup event triggered");
        panel.RemoveFromClassList("hidden");
    }

    private void ClosePopup(VisualElement panel)
    {
        Debug.Log("close popup event triggered");
        panel.AddToClassList("hidden");
    }
    private void UpdateSummary()
    {
        Debug.Log("update summary event triggered");
        SummaryCalculator.UpdateSummaryList(_summaryList);
    }

    private void ExportSummary()
    {
        Debug.Log("export summary event triggered");
        SummaryCalculator.ExportSummary();
    }

    //Used to populate dropdown menu
    private void UpdateBusinessList()
    {
        Debug.Log("update business list event triggered");
        _businessDropdown.choices = new List<string>();
        _businessDropdown.choices = vars.businessesList;
        _businessDropdown.index = vars.businessIndex;
    }

    private void UpdateBusiness()
    {
        Debug.Log("update business event triggered");
        vars.businessIndex = _businessDropdown.index;
        vars.businessSelected = vars.businessesList[vars.businessIndex];
        TransactionIO.UpdateIOPaths();
    }

    private void AddBusiness()
    {
        Debug.Log("add business event triggered");

        string newBusiness = _addBusinessField.value;
        if(newBusiness != null && !vars.businessesList.Contains(newBusiness))
        {
            vars.businessesList.Add(newBusiness);
            _addBusinessField.value = null;
            UpdateBusinessList();
        }
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
        UpdateTransList();
    }

    private void CancelTransaction()
    {
        Debug.Log("cancel transaction event triggered");
        ResetScriptableObject();
        _transPopup.AddToClassList("hidden"); 
        UpdateTransList();
        //The transaction list is the only place this cancel button is
        //If I were to scale the ui up, I would have to refactor this code.
    }

    private void EditTransaction()
    {
        Debug.Log("edit transaction event triggered");
        TransactionIO.EditTransaction(vars.idSelected);
        UpdateTransList();
        ClosePopup(_editPopup);
    }

    private void DeleteTransaction(int id)
    {
        Debug.Log("delete transaction event triggered");
        TransactionIO.DeleteTransaction(id);
        UpdateTransList();
    }

    private void SetTransactionRecordEvents(VisualElement trans)
    {
        IntegerField idField = trans.Q<IntegerField>("TransactionID");
        Button deleteButton = trans.Q<Button>("DeleteButton");
        Button editButton = trans.Q<Button>("EditButton");

        deleteButton.clicked += () => DeleteTransaction(idField.value);
        editButton.clicked += () =>
        {
            TransactionIO.SetTempVariablesForEditing(idField.value);
            OpenPopup(_editPopup);
        };
    }
}
