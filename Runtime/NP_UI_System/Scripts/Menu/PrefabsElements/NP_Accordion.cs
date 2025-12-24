using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class NP_Accordion : NP_UIElements
{
    [SerializeField]
    private Transform contentTransform;
    
    [SerializeField]
    private VerticalLayoutGroup verticalLayoutGroup;

    [SerializeField]
    private ScrollRect scrollRect;

    private Color[] levelColors;

    public Transform ContentTransform { get { return contentTransform; } }
    
    public VerticalLayoutGroup VerticalLayoutGroup { get { return verticalLayoutGroup; } }
    
    public ScrollRect ScrollRect { get { return scrollRect; } }
    
    private void SetColorsLevel(int depth, Color headerColor)
    {
        float colorScalar = 0.05f;
        levelColors = new Color[depth];

        for (int i = 0; i < depth; i++)
        {
            float subtract = (colorScalar * i);
            float r = headerColor.r - subtract;
            float g = headerColor.g - subtract;
            float b = headerColor.b - subtract;

            Color newColor = new Color(r, g, b);
            levelColors[i] = newColor;
        }
    }

    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentTransform.GetComponent<RectTransform>());
        foreach (Transform child in contentTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(child.GetComponent<RectTransform>());
        }
    }

    private Color GetHeaderColor()
    {
        return levelColors[0];
    }

    public void SetBackgroundColor(Color color)
    {
        ScrollRect.gameObject.GetComponent<Image>().color = color;
    }

    public void Rescale(float scalar)
    {
        uiRectTransform.localScale = new Vector2(scalar, scalar);
    }
}
