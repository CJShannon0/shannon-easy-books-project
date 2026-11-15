using System.Reflection;
using System.Xml.Linq;
using UnityEngine.UIElements;

namespace Scripts.UnitTests;

public class UIManager_Tests
{
    #region Set Visual Elements Test
    [Fact]
    public void SetVisualElements_AssignsEveryField()
    {
        var projectRoot = FindProjectRoot();
        var indexPath = Path.Combine(projectRoot, "Assets", "UserInterface", "UXML", "index.uxml");
        var root = LoadIndexUxml(indexPath, projectRoot);
        var uiManager = new UIManager
        {
            Component = new UIDocument(root)
        };
        var setVisualElements = typeof(UIManager).GetMethod(
            "SetVisualElements",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(setVisualElements);
        setVisualElements.Invoke(uiManager, null);

        var fields = typeof(UIManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        var deferredField = fields.Single(field => field.Name == "editTransButtons");
        Assert.Null(deferredField.GetValue(uiManager));

        var addTransactionButtons = Assert.IsType<List<Button>>(
            fields.Single(field => field.Name == "_addTransButtons").GetValue(uiManager));
        var cancelButtons = Assert.IsType<List<Button>>(
            fields.Single(field => field.Name == "_cancelButtons").GetValue(uiManager));
        Assert.Equal(2, addTransactionButtons.Count);
        Assert.Single(cancelButtons);

        var unassignedFields = fields
            .Where(field => field.Name != "editTransButtons")
            .Where(field => field.GetValue(uiManager) is null)
            .Select(field => field.Name);

        Assert.Empty(unassignedFields);
    }

    private static string FindProjectRoot()
    {
        for (DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var indexPath = Path.Combine(directory.FullName, "Assets", "UserInterface", "UXML", "index.uxml");
            if (File.Exists(indexPath))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate index.uxml in the Unity project.");
    }

    private static VisualElement LoadIndexUxml(string indexPath, string projectRoot)
    {
        var root = new VisualElement();
        AddDocumentContents(indexPath, projectRoot, root);
        return root;
    }

    private static void AddDocumentContents(string uxmlPath, string projectRoot, VisualElement parent)
    {
        var document = XDocument.Load(uxmlPath);
        if (document.Root == null)
        {
            throw new InvalidDataException($"UXML document has no root element: {uxmlPath}");
        }

        var templates = document.Descendants()
            .Where(element => element.Name.LocalName == "Template")
            .Where(element => element.Attribute("name") != null && element.Attribute("src") != null)
            .ToDictionary(
                element => (string)element.Attribute("name")!,
                element => ResolveTemplatePath((string)element.Attribute("src")!, projectRoot));

        foreach (var element in document.Root.Elements())
        {
            AddUxmlElement(element, projectRoot, templates, parent);
        }
    }

    private static void AddUxmlElement(
        XElement element,
        string projectRoot,
        Dictionary<string, string> templates,
        VisualElement parent)
    {
        switch (element.Name.LocalName)
        {
            case "Template":
            case "Style":
            case "Bindings":
            case "DataBinding":
                return;
            case "Instance":
                var templateName = (string?)element.Attribute("template");
                if (templateName == null || !templates.TryGetValue(templateName, out var templatePath))
                {
                    throw new InvalidDataException($"UXML instance references an unknown template: {templateName}");
                }

                AddDocumentContents(templatePath, projectRoot, parent);
                return;
        }

        var visualElement = CreateVisualElement(element);
        parent.Add(visualElement);

        foreach (var child in element.Elements())
        {
            AddUxmlElement(child, projectRoot, templates, visualElement);
        }
    }

    private static VisualElement CreateVisualElement(XElement element)
    {
        var name = (string?)element.Attribute("name") ?? string.Empty;
        return element.Name.LocalName switch
        {
            "Label" => new Label(name),
            "ListView" => new ListView(name),
            "Button" => new Button(name),
            "IntegerField" => new IntegerField(name),
            "Tab" => new Tab(name),
            "DropdownField" => new DropdownField(name),
            _ => new VisualElement(name)
        };
    }

    private static string ResolveTemplatePath(string source, string projectRoot)
    {
        const string projectPrefix = "project://database/";
        if (!source.StartsWith(projectPrefix, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Unsupported UXML template path: {source}");
        }

        var relativePath = source.Substring(projectPrefix.Length).Split('?', '#')[0];
        return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
    #endregion
    #region Transaction IO Tests

    [Theory]
    [InlineData("1/2/2024")]
    [InlineData("01/2/2024")]
    [InlineData("1/02/2024")]
    [InlineData("01/02/2024")]
    [InlineData("01022024")]
    public void TestVarFormats_ReturnsTrueForSupportedDateFormats(string date)
    {
        var variables = CreateTransactionVariables(date: date);

        Assert.True(InvokeTestVarFormats(variables));
    }

    [Theory]
    [InlineData(-1, 0, "vendor", "01/02/2024")]
    [InlineData(1, -1, "vendor", "01/02/2024")]
    [InlineData(1, 0, null, "01/02/2024")]
    [InlineData(1, 0, "vendor", null)]
    [InlineData(1, 0, "vendor", "2024-01-02")]
    public void TestVarFormats_ReturnsFalseForInvalidValues(
        float amount,
        int category,
        string? vendor,
        string? date)
    {
        var variables = CreateTransactionVariables(amount, category, vendor, date);

        Assert.False(InvokeTestVarFormats(variables));
    }

    private static UIVariables CreateTransactionVariables(
        float amount = 1,
        int category = 0,
        string? vendor = "vendor",
        string? date = "01/02/2024")
    {
        return new UIVariables
        {
            amount = amount,
            category = category,
            vendor = vendor!,
            date = date!
        };
    }

    private static bool InvokeTestVarFormats(UIVariables variables)
    {
        var variablesField = typeof(TransactionIO).GetField(
            "vars",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(variablesField);
        variablesField.SetValue(null, variables);

        var testMethod = typeof(TransactionIO).GetMethod(
            "TestVarFormats",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(testMethod);
        return Assert.IsType<bool>(testMethod.Invoke(null, null));
    }
    #endregion
}
