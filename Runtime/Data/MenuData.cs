using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

[Serializable]
public class MenuData : IEquatable<MenuData>
{

    //Don't reveal the ID so that users won't get confused.
    [HideInInspector]
    public string ID;
    [HideInInspector]
    public RectTransform ParentCanvas; // Assign in Inspector
    
    public SerializableType ItemType;
    public string MenuName = "New_Dynamic_Menu";
    public Sprite ItemIcon;
    public bool IsAlwaysOn;

    public MenuType MenuType;
    
    public UIMenuGenerator.MenuAlignment Alignment = UIMenuGenerator.MenuAlignment.RightSide;

    [Range(0.01f, 1.0f)] // Provides a slider in the Inspector
    public float ScreenCoveragePercent = 0.25f;

    public UIMenuGenerator.GridLayoutType LayoutType = UIMenuGenerator.GridLayoutType.Vertical;

    public Vector2 ItemCellSize = new Vector2(120, 40);
    public Vector2 ItemSpacing = new Vector2(10, 10);

    // RectOffset can typically be initialized directly as a field in a [Serializable] class.
    // If you encounter the "set_left" error again, you'd need to initialize this in the
    // OnValidate() or Awake() of the MonoBehaviour that holds MenuData.
    public RectOffset GridPadding = new RectOffset(15, 15, 15, 15);

    public TextAnchor GridChildAlignment = TextAnchor.UpperCenter;

    // --- New Fields for GridLayout Constraint ---
    public GridLayoutGroup.Constraint GridLayoutConstraint = GridLayoutGroup.Constraint.Flexible;
    [Range(1, 100)] // Add a range attribute for better Inspector usability
    public int GridLayoutConstraintCount = 1; // Default to 1 (e.g., one column/row)
    // ------------------------------------------
    
    public Color MenuBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color ViewportBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color ItemBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public Color ItemTextColor = Color.white;

    public bool IsBackgroundPanelVisible;
    // Ensure SerializableType is correctly implemented and [System.Serializable]


    // --- Static Readonly Fields for Default Values ---
    // NO LONGER directly initializing RectOffset here.
    // Instead, we store its individual integer components.
    public static readonly Vector2 DefaultItemCellSize = new Vector2(120, 40);
    public static readonly Vector2 DefaultItemSpacing = new Vector2(10, 10);

    // Changed these to individual int components
    public static readonly int DefaultGridPaddingLeft = 15;
    public static readonly int DefaultGridPaddingRight = 15;
    public static readonly int DefaultGridPaddingTop = 15;
    public static readonly int DefaultGridPaddingBottom = 15;

    public const TextAnchor DefaultGridChildAlignment = TextAnchor.UpperCenter;
    // --- New Static Defaults for GridLayout Constraint ---
    public static readonly GridLayoutGroup.Constraint DefaultGridLayoutConstraintStatic = GridLayoutGroup.Constraint.Flexible;
    public const int DefaultGridLayoutConstraintCountStatic = 1;
    // ---------------------------------------------------
    
    
    public static readonly Color DefaultMenuBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public static readonly Color DefaultViewportBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public static readonly Color DefaultItemBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public static readonly Color DefaultItemTextColor = Color.white;
    public static readonly SerializableType DefaultItemType = new SerializableType(typeof(UnityEngine.UI.Button));
    // Added a constant for ScreenCoveragePercent for default
    public const float DefaultScreenCoveragePercent = 0.25f;

    public MenuType TypeOfMenu; // General menu type (e.g., Regular, ScrollRect)
    public MenuFormType TypeOfFormMenu; // Specific form error handling type
    public bool UseEscapeButton;

    public MenuData(string menuName, SerializableType itemType, UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent, UIMenuGenerator.GridLayoutType layoutType,
        MenuType typeOfMenu, MenuFormType typeOfFormMenu) // NEW: Added typeOfFormMenu parameter
    {
        MenuName = menuName;
        ItemType = itemType;
        Alignment = alignment;
        ScreenCoveragePercent = screenCoveragePercent;
        LayoutType = layoutType;
        TypeOfMenu = typeOfMenu;
        TypeOfFormMenu = typeOfFormMenu; // Assign the new field
    }
    // =================================================================================================
    // --- CONSTRUCTORS (Overloaded) ---
    // Now using new RectOffset(...) within the constructor, not in static initialization.
    // =================================================================================================

