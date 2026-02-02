using UnityEngine;
using UnityEngine.Events; // For Action, which is used in IClickableElement

// --- Interfaces ---

/// <summary>
/// Defines common text manipulation functionalities for UI elements.
/// </summary>
public interface ITextableElement
{
    void SetText(string text);
    void SetBackgroundColor(Color color);
    void SetTextColor(Color color);
    void SetBold(bool isBold);
}

/// <summary>
/// Defines common clickable functionalities for UI elements.
/// </summary>
public interface IClickableElement
{
    // Using System.Action for a simple callback when the element is clicked.
    void SetOnClick(UnityAction onClickAction);
}

/// <summary>
/// Defines common image manipulation functionalities for UI elements.
/// </summary>
public interface IImageableElement
{
    // RawImage is a Unity UI component that can display a Texture.
    // If you're using Sprite, you might adjust this to Sprite.
    void SetBackgroundImage(Sprite spriteTexture);
}

// --- Base Class ---

/// <summary>
/// Base class for all custom UI elements, inheriting from MonoBehaviour.
/// Provides common Unity lifecycle methods and utility.
/// </summary>
public class NP_UIElements : MonoBehaviour
{
    public string ID;

    public Vector2 size;
    // A reference to the base UIElements component.
    // This could be used to cache references to common UI components like RectTransform.
    protected RectTransform uiRectTransform; // Common for all UI elements

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Good for initializing references.
    /// </summary>
    protected virtual void Awake()
    {
        // Example: Get a reference to this GameObject's RectTransform
        uiRectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Utility method to get a component of type T.
    /// This might be redundant as GetComponent<T> is already available,
    /// but it shows intent from the diagram.
    /// </summary>
    /// <typeparam name="T">The type of the component to get.</typeparam>
    /// <returns>The component if found, otherwise null.</returns>
    public T GetUnityComponent<T>() where T : Component
    {
        return GetComponent<T>();
    }

    public void SetSize()
    {
        uiRectTransform.sizeDelta = size;
    }
}

// --- NP_GridLayout ---
// Define these enums/types according to your project's needs.
// These are placeholders based on common Unity UI patterns.
public enum LayoutDirection
{
    Horizontal,
    Vertical
}

//public enum GridLayoutGroup
//{
//    None, // If it's just a generic group
//    MainMenu,
//    SubMenu,
//    InventoryGrid
//    // Add more specific group types as needed
//}