using NP_UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

public class NP_CheckBox : NP_UIElements, IImageableElement
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private TextMeshProUGUI _toggleTextRight;
    [SerializeField] private TextMeshProUGUI _toggleTextButtom;
    [SerializeField] public NP_Button _toggleButtonImage;


    private TextMeshProUGUI _toggleText;
    public void SetTextPosition(CheckBoxData.TextPosition position)
    {
        if (position == CheckBoxData.TextPosition.Right)
        {
            _toggleText = _toggleTextRight;
            _toggleText.enabled = true;
            _toggleTextButtom.enabled = false;
        }

        if (position == CheckBoxData.TextPosition.Bottom)
        {
            _toggleText = _toggleTextButtom;
            _toggleText.enabled = true;
            _toggleTextRight.enabled = false;
        }
    }

    public void SetImageButtonOnClick(UnityAction clickAction)
    {
        _toggleButtonImage.SetOnClick(clickAction);
    }
    public void OperateButton(bool isOperate)
    {
        _toggleButtonImage.gameObject.SetActive(isOperate);
    }
    
    public void SetOnValueChanged(UnityAction<bool> onValueChanged)
    {
        _toggle.onValueChanged.AddListener(onValueChanged);
    }

    public void RemoveListenersFromToggles()
    {
        _toggle.onValueChanged.RemoveAllListeners();
    }

    public void SetNormalColor(Color color)
    {
        ColorBlock newColorBlock = _toggle.colors;
        newColorBlock.normalColor = color;
        _toggle.colors = newColorBlock;
    }

    public void SetText(string text)
    {
        _toggleText.SetText(text);
    }

    public void SetColorBlock(ColorBlock newColorBlock)
    {
        _toggle.colors = newColorBlock;
    }

    public ColorBlock GetToggleColor()
    {
        if (_toggle != null)
        {
            return _toggle.colors;
        }
        Debug.LogError("_toggle is null");
        return new ColorBlock();
    }

    public bool GetValue()
    {
        if (_toggle != null)
        {
            return _toggle.isOn;
        }

        Debug.LogError("_toggle is null");
        return false;
    }

    public void SetBackgroundImage(Sprite spriteTexture)
    {
        _toggleButtonImage.SetBackgroundImage(spriteTexture);
    }
    
    public void SetToggleValue(bool isOn)
    {
        _toggle.isOn = isOn;
    }
}