    // --- NEW CONSTRUCTOR FOR WIZARD REGISTRATION ---
    /// <summary>
    /// Constructor for registering a new menu type in MenuCreator, setting key properties and defaulting others.
    /// ParentCanvas is set to null, as it will be assigned by the user in the Inspector later.
    /// </summary>
    public MenuData(
        string menuName, // The "English" name
        SerializableType itemType, // The Type of the new menu MonoBehaviour
        UIMenuGenerator.MenuAlignment alignment = DefaultMenuData.DefaultAlignment, // Provide a default if not given
        float screenCoveragePercent = DefaultMenuData.DefaultScreenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType = DefaultMenuData.DefaultLayoutType)
        : this(
            null, itemType, menuName, null,false, alignment, screenCoveragePercent, layoutType,
            DefaultItemCellSize,
            DefaultItemSpacing,
            // Create RectOffset here using the individual int components
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true,
            MenuType.Regular,
            MenuFormType.PerFieldErrorsType
        )
    {
    }

    // Add a helper class for default values in the new constructor for optional parameters
// This is often needed if you have parameters with no default in C# (like enums)
// or just to organize default values for specific constructors.
    public static class DefaultMenuData
    {
        public const UIMenuGenerator.MenuAlignment DefaultAlignment = UIMenuGenerator.MenuAlignment.Center;
        public const float DefaultScreenCoveragePercent = 0.5f;
        public const UIMenuGenerator.GridLayoutType DefaultLayoutType = UIMenuGenerator.GridLayoutType.Vertical;
        public const int DefaultItemCount = 10;
    }
    
