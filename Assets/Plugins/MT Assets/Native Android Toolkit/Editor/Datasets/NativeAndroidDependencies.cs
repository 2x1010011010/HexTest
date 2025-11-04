using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This script is the Dataset of the scriptable object "Dependencies". This script saves Native Android
     * Dependencies details.
     */

    public class NativeAndroidDependencies : ScriptableObject
    {
#if UNITY_EDITOR
        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(NativeAndroidDependencies))]
        public class CustomInspector : UnityEditor.Editor
        {
            //Variables for inspector
            private string listOfInvalidPathsForDependencies = "";
            private string listOfInvalidVersionsForDependencies = "";
            private bool showDependenciesReadMeFile = false;

            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                NativeAndroidDependencies script = (NativeAndroidDependencies)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Render the dependencies resume
                RenderTheDependenciesResumeAndInformations(script);
                GUILayout.Space(20);

                //Render default inspector
                DrawDefaultInspector();

                GUILayout.Space(20);

                //Run the data checker
                bool haveInvalidPaths = CheckIfHaveInvalidPathsInDependencies(script);
                bool haveInvalidVersion = CheckIfHaveInvalidVersionInDependencies(script);

                //If not have invalid paths, render the buttons
                if (haveInvalidPaths == false && haveInvalidVersion == false)
                {
                    //Render the button to fetch all package name of aars
                    if (GUILayout.Button("Fill All AAR Package Name Where Is Empty"))
                        FillAllAarPackageName_WhereIsEmpty(script);
                    //Render the button to fetch all classes of jars
                    if (GUILayout.Button("Fill All JAR Classes List Where Is Empty"))
                        FillAllJarClassesList_WhereIsEmpty(script);
                    //Render the button to update the dependencies xml
                    if (GUILayout.Button("Update XML for Play Services Resolver"))
                        UpdateDependenciesXmlForPlayServicesResolver(script);
                }

                GUILayout.Space(20);

                //Show tools buttons
                if (GUILayout.Button("List And Show All " + script.obsoleteDependencyFiles.Count + " Obsolete Dependency Files"))
                {
                    string list = "These are the dependency files present in \"MTAssets NAT AAR\" registered as obsolete in the NAT...\n\n";

                    foreach (string item in script.obsoleteDependencyFiles)
                        list += item + "\n";

                    EditorUtility.DisplayDialog("List of Obsolete Dependency Files", list, "Ok");
                }

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {

                }
            }

            private void RenderTheDependenciesResumeAndInformations(NativeAndroidDependencies script)
            {
                var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

                //Render the dependencies resume here
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Native Android Toolkit Dependencies Info File", labelStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("There are a total of " + (script.natCoreAARDependencies.Count + script.natCoreJARDependencies.Count) + " dependencies registered in this file.", new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(10);
                //Show the warning
                EditorGUILayout.HelpBox("This file contains informations regarding NAT dependencies. This file works as a Database that contains all references to NAT dependencies. Do not modify it unless you know what you are doing, or if instructed by MT Assets support.", MessageType.Info);
                GUILayout.Space(10);
                //Show readme file
                EditorGUILayout.LabelField("Dependencies ReadMe File", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                if (showDependenciesReadMeFile == false)
                {
                    if (GUILayout.Button("Show Dependencies ReadMe File"))
                        showDependenciesReadMeFile = true;
                }
                if (showDependenciesReadMeFile == true)
                {
                    EditorGUILayout.HelpBox(File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies/!__READ-ME_ABOUT_DEPENDENCIES__!.txt"), MessageType.None);
                    GUILayout.Space(10);
                    if (GUILayout.Button("Hide Dependencies ReadMe File"))
                        showDependenciesReadMeFile = false;
                }
                EditorGUILayout.EndVertical();
            }

            private bool CheckIfHaveInvalidPathsInDependencies(NativeAndroidDependencies script)
            {
                //Check if have invalid paths in the dependencies and show a warning if have
                listOfInvalidPathsForDependencies = "";
                for (int i = 0; i < script.natCoreAARDependencies.Count; i++)
                    if (File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + script.natCoreAARDependencies[i].filePath) == false && File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + script.natCoreAARDependencies[i].filePath + ".disabled") == false)
                        listOfInvalidPathsForDependencies += "\n[AAR " + i + "] " + script.natCoreAARDependencies[i].packageName;
                for (int i = 0; i < script.natCoreJARDependencies.Count; i++)
                    if (File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + script.natCoreJARDependencies[i].filePath) == false && File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + script.natCoreJARDependencies[i].filePath + ".disabled") == false)
                        listOfInvalidPathsForDependencies += "\n[JAR " + i + "] " + script.natCoreJARDependencies[i].jarName;
                if (string.IsNullOrEmpty(listOfInvalidPathsForDependencies) == false)
                {
                    EditorGUILayout.HelpBox("The following dependencies have invalid paths...\n" + listOfInvalidPathsForDependencies, MessageType.Error);
                    GUILayout.Space(10);
                    return true;
                }

                return false;
            }

            private bool CheckIfHaveInvalidVersionInDependencies(NativeAndroidDependencies script)
            {
                //Check if have invalid versions in the dependencies and show a warning if have
                listOfInvalidVersionsForDependencies = "";
                for (int i = 0; i < script.natCoreAARDependencies.Count; i++)
                    if (script.natCoreAARDependencies[i].filePath.Contains(script.natCoreAARDependencies[i].fileVersion) == false)
                        listOfInvalidVersionsForDependencies += "\n[AAR " + i + "] " + script.natCoreAARDependencies[i].packageName;
                for (int i = 0; i < script.natCoreJARDependencies.Count; i++)
                    if (script.natCoreJARDependencies[i].filePath.Contains(script.natCoreJARDependencies[i].fileVersion) == false)
                        listOfInvalidVersionsForDependencies += "\n[JAR " + i + "] " + script.natCoreJARDependencies[i].jarName;
                if (string.IsNullOrEmpty(listOfInvalidVersionsForDependencies) == false)
                {
                    EditorGUILayout.HelpBox("The following dependencies have versions informed that do not match the version in the dependency filename...\n" + listOfInvalidVersionsForDependencies, MessageType.Error);
                    GUILayout.Space(10);
                    return true;
                }

                return false;
            }

            private void FillAllAarPackageName_WhereIsEmpty(NativeAndroidDependencies script)
            {
                //This method will get all package name of all aars files and insert into the package name of each item
                EditorUtility.DisplayProgressBar("A moment", "Reading AAR files...", 1.0f);
                System.Threading.Thread.Sleep(1000);

                //Loop into all AARs
                foreach (NATCoreAARDependencies aar in script.natCoreAARDependencies)
                {
                    //If this aar have package name registered, skip this
                    if (string.IsNullOrEmpty(aar.packageName) == false)
                        continue;
                    //If this jar file not exists, skip this
                    if (File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath) == false)
                    {
                        Debug.LogError("AAR not found: " + aar.filePath);
                        continue;
                    }

                    //Extract the aar file to get package name
                    using (IonicDotNetZip.Zip.ZipFile zipUnpack = IonicDotNetZip.Zip.ZipFile.Read("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath))
                        foreach (IonicDotNetZip.Zip.ZipEntry e in zipUnpack)
                            if (e.FileName == "AndroidManifest.xml")
                            {
                                string androidManifestContent = "";
                                StreamReader streamReader = new StreamReader(e.OpenReader());
                                while (streamReader.EndOfStream == false)
                                    androidManifestContent += streamReader.ReadLine();
                                XDocument xmlDoc = XDocument.Parse(androidManifestContent);
                                XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
                                xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);
                                aar.packageName = xmlDoc.Descendants("manifest").FirstOrDefault().Attribute("package").Value;
                            }
                }

                //Remove the progressbar
                EditorUtility.ClearProgressBar();
            }

            private void FillAllJarClassesList_WhereIsEmpty(NativeAndroidDependencies script)
            {
                //This method will get all classes name of all jars files and insert into the classes name array
                EditorUtility.DisplayProgressBar("A moment", "Reading JAR files...", 1.0f);
                System.Threading.Thread.Sleep(1000);

                //Loop into all JARs
                foreach (NATCoreJARDependencies jar in script.natCoreJARDependencies)
                {
                    //If this jar have jar classes registered in list, skip this
                    if (jar.jarClasses.Count > 0)
                        continue;
                    //If this jar file not exists, skip this
                    if (File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath) == false)
                    {
                        Debug.LogError("JAR not found: " + jar.filePath);
                        continue;
                    }

                    //Extract the jar file to get classes name
                    using (IonicDotNetZip.Zip.ZipFile zipUnpack = IonicDotNetZip.Zip.ZipFile.Read("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath))
                        foreach (IonicDotNetZip.Zip.ZipEntry e in zipUnpack)
                            if (Path.GetExtension(e.FileName).ToLower() == ".class") //<- Find all .class file
                                jar.jarClasses.Add(e.FileName); //or use "jar.jarClasses.Add(Path.GetFileName(e.FileName));" to not use directories
                }

                //Remove the progressbar
                EditorUtility.ClearProgressBar();
            }

            private void UpdateDependenciesXmlForPlayServicesResolver(NativeAndroidDependencies script)
            {
                //Modify the PlayServicesResolverDependencies.xml file
                EditorUtility.DisplayProgressBar("A moment", "Updating XML Dependencies file...", 1.0f);
                System.Threading.Thread.Sleep(1000);

                //Prepare the document
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                //Load the file
                xmlDoc.Load("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies/Editor/PlayServicesResolverDependencies.xml");
                //Do the operation of update dependencies
                System.Xml.XmlNode rootNode = xmlDoc.SelectSingleNode("//dependencies");
                var androidPackagesNode = rootNode.ChildNodes[0];
                var separatorNode = androidPackagesNode.ChildNodes[2];

                //First clear all existing dependencies
                System.Xml.XmlNodeList nodesFromAndroidPackages = androidPackagesNode.ChildNodes;
                List<System.Xml.XmlNode> listToBeRemoved = new List<System.Xml.XmlNode>();
                for (int i = 0; i < nodesFromAndroidPackages.Count; i++)
                    if (nodesFromAndroidPackages[i].Name == "androidPackage")
                        listToBeRemoved.Add(nodesFromAndroidPackages[i]);
                foreach (System.Xml.XmlNode node in listToBeRemoved)
                    androidPackagesNode.RemoveChild(node);

                //Add all current dependencies existing in "Dependencies.asset"
                foreach (NATCoreAARDependencies aar in script.natCoreAARDependencies)
                {
                    //Skip if is desired to not include
                    if (aar.declareInXml == false)
                        continue;

                    //Convert the file name to package name for maven
                    string preparedFileName = aar.filePath.Replace("Dependencies/", "");
                    string[] fileNameExploded = preparedFileName.Split(new[] { '-' }, 2);
                    string groupPackage = fileNameExploded[0];
                    string package = fileNameExploded[1].Replace(("-" + aar.fileVersion + ".aar"), "");
                    System.Xml.XmlNode packageDependencyNode = xmlDoc.CreateNode(System.Xml.XmlNodeType.Element, "androidPackage", "");
                    System.Xml.XmlAttribute packageDependencyAtt = xmlDoc.CreateAttribute("spec");
                    packageDependencyAtt.Value = groupPackage + ":" + package + ":" + aar.fileVersion;
                    packageDependencyNode.Attributes.SetNamedItem(packageDependencyAtt);
                    androidPackagesNode.AppendChild(packageDependencyNode);
                }
                foreach (NATCoreJARDependencies jar in script.natCoreJARDependencies)
                {
                    //Skip if is desired to not include
                    if (jar.declareInXml == false)
                        continue;

                    //Convert the file name to package name for maven
                    string preparedFileName = jar.filePath.Replace("Dependencies/", "");
                    string[] fileNameExploded = preparedFileName.Split(new[] { '-' }, 2);
                    string groupPackage = fileNameExploded[0];
                    string package = fileNameExploded[1].Replace(("-" + jar.fileVersion + ".jar"), "");
                    System.Xml.XmlNode packageDependencyNode = xmlDoc.CreateNode(System.Xml.XmlNodeType.Element, "androidPackage", "");
                    System.Xml.XmlAttribute packageDependencyAtt = xmlDoc.CreateAttribute("spec");
                    packageDependencyAtt.Value = groupPackage + ":" + package + ":" + jar.fileVersion;
                    packageDependencyNode.Attributes.SetNamedItem(packageDependencyAtt);
                    androidPackagesNode.AppendChild(packageDependencyNode);
                }

                //Save the modified new dependencies xml file
                xmlDoc.Save("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies/Editor/PlayServicesResolverDependencies.xml");
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
        #endregion
#endif

        [System.Serializable]
        public class NATCoreAARDependencies
        {
            [Tooltip("This is the Package Name of this AAR in the AndroidManifest.xml of this AAR.")]
            public string packageName = "";
            [Tooltip("Shows whether this AAR provided by NAT should be enabled or not.")]
            public bool isEnabled = true;
            [Tooltip("Informs whether this dependency can be included in the Play Services XML resolve or not.")]
            public bool declareInXml = true;
            [Tooltip("Shows the path to this AAR provided by NAT.")]
            public string filePath = "";
            [Tooltip("Shows the path to this AAR provided by NAT.\n\n(THIS INFORMATION WILL ONLY BE USED IN THE PREFERENCES WINDOW)")]
            public string fileVersion = "";
        }

        [System.Serializable]
        public class NATCoreJARDependencies
        {
            [Tooltip("This is the group and name (format: group-name) of this JAR file.")]
            public string jarName = "";
            [Tooltip("Here are all the names of the .class files contained in this JAR.")]
            public List<string> jarClasses = new List<string>();
            [Tooltip("Shows whether this JAR provided by NAT should be enabled or not.")]
            public bool isEnabled = true;
            [Tooltip("Informs whether this dependency can be included in the Play Services XML resolve or not.")]
            public bool declareInXml = true;
            [Tooltip("Shows the path to this JAR provided by NAT.")]
            public string filePath = "";
            [Tooltip("Shows the path to this AAR provided by NAT.\n\n(THIS INFORMATION WILL ONLY BE USED IN THE PREFERENCES WINDOW)")]
            public string fileVersion = "";
        }

        public List<NATCoreAARDependencies> natCoreAARDependencies = new List<NATCoreAARDependencies>();
        public List<NATCoreJARDependencies> natCoreJARDependencies = new List<NATCoreJARDependencies>();
        [Space(10)]
        public List<string> obsoleteDependencyFiles = new List<string>();
    }
}