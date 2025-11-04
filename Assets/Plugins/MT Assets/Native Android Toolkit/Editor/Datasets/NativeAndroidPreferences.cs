using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This script is the Dataset of the scriptable object "Preferences". This script saves Native Android Toolkit preferences.
     */

    public class NativeAndroidPreferences : ScriptableObject
    {
        public enum ModifyManifest
        {
            No,
            YesGenerateNewIfNotExists
        }
        public enum ModifyGradleProperties
        {
            No,
            YesGenerateNewIfNotExists
        }
        public enum WebviewIconsColorTheme
        {
            Light,
            Dark
        }
        public enum MapsIconsColorTheme
        {
            Light,
            Dark
        }
        public enum CameraIconsColorTheme
        {
            Light,
            Dark
        }
        public enum EnablePackageQueries
        {
            No,
            Yes
        }

        public string projectName;

        public ModifyManifest modifyAndroidManifest = ModifyManifest.No;
        public bool declareUnityPlayerActivity = true;
        public bool unityPlayerActivityIsMain = true;
        public bool skipPermissionsDialog = false;
        public ModifyGradleProperties modifyGradleProperties = ModifyGradleProperties.No;
        public bool enableDexingArtifactTransform = false;

        public bool vibratePermission = true;
        public bool accessWifiStatePermission = true;
        public bool accessNetworkStatePermission = true;
        public bool accessCoarseLocation = true;
        public bool accessFineLocation = true;
        public bool camera = true;
        public bool recordAudio = true;
        public bool queryAllPackages = true;
        public bool accessFilesAndMedia = true;
        public bool foregroundService = true;
        public bool scheduleExactAlarm = true;
        public Texture2D prwBackgroundImage = null;
        public bool prwResourcesIsValid = true;
        public Color prwTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color prwBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);

        public Texture2D iconOfNotifications = null;
        public bool iconIsValid = true;
        public bool enableAccurateNotify = true;
        public bool enableAccurateNotifyForAndroid12OrNewer = true;
        public bool forceVibrateOnNotify = true;
        public Color interactReceiverColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        public Color colorInNotifyArea = new Color(0.0f, 0.37f, 0.49f, 1.0f);
        public List<string> notificationsActions = new List<string>() { "Undefined", "ClickAction1", "ButtonAction1", "ButtonAction2" };

        public WebviewIconsColorTheme iconsColorTheme = WebviewIconsColorTheme.Light;
        public Texture2D iconWebviewBack = null;
        public Texture2D iconWebviewForward = null;
        public Texture2D iconWebviewClose = null;
        public Texture2D iconWebviewError = null;
        public Texture2D iconWebviewHome = null;
        public Texture2D iconWebviewFavicon = null;
        public Texture2D iconWebviewRefresh = null;
        public bool webviewIconsIsValid = true;
        public Color webviewTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color webviewBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);

        public Texture2D restartBackgroundImage = null;
        public bool utilsResourcesIsValid = true;

        public string googleMapsApiKey = "";
        public MapsIconsColorTheme mapsIconsColorTheme = MapsIconsColorTheme.Light;
        public Texture2D iconMapsClose = null;
        public Texture2D iconMapsFavicon = null;
        public Texture2D iconMapsMarker1 = null;
        public Texture2D iconMapsMarker2 = null;
        public Texture2D iconMapsMarker3 = null;
        public Texture2D iconMapsMarker4 = null;
        public Texture2D iconMapsMarker5 = null;
        public Texture2D iconMapsMarker6 = null;
        public Texture2D iconMapsMarker7 = null;
        public Texture2D iconMapsMarker8 = null;
        public Texture2D iconMapsMarker9 = null;
        public Texture2D iconMapsMarker10 = null;
        public bool mapsIconsIsValid = true;
        public Color mapsTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color mapsBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);

        public CameraIconsColorTheme cameraIconsColorTheme = CameraIconsColorTheme.Light;
        public Texture2D iconCameraClose = null;
        public Texture2D iconCameraFavicon = null;
        public Texture2D iconCameraLightOff = null;
        public Texture2D iconCameraLightOn = null;
        public Texture2D iconCameraStartRec = null;
        public Texture2D iconCameraStopRec = null;
        public Texture2D iconCameraSwitch = null;
        public Texture2D iconCameraTakePhoto = null;
        public bool cameraIconsIsValid = true;
        public Color cameraTitleColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color cameraBackgroundColor = new Color(0.0f, 0.6f, 0.8f, 1.0f);

        public EnablePackageQueries enableManifestPackageQueries = EnablePackageQueries.No;

        public List<string> manifestQueriesVisiblePackages = new List<string>() { "com.google.market", "com.android.vending", "com.samsung.android.lool", "com.huawei.systemmanager", "com.miui.securitycenter", "com.asus.mobilemanager", "com.coloros.safecenter", "com.vivo.permissionmanagerr" };

        public List<string> manifestNtpServers = new List<string>() { "time.google.com", "pool.ntp.org", "time.cloudflare.com", "time.facebook.com", "time.windows.com" };
        public bool ntpServersIsValid = true;

        public Texture2D filePickerBackgroundImage = null;
        public bool filesResourcesIsValid = true;

        public int delayBetweenStepsTask = 500;
        public bool enableAccurateTask = true;
        public bool enableAccurateTaskForAndroid12OrNewer = true;
        public bool enableNotifyTaskProgress = true;
        public string notifyTaskProgressTitle = "Running Task";
        public bool logTaskCrashToFile = false;
        public int maxLoopIterationsInTask = 10;
        public List<string> taskSourceCodeLines = new List<string>();
        public string taskSourceCodeLastSave = "";

        public string playGamesAppId = "";
        public string playGamesPackageName = "";
        public Texture2D playGamesBackgroundImage = null;
        public bool playGamesResourcesIsValid = true;
        public string playGamesXmlResources = "";
        public bool playGamesXmlResourcesSyntaxOk = true;
    }
}