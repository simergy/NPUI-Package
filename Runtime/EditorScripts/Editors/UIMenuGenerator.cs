using System;
using UnityEngine;
using UnityEngine.UI;
using NP_UI;

/// <summary>
/// A static utility class responsible for programmatically generating and configuring 
/// complex UI menus within the Unity UI (uGUI) system.
/// </summary>
public static class UIMenuGenerator
{
    /// <summary>
    /// Bitwise flags used to define the anchor and pivot position of the menu on the screen.
    /// </summary>
    [Flags]
    public enum MenuAlignment
    {
        // --- INDEPENDENT FLAGS (MUST be powers of two) ---
        Top = 1 << 0,       // Binary: 0000000001 (Decimal: 1)
        Bottom = 1 << 1,    // Binary: 0000000010 (Decimal: 2)
        Left = 1 << 2,      // Binary: 0000000100 (Decimal: 4)
        Right = 1 << 3,     // Binary: 0000001000 (Decimal: 8)

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
        BottomCenter = Bottom | Center,   // (2 | 256) = 258 <- This is now a unique flag
        BottomRight = Bottom | Right,     // (2 | 8) = 10
    }

    /// <summary>
    /// Defines how elements inside the menu content area are ordered and how the scroll rect behaves.
    /// </summary>
    public enum GridLayoutType
    {
        Horizontal, // Items lay out horizontally, content scrolls horizontally
        Vertical,   // Items lay out vertically, content scrolls vertically
        Grid,        // Items lay out in a grid, content scrolls vertically
        Stretch
    }

    /// <summary>
    /// The primary entry point to generate a scrollable menu. 
    /// Handles hierarchy creation, layout component attachment, and resizing logic.
    /// </summary>
    /// <param name="config">A data container (MenuData) holding all style and behavior settings.</param>
    /// <returns>The generated NpGenericMenu component attached to the root object.</returns>
    public static NpGenericMenu CreateScrollableGridMenu(MenuData config)
    {
        float actualScreenCoveragePercent = config.ScreenCoveragePercent;
        // Validation ensures the menu isn't created with a 0% size or missing canvas
        if (!ValidateParameters(config.ParentCanvas, ref actualScreenCoveragePercent)) return null;

        GameObject rootMenuGO = CreateRootMenu(config);
        if (rootMenuGO == null) return null;

        RectTransform menuRootRect = rootMenuGO.GetComponent<RectTransform>();
        
        // Viewport acts as the "window" that masks the scrolling content
        RectTransform viewportRect = CreateViewport(rootMenuGO, config.ViewportBackgroundColor);
        
        // Content is the actual container that moves behind the viewport
        RectTransform contentRect = CreateContent(rootMenuGO.GetComponent<NP_Menu>(), viewportRect);

        AddContentSizeFitter(contentRect, config.LayoutType);

        NP_Menu npMenu = rootMenuGO.GetComponent<NP_Menu>();
        SetFields(config, npMenu);
        ApplyComponent(config, rootMenuGO);
        SetColors(config, npMenu);

        // Forces Unity to calculate the UI positions immediately rather than waiting for the next frame
        ForceRebuildLayouts(menuRootRect, viewportRect, contentRect);
        
        // Orchestrate the creation of resizing handles
        AddResizeHandles(rootMenuGO, config); 

        rootMenuGO.SetActive(config.IsAlwaysOn);
        return rootMenuGO.GetComponent<NpGenericMenu>();
    }

    // --- Modular Resizing Handle Logic ---

    /// <summary>
    /// Injects invisible UI handles at the edges of the menu to allow users to click and drag to resize.
    /// </summary>
    private static void AddResizeHandles(GameObject menuRoot, MenuData data)
    {
        if (!data.AllowResizeX && !data.AllowResizeY) return;

        // Create modular settings and callbacks
        ResizeSettings settings = CreateResizeSettings(data);
        Action<Vector2> onResizeAction = CreateResizeCallback(menuRoot, data);

        // Generate Edge Handles based on permissions
        if (data.AllowResizeX)
        {
            CreateEdgeHandle(menuRoot, settings, onResizeAction, true, true);  // Right
            CreateEdgeHandle(menuRoot, settings, onResizeAction, true, false); // Left
        }
        if (data.AllowResizeY)
        {
            CreateEdgeHandle(menuRoot, settings, onResizeAction, false, true);  // Top
            CreateEdgeHandle(menuRoot, settings, onResizeAction, false, false); // Bottom
        }
    }