    /// <summary>
    /// Primary (Comprehensive) Constructor: Initializes all properties of MenuData.
    /// All other constructors will chain to this one, providing default values for omitted parameters.
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        Vector2 itemCellSize,
        Vector2 itemSpacing,
        RectOffset gridPadding, // This is now directly taken as a parameter
        TextAnchor gridChildAlignment,
        GridLayoutGroup.Constraint gridLayoutConstraint,
        int gridLayoutConstraintCount,
        Color menuBackgroundColor,
        Color viewportBackgroundColor,
        Color itemBackgroundColor,
        Color itemTextColor, bool isBackgroundPanelVisible,
        MenuType typeOfMenu, MenuFormType typeOfFormMenu) // NEW: Added typeOfFormMenu parameter)
    {
        ParentCanvas = parentCanvas;
        ItemType = itemType; // <--- NEW ASSIGNMENT
        MenuName = menuName;
        Alignment = alignment;
        ScreenCoveragePercent = screenCoveragePercent;
        LayoutType = layoutType;

        ItemCellSize = itemCellSize;
        ItemSpacing = itemSpacing;
        GridPadding = gridPadding; // Assigning the passed-in RectOffset
        GridChildAlignment = gridChildAlignment;
        // --- New Field Initialization in Preset ---
        GridLayoutConstraint = gridLayoutConstraint;
        GridLayoutConstraintCount = gridLayoutConstraintCount;
        // ------------------------------------------
        MenuBackgroundColor = menuBackgroundColor;
        ViewportBackgroundColor = viewportBackgroundColor;
        ItemBackgroundColor = itemBackgroundColor;
        ItemTextColor = itemTextColor;
        IsBackgroundPanelVisible = isBackgroundPanelVisible;
        ID = menuName + "_" + itemType;
        TypeOfMenu = typeOfMenu;
        TypeOfFormMenu = typeOfFormMenu;
        UseEscapeButton = false;
    }

    /// <summary>
    /// Constructor: Initializes core layout parameters, with all other properties defaulting.
    /// Example usage: `new MenuData(canvas, "MyMenu", Alignment.RightSide, 0.3f, GridLayoutType.Vertical)`
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType)
        : this(
            parentCanvas, itemType, menuName, itemIcon,isAlwaysOn, alignment, screenCoveragePercent, layoutType,
            DefaultItemCellSize,
            DefaultItemSpacing,
            // Create RectOffset here using the individual int components
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true,
            MenuType.Regular,
            MenuFormType.PerFieldErrorsType
        )
    {
        ID = menuName + "_" + itemType;
    }

    /// <summary>
    /// Constructor: Initializes core layout, item count, and cell size, with other properties defaulting.
    /// Example usage: `new MenuData(canvas, "MyMenu", Alignment.RightSide, 0.3f, GridLayoutType.Vertical, 20, new Vector2(100, 50))`
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        Vector2 itemCellSize)
        : this(
            parentCanvas, itemType, menuName, itemIcon,isAlwaysOn, alignment, screenCoveragePercent, layoutType,
            itemCellSize, // Use provided itemCellSize
            DefaultItemSpacing,
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType)
    {
        ID = menuName + "_" + itemType;
    }

    /// <summary>
    /// Constructor: Initializes core layout, item count, cell size, and spacing, with other properties defaulting.
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        int itemCount,
        Vector2 itemCellSize,
        Vector2 itemSpacing)
        : this(
            parentCanvas, itemType, menuName,itemIcon, isAlwaysOn,alignment, screenCoveragePercent, layoutType,
            itemCellSize,
            itemSpacing, // Use provided itemSpacing
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType)
    {
        ID = menuName + "_" + itemType;
    }
    

    // --- Static Factory Methods (adjusting RectOffset initialization here too) ---

    /// <summary>
    /// Creates a default "Right Side Vertical" menu configuration.
    /// </summary>
    public static MenuData CreateRightSideVertical(RectTransform parentCanvas, SerializableType serializableType, string menuName = "RightSide_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.RightSide,
            screenCoveragePercent: DefaultScreenCoveragePercent, // Using DefaultScreenCoveragePercent
            layoutType: UIMenuGenerator.GridLayoutType.Vertical,
            itemCellSize: DefaultItemCellSize,
            itemSpacing: DefaultItemSpacing,
            gridPadding: new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop,
                DefaultGridPaddingBottom),
            gridChildAlignment: DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.1f, 0.1f, 0.3f, 0.9f), // Specific color overrides
            viewportBackgroundColor: new Color(0.2f, 0.2f, 0.4f, 0.7f),
            itemBackgroundColor: new Color(0.4f, 0.4f, 0.6f, 1.0f),
            itemTextColor: Color.white,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType);
    }

    /// <summary>
    /// Creates a default "Bottom Panel Horizontal" menu configuration.
    /// </summary>
    public static MenuData CreateBottomPanelHorizontal(RectTransform parentCanvas, SerializableType serializableType, string menuName = "BottomPanel_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.BottomPanel,
            screenCoveragePercent: 0.15f,
            layoutType: UIMenuGenerator.GridLayoutType.Horizontal,
            itemCellSize: new Vector2(100, 70), // Specific cell size
            itemSpacing: DefaultItemSpacing,
            gridPadding: new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            gridChildAlignment: TextAnchor.MiddleLeft, // Specific alignment
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.3f, 0.1f, 0.1f, 0.9f),
            viewportBackgroundColor: new Color(0.4f, 0.2f, 0.2f, 0.7f),
            itemBackgroundColor: new Color(0.6f, 0.4f, 0.4f, 1.0f),
            itemTextColor: Color.yellow,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType
        );
    }

    /// <summary>
    /// Creates a "Center Grid" menu configuration with configurable item count.
    /// </summary>
    public static MenuData CreateCenterGrid(RectTransform parentCanvas, SerializableType serializableType, string menuName = "Center_Grid_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.Center,
            screenCoveragePercent: 0.6f, // Larger central menu
            layoutType: UIMenuGenerator.GridLayoutType.Grid,
            itemCellSize: new Vector2(80, 80), // Specific cell size
            itemSpacing: new Vector2(15, 15), // Specific spacing
            gridPadding: new RectOffset(25, 25, 25, 25), // Specific padding
            gridChildAlignment: TextAnchor.MiddleCenter, // Specific alignment
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.1f, 0.3f, 0.1f, 0.9f),
            viewportBackgroundColor: new Color(0.2f, 0.4f, 0.2f, 0.7f),
            itemBackgroundColor: new Color(0.4f, 0.6f, 0.4f, 1.0f),
            itemTextColor: Color.white,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType
        );
    }

    public bool Equals(MenuData other)
    {
        if (other is null)
        {
            return false;
        }
        bool equalName = other.MenuName == MenuName;
        bool equalType = other.ItemType == ItemType.Type;
        bool equalID = other.ID == MenuName + "_" + ItemType;
        
        return equalName && equalType && equalID;
    }

    public override bool Equals(object other)
    {
        if (other is null)
        {
            return false;
        }

        bool equalType = other.GetType() == typeof(MenuData);
        bool equalID = Equals((MenuData)other);

        return equalType && equalID;
    }



    // Overloading of Binary "+" operator
    public static bool operator == (MenuData other1,  MenuData other2)
    {
        if (other1 is null && other2 is null)
        {
            return true;
        }
        if (other1 is null)
        {
            return false;
        }
        return other1.Equals(other2);
    }
    
    public static bool operator != (MenuData other1,  MenuData other2)
    {
        if (other1 is null && other2 is null)
        {
            return false;
        }
        if (other1 is null)
        {
            return true;
        }
        return !other1.Equals(other2);
    }

    public MenuData Clone()
    {
        MenuData CloneMenuData = new MenuData(this.MenuName, this.ItemType);
        FieldInfo[] currentFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (FieldInfo currentField in currentFields)
        {
            var value = currentField.GetValue(this);
            currentField.SetValue(CloneMenuData, value);
        }
        return CloneMenuData;
    }
}