using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
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
    [SerializeField] private NP_AccordionItem _npAccordionItem; 
    [SerializeField] private NP_Accordion _npAccordion; 
    
    
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
        
        // Removed unnecessary reassignment: GenericUIData buttonData = uiData;
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
            // Assuming SetSize sets font size or dimensions
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
    
    // NOTE: This factory method assumes the calling code (the Menu View/Controller) 
    // will set the parent transform of the returned NP_AccordionItem immediately.
    private NP_UIElements CreateAccordion(GenericUIData uiData)
    {
        bool useFirst = false;
        AccordionData rootAccordionData = uiData as AccordionData;
        
        if (rootAccordionData == null)
        {
            Debug.LogError("CreateAccordion received non-AccordionData. Returning null.");
            return null;
        }
        
        //Get the root enviroment
        NP_Accordion rootAccordionEnvironment = Instantiate(_npAccordion, Vector3.zero, Quaternion.identity);

        if (useFirst)
        {
            // 1. Instantiate the ROOT accordion item
            NP_AccordionItem rootAccordionItem = Instantiate(_npAccordionItem, Vector3.zero, Quaternion.identity);

            // 2. Configure the ROOT item with its data
            rootAccordionItem.SetText(rootAccordionData.Text);
            if (rootAccordionData.ClickAction != null)
            {
                rootAccordionItem.SetOnClick(rootAccordionData.ClickAction, isRemoveListeners: false);
            }

            // Note: SetParentData is usually null for the root item being created
            rootAccordionItem.SetChildrenData(rootAccordionData.AccordionChildren);
        }

        // 3. Initiate the recursive process for all children
        // The first call uses the root item as the parent, level 0.
        if (rootAccordionData.AccordionChildren != null && rootAccordionData.AccordionChildren.Count > 0)
        {
            // IMPORTANT: We pass the reference to the parent's content transform 
            // (which is the rootAccordionItem's transform in this flat hierarchy)
            RectTransform rectTransform = rootAccordionEnvironment.ContentTransform.GetComponent<RectTransform>();
            rectTransform.rect.Set(200,100,200,100);
            BuildAccordionChildren(rootAccordionData.AccordionChildren, 1, null, rootAccordionEnvironment, 0);
        }
        
        // 4. Initialize the root item (Level 0, Parent = null)
        // This must be done AFTER children are potentially created, or the children 
        // need to be initialized *after* this call. We initialize it now as the root.
        //rootAccordionItem.Initialize(null, 0, rootAccordionEnvironment.ContentTransform); 

        return rootAccordionEnvironment;
    }

    /// <summary>
    /// Recursively instantiates and wires child Accordion items into the hierarchy.
    /// </summary>
    /// <param name="childrenData">The list of AccordionData for the current level's children.</param>
    /// <param name="currentLevel">The hierarchy depth of the items being created.</param>
    /// <param name="parentItem">The instantiated NP_AccordionItem that will be the parent.</param>
    /// <param name="contentTransform">The Transform that serves as the layout container (the Scroll View Content).</param>
    /// <param name="i"></param>
    private void BuildAccordionChildren(List<AccordionData> childrenData, int currentLevel, NP_AccordionItem parentItem,
        NP_Accordion np_Accordion, int i)
    {
        if (childrenData == null || childrenData.Count == 0) return;

        foreach (AccordionData childData in childrenData)
        {
            // 1. Instantiate the CHILD item
            NP_AccordionItem childItem = Instantiate(_npAccordionItem, np_Accordion.ContentTransform);

            // 2. Configure the CHILD item with its data
            childItem.SetText(childData.Text);
            if (childData.ClickAction != null)
            {
                childItem.SetOnClick(childData.ClickAction, isRemoveListeners: false);
            }

            childItem.SetParentData(childData.ParentAccordion);
            childItem.SetChildrenData(childData.AccordionChildren);

            // 3. Wire the hierarchy: Initialize the child
            // This call: 
            // - Sets the level on the child.
            // - Calls parentItem.AddChild(childItem).
            // - parentItem.AddChild() uses SetSiblingIndex() to put the child visually 
            //   right after the parent in the VerticalLayoutGroup.
            childItem.Initialize(parentItem, currentLevel);
            childItem.ApplyElementsToAccordion(childData.UiElementsData);
            childData.SetValue(childItem);
            // 4. Recurse for the grandchildren
            if (childData.AccordionChildren != null && childData.AccordionChildren.Count > 0)
            {
                BuildAccordionChildren(childData.AccordionChildren, currentLevel + 1, childItem, np_Accordion, ++i);
            }
            childItem.SetAccordionContainer(np_Accordion);
        }
    }

    public NP_UIElements CreateUIElementByData(GenericUIData uiData)
    {
        Type typeOfData = uiData.GetType();
        NP_UIElements npUIElements = null;
        
        if (typeOfData == typeof(InputFieldData) || typeOfData == typeof(FormMenu.ValidatableInputFieldData))
        {
            npUIElements = CreateInputField(uiData);
        }
        else if (typeOfData == typeof(ButtonData) || typeOfData == typeof(FormMenu.ValidatableButtonData))
        {
            npUIElements = CreateButton(uiData);
        }
        else if (typeOfData == typeof(LabelData) || typeOfData == typeof(FormMenu.ValidatableLabelData))
        {
            npUIElements = CreateLabel(uiData);
        }
        else if (typeOfData == typeof(SliderData) || typeOfData == typeof(FormMenu.ValidatableSliderData))
        {
            npUIElements = CreateSlider(uiData);
        }
        else if (typeOfData == typeof(CheckBoxData))
        {
            npUIElements = CreateCheckBox(uiData);
        }
        else if (typeOfData == typeof(TabData))
        {
            npUIElements = CreateButton(uiData);
        }
        else if (typeOfData == typeof(AccordionData)) 
        {
            npUIElements = CreateAccordion(uiData);
        }
        
        if (npUIElements != null)
        {
            npUIElements.ID = uiData.ID;
        }
        return npUIElements;
    }
}