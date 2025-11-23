using System;
using Codice.CM.Common;
using UnityEngine;
using UnityEngine.UI;
using NP_UI;
using PlasticGui;

// This is a static class, meaning you can call its methods directly without creating an instance.
public static class UIMenuGenerator
{
    [Flags]
    public enum MenuAlignment
    {
        // 0 is often reserved for None, but can be omitted if you have no "None" state
    
        // --- INDEPENDENT FLAGS (MUST be powers of two) ---
        Top = 1 << 0,       // 1
        Bottom = 1 << 1,    // 2
        Left = 1 << 2,      // 4  <- This is now a unique flag
        Right = 1 << 3,     // 8

        LeftSide = 1 << 4,  // 16
        RightSide = 1 << 5, // 32
        TopPanel = 1 << 6,  // 64 <- This is now a unique flag
        BottomPanel = 1 << 7, // 128
    
        Center = 1 << 8,    // 256
        Stretch = 1 << 9,   // 512
    
        // --- COMBINATION FLAGS (Combine the independent ones using the OR operator |) ---
        TopLeft = Top | Left,             // (1 | 4) = 5
        TopCenter = Top | Center,         // (1 | 256) = 257
        TopRight = Top | Right,           // (1 | 8) = 9
    
        BottomLeft = Bottom | Left,       // (2 | 4) = 6
        BottomCenter = Bottom | Center,   // (2 | 256) = 258 <- This is now a unique combination
        BottomRight = Bottom | Right,     // (2 | 8) = 10
    }

    public enum GridLayoutType
    {
        Horizontal, // Items lay out horizontally, content scrolls horizontally
        Vertical,   // Items lay out vertically, content scrolls vertically
        Grid,        // Items lay out in a grid, content scrolls vertically
        Stretch
    }

    /// <summary>
    /// Generates a scrollable UI menu with a GridLayoutGroup purely from code,
    /// using a MenuData struct/class for all parameters.
    /// </summary>
    /// <param name="config">The MenuData struct/class containing all necessary parameters.</param>
    /// <returns>The root GameObject of the generated menu, or null if creation fails.</returns>
    public static NpGenericMenu CreateScrollableGridMenu(MenuData config)
    {
        // --- 1. Validate parameters ---
        float actualScreenCoveragePercent = config.ScreenCoveragePercent; // Use a local variable to allow modification
        if (!ValidateParameters(config.ParentCanvas, ref actualScreenCoveragePercent)) return null;

        // --- 2. Create Root Menu GameObject ---
        GameObject rootMenuGO = CreateRootMenu(config);

        if (rootMenuGO == null) return null;
        RectTransform menuRootRect = rootMenuGO.GetComponent<RectTransform>();

        // --- 3. Create Viewport and Content GameObjects ---
        RectTransform viewportRect = CreateViewport(rootMenuGO, config.ViewportBackgroundColor);
        RectTransform contentRect = CreateContent(rootMenuGO.GetComponent<NP_Menu>(), viewportRect);

        // --- 4. Add and Configure ScrollRect ---
        ScrollRect
            scrollRect =
                rootMenuGO
                    .GetComponentInChildren<
                        ScrollRect>(); //AddScrollRect(rootMenuGO, viewportRect, contentRect, config.LayoutType);

        NP_Menu npMenu = rootMenuGO.GetComponent<NP_Menu>();

        // --- 5. Add and Configure ContentSizeFitter ---
        AddContentSizeFitter(contentRect, config.LayoutType);

        // --- 6. Add and Configure GridLayoutGroup ---
        AddGridLayoutGroup(config, contentRect);

        //Just For Example!
        // --- 7. Populate with example items ---
        //PopulateMenuItems(contentRect, config.ItemCount, config.ItemBackgroundColor, config.ItemTextColor);
        SetFields(config, npMenu);
        ApplyComponent(config, rootMenuGO);
        SetColors(config, npMenu);
        // --- 8. Force Layout Rebuilds ---
        ForceRebuildLayouts(menuRootRect, viewportRect, contentRect);

        rootMenuGO.SetActive(config.IsAlwaysOn);
        
        return rootMenuGO.GetComponent<NpGenericMenu>();
    }

