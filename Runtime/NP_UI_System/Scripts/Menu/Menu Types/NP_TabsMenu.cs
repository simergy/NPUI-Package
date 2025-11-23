using NP_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NP_TabsMenu : NP_Menu
{
    [SerializeField] private NP_TabsContainer _tabsContainer;

    public void AddTab(NP_Button button)
    {
        button.transform.SetParent(_tabsContainer.gridLayoutGroup.transform, false);
    }
}
