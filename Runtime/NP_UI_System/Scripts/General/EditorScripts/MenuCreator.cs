using UnityEngine;
using System;
using System.Collections.Generic;
using NP_UI;
using NUnit.Framework; // For Type

public class MenuCreator : MonoBehaviour
{
    [SerializeField]
    private List<MenuData> dataForMenu;
    
    [SerializeField]
    private RectTransform canvas;
    
    public static List<MenuData> StaticDataForMenu;
    
    void Start()
    {
        StaticDataForMenu = dataForMenu;
        foreach (MenuData data in dataForMenu)
        {
            data.ParentCanvas = canvas;
            NpGenericMenu genericMenu = UIMenuGenerator.CreateScrollableGridMenu(data);
            NP_EventsManager.OnMenuCreated.Invoke(genericMenu);
        }
    }
    
    public void AddMenuDataToList(MenuData menuData)
    {
        if (dataForMenu == null)
        {
            dataForMenu = new List<MenuData>();
        }

        if (!dataForMenu.Contains(menuData))
        {
            dataForMenu.Add(menuData);
        }
    }
}



/// <summary>
/// This class represents the data required for menu creation
/// </summary>
[System.Serializable]
public struct DataForMenu //Struct
{
    public string MenuName;
    public Texture IconTexture;
    public SerializableType ClassType;
    public RectAlign RectAlign;
    public float PercentOfScreen;
    
}