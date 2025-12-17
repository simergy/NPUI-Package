#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

// This attribute links the PropertyDrawer to your SerializableType class
[CustomPropertyDrawer(typeof(SerializableType))]
public class SerializableTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the SerializedProperty for the _assemblyQualifiedName field within SerializableType
        SerializedProperty assemblyQualifiedNameProperty = property.FindPropertyRelative("_assemblyQualifiedName");

        // Calculate a new Rect for the drag-and-drop area.
        // We'll draw a regular ObjectField for the label and a custom area for drag.
        Rect dropAreaRect = EditorGUI.PrefixLabel(position, label);

        // Draw a box to indicate the drag area
        GUI.Box(dropAreaRect, GUIContent.none, EditorStyles.textArea); // Use textArea for a subtle border

        // Get the currently assigned type, or "None"
        string currentTypeName = "None (Type)";
        if (!string.IsNullOrEmpty(assemblyQualifiedNameProperty.stringValue))
        {
            Type currentType = Type.GetType(assemblyQualifiedNameProperty.stringValue);
            if (currentType != null)
            {
                currentTypeName = currentType.Name;
            } else {
                currentTypeName = "[Missing Type]"; // Indicate if type can't be resolved
            }
        }
        
        // Display the current type name
        Rect textRect = dropAreaRect;
        textRect.xMin += 5; // Add a little padding
        EditorGUI.LabelField(textRect, currentTypeName, EditorStyles.boldLabel);

        // --- Handle Drag and Drop ---
        Event evt = Event.current;

        switch (evt.type)
        {
            case EventType.DragUpdated: // When something is being dragged over the area
            case EventType.DragPerform: // When something is dropped on the area
                if (!dropAreaRect.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // Show a copy icon
                
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag(); // Accept the drag operation

                    // Iterate through the dragged objects
                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        MonoScript monoScript = draggedObject as MonoScript;
                        if (monoScript != null)
                        {
                            Type draggedType = monoScript.GetClass(); // Get the Type from the MonoScript

                            // Optional: Filter for specific base classes or interfaces
                            // Example: Only allow classes that inherit from 'MyBaseClass'
                            // if (draggedType != null && typeof(MyBaseClass).IsAssignableFrom(draggedType))
                            // {
                                assemblyQualifiedNameProperty.stringValue = draggedType.AssemblyQualifiedName;
                                // Set dirty to ensure Unity saves the change
                                EditorUtility.SetDirty(property.serializedObject.targetObject);
                                break; // Only accept the first valid script
                            // }
                            // else if (draggedType != null)
                            // {
                            //     Debug.LogWarning($"Dragged script '{draggedType.Name}' does not derive from MyBaseClass.");
                            // }
                        }
                    }
                    evt.Use(); // Consume the event so other controls don't process it
                }
                break;
            case EventType.MouseDown:
                // Allow clearing the field with a right-click or similar interaction
                if (evt.button == 1 && dropAreaRect.Contains(evt.mousePosition)) // Right-click
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Clear Type"), false, () => {
                        assemblyQualifiedNameProperty.stringValue = null;
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                        property.serializedObject.ApplyModifiedProperties(); // Apply changes immediately
                    });
                    menu.ShowAsContext();
                    evt.Use();
                }
                break;
        }
    }
}
#endif