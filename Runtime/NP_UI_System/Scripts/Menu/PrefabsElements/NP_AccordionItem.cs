using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Represents an item within an accordion UI component.
/// Handles expansion, collapse, and hierarchy management of accordion items.
/// </summary>
public class NP_AccordionItem : NP_UIElements, ITextableElement, IClickableElement
{
    #region Fields
    [Header("References")]
    [SerializeField] private RectTransform header;
    [SerializeField] private Image arrowIcon;
    [SerializeField] private Text headerText;
    [SerializeField] private Transform itemContent;
    [SerializeField] private Button buttonHeader;
    [SerializeField] private Transform contentRawParent;


    [Header("Settings")]
    [SerializeField] private int level = 0;
    [SerializeField] private bool startExpanded = false;
    [SerializeField] private NP_AccordionItem parentAccordion;
    [SerializeField] public List<NP_AccordionItem> childAccordions = new List<NP_AccordionItem>();

    private static int Index = 0;

    private const float OffsetX = 30;

    private bool isExpanded;
    private Transform parentTransform;
    private List<AccordionData> accordionDataList;
    private AccordionData accordionDataParent;
    private RectTransform _headerRectTransform;
    private GridLayoutGroup _itemContentGrid;
    private Image _headerImage, _contentImage;
    private NP_Accordion _accordionContainer;
    #endregion

    #region  Initialisation
    /// <summary>
    /// Initializes the accordion item with parent, hierarchy level, and optional container.
    /// </summary>
    /// <param name="parent">The parent accordion item.</param>
    /// <param name="hierarchyLevel">The depth level in the hierarchy.</param>
    /// <param name="containerTransform">Optional container transform.</param>
    public void Initialize(NP_AccordionItem parent, int hierarchyLevel, Transform containerTransform = null)
    {
        parentAccordion = parent;
        level = hierarchyLevel;
        
        if (parent != null)
        {
            parent.AddChild(this);
        }
        else
        {
            transform.SetSiblingIndex(++Index);
        }
        
        
        isExpanded = startExpanded;

        AddListeners();
        
        if (contentRawParent != null)
        {
            RectTransform rectTransform = contentRawParent.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(rectTransform.anchoredPosition.x + OffsetX * level, rectTransform.offsetMin.y);
        }

        if (containerTransform != null)
        {
            parentTransform = containerTransform;
        }
        UpdateVisibility();
        UpdateArrowRotation();
        ExpandUIElementsPanel();

        FillFields();

    }

    public void SetAccordionContainer(NP_Accordion accordionContainer)
    {
        _accordionContainer = accordionContainer;
    }

    private void FillFields()
    {
        _itemContentGrid = itemContent.GetComponent<GridLayoutGroup>();
        _headerRectTransform = header.GetComponent<RectTransform>();
        _headerImage = header.GetComponent<Image>();
        _contentImage = itemContent.GetComponent<Image>();
    }
    private void Start()
    {
        if (parentAccordion == null && level == 0)
        {
            gameObject.SetActive(true);
            isExpanded = startExpanded;
            transform.SetParent(parentTransform);
            transform.SetSiblingIndex(0);
            UpdateArrowRotation();
        }
    }

    private void AddListeners()
    {
        // Get button if not initialized
        if (buttonHeader == null)
        {
            buttonHeader = header.GetComponent<Button>();
        }
        if (buttonHeader != null)
        {
            buttonHeader.onClick.AddListener(ToggleElementsAppearance);
        }
        
        // Get button and add click listener
        if (arrowIcon != null)
        {
            Button arrowIconButton = arrowIcon.GetComponent<Button>();
            if (arrowIconButton)
            {
                arrowIconButton.onClick.AddListener(Toggle);
            }
        }
    }
    #endregion

    #region Toggle

