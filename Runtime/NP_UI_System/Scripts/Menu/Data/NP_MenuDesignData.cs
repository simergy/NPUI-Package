using System;
using DA_Assets.Extensions;
using NP_UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class NP_MenuDesignData : Singleton<NP_MenuDesignData>
{
    [SerializeField] private NP_Button _npButton;
    [SerializeField] private NP_Label _npLable;
    [SerializeField] private NP_GridLayout _npGridLayout;
    [SerializeField] private NP_InputField _npInputField;
    [SerializeField] private NP_Slider _npSlider;
    [SerializeField] private NP_CheckBox _checkBox;
    
    
    public NP_Button CreateButton()
    {
        return Instantiate(_npButton, Vector3.zero, Quaternion.identity);
    }
    
    public NP_Button CreateButton(UnityAction clickAction, string text)
    {
        NP_Button npButton = Instantiate(_npButton, Vector3.zero, Quaternion.identity);
        npButton.SetOnClick(clickAction);
        npButton.SetText(text);
        return npButton;
    }

    public NP_Button CreateButton(GenericUIData uiData)
    {
        NP_Button npButton = Instantiate(_npButton, Vector3.zero, Quaternion.identity);
        
        GenericUIData buttonData = uiData;
        bool isPictureButton;
        bool isTextButton;
        
        if (uiData is FormMenu.FormGenericUIData)
        {
            HandleValidatableButton(npButton,(FormMenu.ValidatableButtonData)uiData, out isPictureButton, out isTextButton);
        }
        else
        {
            HandleUIGenericButton(npButton, (ButtonData)uiData, out isPictureButton, out isTextButton);
        }
        

        return npButton;
    }

    private void HandleButtonAbilities(Sprite sprite, string text, Color color ,NP_Button npButton, bool isPictureButton, bool isTextButton)
    {
            if (isPictureButton)
            {
                HandlePictureButton(sprite, npButton);
            }
            if(isTextButton)
            {
                HandleTextButton(text, color, npButton);
            }
    }

    private void HandleUIGenericButton(NP_Button npButton, ButtonData buttonData, out bool isPictureButton, out bool isTextButton)
    {
        ButtonData genericButtonData = buttonData;
        npButton.SetOnClick(genericButtonData.ClickAction);
        isPictureButton = genericButtonData.MenuIcon != null;
        isTextButton = !genericButtonData.Text.IsEmpty();
        HandleButtonAbilities(genericButtonData.MenuIcon, genericButtonData.Text, genericButtonData.BackgroundColor, npButton, isPictureButton, isTextButton);
    }

    private void HandleValidatableButton(NP_Button npButton, FormMenu.ValidatableButtonData buttonData, out bool isPictureButton, out bool isTextButton)
    {
        npButton.SetOnClick(buttonData.ClickAction);
        isPictureButton = buttonData.MenuIcon != null;
        isTextButton = !buttonData.Text.IsEmpty();
        HandleButtonAbilities(buttonData.MenuIcon, buttonData.Text, buttonData.BackgroundColor, npButton, isPictureButton, isTextButton);
    }

    private void HandlePictureButton(Sprite menuIcon, NP_Button npButton)
    {
        npButton.SetBackgroundImage(menuIcon);
        npButton.SetText(String.Empty);
    }

    private void HandleTextButton(string textButton, Color color, NP_Button npButton)
    {
        npButton.SetText(textButton);
        if (color == new Color(0,0,0,0))
        {
            color = Color.white;
        }
        npButton.SetBackgroundColor(color);
    }

    public NP_Label CreateLabel(string text)
    {
        NP_Label npLable = Instantiate(_npLable, Vector3.zero, Quaternion.identity);
        npLable.SetText(text);
        return npLable;
    }

    public NP_Label CreateLabel(GenericUIData uiData)
    {
        NP_Label npLable = Instantiate(_npLable, Vector3.zero, Quaternion.identity);
        LabelData labelData = uiData as LabelData;
        if (labelData != null)
        {
            npLable.SetText(labelData.Text);
            npLable.SetSize(labelData.FontSize);
        }
        return npLable;
    }

    public NP_InputField CreateInputField(UnityAction<string> clickAction)
    {
         NP_InputField npInputField = Instantiate(_npInputField, Vector3.zero, Quaternion.identity);
         npInputField.AddListener(clickAction);
         return npInputField;
    }

    public NP_InputField CreateInputField(GenericUIData uiData)
    {
        NP_InputField npInputField = Instantiate(_npInputField, Vector3.zero, Quaternion.identity);

        if (uiData is FormMenu.ValidatableInputFieldData)
        {
            FormMenu.ValidatableInputFieldData inputFieldData = uiData as FormMenu.ValidatableInputFieldData;
            npInputField.AddListener(inputFieldData.OnValueChanged);
            npInputField.SetText(inputFieldData.Text);
            npInputField.SetDescription(inputFieldData.Description);
        }
        else
        {
            InputFieldData inputFieldData = uiData as InputFieldData;
            npInputField.AddListener(inputFieldData.OnValueChanged);
            npInputField.SetText(inputFieldData.Text);
        }

        return npInputField;
    }

    public NP_GridLayout CreateGridLayout()
    {
        return Instantiate(_npGridLayout, Vector3.zero, Quaternion.identity);
    }

    public NP_Slider CreateSlider(GenericUIData uiData)
    {
        SliderData sliderData = uiData as SliderData;
        NP_Slider npSlider = Instantiate(_npSlider, Vector3.zero, Quaternion.identity);
        npSlider.SetMaxVAlue(sliderData.MaxValue);
        npSlider.SetMinVAlue(sliderData.MinValue);
        npSlider.SetValue(sliderData.Value);
        npSlider.SetOnClick(sliderData.OnValueChanged);
        npSlider.SetWholeNumbers(sliderData.WholeNumber);
        return npSlider;
    }
    
    private NP_UIElements CreateCheckBox(GenericUIData uiData)
    {
        CheckBoxData checkBoxData = uiData as CheckBoxData;
        NP_CheckBox npCheckBox = Instantiate(_checkBox, Vector3.zero, Quaternion.identity);
        npCheckBox.SetOnValueChanged(checkBoxData.OnValueChanged);
        npCheckBox.SetTextPosition(checkBoxData._textPosition);
        npCheckBox.SetText(checkBoxData.Text);
        npCheckBox.OperateButton(checkBoxData.UseImageButton);
        npCheckBox.SetBackgroundImage(checkBoxData.ButtonImage);
        npCheckBox.SetImageButtonOnClick(checkBoxData.OnImageButtonClick);
        return npCheckBox;
    }
    
    public NP_UIElements CreateUIElementByData(GenericUIData uiData)
    {
        Type typeOfData = uiData.GetType();
        NP_UIElements npUIElements = null;
        if (typeOfData == typeof(InputFieldData) || typeOfData == typeof(FormMenu.ValidatableInputFieldData))
        {
            npUIElements = CreateInputField(uiData);
        }
        if (typeOfData == typeof(ButtonData) || typeOfData == typeof(FormMenu.ValidatableButtonData))
        {
            npUIElements = CreateButton(uiData);
        }
        if (typeOfData == typeof(LabelData) || typeOfData == typeof(FormMenu.ValidatableLabelData))
        {
            npUIElements = CreateLabel(uiData);
        }
        if (typeOfData == typeof(SliderData) || typeOfData == typeof(FormMenu.ValidatableSliderData))
        {
            npUIElements = CreateSlider(uiData);
        }
        if (typeOfData == typeof(CheckBoxData))
        {
            npUIElements = CreateCheckBox(uiData);
        }
        if (typeOfData == typeof(TabData))
        {
            npUIElements = CreateButton(uiData);
        }
        if (npUIElements != null)
        {
            npUIElements.ID = uiData.ID;
        }
        return npUIElements;
    }


}


