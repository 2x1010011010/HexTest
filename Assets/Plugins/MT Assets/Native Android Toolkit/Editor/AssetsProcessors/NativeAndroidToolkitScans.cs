using UnityEditor;
using System.IO;

public class NativeAndroidToolkitScans : AssetPostprocessor
{
    //Class that monitor imports/deletes of AAR/JAR files, and call de NAT dependencies resolver to run automatically every
    //time that a AAR/JAR asset is imported or deleted.

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //Prepare the information 
        bool mustRunDependenciesResolve = false;

        //Detect if have AARs/JARs importeds (ignore AARs/JARs provided with Native Android Toolkit)
        foreach (string str in importedAssets)
            if (str.Contains("MTAssets NAT AAR") == false)
                if (Path.GetExtension(str).ToLower() == ".aar" || Path.GetExtension(str).ToLower() == ".jar")
                    mustRunDependenciesResolve = true;

        //Detect if have AARs/JARs deleteds (ignore AARs/JARs provided with Native Android Toolkit)
        foreach (string str in deletedAssets)
            if (str.Contains("MTAssets NAT AAR") == false)
                if (Path.GetExtension(str).ToLower() == ".aar" || Path.GetExtension(str).ToLower() == ".jar")
                    mustRunDependenciesResolve = true;

        //If must run the dependencies resolve, call it
        if (mustRunDependenciesResolve == true)
        {
            //Force reimport a image to create a delay before call dependencies resolver, for prevent dependencies resolver not detecting the AARs/JARs
            AssetDatabase.ImportAsset("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/ReImport.png");
            AssetDatabase.Refresh();
            //Finally, call the dependencies resolver to run
            MTAssets.NativeAndroidToolkit.Editor.LibsResolver.RunNativeAndroidToolkitDependenciesResolver(false);
        }
    }
}