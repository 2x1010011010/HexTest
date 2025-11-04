using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class will be runned before "PrefsApplier" run. This will check if the current project is MT Assets. If not, this
     * will run the "Dependencies Resolver" of NAT because the NAT possibily is being updated.
     *
     * This script will be runned if the NAT is being updated or overwrited by the Unity Package Importer/Asset Store...
     */

    public class PostUpdate
    {
        public static void RunThePostUpdateBehaviorForThisCopyOfNativeAndroidToolkit()
        {
            //If the Dependencies Resolver already runned in this project, and the NAT is being updated, force to run the Dependencies Resolver again to resolve all possible dependencies changes
            if (File.Exists("Assets/Plugins/MT Assets/_AssetsData/Editor/NATCoreLastResolverRun.ini") == true)
            {
                Debug.LogWarning("NAT: It looks like Native Android Toolkit is being updated by Unity Package Importer or Asset Store. Running Dependencies Resolver to complete possible dependencies updates...");
                LibsResolver.RunNativeAndroidToolkitDependenciesResolver(false);
            }

            //Inform the conclusion
            Debug.Log("NAT: The post update tasks for this copy of the Native Android Toolkit have been completed and the Native Android Toolkit has been updated.");
        }
    }
}