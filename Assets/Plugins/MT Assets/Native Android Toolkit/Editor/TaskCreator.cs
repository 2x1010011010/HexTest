using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    public class TaskCreator : EditorWindow
    {
        //Classes for this script
        public class MethodPositionInDesignMode
        {
            //This class store the position of start and end of a method in the renderization of design mode
            public int startPosition = 0;
            public int endPosition = 0;

            public MethodPositionInDesignMode(int startPosition, int endPosition)
            {
                this.startPosition = startPosition;
                this.endPosition = endPosition;
            }
        }
        public class RuntimeKeyInformation
        {
            //This class stores information about a runtime key
            public string keyTipe = "";
            public string key = "";
            public int uses = 0;
            public bool isBeingUsedByCsharp = true;
        }
        public class FileValueInformation
        {
            //This class stores information about a a file value
            public string fileType = "";
            public string file = "";
            public List<string> listOfComponentsUsing = new List<string>();
        }

        //Variables of preferences
        private NativeAndroidPreferences natPreferences;

        //Cache variables
        private Dictionary<string, List<string>> currentTaskSourceCodeBeingEdited = new Dictionary<string, List<string>>();
        private bool alreadyLoadedTheRuntimeKeys = false;
        private bool alreadyCheckedTheRuntimeKeysInCsharpCodes = false;
        private Dictionary<string, RuntimeKeyInformation> currentTaskSourceCodeRuntimeKeys = new Dictionary<string, RuntimeKeyInformation>();
        private long lastRuntimeKeysUpdate = DateTime.Now.Ticks;
        private bool alreadyLoadedTheFileValues = false;
        private Dictionary<string, FileValueInformation> currentTaskSourceCodeFileValues = new Dictionary<string, FileValueInformation>();
        private long lastFileValuesUpdate = DateTime.Now.Ticks;

        //Private variables
        private Vector2 scrollPosSourceModeView;
        private Vector2 scrollPosDesignModeView;
        private Vector2 scrollPosDesignMapModeView;
        private Vector2 scrollPosDesignRootHierarchyModeView;
        private Vector2 scrollPosDesignKeysHierarchyModeView;
        private Vector2 scrollPosDesignFilesHierarchyModeView;
        private int currentSourceCodeViewMode = 0;
        private Dictionary<string, MethodPositionInDesignMode> methodsPositionInDesignMode = new Dictionary<string, MethodPositionInDesignMode>();
        private int lastFinalPositionOfMethodInDesignMode = 0;
        private long lastUpdateOfTaskCreatorUI = DateTime.Now.Ticks;

        //Private variables of pendent actions
        private bool isAddingNewMethod = false;
        private string methodNameThatIsAdding = "newMethodName";
        private string pendentMethodToDelete = "";
        private string pendentMethodToCreate = "";
        private string pendentComponentDoDeleteParentMethod = "";
        private int pendentComponentDoDeleteIndex = -1;
        private string pendentComponentToMoveUpParentMethod = "";
        private int pendentComponentToMoveUpIndex = -1;
        private string pendentComponentToMoveDownParentMethod = "";
        private int pendentComponentToMoveDownIndex = -1;
        private string pendentComponentToBeMovedCode = "";
        private string pendentComponentToBeMovedTargetMethod = "";
        private string pendentComponentToBeCopiedCode = "";
        private string pendentComponentToBeCopiedTargetMethod = "";

        //Preferences code

        void LoadThePreferences(TaskCreator thisTaskCreator)
        {
            //Try to load the preferences file
            natPreferences = (NativeAndroidPreferences)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset", typeof(NativeAndroidPreferences));

            //If null, cancel this execution
            if (natPreferences == null)
            {
                EditorUtility.DisplayDialog("Error", "There was a problem loading the Native Android Toolkit Editor preferences save file. Please try to save preferences before opening Task Creator. If the problem persists, please contact MT Assets support.", "Ok");
                thisTaskCreator.Close();
                return;
            }
        }

        void SaveThePreferences()
        {
            EditorUtility.SetDirty(natPreferences);
            AssetDatabase.SaveAssets();
        }

        //Setup window code

        public static void OpenWindow()
        {
            //Method to open the Window
            var window = GetWindow<TaskCreator>("Task Creator");
            window.minSize = new Vector2(1124, 564);
            window.maxSize = new Vector2(1124, 564);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Load the preferences, if is null
            if (natPreferences == null)
            {
                LoadThePreferences(this);
                LoadSourceCode();
            }

            //Load the resources
            Texture taskCreatorIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/TaskCreator.png", typeof(Texture));
            Texture methodIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Method.png", typeof(Texture));
            Texture methodBg = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/MethodBg.png", typeof(Texture));
            Texture methodFg = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/MethodFg.png", typeof(Texture));
            Texture removeIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Remove.png", typeof(Texture));
            Texture addIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Add.png", typeof(Texture));
            Texture demarcatorIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Demarcator.png", typeof(Texture));
            Texture functionSimpleIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/FunctionSimple.png", typeof(Texture));
            Texture functionNormalIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/FunctionNormal.png", typeof(Texture));
            Texture conditionIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Condition.png", typeof(Texture));
            Texture commentIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Comment.png", typeof(Texture));
            Texture topicSeparator = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/TopicSeparator.png", typeof(Texture));
            Texture options = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Options.png", typeof(Texture));
            Texture extras = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Extras.png", typeof(Texture));
            Texture variableIc = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Variable.png", typeof(Texture));
            Texture keysOn = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/KeysOn.png", typeof(Texture));
            Texture keysOff = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/KeysOff.png", typeof(Texture));
            Texture vBool = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Boolean.png", typeof(Texture));
            Texture vInt = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Integer.png", typeof(Texture));
            Texture vFloat = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Float.png", typeof(Texture));
            Texture vString = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/String.png", typeof(Texture));
            //Cancel if some of the resources not found on project
            if (taskCreatorIcon == null || methodIcon == null || methodBg == null || methodFg == null || removeIcon == null || addIcon == null || functionSimpleIcon == null || functionNormalIcon == null || conditionIcon == null || commentIcon == null || topicSeparator == null || options == null || extras == null || variableIc == null || keysOn == null || keysOff == null || vFloat == null || vInt == null || vBool == null || vString == null)
            {
                EditorGUILayout.HelpBox("Unable to load required files. Please reinstall Native Android Toolkit to correct this problem.", MessageType.Error);
                return;
            }

            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);
            //Topbar
            GUILayout.BeginHorizontal("box");
            GUILayout.Box(taskCreatorIcon, iconStyle, GUILayout.Width(24), GUILayout.Height(24));
            GUILayout.Space(4);
            GUILayout.BeginVertical();
            GUILayout.Space(3);
            GUIStyle titulo = new GUIStyle();
            titulo.fontSize = 14;
            titulo.normal.textColor = Color.black;
            titulo.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("NAT Task Creator", titulo);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Space(-2);
            EditorGUILayout.LabelField("Last Save On", new GUIStyle() { fontSize = 10, alignment = TextAnchor.MiddleRight }, GUILayout.Width(80));
            GUILayout.Space(-8);
            if (natPreferences.taskSourceCodeLastSave == "")
                EditorGUILayout.LabelField("Never", new GUIStyle() { fontSize = 10, alignment = TextAnchor.MiddleRight }, GUILayout.Width(80));
            if (natPreferences.taskSourceCodeLastSave != "")
                EditorGUILayout.LabelField(natPreferences.taskSourceCodeLastSave, new GUIStyle() { fontSize = 10, alignment = TextAnchor.MiddleRight }, GUILayout.Width(80));
            GUILayout.Space(-2);
            GUILayout.EndVertical();
            if (alreadyLoadedTheFileValues == false || alreadyLoadedTheRuntimeKeys == false)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Save Task Code", GUILayout.Height(26), GUILayout.Width(114));
                EditorGUI.EndDisabledGroup();
            }
            if (alreadyLoadedTheFileValues == true && alreadyLoadedTheRuntimeKeys == true)
                if (GUILayout.Button("Save Task Code", GUILayout.Height(26), GUILayout.Width(114)))
                    SaveSourceCode();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(3);
            if (GUILayout.Button(new GUIContent(extras, ""), GUIStyle.none, GUILayout.Height(20), GUILayout.Width(20)))
            {
                //Prepare the options
                GenericMenu genericMenu = new GenericMenu();

                //Show options
                if (currentSourceCodeViewMode == 0)
                {
                    genericMenu.AddItem(new GUIContent("Source Mode"), false, () => { currentSourceCodeViewMode = 1; });
                    genericMenu.AddDisabledItem(new GUIContent("Design Mode"));
                }
                if (currentSourceCodeViewMode == 1)
                {
                    genericMenu.AddDisabledItem(new GUIContent("Source Mode"));
                    genericMenu.AddItem(new GUIContent("Design Mode"), false, () => { currentSourceCodeViewMode = 0; });
                }
                genericMenu.AddItem(new GUIContent("Reset"), false, () => { currentTaskSourceCodeBeingEdited.Clear(); });

                //Render the context menu
                genericMenu.ShowAsContext();
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //View
            if (currentSourceCodeViewMode == 0)
            {
                try { UI_View_DesignMode(methodIcon, methodBg, methodFg, removeIcon, addIcon, new Texture[] { demarcatorIcon, functionSimpleIcon, functionNormalIcon, conditionIcon, commentIcon }, topicSeparator, options, variableIc, new Texture[] { keysOff, keysOn }, vString, vFloat, vInt, vBool); }
                catch (ArgumentException e) { if (String.IsNullOrEmpty(e.Message) == false) { this.Repaint(); } }            //<- To ignore UI errors
                catch (IndexOutOfRangeException e) { if (String.IsNullOrEmpty(e.Message) == false) { this.Repaint(); } }     //<- To ignore component initialization errors
            }
            if (currentSourceCodeViewMode == 1)
                UI_View_SourceMode();

            //Inform that the Task Creator UI was updated
            lastUpdateOfTaskCreatorUI = DateTime.Now.Ticks;

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        void UI_View_DesignMode(Texture mIc, Texture mBg, Texture mFg, Texture rI, Texture aI, Texture[] cTypes, Texture tSep, Texture opt, Texture vrs, Texture[] kys, Texture vS, Texture vF, Texture vI, Texture vB)
        {
            //If not have a souce code, create the base
            if (SourceCode_isEmpty() == true)
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(370);
                EditorGUILayout.BeginVertical(GUILayout.Width(148));
                EditorGUILayout.LabelField("It looks like there is no Source Code in this Task", new GUIStyle() { fontSize = 16, fontStyle = FontStyle.Bold });
                GUILayout.Space(16);
                EditorGUILayout.BeginHorizontal(GUILayout.Width(148));
                GUILayout.Space(106);
                if (GUILayout.Button("Create Base Source Code", GUILayout.Height(26), GUILayout.Width(164)))
                    if (currentTaskSourceCodeBeingEdited.ContainsKey("-method=main") == false)
                        currentTaskSourceCodeBeingEdited.Add("-method=main", new List<string>());
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }

            //Render the source code in design view
            if (SourceCode_isEmpty() == false)
            {
                EditorGUILayout.BeginHorizontal();

                //Process the variables, runtime keys and files of the code to render in hierarchy
                UI_View_DesignMode_ProcessFilesAndVariables();

                //Prepare the UI format
                GUIStyle iconStyle = new GUIStyle();
                iconStyle.border = new RectOffset(0, 0, 0, 0);
                iconStyle.margin = new RectOffset(4, 0, 4, 0);

                //Start of Hierarchy block
                EditorGUILayout.BeginVertical("box");
                scrollPosDesignRootHierarchyModeView = EditorGUILayout.BeginScrollView(scrollPosDesignRootHierarchyModeView, GUILayout.Width(250), GUILayout.Height(520));
                GUIStyle methodBoxH = new GUIStyle();
                methodBoxH.normal.background = (Texture2D)mBg;
                //KeysHierarchy renderization
                EditorGUILayout.BeginVertical(methodBoxH);
                scrollPosDesignKeysHierarchyModeView = EditorGUILayout.BeginScrollView(scrollPosDesignKeysHierarchyModeView, GUILayout.Width(249), GUILayout.Height(256));
                //------------------------------------------------ Start the code of KeysHierarchyRenderization ------------------------------------------------

                //Render the Runtime Keys
                if (alreadyLoadedTheRuntimeKeys == false)
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(249), GUILayout.Height(255));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Loading Runtime Keys", GUILayout.Width(128));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }
                if (alreadyLoadedTheRuntimeKeys == true)
                {
                    //If have runtime keys
                    foreach (var runtimeKey in currentTaskSourceCodeRuntimeKeys)   //<- Render all runtime keys in database
                    {
                        //Extract runtime key data
                        RuntimeKeyInformation runtimeKeyInformation = currentTaskSourceCodeRuntimeKeys[runtimeKey.Key];
                        EditorGUILayout.BeginHorizontal();
                        if (runtimeKeyInformation.isBeingUsedByCsharp == false)
                            GUILayout.Box(kys[0], iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                        if (runtimeKeyInformation.isBeingUsedByCsharp == true)
                            GUILayout.Box(kys[1], iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                        GUILayout.Space(-2);
                        switch (runtimeKeyInformation.keyTipe)
                        {
                            case "string":
                                GUILayout.Box(vS, iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                break;
                            case "float":
                                GUILayout.Box(vF, iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                break;
                            case "int":
                                GUILayout.Box(vI, iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                break;
                            case "bool":
                                GUILayout.Box(vB, iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                break;
                        }
                        GUILayout.Space(4);
                        EditorGUILayout.LabelField(runtimeKeyInformation.key, GUILayout.Width(170));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(runtimeKeyInformation.uses.ToString(), new GUIStyle { alignment = TextAnchor.MiddleRight }, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    //If not have runtime keys
                    if (currentTaskSourceCodeRuntimeKeys.Keys.Count == 0)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Width(249), GUILayout.Height(255));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("No Runtime Keys", GUILayout.Width(98));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndVertical();
                    }
                }

                //------------------------------------------------  End the code of KeysHierarchyRenderization -------------------------------------------------
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                GUILayout.Space(8);
                //FilesHierarchy renderization
                EditorGUILayout.BeginVertical(methodBoxH);
                scrollPosDesignFilesHierarchyModeView = EditorGUILayout.BeginScrollView(scrollPosDesignFilesHierarchyModeView, GUILayout.Width(249), GUILayout.Height(256));
                //------------------------------------------------ Start the code of FilesHierarchyRenderization ------------------------------------------------

                //Render the File Values
                if (alreadyLoadedTheFileValues == false)
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(249), GUILayout.Height(255));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Loading File Values", GUILayout.Width(110));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }
                if (alreadyLoadedTheFileValues == true)
                {
                    //If have runtime keys
                    foreach (var runtimeKey in currentTaskSourceCodeFileValues)   //<- Render all file values in database
                    {
                        //Extract runtime key data
                        FileValueInformation fileValueInformation = currentTaskSourceCodeFileValues[runtimeKey.Key];
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Box(vrs, iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                        GUILayout.Space(4);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(4);
                        EditorGUILayout.LabelField("<b>" + fileValueInformation.file + "</b>", new GUIStyle() { richText = true }, GUILayout.Width(170));
                        EditorGUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(fileValueInformation.listOfComponentsUsing.Count.ToString(), new GUIStyle { alignment = TextAnchor.MiddleRight }, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(-2);
                        foreach (string use in fileValueInformation.listOfComponentsUsing)
                        {
                            GUILayout.Space(-5);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(27);
                            string[] useInfo = use.Split(new[] { ' ' }, 2);
                            switch (useInfo[0])
                            {
                                case "string":
                                    GUILayout.Box(vS, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                                    break;
                                case "float":
                                    GUILayout.Box(vF, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                                    break;
                                case "int":
                                    GUILayout.Box(vI, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                                    break;
                                case "bool":
                                    GUILayout.Box(vB, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                                    break;
                            }
                            GUILayout.Space(-2);
                            EditorGUILayout.BeginVertical();
                            int maxCharacters = 28;
                            if (useInfo[1].Length <= maxCharacters)
                                EditorGUILayout.LabelField(useInfo[1], GUILayout.Width(194));
                            if (useInfo[1].Length > maxCharacters)
                                EditorGUILayout.LabelField(useInfo[1].Substring(0, maxCharacters) + "...", GUILayout.Width(194));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        GUILayout.Space(6);
                    }
                    if (currentTaskSourceCodeFileValues.Keys.Count > 0)
                        GUILayout.Space(-6);
                    //If not have file values
                    if (currentTaskSourceCodeFileValues.Keys.Count == 0)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Width(249), GUILayout.Height(255));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("No File Values", GUILayout.Width(82));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndVertical();
                    }
                }

                //------------------------------------------------  End the code of FilesHierarchyRenderization -------------------------------------------------
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                //End of Hierarchy block
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                //Reset the lastFinalPositionOfMethodInDesignMode
                lastFinalPositionOfMethodInDesignMode = 0;

                //Components Renderization
                EditorGUILayout.BeginVertical("box");
                scrollPosDesignModeView = EditorGUILayout.BeginScrollView(scrollPosDesignModeView, GUILayout.Width(590), GUILayout.Height(520));
                //------------------------------------------------ Start the code of ComponentsRenderization ------------------------------------------------

                //Render each method and components inside it
                foreach (var method in currentTaskSourceCodeBeingEdited)
                {
                    //Get the method data
                    string methodCode = method.Key;
                    List<string> methodComponents = method.Value;

                    //Render the method block
                    GUIStyle methodBox = new GUIStyle();
                    methodBox.normal.background = (Texture2D)mBg;
                    EditorGUILayout.BeginHorizontal(methodBox);
                    EditorGUILayout.BeginVertical(GUILayout.Width(570));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(mIc, iconStyle, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.Space(4);
                    GUILayout.BeginVertical();
                    GUILayout.Space(6);
                    EditorGUILayout.LabelField("<b>Method</b> \"" + methodCode.Replace("-method=", "") + "\"", new GUIStyle() { richText = true });
                    GUILayout.Space(4);
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical();
                    GUILayout.Space(6);
                    if (GUILayout.Button(new GUIContent(aI, ""), GUIStyle.none, GUILayout.Height(16), GUILayout.Width(16)))
                    {
                        //============================= Start Renderization Of Context Menu to Add New Component

                        SourceCode_RenderContextMenuToAddNewComponenteHere(methodCode);

                        //=============================== End Renderization Of Context Menu to Add New Component
                    }
                    GUILayout.EndVertical();
                    if (methodCode != "-method=main")
                    {
                        GUILayout.Space(12);
                        GUILayout.BeginVertical();
                        GUILayout.Space(6);
                        if (GUILayout.Button(new GUIContent(rI, ""), GUIStyle.none, GUILayout.Height(16), GUILayout.Width(16)))
                            pendentMethodToDelete = methodCode.Replace("-method=", "");
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(2);
                    EditorGUILayout.EndHorizontal();

                    //============================= Start Renderization Of Components

                    for (int i = 0; i < methodComponents.Count; i++)
                    {
                        //Prepare the component icon and name
                        Texture componentIcon = null;
                        string componentName = "";
                        string componentType = "";
                        //Load the component icon and name
                        if (methodComponents[i].Contains("--demarcator=") == true)
                        {
                            componentIcon = cTypes[0];
                            componentName = "<color=#004680>Demarcator</color>";
                            componentType = methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Replace("--demarcator=", "");
                        }
                        if (methodComponents[i].Contains("--functionSimple=") == true)
                        {
                            componentIcon = cTypes[1];
                            componentName = "FunctionSimple";
                            componentType = methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Replace("--functionSimple=", "");
                        }
                        if (methodComponents[i].Contains("--functionNormal=") == true)
                        {
                            componentIcon = cTypes[2];
                            componentName = "FunctionNormal";
                            componentType = methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Replace("--functionNormal=", "");
                        }
                        if (methodComponents[i].Contains("--condition=") == true)
                        {
                            componentIcon = cTypes[3];
                            componentName = "Condition";
                            componentType = methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Replace("--condition=", "");
                        }
                        if (methodComponents[i].Contains("--comment=") == true)
                        {
                            componentIcon = cTypes[4];
                            componentName = "<color=#06510e>Comment</color>";
                            componentType = methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Replace("--comment=", "");
                        }

                        //Render the component box
                        GUILayout.Space(8);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Box(componentIcon, iconStyle, GUILayout.Width(20), GUILayout.Height(20));
                        GUILayout.Space(4);
                        GUILayout.BeginVertical();
                        GUILayout.Space(6);
                        EditorGUILayout.LabelField("<b>" + componentName + "</b> \"" + componentType + "\"", new GUIStyle() { richText = true });
                        GUILayout.Space(4);
                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical();
                        GUILayout.Space(6);
                        string thisComponentIndex = i.ToString();
                        if (GUILayout.Button(new GUIContent(opt, ""), GUIStyle.none, GUILayout.Height(16), GUILayout.Width(16)))
                        {
                            //Prepare the options
                            GenericMenu genericMenu = new GenericMenu();

                            //Show options
                            if (i > 0)
                                genericMenu.AddItem(new GUIContent("Move Up"), false, () => { pendentComponentToMoveUpParentMethod = methodCode.Replace("-method=", ""); pendentComponentToMoveUpIndex = int.Parse(thisComponentIndex); });
                            if (i == 0)
                                genericMenu.AddDisabledItem(new GUIContent("Move Up"));
                            if (i < (methodComponents.Count - 1))
                                genericMenu.AddItem(new GUIContent("Move Down"), false, () => { pendentComponentToMoveDownParentMethod = methodCode.Replace("-method=", ""); pendentComponentToMoveDownIndex = int.Parse(thisComponentIndex); });
                            if (i == (methodComponents.Count - 1))
                                genericMenu.AddDisabledItem(new GUIContent("Move Down"));
                            if (currentTaskSourceCodeBeingEdited.Keys.Count > 1 && componentName != "Condition")
                                foreach (var key in currentTaskSourceCodeBeingEdited)
                                    if (key.Key != methodCode)   //<- Only show methods that is not the current
                                        genericMenu.AddItem(new GUIContent("Move To/" + key.Key.Replace("-method=", "")), false, () =>
                                        {
                                            pendentComponentToBeMovedCode = currentTaskSourceCodeBeingEdited[methodCode][int.Parse(thisComponentIndex)];
                                            pendentComponentToBeMovedTargetMethod = key.Key;
                                            pendentComponentDoDeleteParentMethod = methodCode.Replace("-method=", ""); pendentComponentDoDeleteIndex = int.Parse(thisComponentIndex);
                                        });
                            if (currentTaskSourceCodeBeingEdited.Keys.Count == 1 || componentName == "Condition")
                                genericMenu.AddDisabledItem(new GUIContent("Move To"));
                            if (componentName != "Condition")
                                foreach (var key in currentTaskSourceCodeBeingEdited)
                                    genericMenu.AddItem(new GUIContent("Copy To/" + key.Key.Replace("-method=", "")), false, () =>
                                        {
                                            pendentComponentToBeCopiedCode = currentTaskSourceCodeBeingEdited[methodCode][int.Parse(thisComponentIndex)];
                                            pendentComponentToBeCopiedTargetMethod = key.Key;
                                        });
                            if (componentName == "Condition")
                                genericMenu.AddDisabledItem(new GUIContent("Copy To"));
                            genericMenu.AddItem(new GUIContent("Remove"), false, () => { pendentComponentDoDeleteParentMethod = methodCode.Replace("-method=", ""); pendentComponentDoDeleteIndex = int.Parse(thisComponentIndex); });

                            //Render the context menu
                            genericMenu.ShowAsContext();
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(2);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        EditorGUILayout.BeginVertical(GUILayout.Width(489));
                        //============================= Start Renderization Of Parameters And UI For This Component

                        SourceCode_RenderComponentParametersHere_FindTheFunctionToRenderParametersOfThisComponent(methodCode, i);

                        //=============================== End Renderization Of Parameters And UI For This Component
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        if (i < (methodComponents.Count - 1))   //<- Only render the separator if this is not the last component of this method
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Box(tSep, iconStyle, GUILayout.Width(570), GUILayout.Height(1));
                            GUILayout.Space(-200);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    //=============================== End Renderization Of Components

                    EditorGUILayout.BeginVertical(GUILayout.Width(570));
                    if (methodComponents.Count == 0)
                    {
                        GUILayout.Space(8);
                        EditorGUILayout.HelpBox("It looks like there is no component in this method. Click the \"Plus\" button to add a new component here!", MessageType.Warning);
                    }
                    if (methodCode == "-method=main")
                    {
                        GUILayout.Space(8);
                        EditorGUILayout.HelpBox("This is the primary method. This method will always be executed first.", MessageType.Info);
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(1);
                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    //Add the position of start and end of this method in the design view
                    int methodRenderizationSize = (int)(GUILayoutUtility.GetLastRect().height + 10);
                    if (methodRenderizationSize > 11)   //<- To ignore fakes sizes generated by the GetLastRect() API
                    {
                        int startPosition = lastFinalPositionOfMethodInDesignMode;
                        int endPosition = (lastFinalPositionOfMethodInDesignMode + methodRenderizationSize);
                        if (methodsPositionInDesignMode.ContainsKey(methodCode) == false)
                            methodsPositionInDesignMode.Add(methodCode, new MethodPositionInDesignMode(startPosition, endPosition));
                        if (methodsPositionInDesignMode.ContainsKey(methodCode) == true)
                        {
                            methodsPositionInDesignMode[methodCode].startPosition = startPosition;
                            methodsPositionInDesignMode[methodCode].endPosition = endPosition;
                        }
                        lastFinalPositionOfMethodInDesignMode = (endPosition + 1);
                    }

                    //Render a space
                    GUILayout.Space(10);
                }

                //============================= Render the AddMethod interface
                GUILayout.Space(10);
                if (isAddingNewMethod == false)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add New Method", GUILayout.Height(18), GUILayout.Width(164)))
                        isAddingNewMethod = true;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (isAddingNewMethod == true)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    methodNameThatIsAdding = EditorGUILayout.TextField(new GUIContent("",
                                "Enter the name of the method to be added here."),
                                methodNameThatIsAdding);
                    if (GUILayout.Button("Cancel", GUILayout.Height(18), GUILayout.Width(64)))
                    {
                        methodNameThatIsAdding = "newMethodName";
                        isAddingNewMethod = false;
                    }
                    if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(64)))
                    {
                        if (methodNameThatIsAdding != "" && methodNameThatIsAdding.Contains(" ") == false && Regex.IsMatch(methodNameThatIsAdding, @"^[a-zA-Z]+$") == true && currentTaskSourceCodeBeingEdited.ContainsKey("-method=" + methodNameThatIsAdding) == false && methodNameThatIsAdding.ToLower() != "none" && methodNameThatIsAdding.ToLower() != "return")
                        {
                            pendentMethodToCreate = methodNameThatIsAdding;
                            methodNameThatIsAdding = "newMethodName";
                            isAddingNewMethod = false;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error Adding Method", "Could not add this method. Please verify that the method name entered matches the requirements below.\n\n- Can not be empty.\n- Cannot contain spaces\n- Can only contain letters\n- Cannot be equal to \"return\"\n- Cannot be equal to \"none\"\n- Can't already be added.", "Ok");
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);

                //------------------------------------------------  End the code of ComponentsRenderization -------------------------------------------------
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                //============================== Do the pendent actions
                if (pendentMethodToCreate != "")                       //<- Create new method
                {
                    currentTaskSourceCodeBeingEdited.Add("-method=" + pendentMethodToCreate, new List<string>());

                    pendentMethodToCreate = "";
                }
                if (pendentMethodToDelete != "")                       //<- Delete method
                {
                    currentTaskSourceCodeBeingEdited.Remove("-method=" + pendentMethodToDelete);

                    pendentMethodToDelete = "";
                }
                if (pendentComponentDoDeleteParentMethod != "")        //<- Delete component
                {
                    currentTaskSourceCodeBeingEdited["-method=" + pendentComponentDoDeleteParentMethod].RemoveAt(pendentComponentDoDeleteIndex);

                    pendentComponentDoDeleteParentMethod = "";
                    pendentComponentDoDeleteIndex = -1;
                }
                if (pendentComponentToMoveUpParentMethod != "")        //<- Move up component
                {
                    string componentCode = currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveUpParentMethod][pendentComponentToMoveUpIndex];
                    currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveUpParentMethod].RemoveAt(pendentComponentToMoveUpIndex);
                    currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveUpParentMethod].Insert(pendentComponentToMoveUpIndex - 1, componentCode);

                    pendentComponentToMoveUpParentMethod = "";
                    pendentComponentToMoveUpIndex = -1;
                }
                if (pendentComponentToMoveDownParentMethod != "")      //<- Move down component
                {
                    string componentCode = currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveDownParentMethod][pendentComponentToMoveDownIndex];
                    currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveDownParentMethod].RemoveAt(pendentComponentToMoveDownIndex);
                    currentTaskSourceCodeBeingEdited["-method=" + pendentComponentToMoveDownParentMethod].Insert(pendentComponentToMoveDownIndex + 1, componentCode);

                    pendentComponentToMoveDownParentMethod = "";
                    pendentComponentToMoveDownIndex = -1;
                }
                if (pendentComponentToBeMovedCode != "")                //<- Move to component
                {
                    currentTaskSourceCodeBeingEdited[pendentComponentToBeMovedTargetMethod].Add(pendentComponentToBeMovedCode);

                    pendentComponentToBeMovedTargetMethod = "";
                    pendentComponentToBeMovedCode = "";
                }
                if (pendentComponentToBeCopiedCode != "")                //<- Copy to component
                {
                    currentTaskSourceCodeBeingEdited[pendentComponentToBeCopiedTargetMethod].Add(pendentComponentToBeCopiedCode);

                    pendentComponentToBeCopiedTargetMethod = "";
                    pendentComponentToBeCopiedCode = "";
                }

                //Map renderization
                EditorGUILayout.BeginVertical("box");
                scrollPosDesignMapModeView = EditorGUILayout.BeginScrollView(scrollPosDesignMapModeView, GUILayout.Width(250), GUILayout.Height(520));
                //------------------------------------------------ Start the code of MapRenderization ------------------------------------------------

                //Render each method and components inside it
                foreach (var method in currentTaskSourceCodeBeingEdited)
                {
                    //Get the method data
                    string methodCode = method.Key;
                    List<string> methodComponents = method.Value;

                    //Calculate if this method is in the screen of the user in the design mode
                    bool isThisMethodCurrentlyOnTheScreenOfTheUser = false;
                    if (methodsPositionInDesignMode.ContainsKey(methodCode) == true)
                    {
                        //Prepare the variables
                        int startScrollPosition = (int)scrollPosDesignModeView.y;
                        int endScrollPosition = ((int)scrollPosDesignModeView.y + 500);
                        int startPosition = methodsPositionInDesignMode[methodCode].startPosition;
                        int endPosition = methodsPositionInDesignMode[methodCode].endPosition;

                        //Do the conclusion...
                        if (startScrollPosition == startPosition && endScrollPosition >= endPosition)
                            isThisMethodCurrentlyOnTheScreenOfTheUser = true;
                        if (startScrollPosition < startPosition && endScrollPosition > endPosition)
                            isThisMethodCurrentlyOnTheScreenOfTheUser = true;
                        if (startScrollPosition < startPosition && endScrollPosition > startPosition)
                            isThisMethodCurrentlyOnTheScreenOfTheUser = true;
                        if (startScrollPosition < endPosition && endScrollPosition > endPosition)
                            isThisMethodCurrentlyOnTheScreenOfTheUser = true;
                        if (startScrollPosition >= startPosition && endScrollPosition < endPosition)
                            isThisMethodCurrentlyOnTheScreenOfTheUser = true;
                    }

                    //Render the method
                    GUIStyle methodNormal = new GUIStyle();
                    methodNormal.normal.background = (Texture2D)mBg;
                    GUIStyle methodScreen = new GUIStyle();
                    methodScreen.normal.background = (Texture2D)mFg;
                    if (isThisMethodCurrentlyOnTheScreenOfTheUser == false)
                        EditorGUILayout.BeginVertical(methodNormal);
                    if (isThisMethodCurrentlyOnTheScreenOfTheUser == true)
                        EditorGUILayout.BeginVertical(methodScreen);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(mIc, iconStyle, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.Space(4);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(6);
                    EditorGUILayout.LabelField("<b>Method</b> \"" + methodCode.Replace("-method=", "") + "\"", new GUIStyle() { richText = true }, GUILayout.Width(200));
                    GUILayout.Space(4);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    //Render the components name
                    for (int i = 0; i < methodComponents.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(31);
                        EditorGUILayout.LabelField(methodComponents[i].Split(new string[] { "|||" }, StringSplitOptions.None)[0].Split('=')[1], new GUIStyle() { richText = true }, GUILayout.Width(200));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    //Render a space
                    GUILayout.Space(8);
                }

                //------------------------------------------------  End the code of MapRenderization -------------------------------------------------
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                //Finish the UI of the Design for task creator
                EditorGUILayout.EndHorizontal();
            }
        }

        void UI_View_DesignMode_ProcessFilesAndVariables()
        {
            //Get the current date time
            long currentDateTimeTicks = DateTime.Now.Ticks;

            //If has passed more than 3 seconds since last Runtime Keys update
            if ((new TimeSpan(currentDateTimeTicks).Subtract(new TimeSpan(lastRuntimeKeysUpdate))).TotalSeconds >= 3)
            {
                //Reset the database
                currentTaskSourceCodeRuntimeKeys.Clear();

                //Read the file of referenced keys and store all referenced keys
                List<string> listOfReferencedKeysInFile = new List<string>();
                string[] referencedKeys = new string[0];
                if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATTasksReferencedKeys.ini") == true)
                    referencedKeys = File.ReadAllLines("Assets/Plugins/MT Assets/_AssetsData/Editor/NATTasksReferencedKeys.ini");
                if (referencedKeys.Length > 0)
                    foreach (string referencedKey in referencedKeys)
                        listOfReferencedKeysInFile.Add(referencedKey);

                //Read entire code to extract all Runtime Keys
                foreach (var method in currentTaskSourceCodeBeingEdited)
                {
                    //Get data about this item
                    string methodCode = method.Key;

                    //Read all components for this method
                    foreach (var component in currentTaskSourceCodeBeingEdited[methodCode])
                    {
                        //Extract all parameters
                        string[] parameters = component.Split(new string[] { "|||" }, StringSplitOptions.None);

                        //Read all parameters of this component
                        foreach (var parameter in parameters)
                        {
                            //Extract parameter data, splitting the divisor, only for the first ocurrency
                            string[] parameterData = parameter.Split(new[] { '=' }, 2);

                            //If the data of parameter contains runtime key metadata, extract the key and store into database
                            if (parameterData[1].Contains("^runtimeValue^") == true)
                            {
                                //Extract all value info
                                string[] valueInfo = parameterData[1].Split('^');

                                //Build the key to add to dictionary
                                string dictionaryKey = valueInfo[0] + " " + valueInfo[2];

                                //Add the runtime key to database
                                if (currentTaskSourceCodeRuntimeKeys.ContainsKey(dictionaryKey) == false)
                                {
                                    RuntimeKeyInformation runtimeKeyInformation = new RuntimeKeyInformation();
                                    runtimeKeyInformation.keyTipe = valueInfo[0];
                                    runtimeKeyInformation.key = valueInfo[2].Replace("%", "");
                                    runtimeKeyInformation.isBeingUsedByCsharp = listOfReferencedKeysInFile.Contains(runtimeKeyInformation.key);
                                    currentTaskSourceCodeRuntimeKeys.Add(dictionaryKey, runtimeKeyInformation);
                                }
                                if (currentTaskSourceCodeRuntimeKeys.ContainsKey(dictionaryKey) == true)
                                    currentTaskSourceCodeRuntimeKeys[dictionaryKey].uses += 1;
                            }
                        }
                    }
                }

                //Run a file inspector in a new thread to check if all csharp scripts that references the keys
                if (currentTaskSourceCodeRuntimeKeys.Keys.Count > 0)
                    new Thread(() =>
                    {
                        //Build a list of runtime keys name
                        List<string> listOfKeys = new List<string>();
                        foreach (var runtimeKey in currentTaskSourceCodeRuntimeKeys)
                            listOfKeys.Add(currentTaskSourceCodeRuntimeKeys[runtimeKey.Key].key);

                        //Prepare a list of keys that are referenced in cs scripts
                        List<string> listOfReferencedKeys = new List<string>();

                        //Get all CS scripts
                        foreach (string csPath in Directory.EnumerateFiles("Assets", "*.cs", SearchOption.AllDirectories))
                        {
                            //If is this file, ignore
                            if (Path.GetFileName(csPath) == "TaskCreator.cs")
                                continue;

                            //Read all lines of the current file
                            string[] csLines = File.ReadAllLines(csPath);

                            //Check if any line contains the key
                            foreach (string csLine in csLines)
                                foreach (string runtimeKey in listOfKeys)
                                    if (csLine.Contains("setRuntimeValue") == true && csLine.Contains("\"" + runtimeKey + "\"") == true)
                                        if (listOfReferencedKeys.Contains(runtimeKey) == false)
                                            listOfReferencedKeys.Add(runtimeKey);
                        }

                        //Write all referenced keys of the list, in a file
                        File.WriteAllLines("Assets/Plugins/MT Assets/_AssetsData/Editor/NATTasksReferencedKeys.ini", listOfReferencedKeys.ToArray());

                        //Reset the time to update runtime keys interface, if this is the first time of the thread checking cs files
                        if (alreadyCheckedTheRuntimeKeysInCsharpCodes == false)
                        {
                            lastRuntimeKeysUpdate = 0;
                            alreadyCheckedTheRuntimeKeysInCsharpCodes = true;
                        }
                    }).Start();

                //Inform that was loaded
                alreadyLoadedTheRuntimeKeys = true;
                //Reset the timer
                lastRuntimeKeysUpdate = DateTime.Now.Ticks;
            }

            //If has passed more than 1.5 seconds since last File Values update
            if ((new TimeSpan(currentDateTimeTicks).Subtract(new TimeSpan(lastFileValuesUpdate))).TotalSeconds >= 1.5f)
            {
                //Reset the database
                currentTaskSourceCodeFileValues.Clear();

                //Read entire code to extract all File Values
                foreach (var method in currentTaskSourceCodeBeingEdited)
                {
                    //Get data about this item
                    string methodCode = method.Key;

                    //Read all components for this method
                    foreach (var component in currentTaskSourceCodeBeingEdited[methodCode])
                    {
                        //Extract all parameters
                        string[] parameters = component.Split(new string[] { "|||" }, StringSplitOptions.None);

                        //Read all parameters of this component
                        foreach (var parameter in parameters)
                        {
                            //Extract parameter data, splitting the divisor, only for the first ocurrency
                            string[] parameterData = parameter.Split(new[] { '=' }, 2);

                            //If the data of parameter contains runtime key metadata, extract the key and store into database
                            if (parameterData[1].Contains("^fileValue^") == true)
                            {
                                //Extract all value info
                                string[] valueInfo = parameterData[1].Split('^');

                                //Build the key to add to dictionary
                                string dictionaryKey = valueInfo[2];

                                //Add the runtime key to database
                                if (currentTaskSourceCodeFileValues.ContainsKey(dictionaryKey) == false)
                                {
                                    FileValueInformation fileValueInformation = new FileValueInformation();
                                    fileValueInformation.fileType = valueInfo[0];
                                    fileValueInformation.file = valueInfo[2];
                                    currentTaskSourceCodeFileValues.Add(dictionaryKey, fileValueInformation);
                                }
                                if (currentTaskSourceCodeFileValues.ContainsKey(dictionaryKey) == true)
                                    currentTaskSourceCodeFileValues[dictionaryKey].listOfComponentsUsing.Add(valueInfo[0] + " " + methodCode.Replace("-method=", "") + " > " + parameters[0].Split('=')[1]);
                            }
                        }
                    }
                }

                //Inform that was loaded
                alreadyLoadedTheFileValues = true;
                //Reset the timer
                lastFileValuesUpdate = DateTime.Now.Ticks;
            }
        }

        void UI_View_SourceMode()
        {
            //Render the souce code in pure text
            EditorGUILayout.BeginVertical("box");
            scrollPosSourceModeView = EditorGUILayout.BeginScrollView(scrollPosSourceModeView, GUILayout.Width(1110), GUILayout.Height(520));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("This is the backend source code responsible for running your Task.\n");
            foreach (var item in currentTaskSourceCodeBeingEdited)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(item.Key);
                foreach (string str in currentTaskSourceCodeBeingEdited[item.Key])
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append(str);
                }
            }
            GUILayout.Label(stringBuilder.ToString(), EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void OnInspectorUpdate()
        {
            //If the last update of the UI is done on much time, force the update of the UI
            if ((new TimeSpan(DateTime.Now.Ticks).Subtract(new TimeSpan(lastUpdateOfTaskCreatorUI))).TotalSeconds >= 1)
            {
                Repaint();

                //Reset the timer
                lastUpdateOfTaskCreatorUI = DateTime.Now.Ticks;
            }
        }

        //Source code manipulation methods

        bool SourceCode_isEmpty()
        {
            //Prepare the value
            bool isEmpty = false;

            //If not have the main method
            if (currentTaskSourceCodeBeingEdited.ContainsKey("-method=main") == false)
                isEmpty = true;

            //Return the value
            return isEmpty;
        }

        string SourceCode_isValid()
        {
            //Store the error
            string errorsFound = "";

            //If the code is empty
            if (SourceCode_isEmpty() == true)
                errorsFound += "\n" + "- Your source code is empty.";

            //If not have at least one function in the main method
            if (currentTaskSourceCodeBeingEdited.ContainsKey("-method=main") == true)
                if (currentTaskSourceCodeBeingEdited["-method=main"].Count == 0)
                    errorsFound += "\n" + "- There is no one component in the \"main\" method.";

            //If runtime keys and file values informations not loaded yet
            if (alreadyLoadedTheRuntimeKeys == false || alreadyLoadedTheRuntimeKeys == false)
                errorsFound += "\n" + "- Runtime Keys and File Values data has not yet been loaded.";

            //If have runtime keys being used by more than one type of variable
            if (currentTaskSourceCodeRuntimeKeys.Keys.Count > 0)
                foreach (var runtimeKey in currentTaskSourceCodeRuntimeKeys)
                {
                    //Get this key
                    string keyName = runtimeKey.Key.Split(new[] { ' ' }, 2)[1];
                    int ocurrencesInDatabase = 0;

                    //Check if have another runtime keys in database
                    foreach (var runtimeKeyChecking in currentTaskSourceCodeRuntimeKeys)
                        if (runtimeKeyChecking.Key.Contains(keyName) == true)
                            ocurrencesInDatabase += 1;

                    //If have more than one ocurrences, inform duplicated keys
                    if (ocurrencesInDatabase > 1)
                        errorsFound += "\n" + "- The Key \"" + runtimeKey.Key.Replace("%", "") + "\" is duplicated. A Key can only be associated with one type of variable.";
                }

            //return the errors
            return errorsFound;
        }

        //Source code of components like conditions, functions, comments etc. Register new components here to show in context menu

        void SourceCode_RenderContextMenuToAddNewComponenteHere(string methodCode)
        {
            //Prepare the context menu
            GenericMenu genericMenu = new GenericMenu();
            bool isMainMethod = (methodCode == "-method=main") ? true : false;

            //Sections
            genericMenu.AddItem(new GUIContent("Demarcators/Section"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--demarcator=section"); });

            //Simple functions
            genericMenu.AddItem(new GUIContent("Functions Simple/ShowToast"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=showToast"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/SendNotification"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=sendNotification"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/CreateFile"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=createFile"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/DeleteFile"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=deleteFile"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/RescheduleThisTask"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=rescheduleThisTask"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/PlaySound"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=playSound"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/BuildAndSaveString"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=buildAndSaveString"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/Vibrate"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=vibrate"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/Arithmetic"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=arithmetic"); });
            genericMenu.AddItem(new GUIContent("Functions Simple/WaitForSeconds"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionSimple=waitForSeconds"); });

            //Normal functions
            genericMenu.AddItem(new GUIContent("Functions Normal/LoadJsonFromWeb"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionNormal=loadJsonFromWeb"); });
            genericMenu.AddItem(new GUIContent("Functions Normal/DownloadFileFromWeb"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionNormal=downloadFileFromWeb"); });
            genericMenu.AddItem(new GUIContent("Functions Normal/FileExists"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionNormal=fileExists"); });
            genericMenu.AddItem(new GUIContent("Functions Normal/GetFileSize"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionNormal=getFileSize"); });
            genericMenu.AddItem(new GUIContent("Functions Normal/ClearTaskDirectory"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--functionNormal=clearTaskDirectory"); });

            //Conditions
            genericMenu.AddItem(new GUIContent("Conditions/IfAndElse"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--condition=ifAndElse"); });
            if (isMainMethod == true)
                genericMenu.AddItem(new GUIContent("Conditions/While"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--condition=while"); });
            if (isMainMethod == false)
                genericMenu.AddDisabledItem(new GUIContent("Conditions/While"));
            if (isMainMethod == true)
                genericMenu.AddItem(new GUIContent("Conditions/CallMethod"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--condition=callMethod"); });
            if (isMainMethod == false)
                genericMenu.AddDisabledItem(new GUIContent("Conditions/CallMethod"));
            genericMenu.AddItem(new GUIContent("Conditions/GoToSection"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--condition=goToSection"); });

            //Comments
            genericMenu.AddItem(new GUIContent("Comments/SimpleComment"), false, () => { currentTaskSourceCodeBeingEdited[methodCode].Add("--comment=simpleComment"); });

            //Render the context menu
            genericMenu.ShowAsContext();
        }

        //Source code of components like conditions, functions, comments etc. Register new componentes here to show its parameters and UI

        void SourceCode_RenderComponentParametersHere_FindTheFunctionToRenderParametersOfThisComponent(string parentMethodCode, int componentIndexInMethod)
        {
            //This method will try to find the best function to render the UI and parameters for this component
            string componentCode = currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod];

            //Extract the component type
            string componentType = componentCode.Split(new string[] { "|||" }, StringSplitOptions.None)[0];

            //Use the correct method to render UI and parameters for this component
            switch (componentType)
            {
                case "--demarcator=section":
                    SourceCode_RenderComponentParametersHere_Demarcators_Section(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=showToast":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_ShowToast(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=sendNotification":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_SendNotification(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=createFile":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_CreateFile(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=deleteFile":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_DeleteFile(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=rescheduleThisTask":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_RescheduleThisTask(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=playSound":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_PlaySound(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=buildAndSaveString":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_BuildAndSaveString(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=vibrate":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_Vibrate(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=arithmetic":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_Arithmetic(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionSimple=waitForSeconds":
                    SourceCode_RenderComponentParametersHere_FunctionSimple_WaitForSeconds(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionNormal=loadJsonFromWeb":
                    SourceCode_RenderComponentParametersHere_FunctionNormal_LoadJsonFromWeb(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionNormal=downloadFileFromWeb":
                    SourceCode_RenderComponentParametersHere_FunctionNormal_DownloadFileFromWeb(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionNormal=fileExists":
                    SourceCode_RenderComponentParametersHere_FunctionNormal_FileExists(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionNormal=getFileSize":
                    SourceCode_RenderComponentParametersHere_FunctionNormal_GetFileSize(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--functionNormal=clearTaskDirectory":
                    SourceCode_RenderComponentParametersHere_FunctionNormal_ClearTaskDirectory(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--condition=ifAndElse":
                    SourceCode_RenderComponentParametersHere_Condition_IfAndElse(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--condition=while":
                    SourceCode_RenderComponentParametersHere_Condition_While(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--condition=callMethod":
                    SourceCode_RenderComponentParametersHere_Condition_CallMethod(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--condition=goToSection":
                    SourceCode_RenderComponentParametersHere_Condition_GoToSection(componentType, parentMethodCode, componentIndexInMethod);
                    break;
                case "--comment=simpleComment":
                    SourceCode_RenderComponentParametersHere_Comment_SimpleComment(componentType, parentMethodCode, componentIndexInMethod);
                    break;
            }
        }

        string[] SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(string componentType, string parentMethodCode, int componentIndexInMethod, string[] defaultParametersOfComponent)
        {
            //Prepare the storage to return parameters of this component
            string[] parametersOnCode = currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod].Split(new string[] { "|||" }, StringSplitOptions.None);

            //If have less parameters than expected or incorrect parameters, reset the component code
            if (parametersOnCode.Length < defaultParametersOfComponent.Length)
            {
                currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentType;
                foreach (string parameter in defaultParametersOfComponent)
                    currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] += "|||" + parameter;
                Debug.Log("NAT Task Creator: The new component \"" + componentType + "\" of method " + parentMethodCode + " in line " + componentIndexInMethod + " was initialized!");
            }

            //Return the parameters of this component
            return parametersOnCode;
        }

        void SourceCode_RenderComponentParametersHere_Demarcators_Section(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "lineInMethod=", "sectionName=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string lineInMethod = parametersOfComponent[1].Replace("lineInMethod=", "");
            string sectionName = parametersOfComponent[2].Replace("sectionName=", "");

            //Render the parameters of this component to the user
            lineInMethod = componentIndexInMethod.ToString();

            sectionName = EditorGUILayout.TextField("", sectionName);
            if (sectionName == "")
                sectionName = "section_" + componentIndexInMethod;
            sectionName = sectionName.Replace("§", "").Replace("|", "").Replace("^", "").Replace("\n", "").Replace("\r", "").Replace(" ", "_");
            sectionName = new Regex("[^a-zA-Z0-9 _]").Replace(sectionName, "");

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("lineInMethod=", lineInMethod)
            .addComponentParameter("sectionName=", sectionName)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_ShowToast(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "toastMessage=", "longDuration=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string toastMessage = parametersOfComponent[1].Replace("toastMessage=", "");
            string longDuration = parametersOfComponent[2].Replace("longDuration=", "");

            //Render the parameters of this component to the user
            toastMessage = SourceCode_RenderComponentVariableUIHere_String("Toast Message",
                                        "Enter here the message that will be displayed by Toast.",
                                        toastMessage);

            longDuration = SourceCode_RenderComponentVariableUIHere_Boolean("Long Duration",
                                        "Turn this on so the Toast lasts for 3 seconds instead of 1.5 seconds.",
                                        longDuration);

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("toastMessage=", toastMessage)
            .addComponentParameter("longDuration=", longDuration)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_SendNotification(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "title=", "message=", "clickAction=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string title = parametersOfComponent[1].Replace("title=", "");
            string message = parametersOfComponent[2].Replace("message=", "");
            string clickAction = parametersOfComponent[3].Replace("clickAction=", "");

            //Render the parameters of this component to the user
            title = SourceCode_RenderComponentVariableUIHere_String("Title",
                                        "The title of the notification to be sended.",
                                        title);

            message = SourceCode_RenderComponentVariableUIHere_String("Message",
                                        "The message of the notification to be sended.",
                                        message);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Click Action", "Enter here the name of a Notification Action that should be executed when the user clicks on the notification."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] notificationActions = new string[Enum.GetValues(typeof(NotificationsActions.Action)).Length];
            for (int i = 0; i < notificationActions.Length; i++)
                notificationActions[i] = Enum.GetName(typeof(NotificationsActions.Action), i);
            int notificationActionsInt = Array.IndexOf<string>(notificationActions, clickAction);
            if (notificationActionsInt == -1)
                notificationActionsInt = 0;
            clickAction = notificationActions[EditorGUILayout.Popup("", notificationActionsInt, notificationActions, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("title=", title)
            .addComponentParameter("message=", message)
            .addComponentParameter("clickAction=", clickAction)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_CreateFile(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "contentTypeToWrite=", "fileContent=", "saveOnPath=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string contentTypeToWrite = parametersOfComponent[1].Replace("contentTypeToWrite=", "");
            string fileContent = parametersOfComponent[2].Replace("fileContent=", "");
            string saveOnPath = parametersOfComponent[3].Replace("saveOnPath=", "");

            //Render the parameters of this component to the user
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Content Type To Write", "The type of content that will be written in the file."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] contentTypes = new string[] { "string", "float", "integer", "boolean" };
            int contentTypesInt = Array.IndexOf<string>(contentTypes, contentTypeToWrite);
            if (contentTypesInt == -1)
                contentTypesInt = 0;
            contentTypeToWrite = contentTypes[EditorGUILayout.Popup("", contentTypesInt, contentTypes, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            string fileContentDescription = "The content of the file to be saved. Here you can write STRINGS. If you want to create a file containing a single value that can be read by \"fileValue\" from other components, insert an INT, BOOL or FLOAT value here in this content. INT values must be formatted as follows: \"64\" or \"-63\" (without quotes). BOOL values must be formatted as follows: \"True\" or \"False\" (without quotes). FLOAT values must be formatted as follows: \"64.3\" or \"-64.3\" (without quotes).";
            if (contentTypeToWrite == "string")
                fileContent = SourceCode_RenderComponentVariableUIHere_String("File Content", fileContentDescription, fileContent);
            if (contentTypeToWrite == "float")
                fileContent = SourceCode_RenderComponentVariableUIHere_Float("File Content", fileContentDescription, fileContent, -999999999999999999, 999999999999999999);
            if (contentTypeToWrite == "integer")
                fileContent = SourceCode_RenderComponentVariableUIHere_Integer("File Content", fileContentDescription, fileContent, -999999999, 999999999);
            if (contentTypeToWrite == "boolean")
                fileContent = SourceCode_RenderComponentVariableUIHere_Boolean("File Content", fileContentDescription, fileContent);

            saveOnPath = SourceCode_RenderComponentVariableUIHere_String("Save On Path",
                                        "The desired directory where the file should be saved, including its name and desired extension. If the file already exists, it will be overwritten. Directories informed here that do not exist will be created automatically.",
                                        saveOnPath);

            //Fix the variable type to "fileValue" only for the Audio Path
            string[] parametersMetadata = saveOnPath.Split('^');
            if (parametersMetadata[1] != "fileValue")
            {
                saveOnPath = "string^fileValue^example.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save On Path\" can only be type \"fileValue\".");
            }
            if (parametersMetadata[2] == "")
            {
                saveOnPath = "string^editorValue^example.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save On Path\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("contentTypeToWrite=", contentTypeToWrite)
            .addComponentParameter("fileContent=", fileContent)
            .addComponentParameter("saveOnPath=", saveOnPath)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_DeleteFile(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "deleteOnPath=", "deleteRecursively=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string deleteOnPath = parametersOfComponent[1].Replace("deleteOnPath=", "");
            string deleteRecursively = parametersOfComponent[2].Replace("deleteRecursively=", "");

            //Render the parameters of this component to the user
            deleteOnPath = SourceCode_RenderComponentVariableUIHere_String("Delete On Path",
                                        "The path of the file to be deleted.",
                                        deleteOnPath);

            deleteRecursively = SourceCode_RenderComponentVariableUIHere_Boolean("Delete Recursively",
                                        "Delete all files and folders if the above directory is a folder.",
                                        deleteRecursively);

            //Fix the variable type to "fileValue" only for the Audio Path
            string[] parametersMetadata = deleteOnPath.Split('^');
            if (parametersMetadata[1] != "fileValue")
            {
                deleteOnPath = "string^fileValue^example.txt";
                Debug.LogWarning("NAT Task Creator: The \"Delete On Path\" can only be type \"fileValue\".");
            }
            if (parametersMetadata[2] == "")
            {
                deleteOnPath = "string^editorValue^example.txt";
                Debug.LogWarning("NAT Task Creator: The \"Delete On Path\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("deleteOnPath=", deleteOnPath)
            .addComponentParameter("deleteRecursively=", deleteRecursively)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_RescheduleThisTask(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "yearsInFuture=", "monthsInFuture=", "daysInFuture=", "hoursInFuture=", "minutesInFuture=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string yearsInFuture = parametersOfComponent[1].Replace("yearsInFuture=", "");
            string monthsInFuture = parametersOfComponent[2].Replace("monthsInFuture=", "");
            string daysInFuture = parametersOfComponent[3].Replace("daysInFuture=", "");
            string hoursInFuture = parametersOfComponent[4].Replace("hoursInFuture=", "");
            string minutesInFuture = parametersOfComponent[5].Replace("minutesInFuture=", "");

            //Render the parameters of this component to the user
            yearsInFuture = SourceCode_RenderComponentVariableUIHere_Integer("Years In Future",
                                        "How many YEARS into the future should the current task be scheduled to be re-executed?",
                                        yearsInFuture, 0, 9999);

            monthsInFuture = SourceCode_RenderComponentVariableUIHere_Integer("Months In Future",
                                        "How many MONTHS into the future should the current task be scheduled to be re-executed?",
                                        monthsInFuture, 0, 9999);

            daysInFuture = SourceCode_RenderComponentVariableUIHere_Integer("Days In Future",
                                        "How many DAYS into the future should the current task be scheduled to be re-executed?",
                                        daysInFuture, 0, 9999);

            hoursInFuture = SourceCode_RenderComponentVariableUIHere_Integer("Hours In Future",
                                        "How many HOURS into the future should the current task be scheduled to be re-executed?",
                                        hoursInFuture, 0, 9999);

            minutesInFuture = SourceCode_RenderComponentVariableUIHere_Integer("Minutes In Future",
                                        "How many MINUTES into the future should the current task be scheduled to be re-executed?",
                                        minutesInFuture, 1, 9999);

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("yearsInFuture=", yearsInFuture)
            .addComponentParameter("monthsInFuture=", monthsInFuture)
            .addComponentParameter("daysInFuture=", daysInFuture)
            .addComponentParameter("hoursInFuture=", hoursInFuture)
            .addComponentParameter("minutesInFuture=", minutesInFuture)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_PlaySound(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "audioPath=", "volume=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string audioPath = parametersOfComponent[1].Replace("audioPath=", "");
            string volume = parametersOfComponent[2].Replace("volume=", "");

            //Render the parameters of this component to the user
            audioPath = SourceCode_RenderComponentVariableUIHere_String("Audio Path",
                                        "The path to the audio file. The audio file must be within the scope of files \"App Files\" and within the \"NAT/Tasks\" folder.",
                                        audioPath);

            volume = SourceCode_RenderComponentVariableUIHere_Float("Volume",
                                        "Set here the volume at which the audio will be played. The maximum volume is 100 and the minimum is 0.",
                                        volume, 0.0f, 100.0f);

            //Fix the variable type to "fileValue" only for the Audio Path
            string[] parametersMetadata = audioPath.Split('^');
            if (parametersMetadata[1] != "fileValue")
            {
                audioPath = "string^fileValue^example.ogg";
                Debug.LogWarning("NAT Task Creator: The \"Audio Path\" can only be type \"fileValue\".");
            }
            if (parametersMetadata[2] == "")
            {
                audioPath = "string^editorValue^example.ogg";
                Debug.LogWarning("NAT Task Creator: The \"Audio Path\" field cannot be empty, it must contain a audio file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("audioPath=", audioPath)
            .addComponentParameter("volume=", volume)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_BuildAndSaveString(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "string1=", "string2=", "string3=", "string4=", "string5=", "string6=", "string7=", "string8=", "string9=", "string10=", "saveFinalStringOn=", "showInUI=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string[] strings = new string[11];
            strings[1] = parametersOfComponent[1].Replace("string1=", "");
            strings[2] = parametersOfComponent[2].Replace("string2=", "");
            strings[3] = parametersOfComponent[3].Replace("string3=", "");
            strings[4] = parametersOfComponent[4].Replace("string4=", "");
            strings[5] = parametersOfComponent[5].Replace("string5=", "");
            strings[6] = parametersOfComponent[6].Replace("string6=", "");
            strings[7] = parametersOfComponent[7].Replace("string7=", "");
            strings[8] = parametersOfComponent[8].Replace("string8=", "");
            strings[9] = parametersOfComponent[9].Replace("string9=", "");
            strings[10] = parametersOfComponent[10].Replace("string10=", "");
            string saveFinalStringOn = parametersOfComponent[11].Replace("saveFinalStringOn=", "");
            string showInUI = parametersOfComponent[12].Replace("showInUI=", "");

            //Render the parameters of this component to the user
            if (showInUI == "")
                showInUI = "2";
            int showInUIInt = int.Parse(showInUI);

            for (int i = 1; i <= showInUIInt; i++)
                strings[i] = SourceCode_RenderComponentVariableUIHere_String("String " + i,
                                            "String " + i + " to be concatenated...",
                                            strings[i]);

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("", ""), GUILayout.Width(148));
            if (showInUIInt < 10)
                if (GUILayout.Button("Add More One String", GUILayout.Height(18)))
                {
                    showInUIInt += 1;
                    showInUI = showInUIInt.ToString();
                }
            if (showInUIInt > 2)
                if (GUILayout.Button("Remove One String", GUILayout.Height(18)))
                {
                    showInUIInt -= 1;
                    showInUI = showInUIInt.ToString();

                    for (int i = (showInUIInt + 1); i <= 10; i++)
                        strings[i] = "";
                }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            saveFinalStringOn = SourceCode_RenderComponentVariableUIHere_String("Save Final String On",
                                        "The path to the file that will final concatenated string will be saved.",
                                        saveFinalStringOn);

            //Fix the variable type to "fileValue" only for the Save Final String On
            string[] parametersMetadata = saveFinalStringOn.Split('^');
            if (parametersMetadata[1] != "fileValue")
            {
                saveFinalStringOn = "string^fileValue^concatenated.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Final String On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata[2] == "")
            {
                saveFinalStringOn = "string^fileValue^concatenated.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Final String On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("string1=", strings[1])
            .addComponentParameter("string2=", strings[2])
            .addComponentParameter("string3=", strings[3])
            .addComponentParameter("string4=", strings[4])
            .addComponentParameter("string5=", strings[5])
            .addComponentParameter("string6=", strings[6])
            .addComponentParameter("string7=", strings[7])
            .addComponentParameter("string8=", strings[8])
            .addComponentParameter("string9=", strings[9])
            .addComponentParameter("string10=", strings[10])
            .addComponentParameter("saveFinalStringOn=", saveFinalStringOn)
            .addComponentParameter("showInUI=", showInUI)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_Vibrate(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "delay1=", "vibration1=", "delay2=", "vibration2=", "delay3=", "vibration3=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string delay1 = parametersOfComponent[1].Replace("delay1=", "");
            string vibration1 = parametersOfComponent[2].Replace("vibration1=", "");
            string delay2 = parametersOfComponent[3].Replace("delay2=", "");
            string vibration2 = parametersOfComponent[4].Replace("vibration2=", "");
            string delay3 = parametersOfComponent[5].Replace("delay3=", "");
            string vibration3 = parametersOfComponent[6].Replace("vibration3=", "");

            //Render the parameters of this component to the user
            delay1 = SourceCode_RenderComponentVariableUIHere_Integer("Delay",
                                        "Delay time (in milliseconds) before vibration.",
                                        delay1, 0, 1000);

            vibration1 = SourceCode_RenderComponentVariableUIHere_Integer("Vibration",
                                        "Vibration time (in milliseconds).",
                                        vibration1, 0, 1000);

            delay2 = SourceCode_RenderComponentVariableUIHere_Integer("Delay",
                                        "Delay time (in milliseconds) before vibration.",
                                        delay2, 0, 1000);

            vibration2 = SourceCode_RenderComponentVariableUIHere_Integer("Vibration",
                                        "Vibration time (in milliseconds).",
                                        vibration2, 0, 1000);

            delay3 = SourceCode_RenderComponentVariableUIHere_Integer("Delay",
                                        "Delay time (in milliseconds) before vibration.",
                                        delay3, 0, 1000);

            vibration3 = SourceCode_RenderComponentVariableUIHere_Integer("Vibration",
                                        "Vibration time (in milliseconds).",
                                        vibration3, 0, 1000);

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("delay1=", delay1)
            .addComponentParameter("vibration1=", vibration1)
            .addComponentParameter("delay2=", delay2)
            .addComponentParameter("vibration2=", vibration2)
            .addComponentParameter("delay3=", delay3)
            .addComponentParameter("vibration3=", vibration3)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_Arithmetic(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "calcType=", "value1=", "value2=", "value3=", "value4=", "value5=", "saveResultOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string calcType = parametersOfComponent[1].Replace("calcType=", "");
            string value1 = parametersOfComponent[2].Replace("value1=", "");
            string value2 = parametersOfComponent[3].Replace("value2=", "");
            string value3 = parametersOfComponent[4].Replace("value3=", "");
            string value4 = parametersOfComponent[5].Replace("value4=", "");
            string value5 = parametersOfComponent[6].Replace("value5=", "");
            string saveResultOn = parametersOfComponent[7].Replace("saveResultOn=", "");

            //Render the parameters of this component to the user
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Operation", "The type of arithmetic operation to be performed."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] operationTypes = new string[] { "addition", "subtraction", "multiplication", "division" };
            int operationTypesInt = Array.IndexOf<string>(operationTypes, calcType);
            if (operationTypesInt == -1)
                operationTypesInt = 0;
            calcType = operationTypes[EditorGUILayout.Popup("", operationTypesInt, operationTypes, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            //Render the UI foreach operation type
            if (calcType == "addition")
            {
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Value 1", "The value to pass through the \"" + calcType + "\" operation.", value1, -999999999999999999, 999999999999999999);
                GUILayout.Space(-6);
                EditorGUILayout.LabelField("+", GUILayout.Width(148));
                GUILayout.Space(-6);
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Value 2", "The value to pass through the \"" + calcType + "\" operation.", value2, -999999999999999999, 999999999999999999);
            }
            if (calcType == "subtraction")
            {
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Value 1", "The value to pass through the \"" + calcType + "\" operation.", value1, -999999999999999999, 999999999999999999);
                GUILayout.Space(-6);
                EditorGUILayout.LabelField("-", GUILayout.Width(148));
                GUILayout.Space(-6);
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Value 2", "The value to pass through the \"" + calcType + "\" operation.", value2, -999999999999999999, 999999999999999999);
            }
            if (calcType == "multiplication")
            {
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Value 1", "The value to pass through the \"" + calcType + "\" operation.", value1, -999999999999999999, 999999999999999999);
                GUILayout.Space(-6);
                EditorGUILayout.LabelField("x", GUILayout.Width(148));
                GUILayout.Space(-6);
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Value 2", "The value to pass through the \"" + calcType + "\" operation.", value2, -999999999999999999, 999999999999999999);
            }
            if (calcType == "division")
            {
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Value 1", "The value to pass through the \"" + calcType + "\" operation.", value1, -999999999999999999, 999999999999999999);
                GUILayout.Space(-6);
                EditorGUILayout.LabelField("÷", GUILayout.Width(148));
                GUILayout.Space(-6);
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Value 2", "The value to pass through the \"" + calcType + "\" operation.", value2, -999999999999999999, 999999999999999999);
            }

            saveResultOn = SourceCode_RenderComponentVariableUIHere_String("Save Result On",
                                        "Path to the file that will have the result of this calculation recorded. The result will be written in \"float\" format.",
                                        saveResultOn);

            //Fix the variable type to "fileValue" only for the Audio Path
            string[] parametersMetadata = saveResultOn.Split('^');
            if (parametersMetadata[1] != "fileValue")
            {
                saveResultOn = "string^fileValue^calcResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Result On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata[2] == "")
            {
                saveResultOn = "string^editorValue^calcResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Result On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("calcType=", calcType)
            .addComponentParameter("value1=", value1)
            .addComponentParameter("value2=", value2)
            .addComponentParameter("value3=", value3)
            .addComponentParameter("value4=", value4)
            .addComponentParameter("value5=", value5)
            .addComponentParameter("saveResultOn=", saveResultOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionSimple_WaitForSeconds(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "type=", "millisecondsToWait=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string type = parametersOfComponent[1].Replace("type=", "");
            string millisecondsToWait = parametersOfComponent[2].Replace("millisecondsToWait=", "");

            //Render the parameters of this component to the user
            millisecondsToWait = SourceCode_RenderComponentVariableUIHere_Integer("Milliseconds To Wait",
                                        "The time that the task will be paused on hold until the execution of the next components continues.",
                                        millisecondsToWait, 50, 5000);

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("type=", type)
            .addComponentParameter("millisecondsToWait=", millisecondsToWait)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionNormal_LoadJsonFromWeb(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "jsonApiUrl=", "getParameters=", "postParameters=", "cookiesToSend=", "expectedPageHeader=", "saveJsonValuesOn=", "saveRequestStatusOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string jsonApiUrl = parametersOfComponent[1].Replace("jsonApiUrl=", "");
            string getParameters = parametersOfComponent[2].Replace("getParameters=", "");
            string postParameters = parametersOfComponent[3].Replace("postParameters=", "");
            string cookiesToSend = parametersOfComponent[4].Replace("cookiesToSend=", "");
            string expectedPageHeader = parametersOfComponent[5].Replace("expectedPageHeader=", "");
            string saveJsonValuesOn = parametersOfComponent[6].Replace("saveJsonValuesOn=", "");
            string saveRequestStatusOn = parametersOfComponent[7].Replace("saveRequestStatusOn=", "");

            //Render the parameters of this component to the user
            jsonApiUrl = SourceCode_RenderComponentVariableUIHere_String("Json API URL",
                                        "The URL of the remote PHP, JSON or API file to be accessed by this component.\n\nThe URL must use the HTTPS protocol and must be formatted similarly to: https://example.com/api-name.php",
                                        jsonApiUrl);

            getParameters = SourceCode_RenderComponentVariableUIHere_String("GET Parameters",
                                        "Insert the GET parameters of this request here. Use the following format: \"parameter1~value\" (without quotes). Use the character \"¬\" to separate each parameter. For example \"param1~value1¬param2~value2¬param3~value3\".",
                                        getParameters);

            postParameters = SourceCode_RenderComponentVariableUIHere_String("POST Parameters",
                                        "Insert the POST parameters of this request here. Use the following format: \"parameter1~value\" (without quotes). Use the character \"¬\" to separate each parameter. For example \"param1~value1¬param2~value2¬param3~value3\".",
                                        postParameters);

            cookiesToSend = SourceCode_RenderComponentVariableUIHere_String("Cookies To Send",
                                        "Insert the COOKIES of this request here. Use the following format: \"cookieName~content~domain\" (without quotes). Use the character \"¬\" to separate each cookie. For example \"sessID~45j4nmj53nj43kl5345~google.com¬cookieFB~345~google.com\".",
                                        cookiesToSend);

            expectedPageHeader = SourceCode_RenderComponentVariableUIHere_String("Expected Page Header",
                                        "Text content that must be contained before the JSON content on the response page when accessing the JSON. It should be in the following format...\n\n[PaheHeader]\n<br/>\n[JSON]\n\nRead the documentation for more details.",
                                        expectedPageHeader);

            saveJsonValuesOn = SourceCode_RenderComponentVariableUIHere_String("Save Json Values On",
                                        "The directory where the URL's JSON values should be stored. NAT will read each variable within the remote JSON and store it in the desired directory. A file with the name of the variable will be created and the value of the variable will be saved inside the corresponding file. A file will be generated for each remote JSON variable. The JSON will only be read and its variables will be saved if the JSON response header contains the expected value, informed on \"expectedPageHeader\" above. See the documentation for more details.",
                                        saveJsonValuesOn);

            saveRequestStatusOn = SourceCode_RenderComponentVariableUIHere_String("Save Request Status On",
                                        "The directory of the file that will be generated to contain the status of this request. The file will have as content, the request response number. For example \"200\" for Success or \"404\" for Not Found.",
                                        saveRequestStatusOn);

            //Fix empty parameters
            string[] expectedPhParametersSplitted1 = jsonApiUrl.Split('^');
            if (expectedPhParametersSplitted1[2] == "")
                jsonApiUrl = expectedPhParametersSplitted1[0] + "^" + expectedPhParametersSplitted1[1] + "^https://example.com/remote-api.php";
            string[] expectedPhParametersSplitted0 = expectedPageHeader.Split('^');
            if (expectedPhParametersSplitted0[2] == "")
                expectedPageHeader = expectedPhParametersSplitted0[0] + "^" + expectedPhParametersSplitted0[1] + "^requestSuccess";

            //Fix the variable type to "fileValue" for the Save Json Values On & Save Request Status On
            string[] parametersMetadata0 = saveJsonValuesOn.Split('^');
            if (parametersMetadata0[1] != "fileValue")
            {
                saveJsonValuesOn = "string^fileValue^Downloaded/JSONs";
                Debug.LogWarning("NAT Task Creator: The \"Save Json Values On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata0[2] == "")
            {
                saveJsonValuesOn = "string^editorValue^Downloaded/JSONs";
                Debug.LogWarning("NAT Task Creator: The \"Save Json Values On\" field cannot be empty, it must contain a folder path.");
            }
            string[] parametersMetadata1 = saveRequestStatusOn.Split('^');
            if (parametersMetadata1[1] != "fileValue")
            {
                saveRequestStatusOn = "string^fileValue^jsonLoadRequestResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Request Status On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata1[2] == "")
            {
                saveRequestStatusOn = "string^editorValue^jsonLoadRequestResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Request Status On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("jsonApiUrl=", jsonApiUrl)
            .addComponentParameter("getParameters=", getParameters)
            .addComponentParameter("postParameters=", postParameters)
            .addComponentParameter("cookiesToSend=", cookiesToSend)
            .addComponentParameter("expectedPageHeader=", expectedPageHeader)
            .addComponentParameter("saveJsonValuesOn=", saveJsonValuesOn)
            .addComponentParameter("saveRequestStatusOn=", saveRequestStatusOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionNormal_DownloadFileFromWeb(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "fileUrl=", "notificationTitle=", "saveFileOn=", "saveStatusOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string fileUrl = parametersOfComponent[1].Replace("fileUrl=", "");
            string notificationTitle = parametersOfComponent[2].Replace("notificationTitle=", "");
            string saveFileOn = parametersOfComponent[3].Replace("saveFileOn=", "");
            string saveStatusOn = parametersOfComponent[4].Replace("saveStatusOn=", "");

            //Render the parameters of this component to the user
            fileUrl = SourceCode_RenderComponentVariableUIHere_String("File URL",
                                        "The URL of the file to be downloaded.",
                                        fileUrl);

            notificationTitle = SourceCode_RenderComponentVariableUIHere_String("Notification Title",
                                        "The title that will appear in the download progress notification.",
                                        notificationTitle);

            saveFileOn = SourceCode_RenderComponentVariableUIHere_String("Save File On",
                                        "The path that the downloaded file will be saved.",
                                        saveFileOn);

            saveStatusOn = SourceCode_RenderComponentVariableUIHere_String("Save Status On",
                                        "The path of the text file that will be saved as the file download status. The status that will be saved will be a number that will indicate the response to the file request. For example \"200\" for Success or \"404\" for Not Found.",
                                        saveStatusOn);

            //Fix empty parameters
            string[] fileUrlSplitted = fileUrl.Split('^');
            if (fileUrlSplitted[2] == "")
                fileUrl = fileUrlSplitted[0] + "^" + fileUrlSplitted[1] + "^https://example.com/file-name.zip";
            string[] notificationTitleSplitted = notificationTitle.Split('^');
            if (notificationTitleSplitted[2] == "")
                notificationTitle = notificationTitleSplitted[0] + "^" + notificationTitleSplitted[1] + "^Downloading File";

            //Fix the variable type to "fileValue" for the Save File On & Save Status On
            string[] parametersMetadata0 = saveFileOn.Split('^');
            if (parametersMetadata0[1] != "fileValue")
            {
                saveFileOn = "string^fileValue^Downloaded/Files/fileName.ext";
                Debug.LogWarning("NAT Task Creator: The \"Save File On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata0[2] == "")
            {
                saveFileOn = "string^fileValue^Downloaded/Files/fileName.ext";
                Debug.LogWarning("NAT Task Creator: The \"Save File On\" field cannot be empty, it must contain a file path.");
            }
            string[] parametersMetadata1 = saveStatusOn.Split('^');
            if (parametersMetadata1[1] != "fileValue")
            {
                saveStatusOn = "string^fileValue^downloadFileRequestResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata1[2] == "")
            {
                saveStatusOn = "string^editorValue^downloadFileRequestResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("fileUrl=", fileUrl)
            .addComponentParameter("notificationTitle=", notificationTitle)
            .addComponentParameter("saveFileOn=", saveFileOn)
            .addComponentParameter("saveStatusOn=", saveStatusOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionNormal_FileExists(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "fileToCheck=", "saveStatusOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string fileToCheck = parametersOfComponent[1].Replace("fileToCheck=", "");
            string saveStatusOn = parametersOfComponent[2].Replace("saveStatusOn=", "");

            //Render the parameters of this component to the user
            fileToCheck = SourceCode_RenderComponentVariableUIHere_String("File To Check",
                                        "The path of the file or directory to check if exists.",
                                        fileToCheck);

            saveStatusOn = SourceCode_RenderComponentVariableUIHere_String("Save Status On",
                                        "The path that the text file with the status of this check will be saved. The file will contain \"True\" if the file to be checked exists but will contain \"False\" if the file to be checked does not exist. The value recorded in the file will be of type \"bool\".",
                                        saveStatusOn);

            //Fix the variable type to "fileValue" for the File To Check & Save Status On
            string[] parametersMetadata0 = fileToCheck.Split('^');
            if (parametersMetadata0[1] != "fileValue")
            {
                fileToCheck = "string^fileValue^fileToCheck.txt";
                Debug.LogWarning("NAT Task Creator: The \"File To Check\" can only be type \"fileValue\".");
            }
            if (parametersMetadata0[2] == "")
            {
                fileToCheck = "string^fileValue^fileToCheck.txt";
                Debug.LogWarning("NAT Task Creator: The \"File To Check\" field cannot be empty, it must contain a file path.");
            }
            string[] parametersMetadata1 = saveStatusOn.Split('^');
            if (parametersMetadata1[1] != "fileValue")
            {
                saveStatusOn = "string^fileValue^fileExistsResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata1[2] == "")
            {
                saveStatusOn = "string^editorValue^fileExistsResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("fileToCheck=", fileToCheck)
            .addComponentParameter("saveStatusOn=", saveStatusOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionNormal_GetFileSize(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "fileToCheck=", "saveSizeAs=", "saveSizeOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string fileToCheck = parametersOfComponent[1].Replace("fileToCheck=", "");
            string saveSizeAs = parametersOfComponent[2].Replace("saveSizeAs=", "");
            string saveSizeOn = parametersOfComponent[3].Replace("saveSizeOn=", "");

            //Render the parameters of this component to the user
            fileToCheck = SourceCode_RenderComponentVariableUIHere_String("File To Check",
                                        "The path of the file to get the file size.",
                                        fileToCheck);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Save Size As", "The unit of measurement that will be used to determine the file size."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] sizeTypes = new string[] { "KB", "KiB", "MB", "MiB", "GB", "GiB" };
            int sizeTypesInt = Array.IndexOf<string>(sizeTypes, saveSizeAs);
            if (sizeTypesInt == -1)
                sizeTypesInt = 0;
            saveSizeAs = sizeTypes[EditorGUILayout.Popup("", sizeTypesInt, sizeTypes, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            saveSizeOn = SourceCode_RenderComponentVariableUIHere_String("Save Size On",
                                        "The text file that will be created will contain the size of the file chosen to be checked. The value recorded in the file will be in \"float\" format.",
                                        saveSizeOn);

            //Fix the variable type to "fileValue" for the File To Check & Save Status On
            string[] parametersMetadata0 = fileToCheck.Split('^');
            if (parametersMetadata0[1] != "fileValue")
            {
                fileToCheck = "string^fileValue^fileToCheck.txt";
                Debug.LogWarning("NAT Task Creator: The \"File To Check\" can only be type \"fileValue\".");
            }
            if (parametersMetadata0[2] == "")
            {
                fileToCheck = "string^fileValue^fileToCheck.txt";
                Debug.LogWarning("NAT Task Creator: The \"File To Check\" field cannot be empty, it must contain a file path.");
            }
            string[] parametersMetadata1 = saveSizeOn.Split('^');
            if (parametersMetadata1[1] != "fileValue")
            {
                saveSizeOn = "string^fileValue^fileSizeResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Size On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata1[2] == "")
            {
                saveSizeOn = "string^editorValue^fileSizeResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Size On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("fileToCheck=", fileToCheck)
            .addComponentParameter("saveSizeAs=", saveSizeAs)
            .addComponentParameter("saveSizeOn=", saveSizeOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_FunctionNormal_ClearTaskDirectory(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "type=", "saveStatusOn=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string type = parametersOfComponent[1].Replace("type=", "");
            string saveStatusOn = parametersOfComponent[2].Replace("saveStatusOn=", "");

            //Render the parameters of this component to the user
            saveStatusOn = SourceCode_RenderComponentVariableUIHere_String("Save Status On",
                                        "The path that the text file with the status of this function will be saved. The file will contain \"True\" if the cleaning was completed successfully but will contain \"False\" if not. The value recorded in the file will be of type \"bool\".",
                                        saveStatusOn);

            //Fix the variable type to "fileValue" for the File To Check & Save Status On
            string[] parametersMetadata1 = saveStatusOn.Split('^');
            if (parametersMetadata1[1] != "fileValue")
            {
                saveStatusOn = "string^fileValue^clearTaskDirResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" can only be type \"fileValue\".");
            }
            if (parametersMetadata1[2] == "")
            {
                saveStatusOn = "string^editorValue^clearTaskDirResult.txt";
                Debug.LogWarning("NAT Task Creator: The \"Save Status On\" field cannot be empty, it must contain a file path.");
            }

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("type=", type)
            .addComponentParameter("saveStatusOn=", saveStatusOn)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_Condition_IfAndElse(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "conditions=", "comparationType=", "value1=", "comparator=", "value2=", "sLogicalOperator=", "sComparationType=", "sValue1=", "sComparator=", "sValue2=", "ifTrue=", "ifFalse=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string conditions = parametersOfComponent[1].Replace("conditions=", "");
            string comparationType = parametersOfComponent[2].Replace("comparationType=", "");
            string value1 = parametersOfComponent[3].Replace("value1=", "");
            string comparator = parametersOfComponent[4].Replace("comparator=", "");
            string value2 = parametersOfComponent[5].Replace("value2=", "");
            string sLogicalOperator = parametersOfComponent[6].Replace("sLogicalOperator=", "");
            string sComparationType = parametersOfComponent[7].Replace("sComparationType=", "");
            string sValue1 = parametersOfComponent[8].Replace("sValue1=", "");
            string sComparator = parametersOfComponent[9].Replace("sComparator=", "");
            string sValue2 = parametersOfComponent[10].Replace("sValue2=", "");
            string ifTrue = parametersOfComponent[11].Replace("ifTrue=", "");
            string ifFalse = parametersOfComponent[12].Replace("ifFalse=", "");

            //Render the parameters of this component to the user
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Conditions", "The number of conditions to use."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] conditionsCount = new string[] { "one", "two" };
            int conditionsCountInt = Array.IndexOf<string>(conditionsCount, conditions);
            if (conditionsCountInt == -1)
                conditionsCountInt = 0;
            conditions = conditionsCount[EditorGUILayout.Popup("", conditionsCountInt, conditionsCount, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Comparation Type", "The type of comparison that will be made. For example String by String, Float by Float and etc."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] comparationTypes = new string[] { "string", "float", "integer", "boolean" };
            int comparationTypesInt = Array.IndexOf<string>(comparationTypes, comparationType);
            if (comparationTypesInt == -1)
                comparationTypesInt = 0;
            comparationType = comparationTypes[EditorGUILayout.Popup("", comparationTypesInt, comparationTypes, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            if (comparationType == "string")
                value1 = SourceCode_RenderComponentVariableUIHere_String("String 1", "The first value of comparation.", value1);
            if (comparationType == "float")
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Float 1", "The first value of comparation.", value1, -999999999999999999, 999999999999999999);
            if (comparationType == "integer")
                value1 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 1", "The first value of comparation.", value1, -999999999, 999999999);
            if (comparationType == "boolean")
                value1 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 1", "The first value of comparation.", value1);

            string[] comparators = null;
            switch (comparationType)
            {
                case "string":
                    comparators = new string[] { "equal", "different" };
                    break;
                case "float":
                    comparators = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                    break;
                case "integer":
                    comparators = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                    break;
                case "boolean":
                    comparators = new string[] { "equal", "different" };
                    break;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Comparator", "The comparator that will be used in this condition."), GUILayout.Width(148));
            GUILayout.Space(1);
            int comparatorsInt = Array.IndexOf<string>(comparators, comparator);
            if (comparatorsInt == -1)
                comparatorsInt = 0;
            comparator = comparators[EditorGUILayout.Popup("", comparatorsInt, comparators, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            if (comparationType == "string")
                value2 = SourceCode_RenderComponentVariableUIHere_String("String 2", "The second value of comparation.", value2);
            if (comparationType == "float")
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Float 2", "The second value of comparation.", value2, -999999999999999999, 999999999999999999);
            if (comparationType == "integer")
                value2 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 2", "The second value of comparation.", value2, -999999999, 999999999);
            if (comparationType == "boolean")
                value2 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 2", "The second value of comparation.", value2);

            if (conditions == "two")
            {
                GUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Logical Operator", "The Logical Operator that will be used to compare the two conditions."), GUILayout.Width(148));
                GUILayout.Space(1);
                string[] logicalOperators = new string[] { "and", "or" };
                int logicalOperatorsInt = Array.IndexOf<string>(logicalOperators, sLogicalOperator);
                if (logicalOperatorsInt == -1)
                    logicalOperatorsInt = 0;
                sLogicalOperator = logicalOperators[EditorGUILayout.Popup("", logicalOperatorsInt, logicalOperators, GUILayout.Width(337))];
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(8);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Comparation Type", "The type of comparison that will be made. For example String by String, Float by Float and etc."), GUILayout.Width(148));
                GUILayout.Space(1);
                string[] comparationTypes2 = new string[] { "string", "float", "integer", "boolean" };
                int comparationTypesInt2 = Array.IndexOf<string>(comparationTypes2, sComparationType);
                if (comparationTypesInt2 == -1)
                    comparationTypesInt2 = 0;
                sComparationType = comparationTypes2[EditorGUILayout.Popup("", comparationTypesInt2, comparationTypes2, GUILayout.Width(337))];
                EditorGUILayout.EndHorizontal();

                if (sComparationType == "string")
                    sValue1 = SourceCode_RenderComponentVariableUIHere_String("String 1", "The first value of comparation.", sValue1);
                if (sComparationType == "float")
                    sValue1 = SourceCode_RenderComponentVariableUIHere_Float("Float 1", "The first value of comparation.", sValue1, -999999999999999999, 999999999999999999);
                if (sComparationType == "integer")
                    sValue1 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 1", "The first value of comparation.", sValue1, -999999999, 999999999);
                if (sComparationType == "boolean")
                    sValue1 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 1", "The first value of comparation.", sValue1);

                string[] comparators2 = null;
                switch (sComparationType)
                {
                    case "string":
                        comparators2 = new string[] { "equal", "different" };
                        break;
                    case "float":
                        comparators2 = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                        break;
                    case "integer":
                        comparators2 = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                        break;
                    case "boolean":
                        comparators2 = new string[] { "equal", "different" };
                        break;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Comparator", "The comparator that will be used in this condition."), GUILayout.Width(148));
                GUILayout.Space(1);
                int comparatorsInt2 = Array.IndexOf<string>(comparators2, sComparator);
                if (comparatorsInt2 == -1)
                    comparatorsInt2 = 0;
                sComparator = comparators2[EditorGUILayout.Popup("", comparatorsInt2, comparators2, GUILayout.Width(337))];
                EditorGUILayout.EndHorizontal();

                if (sComparationType == "string")
                    sValue2 = SourceCode_RenderComponentVariableUIHere_String("String 2", "The second value of comparation.", sValue2);
                if (sComparationType == "float")
                    sValue2 = SourceCode_RenderComponentVariableUIHere_Float("Float 2", "The second value of comparation.", sValue2, -999999999999999999, 999999999999999999);
                if (sComparationType == "integer")
                    sValue2 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 2", "The second value of comparation.", sValue2, -999999999, 999999999);
                if (sComparationType == "boolean")
                    sValue2 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 2", "The second value of comparation.", sValue2);
            }

            List<string> actionsToSelectList = new List<string>();
            actionsToSelectList.Add("none");
            if (parentMethodCode == "-method=main")
                foreach (var method in currentTaskSourceCodeBeingEdited)
                    if (method.Key != "-method=main")
                        actionsToSelectList.Add("call:" + method.Key.Replace("-method=", ""));
            foreach (var component in currentTaskSourceCodeBeingEdited[parentMethodCode])
                if (component.Contains("--demarcator=section") == true && component.Contains("sectionName=") == true)
                {
                    string[] sectionData = component.Split(new string[] { "|||" }, StringSplitOptions.None);
                    actionsToSelectList.Add("goto:" + sectionData[2].Replace("sectionName=", ""));
                }
            actionsToSelectList.Add("return");
            string[] actionsToSelect = actionsToSelectList.ToArray();

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("If TRUE", "Specify what will be done if this condition is TRUE. You can specify nothing, return nothing, or choose a method to call."), GUILayout.Width(148));
            GUILayout.Space(1);
            int methodsToSelectInt0 = Array.IndexOf<string>(actionsToSelect, ifTrue);
            if (methodsToSelectInt0 == -1)
                methodsToSelectInt0 = 0;
            ifTrue = actionsToSelect[EditorGUILayout.Popup("", methodsToSelectInt0, actionsToSelect, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("If FALSE", "Specify what will be done if this condition is FALSE. You can specify nothing, return nothing, or choose a method to call."), GUILayout.Width(148));
            GUILayout.Space(1);
            int methodsToSelectInt1 = Array.IndexOf<string>(actionsToSelect, ifFalse);
            if (methodsToSelectInt1 == -1)
                methodsToSelectInt1 = 0;
            ifFalse = actionsToSelect[EditorGUILayout.Popup("", methodsToSelectInt1, actionsToSelect, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("conditions=", conditions)
            .addComponentParameter("comparationType=", comparationType)
            .addComponentParameter("value1=", value1)
            .addComponentParameter("comparator=", comparator)
            .addComponentParameter("value2=", value2)
            .addComponentParameter("sLogicalOperator=", sLogicalOperator)
            .addComponentParameter("sComparationType=", sComparationType)
            .addComponentParameter("sValue1=", sValue1)
            .addComponentParameter("sComparator=", sComparator)
            .addComponentParameter("sValue2=", sValue2)
            .addComponentParameter("ifTrue=", ifTrue)
            .addComponentParameter("ifFalse=", ifFalse)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_Condition_While(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "comparationType=", "value1=", "comparator=", "value2=", "whileTrue=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string comparationType = parametersOfComponent[1].Replace("comparationType=", "");
            string value1 = parametersOfComponent[2].Replace("value1=", "");
            string comparator = parametersOfComponent[3].Replace("comparator=", "");
            string value2 = parametersOfComponent[4].Replace("value2=", "");
            string whileTrue = parametersOfComponent[5].Replace("whileTrue=", "");

            //Render the parameters of this component to the user
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Comparation Type", "The type of comparison that will be made. For example String by String, Float by Float and etc."), GUILayout.Width(148));
            GUILayout.Space(1);
            string[] comparationTypes = new string[] { "string", "float", "integer", "boolean" };
            int comparationTypesInt = Array.IndexOf<string>(comparationTypes, comparationType);
            if (comparationTypesInt == -1)
                comparationTypesInt = 0;
            comparationType = comparationTypes[EditorGUILayout.Popup("", comparationTypesInt, comparationTypes, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            if (comparationType == "string")
                value1 = SourceCode_RenderComponentVariableUIHere_String("String 1", "The first value of comparation.", value1);
            if (comparationType == "float")
                value1 = SourceCode_RenderComponentVariableUIHere_Float("Float 1", "The first value of comparation.", value1, -999999999999999999, 999999999999999999);
            if (comparationType == "integer")
                value1 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 1", "The first value of comparation.", value1, -999999999, 999999999);
            if (comparationType == "boolean")
                value1 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 1", "The first value of comparation.", value1);

            string[] comparators = null;
            switch (comparationType)
            {
                case "string":
                    comparators = new string[] { "equal", "different" };
                    break;
                case "float":
                    comparators = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                    break;
                case "integer":
                    comparators = new string[] { "equal", "different", "biggerOrEqual", "lessOrEqual", "bigger", "less" };
                    break;
                case "boolean":
                    comparators = new string[] { "equal", "different" };
                    break;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Comparator", "The comparator that will be used in this condition."), GUILayout.Width(148));
            GUILayout.Space(1);
            int comparatorsInt = Array.IndexOf<string>(comparators, comparator);
            if (comparatorsInt == -1)
                comparatorsInt = 0;
            comparator = comparators[EditorGUILayout.Popup("", comparatorsInt, comparators, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            if (comparationType == "string")
                value2 = SourceCode_RenderComponentVariableUIHere_String("String 2", "The second value of comparation.", value2);
            if (comparationType == "float")
                value2 = SourceCode_RenderComponentVariableUIHere_Float("Float 2", "The second value of comparation.", value2, -999999999999999999, 999999999999999999);
            if (comparationType == "integer")
                value2 = SourceCode_RenderComponentVariableUIHere_Integer("Integer 2", "The second value of comparation.", value2, -999999999, 999999999);
            if (comparationType == "boolean")
                value2 = SourceCode_RenderComponentVariableUIHere_Boolean("Boolean 2", "The second value of comparation.", value2);

            List<string> actionsToSelectList = new List<string>();
            actionsToSelectList.Add("none");
            foreach (var method in currentTaskSourceCodeBeingEdited)
                if (method.Key != "-method=main")
                    actionsToSelectList.Add("call:" + method.Key.Replace("-method=", ""));
            string[] actionsToSelect = actionsToSelectList.ToArray();

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("While TRUE", "Specify what will be done WHILE this condition is TRUE. You can specify nothing, or choose a method to call."), GUILayout.Width(148));
            GUILayout.Space(1);
            int methodsToSelectInt = Array.IndexOf<string>(actionsToSelect, whileTrue);
            if (methodsToSelectInt == -1)
                methodsToSelectInt = 0;
            whileTrue = actionsToSelect[EditorGUILayout.Popup("", methodsToSelectInt, actionsToSelect, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("", ""), GUILayout.Width(148));
            EditorGUILayout.HelpBox("The maximum quantity of iterations is limited to " + natPreferences.maxLoopIterationsInTask + " iterations per loop. You can change this limit in NAT Preferences.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("comparationType=", comparationType)
            .addComponentParameter("value1=", value1)
            .addComponentParameter("comparator=", comparator)
            .addComponentParameter("value2=", value2)
            .addComponentParameter("whileTrue=", whileTrue)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_Condition_CallMethod(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "type=", "methodToCall=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string type = parametersOfComponent[1].Replace("type=", "");
            string methodToCall = parametersOfComponent[2].Replace("methodToCall=", "");

            List<string> actionsToSelectList = new List<string>();
            actionsToSelectList.Add("none");
            foreach (var method in currentTaskSourceCodeBeingEdited)
                if (method.Key != "-method=main")
                    actionsToSelectList.Add("call:" + method.Key.Replace("-method=", ""));
            string[] actionsToSelect = actionsToSelectList.ToArray();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Method To Call", "You can specify nothing, or choose a method to call."), GUILayout.Width(148));
            GUILayout.Space(1);
            int methodsToSelectInt = Array.IndexOf<string>(actionsToSelect, methodToCall);
            if (methodsToSelectInt == -1)
                methodsToSelectInt = 0;
            methodToCall = actionsToSelect[EditorGUILayout.Popup("", methodsToSelectInt, actionsToSelect, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("type=", type)
            .addComponentParameter("methodToCall=", methodToCall)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_Condition_GoToSection(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "type=", "goTo=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string type = parametersOfComponent[1].Replace("type=", "");
            string goTo = parametersOfComponent[2].Replace("goTo=", "");

            List<string> actionsToSelectList = new List<string>();
            actionsToSelectList.Add("none");
            foreach (var component in currentTaskSourceCodeBeingEdited[parentMethodCode])
                if (component.Contains("--demarcator=section") == true && component.Contains("sectionName=") == true)
                {
                    string[] sectionData = component.Split(new string[] { "|||" }, StringSplitOptions.None);
                    actionsToSelectList.Add("goto:" + sectionData[2].Replace("sectionName=", ""));
                }
            string[] actionsToSelect = actionsToSelectList.ToArray();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Section To Go", "You can specify nothing, or choose a section to script jump."), GUILayout.Width(148));
            GUILayout.Space(1);
            int methodsToSelectInt = Array.IndexOf<string>(actionsToSelect, goTo);
            if (methodsToSelectInt == -1)
                methodsToSelectInt = 0;
            goTo = actionsToSelect[EditorGUILayout.Popup("", methodsToSelectInt, actionsToSelect, GUILayout.Width(337))];
            EditorGUILayout.EndHorizontal();

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("type=", type)
            .addComponentParameter("goTo=", goTo)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        void SourceCode_RenderComponentParametersHere_Comment_SimpleComment(string componentType, string parentMethodCode, int componentIndexInMethod)
        {
            //------------- DESERIALIZE THE COMPONENT PARAMETERS -------------//

            //Declare the default parameters for this component
            string[] defaultParametersOfComponent = new string[] { "type=", "comment=" };

            //Get all parameters and respective values of the component
            string[] parametersOfComponent = SourceCode_GetAllParametersOfComponentAndInitializeItIfNotInitialized(componentType, parentMethodCode, componentIndexInMethod, defaultParametersOfComponent);

            //------------- DISPLAY THE COMPONENT PARAMETERS -------------//

            //Prepare the parameters of this component
            string type = parametersOfComponent[1].Replace("type=", "");
            string comment = parametersOfComponent[2].Replace("comment=", "");

            //Fix the pre input value
            comment = comment.Replace("₢", "\n");

            //Render the parameters of this component to the user
            comment = EditorGUILayout.TextArea(comment);

            //Fix the post input value
            comment = comment.Replace("§", "").Replace("|", "").Replace("^", "").Replace("\n", "₢").Replace("\r", "");

            //------------- SERIALIZE THE COMPONENT PARAMETERS -------------//

            //Save the updated code of this component in the default place of script
            string componentAndSerializedParameters = new ComponentParametersSerialization(componentType)
            .addComponentParameter("type=", type)
            .addComponentParameter("comment=", comment)
            .GetSerialized();
            currentTaskSourceCodeBeingEdited[parentMethodCode][componentIndexInMethod] = componentAndSerializedParameters;
        }

        //Source code variables modifiers, these methods creates UI to user insert inputs for variables

        string SourceCode_RenderComponentVariableUIHere_String(string variableTitle, string variableDescription, string currentValueOfVariable)
        {
            //This method will receive the content of paramter of some component, decodify, render a UI to user interact and then return the variable value with type and other details

            //Deserialize the current value of variable
            string[] variableTypes = new string[] { "editorValue", "runtimeValue", "fileValue", "constantValue" };
            string[] variableLoaded = currentValueOfVariable.Split('^');
            //If the variable is empty, create a new value and a default type
            if (variableLoaded.Length != 3)
                variableLoaded = new string[] { "string", "editorValue", "" };

            //Prepare the storage of variable
            string variableType = variableLoaded[1];
            string variableValue = variableLoaded[2];

            //Render the UI of variable
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(variableTitle, variableDescription + VariableTypeDescriptions.variableTypeDescription), GUILayout.Width(148));
            GUILayout.Space(1);
            variableType = variableTypes[EditorGUILayout.Popup("", Array.IndexOf<string>(variableTypes, variableType), variableTypes, GUILayout.Width(104))];
            if (variableType == variableTypes[0])   //<- case "editorValue"
                variableValue = EditorGUILayout.TextField("", variableValue, GUILayout.Width(230));
            if (variableType == variableTypes[1])   //<- case "runtimeValue"
            {
                variableValue = variableValue.Replace("%", "");
                EditorGUILayout.LabelField("Key", GUILayout.Width(23));
                variableValue = EditorGUILayout.TextField("", variableValue, GUILayout.Width(204));
                if (variableValue == "")
                    variableValue = "runtimeValueKey";
                variableValue = "%" + variableValue + "%";
            }
            if (variableType == variableTypes[2])   //<- case "fileValue"
            {
                EditorGUILayout.LabelField("AppFiles/NAT/Tasks/", GUILayout.Width(124));
                variableValue = EditorGUILayout.TextField(new GUIContent(""), variableValue, GUILayout.Width(103));
            }
            if (variableType == variableTypes[3])   //<- case "constantValue"
            {
                string[] constants = new string[] { "appVersion", "currentDateTimeMillis", "appPackageName", "downloadFileSuccessCode", "loadJsonSuccessCode" };
                int variableValueInt = Array.IndexOf<string>(constants, variableValue);
                if (variableValueInt == -1)
                    variableValueInt = 0;
                variableValue = constants[EditorGUILayout.Popup("", variableValueInt, constants, GUILayout.Width(230))];
            }
            GUILayout.Space(-200);
            EditorGUILayout.EndHorizontal();

            //Fix the input value
            variableValue = variableValue.Replace("§", "").Replace("|", "").Replace("^", "");

            //Return the value serialized
            return "string^" + variableType + "^" + variableValue;
        }

        string SourceCode_RenderComponentVariableUIHere_Float(string variableTitle, string variableDescription, string currentValueOfVariable, float minValue, float maxValue)
        {
            //This method will receive the content of paramter of some component, decodify, render a UI to user interact and then return the variable value with type and other details

            //Deserialize the current value of variable
            string[] variableTypes = new string[] { "editorValue", "runtimeValue", "fileValue", "constantValue", "randomValue" };
            string[] variableLoaded = currentValueOfVariable.Split('^');
            //If the variable is empty, create a new value and a default type
            if (variableLoaded.Length != 3)
                variableLoaded = new string[] { "float", "editorValue", "0.0" };

            //Prepare the storage of variable
            string variableType = variableLoaded[1];
            string variableValue = variableLoaded[2];

            //Render the UI of variable
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(variableTitle, variableDescription + VariableTypeDescriptions.variableTypeDescription), GUILayout.Width(148));
            GUILayout.Space(1);
            variableType = variableTypes[EditorGUILayout.Popup("", Array.IndexOf<string>(variableTypes, variableType), variableTypes, GUILayout.Width(104))];
            if (variableType == variableTypes[0])   //<- case "editorValue"
            {
                float testFloat = 0;
                if (float.TryParse(variableValue, out testFloat) == false) { variableValue = "0.0"; }
                variableValue = EditorGUILayout.Slider("", float.Parse(variableValue), minValue, maxValue, GUILayout.Width(230)).ToString();
            }
            if (variableType == variableTypes[1])   //<- case "runtimeValue"
            {
                variableValue = variableValue.Replace("%", "");
                EditorGUILayout.LabelField("Key", GUILayout.Width(23));
                variableValue = EditorGUILayout.TextField("", variableValue, GUILayout.Width(204));
                if (variableValue == "")
                    variableValue = "runtimeValueKey";
                variableValue = "%" + variableValue + "%";
            }
            if (variableType == variableTypes[2])   //<- case "fileValue"
            {
                EditorGUILayout.LabelField("AppFiles/NAT/Tasks/", GUILayout.Width(124));
                variableValue = EditorGUILayout.TextField(new GUIContent(""), variableValue, GUILayout.Width(103));
            }
            if (variableType == variableTypes[3])   //<- case "constantValue"
            {
                string[] constants = new string[] { "currentMediaVolume", "currentBrightness" };
                int variableValueInt = Array.IndexOf<string>(constants, variableValue);
                if (variableValueInt == -1)
                    variableValueInt = 0;
                variableValue = constants[EditorGUILayout.Popup("", variableValueInt, constants, GUILayout.Width(230))];
            }
            if (variableType == variableTypes[4])   //<- case "randomValue"
            {
                if (variableValue.Contains("random:") == false || variableValue.Contains("%") == true)
                    variableValue = "random:0;5";
                float min = float.Parse(variableValue.Replace("random:", "").Split(';')[0]);
                float max = float.Parse(variableValue.Replace("random:", "").Split(';')[1]);
                EditorGUILayout.LabelField(new GUIContent("Min", ""), GUILayout.Width(28));
                min = EditorGUILayout.FloatField("", min, GUILayout.Width(82));
                EditorGUILayout.LabelField(new GUIContent("Max", ""), GUILayout.Width(29));
                max = EditorGUILayout.FloatField("", max, GUILayout.Width(82));
                variableValue = "random:" + min + ";" + max;
            }
            GUILayout.Space(-200);
            EditorGUILayout.EndHorizontal();

            //Fix the input value
            variableValue = variableValue.Replace("§", "").Replace("|", "").Replace("^", "");

            //Return the value serialized
            return "float^" + variableType + "^" + variableValue;
        }

        string SourceCode_RenderComponentVariableUIHere_Integer(string variableTitle, string variableDescription, string currentValueOfVariable, int minValue, int maxValue)
        {
            //This method will receive the content of paramter of some component, decodify, render a UI to user interact and then return the variable value with type and other details

            //Deserialize the current value of variable
            string[] variableTypes = new string[] { "editorValue", "runtimeValue", "fileValue", "constantValue", "randomValue" };
            string[] variableLoaded = currentValueOfVariable.Split('^');
            //If the variable is empty, create a new value and a default type
            if (variableLoaded.Length != 3)
                variableLoaded = new string[] { "int", "editorValue", "0" };

            //Prepare the storage of variable
            string variableType = variableLoaded[1];
            string variableValue = variableLoaded[2];

            //Render the UI of variable
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(variableTitle, variableDescription + VariableTypeDescriptions.variableTypeDescription), GUILayout.Width(148));
            GUILayout.Space(1);
            variableType = variableTypes[EditorGUILayout.Popup("", Array.IndexOf<string>(variableTypes, variableType), variableTypes, GUILayout.Width(104))];
            if (variableType == variableTypes[0])   //<- case "editorValue"
            {
                int testInt = 0;
                if (int.TryParse(variableValue, out testInt) == false) { variableValue = "0"; }
                variableValue = EditorGUILayout.IntSlider("", int.Parse(variableValue), minValue, maxValue, GUILayout.Width(230)).ToString();
            }
            if (variableType == variableTypes[1])   //<- case "runtimeValue"
            {
                variableValue = variableValue.Replace("%", "");
                EditorGUILayout.LabelField("Key", GUILayout.Width(23));
                variableValue = EditorGUILayout.TextField("", variableValue, GUILayout.Width(204));
                if (variableValue == "")
                    variableValue = "runtimeValueKey";
                variableValue = "%" + variableValue + "%";
            }
            if (variableType == variableTypes[2])   //<- case "fileValue"
            {
                EditorGUILayout.LabelField("AppFiles/NAT/Tasks/", GUILayout.Width(124));
                variableValue = EditorGUILayout.TextField(new GUIContent(""), variableValue, GUILayout.Width(103));
            }
            if (variableType == variableTypes[3])   //<- case "constantValue"
            {
                string[] constants = new string[] { "bundleVersionCode", "deviceApiVersion", "downloadFileSuccessCode", "loadJsonSuccessCode", "elapsedHoursSinceLastTaskRun", "elapsedMinutesSinceLastTaskRun", "elapsedDaysSinceLastTaskRun" };
                int variableValueInt = Array.IndexOf<string>(constants, variableValue);
                if (variableValueInt == -1)
                    variableValueInt = 0;
                variableValue = constants[EditorGUILayout.Popup("", variableValueInt, constants, GUILayout.Width(230))];
            }
            if (variableType == variableTypes[4])   //<- case "randomValue"
            {
                if (variableValue.Contains("random:") == false || variableValue.Contains("%") == true || variableValue.Contains(",") == true)
                    variableValue = "random:0;5";
                int min = int.Parse(variableValue.Replace("random:", "").Split(';')[0]);
                int max = int.Parse(variableValue.Replace("random:", "").Split(';')[1]);
                EditorGUILayout.LabelField(new GUIContent("Min", ""), GUILayout.Width(28));
                min = EditorGUILayout.IntField("", min, GUILayout.Width(82));
                EditorGUILayout.LabelField(new GUIContent("Max", ""), GUILayout.Width(29));
                max = EditorGUILayout.IntField("", max, GUILayout.Width(82));
                variableValue = "random:" + min + ";" + max;
            }
            GUILayout.Space(-200);
            EditorGUILayout.EndHorizontal();

            //Fix the input value
            variableValue = variableValue.Replace("§", "").Replace("|", "").Replace("^", "");

            //Return the value serialized
            return "int^" + variableType + "^" + variableValue;
        }

        string SourceCode_RenderComponentVariableUIHere_Boolean(string variableTitle, string variableDescription, string currentValueOfVariable)
        {
            //This method will receive the content of paramter of some component, decodify, render a UI to user interact and then return the variable value with type and other details

            //Deserialize the current value of variable
            string[] variableTypes = new string[] { "editorValue", "runtimeValue", "fileValue", "constantValue" };
            string[] variableLoaded = currentValueOfVariable.Split('^');
            //If the variable is empty, create a new value and a default type
            if (variableLoaded.Length != 3)
                variableLoaded = new string[] { "bool", "editorValue", "False" };

            //Prepare the storage of variable
            string variableType = variableLoaded[1];
            string variableValue = variableLoaded[2];

            //Render the UI of variable
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(variableTitle, variableDescription + VariableTypeDescriptions.variableTypeDescription), GUILayout.Width(148));
            GUILayout.Space(1);
            variableType = variableTypes[EditorGUILayout.Popup("", Array.IndexOf<string>(variableTypes, variableType), variableTypes, GUILayout.Width(104))];
            if (variableType == variableTypes[0])   //<- case "editorValue"
            {
                if (variableValue != "False" && variableValue != "True") { variableValue = "False"; }
                GUILayout.Space(216);
                variableValue = EditorGUILayout.Toggle("", bool.Parse(variableValue)).ToString();
            }
            if (variableType == variableTypes[1])   //<- case "runtimeValue"
            {
                variableValue = variableValue.Replace("%", "");
                EditorGUILayout.LabelField("Key", GUILayout.Width(23));
                variableValue = EditorGUILayout.TextField("", variableValue, GUILayout.Width(204));
                if (variableValue == "")
                    variableValue = "runtimeValueKey";
                variableValue = "%" + variableValue + "%";
            }
            if (variableType == variableTypes[2])   //<- case "fileValue"
            {
                EditorGUILayout.LabelField("AppFiles/NAT/Tasks/", GUILayout.Width(124));
                variableValue = EditorGUILayout.TextField(new GUIContent(""), variableValue, GUILayout.Width(103));
            }
            if (variableType == variableTypes[3])   //<- case "constantValue"
            {
                string[] constants = new string[] { "isDeveloperModeEnabled", "isDeviceRooted" };
                int variableValueInt = Array.IndexOf<string>(constants, variableValue);
                if (variableValueInt == -1)
                    variableValueInt = 0;
                variableValue = constants[EditorGUILayout.Popup("", variableValueInt, constants, GUILayout.Width(230))];
            }
            GUILayout.Space(-200);
            EditorGUILayout.EndHorizontal();

            //Fix the input value
            variableValue = variableValue.Replace("§", "").Replace("|", "").Replace("^", "");

            //Return the value serialized
            return "bool^" + variableType + "^" + variableValue;
        }

        //Load and Save of task source code

        void LoadSourceCode()
        {
            //Show progress dialog
            EditorUtility.DisplayProgressBar("A moment", "Loading The Source Code...", 1f);

            //Extract the source code of lines of preferences and store into the dictionary
            string currentMethodThatIsReading = "";
            foreach (string str in natPreferences.taskSourceCodeLines)
            {
                if (str.Contains("-method=") == true)
                {
                    if (currentTaskSourceCodeBeingEdited.ContainsKey(str) == false)
                        currentTaskSourceCodeBeingEdited.Add(str, new List<string>());
                    currentMethodThatIsReading = str;
                }
                else
                {
                    currentTaskSourceCodeBeingEdited[currentMethodThatIsReading].Add(str);
                }
            }

            //Remove progress dialog
            EditorUtility.ClearProgressBar();
        }

        void SaveSourceCode()
        {
            //If the code is not valid, show a error and cancel
            if (SourceCode_isValid() != "")
            {
                EditorUtility.DisplayDialog("Error On Save", "Unable to save the source code of your task. Apparently there is some structural error, syntax error or something is invalid in the code. See below for more details on the error...\n" + SourceCode_isValid(), "Ok");
                return;
            }
            //Check if have not used runtime keys and warn about this
            string notReferencedKeys = "";
            foreach (var runtimeKeyInDb in currentTaskSourceCodeRuntimeKeys)
                if (currentTaskSourceCodeRuntimeKeys[runtimeKeyInDb.Key].isBeingUsedByCsharp == false)
                    notReferencedKeys += "\n[ " + currentTaskSourceCodeRuntimeKeys[runtimeKeyInDb.Key].keyTipe.ToUpper() + " ] " + currentTaskSourceCodeRuntimeKeys[runtimeKeyInDb.Key].key;
            if (notReferencedKeys != "")
                EditorUtility.DisplayDialog("Not Used Runtime Values Keys", "There are components using RuntimeValues with Key that were not referenced in any of your C# Scripts! Try referencing Keys in your script, where you enable Task. That way you can provide values for your Task from your scripts and use additional custom logic. Keys not referenced in any C# scripts are...\n" + notReferencedKeys, "Ok");

            //Show progress dialog
            EditorUtility.DisplayProgressBar("A moment", "Saving The Source Code...", 1f);

            //Get current date
            DateTime currentTime = DateTime.Now;
            string day = ((currentTime.Day < 10) ? "0" + currentTime.Day : currentTime.Day.ToString());
            string month = ((currentTime.Month < 10) ? "0" + currentTime.Month : currentTime.Month.ToString());
            string year = currentTime.Year.ToString();
            string hour = ((currentTime.Hour < 10) ? "0" + currentTime.Hour : currentTime.Hour.ToString());
            string minute = ((currentTime.Minute < 10) ? "0" + currentTime.Minute : currentTime.Minute.ToString());
            string seconds = ((currentTime.Second < 10) ? "0" + currentTime.Second : currentTime.Second.ToString());

            //Save the source code to the preferences file
            List<string> currentTaskSourceCodeBeingEditedLines = new List<string>();
            foreach (var item in currentTaskSourceCodeBeingEdited)
            {
                currentTaskSourceCodeBeingEditedLines.Add(item.Key);
                foreach (string str in currentTaskSourceCodeBeingEdited[item.Key])
                    currentTaskSourceCodeBeingEditedLines.Add(str);
            }
            natPreferences.taskSourceCodeLines = currentTaskSourceCodeBeingEditedLines;
            natPreferences.taskSourceCodeLastSave = month + "/" + day + "/" + year + " at " + hour + ":" + minute + ":" + seconds;

            //Save the preferences
            SaveThePreferences();

            //Remove progress dialog
            EditorUtility.ClearProgressBar();

            //Show the warning
            Debug.Log("NAT: The source code of your task has been saved successfully on \"" + month + "/" + day + "/" + year + " at " + hour + ":" + minute + ":" + seconds + "\"! Now you just need to click on \"Save Preferences\" in the NAT Preferences dialog and your Task's Source Code will be applied to your project and be available to be activated through your application's C# code.");
        }

        //Import and export task source code methods

        public static void ExportSourceCode(NativeAndroidPreferences natPreferences)
        {
            //This method will export the source code of the task into a file

            //Open the export window
            string folder = EditorUtility.OpenFolderPanel("Select Folder To Export", "", "");
            if (System.String.IsNullOrEmpty(folder) == true)
                return;

            //Show progress bar
            EditorUtility.DisplayProgressBar("A moment", "Exporting Task Source Code", 1.0f);

            //Prepare to save task source code
            StringBuilder taskSourceCodeCompiled = new StringBuilder();
            bool isFirstLine = true;
            foreach (string line in natPreferences.taskSourceCodeLines)
            {
                if (isFirstLine == false)
                    taskSourceCodeCompiled.Append("§");
                taskSourceCodeCompiled.Append(line);
                isFirstLine = false;
            }
            //Save the json in the desired path
            System.IO.File.WriteAllText(folder + "/nat-task-source-code (Task Source Code).tsk", taskSourceCodeCompiled.ToString());

            //Clear progress bar
            EditorUtility.ClearProgressBar();

            //Show warning
            Debug.Log("The Task Source Code was successfully exported to the directory \"" + folder + "\".");
        }

        public static void ImportSourceCode(NativeAndroidPreferences natPreferences)
        {
            //This method will import the source code of the task from a file

            //Show the warning
            if (EditorUtility.DisplayDialog("Warning", "Importing the Task Source Code of a file will make the current Source Code save in Native Android Toolkit is replaced by the file code you specify. Are you sure you want to continue?", "Yes", "No") == false)
                return;

            //Open the import window
            string file = EditorUtility.OpenFilePanel("Select TSK File To Import", "", "tsk");
            if (System.String.IsNullOrEmpty(file) == true)
                return;

            //Read the file
            string[] fileLines = System.IO.File.ReadAllText(file).Split('§');

            //Reset the lines of source code saved into nat preferences
            natPreferences.taskSourceCodeLines.Clear();

            //Import all lines of source code from file into nat preferences
            foreach (string fileLine in fileLines)
                natPreferences.taskSourceCodeLines.Add(fileLine);
            natPreferences.taskSourceCodeLastSave = "Imported-" + (DateTime.Now.Ticks / 10000000);
        }
    }

    #region VARIABLE_TYPE_DESCRIPTION

    public class VariableTypeDescriptions
    {
        //This class stores a description of variable types

        public const string variableTypeDescription = "\n\n===================\nEditorValue - The content of this field can only be filled through here, the Task Creator and cannot be filled in different ways.\n\nRuntimeValue - You can enter a key (on the field on right side) that will serve as an ID. This key will be replaced by another value in Runtime when you call \"ScheduledTask.setRuntimeValue()\" and then call \"ScheduledTask.EnableThisTask()\" in your C# script. That way this field can be filled in by your C# script before activating the task. When filling this field in Runtime, you only need to inform the KEY (ID) and the value you want to put in this field.\n\nFileValue - The value of this field will be filled in by the text content of a file present in the \"AppFiles/NAT/Tasks\" directory in the \"AppFiles\" scope. Here in the field on the right you will need to inform the name of the file that is in the mentioned directory and then during execution the text value will be extracted from the file and then placed in this field.\n\nConstantValue - The value of this field will be filled by some constant value that cannot be changed by Task Creator or Runtime. It is an automatic value provided by NAT.\n\nRandomValue - An automatically generated random numeric value within a minimum and maximum limit set by you.";
    }

    #endregion

    #region COMPONENT_PARAMETERS_SERIALIZATION

    public class ComponentParametersSerialization
    {
        //This class build a serialized code using parameters and values inserted

        //Private variables
        private string componentType;
        private List<string> componentParameters = new List<string>();

        public ComponentParametersSerialization(string componentType)
        {
            //Insert the component type
            this.componentType = componentType;
        }

        public ComponentParametersSerialization addComponentParameter(string parameterName, string parameterValue)
        {
            //Add the new parameter name and value
            componentParameters.Add((parameterName + parameterValue));
            return this;
        }

        public string GetSerialized()
        {
            //Initialize the paramter code
            string componentCode = componentType;

            //Add the parameters and values
            foreach (string componentParameter in componentParameters)
                componentCode += "|||" + componentParameter;

            //Return the serialized component code
            return componentCode;
        }
    }

    #endregion
}