    private static void SetColors(MenuData config, NP_Menu menu)
    {
        if (menu == null)
        {
            return;
        }

        Color npColor = config.MenuBackgroundColor;
        menu.backgroundImage.color = new Color(npColor.r, npColor.g, npColor.b, npColor.a);
        
    }
    private static void SetFields(MenuData config, NP_Menu menu)
    {
        if (menu == null)
        {
            return;
        }
        menu.headLineText.SetText(config.MenuName);
        menu.menuData = config;
        menu.EscapeButton.gameObject.SetActive(config.UseEscapeButton);
    }

    private static void ApplyComponent(MenuData config, GameObject rootMenuGo)
    {
        rootMenuGo.AddComponent(config.ItemType);
    }
    
    // =================================================================================================
    // --- PRIVATE STATIC HELPER FUNCTIONS
    // =================================================================================================

    /// <summary>
    /// Helper: Validates essential parameters and clamps values if necessary.
    /// </summary>
    private static bool ValidateParameters(RectTransform parentCanvas, ref float screenCoveragePercent)
    {
        if (parentCanvas == null)
        {
            Debug.LogError("UIMenuGenerator: 'parentCanvas' is null. Cannot create menu.");
            return false;
        }
        if (screenCoveragePercent <= 0 || screenCoveragePercent > 1)
        {
            Debug.LogWarning($"UIMenuGenerator: 'screenCoveragePercent' should be between 0 and 1. Clamping to 0.3. Current: {screenCoveragePercent}");
            screenCoveragePercent = Mathf.Clamp(screenCoveragePercent, 0.01f, 1f);
        }
        return true;
    }

    /// <summary>
    /// Helper: Creates the root GameObject for the menu and sets its basic properties.
    /// </summary>
    private static GameObject CreateRootMenu(MenuData menuData)
    {
        GameObject rootGO = CreateNewRootMenuGameObject(menuData); 
        
        rootGO.transform.SetParent(menuData.ParentCanvas, false);

        RectTransform rootRect = rootGO.GetComponent<RectTransform>();

        SetRectTransformProperties(rootRect, menuData.Alignment, menuData.ScreenCoveragePercent, menuData.ParentCanvas);
        return rootGO;
    }

    private static GameObject CreateNewRootMenuGameObject(MenuData config)
    {
        Type type = config.ItemType.Type;
        
        bool isSubclassOfNPGeneric = type.IsSubclassOf(typeof(NpGenericMenu));
        bool isSubclassOfFormMenu = type.IsSubclassOf(typeof(FormMenu));
        bool isSubclassOfTabsMenu = type.IsSubclassOf(typeof(TabsMenu));

        MenuType menuType = MenuType.Regular;

        if (isSubclassOfNPGeneric)
        {
            menuType = MenuType.Regular;

            if (isSubclassOfFormMenu)
            {
                menuType = MenuType.Form;
            }

            if (isSubclassOfTabsMenu)
            {
                switch (config.Alignment)
                {
                    case MenuAlignment.Left:
                    case MenuAlignment.LeftSide:
                        menuType = MenuType.TabsLeft;
                        break;
                    case MenuAlignment.Right:
                    case MenuAlignment.RightSide:
                        menuType = MenuType.TabsRight;
                        break;
                    case MenuAlignment.TopPanel:
                        menuType = MenuType.TabsUp;
                        break;
                    case MenuAlignment.BottomPanel:
                        menuType = MenuType.TabsDown;
                        break;
                }
            }
        }        
        
        return NP_MenusManager.Instance.GetNewNpMenu(config, menuType).gameObject;
    }
    
    /// <summary>
    /// Helper: Creates and configures the ScrollRect's Viewport GameObject.
    /// </summary>
    private static RectTransform CreateViewport(GameObject parentGO, Color bgColor)
    {
        NP_Menu npMenu = parentGO.GetComponent<NP_Menu>();
        GameObject viewportGO = npMenu.viewPort;

        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        Image viewportImage = viewportGO.GetComponent<Image>();
        //viewportImage.color = bgColor;
        viewportImage.raycastTarget = false;

        Mask viewportMask = viewportGO.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        LayoutElement viewportLayoutElement = viewportGO.AddComponent<LayoutElement>();
        viewportLayoutElement.flexibleHeight = 1;
        viewportLayoutElement.flexibleWidth = 1; // Crucial for Viewport to fill parent

        return viewportRect;
    }

    /// <summary>
    /// Helper: Creates and configures the ScrollRect's Content GameObject.
    /// </summary>
    private static RectTransform CreateContent(NP_Menu npMenu, RectTransform viewportRect)
    {
        GameObject contentGO = npMenu.scrollRect.content.gameObject;
        contentGO.transform.SetParent(viewportRect.transform, false);

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        return contentRect;
    }

