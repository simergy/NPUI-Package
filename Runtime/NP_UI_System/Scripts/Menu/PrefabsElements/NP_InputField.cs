using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NP_InputField : NP_UIElements, ITextableElement
{
    [SerializeField] private TextMeshProUGUI textLabel;
    [SerializeField] private TextMeshProUGUI errorLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private TMP_InputField inputField;
    
    public void SetText(string text)
    {
        inputField.text = text;
    }

    public string GetValueText()
    {
        return inputField.text;
    }

    public void AddListener(UnityAction<string> listener)
    {
        if (listener == null)
        {
            Debug.LogWarning("[NP_InputField] Listener is null");
            return;
        }
        inputField.onValueChanged.AddListener(listener);
    }

    public void SetErrorMessage(string error)
    {
        errorLabel.text = error;
    }

    public void SetDescription(string description)
    {
        descriptionLabel.text = description;
    }
    
    public void SetBackgroundColor(Color color)
    {
        
    }

    public void SetTextColor(Color color)
    {
        throw new System.NotImplementedException();
    }

    public void SetBold(bool isBold)
    {
        throw new System.NotImplementedException();
    }
}
