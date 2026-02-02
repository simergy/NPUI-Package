using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using NP_UI;

public class MenuScriptGeneratorWizard : EditorWindow
{
    // --- Script Generation Variables ---
    private string _englishMenuName = "Login Screen";
    private string _scriptSavePath = "Assets/Scripts/Menu/Menus/";
    private string _targetNamespace = "NP_UI";

    // --- Manual Registration Variables (These are explicitly for registering existing scripts) ---
    private string _manualRegClassName = ""; // For manually typing the class name to register
    private UIMenuGenerator.MenuAlignment _manualRegAlignment = UIMenuGenerator.MenuAlignment.Center;
    private float _manualRegScreenCoverage = MenuData.DefaultScreenCoveragePercent;
    private UIMenuGenerator.GridLayoutType _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Vertical;


    // --- Enum for Predefined Window Types (UI Design) ---
    public enum WindowType
    {
        Custom,             // Allows manual override of settings
        PopUp,              // Centered, medium size (e.g., confirmation dialogs)
        Fullscreen,         // Stretched to cover whole screen (e.g., main menu, loading screen)
        SidebarLeft,        // Aligned left, narrow (e.g., inventory panel)
        SidebarRight,       // Aligned right, narrow (e.g., quest log)
        NotificationTop,    // Small, centered at top (e.g., "Item Collected!")
        NotificationBottom, // Small, centered at bottom (e.g., "New Message!")
        DialogBox,          // Small, centered, often for simple input/info
        LoadingOverlay      // Fullscreen, non-interactive (e.g., while scene loads)
    }

    private WindowType _selectedWindowType = WindowType.Custom; // Default to Custom

    // --- NEW: Enum for Base Class Type (Class Design) ---
    public enum BaseMenuClassType
    {
        StandardMenu,   // Inherits from NpGenericMenu
        DataEntryForm   // Inherits from FormMenu (for validation capabilities)
        // Add other intuitive base class types here as needed
    }

    private BaseMenuClassType _selectedBaseClassType = BaseMenuClassType.StandardMenu; // Default to StandardMenu

    // NEW: Selected MenuType for the generated script
    private MenuType _selectedMenuType = MenuType.Regular;
    // NEW: Selected MenuFormType for the generated script (only relevant for DataEntryForm)
    private MenuFormType _selectedMenuFormType = MenuFormType.PerFieldErrorsType;
    
    
    // --- Menu Item to Open the Window ---
    [MenuItem("Tools/Menu/Generate New Menu Script Wizard")]
    public static void ShowWindow()
    {
        GetWindow<MenuScriptGeneratorWizard>("Menu Script Wizard");
    }

