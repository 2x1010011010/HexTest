using UnityEngine;
using UnityEditor;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This script only store the current product name that is owner of this copy of NAT. This way, the NAT can know
     * if it is being updated by unity package importer. The variable "productOwnerOfThisNATCopy" will be overwrited in every time that NAT is updated.
     */

    public class ProductNameOwnerOfThisNATCopy
    {
        public static string productOwnerOfThisNATCopy = "HexTest";
    }
}