    private void ExpandUIElementsPanel()
    {
        itemContent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Adds a child accordion item to this item.
    /// </summary>
    /// <param name="child">The child item to add.</param>
    public void AddChild(NP_AccordionItem child)
    {
        if (!childAccordions.Contains(child))
        {
            childAccordions.Add(child);
            child.transform.SetSiblingIndex(++Index);
            print(child.headerText.text + Index.ToString());
        }
    }
    
    /// <summary>
    /// Toggles the expansion state of the accordion item.
    /// </summary>
    public void Toggle()
    {
        if (childAccordions.Count == 0)
        {
            UpdateColor();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_headerRectTransform);
            _accordionContainer.RebuildLayout();
            return;
        }
        
        isExpanded = !isExpanded;
        UpdateChildrenVisibility();
        UpdateArrowRotation();
        UpdateColor();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_headerRectTransform);
        _accordionContainer.RebuildLayout();
    }

    private void ToggleElementsAppearance()
    {
        if (itemContent.childCount == 0)
        {
            return;
        }
        itemContent.gameObject.SetActive(!itemContent.gameObject.activeSelf);
        if (itemContent.parent == contentRawParent)
        {
            PositionItemContent();
        }

        _accordionContainer.RebuildLayout();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_headerRectTransform);
    }

    private void PositionItemContent()
    {
        itemContent.SetParent(transform.parent);
        itemContent.SetSiblingIndex(transform.GetSiblingIndex()+1);
        GridLayoutGroup gridLayoutGroup = _itemContentGrid;
        gridLayoutGroup.padding.left = ((int)OffsetX-5)* level;
    }

    //To be used
    private void HandleColorsManagement(Transform contentTransform, Transform headerTransform)
    {
        if (contentTransform.gameObject.activeSelf)
        {
            _headerImage.color = Color.gray;
            _contentImage.color = Color.gray;
        }
        else
        {
            _headerImage.color = Color.white;
            _contentImage.color = Color.white;
        }
    }

    private void UpdateColor()
    {
        var colorBlock = buttonHeader.colors;
        if (parentAccordion != null)
        {
            colorBlock.normalColor = parentAccordion.buttonHeader.colors.highlightedColor;
        }
        else
        {
            colorBlock.normalColor = colorBlock.highlightedColor;
        }
        buttonHeader.colors = colorBlock;
    }

    private void UpdateChildrenVisibility()
    {
        foreach (NP_AccordionItem child in childAccordions)
        {
            if (child != null)
            {
                child.UpdateVisibility();
            }
        }
    }
    
    private void UpdateVisibility()
    {
        // Visible if parent is null (root level) or parent is expanded
        bool shouldBeVisible = parentAccordion == null || (parentAccordion.isExpanded && parentAccordion.gameObject.activeSelf);
        
        gameObject.SetActive(shouldBeVisible);
        
        // If we're being hidden, collapse and hide our children too
        if (!shouldBeVisible && isExpanded)
        {
            UpdateChildrenVisibility();
            isExpanded = !isExpanded;
            UpdateArrowRotation();
        }
    }
    
    private void UpdateArrowRotation()
    {
        if (arrowIcon != null)
        {
            // Hide arrow if no children
            if (accordionDataList == null || accordionDataList.Count == 0)
            {
                arrowIcon.enabled = false;
            }
            else
            {
                arrowIcon.enabled = true;
                arrowIcon.transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 0 : -90);
            }
        }
    }
    
    #endregion

    #region Layout Orgenizer
    private void AddElements(List<GenericUIData> dataElementsList)
    {
        if (dataElementsList == null || dataElementsList.Count == 0)
        {
            return;
        }
        foreach (GenericUIData dataElement in dataElementsList)
        {
            CreateElement(dataElement);
        }
    }
    private void AddElementLayoutGroup(NP_UIElements npElement)
    {
        if (npElement == null)
        {
            return;
        }

        LayoutElement layoutElement = npElement.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 30;
        layoutElement.preferredWidth = 150;
    }

    private void AddContentSizeFitterToElement(NP_UIElements npElement)
    {
        if (npElement == null)
        {
            return;
        }

        ContentSizeFitter contentSizeFitter = npElement.gameObject.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = npElement.gameObject.AddComponent<ContentSizeFitter>();
        }

        contentSizeFitter.enabled = true;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    

    #endregion

    #region API
    /// <summary>
    /// Sets the text of the header.
    /// </summary>
    /// <param name="text">The text to display.</param>
    public void SetText(string text)
    {
        headerText.text = text;
    }

    /// <summary>
    /// Gets the current header text.
    /// </summary>
    /// <returns>The header text.</returns>
    public string GetHeaderText()
    {
        return headerText.text;
    }

    /// <summary>
    /// Sets the parent accordion item.
    /// </summary>
    /// <param name="parent">The parent item.</param>
    public void SetParent(NP_AccordionItem parent)
    {
        parentAccordion = parent;
    }
    
    /// <summary>
    /// Gets a value indicating whether the item is expanded.
    /// </summary>
    public bool IsExpanded => isExpanded;

    /// <summary>
    /// Gets the hierarchy level of the item.
    /// </summary>
    public int Level => level;

    /// <summary>
    /// Gets the list of child accordion items.
    /// </summary>
    /// <returns>The list of children.</returns>
    public List<NP_AccordionItem> GetChildren() => childAccordions;

    /// <inheritdoc />
    public void SetBackgroundColor(Color color)
    {
        _headerImage.color = color;
    }

    /// <inheritdoc />
    public void SetTextColor(Color color)
    {
        headerText.color = color;
    }

    /// <inheritdoc />
    public void SetBold(bool isBold)
    {
        print("Unimplemented");
    }

    /// <summary>
    /// Sets the action to be performed on click.
    /// </summary>
    /// <param name="onClickAction">The action to execute.</param>
    public void SetOnClick(UnityAction onClickAction)
    {
        buttonHeader.onClick.AddListener(onClickAction);
    }
    
    /// <summary>
    /// Sets the action to be performed on click, optionally removing existing listeners.
    /// </summary>
    /// <param name="onClickAction">The action to execute.</param>
    /// <param name="isRemoveListeners">If true, removes existing listeners.</param>
    public void SetOnClick(UnityAction onClickAction, bool isRemoveListeners)
    {
        if (isRemoveListeners)
        {
            buttonHeader.onClick.RemoveAllListeners();
        }
        buttonHeader.onClick.AddListener(onClickAction);
    }

    /// <summary>
    /// Sets the data for the parent accordion.
    /// </summary>
    /// <param name="data">The parent data.</param>
    public void SetParentData(AccordionData data)
    {
        accordionDataParent = data;
    }
    
    /// <summary>
    /// Sets the data for the child accordions.
    /// </summary>
    /// <param name="children">The list of child data.</param>
    public void SetChildrenData(List<AccordionData> children)
    {
        accordionDataList  = new List<AccordionData>();
        if (children != null)
        {
            accordionDataList = children;
        }
    }

    /// <summary>
    /// Creates a UI element within the accordion content panel.
    /// </summary>
    /// <param name="dataElement">The data for the element to create.</param>
    /// <returns>The created UI element.</returns>
    public NP_UIElements CreateElement(GenericUIData dataElement)
    {
        NP_UIElements npElement = NP_MenuDesignData.Instance.CreateUIElementByData(dataElement);
        npElement.transform.SetParent(itemContent, false);
        AddContentSizeFitterToElement(npElement);
        AddElementLayoutGroup(npElement);
        return npElement;
    }

 

    /// <summary>
    /// Gets the grid layout group of the content panel.
    /// </summary>
    /// <returns>The GridLayoutGroup component.</returns>
    public VerticalLayoutGroup GetContentLayoutGroup()
    {
        return itemContent.GetComponent<VerticalLayoutGroup>();
    }
    
    /// <summary>
    /// Applies a list of UI elements to the accordion and its children recursively.
    /// </summary>
    /// <param name="dataElementsList">The list of data elements to apply.</param>
    public void ApplyElementsToAccordion(List<GenericUIData> dataElementsList)
    {
        bool isRootChildrenNull = childAccordions == null;
        if (isRootChildrenNull || childAccordions.Count == 0)
        {
            AddElements(dataElementsList);
            return;
        }
            
        foreach (AccordionData accordionData in accordionDataList)
        {
            NP_AccordionItem npAccordion = accordionData.GetUIElement() as NP_AccordionItem;
            npAccordion.ApplyElementsToAccordion(dataElementsList);
        }
        AddElements(dataElementsList);
    }

    
    /// <summary>
    /// Traverses the hierarchy starting from this item, executing an action on each item.
    /// </summary>
    /// <param name="action">The action to execute on each item.</param>
    public void TraverseHierarchy(UnityAction<NP_AccordionItem> action)
    {
        // 1. Execute the action on the current item (the starting point of the traversal).
        if (action != null)
        {
            action.Invoke(this);
        }

        // 2. Recursively call TraverseHierarchy on all children.
        foreach (NP_AccordionItem child in childAccordions)
        {
            if (child != null)
            {
                child.TraverseHierarchy(action);
            }
        }
    }
    #endregion

}