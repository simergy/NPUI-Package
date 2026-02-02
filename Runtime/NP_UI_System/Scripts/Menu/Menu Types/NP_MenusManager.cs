using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using NP_UI;
using System;

public class NP_MenusManager :  Singleton<NP_MenusManager> 
{
    public Transform menuContainer;
    public Transform iconsContainer;
    public RectTransform parentCanvas;
    
    // --- Private References to Generated Components ---
    private GameObject generatedMenuRoot;
    private ScrollRect generatedScrollRect;
    private RectTransform generatedContentViewport;
    private UnityEngine.UI.GridLayoutGroup generatedGridLayoutGroup; // Full namespace to avoid ambiguity
    

    [Header("GridLayout Settings")]
    public Vector2 itemCellSize;
    public Vector2 itemSpacing;
    public RectOffset gridPadding;
    public TextAnchor gridChildAlignment;
    private RectTransform _generatedContentViewport;


    private void Awake()
    {
        itemCellSize = new Vector2(100, 30);
        itemSpacing = new Vector2(5, 5);
        gridPadding = new RectOffset(10, 10, 10, 10);
        gridChildAlignment = TextAnchor.UpperLeft;
        NP_EventsManager.OpenMenuEvent.AddListener(OnOpenMenuEvent);
        NP_EventsManager.CloseMenuEvent.AddListener(OnCloseMenuEvent);
        NP_EventsManager.CloseMenuEvent.AddListener(OnCloseMenuEvent);

    }

    private void OnOpenMenuEvent(NpGenericMenu menu)
    {
        menu.OpenMenu();
    }

    private void OnCloseMenuEvent(NpGenericMenu menu)
    {
        menu.CloseMenu();
    }

    private void CloseAllMenus()
    {
        NP_EventsManager.CloseAllMenus.Invoke();
    }

    public NpGenericMenu AddMenu(NP_UIMenuData menuData)
    {
        NpGenericMenu genericMenu = GenerateMenu(menuData);
        //genericMenu.SetFields();
        //DesignMenu(genericMenu, menuData);
        genericMenu.transform.SetParent(menuContainer.transform, false);
        AddIconToMenusList(menuData, genericMenu);
        genericMenu.gameObject.SetActive(false);
        return genericMenu;
    }

    private NpGenericMenu CreateMenuOldVersion(NP_UIMenuData menuData)
    {
        NP_Menu npMenu = GetNewNpMenu(null, MenuType.Regular);//GenerateMenu(menuData);
        if (npMenu == null)
        {
            return null;
        }

        if (menuData.GenericMenuType.IsSubclassOf( typeof(NpGenericMenu)))
        {
            npMenu.gameObject.AddComponent(menuData.GenericMenuType);
        }
        else
        {
            Debug.LogError("menuData.GenericMenuType is not subclass of NP_GenericMenu");
            return null;
        }
        
        NpGenericMenu genericMenu = npMenu.gameObject.GetComponent<NpGenericMenu>();
        genericMenu.SetFields(npMenu);
        DesignMenu(genericMenu, menuData);
        genericMenu.transform.SetParent(menuContainer.transform, false);
        AddIconToMenusList(menuData, genericMenu);
        genericMenu.gameObject.SetActive(false);

        return genericMenu;
    }

    private void DesignMenu(NpGenericMenu npGenericMenu, NP_UIMenuData menuData)
    { 
        npGenericMenu.InitializeMenu(menuData);
    }

    private void AddIconToMenusList(NP_UIMenuData menuData, NpGenericMenu genericMenu)
    {
        GameObject iconGO = new GameObject(menuData.headLine);
        RawImage rawImageIcon = iconGO.AddComponent<RawImage>( );
        rawImageIcon.texture = menuData.menuIcon;
        iconGO.transform.SetParent(iconsContainer.transform, false);
        Button button = iconGO.AddComponent<Button>();
        button.onClick.AddListener(CloseAllMenus);
        button.onClick.AddListener(genericMenu.OpenMenu);
    }

