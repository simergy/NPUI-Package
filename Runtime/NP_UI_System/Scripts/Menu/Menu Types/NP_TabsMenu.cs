using NP_UI;
using UnityEngine;

public class NP_TabsMenu : NP_Menu
{
    [SerializeField] private NP_TabsContainer _tabsContainer;

    public void AddTabButtonToPanel(NP_Button button)
    {
        button.transform.SetParent(_tabsContainer.gridLayoutGroup.transform, false);
    }

    public NP_TabsContainer GetTabsContainer()
    {
        return _tabsContainer;
    }
}
