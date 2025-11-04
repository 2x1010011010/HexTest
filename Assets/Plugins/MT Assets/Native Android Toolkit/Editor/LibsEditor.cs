using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    public class LibsEditor : EditorWindow
    {
        //Private variables
        private Vector2 scrollPosPreferences;

        //Setup window code

        public static void OpenWindow()
        {
            //Method to open the Window
            var window = GetWindow<LibsEditor>("NAT Libs");
            window.minSize = new Vector2(450, 800);
            window.maxSize = new Vector2(450, 800);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.margin = new RectOffset(4, 0, 4, 0);

            //UI Code
            EditorGUILayout.LabelField("NAT Dependencies Editor", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Please note that the changes made here are not permanent as they may be reset when NAT is updated in your project, or when the NAT Dependencies Resolver run.", MessageType.Warning);
            GUILayout.Space(10);

            //Try to load the dependencies informations
            NativeAndroidDependencies natDependencies = (NativeAndroidDependencies)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies.asset", typeof(NativeAndroidDependencies));
            //Show all dependencies information
            Texture dependencyRow = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/DependencyRow.png", typeof(Texture));
            Texture aarNotFound = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Not-Found.png", typeof(Texture));
            Texture aarEnabled = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Enabled.png", typeof(Texture));
            Texture aarDisabled = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/AAR-Disabled.png", typeof(Texture));

            //Render all dependencies
            scrollPosPreferences = EditorGUILayout.BeginScrollView(scrollPosPreferences, GUILayout.Width(446), GUILayout.Height(628));
            GUIStyle dependencyRowBox = new GUIStyle();
            dependencyRowBox.normal.background = (Texture2D)dependencyRow;
            bool isRowWithBackground = true;
            int libsDisabled = 0;
            int libsEnabled = 0;
            foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in natDependencies.natCoreAARDependencies)
            {
                if (isRowWithBackground == true)
                    EditorGUILayout.BeginVertical(dependencyRowBox);
                if (isRowWithBackground == false)
                    EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(7);
                bool aarEnabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath);
                bool aarDisabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled");
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If AAR is enabled
                    GUILayout.Box(aarEnabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If AAR is disabled
                    GUILayout.Box(aarDisabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == false) //<- If AAR not found
                    GUILayout.Box(aarNotFound, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                EditorGUILayout.LabelField(aar.packageName, GUILayout.Width(310));
                GUILayout.FlexibleSpace();
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If AAR is enabled
                {
                    if (GUILayout.Button("Disable", GUILayout.Height(18), GUILayout.Width(80)))
                    {
                        File.Move("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath, "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled");
                        File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".meta");
                        AssetDatabase.Refresh();
                    }
                    libsEnabled += 1;
                }
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If AAR is disabled
                {
                    if (GUILayout.Button("Enable", GUILayout.Height(18), GUILayout.Width(80)))
                    {
                        File.Move("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled", "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath);
                        File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled.meta");
                        AssetDatabase.Refresh();
                    }
                    libsDisabled += 1;
                }
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.EndVertical();
                //Change the next background
                isRowWithBackground = !isRowWithBackground;
            }
            foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in natDependencies.natCoreJARDependencies)
            {
                if (isRowWithBackground == true)
                    EditorGUILayout.BeginVertical(dependencyRowBox);
                if (isRowWithBackground == false)
                    EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(7);
                bool aarEnabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath);
                bool aarDisabledFound = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled");
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If AAR is enabled
                    GUILayout.Box(aarEnabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If AAR is disabled
                    GUILayout.Box(aarDisabled, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                if (aarEnabledFound == false && aarDisabledFound == false) //<- If AAR not found
                    GUILayout.Box(aarNotFound, iconStyle, GUILayout.Width(14), GUILayout.Height(14));
                EditorGUILayout.LabelField(jar.jarName, GUILayout.Width(310));
                GUILayout.FlexibleSpace();
                if (aarEnabledFound == true && aarDisabledFound == false)  //<- If AAR is enabled
                {
                    if (GUILayout.Button("Disable", GUILayout.Height(18), GUILayout.Width(80)))
                    {
                        File.Move("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath, "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled");
                        File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".meta");
                        AssetDatabase.Refresh();
                    }
                    libsEnabled += 1;
                }
                if (aarEnabledFound == false && aarDisabledFound == true)  //<- If AAR is disabled
                {
                    if (GUILayout.Button("Enable", GUILayout.Height(18), GUILayout.Width(80)))
                    {
                        File.Move("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled", "Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath);
                        File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled.meta");
                        AssetDatabase.Refresh();
                    }
                    libsDisabled += 1;
                }
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.EndVertical();
                //Change the next background
                isRowWithBackground = !isRowWithBackground;
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("There are " + libsEnabled + " dependencies enabled and " + libsDisabled + " dependencies disabled", new GUIStyle() { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            if (GUILayout.Button("Done", GUILayout.Height(40)))
                this.Close();

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }
    }
}