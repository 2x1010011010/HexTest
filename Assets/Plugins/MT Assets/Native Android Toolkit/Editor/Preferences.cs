using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    public class Preferences : EditorWindow
    {
        //Variables of preferences
        public static NativeAndroidPreferences natPreferences;

        //Cache variables
        private Vector2 scrollPosTopics;
        private Vector2 scrollPosPreferences;
        public static bool savePreferencesNowOnOpen = false;
        public int numberOfUpdates = 0;
        public string currentSelectedPreferencesTopic = "Main Preferences";
        private string newNotificationActionToAdd = "";
        private string searchInDependenciesValue = "";
        private string newPackageNameToAdd = "";

        //Preferences code

        static void LoadThePreferences()
        {
            //Create the default directory, if not exists
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData/Preferences"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets/_AssetsData", "Preferences");

            //Try to load the preferences file
            natPreferences = (NativeAndroidPreferences)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset", typeof(NativeAndroidPreferences));
            //Validate the preference file. if this preference file is of another project, delete then 
            if (natPreferences != null)
            {
                if (natPreferences.projectName != Application.productName)
                {
                    AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                    natPreferences = null;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            //If null, create and save a preferences file 
            if (natPreferences == null)
            {
                natPreferences = ScriptableObject.CreateInstance<NativeAndroidPreferences>();
                natPreferences.projectName = Application.productName;
                AssetDatabase.CreateAsset(natPreferences, "Assets/Plugins/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SaveThePreferences()
        {
            //Save the preferences in Prefs.asset
            natPreferences.projectName = Application.productName;

            EditorUtility.SetDirty(natPreferences);
            AssetDatabase.SaveAssets();
        }

        //Setup window code

        public static void OpenWindow(bool savePreferencesOnOpen)
        {
            //Method to open the Window
            var window = GetWindow<Preferences>("NAT Prefs");
            window.minSize = new Vector2(700, 600);
            window.maxSize = new Vector2(700, 600);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();

            //Get parameters
            savePreferencesNowOnOpen = savePreferencesOnOpen;
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Load the preferences, if is null
            if (natPreferences == null)
                LoadThePreferences();
            //Load the resources
            Texture iconOfUi = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Icon.png", typeof(Texture));
            Texture topicDefault = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/TopicDefault.png", typeof(Texture));
            Texture topicSelected = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/TopicSelected.png", typeof(Texture));
            Texture nadIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/NAD.png", typeof(Texture));
            Texture ilvIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/ILV.png", typeof(Texture));
            Texture notificationActionIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/NotificationAction.png", typeof(Texture));
            Texture removeIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Remove.png", typeof(Texture));
            Texture dependencyRow = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/DependencyRow.png", typeof(Texture));
            Texture packageNameIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/PackageName.png", typeof(Texture));
            Texture taskSourceCodeIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/SourceCode.png", typeof(Texture));
            //Load the null images from default images, if have null images in preferences
            if (natPreferences.iconOfNotifications == null)
                natPreferences.iconOfNotifications = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/NotificationsIcon.png", typeof(Texture2D));
            if (natPreferences.iconWebviewBack == null)
                natPreferences.iconWebviewBack = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewBack.png", typeof(Texture2D));
            if (natPreferences.iconWebviewForward == null)
                natPreferences.iconWebviewForward = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewForward.png", typeof(Texture2D));
            if (natPreferences.iconWebviewClose == null)
                natPreferences.iconWebviewClose = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewClose.png", typeof(Texture2D));
            if (natPreferences.iconWebviewError == null)
                natPreferences.iconWebviewError = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewError.png", typeof(Texture2D));
            if (natPreferences.iconWebviewHome == null)
                natPreferences.iconWebviewHome = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewHome.png", typeof(Texture2D));
            if (natPreferences.iconWebviewFavicon == null)
                natPreferences.iconWebviewFavicon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewFavicon.png", typeof(Texture2D));
            if (natPreferences.iconWebviewRefresh == null)
                natPreferences.iconWebviewRefresh = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/WebviewRefresh.png", typeof(Texture2D));
            if (natPreferences.prwBackgroundImage == null)
                natPreferences.prwBackgroundImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/BackgroundOne.png", typeof(Texture2D));
            if (natPreferences.restartBackgroundImage == null)
                natPreferences.restartBackgroundImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/BackgroundTwo.png", typeof(Texture2D));
            if (natPreferences.iconMapsClose == null)
                natPreferences.iconMapsClose = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsClose.png", typeof(Texture2D));
            if (natPreferences.iconMapsFavicon == null)
                natPreferences.iconMapsFavicon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsIcon.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker1 == null)
                natPreferences.iconMapsMarker1 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker1.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker2 == null)
                natPreferences.iconMapsMarker2 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker2.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker3 == null)
                natPreferences.iconMapsMarker3 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker3.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker4 == null)
                natPreferences.iconMapsMarker4 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker4.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker5 == null)
                natPreferences.iconMapsMarker5 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker5.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker6 == null)
                natPreferences.iconMapsMarker6 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker6.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker7 == null)
                natPreferences.iconMapsMarker7 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker7.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker8 == null)
                natPreferences.iconMapsMarker8 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker8.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker9 == null)
                natPreferences.iconMapsMarker9 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker9.png", typeof(Texture2D));
            if (natPreferences.iconMapsMarker10 == null)
                natPreferences.iconMapsMarker10 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/MapsMarker10.png", typeof(Texture2D));
            if (natPreferences.iconCameraClose == null)
                natPreferences.iconCameraClose = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraClose.png", typeof(Texture2D));
            if (natPreferences.iconCameraFavicon == null)
                natPreferences.iconCameraFavicon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraIcon.png", typeof(Texture2D));
            if (natPreferences.iconCameraLightOff == null)
                natPreferences.iconCameraLightOff = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraLightOff.png", typeof(Texture2D));
            if (natPreferences.iconCameraLightOn == null)
                natPreferences.iconCameraLightOn = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraLightOn.png", typeof(Texture2D));
            if (natPreferences.iconCameraStartRec == null)
                natPreferences.iconCameraStartRec = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraStartRecording.png", typeof(Texture2D));
            if (natPreferences.iconCameraStopRec == null)
                natPreferences.iconCameraStopRec = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraStopRecording.png", typeof(Texture2D));
            if (natPreferences.iconCameraSwitch == null)
                natPreferences.iconCameraSwitch = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraSwitch.png", typeof(Texture2D));
            if (natPreferences.iconCameraTakePhoto == null)
                natPreferences.iconCameraTakePhoto = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/CameraTakePhoto.png", typeof(Texture2D));
            if (natPreferences.filePickerBackgroundImage == null)
                natPreferences.filePickerBackgroundImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/BackgroundThree.png", typeof(Texture2D));
            if (natPreferences.playGamesBackgroundImage == null)
                natPreferences.playGamesBackgroundImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT Res/BackgroundFour.png", typeof(Texture2D));
            //Cancel if NAT Core not found on project
            if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar", typeof(object)) == null)
            {
                EditorGUILayout.HelpBox("Could not find \"NAT Core.aar\", please reinstall Native Android Toolkit to correct this problem.", MessageType.Error);
                return;
            }
            //Cancel if some of the resources not found on project
            if (iconOfUi == null || topicDefault == null || topicSelected == null || nadIcon == null || notificationActionIcon == null || removeIcon == null || dependencyRow == null || ilvIcon == null || packageNameIcon == null || taskSourceCodeIcon == null)
            {
                EditorGUILayout.HelpBox("Unable to load required files. Please reinstall Native Android Toolkit to correct this problem.", MessageType.Error);
                return;
            }

            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);
            //Topbar
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(4);
            GUILayout.BeginVertical(GUILayout.Height(44), GUILayout.Width(48));
            GUILayout.Space(6);
            GUILayout.Box(iconOfUi, iconStyle, GUILayout.Width(48), GUILayout.Height(44));
            GUILayout.Space(6);
            GUILayout.EndVertical();
            GUILayout.Space(6);
            GUILayout.BeginVertical();
            GUILayout.Space(11);
            GUIStyle titulo = new GUIStyle();
            titulo.fontSize = 25;
            titulo.normal.textColor = Color.black;
            titulo.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Native Android Toolkit", titulo);
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            GUIStyle subTitulo = new GUIStyle();
            subTitulo.fontSize = 11;
            subTitulo.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Preferences", subTitulo);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //Start render the layout
            EditorGUILayout.BeginHorizontal();
            //Render the preferences topics
            EditorGUILayout.BeginVertical("box");
            scrollPosTopics = EditorGUILayout.BeginScrollView(scrollPosTopics, GUILayout.Width(200), GUILayout.Height(484));
            UI_PreferencesTopics(topicDefault, topicSelected);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            //Render the preferences for selected topic
            EditorGUILayout.BeginVertical("box");
            scrollPosPreferences = EditorGUILayout.BeginScrollView(scrollPosPreferences, GUILayout.Width(476), GUILayout.Height(484));
            switch (currentSelectedPreferencesTopic)
            {
                case "Main Preferences":
                    UI_Topic_MainPreferences(dependencyRow);
                    break;
                case "Permissions":
                    UI_Topic_PermissionsUsage();
                    break;
                case "Notifications":
                    UI_Topic_Notifications(notificationActionIcon, removeIcon);
                    break;
                case "Webview":
                    UI_Topic_WebviewLayout();
                    break;
                case "Utils":
                    UI_Topic_Utils();
                    break;
                case "Location":
                    UI_Topic_Location();
                    break;
                case "Camera":
                    UI_Topic_Camera();
                    break;
                case "Applications":
                    UI_Topic_Applications(packageNameIcon, removeIcon);
                    break;
                case "Date Time":
                    UI_Topic_DateTime();
                    break;
                case "Files":
                    UI_Topic_Files();
                    break;
                case "Tasks":
                    UI_Topic_Tasks(dependencyRow, taskSourceCodeIcon);
                    break;
                case "Play Games":
                    UI_Topic_PlayGames();
                    break;
                case "NAT Core Library":
                    UI_Topic_NATCoreLibrary();
                    break;
                case "Additional Tools":
                    UI_Topic_AdditionalTools(nadIcon, ilvIcon);
                    break;
            }
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Remember to read the Native Android Toolkit documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            //End the render of the layout
            EditorGUILayout.EndHorizontal();

            //Render the bottom bar
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            //Save the changes
            if (GUILayout.Button("Save Preferences", GUILayout.Height(32), GUILayout.Width(200)))
            {
                SavePreferencesChanges();
                return;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            //If save preferences on open is true, save preferences
            if (savePreferencesNowOnOpen == true)
            {
                numberOfUpdates += 1;
                if (numberOfUpdates >= 5)
                {
                    SavePreferencesChanges();
                    savePreferencesNowOnOpen = false;
                    numberOfUpdates = 0;
                    return;
                }
            }

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        void UI_PreferencesTopics(Texture topicDefault, Texture topicSelected)
        {
            //Prepare the list of options
            string[] topics = new string[] { "Main Preferences", "Permissions", "Notifications", "Webview", "Utils", "Location", "Camera", "Applications", "Date Time", "Files", "Tasks", "Play Games", "NAT Core Library", "Additional Tools" };
            //Render all topics
            for (int i = 0; i < topics.Length; i++)
            {
                //Style for selected
                if (currentSelectedPreferencesTopic == topics[i])
                {
                    if (GUILayout.Button(new GUIContent("", topicSelected), GUIStyle.none, GUILayout.Height(24), GUILayout.Width(200)))
                        currentSelectedPreferencesTopic = topics[i];
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUI.LabelField(new Rect(lastRect.x + 4, lastRect.y, 200, 24), topics[i]);
                }
                //Style for non selected
                if (currentSelectedPreferencesTopic != topics[i])
                {
                    if (GUILayout.Button(new GUIContent("", topicDefault), GUIStyle.none, GUILayout.Height(24), GUILayout.Width(200)))
                        currentSelectedPreferencesTopic = topics[i];
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUI.LabelField(new Rect(lastRect.x + 4, lastRect.y, 200, 24), topics[i]);
                }
            }
        }

        void UI_Topic_MainPreferences(Texture dependencyRow)
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //Android Manifest Settings
            EditorGUILayout.LabelField("Project Android Manifest", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.modifyAndroidManifest = (NativeAndroidPreferences.ModifyManifest)EditorGUILayout.EnumPopup(new GUIContent("Modify Android Manifest",
                                            "Enable this option if you want Native Android Toolkit to make changes to your project AndroidManifest.xml."),
                                            natPreferences.modifyAndroidManifest);
            if (natPreferences.modifyAndroidManifest == NativeAndroidPreferences.ModifyManifest.YesGenerateNewIfNotExists)
            {
                natPreferences.declareUnityPlayerActivity = EditorGUILayout.Toggle(new GUIContent("Declare UnityActivity",
                    "If you enable this option, Native Android Toolkit will declare UnityPlayer Activity in the manifest so that the Android system can access this default activity from your app. If you do not enable this option, Unity activity will not be declared in the manifest and the Android system will not be able to access it. The default setting is to leave this option enabled. If you disable this option, the default Unity activity (which your app will run on) will not be accessible by the Android system, and if it is the Main activity, your app icon will no longer appear to the user."),
                    natPreferences.declareUnityPlayerActivity);
                if (natPreferences.declareUnityPlayerActivity == true)
                {
                    EditorGUI.indentLevel += 1;
                    natPreferences.unityPlayerActivityIsMain = EditorGUILayout.Toggle(new GUIContent("Is Main Activity",
                    "If you enable this option, UnityPlayerActivity will be set as the Main activity and whenever the user touches your app icon, the UnityPlayer Activity will open. This is the default setting for Unity Engine generated APKs. If you need to change the activity that will be runned when the user clicks on your app icon, disable this option. If your app does not have a Main activity, your app will not display an icon on the Android system even if it is installed."),
                    natPreferences.unityPlayerActivityIsMain);
                    EditorGUI.indentLevel -= 1;
                }

                natPreferences.skipPermissionsDialog = EditorGUILayout.Toggle(new GUIContent("Skip Permissions Dialog",
                    "By default Unity always asks the user for the crucial permissions that your application will use. This happens right away when the user opens your app. Enabling this option will cause your app not to automatically request permissions when opened, so you can request these permissions whenever you prefer using the NAT C# API."),
                    natPreferences.skipPermissionsDialog);
                GUILayout.Space(10);

                //If not found a AndroidManifest.xml file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) == null)
                {
                    EditorGUILayout.HelpBox("The file \"AndroidManifest.xml\" could not be found. A new file will be generated in your project, containing only the options you choose above. This will not change the way your game works.", MessageType.Info);
                }
                //If found a AndroidManifest.xml file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) != null)
                {
                    //Butons to manager the AndroidManifest.xml
                    EditorGUILayout.HelpBox("The file \"AndroidManifest.xml\" was found. It will only be modified in the options you choose above. The other parameters will remain intact.\n\nThe path to AndroidManifest.xml is\nAssets/Plugins/Android/AndroidManifest.xml", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Open Manifest", GUILayout.Height(20), GUILayout.Width(226)))
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(TextAsset)));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete Manifest", GUILayout.Height(20), GUILayout.Width(226)))
                        if (EditorUtility.DisplayDialog("Delete Android Manifest?", "Deleting AndroidManifest will cause Unity to use the default AndroidManifest that itself generates. Continue?", "Yes", "Cancel") == true)
                        {
                            AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest.xml");
                            natPreferences.modifyAndroidManifest = NativeAndroidPreferences.ModifyManifest.No;
                        }
                    EditorGUILayout.EndHorizontal();
                }
            }

            //Gradle Properties Settings
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Project Gradle Properties", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.modifyGradleProperties = (NativeAndroidPreferences.ModifyGradleProperties)EditorGUILayout.EnumPopup(new GUIContent("Modify Gradle Properties",
                                            "Enable this option if you want Native Android Toolkit to make changes to your project gradleTemplate.properties."),
                                            natPreferences.modifyGradleProperties);
            if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.YesGenerateNewIfNotExists)
            {
                natPreferences.enableDexingArtifactTransform = EditorGUILayout.Toggle(new GUIContent("Enable Dex. Artif. Transf.",
                    "With \"Dexing Artifact Transform\" disabled in Gradle Properties, all Native Android Toolkit libraries and functions can work without problems, regardless of the current version of Gradle your Unity Editor uses."),
                    natPreferences.enableDexingArtifactTransform);
                if (natPreferences.enableDexingArtifactTransform == true)
                    EditorGUILayout.HelpBox("The \"Dexing Artifact Transform\" option needs to be turned off so that all NAT functions and libraries can work without problems in different versions of Gradle.", MessageType.Error);
                GUILayout.Space(10);

                //If not found a gradleTemplate.properties file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/gradleTemplate.properties", typeof(object)) == null)
                {
                    EditorGUILayout.HelpBox("The file \"gradleTemplate.properties\" could not be found. A new file will be generated in your project, containing only the options you choose above. This will not change the way your game works.", MessageType.Info);
                }
                //If found a gradleTemplate.properties file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/gradleTemplate.properties", typeof(object)) != null)
                {
                    //Buttons to manager the gradleTemplate.properties
                    EditorGUILayout.HelpBox("The file \"gradleTemplate.properties\" was found. It will only be modified in the options you choose above. The other parameters will remain intact.\n\nThe path to gradleTemplate.properties is\nAssets/Plugins/Android/gradleTemplate.properties", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Show Properties", GUILayout.Height(20), GUILayout.Width(226)))
                    {
                        string gradleTemplateContent = File.ReadAllText("Assets/Plugins/Android/gradleTemplate.properties");
                        EditorUtility.DisplayDialog("Content of gradleTemplate.properties", "This is the content of file gradleTemplate.properties...\n\n" + gradleTemplateContent, "Ok");
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete Properties", GUILayout.Height(20), GUILayout.Width(226)))
                        if (EditorUtility.DisplayDialog("Delete Gradle Template Properties?", "Deleting Gradle Template Properties will cause Unity to use the default Gradle Template Properties that itself generates. Continue?", "Yes", "Cancel") == true)
                        {
                            AssetDatabase.DeleteAsset("Assets/Plugins/Android/gradleTemplate.properties");
                            natPreferences.modifyGradleProperties = NativeAndroidPreferences.ModifyGradleProperties.No;
                        }
                    EditorGUILayout.EndHorizontal();
                }
            }

            //NAT Dependencies Resolver
            GUILayout.Space(20);
            EditorGUILayout.LabelField("NAT Dependencies Resolver", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);
            string lastDependenciesResolverRunDateTime = File.ReadAllLines("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini")[1].Replace(" ", " at ");
            int dependenciesResolverLogFileSizeKb = 0;
            if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt") == true)
                dependenciesResolverLogFileSizeKb = (int)(new FileInfo("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt").Length / 1024);
            EditorGUILayout.HelpBox("Below are all the AAR/JAR Libraries included and needed for the Native Android Toolkit to work properly. If you are having issues with duplicate AAR/JAR Libraries, compile or build issues involving NAT, try running the NAT Dependencies Resolver.\n\nThe Dependencies Resolver was last run on\n" + lastDependenciesResolverRunDateTime, MessageType.Info);
            //Try to load the dependencies informations
            NativeAndroidDependencies natDependencies = (NativeAndroidDependencies)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies.asset", typeof(NativeAndroidDependencies));
            //Show all dependencies information
            Texture aarIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Icon.png", typeof(Texture));
            Texture aarNotFound = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Not-Found.png", typeof(Texture));
            Texture aarEnabled = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Enabled.png", typeof(Texture));
            Texture aarDisabled = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Disabled.png", typeof(Texture));
            GUIStyle dependencyRowBox = new GUIStyle();
            dependencyRowBox.normal.background = (Texture2D)dependencyRow;
            GUIStyle versionInfoText = new GUIStyle();
            versionInfoText.alignment = TextAnchor.MiddleRight;
            versionInfoText.normal.textColor = new Color(0.36f, 0.36f, 0.36f, 1.0f);
            searchInDependenciesValue = EditorGUILayout.TextField(new GUIContent("",
                  "Type here to search the dependencies included with the NAT."),
                  searchInDependenciesValue);
            GUILayout.Space(8);
            EditorGUILayout.BeginVertical(dependencyRowBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(7);
            GUILayout.Box(aarIcon, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
            EditorGUILayout.LabelField("AAR - xyz.windsoft.mtassets.nat", GUILayout.Width(310));
            EditorGUILayout.LabelField("Updated", versionInfoText, GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.Box(aarEnabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
            EditorGUILayout.EndVertical();
            bool isRowWithBackground = false;
            int resultsFound = 0;
            foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in natDependencies.natCoreAARDependencies)
            {
                //If typed something in seach, and this dependency not have the searched term, skip
                if (string.IsNullOrEmpty(searchInDependenciesValue) == false && aar.packageName.Contains(searchInDependenciesValue) == false)
                    continue;

                if (isRowWithBackground == true)
                    EditorGUILayout.BeginVertical(dependencyRowBox);
                if (isRowWithBackground == false)
                    EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(7);
                GUILayout.Box(aarIcon, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                EditorGUILayout.LabelField("AAR - " + aar.packageName, GUILayout.Width(310));
                EditorGUILayout.LabelField(aar.fileVersion, versionInfoText, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                bool aarEnabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath);
                bool aarDisabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled");
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If AAR is enabled
                    GUILayout.Box(aarEnabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If AAR is disabled
                    GUILayout.Box(aarDisabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == false) //<- If AAR not found
                    GUILayout.Box(aarNotFound, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.EndVertical();
                //Change the next background
                isRowWithBackground = !isRowWithBackground;
                resultsFound += 1;
            }
            foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in natDependencies.natCoreJARDependencies)
            {
                //If typed something in seach, and this dependency not have the searched term, skip
                if (string.IsNullOrEmpty(searchInDependenciesValue) == false && jar.jarName.Contains(searchInDependenciesValue) == false)
                    continue;

                if (isRowWithBackground == true)
                    EditorGUILayout.BeginVertical(dependencyRowBox);
                if (isRowWithBackground == false)
                    EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(7);
                GUILayout.Box(aarIcon, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                EditorGUILayout.LabelField(new GUIContent("JAR - " + jar.jarName, "Total of " + jar.jarClasses.Count + " classes found!"), GUILayout.Width(310));
                EditorGUILayout.LabelField(jar.fileVersion, versionInfoText, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                bool aarEnabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath);
                bool aarDisabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled");
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If JAR is enabled
                    GUILayout.Box(aarEnabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If JAR is disabled
                    GUILayout.Box(aarDisabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == false) //<- If JAR not found
                    GUILayout.Box(aarNotFound, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.EndVertical();
                //Change the next background
                isRowWithBackground = !isRowWithBackground;
                resultsFound += 1;
            }
            if (string.IsNullOrEmpty(searchInDependenciesValue) == false)
            {
                GUILayout.Space(15);
                if (resultsFound > 0)
                    EditorGUILayout.LabelField(resultsFound + " dependencies found for search term", new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                if (resultsFound == 0)
                    EditorGUILayout.LabelField("No dependencies found for the search term!", new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(15);
            }
            GUILayout.Space(10);
            //Show button controls
            if (GUILayout.Button("Edit Dependencies", GUILayout.Height(20)))
                LibsEditor.OpenWindow();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Dependencies Resolver", GUILayout.Height(20), GUILayout.Width(226)))
                LibsResolver.RunNativeAndroidToolkitDependenciesResolver(true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Log - " + dependenciesResolverLogFileSizeKb + " Kb", GUILayout.Height(20), GUILayout.Width(226)))
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", typeof(TextAsset)));
            EditorGUILayout.EndHorizontal();
        }

        void UI_Topic_PermissionsUsage()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Permissions Settings
            EditorGUILayout.LabelField("Permissions Usage Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            EditorGUILayout.HelpBox("Here you can disable or enable permissions that will be used by NAT. If you disable any permission here, NAT will no longer ask for them in your APK. This is useful to control which permissions NAT will ask and will not ask for in your APK and consequently in stores, like Google Play. Permissions enabled or disabled here only apply to NAT.\nAlso note that even if some permission are disabled here, if there is any other plugin in your project that might be asking for that permission, the plugin in question will still put the permission in your APK.", MessageType.Info);
            GUILayout.Space(10);

            natPreferences.vibratePermission = EditorGUILayout.Toggle(new GUIContent("Vibrate",
                        "This permission is used for device vibration functions. With this permission your app can make the device vibrate using NAT."),
                        natPreferences.vibratePermission);

            natPreferences.accessWifiStatePermission = EditorGUILayout.Toggle(new GUIContent("Access Wifi State",
                       "This permission allows your app to obtain Wi-Fi network operating data, such as the name of the connected Wi-Fi network, if there is internet available, and so on."),
                       natPreferences.accessWifiStatePermission);

            natPreferences.accessNetworkStatePermission = EditorGUILayout.Toggle(new GUIContent("Access Network State",
                       "With this permission NAT can check for Internet connectivity information on the device. Among this information are network availability, type of network (3G, 4G, etc.)."),
                       natPreferences.accessNetworkStatePermission);

            natPreferences.accessCoarseLocation = EditorGUILayout.Toggle(new GUIContent("Access Coarse Location",
                       "This permission allows your app to get an estimated and current location of the device your app is running on. This location is obtained based on the device's network provider."),
                       natPreferences.accessCoarseLocation);

            natPreferences.accessFineLocation = EditorGUILayout.Toggle(new GUIContent("Access Fine Location",
                       "This permission allows your app to get a current and accurate location of the device your app is running on. This location is obtained based on the device's GPS and network provider.\n\nThis permission is seen as more dangerous than permission \"Access Coarse Location\" by the Android system. Use it only if necessary."),
                       natPreferences.accessFineLocation);

            natPreferences.camera = EditorGUILayout.Toggle(new GUIContent("Camera",
                       "This permission is required for NAT Camera functions. How to get the current Camera view, take pictures, use the device's Flash light, and so on."),
                       natPreferences.camera);

            natPreferences.recordAudio = EditorGUILayout.Toggle(new GUIContent("Record Audio",
                       "This permission is required for NAT Micrphone functions. How to record what is being captured by the Microphone and so on."),
                       natPreferences.recordAudio);

            natPreferences.queryAllPackages = EditorGUILayout.Toggle(new GUIContent("Query All Packages",
                       "This permission allows your app to query and interact with all Apps installed on the user's device. It allows your application to interact with ALL installed applications, even those not listed in the \"Applications\" class preferences.\n\nThis permission is considered sensitive by Google Play. If you want to publish your app on this store, it will not accept that your app uses this permission unless your app depends on this functionality as a core feature for example."),
                       natPreferences.queryAllPackages);

            natPreferences.accessFilesAndMedia = EditorGUILayout.Toggle(new GUIContent("Access Files And Media",
                       "Enabling this permission will allow your app to read and write files in device memory, outside the \"AppFiles\" and \"AppCache\" scopes. That way your application will be able to read, edit, create or update files in the protected scopes, in the device's internal memory, such as \"DCIM\", \"Pictures\", \"Recordings\", \"Musics\" and etc. See the documentation for more details.\n\nNote that you do not need to activate this permission to have full read and write access to the \"AppFiles\" and \"AppCache\" scopes as these two scopes are intended for your app's exclusive use and therefore do not have any type of protection applied by the Android system.\n\nEnabling this permission will ensure that your APK contains READ_EXTERNAL_STORAGE and WRITE_EXTERNAL_STORAGE permissions. The WRITE_EXTERNAL_STORAGE permission will only be displayed and used by your APK if the device is Android 10 or lower."),
                       natPreferences.accessFilesAndMedia);

            natPreferences.foregroundService = EditorGUILayout.Toggle(new GUIContent("Foreground Service",
                       "This permission allows your app to start services on the user's device. Typically the services are used to perform quick background tasks on the user's device like creating cache files to speed up functions etc. If you disable this permission, NAT should not be adversely affected, furthermore, with this permission enabled, the user's device should also not have its speed, battery, or other things affected. Usually Unity also automatically includes this permission in the built APKs."),
                       natPreferences.foregroundService);

            natPreferences.scheduleExactAlarm = EditorGUILayout.Toggle(new GUIContent("Schedule Exact Alarm",
                       "Starting with Android 12, for apps to be able to schedule precise tasks with more accurate times and a certain higher execution priority, this permission is required. This permission does nothing from Android 1 to Android 11, but is required from Android 12 onwards if you want your app to be able to deliver more accurate notifications and perform more accurate tasks on devices running Android 12 or newer.\n\nThis permission is not considered sensitive or dangerous by Google Play, nor does it require the user to allow it through a permission window, however, Google recommends that it only be used if you intend to use it to deliver a feature aimed at the user of your application and that is useful.\n\nThis permission loses effect if the user removes your app from \"Special App Access/Alarms and Reminders\" in the Android settings, but it may work again if the user adds your app again to \"Special App Access/Alarms and Reminders\" in the settings."),
                       natPreferences.scheduleExactAlarm);
            //Validate permission activation
            if (natPreferences.accessFineLocation == true && natPreferences.accessCoarseLocation == false)
            {
                EditorUtility.DisplayDialog("Permissions Error", "Unable to activate \"Access Fine Location\" permission. This permission requires \"Access Coarse Location\" to be enabled too!", "Ok");
                natPreferences.accessFineLocation = false;
            }
            if (natPreferences.scheduleExactAlarm == false)
            {
                natPreferences.enableAccurateNotifyForAndroid12OrNewer = false;
                natPreferences.enableAccurateTaskForAndroid12OrNewer = false;
            }
            //Show the permissions warnign, if necessary
            if (natPreferences.vibratePermission == false || natPreferences.accessWifiStatePermission == false || natPreferences.accessNetworkStatePermission == false ||
                natPreferences.accessCoarseLocation == false || natPreferences.accessFineLocation == false || natPreferences.camera == false || natPreferences.recordAudio == false ||
                natPreferences.queryAllPackages == false || natPreferences.accessFilesAndMedia == false || natPreferences.foregroundService == false || natPreferences.scheduleExactAlarm == false)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Disabling a permission will cause some Native Android Toolkit API methods to stop working. For example, disabling Camera permission will cause Camera, Video and QR Reader functions to stop working.\n\nConsult the documentation to understand what permissions are used by the Native Android Toolkit functions.", MessageType.Warning);
            }

            //Permissions Requester Wizard Settings
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Permissions Requester Wizard Layout", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.prwBackgroundImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Background Image",
                                    "This will be the image applied as the background of the Permissions Requester Wizard interface when you call it."),
                                    natPreferences.prwBackgroundImage, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.prwBackgroundImage != null && AssetDatabase.GetAssetPath(natPreferences.prwBackgroundImage).Contains("MTAssets NAT AAR") == true)
                natPreferences.prwBackgroundImage = null;
            if (natPreferences.prwBackgroundImage != null)
            {
                bool isExtValid = true;
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.prwBackgroundImage)).Replace(".", "");
                if (extension != "png")
                {
                    EditorGUILayout.HelpBox("Please provide a PNG image to ensure the best image display.", MessageType.Error);
                    isExtValid = false;
                }
                natPreferences.prwResourcesIsValid = isExtValid;
            }

            EditorGUILayout.BeginHorizontal();
            natPreferences.prwTitleColor = EditorGUILayout.ColorField(new GUIContent("Wizard Title Color",
                        "This will be the color applied to the Permissions Requester Wizard title text, which appears in the upper Toolbox."),
                        natPreferences.prwTitleColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.prwTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            natPreferences.prwBackgroundColor = EditorGUILayout.ColorField(new GUIContent("Wizard Back Color",
                                    "This is the background color of the top Toolbox of Permissions Requester Wizard."),
                                    natPreferences.prwBackgroundColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.prwBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);
            EditorGUILayout.EndHorizontal();
        }

        void UI_Topic_Notifications(Texture notificationActionIcon, Texture removeIcon)
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //Notifications Settings
            EditorGUILayout.LabelField("Notifications Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.iconOfNotifications = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon of Notifications",
                        "The icon that will represent the notifications launched by your app in the device status bar."),
                        natPreferences.iconOfNotifications, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconOfNotifications != null && AssetDatabase.GetAssetPath(natPreferences.iconOfNotifications).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconOfNotifications = null;
            if (natPreferences.iconOfNotifications != null)
            {
                bool isResValid = true;
                bool isExtValid = true;
                //Verify resolution
                if (natPreferences.iconOfNotifications.width != 64 || natPreferences.iconOfNotifications.height != 64)
                {
                    EditorGUILayout.HelpBox("Please provide a transparent icon of exactly 64x64 pixels resolution to ensure the best image display in the status bar.", MessageType.Error);
                    isResValid = false;
                }
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconOfNotifications)).Replace(".", "");
                if (extension != "png")
                {
                    EditorGUILayout.HelpBox("Please provide a transparent PNG icon to ensure the best image display in the status bar.", MessageType.Error);
                    isExtValid = false;
                }
                natPreferences.iconIsValid = (isResValid == true && isExtValid == true) ? true : false;
            }
            natPreferences.enableAccurateNotify = EditorGUILayout.Toggle(new GUIContent("Enable Accurate Notify",
                       "Some Android devices have their own battery savers. Many of these savers are aggressive to the point of killing background tasks from third-party apps (like MIUI from Xiaomi, Smart Manager from Samsung, etc.) and generally this prevents apps from running background tasks how to deliver scheduled notifications. NAT does its best to deliver notifications in the best way for the user and this option is one of those ways.\n\nIf this option is deactivated, the device will manage the delivery of notifications from your app in its own way and with normal priority, according to the rules of the Android system and battery optimizers of each brand. Your notification may even be delayed or never delivered depending on how the device brand has modified the Android system to optimize it.\n\nBy enabling this option, NAT will use a different method to deliver notifications. This method will consume a little more battery (only when delivering the notification) on the device (if it works on that device), but will try to deliver notifications as accurately as possible.\n\nThis option only works up to Android 11. On Android 12 or newer, this option has no effect, due to the new accuracy and task execution policies applied in this version of Android.\n\nPlease refer to the documentation for more details on notification delivery accuracy, battery usage and Android system optimizations."),
                       natPreferences.enableAccurateNotify);
            if (natPreferences.enableAccurateNotify == true)
            {
                EditorGUI.indentLevel += 1;
                natPreferences.enableAccurateNotifyForAndroid12OrNewer = EditorGUILayout.Toggle(new GUIContent("For Android 12+ Too",
                            "If this option is enabled, NAT will attempt to deliver notifications more accurately on Android 12 and above devices as well. This option is designed for devices with Android 12 and above, since for the delivery of accurate notifications on devices with Android 12 and above, some other methods are required, including the use of an additional permission.\n\nTo enable this option, you need to have the \"Schedule Exact Alarm\" permission enabled in the NAT Permission settings!"),
                            natPreferences.enableAccurateNotifyForAndroid12OrNewer);
                if (natPreferences.scheduleExactAlarm == false)
                {
                    EditorGUILayout.HelpBox("In order for you to enable this option, the \"Schedule Exact Alarm\" permission must be enabled in the NAT Permission settings!", MessageType.Warning);
                    natPreferences.enableAccurateNotifyForAndroid12OrNewer = false;
                }
                EditorGUI.indentLevel -= 1;
            }
            if (natPreferences.enableAccurateNotify == false)
                natPreferences.enableAccurateNotifyForAndroid12OrNewer = false;

            natPreferences.forceVibrateOnNotify = EditorGUILayout.Toggle(new GUIContent("Force Vibrate On Notify",
                       "Enable this option to force the device to vibrate when receiving a notification from your app when using NAT."),
                       natPreferences.forceVibrateOnNotify);

            EditorGUILayout.BeginHorizontal();
            natPreferences.colorInNotifyArea = EditorGUILayout.ColorField(new GUIContent("Color In Notify Area",
                                    ""),
                                    natPreferences.colorInNotifyArea);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.colorInNotifyArea = new Color(0.0f, 0.37f, 0.49f, 1.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            natPreferences.interactReceiverColor = EditorGUILayout.ColorField(new GUIContent("Interact Receiver Color",
                        "Notification Interact Receiver is an activity that is opened before UnityPlayerActivity is opened. The Interact Receiver only opens when the user interacts with a notification that has an Action defined. The default Interact Receiver color is black and then a black screen is displayed for a few moments before continuing to UnityPlayerActivity. Here you can change the color of this screen.\n\nThe Notification Interact Receiver is responsible for processing the interaction that the user has made with your notification that has one or more Actions."),
                        natPreferences.interactReceiverColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.interactReceiverColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            //Notifications Actions Settings
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Notifications Actions", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            EditorGUILayout.HelpBox("Below you can view, remove or add Notifications Actions to Native Android Toolkit. Notifications Actions added here will be available for you to use in your C# codes.\n\nNotifications Actions can be assigned to Notifications or Notification Buttons and serve as an identifier. When a user clicks on a Notification or Button of a Notification sent by your app, your app opens and NAT informs you through an Event that your app was opened through an interaction with a Notification and informs the respective Notification Action regarding that interaction. See documentation for more details.", MessageType.Info);
            GUILayout.Space(10);
            int idOfItemToRemove = -1;
            foreach (string notificationAction in natPreferences.notificationsActions)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(notificationActionIcon, iconStyle, GUILayout.Width(16), GUILayout.Height(20));
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Action: <b>" + notificationAction + "</b>", new GUIStyle { richText = true });
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                GUILayout.Space(6);
                if (notificationAction == "Undefined" || notificationAction == "ClickAction1" || notificationAction == "ButtonAction1" || notificationAction == "ButtonAction2")
                    if (GUILayout.Button(new GUIContent(removeIcon, ""), GUIStyle.none, GUILayout.Height(14), GUILayout.Width(14)))
                        EditorUtility.DisplayDialog("Error", "It is not possible to delete a predefined Notification Action.", "Ok");
                if (notificationAction != "Undefined" && notificationAction != "ClickAction1" && notificationAction != "ButtonAction1" && notificationAction != "ButtonAction2")
                    if (GUILayout.Button(new GUIContent(removeIcon, "Click here to remove \"" + notificationAction + "\" Notification Action from NAT."), GUIStyle.none, GUILayout.Height(14), GUILayout.Width(14)))
                        for (int i = 0; i < natPreferences.notificationsActions.Count; i++)
                            if (natPreferences.notificationsActions[i] == notificationAction)
                                if (EditorUtility.DisplayDialog("Remove Notification Action?", "If you have any C# scripts that are using this action, you will get compilation errors after removing it.\n\nAre you sure you want to remove Notification Action \"" + notificationAction + "\"?", "Yes", "No") == true)
                                    idOfItemToRemove = i;
                EditorGUILayout.EndVertical();
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
            }
            if (idOfItemToRemove != -1)
            {
                natPreferences.notificationsActions.RemoveAt(idOfItemToRemove);
                UpdateNotificationsActionsAvailabilityForCSharp();
                AssetDatabase.Refresh();
            }
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            newNotificationActionToAdd = EditorGUILayout.TextField(new GUIContent("",
                  "Type here the new Notification Action to add."),
                  newNotificationActionToAdd);
            if (string.IsNullOrEmpty(newNotificationActionToAdd) == false)
                if (newNotificationActionToAdd.Contains(" ") == false && natPreferences.notificationsActions.Contains(newNotificationActionToAdd, StringComparer.OrdinalIgnoreCase) == false && Regex.IsMatch(newNotificationActionToAdd, @"^[a-zA-Z]+$") == true)
                    if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(50)))
                    {
                        natPreferences.notificationsActions.Add(newNotificationActionToAdd);
                        newNotificationActionToAdd = "";
                        UpdateNotificationsActionsAvailabilityForCSharp();
                        AssetDatabase.Refresh();
                    }
            if (string.IsNullOrEmpty(newNotificationActionToAdd) == true)
                if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(50)))
                    EditorUtility.DisplayDialog("Error", "Please enter a valid Notification Action before add.", "Ok");
            EditorGUILayout.EndHorizontal();
            //Validate the content
            if (string.IsNullOrEmpty(newNotificationActionToAdd) == false)
                if (newNotificationActionToAdd.Contains(" ") == true || natPreferences.notificationsActions.Contains(newNotificationActionToAdd, StringComparer.OrdinalIgnoreCase) == true || Regex.IsMatch(newNotificationActionToAdd, @"^[a-zA-Z]+$") == false)
                    EditorGUILayout.HelpBox("Notification Action written above is invalid. Reasons could be one of the reasons below...\n\n- Can not be empty.\n- It must contain only letters.\n- It cannot contain spaces.\n- It shouldn't be a Notification Action that already exists.\n\nReview the Notification Action and make sure it meets these requirements.", MessageType.Error);
        }

        void UI_Topic_WebviewLayout()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Webview Layout Settings
            EditorGUILayout.LabelField("Webview Layout Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.iconsColorTheme = (NativeAndroidPreferences.WebviewIconsColorTheme)EditorGUILayout.EnumPopup(new GUIContent("Icons Color Theme",
                        "This option sets the color theme for the NAT Webview. The theme set here will tint the icons in the Webview. Using the \"Light\" theme will make the icons colored white and using the \"Dark\" theme will make the icons colored black."),
                        natPreferences.iconsColorTheme);

            natPreferences.iconWebviewBack = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Back",
                        "This will be the icon that will represent the \"Back\" button on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewBack, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewBack != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewBack).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewBack = null;

            natPreferences.iconWebviewForward = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Forward",
                        "This will be the icon that will represent the \"Forward\" button on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewForward, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewForward != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewForward).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewForward = null;

            natPreferences.iconWebviewClose = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Close",
                        "This will be the icon that will represent the \"Close\" button on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewClose, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewClose != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewClose).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewClose = null;

            natPreferences.iconWebviewError = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Network Error",
                        "This will be the icon that will represent the \"Network Error Icon\" on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewError, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewError != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewError).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewError = null;

            natPreferences.iconWebviewHome = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Home",
                        "This will be the icon that will represent the \"Home\" button on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewHome, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewHome != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewHome).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewHome = null;

            natPreferences.iconWebviewFavicon = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Default Page Favicon",
                        "This will be the icon that will represent the \"Default Favicon\" on the Webview NAT."),
                        natPreferences.iconWebviewFavicon, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewFavicon != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewFavicon).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewFavicon = null;

            natPreferences.iconWebviewRefresh = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Refresh",
                        "This will be the icon that will represent the \"Refresh\" button on the Webview NAT. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconWebviewRefresh, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconWebviewRefresh != null && AssetDatabase.GetAssetPath(natPreferences.iconWebviewRefresh).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconWebviewRefresh = null;

            EditorGUILayout.BeginHorizontal();
            natPreferences.webviewTitleColor = EditorGUILayout.ColorField(new GUIContent("Webview Title Color",
                        "This will be the color applied to the Webview title text, which appears in the upper Toolbox."),
                        natPreferences.webviewTitleColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.webviewTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            natPreferences.webviewBackgroundColor = EditorGUILayout.ColorField(new GUIContent("Webview Back Color",
                                    "This is the background color of the top Toolbox bars and the bottom Controls."),
                                    natPreferences.webviewBackgroundColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.webviewBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);
            EditorGUILayout.EndHorizontal();
            if (natPreferences.iconWebviewBack != null && natPreferences.iconWebviewForward != null && natPreferences.iconWebviewClose != null && natPreferences.iconWebviewError != null &&
                natPreferences.iconWebviewHome != null && natPreferences.iconWebviewFavicon != null && natPreferences.iconWebviewRefresh != null)
            {
                bool isResValid = true;
                bool isExtValid = true;
                Vector2[] allIconsRes = new Vector2[7];
                string[] allIconsExt = new string[7];

                //Collect data
                allIconsRes[0] = new Vector2(natPreferences.iconWebviewBack.width, natPreferences.iconWebviewBack.height);
                allIconsRes[1] = new Vector2(natPreferences.iconWebviewForward.width, natPreferences.iconWebviewForward.height);
                allIconsRes[2] = new Vector2(natPreferences.iconWebviewClose.width, natPreferences.iconWebviewClose.height);
                allIconsRes[3] = new Vector2(natPreferences.iconWebviewError.width, natPreferences.iconWebviewError.height);
                allIconsRes[4] = new Vector2(natPreferences.iconWebviewHome.width, natPreferences.iconWebviewHome.height);
                allIconsRes[5] = new Vector2(natPreferences.iconWebviewFavicon.width, natPreferences.iconWebviewFavicon.height);
                allIconsRes[6] = new Vector2(natPreferences.iconWebviewRefresh.width, natPreferences.iconWebviewRefresh.height);
                allIconsExt[0] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewBack)).Replace(".", "");
                allIconsExt[1] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewForward)).Replace(".", "");
                allIconsExt[2] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewClose)).Replace(".", "");
                allIconsExt[3] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewError)).Replace(".", "");
                allIconsExt[4] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewHome)).Replace(".", "");
                allIconsExt[5] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewFavicon)).Replace(".", "");
                allIconsExt[6] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconWebviewRefresh)).Replace(".", "");
                //Verify resolution
                foreach (Vector2 resolution in allIconsRes)
                    if (resolution.x != resolution.y)
                        isResValid = false;
                //Verify extension
                foreach (string extension in allIconsExt)
                    if (extension != "png")
                        isExtValid = false;
                if (isResValid == false)
                    EditorGUILayout.HelpBox("Please provide icons that have transparency and that are square (must have the same width and height) for better icon quality.", MessageType.Error);
                if (isExtValid == false)
                    EditorGUILayout.HelpBox("Please provide only PNG icons with transparency for better icon quality.", MessageType.Error);

                natPreferences.webviewIconsIsValid = (isResValid == true && isExtValid == true) ? true : false;
            }
        }

        void UI_Topic_Utils()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Utils Settings
            EditorGUILayout.LabelField("Utils Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.restartBackgroundImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Restart Backg. Image",
                                                "This will be the image applied as the background of the Restarter interface when you call it."),
                                                natPreferences.restartBackgroundImage, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.restartBackgroundImage != null && AssetDatabase.GetAssetPath(natPreferences.restartBackgroundImage).Contains("MTAssets NAT AAR") == true)
                natPreferences.restartBackgroundImage = null;
            if (natPreferences.restartBackgroundImage != null)
            {
                bool isExtValid = true;
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.restartBackgroundImage)).Replace(".", "");
                if (extension != "png")
                {
                    EditorGUILayout.HelpBox("Please provide a PNG image to ensure the best image display.", MessageType.Error);
                    isExtValid = false;
                }
                natPreferences.utilsResourcesIsValid = isExtValid;
            }
        }

        void UI_Topic_Location()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Location Settings
            EditorGUILayout.LabelField("Location Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.googleMapsApiKey = EditorGUILayout.TextField(new GUIContent("Google Maps API Key",
                                                    "This API key is required for NAT and Google Maps Services to be able to access Google Cloud servers and display the map in your app."),
                                                    natPreferences.googleMapsApiKey);
            if (string.IsNullOrEmpty(natPreferences.googleMapsApiKey) == true)
                EditorGUILayout.HelpBox("NAT requires you to provide your Google Cloud API key in order to be able to access Google Maps and display maps in your app. If you don't provide a key, NAT won't be able to display Google Maps in your app, and your app may crash if you still try to use some code to show Google Maps maps in your app. See the documentation to find out how to get an API key.\n\nNote: Location tracking and other location methods will work even without you providing an API key.Only Google Maps maps will be unavailable if you don't provide an API key.", MessageType.Warning);

            //Maps Settings
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Google Maps Layout", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.mapsIconsColorTheme = (NativeAndroidPreferences.MapsIconsColorTheme)EditorGUILayout.EnumPopup(new GUIContent("Icons Color Theme",
                        "This option sets the color theme for the NAT Google Maps. The theme set here will tint the icons in the Map interface. Using the \"Light\" theme will make the icons colored white and using the \"Dark\" theme will make the icons colored black."),
                        natPreferences.mapsIconsColorTheme);

            natPreferences.iconMapsClose = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Close",
                        "This will be the icon that will represent the \"Close\" button on the NAT Google Map. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                        natPreferences.iconMapsClose, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsClose != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsClose).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsClose = null;

            natPreferences.iconMapsFavicon = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Default Map Favicon",
                        "This will be the icon that will represent the \"Default Favicon\" on the NAT Google Map."),
                        natPreferences.iconMapsFavicon, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsFavicon != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsFavicon).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsFavicon = null;

            natPreferences.iconMapsMarker1 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 1",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker1, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker1 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker1).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker1 = null;

            natPreferences.iconMapsMarker2 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 2",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker2, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker2 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker2).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker2 = null;

            natPreferences.iconMapsMarker3 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 3",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker3, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker3 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker3).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker3 = null;

            natPreferences.iconMapsMarker4 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 4",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker4, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker4 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker4).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker4 = null;

            natPreferences.iconMapsMarker5 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 5",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker5, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker5 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker5).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker5 = null;

            natPreferences.iconMapsMarker6 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 6",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker6, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker6 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker6).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker6 = null;

            natPreferences.iconMapsMarker7 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 7",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker7, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker7 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker7).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker7 = null;

            natPreferences.iconMapsMarker8 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 8",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker8, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker8 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker8).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker8 = null;

            natPreferences.iconMapsMarker9 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 9",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker9, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker9 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker9).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker9 = null;

            natPreferences.iconMapsMarker10 = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Map Marker 10",
                        "One of 10 icons that can be used as marker on NAT's Google Maps."),
                        natPreferences.iconMapsMarker10, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconMapsMarker10 != null && AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker10).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconMapsMarker10 = null;

            EditorGUILayout.BeginHorizontal();
            natPreferences.mapsTitleColor = EditorGUILayout.ColorField(new GUIContent("Maps Title Color",
                        "This will be the color applied to the NAT Google Maps title text, which appears in the upper Toolbox."),
                        natPreferences.mapsTitleColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.mapsTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            natPreferences.mapsBackgroundColor = EditorGUILayout.ColorField(new GUIContent("Maps Back Color",
                                    "This is the background color of the top Toolbox bars and the bottom Controls."),
                                    natPreferences.mapsBackgroundColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.mapsBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);
            EditorGUILayout.EndHorizontal();
            if (natPreferences.iconMapsClose != null && natPreferences.iconMapsFavicon != null && natPreferences.iconMapsMarker1 != null && natPreferences.iconMapsMarker2 != null && natPreferences.iconMapsMarker3 != null &&
                natPreferences.iconMapsMarker4 != null && natPreferences.iconMapsMarker5 != null && natPreferences.iconMapsMarker6 != null && natPreferences.iconMapsMarker7 != null && natPreferences.iconMapsMarker8 != null &&
                natPreferences.iconMapsMarker9 != null && natPreferences.iconMapsMarker10 != null)
            {
                bool isResValid = true;
                bool isExtValid = true;
                Vector2[] allIconsRes = new Vector2[12];
                string[] allIconsExt = new string[12];

                //Collect data
                allIconsRes[0] = new Vector2(natPreferences.iconMapsClose.width, natPreferences.iconMapsClose.height);
                allIconsRes[1] = new Vector2(natPreferences.iconMapsFavicon.width, natPreferences.iconMapsFavicon.height);
                allIconsRes[2] = new Vector2(natPreferences.iconMapsMarker1.width, natPreferences.iconMapsMarker1.height);
                allIconsRes[3] = new Vector2(natPreferences.iconMapsMarker2.width, natPreferences.iconMapsMarker2.height);
                allIconsRes[4] = new Vector2(natPreferences.iconMapsMarker3.width, natPreferences.iconMapsMarker3.height);
                allIconsRes[5] = new Vector2(natPreferences.iconMapsMarker4.width, natPreferences.iconMapsMarker4.height);
                allIconsRes[6] = new Vector2(natPreferences.iconMapsMarker5.width, natPreferences.iconMapsMarker5.height);
                allIconsRes[7] = new Vector2(natPreferences.iconMapsMarker6.width, natPreferences.iconMapsMarker6.height);
                allIconsRes[8] = new Vector2(natPreferences.iconMapsMarker7.width, natPreferences.iconMapsMarker7.height);
                allIconsRes[9] = new Vector2(natPreferences.iconMapsMarker8.width, natPreferences.iconMapsMarker8.height);
                allIconsRes[10] = new Vector2(natPreferences.iconMapsMarker9.width, natPreferences.iconMapsMarker9.height);
                allIconsRes[11] = new Vector2(natPreferences.iconMapsMarker10.width, natPreferences.iconMapsMarker10.height);
                allIconsExt[0] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsClose)).Replace(".", "");
                allIconsExt[1] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsFavicon)).Replace(".", "");
                allIconsExt[2] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker1)).Replace(".", "");
                allIconsExt[3] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker2)).Replace(".", "");
                allIconsExt[4] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker3)).Replace(".", "");
                allIconsExt[5] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker4)).Replace(".", "");
                allIconsExt[6] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker5)).Replace(".", "");
                allIconsExt[7] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker6)).Replace(".", "");
                allIconsExt[8] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker7)).Replace(".", "");
                allIconsExt[9] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker8)).Replace(".", "");
                allIconsExt[10] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker9)).Replace(".", "");
                allIconsExt[11] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker10)).Replace(".", "");

                //Verify resolution
                foreach (Vector2 resolution in allIconsRes)
                    if (resolution.x != resolution.y)
                        isResValid = false;
                //Verify extension
                foreach (string extension in allIconsExt)
                    if (extension != "png")
                        isExtValid = false;
                if (isResValid == false)
                    EditorGUILayout.HelpBox("Please provide icons that have transparency and that are square (must have the same width and height) for better icon quality.", MessageType.Error);
                if (isExtValid == false)
                    EditorGUILayout.HelpBox("Please provide only PNG icons with transparency for better icon quality.", MessageType.Error);

                natPreferences.mapsIconsIsValid = (isResValid == true && isExtValid == true) ? true : false;
            }
        }

        void UI_Topic_Camera()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Camera Activation Settings
            if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.No || natPreferences.enableDexingArtifactTransform == true)
            {
                EditorGUILayout.LabelField("Camera Activation", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("Currently the NAT Camera function is disabled in your project. The NAT Camera API will always return that a Camera is not supported when called in Runtime in devices and it will not be possible to use the Camera in runtime. To activate the NAT Camera function it is necessary that you activate some settings that can be seen below, you can click the button below to activate these settings automatically. Basically, by activating these settings, NAT will apply a new rule to your project's Gradle file, which will not affect the functioning of your application, but will allow all NAT Camera dependencies to work without problems, regardless of the Gradle version that Unity uses.\n\nMain Preferences -> Modify Gradle Properties -> Yes Generate New If Not Exists\nMain Preferences -> Enable Dex. Artif. Transf. -> False", MessageType.Error);
                GUILayout.Space(10);
                if (GUILayout.Button("Set Required Settings", GUILayout.Height(20)))
                {
                    natPreferences.modifyGradleProperties = NativeAndroidPreferences.ModifyGradleProperties.YesGenerateNewIfNotExists;
                    natPreferences.enableDexingArtifactTransform = false;
                }
                GUILayout.Space(20);
            }

            //Camera Layout Settings
            EditorGUILayout.LabelField("Camera Layout", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.cameraIconsColorTheme = (NativeAndroidPreferences.CameraIconsColorTheme)EditorGUILayout.EnumPopup(new GUIContent("Icons Color Theme",
                                    "This option sets the color theme for the NAT Camera. The theme set here will tint the icons in the Camera interface. Using the \"Light\" theme will make the icons colored white and using the \"Dark\" theme will make the icons colored black.\n\n** This setting only applies to icons that appear on the Camera interface toolbar. **"),
                                    natPreferences.cameraIconsColorTheme);

            natPreferences.iconCameraClose = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Button Close",
                                    "This will be the icon that will represent the \"Close\" button on the NAT Camera. This icon will be tinted by the selected theme in \"Icons Color Theme\", no matter what the color of the image is."),
                                    natPreferences.iconCameraClose, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraClose != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraClose).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraClose = null;

            natPreferences.iconCameraFavicon = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Default Camera Favicon",
                        "This will be the icon that will represent the \"Default Favicon\" on the NAT Camera."),
                        natPreferences.iconCameraFavicon, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraFavicon != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraFavicon).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraFavicon = null;

            natPreferences.iconCameraLightOff = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Light Off",
                        "This icon represents the flash light on/off button on the device."),
                        natPreferences.iconCameraLightOff, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraLightOff != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOff).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraLightOff = null;

            natPreferences.iconCameraLightOn = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Light On",
                       "This icon represents the flash light on/off button on the device."),
                       natPreferences.iconCameraLightOn, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraLightOn != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOn).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraLightOn = null;

            natPreferences.iconCameraStartRec = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Start Rec",
                       "This icon represents the start recording button on NAT Camera."),
                       natPreferences.iconCameraStartRec, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraStartRec != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraStartRec).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraStartRec = null;

            natPreferences.iconCameraStopRec = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Stop Rec",
                       "This icon represents the stop recording button on NAT Camera."),
                       natPreferences.iconCameraStopRec, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraStopRec != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraStopRec).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraStopRec = null;

            natPreferences.iconCameraSwitch = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Switch",
                       "This icon represents the Switch Camera button."),
                       natPreferences.iconCameraSwitch, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraSwitch != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraSwitch).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraSwitch = null;

            natPreferences.iconCameraTakePhoto = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon Camera Take Photo",
                       "This icon represents the Take Photo button on NAT Camera."),
                       natPreferences.iconCameraTakePhoto, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconCameraTakePhoto != null && AssetDatabase.GetAssetPath(natPreferences.iconCameraTakePhoto).Contains("MTAssets NAT AAR") == true)
                natPreferences.iconCameraTakePhoto = null;

            EditorGUILayout.BeginHorizontal();
            natPreferences.cameraTitleColor = EditorGUILayout.ColorField(new GUIContent("Camera Title Color",
                        "This will be the color applied to the NAT Camera title text, which appears in the upper Toolbox."),
                        natPreferences.cameraTitleColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.cameraTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            natPreferences.cameraBackgroundColor = EditorGUILayout.ColorField(new GUIContent("Camera Back Color",
                                    "This is the background color of the top Toolbox bar."),
                                    natPreferences.cameraBackgroundColor);
            if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(18)))
                natPreferences.cameraBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);
            EditorGUILayout.EndHorizontal();
            if (natPreferences.iconCameraClose != null && natPreferences.iconCameraFavicon != null && natPreferences.iconCameraLightOff != null && natPreferences.iconCameraLightOn != null && natPreferences.iconCameraStartRec != null &&
                natPreferences.iconCameraStopRec != null && natPreferences.iconCameraSwitch != null && natPreferences.iconCameraTakePhoto != null)
            {
                bool isResValid = true;
                bool isExtValid = true;
                Vector2[] allIconsRes = new Vector2[8];
                string[] allIconsExt = new string[8];

                //Collect data
                allIconsRes[0] = new Vector2(natPreferences.iconCameraClose.width, natPreferences.iconCameraClose.height);
                allIconsRes[1] = new Vector2(natPreferences.iconCameraFavicon.width, natPreferences.iconCameraFavicon.height);
                allIconsRes[2] = new Vector2(natPreferences.iconCameraLightOff.width, natPreferences.iconCameraLightOff.height);
                allIconsRes[3] = new Vector2(natPreferences.iconCameraLightOn.width, natPreferences.iconCameraLightOn.height);
                allIconsRes[4] = new Vector2(natPreferences.iconCameraStartRec.width, natPreferences.iconCameraStartRec.height);
                allIconsRes[5] = new Vector2(natPreferences.iconCameraStopRec.width, natPreferences.iconCameraStopRec.height);
                allIconsRes[6] = new Vector2(natPreferences.iconCameraSwitch.width, natPreferences.iconCameraSwitch.height);
                allIconsRes[7] = new Vector2(natPreferences.iconCameraTakePhoto.width, natPreferences.iconCameraTakePhoto.height);
                allIconsExt[0] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraClose)).Replace(".", "");
                allIconsExt[1] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraFavicon)).Replace(".", "");
                allIconsExt[2] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOff)).Replace(".", "");
                allIconsExt[3] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOn)).Replace(".", "");
                allIconsExt[4] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraStartRec)).Replace(".", "");
                allIconsExt[5] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraStopRec)).Replace(".", "");
                allIconsExt[6] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraSwitch)).Replace(".", "");
                allIconsExt[7] = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconCameraTakePhoto)).Replace(".", "");

                //Verify resolution
                foreach (Vector2 resolution in allIconsRes)
                    if (resolution.x != resolution.y)
                        isResValid = false;
                //Verify extension
                foreach (string extension in allIconsExt)
                    if (extension != "png")
                        isExtValid = false;
                if (isResValid == false)
                    EditorGUILayout.HelpBox("Please provide icons that have transparency and that are square (must have the same width and height) for better icon quality.", MessageType.Error);
                if (isExtValid == false)
                    EditorGUILayout.HelpBox("Please provide only PNG icons with transparency for better icon quality.", MessageType.Error);

                natPreferences.cameraIconsIsValid = (isResValid == true && isExtValid == true) ? true : false;
            }
        }

        void UI_Topic_Applications(Texture packageNameIcon, Texture removeIcon)
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //Queries Visible Packages
            EditorGUILayout.LabelField("Queries Visible Packages", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            //If the manifest package queries is enabled, show the settings
            if (natPreferences.enableManifestPackageQueries == NativeAndroidPreferences.EnablePackageQueries.Yes)
            {
                EditorGUILayout.HelpBox("Below you can register the Package Names of apps that you would like your app to be able to interact with or query while your app runs on the user's Android device. The apps you register below can be interacted with or queried by your app when you use the APIs of the \"Applications\" class.\n\nOn devices with Android API 30 or higher, apps are unable to or interact with other apps installed on the user's device. This was a change imposed by Google to improve system security and reliability, so as of API 30, your app can only securely access and interact with a limited amount of apps that are installed on the user's device, and even therefore, the Package Names of these applications must be registered in your application's Manifest. By entering the Package Names of the applications that your app can interact with here, the Package Names will be automatically inserted into the NAT Core Manifest which will make it possible for you to access and interact with such applications using the APIs of the \"Applications\" class. See the Documentation for more details.", MessageType.Info);
                GUILayout.Space(10);
                int idOfItemToRemove = -1;
                foreach (string packageName in natPreferences.manifestQueriesVisiblePackages)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(packageNameIcon, iconStyle, GUILayout.Width(16), GUILayout.Height(20));
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Package: <b>" + packageName + "</b>", new GUIStyle { richText = true });
                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(6);
                    if (packageName == "com.google.market" || packageName == "com.android.vending")
                        if (GUILayout.Button(new GUIContent(removeIcon, ""), GUIStyle.none, GUILayout.Height(14), GUILayout.Width(14)))
                            EditorUtility.DisplayDialog("Error", "It is not possible to delete a predefined Package Name.", "Ok");
                    if (packageName != "com.google.market" && packageName != "com.android.vending")
                        if (GUILayout.Button(new GUIContent(removeIcon, "Click here to remove \"" + packageName + "\" Package Name from Manifest Pacakge Queries."), GUIStyle.none, GUILayout.Height(14), GUILayout.Width(14)))
                            for (int i = 0; i < natPreferences.manifestQueriesVisiblePackages.Count; i++)
                                if (natPreferences.manifestQueriesVisiblePackages[i] == packageName)
                                    if (EditorUtility.DisplayDialog("Remove Package Name?", "Your application will no longer be able to query, open or interact with the application that has this Package Name in Runtime, on the users device.\n\nAre you sure you want to remove Package Name \"" + packageName + "\"?", "Yes", "No") == true)
                                        idOfItemToRemove = i;
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(3);
                    EditorGUILayout.EndHorizontal();
                }
                if (idOfItemToRemove != -1)
                    natPreferences.manifestQueriesVisiblePackages.RemoveAt(idOfItemToRemove);
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                newPackageNameToAdd = EditorGUILayout.TextField(new GUIContent("",
                      "Type here the new Package Name to add."),
                      newPackageNameToAdd);
                if (string.IsNullOrEmpty(newPackageNameToAdd) == false)
                    if (newPackageNameToAdd.Contains(" ") == false && natPreferences.manifestQueriesVisiblePackages.Contains(newPackageNameToAdd, StringComparer.OrdinalIgnoreCase) == false && Regex.IsMatch(newPackageNameToAdd, @"^[a-zA-Z0-9._]*$") == true)
                        if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(50)))
                        {
                            natPreferences.manifestQueriesVisiblePackages.Add(newPackageNameToAdd);
                            newPackageNameToAdd = "";
                        }
                if (string.IsNullOrEmpty(newPackageNameToAdd) == true)
                    if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(50)))
                        EditorUtility.DisplayDialog("Error", "Please enter a valid Package Name before add.", "Ok");
                EditorGUILayout.EndHorizontal();
                //Validate the content
                if (string.IsNullOrEmpty(newPackageNameToAdd) == false)
                    if (newPackageNameToAdd.Contains(" ") == true || natPreferences.manifestQueriesVisiblePackages.Contains(newPackageNameToAdd, StringComparer.OrdinalIgnoreCase) == true || Regex.IsMatch(newPackageNameToAdd, @"^[a-zA-Z0-9._]*$") == false)
                        EditorGUILayout.HelpBox("Package Name written above is invalid. Reasons could be one of the reasons below...\n\n- Can not be empty.\n- It must contain only letters, numbers, underlines and dots.\n- It cannot contain spaces.\n- It shouldn't be a Package Name that already exists.\n\nReview the Package Name and make sure it meets these requirements.", MessageType.Error);
            }
            //If the manifest package queries is disabled, show the warning
            if (natPreferences.enableManifestPackageQueries == NativeAndroidPreferences.EnablePackageQueries.No)
                EditorGUILayout.HelpBox("In these settings you can define package names that will be visible to your application and your application will be able to interact with or consult these applications installed on the Android device. However, these settings will only become available if the following settings below are enabled.\n\nNAT Core Library -> Enable Package Queries -> Yes", MessageType.Error);
        }

        void UI_Topic_DateTime()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //NTP Servers settings
            EditorGUILayout.LabelField("NTP Servers To Use", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.manifestNtpServers[0] = EditorGUILayout.TextField(new GUIContent("NTP Server 1",
                "Enter your NTP Server #1 domain here. If time cannot be obtained from this server, the next server will be used."),
                natPreferences.manifestNtpServers[0]);

            natPreferences.manifestNtpServers[1] = EditorGUILayout.TextField(new GUIContent("NTP Server 2",
                "Enter your NTP Server #2 domain here. If time cannot be obtained from this server, the next server will be used."),
                natPreferences.manifestNtpServers[1]);

            natPreferences.manifestNtpServers[2] = EditorGUILayout.TextField(new GUIContent("NTP Server 3",
                "Enter your NTP Server #3 domain here. If time cannot be obtained from this server, the next server will be used."),
                natPreferences.manifestNtpServers[2]);

            natPreferences.manifestNtpServers[3] = EditorGUILayout.TextField(new GUIContent("NTP Server 4",
                "Enter your NTP Server #4 domain here. If time cannot be obtained from this server, the next server will be used."),
                natPreferences.manifestNtpServers[3]);

            natPreferences.manifestNtpServers[4] = EditorGUILayout.TextField(new GUIContent("NTP Server 5",
                "Enter your NTP Server #5 domain here."),
                natPreferences.manifestNtpServers[4]);

            //Validate the settings
            GUILayout.Space(10);
            natPreferences.ntpServersIsValid = true;
            foreach (string ntpServer in natPreferences.manifestNtpServers)
                if (string.IsNullOrEmpty(ntpServer) == true || ntpServer.Contains(" ") == true || Regex.IsMatch(ntpServer, @"^[a-zA-Z0-9.]*$") == false)
                {
                    EditorGUILayout.HelpBox("Some of the NTP servers are invalid. Please make sure that each of the NTP Servers configured above meets each of the prerequisites below.\n\n- It cannot contain spaces.\n- It can only contain letters, numbers and dots.\n- Cannot be empty.", MessageType.Error);
                    natPreferences.ntpServersIsValid = false;
                    break;
                }
        }

        void UI_Topic_Files()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Files Settings
            EditorGUILayout.LabelField("Files Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.filePickerBackgroundImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("File Picker Backg. Image",
                                                "This image will be applied as the background for the File Picker of the Files class."),
                                                natPreferences.filePickerBackgroundImage, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.filePickerBackgroundImage != null && AssetDatabase.GetAssetPath(natPreferences.filePickerBackgroundImage).Contains("MTAssets NAT AAR") == true)
                natPreferences.filePickerBackgroundImage = null;
            if (natPreferences.filePickerBackgroundImage != null)
            {
                bool isExtValid = true;
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.filePickerBackgroundImage)).Replace(".", "");
                if (extension != "png")
                {
                    EditorGUILayout.HelpBox("Please provide a PNG image to ensure the best image display.", MessageType.Error);
                    isExtValid = false;
                }
                natPreferences.filesResourcesIsValid = isExtValid;
            }
        }

        void UI_Topic_Tasks(Texture taskCodeContainer, Texture taskCodeIcon)
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //Tasks settings
            EditorGUILayout.LabelField("Tasks Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.delayBetweenStepsTask = EditorGUILayout.IntField(new GUIContent("Delay Between Steps",
                       "There is a delay between each function executed by a NAT Task. The delay allows a waiting time between each programmed function so that everything does not happen at once and more than one function accesses a single file for example. You can configure the delay here. Longer delays can guarantee greater stability, but will prolong the duration of the task, especially in tasks that contain a large number of functions.\n\nDelay time is measured in milliseconds."),
                       natPreferences.delayBetweenStepsTask);
            if (natPreferences.delayBetweenStepsTask < 500)
                natPreferences.delayBetweenStepsTask = 500;
            if (natPreferences.delayBetweenStepsTask > 1500)
                natPreferences.delayBetweenStepsTask = 1500;

            natPreferences.enableAccurateTask = EditorGUILayout.Toggle(new GUIContent("Enable Accurate Tasks",
                       "Some Android devices have their own battery savers. Many of these savers are aggressive to the point of killing background tasks from third-party apps (like MIUI from Xiaomi, Smart Manager from Samsung, etc.) and generally this prevents apps from running background tasks how to scheduled tasks. NAT does its best to deliver the tasks in the best way for the user and this option is one of those ways.\n\nIf this option is deactivated, the device will manage the execution of your app's tasks in its own way and with normal priority, according to the rules of the Android system and battery optimizers of each brand. Your task may even be delayed or never performed at all depending on how the device brand has modified the Android system to optimize it.\n\nBy enabling this option, NAT will use a different method to deliver tasks. This method will consume a little more battery (only when delivering the task) on the device (if it works on that device), but will try to deliver tasks as accurately as possible.\n\nThis option only works up to Android 11. On Android 12 or newer, this option has no effect, due to the new accuracy and task execution policies applied in this version of Android.\n\nPlease refer to the documentation for more details on tasks delivery accuracy, battery usage and Android system optimizations."),
                       natPreferences.enableAccurateTask);
            if (natPreferences.enableAccurateTask == true)
            {
                EditorGUI.indentLevel += 1;
                natPreferences.enableAccurateTaskForAndroid12OrNewer = EditorGUILayout.Toggle(new GUIContent("For Android 12+ Too",
                            "If this option is enabled, NAT will try to perform tasks more accurately on Android 12 and above devices as well. This option is designed for devices with Android 12 and above, since to perform accurate tasks on devices with Android 12 and above, some other methods are required, including using an additional permission.\n\nTo enable this option, you need to have the \"Schedule Exact Alarm\" permission enabled in the NAT Permission settings!"),
                            natPreferences.enableAccurateTaskForAndroid12OrNewer);
                if (natPreferences.scheduleExactAlarm == false)
                {
                    EditorGUILayout.HelpBox("In order for you to enable this option, the \"Schedule Exact Alarm\" permission must be enabled in the NAT Permission settings!", MessageType.Warning);
                    natPreferences.enableAccurateTaskForAndroid12OrNewer = false;
                }
                EditorGUI.indentLevel -= 1;
            }
            if (natPreferences.enableAccurateTask == false)
                natPreferences.enableAccurateTaskForAndroid12OrNewer = false;

            natPreferences.enableNotifyTaskProgress = EditorGUILayout.Toggle(new GUIContent("Enable Notify Progress",
                      "If this option is enabled, Native Android Toolkit will display a progress bar in device notifications that indicate progress towards task completion."),
                      natPreferences.enableNotifyTaskProgress);
            if (natPreferences.enableNotifyTaskProgress == true)
            {
                EditorGUI.indentLevel += 1;
                natPreferences.notifyTaskProgressTitle = EditorGUILayout.TextField(new GUIContent("Notification Title",
                            "The title of the task execution notification."),
                            natPreferences.notifyTaskProgressTitle);
                if (natPreferences.notifyTaskProgressTitle == "")
                    natPreferences.notifyTaskProgressTitle = "Running Task";
                EditorGUI.indentLevel -= 1;
            }
            if (natPreferences.enableNotifyTaskProgress == false)
                natPreferences.notifyTaskProgressTitle = "Running Task";

            natPreferences.logTaskCrashToFile = EditorGUILayout.Toggle(new GUIContent("Log Task Crash To File",
                      "If this option is enabled and a Java Exception or Crash occurs during the execution of a task, the Native Android Toolkit will save a file called \"last-crash.txt\" in the \"AppFiles\" scope in the \"AppFiles/NAT/Tasks\" directory that will contain information about the Crash or Exception so that you can fix this problem in your Task code or report it to MT Assets support.\n\n** It is highly recommended that you leave this option disabled in production as it may contain sensitive information from your code. **"),
                      natPreferences.logTaskCrashToFile);
            if (natPreferences.logTaskCrashToFile == true)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.HelpBox("It is recommended that this option is disabled in production builds like Google Play release for example.", MessageType.Warning);
                EditorGUI.indentLevel -= 1;
            }

            natPreferences.maxLoopIterationsInTask = EditorGUILayout.IntSlider(new GUIContent("Max Loop Iterations",
                       "This parameter defines the maximum number of times that iterations can happen in loops (like \"while\" or \"for\") within the Tasks code. The Native Android Toolkit limits the amount of iteration that loops can take, in order to impose limitations on tasks so that they do not consume large amounts of device resources."),
                       natPreferences.maxLoopIterationsInTask, 10, 50);

            //Tasks source code
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Task Source Code", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            EditorGUILayout.HelpBox("Here you can program your application's Task code. Tasks are a sequence of steps or functions that your application can execute even while it is closed. These Tasks and what they're supposed to do are programmed here and then activated through C# code in Runtime, and then they run at times scheduled by your application. You can program whatever you want, such as checking for updates, delivering push notifications, queries to a server, etc. Consult the NAT documentation for more details on how to use it and to solve your doubts!", MessageType.Info);
            GUILayout.Space(10);
            GUIStyle taskCodeContainerBox = new GUIStyle();
            taskCodeContainerBox.normal.background = (Texture2D)taskCodeContainer;
            EditorGUILayout.BeginVertical(taskCodeContainerBox);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(60));
            GUILayout.Space(3);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(6);
            GUILayout.Box(taskCodeIcon, iconStyle, GUILayout.Width(48), GUILayout.Height(48));
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(11);
            if (natPreferences.taskSourceCodeLines.Count == 0)
            {
                EditorGUILayout.LabelField("There is no source code!", new GUIStyle() { fontStyle = FontStyle.Bold }, GUILayout.Width(280));
                EditorGUILayout.LabelField("Click the button on the side to start programming!", GUILayout.Width(280));
            }
            if (natPreferences.taskSourceCodeLines.Count > 0)
            {
                //Count quantity of methods and components
                int methodsQuantity = 0;
                int componentsQuantity = 0;
                foreach (String line in natPreferences.taskSourceCodeLines)
                {
                    bool isMethod = line.Contains("-method=");
                    if (isMethod == false)
                        componentsQuantity += 1;
                    if (isMethod == true)
                        methodsQuantity += 1;
                }

                EditorGUILayout.LabelField("There is a Programmed Task Source Code", new GUIStyle() { fontStyle = FontStyle.Bold }, GUILayout.Width(280));
                EditorGUILayout.LabelField("The task has " + methodsQuantity + " methods and " + componentsQuantity + " components.", GUILayout.Width(280));
            }
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(6);
            if (natPreferences.taskSourceCodeLines.Count == 0)
                if (GUILayout.Button("Create", GUILayout.Height(48), GUILayout.Width(60)))
                    TaskCreator.OpenWindow();
            if (natPreferences.taskSourceCodeLines.Count > 0)
            {
                if (GUILayout.Button("Edit", GUILayout.Height(30), GUILayout.Width(60)))
                    TaskCreator.OpenWindow();
                if (GUILayout.Button("•••", GUILayout.Height(16), GUILayout.Width(60)))
                {
                    //Prepare the context menu
                    GenericMenu genericMenu = new GenericMenu();
                    //Add options
                    genericMenu.AddItem(new GUIContent("Export Source Code"), false, () => { TaskCreator.ExportSourceCode(natPreferences); });
                    genericMenu.AddItem(new GUIContent("Import Source Code"), false, () => { TaskCreator.ImportSourceCode(natPreferences); });
                    genericMenu.AddItem(new GUIContent("Reset Source Code"), false, () =>
                    {
                        if (EditorUtility.DisplayDialog("Reset Task Source Code?", "This action will erase all code currently programmed for Tasks. This action cannot be undone. Do you want to proceed with the reset?", "Yes", "No") == true)
                        {
                            natPreferences.taskSourceCodeLines.Clear();
                            natPreferences.taskSourceCodeLastSave = "";
                        }
                    });
                    //Render the context menu
                    genericMenu.ShowAsContext();
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(7);
            EditorGUILayout.EndHorizontal();
            //Show warning if is not saved
            if (natPreferences.taskSourceCodeLines.Count > 0 && TasksSourceCode.TASK_LAST_SAVE != natPreferences.taskSourceCodeLastSave)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8);
                EditorGUILayout.HelpBox("There have been changes in the Task Source Code and you haven't saved Native Android Toolkit preferences yet! You need to save preferences so that the modified Task code is available so you can enable it in Runtime!", MessageType.Error);
                GUILayout.Space(4);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            EditorGUILayout.EndVertical();
        }

        void UI_Topic_PlayGames()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Play Games Settings
            EditorGUILayout.LabelField("Play Games Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.playGamesAppId = EditorGUILayout.TextField(new GUIContent("Play Games App ID",
                                                    "You are required to provide your app's Google Play Games App ID for your app to be able to access these services, in order for the Play Games service to work in your game.\n\nIf you do not provide your App ID, NAT Play Games will not work, it will be disabled and you will not be able to use the NAT Google Play Games API."),
                                                    natPreferences.playGamesAppId);
            if (string.IsNullOrEmpty(natPreferences.playGamesAppId) == true)
                EditorGUILayout.HelpBox("NAT requires you to provide your Google Play Games App ID in order to be able to access Google Play Games services in your app. If you don't provide a App ID, NAT won't be able to access Google Play Games in your app, and your app may crash if you still try to use some code to access Google Play Games in your app. See the documentation to find out how to get the App ID.", MessageType.Warning);

            natPreferences.playGamesPackageName = EditorGUILayout.TextField(new GUIContent("Play Games Pack. Name",
                                                    "You need to provide the package name for your Google Play Games project. It is usually the same name as the package name you set here at Unity."),
                                                    natPreferences.playGamesPackageName);
            if (string.IsNullOrEmpty(natPreferences.playGamesPackageName) == true)
                EditorGUILayout.HelpBox("Please provide the package name for your Google Play Games project on Google Play Developer Console. If you do not provide, you can get errors when trying to use NAT Play Games.", MessageType.Warning);

            natPreferences.playGamesBackgroundImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Play Games Backg. Image",
                                                            "This image will be applied as the background for the Play Games class."),
                                                            natPreferences.playGamesBackgroundImage, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.playGamesBackgroundImage != null && AssetDatabase.GetAssetPath(natPreferences.playGamesBackgroundImage).Contains("MTAssets NAT AAR") == true)
                natPreferences.playGamesBackgroundImage = null;
            if (natPreferences.playGamesBackgroundImage != null)
            {
                bool isExtValid = true;
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.playGamesBackgroundImage)).Replace(".", "");
                if (extension != "png")
                {
                    EditorGUILayout.HelpBox("Please provide a PNG image to ensure the best image display.", MessageType.Error);
                    isExtValid = false;
                }
                natPreferences.playGamesResourcesIsValid = isExtValid;
            }

            //Play Games Resources
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Play Games Resources", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("If your app has Achievements, Events or Leaderboards configured on Google Play Console, then go to the console, click \"See Resources\", select \"Android (XML)\", copy everything and paste here! This is how NAT will make available the Achievements, Leaderboards and Scores for your app's C# code! Always keep this up to date!", MessageType.Info);
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            int playGamesXmlResourcesFieldWidth = 470;
            if (natPreferences.playGamesXmlResourcesSyntaxOk == false)
                playGamesXmlResourcesFieldWidth = 456;
            if (string.IsNullOrEmpty(natPreferences.playGamesAppId) == true || string.IsNullOrEmpty(natPreferences.playGamesPackageName) == true)
                playGamesXmlResourcesFieldWidth = 456;
            natPreferences.playGamesXmlResources = EditorGUILayout.TextArea(natPreferences.playGamesXmlResources, GUILayout.Height(200), GUILayout.Width(playGamesXmlResourcesFieldWidth));
            EditorGUILayout.EndHorizontal();
            if (natPreferences.playGamesXmlResources != "")
            {
                //Check if the syntax of the play games resources XML is valid
                try
                {
                    XElement xmlResources = XElement.Parse(natPreferences.playGamesXmlResources);
                    natPreferences.playGamesXmlResourcesSyntaxOk = true;
                }
                catch (System.Xml.XmlException e) { natPreferences.playGamesXmlResourcesSyntaxOk = false; Debug.LogError("NAT: Found XML syntax error on Play Games Resources\n\n" + e.StackTrace); }
            }
            if (natPreferences.playGamesXmlResources == "")
                natPreferences.playGamesXmlResourcesSyntaxOk = true;
            if (natPreferences.playGamesXmlResourcesSyntaxOk == false)
                EditorGUILayout.HelpBox("WARNING: Syntax Error detected on Play Games Resources XML! Please fix the XML syntax errors to Save Preferences.", MessageType.Error);
        }

        void UI_Topic_NATCoreLibrary()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            //Manifest settings
            EditorGUILayout.LabelField("NAT Core Android Manifest", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            natPreferences.enableManifestPackageQueries = (NativeAndroidPreferences.EnablePackageQueries)EditorGUILayout.EnumPopup(new GUIContent("Enable Packages Queries",
                        "Enabling this option will cause NAT to activate some Queries in your APK's Manifest. Queries are required for some functions of the Native Android Toolkit to work. Generally these functions work normally on APIs 29 or earlier, but due to some limitations imposed by android, on APIs 30 or later it is necessary that some Queries are enabled in your APK's Manifest in order for certain functions to work on APIs 30+, such as for example, the Speech To Text API.\n\n** Please note that this option is not compatible with versions older than Unity Editor 2020.3. You need to be using Unity Editor 2020.3 or higher to enable this option. This is because of some Gradle limitations of these Unity versions. **"),
                        natPreferences.enableManifestPackageQueries);

#if !UNITY_2020_3_OR_NEWER
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The option to enable Queries in Manifest of NAT Core is not supported in this version of Unity Editor due to some limitations with Gradle. You need to use Unity Editor version 2020.3 or higher to use this option.", MessageType.Warning);
            natPreferences.enableManifestPackageQueries = NativeAndroidPreferences.EnablePackageQueries.No;
#endif

            //About NAT Core
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Native Android Toolkit Core (NAT Core) Library", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            string unformattedCompilationDate = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.txt").Split(new string[] { ": " }, StringSplitOptions.None)[1];
            string formattedCompilationDate = unformattedCompilationDate.Split(' ')[0].Replace("-", "/");
            string formattedCompilationTime = unformattedCompilationDate.Split(' ')[1].Replace("-", ":").Replace(".", "");
            string currentNatCoreCompilationDate = formattedCompilationDate + " at " + formattedCompilationTime;
            EditorGUILayout.HelpBox("The Native Android Toolkit or NAT Core library is an AAR library that contains the Native Android code developed for the Native Android Toolkit. While CSharp codes run on the Game/Editor side, NAT Core runs on the native Android side and so the two work together so that all the functions of the Native Android Toolkit run smoothly.\n\nCurrent NAT Core compilation date is\n" + currentNatCoreCompilationDate, MessageType.Info);
        }

        void UI_Topic_AdditionalTools(Texture nadIcon, Texture ilvIcon)
        {
            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //Native Android Debugger
            EditorGUILayout.LabelField("Native Android Debugger", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(18);
            GUILayout.Box(nadIcon, iconStyle, GUILayout.Width(48), GUILayout.Height(48));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("If your Unity Editor was installed from Unity Hub, it must have a built-in ADB, which is responsible for communicating with your Android device to send your APKs and so on. Native Android Debugger is a tool that allows you to manage your Unity Editor's ADB, sending commands to it, to interact with your device, send APKs over Wi-Fi and so on.", MessageType.None);
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(18);
            //Check if the AndroidDebuggerTool exists
            bool natDebuggerFileExists = File.Exists("Assets/Plugins/MT Assets/Native Android Debugger/Editor/AndroidDebuggerTool.cs");
            //If tool is installed
            if (natDebuggerFileExists == true)
                if (GUILayout.Button("Open", GUILayout.Height(48), GUILayout.Width(80)))
                {
                    System.Reflection.Assembly editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Assembly-CSharp-Editor,")); //',' included to ignore  Assembly-CSharp-Editor-FirstPass
                    Type utilityType = editorAssembly.GetTypes().FirstOrDefault(t => t.FullName.Contains("MTAssets.NativeAndroidDebugger.Editor.AndroidDebuggerTool"));
                    utilityType.GetMethod("OpenWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: new object[] { });
                }
            //If tool is not installed
            if (natDebuggerFileExists == false)
                if (GUILayout.Button("GET", GUILayout.Height(48), GUILayout.Width(80)))
                    Help.BrowseURL("https://assetstore.unity.com/packages/tools/utilities/native-android-debugger-mt-wireless-debug-adb-manager-more-198639");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Ingame Logs Viewer
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Ingame Logs Viewer", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(12);
            GUILayout.Box(ilvIcon, iconStyle, GUILayout.Width(48), GUILayout.Height(48));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("Ingame Logs Viewer is a tool that implements a Console (similar to the Editor Console) within your game. This console is easily accessible and displays all the Logs your game emits and has a Stack Trace system as well. It's like you have an Android Logcat inside your game.", MessageType.None);
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(12);
            //Check if the AndroidDebuggerTool exists
            bool ilvDebuggerFileExists = File.Exists("Assets/Plugins/MT Assets/Ingame Logs Viewer/Scripts/IngameLogsViewer.cs");
            //If tool is installed
            if (ilvDebuggerFileExists == true)
                if (GUILayout.Button("Open", GUILayout.Height(48), GUILayout.Width(80)))
                    Help.BrowseURL("https://assetstore.unity.com/packages/tools/utilities/ingame-logs-viewer-mt-141657");
            //If tool is not installed
            if (ilvDebuggerFileExists == false)
                if (GUILayout.Button("GET", GUILayout.Height(48), GUILayout.Width(80)))
                    Help.BrowseURL("https://assetstore.unity.com/packages/tools/utilities/ingame-logs-viewer-mt-141657");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void SavePreferencesChanges()
        {
            //Validate the icon
            if (natPreferences.iconIsValid == false || natPreferences.webviewIconsIsValid == false || natPreferences.prwResourcesIsValid == false || natPreferences.utilsResourcesIsValid == false || natPreferences.mapsIconsIsValid == false || natPreferences.cameraIconsIsValid == false || natPreferences.filesResourcesIsValid == false || natPreferences.ntpServersIsValid == false || natPreferences.playGamesXmlResourcesSyntaxOk == false || natPreferences.playGamesResourcesIsValid == false)
            {
                EditorUtility.DisplayDialog("Invalid Resources or Settings", "Please review all resources related settings and check/review for possible errors involving the textures, images, icons or settings provided by you.", "Ok");
                return;
            }

            //Show progress dialog
            EditorUtility.DisplayProgressBar("A moment", "Saving Preferences...", 1f);

            //Save the preferences
            SaveThePreferences();

            //Crash protection
            bool errorsFounded = false;
            try
            {
                //------------- ANDROID MANIFEST MODIFICATIONS -------------//

                //If is desired to modify android manifest
                if (natPreferences.modifyAndroidManifest == NativeAndroidPreferences.ModifyManifest.YesGenerateNewIfNotExists)
                {
                    //If not exists the AndroidManifest.xml, create then
                    if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) == null)
                        ProjectAndroidManifest_GenerateProjectBaseAndroidManifest();

                    //Apply the declaration of UnityPlayerActivity
                    ProjectAndroidManifest_DeclareUnityPlayerActivity(natPreferences.declareUnityPlayerActivity, natPreferences.unityPlayerActivityIsMain);

                    //If skip permissions dialog is disabled
                    if (natPreferences.skipPermissionsDialog == false)
                        ProjectAndroidManifest_ChangeMetaDataValue("unityplayer.SkipPermissionsDialog", "false");
                    //If skip permissions dialog is enabled
                    if (natPreferences.skipPermissionsDialog == true)
                        ProjectAndroidManifest_ChangeMetaDataValue("unityplayer.SkipPermissionsDialog", "true");
                }

                //------------- GRADLE TEMPLATE PROPERTIES MODIFICATIONS -------------//

                //If is desired to modify gradle template properties
                if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.YesGenerateNewIfNotExists)
                {
                    //If not exists the gradleTemplate.properties, create then
                    if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/gradleTemplate.properties", typeof(object)) == null)
                        ProjectGradleTemplateProperties_GenerateProjectBaseGradleTemplateProperties();

                    //Apply all desired parameters
                    ProjectGradleTemplateProperties_ChangeParameter("android.enableDexingArtifactTransform", (natPreferences.enableDexingArtifactTransform == true) ? "true" : "false");
                }

                //------------- CORE AAR AND MANIFEST MODIFICATIONS -------------//

                //Extract the AAR
                string zipToUnpack = "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar";
                string unpackDirectory = "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted";
                using (IonicDotNetZip.Zip.ZipFile zipUnpack = IonicDotNetZip.Zip.ZipFile.Read(zipToUnpack))
                {
                    //Here, we extract every entry, but we could extract conditionally
                    //Based on entry name, size, date, checkbox status, etc.  
                    foreach (IonicDotNetZip.Zip.ZipEntry e in zipUnpack)
                        e.Extract(unpackDirectory, IonicDotNetZip.Zip.ExtractExistingFileAction.OverwriteSilently);
                }
                AssetDatabase.Refresh();

                //Replace the icons of AAR with the selected icons/resources
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/notifications_icon.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_back.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_close.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_error.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_forward.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_home.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_page_icon.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_refresh.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_one.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_two.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_close.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_icon.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_1.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_2.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_3.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_4.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_5.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_6.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_7.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_8.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_9.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_10.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_close.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_icon.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_light_off.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_light_on.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_start_recording.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_stop_recording.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_switch.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_take_photo.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_three.png");
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_four.png");
                AssetDatabase.Refresh();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconOfNotifications), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/notifications_icon.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewBack), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_back.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewClose), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_close.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewError), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_error.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewForward), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_forward.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewHome), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_home.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewFavicon), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_page_icon.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconWebviewRefresh), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/webview_refresh.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.prwBackgroundImage), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_one.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.restartBackgroundImage), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_two.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsClose), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_close.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsFavicon), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_icon.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker1), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_1.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker2), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_2.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker3), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_3.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker4), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_4.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker5), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_5.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker6), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_6.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker7), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_7.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker8), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_8.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker9), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_9.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconMapsMarker10), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/google_maps_marker_10.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraClose), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_close.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraFavicon), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_icon.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOff), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_light_off.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraLightOn), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_light_on.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraStartRec), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_start_recording.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraStopRec), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_stop_recording.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraSwitch), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_switch.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconCameraTakePhoto), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/camera_take_photo.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.filePickerBackgroundImage), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_three.png");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.playGamesBackgroundImage), "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/drawable/background_four.png");
                AssetDatabase.Refresh();

                //Apply interface colors values and data on values file of AAR
                AARPluginValues_ChangeWebviewColorsValues(natPreferences.iconsColorTheme, natPreferences.webviewBackgroundColor, natPreferences.webviewTitleColor);
                AARPluginValues_ChangePermissionsRequesterWizardColorsValues(natPreferences.prwBackgroundColor, natPreferences.prwTitleColor);
                AARPluginValues_ChangeNotificationsColorsValues(natPreferences.interactReceiverColor, natPreferences.colorInNotifyArea);
                AARPluginValues_ChangeGoogleMapsColorsValues(natPreferences.mapsIconsColorTheme, natPreferences.mapsBackgroundColor, natPreferences.mapsTitleColor);
                AARPluginValues_ChangeCameraColorsValues(natPreferences.cameraIconsColorTheme, natPreferences.cameraBackgroundColor, natPreferences.cameraTitleColor);
                AARPluginValues_ChangePlayGamesMetadataInformations(natPreferences.playGamesAppId, natPreferences.playGamesPackageName);

                //Add or remove desired permissions of AAR Manifest
                AARPluginAndroidManifest_AddOrRemovePermission("VIBRATE", natPreferences.vibratePermission);
                AARPluginAndroidManifest_AddOrRemovePermission("ACCESS_WIFI_STATE", natPreferences.accessWifiStatePermission);
                AARPluginAndroidManifest_AddOrRemovePermission("ACCESS_NETWORK_STATE", natPreferences.accessNetworkStatePermission);
                AARPluginAndroidManifest_AddOrRemovePermission("ACCESS_COARSE_LOCATION", natPreferences.accessCoarseLocation);
                AARPluginAndroidManifest_AddOrRemovePermission("ACCESS_FINE_LOCATION", natPreferences.accessFineLocation);
                AARPluginAndroidManifest_AddOrRemovePermission("CAMERA", natPreferences.camera);
                AARPluginAndroidManifest_AddOrRemovePermission("RECORD_AUDIO", natPreferences.recordAudio);
                AARPluginAndroidManifest_AddOrRemovePermission("QUERY_ALL_PACKAGES", natPreferences.queryAllPackages);
                AARPluginAndroidManifest_AddOrRemovePermission("READ_EXTERNAL_STORAGE", natPreferences.accessFilesAndMedia);
                AARPluginAndroidManifest_AddOrRemovePermission("WRITE_EXTERNAL_STORAGE", natPreferences.accessFilesAndMedia);
                AARPluginAndroidManifest_AddOrRemovePermission("FOREGROUND_SERVICE", natPreferences.foregroundService);
                AARPluginAndroidManifest_AddOrRemovePermission("SCHEDULE_EXACT_ALARM", natPreferences.scheduleExactAlarm);

                //Change all desired meta-data of AAR Manifest
                AARPluginAndroidManifest_ChangeAarMetaDataValue("enableAccurateNotify", (natPreferences.enableAccurateNotify == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("enableAccurateNotifyForAndroid12OrNewer", (natPreferences.enableAccurateNotifyForAndroid12OrNewer == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("forceVibrateOnNotify", (natPreferences.forceVibrateOnNotify == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("com.google.android.geo.API_KEY", natPreferences.googleMapsApiKey);
                AARPluginAndroidManifest_ChangeAarMetaDataValue("ntpServers", natPreferences.manifestNtpServers[0] + "!!" + natPreferences.manifestNtpServers[1] + "!!" + natPreferences.manifestNtpServers[2] + "!!" + natPreferences.manifestNtpServers[3] + "!!" + natPreferences.manifestNtpServers[4]);
                AARPluginAndroidManifest_ChangeAarMetaDataValue("delayBetweenStepsTask", natPreferences.delayBetweenStepsTask.ToString());
                AARPluginAndroidManifest_ChangeAarMetaDataValue("enableAccurateTask", (natPreferences.enableAccurateTask == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("enableAccurateTaskForAndroid12OrNewer", (natPreferences.enableAccurateTaskForAndroid12OrNewer == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("enableNotifyTaskProgress", (natPreferences.enableNotifyTaskProgress == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("notifyTaskProgressTitle", natPreferences.notifyTaskProgressTitle);
                AARPluginAndroidManifest_ChangeAarMetaDataValue("logTaskCrashToFile", (natPreferences.logTaskCrashToFile == true) ? "true" : "false");
                AARPluginAndroidManifest_ChangeAarMetaDataValue("maxLoopIterationsInTask", natPreferences.maxLoopIterationsInTask.ToString());
                AARPluginAndroidManifest_EnableOrDisablePackagesQueries(natPreferences.enableManifestPackageQueries);

                //Run updates in scripts processed by preferences in Editor
                UpdateNotificationsActionsAvailabilityForCSharp();
                UpdateCameraInitializationScriptForCSharp();
                UpdateTasksSourceCodeScriptForCSharp();
                UpdatePlayGamesAchievementsLeaderboardsAndEventsAvailabilityForCSharp();

                errorsFounded = false;
            }
            catch (Exception e)
            {
                //Show the warning
                Debug.LogError("An error occurred while saving preferences, so saving was interrupted. Click here for more information.\n\n" + e);
                errorsFounded = true;
            }

            //If the protection not found errors
            if (errorsFounded == false)
            {
                //Delete the original AAR
                AssetDatabase.DeleteAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar");
                AssetDatabase.Refresh();

                //Zip the Extracted folder into a new AAR again
                DirectoryInfo dirToCompact = new DirectoryInfo("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted");
                var ext = new List<string> { ".jpg", ".gif", ".png", ".xml", ".jar", ".txt", ".properties", ".wav" };
                var filePaths = Directory.GetFiles(dirToCompact.ToString(), "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s)));
                IonicDotNetZip.Zip.ZipFile zipPack = new IonicDotNetZip.Zip.ZipFile("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar");
                zipPack.CompressionLevel = IonicDotNetZip.Zlib.CompressionLevel.BestCompression;
                foreach (string filePath in filePaths)
                {
                    FileAttributes fileAttributes = File.GetAttributes(filePath);
                    if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        zipPack.AddDirectory(filePath, Path.GetDirectoryName(filePath).Replace("Assets\\Plugins\\MT Assets\\Native Android Toolkit\\Libraries\\MTAssets NAT AAR\\Extracted", ""));
                    }
                    else
                    {
                        zipPack.AddFile(filePath, Path.GetDirectoryName(filePath).Replace("Assets\\Plugins\\MT Assets\\Native Android Toolkit\\Libraries\\MTAssets NAT AAR\\Extracted", ""));
                    }
                }
                zipPack.AddDirectoryByName("anim");
                zipPack.AddDirectoryByName("drawable");
                zipPack.AddDirectoryByName("layout");
                zipPack.AddDirectoryByName("raw");
                zipPack.AddDirectoryByName("values");
                zipPack.AddDirectoryByName("xml");
                zipPack.Save();
                AssetDatabase.Refresh();

                //Apply the last modification of NAT Core.aar to the file
                string lastModifyDate = File.GetLastWriteTimeUtc("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar").ToString();
                File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastModifyDate.ini", lastModifyDate);

                //Refresh assets
                AssetDatabase.Refresh();

                //Hide progress dialog
                EditorUtility.ClearProgressBar();

                //Show warn
                EditorUtility.DisplayDialog("Done", "All your preferences have been successfully applied to Native Android Toolkit and NAT Core.", "Ok");
            }
            if (errorsFounded == true)
            {
                //Refresh assets
                AssetDatabase.Refresh();

                //Hide progress dialog
                EditorUtility.ClearProgressBar();

                //Show warn
                EditorUtility.DisplayDialog("Error", "Errors occurred while saving preferences, please check your Unity console for more information.\n\nIf you are experiencing errors frequently, please try to update your Native Android Toolkit, re-install it in your project.\n\nIf you are still having problems, contact us at mtassets@windsoft.xyz", "Ok");
            }
        }

        public void ProjectAndroidManifest_GenerateProjectBaseAndroidManifest()
        {
            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/Plugins"))
                AssetDatabase.CreateFolder("Assets", "Plugins");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/Android"))
                AssetDatabase.CreateFolder("Assets/Plugins", "Android");

            //Load the AndroidManifestBase text
            string androidManifestBaseStr = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT XML/AndroidManifestBase.xml");

            //Insert the current date
            DateTime dateNow = DateTime.Now;
            string newAndroidManifestBaseStr = androidManifestBaseStr.Replace("%DATE%", dateNow.Year + "/" + dateNow.Month + "/" + dateNow.Day + "-" + dateNow.Hour + ":" + dateNow.Minute + ":" + dateNow.Second);

            //Write the new AndroidManifest
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", newAndroidManifestBaseStr);
        }

        public void ProjectAndroidManifest_DeclareUnityPlayerActivity(bool declare, bool isMain)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //If is desired to not declare
            if (declare == false)
                foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList()) //<- Get all nodes of activity
                    node.Remove();

            //If is desired to declare
            if (declare == true)
            {
                //Count quantity of activities declared that exists, where attributes is equal to informed
                var nodes = xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList();

                //If exists more than one activity declaration with that attributes, delete all to create only one
                if (nodes.Count > 1)
                {
                    foreach (var node in nodes)
                        node.Remove();
                    nodes.Clear();
                }

                //If not exists the declaration of UnityPlayerActivity, create then
                if (nodes.Count == 0)
                {
                    var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                    manifest.Add(new XElement("activity", new XAttribute(xmlAndroidNs + "name", "com.unity3d.player.UnityPlayerActivity"), new XAttribute(xmlAndroidNs + "label", "@string/app_name"), new XAttribute(xmlAndroidNs + "exported", "true")));
                    foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                    {
                        XElement action = new XElement("action", new XAttribute(xmlAndroidNs + "name", "android.intent.action.MAIN"));
                        XElement category = new XElement("category", new XAttribute(xmlAndroidNs + "name", "android.intent.category.LAUNCHER"));
                        node.Add(new XElement("intent-filter", action, category));
                        node.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", "unityplayer.UnityActivity"), new XAttribute(xmlAndroidNs + "value", "true")));
                        node.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", "unityplayer.ForwardNativeEventsToDalvik"), new XAttribute(xmlAndroidNs + "value", "true")));
                    }
                }

                //Delete the intent filter
                foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                    node.Descendants("intent-filter").Remove();

                //If is desired to set UnityPlayer Activity as Main
                if (isMain == true)
                    foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                    {
                        XElement action = new XElement("action", new XAttribute(xmlAndroidNs + "name", "android.intent.action.MAIN"));
                        XElement category = new XElement("category", new XAttribute(xmlAndroidNs + "name", "android.intent.category.LAUNCHER"));
                        node.Add(new XElement("intent-filter", action, category));
                    }
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", stringResult);
        }

        public void ProjectAndroidManifest_ChangeMetaDataValue(string name, string value)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Count quantity of meta-data that exists, where attributes is equal to informed
            var nodes = xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList();

            //If exists more than one meta-data with that attributes, delete all to create only one
            if (nodes.Count > 1)
            {
                foreach (var node in nodes)
                    node.Remove();
                nodes.Clear();
            }

            //If not exists a meta-data with that attributes, create then
            if (nodes.Count == 0)
            {
                var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                manifest.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", name), new XAttribute(xmlAndroidNs + "value", value)));
            }

            //Get all nodes of meta-data
            foreach (var node in xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList())
                node.Attribute(xmlAndroidNs + "value").SetValue(value);

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", stringResult);
        }

        public void ProjectGradleTemplateProperties_GenerateProjectBaseGradleTemplateProperties()
        {
            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/Plugins"))
                AssetDatabase.CreateFolder("Assets", "Plugins");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/Android"))
                AssetDatabase.CreateFolder("Assets/Plugins", "Android");

            //Load the GradleTemplateBase text
            string gradleTemplateBaseStr = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT XML/GradleTemplateBase.properties");
            string gradleTemplateBaseStrFixedByUnityVersion = gradleTemplateBaseStr;

            //If is unity 2019.X, fix to Android Gradle Plugin 3.4.0
#if UNITY_2019_3 || UNITY_2019_4
            gradleTemplateBaseStrFixedByUnityVersion = gradleTemplateBaseStr.Replace("android.enableR8=**MINIFY_WITH_R_EIGHT**", "#android.enableR8=**MINIFY_WITH_R_EIGHT**")
                                                                            .Replace("unityStreamingAssets=.unity3d**STREAMING_ASSETS**", "#unityStreamingAssets=.unity3d**STREAMING_ASSETS**");
#endif
            //If is unity 2020.1, fix to Android Gradle Plugin 3.6.0
#if UNITY_2020_1
            gradleTemplateBaseStrFixedByUnityVersion = gradleTemplateBaseStr.Replace("unityStreamingAssets=.unity3d**STREAMING_ASSETS**", "#unityStreamingAssets=.unity3d**STREAMING_ASSETS**");
#endif
            //If is unity 2020.2+, fix to Android Gradle Plugin 4.0.1+
#if UNITY_2020_2_OR_NEWER
            gradleTemplateBaseStrFixedByUnityVersion = gradleTemplateBaseStr;
#endif

            //Write the new Gradle template properties
            File.WriteAllText("Assets/Plugins/Android/gradleTemplate.properties", gradleTemplateBaseStrFixedByUnityVersion);
        }

        public void ProjectGradleTemplateProperties_ChangeParameter(string parameterName, string value)
        {
            //Load gradle template properties file content
            string[] gradleTemplatePropertiesStr = File.ReadAllLines("Assets/Plugins/Android/gradleTemplate.properties");

            //Store a variable to inform that is finded the parameter or not
            List<string> modifiedLinesOfGradleTemplateProperties = new List<string>();
            bool wasFindedTheParameterInFile = false;
            //Remove all lines that this parameter is present and add this paramter again with the desired new value
            for (int i = 0; i < gradleTemplatePropertiesStr.Length; i++)
            {
                if (gradleTemplatePropertiesStr[i].Contains(parameterName) == true)
                {
                    gradleTemplatePropertiesStr[i] = "";
                    gradleTemplatePropertiesStr[i] = parameterName + "=" + value;
                    wasFindedTheParameterInFile = true;
                }

                //Add this line in list
                modifiedLinesOfGradleTemplateProperties.Add(gradleTemplatePropertiesStr[i]);
            }

            //If not found a line with this paramter, create a new one
            if (wasFindedTheParameterInFile == false)
            {
                modifiedLinesOfGradleTemplateProperties.Add("");
                modifiedLinesOfGradleTemplateProperties.Add(parameterName + "=" + value);
            }

            //Write the modified gradle template properties
            File.WriteAllLines("Assets/Plugins/Android/gradleTemplate.properties", modifiedLinesOfGradleTemplateProperties.ToArray());
        }

        public void AARPluginAndroidManifest_AddOrRemovePermission(string permissionName, bool enabled)
        {
            //Load the AndroidManifest of AAR
            string manifestXml = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Remove or add the desired Permission, according to option
            if (enabled == true)
            {
                //Delete all nodes of desired permission found, to reset
                foreach (var node in xmlDoc.Descendants("uses-permission").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "android.permission." + permissionName).ToList())
                    node.Remove();

                //Get the uses-sdk node to add the permission after this
                var internetPermissionNode = xmlDoc.Descendants("uses-sdk").ToList();

                //Prepare the node of permission
                XElement permissionNode = new XElement("uses-permission", new XAttribute(xmlAndroidNs + "name", "android.permission." + permissionName));
                //If this node is the WRITE_EXTERNAL_STORAGE permission, modify the permission node to add the attribute maxSdkVersion with value 29
                if (permissionName == "WRITE_EXTERNAL_STORAGE")
                    permissionNode = new XElement("uses-permission", new XAttribute(xmlAndroidNs + "name", "android.permission." + permissionName), new XAttribute(xmlAndroidNs + "maxSdkVersion", "29"));

                //Add the node of desired permission after uses-sdk node
                internetPermissionNode[0].AddAfterSelf(permissionNode);
            }
            if (enabled == false)
                foreach (var node in xmlDoc.Descendants("uses-permission").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "android.permission." + permissionName).ToList()) //<- Delete all desired permission
                    node.Remove();

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the modified new AndroidManifest in AAR to compress
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml", stringResult);
        }

        public void AARPluginAndroidManifest_ChangeAarMetaDataValue(string name, string value)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Count quantity of meta-data that exists, where attributes is equal to informed
            var nodes = xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList();

            //If exists more than one meta-data with that attributes, delete all to create only one
            if (nodes.Count > 1)
            {
                foreach (var node in nodes)
                    node.Remove();
                nodes.Clear();
            }

            //If not exists a meta-data with that attributes, create then
            if (nodes.Count == 0)
            {
                var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                manifest.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", name), new XAttribute(xmlAndroidNs + "value", value)));
            }

            //Get all nodes of meta-data
            foreach (var node in xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList())
                node.Attribute(xmlAndroidNs + "value").SetValue(value);

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml", stringResult);
        }

        public void AARPluginValues_ChangePermissionsRequesterWizardColorsValues(Color backgroundColor, Color titleColor)
        {
            //Modify the file values.xml of AAR to modify colors of Permissions Requester
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new colors on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //If is backgroundColor
                if (valueNode.Attributes[0].Value == "permissions_requester_wizard_toolbar_background")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(backgroundColor);

                //If is titleColor
                if (valueNode.Attributes[0].Value == "permissions_requester_wizard_toolbar_title")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(titleColor);
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void AARPluginValues_ChangeNotificationsColorsValues(Color notificationInteractReceiver, Color colorInNotifyArea)
        {
            //Modify the file values.xml of AAR to modify colors of Notifications Colors
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new colors on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //If is notificationInteractReceiver
                if (valueNode.Attributes[0].Value == "notification_interact_receiver_color")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(notificationInteractReceiver);

                //If is colorInNotifyArea
                if (valueNode.Attributes[0].Value == "notification_color")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(colorInNotifyArea);
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void AARPluginValues_ChangeWebviewColorsValues(NativeAndroidPreferences.WebviewIconsColorTheme iconsTheme, Color backgroundColor, Color titleColor)
        {
            //Modify the file values.xml of AAR to modify colors of Webview
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new colors on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //If is desired light theme
                if (iconsTheme == NativeAndroidPreferences.WebviewIconsColorTheme.Light)
                {
                    if (valueNode.Attributes[0].Value == "webview_inside_webview_icons_tint")
                        valueNode.InnerText = "#454545";
                    if (valueNode.Attributes[0].Value == "webview_outside_webview_icons_tint")
                        valueNode.InnerText = "#FFFFFF";
                }
                //If is desired dark theme
                if (iconsTheme == NativeAndroidPreferences.WebviewIconsColorTheme.Dark)
                {
                    if (valueNode.Attributes[0].Value == "webview_inside_webview_icons_tint")
                        valueNode.InnerText = "#454545";
                    if (valueNode.Attributes[0].Value == "webview_outside_webview_icons_tint")
                        valueNode.InnerText = "#000000";
                }

                //If is backgroundColor
                if (valueNode.Attributes[0].Value == "webview_toolbar_background")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(backgroundColor);

                //If is titleColor
                if (valueNode.Attributes[0].Value == "webview_toolbar_title")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(titleColor);
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void AARPluginValues_ChangeGoogleMapsColorsValues(NativeAndroidPreferences.MapsIconsColorTheme iconsTheme, Color backgroundColor, Color titleColor)
        {
            //Modify the file values.xml of AAR to modify colors of Webview
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new colors on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //If is desired light theme
                if (iconsTheme == NativeAndroidPreferences.MapsIconsColorTheme.Light)
                    if (valueNode.Attributes[0].Value == "google_maps_toolbar_icons_tint")
                        valueNode.InnerText = "#FFFFFF";
                //If is desired dark theme
                if (iconsTheme == NativeAndroidPreferences.MapsIconsColorTheme.Dark)
                    if (valueNode.Attributes[0].Value == "google_maps_toolbar_icons_tint")
                        valueNode.InnerText = "#000000";

                //If is backgroundColor
                if (valueNode.Attributes[0].Value == "google_maps_toolbar_background")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(backgroundColor);

                //If is titleColor
                if (valueNode.Attributes[0].Value == "google_maps_toolbar_title")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(titleColor);
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void AARPluginValues_ChangeCameraColorsValues(NativeAndroidPreferences.CameraIconsColorTheme iconsTheme, Color backgroundColor, Color titleColor)
        {
            //Modify the file values.xml of AAR to modify colors of Webview
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new colors on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //If is desired light theme
                if (iconsTheme == NativeAndroidPreferences.CameraIconsColorTheme.Light)
                    if (valueNode.Attributes[0].Value == "camera_toolbar_icons_tint")
                        valueNode.InnerText = "#FFFFFF";
                //If is desired dark theme
                if (iconsTheme == NativeAndroidPreferences.CameraIconsColorTheme.Dark)
                    if (valueNode.Attributes[0].Value == "camera_toolbar_icons_tint")
                        valueNode.InnerText = "#000000";

                //If is backgroundColor
                if (valueNode.Attributes[0].Value == "camera_toolbar_background")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(backgroundColor);

                //If is titleColor
                if (valueNode.Attributes[0].Value == "camera_toolbar_title")
                    valueNode.InnerText = "#" + ColorUtility.ToHtmlStringRGB(titleColor);
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void AARPluginValues_ChangePlayGamesMetadataInformations(string playGamesAppId, string playGamesPackageName)
        {
            //Modify the file values.xml of AAR to apply metadata of play games
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //Load the values.xml
            xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
            //Apply the new metadata on each xml node
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("//resources");
            var allValuesNodes = node.ChildNodes;
            foreach (System.Xml.XmlNode valueNode in allValuesNodes)
            {
                //Apply App ID
                if (valueNode.Attributes[0].Value == "app_id")
                    valueNode.InnerText = (playGamesAppId != "") ? playGamesAppId : "0";
                //Apply App Package Name
                if (valueNode.Attributes[0].Value == "package_name")
                    valueNode.InnerText = (playGamesPackageName != "") ? playGamesPackageName : "domain.appName";
            }
            //Save the modified new values.xml
            xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/res/values/values.xml");
        }

        public void UpdateNotificationsActionsAvailabilityForCSharp()
        {
            //Refresh the file NotificationsActions.cs with all Notifications Actions registered
            string newNotificationsActionsEnum = "";
            for (int i = 0; i < natPreferences.notificationsActions.Count; i++)
            {
                if (i > 0)
                    newNotificationsActionsEnum += ", ";
                newNotificationsActionsEnum += natPreferences.notificationsActions[i].ToString();
            }
            string notificationsActionsCode = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/NotificationsActions.txt").Replace("%ActionsList%", newNotificationsActionsEnum);
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/NotificationsActions.cs", notificationsActionsCode);
        }

        public void UpdateCameraInitializationScriptForCSharp()
        {
            //Calculate the isSafeToInitializeCamera
            string isSafeToInitializeCamera = "";
            if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.No)
                isSafeToInitializeCamera = "false";
            if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.YesGenerateNewIfNotExists && natPreferences.enableDexingArtifactTransform == true)
                isSafeToInitializeCamera = "false";
            if (natPreferences.modifyGradleProperties == NativeAndroidPreferences.ModifyGradleProperties.YesGenerateNewIfNotExists && natPreferences.enableDexingArtifactTransform == false)
                isSafeToInitializeCamera = "true";

            //Refresh the file CameraInitialization.cs with all preferences about camera initialization
            string cameraInitializationCode = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/CameraInitialization.txt")
                                                  .Replace("%isSafeToInitializeCamera%", isSafeToInitializeCamera)
                                                  .Replace("%productNameThatGeneratedThisScript%", "\"" + Application.productName + "\"");
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/CameraInitialization.cs", cameraInitializationCode);
        }

        public void UpdateTasksSourceCodeScriptForCSharp()
        {
            //Compile the new current TaskSourceCode
            StringBuilder taskSourceCodeCompiled = new StringBuilder();
            bool isFirstLine = true;
            foreach (string line in natPreferences.taskSourceCodeLines)
            {
                if (isFirstLine == false)
                    taskSourceCodeCompiled.Append("§");
                taskSourceCodeCompiled.Append(line.Replace("\"", "\\\""));
                isFirstLine = false;
            }

            //Refresh the file TasksSourceCode.cs with the current code created in Tasks
            string tasksSourceCodeBase = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/TasksSourceCode.txt")
                                             .Replace("%TaskSourceCode%", taskSourceCodeCompiled.ToString())
                                             .Replace("%TaskLastSave%", natPreferences.taskSourceCodeLastSave);
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/TasksSourceCode.cs", tasksSourceCodeBase);
        }

        public void UpdatePlayGamesAchievementsLeaderboardsAndEventsAvailabilityForCSharp()
        {
            //If not have XML play games resources or syntax is bad, cancel
            if (natPreferences.playGamesXmlResources == "" || natPreferences.playGamesXmlResourcesSyntaxOk == false)
            {
                //Read the play games resources text file to get base C# code reseted
                string playGamesResourcesBaseCodeReseted = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/PlayGamesResources.txt")
                                                        .Replace("%PLAY_GAMES_ACHIEVEMENTS_NAMES%", "").Replace("%PLAY_GAMES_ACHIEVEMENTS_IDs%", "")
                                                        .Replace("%PLAY_GAMES_LEADERBOARDS_NAMES%", "").Replace("%PLAY_GAMES_LEADERBOARDS_IDs%", "")
                                                        .Replace("%PLAY_GAMES_EVENTS_NAMES%", "").Replace("%PLAY_GAMES_EVENTS_IDs%", "");
                //Save the play games resources reseted C# code
                File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/PlayGamesResources.cs", playGamesResourcesBaseCodeReseted);
                return;
            }

            //Prepare the list of items
            Dictionary<string, string> achievementsList = new Dictionary<string, string>();
            Dictionary<string, string> leaderboardsList = new Dictionary<string, string>();
            Dictionary<string, string> eventsList = new Dictionary<string, string>();

            //Read xml of resources to build a list of items
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(natPreferences.playGamesXmlResources);
            System.Xml.XmlNode rootNode = xmlDoc.SelectSingleNode("//resources");
            var allChildNodes = rootNode.ChildNodes;
            foreach (System.Xml.XmlNode node in allChildNodes)
            {
                //If is comment, ignore
                if (node.Name == "#comment")
                    continue;

                //If is achievement node
                if (node.Attributes[0].Value.Split('_')[0].Equals("achievement") == true)
                {
                    //Get achievement pure name
                    string achievName = node.Attributes[0].Value.Split(new[] { '_' }, 2)[1];
                    if ("0123456789".Contains(achievName[0]) == true)   //<- If have a number as first character, add a underline to avoid compile errors in C#
                        achievName = "_" + achievName;

                    //Add to list
                    if (achievementsList.ContainsKey(achievName) == false)
                        achievementsList.Add(achievName, node.InnerText);
                }
                //If is leaderboard node
                if (node.Attributes[0].Value.Split('_')[0].Equals("leaderboard") == true)
                {
                    //Get leaderboard pure name
                    string boardName = node.Attributes[0].Value.Split(new[] { '_' }, 2)[1];
                    if ("0123456789".Contains(boardName[0]) == true)   //<- If have a number as first character, add a underline to avoid compile errors in C#
                        boardName = "_" + boardName;

                    //Add to list
                    if (leaderboardsList.ContainsKey(boardName) == false)
                        leaderboardsList.Add(boardName, node.InnerText);
                }
                //If is event node
                if (node.Attributes[0].Value.Split('_')[0].Equals("event") == true)
                {
                    //Get event pure name
                    string eventName = node.Attributes[0].Value.Split(new[] { '_' }, 2)[1];
                    if ("0123456789".Contains(eventName[0]) == true)   //<- If have a number as first character, add a underline to avoid compile errors in C#
                        eventName = "_" + eventName;

                    //Add to list
                    if (eventsList.ContainsKey(eventName) == false)
                        eventsList.Add(eventName, node.InnerText);
                }
            }

            //Prepare all achievements to C# code
            string achievementsNames = "";
            string achievementsIds = "";
            foreach (var item in achievementsList)
            {
                achievementsNames += ", " + item.Key;
                achievementsIds += ", \"" + item.Value + "\"";
            }
            //Prepare all leaderboards to C# code
            string leaderboardsNames = "";
            string leaderboardsIds = "";
            foreach (var item in leaderboardsList)
            {
                leaderboardsNames += ", " + item.Key;
                leaderboardsIds += ", \"" + item.Value + "\"";
            }
            //Prepare all events to C# code
            string eventsNames = "";
            string eventsIds = "";
            foreach (var item in eventsList)
            {
                eventsNames += ", " + item.Key;
                eventsIds += ", \"" + item.Value + "\"";
            }

            //Read the play games resources text file to get base C# code
            string playGamesResourcesBaseCode = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/PlayGamesResources.txt")
                                                    .Replace("%PLAY_GAMES_ACHIEVEMENTS_NAMES%", achievementsNames).Replace("%PLAY_GAMES_ACHIEVEMENTS_IDs%", achievementsIds)
                                                    .Replace("%PLAY_GAMES_LEADERBOARDS_NAMES%", leaderboardsNames).Replace("%PLAY_GAMES_LEADERBOARDS_IDs%", leaderboardsIds)
                                                    .Replace("%PLAY_GAMES_EVENTS_NAMES%", eventsNames).Replace("%PLAY_GAMES_EVENTS_IDs%", eventsIds);
            //Save the play games resources modified C# code
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Scripts/ProcessedByPreferencesInEditor/PlayGamesResources.cs", playGamesResourcesBaseCode);
        }

        public void AARPluginAndroidManifest_EnableOrDisablePackagesQueries(NativeAndroidPreferences.EnablePackageQueries enablePackageQueries)
        {
            //If is desired to enable the Queries
            if (enablePackageQueries == NativeAndroidPreferences.EnablePackageQueries.Yes)
            {
                //Load the AndroidManifest of AAR
                string[] manifestLines = File.ReadAllLines("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

                //Check each line to find the QUERIES line
                for (int i = 0; i < manifestLines.Length; i++)
                    if (manifestLines[i].Contains("<queries>") == true && manifestLines[i].Contains("</queries>") == true)
                        if (manifestLines[i].Contains("<!-- <queries>") == true && manifestLines[i].Contains("</queries> -->") == true)
                            manifestLines[i] = manifestLines[i].Replace("<!-- <queries>", "<!-- %SQUERIES% -->\n<queries>").Replace("</queries> -->", "</queries>\n<!-- %EQUERIES% -->");

                //Save the modified new AndroidManifest in AAR to compress
                File.WriteAllLines("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml", manifestLines);

                //Apply the Package Names for the new Queries tag
                AARPluginAndroidManifest_RemoveAllPackagesInQueriesTagAndAddAgainIfDesired(true, natPreferences.manifestQueriesVisiblePackages.ToArray());
            }

            //If is desired to disable the Queries
            if (enablePackageQueries == NativeAndroidPreferences.EnablePackageQueries.No)
            {
                //Load the AndroidManifest of AAR
                string manifestText = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

                //If not contains the markers of enabled Queries code, return
                if (manifestText.Contains("<!-- %SQUERIES% -->") == false || manifestText.Contains("<!-- %EQUERIES% -->") == false)
                    return;

                //Remove all the Package Names from the Queries tag before disable the Queries tag
                AARPluginAndroidManifest_RemoveAllPackagesInQueriesTagAndAddAgainIfDesired(false, new string[] { });

                //Re-load the AndroidManifest of AAR to get the updated AndroidManifest without the "package" tags inside of "queries" tag
                manifestText = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

                //Split the manifest
                string[] manifestParts0 = manifestText.Split(new string[] { "<!-- %SQUERIES% -->" }, StringSplitOptions.None);
                string[] manifestParts1 = manifestParts0[1].Split(new string[] { "<!-- %EQUERIES% -->" }, StringSplitOptions.None);

                //Get each part of the manifest
                string manifestPartA = manifestParts0[0];
                string manifestQueriesXml = manifestParts1[0];
                string manifestPartB = manifestParts1[1];

                //Format and disable the manifest queries xml
                manifestQueriesXml = manifestQueriesXml.Replace("action ", "action§").Replace(" ", "").Replace("§", " ").Replace("<queries>", "<!-- <queries>").Replace("</queries>", "</queries> -->");
                manifestQueriesXml = manifestQueriesXml.Replace("\n", "").Replace("\r", "").Replace(System.Environment.NewLine, "");

                //Build the final manifest xml code
                string finalManifestXml = manifestPartA + manifestQueriesXml + manifestPartB;

                //Save the modified new AndroidManifest in AAR to compress
                File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml", finalManifestXml);
            }
        }

        public void AARPluginAndroidManifest_RemoveAllPackagesInQueriesTagAndAddAgainIfDesired(bool addThePackagesName, string[] packagesNamesToAddToAARManifestQueries)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //First, try to find the Queries node
            var manifestQueries = xmlDoc.Descendants("queries").FirstOrDefault();

            //If find a "queries" tag, continue to execute the script
            if (manifestQueries != null)
            {
                //Now, remove all "package" tags childs of "queries" tag to reset and remove all package names
                foreach (var node in manifestQueries.Descendants("package").Where(node => node.Name == "package").ToList())
                    node.Remove();

                //Now, if is desired to add all the package names again, add all
                if (addThePackagesName == true)
                    foreach (string packageName in packagesNamesToAddToAARManifestQueries)
                    {
                        //Get the intent node to add the packages nodes after this
                        var intentNode = manifestQueries.Descendants("intent").ToList();

                        //Add the nodes of package after intent node
                        intentNode[0].AddAfterSelf(new XElement("package", new XAttribute(xmlAndroidNs + "name", packageName)));
                    }
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Extracted/AndroidManifest.xml", stringResult);
        }
    }

    #region STRING_WRITER_EXTENSION
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }
    #endregion
}