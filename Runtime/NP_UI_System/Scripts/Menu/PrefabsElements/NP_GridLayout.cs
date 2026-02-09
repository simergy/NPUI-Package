using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a UI element that manages a grid layout for its children.
/// </summary>
public class NP_GridLayout : NP_UIElements
{
    [SerializeField] protected UnityEngine.UI.GridLayoutGroup gridLayoutGroup; // Note: full namespace to avoid conflict with enum

    protected override void Awake()
    {
        base.Awake();
        // Get references to potential layout groups
        gridLayoutGroup = GetComponent<UnityEngine.UI.GridLayoutGroup>();
    }

    public void SetPadding(RectOffset padding)
    {
        if (padding != null)
        {
            gridLayoutGroup.padding = padding;
        }
    }

    public void SetCellSize(Vector2 cellSize)
    {
        if (cellSize != null)
        {
            gridLayoutGroup.cellSize = cellSize;
        }
    }

    public void SetSpacing(Vector2 spacing)
    {
        if (spacing != null)
        {
            gridLayoutGroup.spacing = spacing;
        }
    }
    public void SetChildAlignment(TextAnchor alignment)
    {
        if (alignment != null)
        {
            gridLayoutGroup.childAlignment = alignment;
        }
    }

    public void SetStartCorner(GridLayoutGroup.Corner corner)
    {
        if (corner != null)
        {
            gridLayoutGroup.startCorner = corner;
        }
    }

    public void SetStartAxis(GridLayoutGroup.Axis axis)
    {
        if (axis != null)
        {
            gridLayoutGroup.startAxis = axis;
        }
    }

    public void SetConstraint(GridLayoutGroup.Constraint constraint, int constraintCount = 0)
    {
        if (constraint != null)
        {
            gridLayoutGroup.constraint = constraint;
            gridLayoutGroup.constraintCount = constraintCount;
        }
    }

    public void SetChilderns(List<GenericUIData> childrens)
    {
        foreach (GenericUIData child in childrens)
        {
            child.GetUIElement().transform.SetParent(gridLayoutGroup.transform);
        }
    }

    public void SetDirection(GridLayoutGroup gridLayoutGroupType)
    {
        // This method implies controlling which layout group component is active or
        // how it's configured based on the desired direction.
        // Example: Only one layout group should be active at a time.

        // You might use gridLayoutGroupType to further configure settings,
        // e.g., spacing, padding, child alignment based on the type.
        // This part would require more specific logic based on your design.
    }

    public void SetNumberOfChildren(int count, GridLayoutGroup gridLayoutGroupType)
    {
        // This method typically involves instantiating or managing a fixed number of child elements
        // within the layout. It's not directly related to the layout group components themselves,
        // but rather the *content* of the grid.
        // You'd typically iterate through existing children, create new ones, or destroy excess ones.
        Debug.Log($"Setting number of children for grid type {gridLayoutGroupType} to: {count}");

        // Example: Clear existing children and instantiate new ones (very simplified)
        // For actual implementation, you'd use object pooling or more robust management.
        // foreach (Transform child in transform)
        // {
        //     Destroy(child.gameObject);
        // }
        // for (int i = 0; i < count; i++)
        // {
        //     // Instantiate a prefab or create a new GameObject as a child
        //     // GameObject newChild = Instantiate(yourChildPrefab, transform);
        // }
    }
    
}