using UnityEditor;
using UnityEngine;
using System;
using NP_UI;

// This class defines your custom Editor Window
public class ClassSelectionWindow : EditorWindow
{
    // Our serializable type wrapper
    // This is the field that will be drawn by our PropertyDrawer
    private SerializableType _selectedClassType; 

    // Used to make the window scrollable if content goes beyond bounds
    private Vector2 _scrollPos;

    [MenuItem("Tools/Class Selector Window")] // Adds an entry to the Unity Editor's "Tools" menu
    public static void ShowWindow()
    {
        // Get existing open window or create a new one
        ClassSelectionWindow window = GetWindow<ClassSelectionWindow>("Class Selector");
        window.minSize = new Vector2(300, 150); // Set a minimum size for the window
        window.Show(); // Display the window
    }

    private void OnEnable()
    {
        // Initialize if null, especially when the window is first opened or recompiled
        if (_selectedClassType == null)
        {
            _selectedClassType = new SerializableType(null); // Start with no type selected
        }
    }

    void OnGUI()
    {
        // Add a scroll view for robustness
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        GUILayout.Label("Select a Class Type (Drag & Drop Script)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- Draw the SerializableType field using the PropertyDrawer ---
        // PropertyDrawers work on SerializedProperties, so we need to create one.
        // For simple EditorWindow fields, we often manually manage the GUI state.
        // However, to leverage PropertyDrawer, we need to create a SerializedObject
        // and SerializedProperty for our own EditorWindow instance.

        // This is a common pattern for EditorWindows to make fields serializable and leverage PropertyDrawers.
        // We get a SerializedObject for THIS EditorWindow instance.
        SerializedObject so = new SerializedObject(this);
        SerializedProperty classTypeProperty = so.FindProperty("_selectedClassType");

        // Draw the property field. The SerializableTypeDrawer will handle this.
        EditorGUILayout.PropertyField(classTypeProperty, new GUIContent("Chosen Class Type"));

        // Apply changes made in the Inspector to the actual object
        so.ApplyModifiedProperties();

        EditorGUILayout.Space();

        // --- Display information about the selected type ---
        if (_selectedClassType != null && _selectedClassType.Type != null)
        {
            GUILayout.Label("Selected Class Details:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name:", _selectedClassType.Type.Name);
            EditorGUILayout.LabelField("Full Name:", _selectedClassType.Type.FullName);
            EditorGUILayout.LabelField("Assembly:", _selectedClassType.Type.Assembly.GetName().Name);
            EditorGUILayout.LabelField("Is Abstract:", _selectedClassType.Type.IsAbstract.ToString());
            EditorGUILayout.LabelField("Is Class:", _selectedClassType.Type.IsClass.ToString());
            EditorGUILayout.LabelField("Is Interface:", _selectedClassType.Type.IsInterface.ToString());
            
            // Example: Button to do something with the selected type
            if (GUILayout.Button("Log Type Info to Console"))
            {
                Debug.Log($"Selected Type: {_selectedClassType.Type.Name}", this);
                Debug.Log($"Namespace: {_selectedClassType.Type.Namespace}");
                Debug.Log($"AssemblyQualifiedName: {_selectedClassType.Type.AssemblyQualifiedName}");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Drag a C# script asset from the Project window onto the 'Chosen Class Type' field above.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }
}