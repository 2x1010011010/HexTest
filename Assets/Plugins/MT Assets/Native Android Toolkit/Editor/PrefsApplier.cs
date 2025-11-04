using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class is responsible for detecting if NAT Core.aar was modified, and request the apply of preferences for this
     */

    [InitializeOnLoad]
    public class PrefsApplier
    {
        static PrefsApplier()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += DetectIfNatCoreAarWasModifiedAndPromptyToApplyCurrentPreferences;
        }

        public static void DetectIfNatCoreAarWasModifiedAndPromptyToApplyCurrentPreferences()
        {
            //Check if the NAT is being updated/overwrited by the Unity Package Importer or Asset Store and run the Post Update behavior before all, if is necessary.
            CheckIfTheNATIsBeingUpdatedAndIsNecessaryRunThePostUpdateOfNativeAndroidToolkit();

            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets"))
                AssetDatabase.CreateFolder("Assets/Plugins", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/MT Assets/_AssetsData/Editor"))
                AssetDatabase.CreateFolder("Assets/Plugins/MT Assets/_AssetsData", "Editor");

            //Get modify date of Nat Core.aar
            string lastModifyDate = File.GetLastWriteTimeUtc("Assets/Plugins/MT Assets/Native Android Toolkit/Libraries/MTAssets NAT AAR/NAT Core.aar").ToString();

            //If the file not exists, create then with current modified date to ignore the first time on inport
            if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastModifyDate.ini", typeof(object)) == null)
                File.WriteAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastModifyDate.ini", lastModifyDate);

            //Get last modify date done by preferences window
            string lastModifyDateByPreferences = File.ReadAllText("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastModifyDate.ini");

            //Verify if modyfied date is different from the last modyfied date by preferences window
            if (lastModifyDate != lastModifyDateByPreferences)
                if (EditorUtility.DisplayDialog("Native Android Toolkit", "The Native Android Toolkit has detected that the Native Code Library has been updated or modified externally, so it is necessary for your preferences to be applied again for everything to work. Do you want to apply now?", "Apply", "Ignore") == true)
                    Preferences.OpenWindow(true);
        }

        public static void CheckIfTheNATIsBeingUpdatedAndIsNecessaryRunThePostUpdateOfNativeAndroidToolkit()
        {
            //If the product name of the file "ProductNameOwnerOfThisNATCopy" is different of product name of this project, run the PostUpdate behavior and update the product name of the "ProductNameOwnerOfThisNATCopy"
            if (ProductNameOwnerOfThisNATCopy.productOwnerOfThisNATCopy != Application.productName)
            {
                //Run the PostUpdate behavior
                PostUpdate.RunThePostUpdateBehaviorForThisCopyOfNativeAndroidToolkit();

                //Update the product name of the script, to inform that the NAT is updated. The product name of the script will be overwrited only in the next update of this NAT copy
                File.WriteAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/PostUpdateInfo/ProductNameOwnerOfThisNATCopy.cs", File.ReadAllText("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/PostUpdateInfo/ProductNameOwnerOfThisNATCopy.txt").Replace("%PRODUCT_NAME%", Application.productName));

                //Force the update of asset database
                AssetDatabase.Refresh();
            }
        }
    }
}