    /// <summary>
    /// Creates a single RectTransform handle and attaches NP_MenuResizer to detect drag events.
    /// </summary>
    private static void CreateEdgeHandle(GameObject menuRoot, ResizeSettings globalSettings, Action<Vector2> callback, bool isVertical, bool isPositiveSide)
    {
        GameObject handleGO = new GameObject(isVertical ? "EdgeHandle_V" : "EdgeHandle_H");
        handleGO.transform.SetParent(menuRoot.transform, false);
        
        RectTransform rect = handleGO.AddComponent<RectTransform>();
        ConfigureHandleAnchors(rect, isVertical, isPositiveSide);

        // Create specific settings for this edge
        ResizeSettings edgeSettings = globalSettings; 
        edgeSettings.CanResizeRight = isVertical && isPositiveSide;
        edgeSettings.CanResizeLeft = isVertical && !isPositiveSide;
        edgeSettings.CanResizeTop = !isVertical && isPositiveSide;
        edgeSettings.CanResizeBottom = !isVertical && !isPositiveSide;

        // An alpha of 0 makes the handle invisible but still clickable (Raycast Target)
        handleGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var resizer = handleGO.AddComponent<NP_MenuResizer>();
        resizer.Setup(menuRoot.GetComponent<RectTransform>(), edgeSettings, callback);
    }

    /// <summary>
    /// Calculates the anchor and size of a resizing handle so it sits flush against a specific edge.
    /// </summary>
    private static void ConfigureHandleAnchors(RectTransform rect, bool isVertical, bool isPositiveSide)
    {
        if (isVertical) // Left/Right Edge
        {
            float x = isPositiveSide ? 1 : 0;
            rect.anchorMin = new Vector2(x, 0);
            rect.anchorMax = new Vector2(x, 1);
            rect.pivot = new Vector2(x, 0.5f);
            rect.sizeDelta = new Vector2(10, 0); // Grab area width
        }
        else // Top/Bottom Edge
        {
            float y = isPositiveSide ? 1 : 0;
            rect.anchorMin = new Vector2(0, y);
            rect.anchorMax = new Vector2(1, y);
            rect.pivot = new Vector2(0.5f, y);
            rect.sizeDelta = new Vector2(0, 10); // Grab area height
        }
    }

    /// <summary>
    /// Maps MenuData sizing limits to a ResizeSettings object for the NP_MenuResizer component.
    /// </summary>
    private static ResizeSettings CreateResizeSettings(MenuData data)
    {
        return new ResizeSettings
        {
            MinPercent = data.MinSizePercent,
            MaxPercent = data.MaxSizePercent
        };
    }

    /// <summary>
    /// Generates the Action delegate that runs whenever the menu is resized.
    /// </summary>
    private static Action<Vector2> CreateResizeCallback(GameObject menuRoot, MenuData data)
    {
        return (newSize) => 
        {
            NpGenericMenu menu = menuRoot.GetComponent<NpGenericMenu>();
            if (menu == null) return;

            // Conditional Cell Resizing: Makes grid items scale with the window
            GridLayoutGroup grid = menu.GetGridLayoutGroup();
            if (grid != null && data.ShouldResizeCells)
            {
                float paddingX = grid.padding.left + grid.padding.right;
                grid.cellSize = new Vector2(newSize.x - paddingX, grid.cellSize.y);
            }
            
            // Rebuilds accordion components to match new width
            NP_Accordion accordion = menuRoot.GetComponentInChildren<NP_Accordion>();
            accordion?.RebuildLayout();
        };
    }
    
    /// <summary>
    /// Applies the background color settings from the configuration to the menu component.
    /// </summary>
    private static void SetColors(MenuData config, NP_Menu menu)
    {
        if (menu == null)
        {
            return;
        }

        Color npColor = config.MenuBackgroundColor;
        menu.backgroundImage.color = new Color(npColor.r, npColor.g, npColor.b, npColor.a);
        
    }

    /// <summary>
    /// Updates the text and button visibility on the NP_Menu based on MenuData parameters.
    /// </summary>
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

    /// <summary>
    /// Dynamically attaches the specified script component to the menu root.
    /// </summary>
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

    /// <summary>
    /// Determines the specific MenuType (Regular, Form, Tabs) based on the ItemType provided in config.
    /// </summary>
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
                // Tab menus change visual orientation based on alignment
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
        if (!contentSizeFitter)
        {
            return;
        }
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
    /// Helper: Creates and configures a single Button item with Text (Debugging utility).
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
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");// Use a default Unity font
        }
    }

    /// <summary>
    /// Helper: Sets the RectTransform properties based on alignment and screen percentage.
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