    // --- GUI Layout for the Window ---
    void OnGUI()
    {
        GUILayout.Label("Generate New Menu MonoBehaviour Script", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This section generates a new C# script file for a menu. It does NOT automatically register it with MenuCreator.", MessageType.Info);

        EditorGUILayout.Space(10);

        _englishMenuName = EditorGUILayout.TextField(new GUIContent("English Menu Name", "The user-friendly name for your menu. This will be converted into a valid C# class name."), _englishMenuName);
        
        string cSharpClassName = ToValidCSharpClassName(_englishMenuName);
        EditorGUILayout.LabelField("Generated C# Class Name:", cSharpClassName);
        EditorGUILayout.LabelField("Generated Script File:", $"{cSharpClassName}.cs");

        EditorGUILayout.Space();

        _scriptSavePath = EditorGUILayout.TextField(new GUIContent("Script Save Path", "The folder where the script will be saved. Must be within the Assets folder."), _scriptSavePath);
        if (GUILayout.Button("Browse Save Path"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Script Save Folder", _scriptSavePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    _scriptSavePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    if (!_scriptSavePath.EndsWith("/"))
                    {
                        _scriptSavePath += "/";
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a folder within your Unity project's Assets folder.", "OK");
                }
            }
        }

        EditorGUILayout.Space();

        _targetNamespace = EditorGUILayout.TextField(new GUIContent("Namespace", "The C# namespace for the generated script."), _targetNamespace);
        EditorGUILayout.HelpBox("Leave empty for no namespace. Using a namespace is recommended (e.g., 'NP_UI').", MessageType.None);

        EditorGUILayout.Space(20);

        // --- Class Design Settings ---
        GUILayout.Label("Class Design Settings", EditorStyles.boldLabel);
        _selectedBaseClassType = (BaseMenuClassType)EditorGUILayout.EnumPopup(new GUIContent("Menu Type", "Select the type of menu functionality (e.g., standard, form)."), _selectedBaseClassType);
        
        // This conditional block now ensures MenuFormType dropdown appears ONLY for DataEntryForm
        // and no specific type dropdown appears for StandardMenu.
        if (_selectedBaseClassType == BaseMenuClassType.DataEntryForm)
        {
            _selectedMenuFormType = (MenuFormType)EditorGUILayout.EnumPopup(new GUIContent("Form Error Type", "Select how errors are displayed in this form menu."), _selectedMenuFormType);
        }
        // Removed the 'else' block that showed _selectedMenuType for StandardMenu.
        
        EditorGUILayout.HelpBox("This determines the core functionality inherited by your new menu class.", MessageType.None);
        EditorGUILayout.Space();

        EditorGUILayout.Space(20);

        // --- Generate Script Button ---
        if (GUILayout.Button("Generate Menu Script File", GUILayout.Height(40)))
        {
            GenerateScriptFile();
        }

        EditorGUILayout.Space(30); // More space between sections

        // --- Manual Registration Section ---
        GUILayout.Label("Manually Register Existing Menu with MenuCreator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Use this section to register a menu script that has already been created and compiled. Its settings are independent of the 'New Script' generation.", MessageType.Info);

        // This field will be auto-filled after script generation, but can also be typed manually
        _manualRegClassName = EditorGUILayout.TextField(new GUIContent("Menu Class Name (to register)", "The EXACT C# class name of the menu (e.g., LoginScreenMenu)"), _manualRegClassName);
        EditorGUILayout.HelpBox("Make sure the script is compiled and present in your project!", MessageType.Warning);
        
        EditorGUILayout.Space();

        // MenuData settings for manual registration (these are now separate variables)
        GUILayout.Label("MenuData Settings for Registration", EditorStyles.boldLabel);
        
        // Moved Predefined Window Type here to influence manual registration settings
        EditorGUI.BeginChangeCheck();
        _selectedWindowType = (WindowType)EditorGUILayout.EnumPopup(new GUIContent("Predefined Window Type", "Select a preset for common UI layouts. Choose 'Custom' to manually set values."), _selectedWindowType);
        if (EditorGUI.EndChangeCheck())
        {
            ApplyWindowTypeSettings(_selectedWindowType);
        }

        _manualRegAlignment = (UIMenuGenerator.MenuAlignment)EditorGUILayout.EnumPopup("Alignment", _manualRegAlignment);
        _manualRegScreenCoverage = EditorGUILayout.Slider("Screen Coverage %", _manualRegScreenCoverage, 0.01f, 1.0f);
        _manualRegLayoutType = (UIMenuGenerator.GridLayoutType)EditorGUILayout.EnumPopup("Layout Type", _manualRegLayoutType);


        EditorGUILayout.Space(20);

        // --- Register Button ---
        if (GUILayout.Button("Register Menu with MenuCreator", GUILayout.Height(40)))
        {
            ManuallyRegisterMenu();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Ensure 'NpGenericMenu' (inheriting MonoBehaviour, with 'protected virtual void CreateMenuItems()' and 'protected virtual void StartAfterCreation()'), 'LabelData', 'MenuCreator', and 'SerializableType' exist and are correctly set up in your project. Also verify 'LabelData' has a constructor taking a string and 'genericUIDatas' handles the '+= LabelData' operation.", MessageType.None);
    }

    // --- Script Generation Logic ---
    private void GenerateScriptFile()
    {
        if (string.IsNullOrWhiteSpace(_englishMenuName))
        {
            EditorUtility.DisplayDialog("Error", "English Menu Name cannot be empty.", "OK");
            return;
        }
        string cSharpClassName = ToValidCSharpClassName(_englishMenuName);
        if (string.IsNullOrWhiteSpace(cSharpClassName))
        {
             EditorUtility.DisplayDialog("Error", "Could not generate a valid C# class name from the English Menu Name. Please use valid characters.", "OK");
             return;
        }

        if (string.IsNullOrWhiteSpace(_scriptSavePath) || !_scriptSavePath.StartsWith("Assets/"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid save path. Must be within the Assets folder.", "OK");
            return;
        }

        string fullPath = Path.Combine(Application.dataPath, _scriptSavePath.Substring("Assets/".Length));
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        string filePath = Path.Combine(fullPath, cSharpClassName + ".cs");
        
        if (File.Exists(filePath))
        {
            if (!EditorUtility.DisplayDialog("Overwrite Script?",
                $"A script named '{cSharpClassName}.cs' already exists at '{_scriptSavePath}'. Do you want to overwrite it?",
                "Overwrite", "Cancel"))
            {
                return;
            }
        }

        string scriptContent = GenerateScriptContent(cSharpClassName, _targetNamespace, _selectedBaseClassType);
        try
        {
            File.WriteAllText(filePath, scriptContent);
            AssetDatabase.Refresh(); // Trigger compilation

            // Auto-fill the registration field in the manual section
            _manualRegClassName = cSharpClassName; 
            
            EditorUtility.DisplayDialog("Script Generated", $"Script '{cSharpClassName}.cs' successfully generated. Please wait for Unity to finish compiling before attempting to register it.", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to generate script: {e.Message}", "OK");
            Debug.LogError($"Error generating script: {e}");
        }
    }



    // --- Manual Menu Registration Logic ---
    private void ManuallyRegisterMenu()
    {
        if (string.IsNullOrWhiteSpace(_manualRegClassName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter the C# Class Name of the menu you wish to register.", "OK");
            return;
        }

        Type menuTypeToRegister = AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(s => s.GetTypes())
                                    .FirstOrDefault(t => t.Name == _manualRegClassName);

        if (menuTypeToRegister == null)
        {
            EditorUtility.DisplayDialog("Error", $"Could not find compiled type '{_manualRegClassName}'. Make sure:\n1. The script exists and is compiled.\n2. The class name is spelled correctly (case-sensitive).\n3. If using a namespace, the correct namespace is applied and the assembly is loaded.", "OK");
            Debug.LogError($"MenuScriptGeneratorWizard: Type '{_manualRegClassName}' not found for manual registration.");
            return;
        }
        
        MenuCreator menuCreator = FindObjectOfType<MenuCreator>();
        if (menuCreator == null)
        {
            EditorUtility.DisplayDialog("Error", "No 'MenuCreator' script found in the current scene. Please add one to a GameObject.\nCreating one.", "OK");
            Debug.Log("MenuScriptGeneratorWizard: No MenuCreator found in scene for registration. Creating it first.");

            GameObject menuCreatorGO = Resources.Load<GameObject>("Localizations/GeneralScripts");
            menuCreator = Instantiate(menuCreatorGO).GetComponentInChildren<MenuCreator>();
        }
        
        SerializableType newMenuSerializableType = new SerializableType (menuTypeToRegister);

        // Use the _manualReg* variables for registration
        MenuData newMenuDataEntry = new MenuData(
            menuName: _manualRegClassName,
            itemType: newMenuSerializableType,
            alignment: _manualRegAlignment,
            screenCoveragePercent: _manualRegScreenCoverage,
            layoutType: _manualRegLayoutType,
            typeOfMenu: _selectedMenuType,
            typeOfFormMenu: _selectedMenuFormType
        );
        
        Undo.RecordObject(menuCreator, $"Register MenuData for {_manualRegClassName} to MenuCreator");
        menuCreator.AddMenuDataToList(newMenuDataEntry);

        EditorUtility.SetDirty(menuCreator);

        Selection.activeGameObject = menuCreator.gameObject;
        EditorGUIUtility.PingObject(menuCreator.gameObject);

        EditorUtility.DisplayDialog("Registration Complete",
            $"MenuData for '{_manualRegClassName}' successfully added to '{menuCreator.gameObject.name}'. You can now configure its details in the Inspector.", "OK");
    }

    // --- Helper to Apply Predefined Window Type Settings ---
    private void ApplyWindowTypeSettings(WindowType type)
    {
        // These now directly update the _manualReg* variables
        switch (type)
        {
            case WindowType.Custom:
                // Do nothing, manual settings apply.
                break;
            case WindowType.PopUp:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Center;
                _manualRegScreenCoverage = 0.5f; // 50% coverage
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Vertical;
                break;
            case WindowType.Fullscreen:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Stretch;
                _manualRegScreenCoverage = 1.0f; // 100% coverage
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Stretch; // Stretches to fill available space
                break;
            case WindowType.SidebarLeft:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Left;
                _manualRegScreenCoverage = 0.25f; // 25% width coverage (assume horizontal screens)
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Vertical; // Items stack vertically
                break;
            case WindowType.SidebarRight:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Right;
                _manualRegScreenCoverage = 0.25f; // 25% width coverage
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Vertical;
                break;
            case WindowType.NotificationTop:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.TopCenter;
                _manualRegScreenCoverage = 0.15f; // 15% height coverage
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Horizontal; // Items flow horizontally
                break;
            case WindowType.NotificationBottom:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.BottomCenter;
                _manualRegScreenCoverage = 0.15f; // 15% height coverage
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Horizontal;
                break;
            case WindowType.DialogBox:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Center;
                _manualRegScreenCoverage = 0.3f; // Smaller than PopUp, for simple confirmations
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Vertical;
                break;
            case WindowType.LoadingOverlay:
                _manualRegAlignment = UIMenuGenerator.MenuAlignment.Stretch;
                _manualRegScreenCoverage = 1.0f; // Full screen, often non-interactive
                _manualRegLayoutType = UIMenuGenerator.GridLayoutType.Stretch; // Just covers, no specific layout implied for items
                break;
        }
    }

    // --- Helper to Convert English Name to Valid C# Class Name ---
    private string ToValidCSharpClassName(string englishName)
    {
        if (string.IsNullOrWhiteSpace(englishName)) return string.Empty;
        
        string cleanedName = Regex.Replace(englishName, "[^a-zA-Z0-9_ ]", "");
        cleanedName = Regex.Replace(cleanedName, "(?:^|\\s)([a-zA-Z0-9_])", match => match.Groups[1].Value.ToUpper());
        cleanedName = cleanedName.Replace(" ", "");
        
        if (cleanedName.Length > 0 && char.IsDigit(cleanedName[0])) cleanedName = "_" + cleanedName;
        
        if (!cleanedName.EndsWith("Menu") || cleanedName.Equals("Menu", StringComparison.OrdinalIgnoreCase)) cleanedName += "Menu";

        return cleanedName;
    }

    // --- Helper to Generate the C# Script String ---
    private string GenerateScriptContent(string className, string targetNamespace, BaseMenuClassType baseClassType)
    {
        string baseClass = GetBaseClassName(baseClassType); // Use helper to get actual class name

        string content = "";
        content += "using UnityEngine;\n";
        content += "using System.Collections;\n";
        content += "using System.Collections.Generic; // For genericUIDatas\n";
        content += "using UnityEngine.UI;\n"; // For UI elements if used in CreateMenuItems
        content += "using NP_UI; // Ensure NP_UI namespace is included for NpGenericMenu\n";
        // Add specific usings for FormMenu if it requires them (e.g., for validation)
        if (baseClassType == BaseMenuClassType.DataEntryForm) // Check for DataEntryForm
        {
            content += "using UnityEngine.Events;\n"; // Example: if FormMenu uses UnityEvents for validation callbacks
        }


        if (!string.IsNullOrWhiteSpace(targetNamespace))
        {
            content += $"\nnamespace {targetNamespace}\n";
            content += "{\n";
        }

        content += $"    public class {className} : {baseClass}\n";
        content += "    {\n";
        
        // Add a protected field for the error label
        if (baseClassType == BaseMenuClassType.DataEntryForm)
        {
            content += "        protected NP_Lable _errorLabel;\n";
        }

        content += "        public override void CreateMenuItems()\n";
        content += "        {\n";
        content += "            // This method is called when the menu is being created. Implement your UI setup here.\n";
        content += "            // Example: LabelData helloWorldLabelData = new LabelData(\"Hello World\");\n";
        content += "            // genericUIDatas += helloWorldLabelData; // Assumes operator overloading or event subscription\n";
        content += "            base.CreateMenuItems(); // Call base to ensure NpGenericMenu's (or FormMenu's) setup runs\n";

        
        content += "        }\n"; // Closing CreateMenuItems

        content += "        \n"; 
        content += "        public override void StartAfterCreation()\n";
        content += "        {\n";
        content += "            base.StartAfterCreation(); // Call base to ensure NpGenericMenu's (or FormMenu's) setup runs\n";
        content += "            // This method can be used for any setup or initialization that needs to happen \n";
        content += "            // after the menu's GameObject is fully created and added to the scene.\n";
        content += "            // This function also invokes the OnMenuCreated event and sends the current menu through it:\n";
        content += "            // OnMenuCreated.Invoke(this);\n";

        content += "        }\n"; // Closing CreateMenuItems


        // Add FormMenu specific overrides if DataEntryForm is selected
        if (baseClassType == BaseMenuClassType.DataEntryForm)
        {
            content += "\n        // --- FormMenu Specific Overrides ---\n";
            content += "        public override bool ValidateForm()\n";
            content += "        {\n";
            content += "            // Clear previous error messages\n";
            content += "            // Implement your form validation logic here.\n";
            content += "            Debug.Log(\"FormMenu: ValidateForm overridden. Implement your specific validation here.\");\n";
            content += "            bool baseValidation = base.ValidateForm(); // Call base if you want to extend base validation\n";
            content += "            \n";
            content += "            // Example: Add a custom validation rule\n";
            content += "            // if (someConditionIsFalse) { SetErrorText(\"Some error message.\"); return false; }\n";
            content += "            \n";
            content += "            return baseValidation; \n";
            content += "        }\n";

            content += "\n        protected override void OnSubmitForm()\n";
            content += "        {\n";
            content += "            // Implement what happens when the form is submitted (after validation).\n";
            content += "            base.OnSubmitForm(); // Call base if you want to extend base submission logic\n";
            content += "        }\n";

            content += "\n        protected override void ClearForm()\n";
            content += "        {\n";
            content += "            // Implement logic to clear all input fields in your form.\n";
            content += "            base.ClearForm(); // Call base if you want to extend base clear logic\n";
            content += "        }\n";

            content += "\n        protected override void ResetForm()\n";
            content += "        {\n";
            content += "            // Implement logic to reset form fields to their initial/default state.\n";
            content += "            base.ResetForm(); // Call base if you want to extend base reset logic\n";
            content += "        }\n";
            
            
        }

        content += "    }\n";

        if (!string.IsNullOrWhiteSpace(targetNamespace))
        {
            content += "}\n";
        }
        return content;
    }

    // --- NEW: Helper to map intuitive enum to actual class name ---
    private string GetBaseClassName(BaseMenuClassType type)
    {
        switch (type)
        {
            case BaseMenuClassType.StandardMenu:
                return "NpGenericMenu";
            case BaseMenuClassType.DataEntryForm:
                return "FormMenu";
            default:
                return "NpGenericMenu"; // Fallback
        }
    }
}
