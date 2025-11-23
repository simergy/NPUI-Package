using System;
using System.Collections.Generic;
using NP_UI;
using UnityEngine;
using UnityEngine.Events;

// Define the RectAlign enum/type if it doesn't exist elsewhere in your project.
// This is an example; adjust values as per your actual design.
    public enum RectAlign
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        LeftPanel, // Added for panel-style menus
        RightPanel,
        TopPanel,
        BottomPanel
    }

// Define the GridLayoutType enum/type if it doesn't exist elsewhere in your project.
// This is an example; adjust values as per your actual design.
    public enum GridLayoutType
    {
        Horizontal,
        Vertical,
        Grid
    }

    public enum MenuType
    {
        Regular,
        Form,
        Tabs
    }

    public enum MenuFormType
    {
        PerFieldErrorsType,
        BottomErrorsForm,
        Both
    }

    [Serializable]
    public class NP_UIMenuData
    {
        public MenuType menuType;
        public string headLine;
        public RectAlign alignmentType;
        public UnityEngine.Texture menuIcon;
        public float percentOfScreen;
        public GridLayoutType gridType;
        public bool isScrollRect;
        public bool isSelectButton;
        public Type GenericMenuType;
    }

    public abstract class GenericUIData : ISetterable
    {
        public string ID;

        protected NP_UIElements UIElement;

        public abstract NP_UIElements GetUIElement();

        public static List<GenericUIData> operator +(GenericUIData a, GenericUIData b)
        {
            List<GenericUIData> list = new List<GenericUIData>();
            if (a == null && b == null)
            {
                return list;
            }

            if (a != null)
            {
                list.Add(a);
            }

            if (b != null)
            {
                list.Add(b);
            }

            return list;
        }

        public static List<GenericUIData> operator +(List<GenericUIData> a, GenericUIData b)
        {
            if (a == null && b == null)
            {
                return new List<GenericUIData>();
            }

            if (a != null)
            {
                if (b != null)
                {
                    a.Add(b);
                }
            }

            return a;
        }

        public static List<GenericUIData> operator -(List<GenericUIData> a, GenericUIData b)
        {
            if (a == null && b == null)
            {
                return a;
            }

            if (a != null)
            {
                if (b != null)
                {
                    if (a.Contains(b))
                    {
                        a.Remove(b);
                    }
                }
            }

            return a;
        }

        public void SetValue(object targetObject)
        {
            UIElement = targetObject as NP_UIElements;
        }
    }

    public interface ISetterable
    {
        public void SetValue(object targetObject);
    }


    public class ButtonData : GenericUIData
    {
        public UnityAction ClickAction;
        public string Text;
        public UnityEngine.Color BackgroundColor;
        public UnityEngine.Sprite MenuIcon;

        public ButtonData(UnityAction onClick, string textButton)
        {
            ClickAction = onClick;
            Text = textButton;
            MenuIcon = null;
        }

        public ButtonData(UnityAction onClick, UnityEngine.Sprite menuIcon)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = String.Empty;
        }

        public ButtonData(UnityAction onClick, UnityEngine.Sprite menuIcon, string text)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = text;
        }

        public ButtonData(UnityAction onClick, string text, UnityEngine.Color backgroundColor)
        {
            ClickAction = onClick;
            MenuIcon = null;
            Text = text;
            BackgroundColor = backgroundColor;
        }

        protected ButtonData()
        {
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }

    public class TabData : ButtonData
    {
        public MenuData NpMenuData;
        public Type TypeOfMenu;
        public UnityAction ClickAction;
        public string Text;
        public UnityEngine.Color BackgroundColor;
        public UnityEngine.Sprite MenuIcon;

        public TabData (UnityAction onClick, string textButton) : base(onClick, textButton)
        {
            ClickAction = onClick;
            Text = textButton;
            MenuIcon = null;
        }
        public TabData (MenuData menuData, string textButton)
        {
            Text = textButton;
            NpMenuData = menuData;
        }
        public TabData (Type typeOfMenu, string textButton, UnityAction onClick)
        {
            Text = textButton;
            TypeOfMenu = typeOfMenu;
            ClickAction = onClick;
        }

        public TabData (UnityAction onClick, UnityEngine.Sprite menuIcon) : base(onClick, menuIcon)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = String.Empty;
        }

        public TabData (UnityAction onClick, UnityEngine.Sprite menuIcon, string text) : base(onClick, menuIcon, text)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = text;
        }

        public TabData (UnityAction onClick, string text, UnityEngine.Color backgroundColor) : base(onClick, text, backgroundColor)
        {
            ClickAction = onClick;
            MenuIcon = null;
            Text = text;
            BackgroundColor = backgroundColor;
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }

    public class LabelData : GenericUIData
    {
        public string Text;
        public int FontSize;

        public LabelData(string text, int fontSize = -1)
        {
            Text = text;
            FontSize = fontSize;
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }

    public class InputFieldData : GenericUIData
    {
        public UnityAction<string> OnValueChanged;
        public UnityAction<bool> OnValidate; //TODO::
        public string Text;

        public InputFieldData()
        {
            OnValueChanged = null;
            Text = "";
        }

        public InputFieldData(UnityAction<string> onValueChanged)
        {
            OnValueChanged = onValueChanged;
            Text = "";
        }

        public InputFieldData(string text)
        {
            OnValueChanged = null;
            Text = text;
        }

        public InputFieldData(UnityAction<string> onValueChanged, string text)
        {
            OnValueChanged = onValueChanged;
            Text = text;
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }

    public class CheckBoxData : GenericUIData
    {
        public enum TextPosition
        {
            Bottom, 
            Right
        }
        
        public UnityAction<bool> OnValueChanged;
        public string Text;
        public TextPosition _textPosition;
        public bool UseImageButton;
        public UnityEngine.Sprite ButtonImage;
        public UnityAction OnImageButtonClick;
        

        public CheckBoxData(TextPosition textPosition, bool useImageButton = false, Sprite buttonImage = null, UnityAction onImageButtonClick = null)
        {
            OnValueChanged = null;
            Text = "";
            UseImageButton = useImageButton;
            _textPosition = textPosition;
            ButtonImage = buttonImage;
            OnImageButtonClick = onImageButtonClick;
        }

        public CheckBoxData(UnityAction<bool> onValueChanged, TextPosition textPosition, bool useImageButton = false, Sprite buttonImage = null, UnityAction onImageButtonClick = null)
        {
            OnValueChanged = onValueChanged;
            Text = "";
            _textPosition = textPosition;
            UseImageButton = useImageButton;
            ButtonImage = buttonImage;
            OnImageButtonClick = onImageButtonClick;
        }

        public CheckBoxData(string text, TextPosition textPosition, bool useImageButton = false, Sprite buttonImage = null, UnityAction onImageButtonClick = null)
        {
            OnValueChanged = null;
            Text = text;
            _textPosition = textPosition;
            UseImageButton = useImageButton;
            ButtonImage = buttonImage;
            OnImageButtonClick = onImageButtonClick;
        }

        public CheckBoxData(UnityAction<bool> onValueChanged, string text, TextPosition textPosition, bool useImageButton = false, Sprite buttonImage = null, UnityAction onImageButtonClick = null)
        {
            OnValueChanged = onValueChanged;
            Text = text;
            _textPosition = textPosition;
            UseImageButton = useImageButton;
            ButtonImage = buttonImage;
            OnImageButtonClick = onImageButtonClick;
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }

    public class SliderData : GenericUIData
    {
        public UnityAction<float> OnValueChanged;
        public string Text;
        public float Value;
        public float MinValue;
        public float MaxValue;
        public bool WholeNumber;

        public SliderData(float value, float minValue, float maxValue, UnityAction<float> onValueChanged,
            bool wholeNumber = true)
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue; //Max Value Can't be null
            WholeNumber = wholeNumber;
            OnValueChanged = onValueChanged;
            Text = "";
        }

        public SliderData(float value, float minValue, float maxValue, bool wholeNumber = true)
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue; //Max Value Can't be null
            WholeNumber = wholeNumber;
            OnValueChanged = (x) => { };
            Text = "";
        }

        public SliderData(UnityAction<float> onValueChanged)
        {
            OnValueChanged = onValueChanged;
            Text = "";
        }

        public SliderData(string text)
        {
            OnValueChanged = null;
            Text = text;
        }

        public SliderData(UnityAction<float> onValueChanged, string text)
        {
            OnValueChanged = onValueChanged;
            Text = text;
        }

        public void SetValue(float initialValue)
        {

        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement;
        }
    }