    public NP_Menu GetNewNpMenu(MenuData menuData, MenuType menuType)
    {
        string menuPath = GetPathByType(menuType, menuData.TypeOfFormMenu);
        
        //Duplicate the correct prefab.
        GameObject menuReference = Resources.Load(menuPath) as GameObject;
        if (menuReference == null)
        {
            Debug.LogError(menuPath + " could not be found.\nMenu could not be found, please check your path. Menu did not created.");
            return null;
        }

        GameObject newMenuInScene = Instantiate(menuReference);
        newMenuInScene.name = menuData.MenuName;
        NP_Menu np_menu = newMenuInScene.GetComponent<NP_Menu>();
        if (np_menu == null)
        {
            Debug.LogError("Menu GameObject did not have a NP_GenericMenu component.");
            return null;
        }

        return np_menu;
    }
    private string GetPathByType(MenuType menuType, MenuFormType menuFormType)
    {
        string path = "";
        switch (menuType)
        {
            case MenuType.Regular:
                path = @"Menus\EmptyMenu";
                break;
            case MenuType.Form:
                path = @"Menus\EmptyForm";
                if(menuFormType == MenuFormType.BottomErrorsForm)
                    path = @"Menus\EmptyFormBottomLayout";
                break;
        }

        return path;
    }
    
    public NpGenericMenu GenerateMenu(NP_UIMenuData menuData)
    {
        // Basic validation
        if (parentCanvas == null)
        {
            Debug.LogError("DynamicMenuGenerator: 'Parent Canvas' is not assigned. Please assign your UI Canvas RectTransform.", this);
            return null;
        }
        

        // 1. Create the Root Menu GameObject
       GameObject _generatedMenuRoot = new GameObject(menuData.headLine);
        _generatedMenuRoot.transform.SetParent(parentCanvas, false); // false to reset local scale/position

        RectTransform menuRectTransform = _generatedMenuRoot.AddComponent<RectTransform>();
        _generatedMenuRoot.AddComponent<CanvasRenderer>(); // Needed for UI elements
        Image backgroundImage = _generatedMenuRoot.AddComponent<Image>(); // Add a background for visibility
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

        // 2. Apply positioning and scaling based on menuAlignment and percentOfScreenCoverage
        SetMenuRectTransform(menuRectTransform, menuData.alignmentType, menuData.percentOfScreen, parentCanvas);

        // Define the RectTransform that will hold the GridLayoutGroup (either menu root or scroll content)
        RectTransform layoutContentRectTransform = menuRectTransform;

        GridLayoutType menuLayoutType = GetGridLayoutType(menuData);
        var isScrollable = true;
        // 3. Add ScrollRect if required
        if (isScrollable)
        {
            ScrollRect _generatedScrollRect = _generatedMenuRoot.AddComponent<ScrollRect>();

            // Create Viewport GameObject
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(_generatedMenuRoot.transform, false);
            RectTransform viewportRectTransform = viewportGO.AddComponent<RectTransform>();
            Image viewportImage = viewportGO.AddComponent<Image>(); // Visual for viewport
            viewportImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Slightly lighter background
            viewportImage.raycastTarget = false; // Viewport image shouldn't block clicks

            // Set viewport to fill parent and mask content
            viewportRectTransform.anchorMin = Vector2.zero;
            viewportRectTransform.anchorMax = Vector2.one;
            viewportRectTransform.sizeDelta = Vector2.zero;
            viewportRectTransform.anchoredPosition = Vector2.zero;
            Mask viewportMask = viewportGO.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false; // We use the Image for visual, not the mask graphic itself

            // Create Content GameObject
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform _generatedContentViewport = contentGO.AddComponent<RectTransform>();
            _generatedContentViewport.anchorMin = new Vector2(0, 1); // Top-left pivot for content
            _generatedContentViewport.anchorMax = new Vector2(1, 1);
            _generatedContentViewport.pivot = new Vector2(0.5f, 1); // Center-top pivot
            _generatedContentViewport.anchoredPosition = Vector2.zero; // Reset content position

            // Configure ScrollRect
            _generatedScrollRect.content = _generatedContentViewport;
            _generatedScrollRect.viewport = viewportRectTransform;
            _generatedScrollRect.horizontal =  menuLayoutType == GridLayoutType.Horizontal;
            _generatedScrollRect.vertical = menuLayoutType == GridLayoutType.Vertical || menuLayoutType == GridLayoutType.Grid; // Grid can scroll vertically
            _generatedScrollRect.elasticity = 0.1f;
            _generatedScrollRect.scrollSensitivity = 10f;

            // Add ContentSizeFitter to the scroll content to adapt to items
            ContentSizeFitter contentSizeFitter = _generatedContentViewport.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = menuLayoutType == GridLayoutType.Horizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = menuLayoutType == GridLayoutType.Vertical || menuLayoutType == GridLayoutType.Grid ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

            layoutContentRectTransform = _generatedContentViewport; // GridLayoutGroup goes on this
        }
        

        // 5. Add GridLayoutGroup to the content area
        GridLayoutGroup _generatedGridLayoutGroup = layoutContentRectTransform.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        _generatedGridLayoutGroup.padding = gridPadding;
        _generatedGridLayoutGroup.spacing = itemSpacing;
        _generatedGridLayoutGroup.cellSize = itemCellSize;
        _generatedGridLayoutGroup.childAlignment = gridChildAlignment;

        switch (menuLayoutType)
        {
            case GridLayoutType.Horizontal:
                _generatedGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                break;
            case GridLayoutType.Vertical:
                _generatedGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                break;
            case GridLayoutType.Grid:
                _generatedGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal; // Start filling horizontally
                // You might add constraint property for grid layout, e.g., constraint = FixedColumnCount
                // _generatedGridLayoutGroup.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.Flexible;
                break;
        }

        // Force a layout rebuild to ensure everything positions correctly immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuRectTransform);
        if (_generatedContentViewport != null)
        {
             LayoutRebuilder.ForceRebuildLayoutImmediate(_generatedContentViewport);
        }

        return (NpGenericMenu)_generatedMenuRoot.AddComponent(menuData.GenericMenuType);
    }

