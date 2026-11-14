namespace Unity.VisualScripting
{
}

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName { get; set; } = string.Empty;
        public string menuName { get; set; } = string.Empty;
    }

    public class ScriptableObject
    {
    }

    public class MonoBehaviour
    {
        public object? Component { get; set; }

        protected T GetComponent<T>() where T : class
        {
            return Component as T ?? throw new InvalidOperationException($"No component of type {typeof(T).Name} is attached.");
        }
    }

    public static class Application
    {
        public static string persistentDataPath => Path.GetTempPath();
    }

    public static class Debug
    {
        public static void Log(object? message) { }
        public static void LogError(object? message) { }
    }
}

namespace UnityEngine.UIElements
{
    public class VisualElement
    {
        private readonly List<VisualElement> _children = new();

        public VisualElement(string name = "")
        {
            this.name = name;
        }

        public string name { get; }

        public void Add(VisualElement child)
        {
            _children.Add(child);
        }

        public T? Q<T>(string name) where T : VisualElement
        {
            return DescendantsAndSelf()
                .OfType<T>()
                .FirstOrDefault(element => element.name == name);
        }

        public IEnumerable<T> Query<T>(string name) where T : VisualElement
        {
            return DescendantsAndSelf()
                .OfType<T>()
                .Where(element => element.name == name);
        }

        private IEnumerable<VisualElement> DescendantsAndSelf()
        {
            yield return this;

            foreach (var child in _children)
            {
                foreach (var descendant in child.DescendantsAndSelf())
                {
                    yield return descendant;
                }
            }
        }
    }

    public class Label : VisualElement
    {
        public Label(string name) : base(name) { }
    }

    public class ListView : VisualElement
    {
        public ListView(string name) : base(name) { }
    }

    public class Button : VisualElement
    {
        public Button(string name) : base(name) { }

        public event Action? clicked;
    }

    public class IntegerField : VisualElement
    {
        public IntegerField(string name) : base(name) { }

        public int value { get; set; }
    }

    public class Tab : VisualElement
    {
        public Tab(string name) : base(name) { }

        public event Action<Tab>? selected;
    }

    public class DropdownField : VisualElement
    {
        public DropdownField(string name) : base(name) { }

        public void RegisterValueChangedCallback(Action<ChangeEvent<string>> callback) { }
    }

    public class ChangeEvent<T>
    {
    }

    public class UIDocument
    {
        public UIDocument(VisualElement rootVisualElement)
        {
            this.rootVisualElement = rootVisualElement;
        }

        public VisualElement rootVisualElement { get; }
    }
}
