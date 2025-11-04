using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Xml.Linq;
using System.Linq;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class is responsible for auto resolve AAR/JAR dependencies of NAT Core AAR avoiding
     * duplicate AARs/JARs in project, in case of the project already have the same dependencies
     * of the NAT Core. Also run automatic dependency resolution.
     */

    [InitializeOnLoad]
    public class LibsResolver
    {
        static LibsResolver()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += DetectIfDependecyResolverAlreadyRunnedAndRunIfNotRunnedInProjectYet;
        }

        public static void DetectIfDependecyResolverAlreadyRunnedAndRunIfNotRunnedInProjectYet()
        {
            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets"))
                AssetDatabase.CreateFolder("Assets/Plugins", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData/Editor"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets/_AssetsData", "Editor");

            //Try to load the file that informs that dependency resolver was runned
            object lastDependencyResolverRun = AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastResolverRun.ini", typeof(object));

            //If have the file that informs that dependency resolver was runned, cancel
            if (lastDependencyResolverRun != null)
                return;

            //Run dependency resolver
            RunNativeAndroidToolkitDependenciesResolver(false);

            //Create the file that informs that dependency resolver was auto runned
            DateTime currentDateTime = DateTime.Now;
            File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastResolverRun.ini", currentDateTime.ToString());
        }

        //Core methods

        public static void RunNativeAndroidToolkitDependenciesResolver(bool calledByTheUser)
        {
            //If called by the user, show alert dialog
            if (calledByTheUser == true)
                if (EditorUtility.DisplayDialog("Run NAT Dependencies Resolver?", "Native Android Toolkit's Dependency Resolver will automatically look for missing or duplicate AAR/JAR libraries in your project.\n\nIn case of missing AAR/JAR libraries, it will automatically add them. In case of duplicate AAR/JAR libraries, it will remove the duplicates (but it will only remove the duplicate AAR/JAR libraries added by Native Android Toolkit, any other library added by you or another tool will not be modified).\n\nThis is very useful if you are having some compilation error when trying to build your APK, due to some issue with missing or duplicated AAR/JAR libraries.\n\nNote that NAT Dependency Resolver will only resolve NAT Core AAR/JAR dependencies. It will not resolve the dependencies of any other plugins, AARs, JARs, libraries or tools in your project.\n\nDo you want to run NAT Dependencies Resolver?", "Run", "Cancel") == false)
                    return;

            //Show progress bar to indicate the dependency resolver running
            EditorUtility.DisplayProgressBar("NAT Dependencies Resolver", "Running...", 1.0f);

            //Allocate some variables for use of dependencies resolver
            string dependenciesResolverLog = GetNewFormattedLineForLogFile("NAT Dependencies Resolver started its execution.", false);
            bool exceptionOcurred = false;

            //Check if the log is major than 1MB. If is greather than this zide, reset the log
            ResetTheCurrentLogFileIfHaveSizeGreaterThan(1024);
            //Do the initialization task of dependencies resolver and wait the time of initialization
            WaitDependenciesResolverInitializationTime();

            //---------------------------------- Start of Dependencies Resolver code ----------------------------------

            //Inform if this task is called by the user
            if (calledByTheUser == true)
                dependenciesResolverLog += GetNewFormattedLineForLogFile("Detail: This NAT Dependencies Resolver run was manually started by the user.", false);
            if (calledByTheUser == false)
                dependenciesResolverLog += GetNewFormattedLineForLogFile("Detail: This NAT Dependencies Resolver run was automatically started by the tool.", false);

            //Try to load the dependencies informations
            NativeAndroidDependencies natDependencies = (NativeAndroidDependencies)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/Dependencies.asset", typeof(NativeAndroidDependencies));

            //If nat dependencies informations not loaded, inform in log
            if (natDependencies == null)
            {
                dependenciesResolverLog += GetNewFormattedLineForLogFile("Could not load NAT dependencies information file. Stopping Dependencies Resolver.", false);
                exceptionOcurred = true;
            }
            //If nat dependencies informations was loaded, continue to resolve
            if (natDependencies != null)
                try //<- Protect the code from crash with try
                {
                    //Inform that dependencies informations was loaded
                    dependenciesResolverLog += GetNewFormattedLineForLogFile("The NAT dependencies information file has been successfully loaded.", false);

                    //Check if have AAR/JARs duplicateds where one of these two duplicates have a ".disabled" extension, and remove the duplicate to avoid conflicts
                    CheckIfHaveDependenciesEnabledAndDisabledAtSameTimeInNATAndRemoveDuplicated(natDependencies);

                    //======================================== AAR Resolution ========================================

                    //Inform that is starting the project AARs scanner to find all AARs in project
                    dependenciesResolverLog += GetNewFormattedLineForLogFile("Scan: Scanning and resolving of the AARs in the project has started.", false);

                    //Mark all AARs included with NAT to be enabled (this is reset all AARs states and duplicate AARs will be marked to be disabled below)
                    foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in natDependencies.natCoreAARDependencies)
                        aar.isEnabled = true;

                    //Find all AARs paths in the project and split all AARs that is not provided by NAT to be checked
                    List<string> nonNatAARs = new List<string>();
                    foreach (string aarPath in Directory.EnumerateFiles("Assets", "*.aar", SearchOption.AllDirectories))
                        if (aarPath.Contains("MTAssets NAT AAR") == false)
                            nonNatAARs.Add(aarPath);

                    //If not found any AAR not provided by NAT, warn in the log
                    if (nonNatAARs.Count == 0)
                    {
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("No other AAR libraries (not provided by NAT) were found in this project.", false);
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                    }
                    //If found any other AAR not provided by NAT, try to find duplicates or missing AARs, and mark to be enabled or disabled a equal AAR provided by NAT
                    if (nonNatAARs.Count >= 1)
                        foreach (string aarFile in nonNatAARs)
                        {
                            //Inform a AAR found in project, to log
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Found AAR not provided by NAT: An AAR library not included with NAT was found!", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("AAR File Name: \"" + Path.GetFileName(aarFile) + "\"", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("AAR File Path: \"" + aarFile + "\"", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Unpacking and reading this AAR to get their informations and details...", false);

                            //Allocate a variable to store package name of this AAR
                            string aarPackageName = "";
                            //Try to read the packageName of this AAR, reading the AndroidManifest.xml inside of the AAR
                            try
                            {
                                using (IonicDotNetZip.Zip.ZipFile zipUnpack = IonicDotNetZip.Zip.ZipFile.Read(aarFile))
                                    foreach (IonicDotNetZip.Zip.ZipEntry e in zipUnpack)
                                        if (e.FileName == "AndroidManifest.xml")
                                            aarPackageName = ReadAndroidManifestFromAARAndGetPackageName(e.OpenReader());
                            }
                            catch (Exception e)
                            {
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("ERROR: Unable to unzip this AAR. It will not be possible to verify it, it will be ignored. Exception Ocurred: \"" + e.Message + "\".", false);
                                continue;   //<- Go to next AAR because this is unreadable
                            }
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("AAR Package Name: \"" + aarPackageName + "\"", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Checking if any AAR provided by NAT is a duplicate of this AAR...", false);
                            //Try to find a AAR provided by NAT, that is a duplicate of this AAR,  if find a duplicate, mark to be disabled
                            bool haveDuplicateAAR = false;
                            foreach (NativeAndroidDependencies.NATCoreAARDependencies natAAR in natDependencies.natCoreAARDependencies)
                                if (natAAR.packageName == aarPackageName)
                                {
                                    natAAR.isEnabled = false; //<- Mark to be disabled
                                    haveDuplicateAAR = true;  //<- Inform that was found a duplicate AAR
                                }
                            if (haveDuplicateAAR == true)
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("An AAR provided by NAT, which is a duplicate of this AAR, was found. The duplicate AAR provided by NAT has been marked to be disabled.", false);
                            if (haveDuplicateAAR == false)
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("No AAR provided by NAT, which is a duplicate of this AAR, was found.", false);

                            //Inform the end of this package check in log
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                        }

                    //Inform that is finished the project AARs scanner
                    dependenciesResolverLog += GetNewFormattedLineForLogFile("Scan: Scanning and resolving of the AARs has been completed.", false);

                    //======================================== JAR Resolution ========================================

                    //Inform that is starting the project JARs scanner to find all JARs in project
                    dependenciesResolverLog += GetNewFormattedLineForLogFile("Scan: Scanning and resolving of the JARs in the project has started.", false);

                    //Mark all JARs included with NAT to be enabled (this is reset all JARs states and duplicate JARs will be marked to be disabled below)
                    foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in natDependencies.natCoreJARDependencies)
                        jar.isEnabled = true;

                    //Find all JARs paths in the project and split all JARs that is not provided by NAT to be checked
                    List<string> nonNatJARs = new List<string>();
                    foreach (string jarPath in Directory.EnumerateFiles("Assets", "*.jar", SearchOption.AllDirectories))
                        if (jarPath.Contains("MTAssets NAT AAR") == false)
                            nonNatJARs.Add(jarPath);

                    //If not found any JAR not provided by NAT, warn in the log
                    if (nonNatJARs.Count == 0)
                    {
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("No other JAR libraries (not provided by NAT) were found in this project.", false);
                        dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                    }
                    //If found any other JAR not provided by NAT, try to find duplicates or missing JARs, and mark to be enabled or disabled a equal JAR provided by NAT
                    if (nonNatJARs.Count >= 1)
                        foreach (string jarFile in nonNatJARs)
                        {
                            //Inform a JAR found in project, to log
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Found JAR not provided by NAT: An JAR library not included with NAT was found!", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("JAR File Name: \"" + Path.GetFileName(jarFile) + "\"", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("JAR File Path: \"" + jarFile + "\"", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Unpacking and reading this JAR to get their informations and details...", false);

                            //Allocate a variable to store all classes name of this JAR
                            List<string> jarClassesName = new List<string>();
                            //Try to get all classes names of this JAR, reading .class files inside of the JAR
                            try
                            {
                                using (IonicDotNetZip.Zip.ZipFile zipUnpack = IonicDotNetZip.Zip.ZipFile.Read(jarFile))
                                    foreach (IonicDotNetZip.Zip.ZipEntry e in zipUnpack)
                                        if (Path.GetExtension(e.FileName).ToLower() == ".class")
                                            jarClassesName.Add(e.FileName); //or use "jarClassesName.Add(Path.GetFileName(e.FileName));" to ignore directories
                            }
                            catch (Exception e)
                            {
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("ERROR: Unable to unzip this JAR. It will not be possible to verify it, it will be ignored. Exception Ocurred: \"" + e.Message + "\".", false);
                                continue;   //<- Go to next JAR because this is unreadable
                            }
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("JAR Classes: Count of " + jarClassesName.Count + " classes were found inside this JAR.", false);
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("Checking if any JAR provided by NAT is a duplicate of this JAR...", false);
                            //Try to find a JAR provided by NAT, that is a duplicate of this JAR, if find a duplicate, mark to be disabled
                            bool haveDuplicateJAR = false;
                            foreach (NativeAndroidDependencies.NATCoreJARDependencies natJAR in natDependencies.natCoreJARDependencies)
                                foreach (string natJARClassName in natJAR.jarClasses)
                                    if (jarClassesName.Contains(natJARClassName) == true)
                                    {
                                        natJAR.isEnabled = false; //<- Mark to be disabled
                                        haveDuplicateJAR = true;  //<- Inform that was found a duplicate JAR
                                    }
                            if (haveDuplicateJAR == true)
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("An JAR provided by NAT, which is a duplicate of this JAR, was found. The duplicate JAR provided by NAT has been marked to be disabled.", false);
                            if (haveDuplicateJAR == false)
                                dependenciesResolverLog += GetNewFormattedLineForLogFile("No JAR provided by NAT, which is a duplicate of this JAR, was found.", false);

                            //Inform the end of this package check in log
                            dependenciesResolverLog += GetNewFormattedLineForLogFile("-------------------------------------------", false);
                        }

                    //Inform that is finished the project JARs scanner
                    dependenciesResolverLog += GetNewFormattedLineForLogFile("Scan: Scanning and resolving of the JARs has been completed.", false);

                    //============================= Do Dependencies Enable Or Disable Sync =============================

                    //Do the sync where dependencies marked as isEnabled=true is enabled and isEnabled=false is disabled from NAT
                    dependenciesResolverLog += SyncTheDependenciesEnableOrDisableAccordingToDependenciesInfoFileAndGetResultLog(natDependencies);

                    //============================= Do the task of deleting all obsolete files =============================

                    //Delete all obsolete dependencies files, all that is listed in the dependencies file
                    dependenciesResolverLog += DeleteAllObsoleteDependenciesFiles(natDependencies);
                }
                catch (Exception e) { exceptionOcurred = true; dependenciesResolverLog += GetNewFormattedLineForLogFile("Exception occurred: \"" + e.Message + "\" Stack Trace: \"" + e.StackTrace.Replace("\n", " --- ") + "\". Stopping Dependencies Resolver execution.", false); }

            //----------------------------------- End of Dependencies Resolver code -----------------------------------

            //Append the log of this operation to logs of dependencies resolver
            dependenciesResolverLog += GetNewFormattedLineForLogFile("NAT Dependencies Resolver has completed its execution.", true);
            File.AppendAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", dependenciesResolverLog);

            //Update all assets
            AssetDatabase.Refresh();
            //Inform that dependencies resolver was runned
            Debug.Log("The NAT Dependencies Resolver has completed execution. You can see everything that happened in the NAT Dependencies Resolver log which can be seen in the NAT Preferences.\n\nRemember: NAT Dependencies Resolver only resolves NAT dependencies. See the documentation for more details.\n");

            //Hide progress bar
            EditorUtility.ClearProgressBar();

            //Show a dialog to inform conclusion
            if (exceptionOcurred == false && calledByTheUser == true)
                if (EditorUtility.DisplayDialog("NAT Dependencies Resolver Was Runned", "The NAT Dependencies Resolver was runned successfully. Missing NAT libraries have been added and duplicate AAR/JAR libraries (added by NAT) have been removed.\n\nIf you are having any issues involving NAT AAR/JAR Libraries or compilation/build errors caused by Native Android Toolkit, please don't hesitate to contact MT Assets support at mtassets@windsoft.xyz.", "Done", "Open Log") == false)
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", typeof(TextAsset)));
            if (exceptionOcurred == true && calledByTheUser == true)
                if (EditorUtility.DisplayDialog("NAT Dependencies Resolver Error", "An error occurred while running NAT Dependecies Resolver. An exception or other generic error occurred while trying to run Dependencies Resolver. Probably the task was interrupted. Please see the log for more details.\n\nIf you continue to get these errors, please do not hesitate to contact MT Assets support at mtassets@windsoft.xyz.", "Done", "Open Log") == false)
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", typeof(TextAsset)));
        }

        //Tools methods

        private static void CheckIfHaveDependenciesEnabledAndDisabledAtSameTimeInNATAndRemoveDuplicated(NativeAndroidDependencies dependenciesInfoFile)
        {
            //This method will run before the run of Dependencies Resolver and is called by Dependencies Resolver.
            //This method checks if have a AAR/JAR enabled and a same AAR/JAR with same name and extension ".disabled", and will delete the AAR/JAr with ".disabled" extension to avoid duplicates
            foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in dependenciesInfoFile.natCoreAARDependencies)
            {
                //Check if the files exists
                bool existsWithoutExtension = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath);
                bool existsWithExtension = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled");

                //Delete the file with .disabled extension
                if (existsWithExtension == true && existsWithoutExtension == true)
                {
                    File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled");
                    File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath + ".disabled.meta");

                    Debug.LogWarning("NAT Dependencies Resolver: A duplicated AAR/JAR was found, where one has the extension \".disabled\" and the other does not. The duplicate extension \".disabled\" was deleted in path \"" + aar.filePath + "\".");
                }
            }
            foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in dependenciesInfoFile.natCoreJARDependencies)
            {
                //Check if the files exists
                bool existsWithoutExtension = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath);
                bool existsWithExtension = File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled");

                //Delete the file with .disabled extension
                if (existsWithExtension == true && existsWithoutExtension == true)
                {
                    File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled");
                    File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath + ".disabled.meta");

                    Debug.LogWarning("NAT Dependencies Resolver: A duplicated AAR/JAR was found, where one has the extension \".disabled\" and the other does not. The duplicate extension \".disabled\" was deleted in path \"" + jar.filePath + "\".");
                }
            }

            //Update the Asset Database
            AssetDatabase.Refresh();
        }

        private static string GetNewFormattedLineForLogFile(string lineContent, bool doubleLineBreak)
        {
            //Return a new formatted line for 
            DateTime dateTime = DateTime.Now;
            return "[" + dateTime.ToString() + "] > " + lineContent + ((doubleLineBreak == false) ? "\n" : "\n\n");
        }

        private static void ResetTheCurrentLogFileIfHaveSizeGreaterThan(int maxSizeInKbytes)
        {
            //If the file of logs not exists, cancel
            if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt") == false)
                return;

            //Get the log file, size in kbytes
            int lenght = (int)(new FileInfo("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt").Length / 1024);
            //If the file size is bigger than the max size, reset the file
            if (lenght > maxSizeInKbytes)
            {
                //Get current date
                int currentDateTime = (int)(DateTime.Now.Ticks / 2);

                //Rename the current log file to create a new log
                File.Move("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", "Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog-Old" + currentDateTime + ".txt");

                //Create the new empty log file
                File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverLog.txt", "This is a new log file. The old log file is past the maximum size of " + maxSizeInKbytes + " Kb and has been renamed to \"NATDependenciesResolverLog-Old" + currentDateTime + ".txt\".\n\n");
            }
        }

        private static void WaitDependenciesResolverInitializationTime()
        {
            //Do the initialization task time of Dependencies Resolver
            int waitTime = 8000;

            //If the initialization time info file exists
            if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini") == true)
            {
                //Read the time in the info file
                long fileTicks = long.Parse(File.ReadAllLines("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini")[0]);

                //If the difference of time between current time and time of file is smaller than 10 minutes, reduces the wait time
                TimeSpan currenTime = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan fileTime = new TimeSpan(fileTicks);
                TimeSpan differenceTime = currenTime - fileTime;
                if (differenceTime.Minutes < 10)
                {
                    EditorUtility.DisplayProgressBar("NAT Dependencies Resolver", "Resolving...", 1.0f);
                    waitTime = 1000;
                }

                //Write the new time in file
                DateTime currentDateTime = DateTime.Now;
                File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini", currentDateTime.Ticks + "\n" + currentDateTime.ToString());
            }
            //If the initialization time info file not exists, create one
            if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini") == false)
            {
                DateTime currentDateTime = DateTime.Now;
                File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATDependenciesResolverTime.ini", currentDateTime.Ticks + "\n" + currentDateTime.ToString());
            }

            //Do the wait time
            System.Threading.Thread.Sleep(waitTime);
        }

        private static string ReadAndroidManifestFromAARAndGetPackageName(Stream aarFileStream)
        {
            //Get the AndroidManifest.xml file from AAR, with a Stream and return the package name of AAR
            string androidManifestContent = "";
            StreamReader streamReader = new StreamReader(aarFileStream);
            while (streamReader.EndOfStream == false)
                androidManifestContent += streamReader.ReadLine();
            XDocument xmlDoc = XDocument.Parse(androidManifestContent);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);
            return xmlDoc.Descendants("manifest").FirstOrDefault().Attribute("package").Value;
        }

        private static string SyncTheDependenciesEnableOrDisableAccordingToDependenciesInfoFileAndGetResultLog(NativeAndroidDependencies dependenciesInfoFile)
        {
            //Do the sync task where all dependencies files markted as isEnabled=true is enabled and isEnabled=false is disabled
            string resultLog = "";
            resultLog += GetNewFormattedLineForLogFile("===========================================", false);
            resultLog += GetNewFormattedLineForLogFile("Sync: Starting state synchronization of dependencies provided by NAT.", false);
            resultLog += GetNewFormattedLineForLogFile("Sync: Dependencies marked to be disabled (isEnabled=false) will now be disabled.", false);
            resultLog += GetNewFormattedLineForLogFile("Sync: Dependencies marked to be enabled (isEnabled=true) will now be enabled.", false);

            //========================================= Dependencies Marked To Be Enabled =========================================

            //Enable all dependencies provided by NAT where isEnabled is true
            foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in dependenciesInfoFile.natCoreAARDependencies)
                if (aar.isEnabled == true)
                {
                    Sync_SetEnabledOrDisabledOnTheNATDependencyFile("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath, true);

                    //Inform in the log
                    resultLog += GetNewFormattedLineForLogFile("Dependency Enabled: The AAR (\"" + aar.packageName + "\") dependency provided by NAT has been enabled.", false);
                }
            foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in dependenciesInfoFile.natCoreJARDependencies)
                if (jar.isEnabled == true)
                {
                    Sync_SetEnabledOrDisabledOnTheNATDependencyFile("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath, true);

                    //Inform in the log
                    resultLog += GetNewFormattedLineForLogFile("Dependency Enabled: The JAR (\"" + jar.jarName + "\") dependency provided by NAT has been enabled.", false);
                }

            //========================================= Dependencies Marked To Be Disabled =========================================

            //Disable all dependencies provided by NAT where isEnabled is false
            foreach (NativeAndroidDependencies.NATCoreAARDependencies aar in dependenciesInfoFile.natCoreAARDependencies)
                if (aar.isEnabled == false)
                {
                    Sync_SetEnabledOrDisabledOnTheNATDependencyFile("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + aar.filePath, false);

                    //Inform in the log
                    resultLog += GetNewFormattedLineForLogFile("Dependency Disabled: The AAR (\"" + aar.packageName + "\") dependency provided by NAT has been disabled.", false);
                }
            foreach (NativeAndroidDependencies.NATCoreJARDependencies jar in dependenciesInfoFile.natCoreJARDependencies)
                if (jar.isEnabled == false)
                {
                    Sync_SetEnabledOrDisabledOnTheNATDependencyFile("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + jar.filePath, false);

                    //Inform in the log
                    resultLog += GetNewFormattedLineForLogFile("Dependency Disabled: The JAR (\"" + jar.jarName + "\") dependency provided by NAT has been disabled.", false);
                }

            //========================================================= End ========================================================

            //End the log of sync
            resultLog += GetNewFormattedLineForLogFile("Sync: Finished state synchronization of dependencies provided by NAT.", false);
            resultLog += GetNewFormattedLineForLogFile("===========================================", false);

            //Return the result log
            return resultLog;
        }

        private static void Sync_SetEnabledOrDisabledOnTheNATDependencyFile(string baseDependencyFilePath, bool setEnabled)
        {
            //If is set to be enabled
            if (setEnabled == true)
                if (File.Exists(baseDependencyFilePath + ".disabled") == true)
                    File.Move(baseDependencyFilePath + ".disabled", baseDependencyFilePath);
            //If is set to be disabled
            if (setEnabled == false)
                if (File.Exists(baseDependencyFilePath) == true)
                    File.Move(baseDependencyFilePath, baseDependencyFilePath + ".disabled");

            //Delete the meta files to avoid log errors
            if (File.Exists(baseDependencyFilePath + ".meta") == true)
                File.Delete(baseDependencyFilePath + ".meta");
            if (File.Exists(baseDependencyFilePath + ".disabled.meta") == true)
                File.Delete(baseDependencyFilePath + ".disabled.meta");
        }

        private static string DeleteAllObsoleteDependenciesFiles(NativeAndroidDependencies dependenciesInfoFile)
        {
            //Delete all files in the list of obsolete dependencies files
            string resultLog = "";
            string resultLog2 = "";
            resultLog2 += GetNewFormattedLineForLogFile("============ POST UPDATE CLEAR ============", false);
            bool haveObsoleteFiles = false;

            foreach (string file in dependenciesInfoFile.obsoleteDependencyFiles)
                if (File.Exists("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + file) == true)
                {
                    File.Delete("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/" + file);
                    resultLog2 += GetNewFormattedLineForLogFile("The obsolete dependency file \"" + file + "\" has been deleted.", false);
                    haveObsoleteFiles = true;
                }

            resultLog2 += GetNewFormattedLineForLogFile("===========================================", false);

            //If have obsolete files found, pass the content of log2 to the resultLog main to show in the log
            if (haveObsoleteFiles == true)
                resultLog += resultLog2;

            //Return the result log
            return resultLog;
        }
    }
}