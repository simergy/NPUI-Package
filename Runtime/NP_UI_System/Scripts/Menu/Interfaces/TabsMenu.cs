using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NP_UI
{
    public class TabsMenu : NpGenericMenu
    {
        protected NP_TabsMenu tabsMenu;
        protected MenuData currentMenu;
        private NpGenericMenu currentNpGenericMenu;
        private List<MenuData> tabsMenuList;
        private Vector3 originalPosition;

        private Texture upIcon;
        private Texture downIcon;
        private bool isOpen = true;
        
        
        public override void StartAfterCreation()
        {
            base.StartAfterCreation();
            SetAllTabsButtons();
            SetEscapeButtonAction(HandleTransition);
            SetEscapeButtonTexture(downIcon);
            HideAndShowHeadLine(false);
        }

        public bool GetIsOpen()
        {
            return isOpen;
        }

        public override void OpenMenu()
        {
            SetTabsMenu();
            base.OpenMenu();
        }

        private void SetAllTabsButtons()
        {
            foreach (GenericUIData dataElement in genericUIDatas)
            {
                if (dataElement is TabData)
                {
                    TabData tabData = dataElement as TabData;
                    SetNewTabData(tabData);
                    
                    NP_Button npButton = CreateNPButton(tabData);

                    CreateAndRotateMenuIcons(tabData, npButton);
   
                    tabsMenu.AddTabButtonToPanel(npButton);
                    AddTabToList(tabData.NpMenuData);
                }
            }
        }

        private void CreateAndRotateMenuIcons(TabData tabData, NP_Button npButton)
        {
            Vector2 directionVector = GetDirection(npMenu.menuData);
            bool isHorizontal = directionVector.x != 0 && directionVector.y == 0;
                    
            if (isHorizontal && tabData.MenuIcon == null)
            {
                RectTransform rectTransform = npButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    int rotateDirection = (int)directionVector.x;
                        
                    rectTransform.transform.eulerAngles = new Vector3(rectTransform.rotation.eulerAngles.x,
                        rectTransform.rotation.eulerAngles.y, 90 * rotateDirection);
                }
            }
            else if (isHorizontal && tabData.MenuIcon!= null)
            {
                npButton.SetBackgroundImage(tabData.MenuIcon);
                npButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100,100);
                AspectRatioFitter aspectRatioFitter = npButton.gameObject.AddComponent<AspectRatioFitter>();
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            }
            
            if (tabData.ClickAction != null)
            {
                npButton.AddOnClick(tabData.ClickAction);
                npButton.AddOnClick(OpenMenuIfClosed);
            }
        }
        private NP_Button CreateNPButton(TabData tabData)
        {
            NP_Button npButton = tabData.GetUIElement() as NP_Button;
            npButton.SetText(tabData.Text);
            npButton.SetOnClick(()=>SelectTab(tabData.NpMenuData));
            return npButton;
        }

        private void SetNewTabData(TabData tabData)
        {
            MenuData tabMenuData = MenuCreator.StaticDataForMenu.FirstOrDefault(x=>(x.ItemType.Type == tabData.TypeOfMenu));
            if (tabMenuData != null)
            {
                tabData.NpMenuData = npMenu.menuData.Clone();
                tabData.NpMenuData.MenuName = tabMenuData.MenuName;
                tabData.NpMenuData.ItemType = tabMenuData.ItemType;
                tabData.NpMenuData.ID = tabMenuData.ID;
            }
            else
            {
                Debug.Log("No Menu Data Found with this name");
            }
        }

        private void SetTabsMenu()
        {
            tabsMenuList = new List<MenuData>();
            tabsMenu = GetComponentInChildren<NP_TabsMenu>();
            if (tabsMenu == null)
            {
                tabsMenu  = gameObject.GetComponent<NP_TabsMenu>();
            }
        }

        protected void AddTabToList(MenuData menuData)
        {
            if (menuData == null || tabsMenuList == null)
            {
                return;
            }
            tabsMenuList.Add(menuData);
        }

        protected void SelectTab(MenuData menu)
        {
            if (menu == null || tabsMenuList == null)
            {
                return;
            }

            Button escapeButton = npMenu.EscapeButton;
            npMenu.EscapeButton = null;
            if (tabsMenuList.Exists(x=>x.ID == menu.ID && x.ItemType == menu.ItemType))
            {
                currentMenu = menu;
                SwitchContent(currentMenu);
                HighLightSelectedTab(currentMenu);
            }

            npMenu.EscapeButton = escapeButton;
        }

        private void SwitchContent(MenuData menu)
        {
            ClearUI(true, false);
            
            // Destroy the current menu component
            if (currentNpGenericMenu != null)
            {
                currentNpGenericMenu.ClearUI();
                Destroy(currentNpGenericMenu);
            }

            // Use reflection to create a component of the type specified in menu.ItemType
            Type menuType = menu.ItemType.Type; // Assuming ItemType has a property 'Type' that returns the System.Type
            Component newMenuComponent = gameObject.AddComponent(menuType);

            if (newMenuComponent is NpGenericMenu genericMenu)
            {
                currentNpGenericMenu = genericMenu;
                // Initialize the new menu's UI elements directly in TabsMenu
                //SetMenuToNewUI(genericMenu, menu);
            }
            else
            {
                Debug.LogError($"Failed to create component of type {menuType}.");
                Destroy(newMenuComponent); // Remove the invalid component
            }
        }

        private void HighLightSelectedTab(MenuData menu)
        {
            // Implement highlighting logic for the selected tab
        }
        
        protected void HandleTransition()
        {
            if (isOpen)
            {
                TransitMenuTo(CalculateTargetPosition());
            }
            else
            {
                TransitMenuToStart();
            }
            isOpen = !isOpen;
            ChangeArrowDirectionAccordingToMenuState();
        }

        private void ChangeArrowDirectionAccordingToMenuState()
        {
            Texture arrowTexture;
            if (isOpen)
            {
                arrowTexture = downIcon;
            }
            else
            {
                arrowTexture = upIcon;
            }

            SetEscapeButtonTexture(arrowTexture);
        }

        protected void OpenMenuIfClosed()
        {
            if (!isOpen)
            {
                TransitMenuToStart();
                isOpen = true;
            }
            ChangeArrowDirectionAccordingToMenuState();
        }

        private void OnEnable()
        {
            Initialize();
        }
        
        private Vector3 CalculateTargetPosition()
        {
            Vector2 direction = GetDirection(npMenu.menuData);
            float visibleButtonDimensionSize = 0;
            float panelDimensionSize = 0;

            if (tabsMenu != null)
            {
                RectTransform tabsMenuRectTransform = tabsMenu.GetComponent<RectTransform>();
                if (tabsMenuRectTransform == null)
                {
                    return Vector3.zero;
                }

                NP_TabsContainer npTabsContainer = tabsMenu.GetTabsContainer();
                if (npTabsContainer == null)
                {
                    return Vector3.zero;
                }

                RectTransform npTabsContainerRectTransform = npTabsContainer.GetComponent<RectTransform>();
                if (npTabsContainerRectTransform == null)
                {
                    return Vector3.zero;
                }

                Vector2 panelDimensionDelta = tabsMenuRectTransform.sizeDelta;

                Vector2 visibleButtonDimensionDelta = npTabsContainerRectTransform.sizeDelta;

                bool isVertical = direction == Vector2.up || direction == Vector2.down;

                if (isVertical)
                {
                    visibleButtonDimensionSize = visibleButtonDimensionDelta.y;
                    panelDimensionSize = panelDimensionDelta.y;
                }
                else
                {
                    visibleButtonDimensionSize = visibleButtonDimensionDelta.x;
                    panelDimensionSize = panelDimensionDelta.x;
                }

                // Calculate the distance to move:
                // Move by the full width, then move back by the visibleButtonWidth
                float movementDistance = panelDimensionSize - visibleButtonDimensionSize;

                //int directionScalar = SetDirectionScalar(npMenu.menuData);
                //movementDistance *= directionScalar;

                Vector2 targetVector = direction * movementDistance;

                // Create the target position:
                // Start from the original position, and subtract the movement distance from the X-coordinate
                Vector2 targetPos = originalPosition;
                targetPos += targetVector; // Use '+' for sliding right, '-' for sliding left

                return targetPos;
            }

            return Vector3.zero;
        }

        private Vector2 GetDirection(MenuData menuData)
        {
            UIMenuGenerator.MenuAlignment alignment = menuData.Alignment;

            // In C# 8, we use standard logical OR (||) and equality operators (==)
            bool isLeftDirection = alignment == UIMenuGenerator.MenuAlignment.Left
                                   || alignment == UIMenuGenerator.MenuAlignment.LeftSide;

            bool isDownDirection = alignment == UIMenuGenerator.MenuAlignment.Bottom
                                   || alignment == UIMenuGenerator.MenuAlignment.BottomPanel
                                   || alignment == UIMenuGenerator.MenuAlignment.BottomCenter;

            bool isRightDirection = alignment == UIMenuGenerator.MenuAlignment.Right
                                    || alignment == UIMenuGenerator.MenuAlignment.RightSide;

            // We can skip the bool check for Up and just return it as the default
            if (isLeftDirection) return Vector2.left;
            if (isDownDirection) return Vector2.down;
            if (isRightDirection) return Vector2.right;

            return Vector2.up;
        }

        protected void Initialize()
        {
            originalPosition = GetComponent<RectTransform>().anchoredPosition;
            
            if(upIcon == null)
            {
                upIcon = Resources.Load<Texture>("Small Icons/UpArrow");
            }

            if (downIcon == null)
            {
                downIcon = Resources.Load<Texture>("Small Icons/DownArrow");
            }
        }
    }
}