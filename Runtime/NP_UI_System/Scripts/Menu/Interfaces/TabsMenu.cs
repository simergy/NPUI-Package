using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace NP_UI
{
    public class TabsMenu : NpGenericMenu
    {
        protected NP_TabsMenu tabsMenu;
        protected MenuData currentMenu;
        private NpGenericMenu currentNpGenericMenu;
        private List<MenuData> tabsMenuList;

        public override void StartAfterCreation()
        {
            base.StartAfterCreation();
            SetAllTabsButtons();
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
                    MenuData tabMenuData = MenuCreator.StaticDataForMenu.First(x=>(x.ItemType.Type == tabData.TypeOfMenu));

                    if (tabMenuData != null)
                    {
                        tabData.NpMenuData = npMenu.menuData.Clone();
                        tabData.NpMenuData.MenuName = tabMenuData.MenuName;
                        tabData.NpMenuData.ItemType = tabMenuData.ItemType;
                        tabData.NpMenuData.ID = tabMenuData.ID;
                    }
                    NP_Button npButton = dataElement.GetUIElement() as NP_Button;
                    npButton.SetText(tabData.Text);
                    npButton.SetOnClick(()=>SelectTab(tabData.NpMenuData));

                    bool isLeft = npMenu.menuData.Alignment is UIMenuGenerator.MenuAlignment.Left or UIMenuGenerator.MenuAlignment.LeftSide;
                    bool isRight = npMenu.menuData.Alignment is UIMenuGenerator.MenuAlignment.Right or UIMenuGenerator.MenuAlignment.RightSide;
                    bool isSide = isLeft || isRight;
                    
                    if (isSide && tabData.MenuIcon == null)
                    {
                        RectTransform rectTransform = npButton.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            int rotateDirection = 0;
                            if (isLeft)
                            {
                                rotateDirection = -1;
                            }
                            else
                            {
                                rotateDirection = 1;
                            }
                        
                            rectTransform.transform.eulerAngles = new Vector3(rectTransform.rotation.eulerAngles.x,
                                rectTransform.rotation.eulerAngles.y, 90 * rotateDirection);
                        }
                    }
                    else if (isSide && tabData.MenuIcon!= null)
                    {
                         npButton.SetBackgroundImage(tabData.MenuIcon);
                         npButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100,100);
                         AspectRatioFitter aspectRatioFitter = npButton.gameObject.AddComponent<AspectRatioFitter>();
                         aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                    }

                    if (tabData.ClickAction != null)
                    {
                        npButton.AddOnClick(tabData.ClickAction);
                    }
                    tabsMenu.AddTab(npButton);
                    AddTab(tabData.NpMenuData);
                }
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

        protected void AddTab(MenuData menuData)
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

            NP_Menu temporaryNPMenu = npMenu;
            npMenu.EscapeButton = null;
            if (tabsMenuList.Exists(x=>x.ID == menu.ID && x.ItemType == menu.ItemType))
            {
                currentMenu = menu;
                SwitchContent(currentMenu);
                HighLightSelectedTab(currentMenu);
            }
            npMenu = temporaryNPMenu;
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
    }
}