    private GridLayoutType GetGridLayoutType(NP_UIMenuData item)
    {
        string stringType = item.alignmentType.ToString();
        if (stringType.Contains("Left") || stringType.Contains("Right"))
        {
            return GridLayoutType.Vertical;
        }

        return GridLayoutType.Horizontal;
    }
    /// <summary>
    /// Helper method to set the RectTransform properties based on alignment and screen percentage.
    /// </summary>
    private void SetMenuRectTransform(RectTransform rect, RectAlign alignment, float percent, RectTransform canvas)
    {
        rect.SetParent(canvas, false);
        rect.anchoredPosition = Vector2.zero; // Start centered and then adjust

        // Get actual dimensions of the parent canvas
        float canvasWidth = canvas.rect.width;
        float canvasHeight = canvas.rect.height;

        switch (alignment)
        {
            case RectAlign.TopLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.TopRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.BottomLeft:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.BottomRight:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case RectAlign.LeftPanel:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 1); // Stretches vertically
                rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0); // Width is percent, height stretches
                break;
            case RectAlign.RightPanel:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 1); // Stretches vertically
                rect.pivot = new Vector2(1, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0); // Width is percent, height stretches
                break;
            case RectAlign.TopPanel:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1); // Stretches horizontally
                rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent); // Height is percent, width stretches
                break;
            case RectAlign.BottomPanel:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0); // Stretches horizontally
                rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent); // Height is percent, width stretches
                break;
            case RectAlign.MiddleCenter:
            default:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
        }
    }
}