    /// <summary>
    /// Helper: Adds and configures the ScrollRect component.
    /// </summary>
    private static ScrollRect AddScrollRect(
        GameObject rootGO, RectTransform viewportRect, RectTransform contentRect,
        GridLayoutType layoutType)
    {
        ScrollRect scrollRect = rootGO.AddComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = (layoutType == GridLayoutType.Horizontal);
        scrollRect.vertical = (layoutType == GridLayoutType.Vertical || layoutType == GridLayoutType.Grid);
        scrollRect.elasticity = 0.1f;
        scrollRect.scrollSensitivity = 10f;
        return scrollRect;
    }

    /// <summary>
    /// Helper: Adds and configures the ContentSizeFitter component.
    /// </summary>
    private static void AddContentSizeFitter(RectTransform targetRect, GridLayoutType layoutType)
    {
        ContentSizeFitter contentSizeFitter = targetRect.gameObject.GetComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = (layoutType == GridLayoutType.Horizontal || layoutType == GridLayoutType.Grid) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = (layoutType == GridLayoutType.Vertical || layoutType == GridLayoutType.Grid) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
    }

    /// <summary>
    /// Helper: Adds and configures the GridLayoutGroup component.
    /// </summary>
    private static void AddGridLayoutGroup(MenuData menuData, 
        RectTransform targetRect)
    {
        GridLayoutGroup gridLayoutGroup = targetRect.gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.padding = menuData.GridPadding;
        gridLayoutGroup.spacing = menuData.ItemSpacing;
        gridLayoutGroup.cellSize = menuData.ItemCellSize;
        gridLayoutGroup.childAlignment = menuData.GridChildAlignment;
        gridLayoutGroup.constraint = menuData.GridLayoutConstraint;
        gridLayoutGroup.constraintCount = menuData.GridLayoutConstraintCount;
        
        switch (menuData.LayoutType)
        {
            case GridLayoutType.Horizontal:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                break;
            case GridLayoutType.Vertical:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                break;
            case GridLayoutType.Grid:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal; // Grid usually flows horizontally then wraps vertically
                break;
        }
    }

    /// <summary>
    /// Helper: Creates and configures a single Button item with Text.
    /// </summary>
    private static void PopulateMenuItems(Transform contentParent, int count, Color itemBackgroundColor, Color itemTextColor)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject itemGO = new GameObject("Button_Item_" + (i + 1));
            itemGO.transform.SetParent(contentParent, false);

            itemGO.AddComponent<RectTransform>(); // GridLayoutGroup will manage its size

            Image itemImage = itemGO.AddComponent<Image>();
            itemImage.color = itemBackgroundColor;

            Button buttonComponent = itemGO.AddComponent<Button>();
            int itemIndex = i + 1; // Capture for lambda
            buttonComponent.onClick.AddListener(() => Debug.Log($"Button {itemIndex} Clicked!"));

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(itemGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = "Item " + (i + 1);
            textComponent.color = itemTextColor;
            textComponent.fontSize = 20;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Use a default Unity font
        }
    }

    /// <summary>
    /// Helper: Sets the RectTransform properties of a UI element based on alignment and screen percentage.
    /// </summary>
    private static void SetRectTransformProperties(RectTransform rect, MenuAlignment alignment, float percent, RectTransform canvas)
    {
        rect.anchoredPosition = Vector2.zero;

        float canvasWidth = canvas.rect.width;
        float canvasHeight = canvas.rect.height;

        switch (alignment)
        {
            case MenuAlignment.TopLeft:
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1); rect.anchorMax = new Vector2(0.5f, 1); rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.TopRight:
                rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomLeft:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 0); rect.pivot = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0); rect.anchorMax = new Vector2(0.5f, 0); rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomRight:
                rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(1, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.LeftSide:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0);
                break;
            case MenuAlignment.RightSide:
                rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0);
                break;
            case MenuAlignment.TopPanel:
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent);
                break;
            case MenuAlignment.BottomPanel:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent);
                break;
            case MenuAlignment.Center:
            default:
                rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
        }
    }

    /// <summary>
    /// Helper: Forces an immediate layout rebuild for multiple RectTransforms.
    /// </summary>
    private static void ForceRebuildLayouts(params RectTransform[] rects)
    {
        foreach (RectTransform rect in rects)
        {
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }
    }
}

