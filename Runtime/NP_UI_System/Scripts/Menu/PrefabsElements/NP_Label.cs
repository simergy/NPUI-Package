using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
// --- NP_Label ---
/// <summary>
/// Represents a UI Label element, capable of displaying text.
/// Implements ITextableElement interface.
/// </summary>
public class NP_Label : NP_UIElements, ITextableElement
{
    [SerializeField] private TextMeshProUGUI labelTextComponent; // Reference to Unity's Text component

    protected override void Awake()
    {
        base.Awake(); // Call base Awake to initialize uiRectTransform
        labelTextComponent = GetComponent<TextMeshProUGUI>();
        if (labelTextComponent == null)
        {
            Debug.LogError("NP_Label requires a Text component on its GameObject.", this);
        }
    }

    public string GetText()
    {
        if (labelTextComponent != null)
        {
            return labelTextComponent.text;
        }
        return string.Empty;
    }

    public void SetText(string text)
    {
        if (labelTextComponent != null)
        {
            labelTextComponent.text = text;
        }
    }

    public void SetBackgroundColor(Color color)
    {
        // For a label, you might be setting the color of a background Image component
        // or a sprite renderer if it's a sprite-based label.
        // This example assumes a Graphic component.
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.color = color;
        }
        else
        {
            Debug.LogWarning("NP_Label: No Graphic component found to set background color.", this);
        }
    }

    public void SetTextColor(Color color)
    {
        if (labelTextComponent != null)
        {
            labelTextComponent.color = color;
        }
    }

    public void SetBold(bool isBold)
    {
        if (labelTextComponent != null)
        {
            // This is a simplified example.
            // For true bolding, you might need to change the font asset or rich text tags.
            labelTextComponent.fontStyle = (FontStyles)(isBold ? FontStyle.Bold : FontStyle.Normal);
        }
    }

    public void SetFaceColor(Color color)
    {
        labelTextComponent.faceColor = color;
    }

    public void SetSize(int labelDataFontSize)
    {
        if (labelDataFontSize > -1)
        {
            labelTextComponent.enableAutoSizing = false;
            labelTextComponent.fontSize = labelDataFontSize;
        }
    }
}
