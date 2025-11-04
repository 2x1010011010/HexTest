using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace MTAssets.NativeAndroidToolkit
{
    /*
      This class is responsible for the functioning of the "Native Android Toolkit"
    */
    /*
     * The Native Android Toolkit was developed by Marcos Tomaz in 2021.
     * Need help? Contact me (mtassets@windsoft.xyz)
     */

    public class NativeAndroidToolkit
    {
        /*
        * This class store base information about Native Android Toolkit. Informations stored here
        * can be used by all subclasses from Native Android Toolkit.
        */

        //Private classes

        public class InitBaseParemeters
        {
            /*
            * This class stores references, informations and variables to return to other scripts
            * by method "GetNativeAndroidToolkitInitBaseParameters".
            */

            public GameObject generatedDataGameObject = null;
            public NativeAndroidToolkitDataHandler generatedDataHandler = null;
            public AndroidJavaObject unityPlayerJavaClass = null;
            public AndroidJavaObject unityPlayerActivity = null;
            public AndroidJavaObject unityPlayerContext = null;
        }

        //Private variables

        private static GameObject generatedDataGameObject = null;
        private static NativeAndroidToolkitDataHandler generatedDataHandler = null;
        private static AndroidJavaObject unityPlayerJavaClass = null;
        private static AndroidJavaObject unityPlayerActivity = null;
        private static AndroidJavaObject unityPlayerContext = null;

        //Public variables

        public static bool isInitialized = false;

        //Core Methods

        public static void Initialize()
        {
            //If is not a Android device, and not a Editor, don't initialize the NAT
            if (Application.platform == RuntimePlatform.Android == false && Application.isEditor == false)
            {
                Debug.LogError("Unable to initialize Native Android Toolkit. This is not a platform supported by the tool. NAT only supports Unity Editor and Android.");
                return;
            }
            //If Native Android Toolkit is already initialized, ignore this
            if (isInitialized == true)
            {
                Debug.LogWarning("Native Android Toolkit has already been initialized in this session! This initialization attempt was ignored.");
                return;
            }

            //Continue to initialization.. Create the NAT Data Bridge and install the DataHandler on it
            generatedDataGameObject = new GameObject("NAT Data Bridge");
            generatedDataGameObject.transform.position = Vector3.zero;
            generatedDataGameObject.transform.eulerAngles = Vector3.zero;
            generatedDataHandler = generatedDataGameObject.AddComponent<NativeAndroidToolkitDataHandler>();
            GameObject.DontDestroyOnLoad(generatedDataGameObject);

            //Create the emulated Android UI if NAT was initialized in an Unity Editor and do other tasks
            if (Application.isEditor == true)
            {
                generatedDataHandler.emulatedAndroidInterface.CreateFullInterfaceNow(generatedDataGameObject);
                isInitialized = true;

                //Run the post initialization tasks of Native Android Toolkit WITH SUCCESS
                RunSuccessfullyPostInitializationTasks();
            }

            //If is in Android, Initialize the NAT AAR Java side
            if (Application.platform == RuntimePlatform.Android == true)
            {
                //Get current activity
                unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityPlayerActivity = unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

                //Get current context
                unityPlayerContext = unityPlayerActivity.Call<AndroidJavaObject>("getApplicationContext");

                //Start the plugin
                bool isNativeLibInitialized = false;
                AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AAR.STATIC.GLOBAL");
                isNativeLibInitialized = javaClass.CallStatic<bool>("InitializeThisLib", unityPlayerContext);

                //If initialization of native Java Library is success, inform
                if (isNativeLibInitialized == true)
                {
                    isInitialized = true;
                    Debug.Log("Native Android Toolkit was successfully initialized! (Native Side)");

                    //Run the post initialization tasks of Native Android Toolkit WITH SUCCESS
                    RunSuccessfullyPostInitializationTasks();
                }
                //If occur error on initialize native Java library, inform
                if (isNativeLibInitialized == false)
                {
                    isInitialized = false;
                    Debug.LogError("An error occurred while Initializing NAT native Java code. The vast majority of functions will not work.");

                    //Run the post initialization tasks of Native Android Toolkit WITH ERRROR
                    RunUnsuccessfulPostInitializationTasks();
                }
            }

            //Inform the intitialization
            if (isInitialized == true)
                Debug.Log("Native Android Toolkit was successfully initialized! (Unity Side)" + ((Application.isEditor == true) ? " The Emulated Android Interface has also been created for your Unity Editor." : ""));
        }

        private static void RunSuccessfullyPostInitializationTasks()
        {
            //This method runs all post initialization tasks of Native Android Toolkit and call post initialization events

            //--------- NAT General ---------//

            //Create the NAT directory in persistent data path, if not exists
            Directory.CreateDirectory(Application.persistentDataPath + "/NAT");

            //Call the event to shows that NAT was initialized successfully
            if (NATEvents.onInitializeNAT_PostInitialize != null)
                NATEvents.onInitializeNAT_PostInitialize(true);
            //Reset all events after interact with this
            NATEvents.onInitializeNAT_PostInitialize = null;

            //--------- NOTIFICATIONS ---------//

            //Call the event "onOpenApplicationByNotificationIteraction_PostInitialize" if necessary
            bool notifyFileExists = File.Exists(Application.persistentDataPath + "/NAT/notify.nat");
            if (notifyFileExists == true)
                if (String.IsNullOrEmpty(System.IO.File.ReadAllText(Application.persistentDataPath + "/NAT/notify.nat")) == false) //<- If the file is not empty, call the event
                {
                    try
                    {
                        //Get content of notify.dat and clear the file
                        string notifyFileContent = System.IO.File.ReadAllText(Application.persistentDataPath + "/NAT/notify.nat");
                        File.WriteAllText(Application.persistentDataPath + "/NAT/notify.nat", "");

                        //Try to get the corresponding Enum
                        NotificationsActions.Action action = (NotificationsActions.Action)Enum.Parse(typeof(NotificationsActions.Action), notifyFileContent);

                        //Call the event
                        if (NATEvents.onOpenApplicationByNotificationIteraction_PostInitialize != null)
                            NATEvents.onOpenApplicationByNotificationIteraction_PostInitialize(action);

                        //Reset all events after interact with this
                        NATEvents.onOpenApplicationByNotificationIteraction_PostInitialize = null;
                    }
                    catch (Exception e) { Debug.LogError(e.Message); }
                }
            if (notifyFileExists == false)
                File.WriteAllText(Application.persistentDataPath + "/NAT/notify.nat", "");

            //--------- WEBVIEW ---------//

            //Call the event "onResumeApplicationAfterCloseFullscreenWebview_PostInitialize" if necessary
            bool fullscreenWebviewFileExists = File.Exists(Application.persistentDataPath + "/NAT/webview.nat");
            if (fullscreenWebviewFileExists == true)
                if (String.IsNullOrEmpty(System.IO.File.ReadAllText(Application.persistentDataPath + "/NAT/webview.nat")) == false) //<- If the file is not empty, call the event
                {
                    //Call the event
                    if (NATEvents.onResumeApplicationAfterCloseFullscreenWebview_PostInitialize != null)
                        NATEvents.onResumeApplicationAfterCloseFullscreenWebview_PostInitialize(new NAT.Webview.WebviewBrowsing(Application.persistentDataPath + "/NAT/webview.nat"));

                    //Reset all events after interact with this
                    NATEvents.onResumeApplicationAfterCloseFullscreenWebview_PostInitialize = null;
                }
            if (fullscreenWebviewFileExists == false)
                File.WriteAllText(Application.persistentDataPath + "/NAT/webview.nat", "");

            //--------- CAMERA ---------//

            //If the camera of NAT can be safe initalized, call the method of NAT Core to initialize the NAT Camera
            if (CameraInitialization.isSafeToInitializeCamera == true && CameraInitialization.productNameThatGeneratedThisScript == Application.productName)
            {
                AndroidJavaClass cameraJavaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Camera");
                AndroidJavaObject cameraContext = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext;
                cameraJavaClass.CallStatic("InitializeTheCameraAndCheckIfDeviceSupportsCameraXFrontAndBackCamerasAndStoreInCache", cameraContext);
            }
            if (CameraInitialization.isSafeToInitializeCamera == false)
                Debug.LogWarning("Unable to initialize NAT Camera class. This is apparently not supported in the project's Gradle settings.");
            if (CameraInitialization.productNameThatGeneratedThisScript != Application.productName)
                Debug.LogWarning("Unable to initialize NAT Camera class. Apparently the current Gradle settings weren't made for this project.");

            //--------- DATE TIME ---------//

            //Call the event "onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize" if have a file with information of time of last close
            bool lastCloseFileExists = File.Exists(Application.persistentDataPath + "/NAT/last-close.alc");
            if (lastCloseFileExists == true)
            {
                //Read the last close file and get lines
                string[] lastCloseLines = File.ReadAllLines(Application.persistentDataPath + "/NAT/last-close.alc");

                //Convert the last close strings to longs to get unix millis of the last close time of app and divide by 7 to remove the salt
                long lastCloseMillisFromSystemClock = 0;
                if (lastCloseLines.Length >= 1)
                    lastCloseMillisFromSystemClock = (long.Parse(lastCloseLines[0]) / 7);

                //Create the TimeElapsedWhileClosed object
                NAT.DateTime.TimeElapsedWhileClosed timeElapsedWhileClosed = new NAT.DateTime.TimeElapsedWhileClosed();
                timeElapsedWhileClosed.timeElapsed_accordingSystemClock = new NAT.DateTime.Calendar().DecreaseThisWithDate(new NAT.DateTime.Calendar(lastCloseMillisFromSystemClock));

                //Call the event
                if (NATEvents.onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize != null)
                    NATEvents.onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize(timeElapsedWhileClosed);

                //Reset all events after interact with this
                NATEvents.onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize = null;
            }
            //Inform to Native Android Toolkit data handler, that can monitor time of last close of this app now
            generatedDataHandler.canMonitorTimeOfLastCloseForThisApp = true;

            //--------- FILES ---------//

            //Initialize the cache of the files class, and pre-load all data needed
            NAT.Files.FilesClassCache.InitializeFilesClassCache();

            //--------- PLAY GAMES ---------//

            //Call the method of NAT Core to try to initialize the NAT Play Games automatically, if can initialize
            AndroidJavaClass playGamesJavaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
            AndroidJavaObject playGamesActivity = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity;
            AndroidJavaObject playGamesContext = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext;
            playGamesJavaClass.CallStatic("InitializeGooglePlayGamesSdk", playGamesActivity, playGamesContext);
        }

        private static void RunUnsuccessfulPostInitializationTasks()
        {
            //This method runs all post initialization tasks of Native Android Toolkit on have error on initialization

            //--------- NAT General ---------//

            //Create the NAT directory in persistent data path, if not exists
            Directory.CreateDirectory(Application.persistentDataPath + "/NAT");

            //Call the event to shows that NAT was not initialized with success
            if (NATEvents.onInitializeNAT_PostInitialize != null)
                NATEvents.onInitializeNAT_PostInitialize(false);
            //Reset all events after interact with this
            NATEvents.onInitializeNAT_PostInitialize = null;
        }

        //Tools methods

        public static InitBaseParemeters GetNativeAndroidToolkitInitBaseParameters()
        {
            //Return the needed informations
            InitBaseParemeters parameters = new InitBaseParemeters();
            parameters.generatedDataGameObject = generatedDataGameObject;
            parameters.generatedDataHandler = generatedDataHandler;
            parameters.unityPlayerJavaClass = unityPlayerJavaClass;
            parameters.unityPlayerActivity = unityPlayerActivity;
            parameters.unityPlayerContext = unityPlayerContext;

            return parameters;
        }
    }

    public class NAT
    {
        /*
        * This class stores all native functions, methods and funtionalities of NAT
        */

        //This method is called by all methods inside all sublcasses to check and validate all
        public static bool canContinueToCallJavaSideMethod()
        {
            //Check if asset is initialized
            if (NativeAndroidToolkit.isInitialized == false)
            {
                Debug.LogError("Unable to perform NAT function. It looks like the Native Android Toolkit hasn't been initialized yet.");
                return false;
            }

            //Return true if all is passed
            return true;
        }

        //Subclasses for functions of Native Android Toolkit

        public class Dialogs
        {
            //Core methods

            public static void ShowSimpleAlertDialog(string title, string text, bool isCancelable)
            {
                //Calls Java Side to show a simple alert dialog

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowSimpleAlertDialog(title, text, isCancelable);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowSimpleAlertDialog", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, text, isCancelable);
                }
            }

            public static void ShowConfirmationDialog(string title, string text, bool isCancelable, string yesButton, string noButton)
            {
                //Calls Java Side to show a confirmation alert dialog

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowConfirmationDialog(title, text, isCancelable, yesButton, noButton);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowConfirmationDialog", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, text, yesButton, noButton, isCancelable);
                }
            }

            public static void ShowNeutralDialog(string title, string text, bool isCancelable, string yesButton, string noButton, string neutralButton)
            {
                //Calls Java Side to show a confirmation alert dialog with a neutral button

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowNeutralDialog(title, text, isCancelable, yesButton, noButton, neutralButton);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowNeutralDialog", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, text, yesButton, noButton, neutralButton, isCancelable);
                }
            }

            public static void ShowRadialListDialog(string title, string[] options, bool isCancelable, string doneButton, int defaultCheckedOption)
            {
                //Calls Java Side to show a radial list dialog

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowRadialListDialog(title, options, isCancelable, doneButton, defaultCheckedOption);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowRadialListDialog", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, doneButton, options, defaultCheckedOption, isCancelable);
                }
            }

            public static void ShowCheckListDialog(string title, string[] options, bool isCancelable, string doneButton, bool[] defaultCheckedOptions)
            {
                //Calls Java Side to show a check list dialog

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowCheckListDialog(title, options, isCancelable, doneButton, defaultCheckedOptions);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowCheckboxListDialog", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, doneButton, options, defaultCheckedOptions, isCancelable);
                }
            }
        }

        public class Notifications
        {
            //Subclasses

            public enum Channel
            {
                Ch_1,
                Ch_2,
                Ch_3,
                Ch_4,
                Ch_5,
                Ch_6,
                Ch_7,
                Ch_8,
                Ch_9,
                Ch_10,
                Ch_11,
                Ch_12,
                Ch_13,
                Ch_14,
                Ch_15,
                Ch_16,
                Ch_17,
                Ch_18,
                Ch_19,
                Ch_20,
            }

            public class ChannelUtils
            {
                //This class have tools to manipulate notification channels variables

                public static int ChToInt(Channel channel)
                {
                    //Will return a number corresponding to channel
                    switch (channel)
                    {
                        case Channel.Ch_1:
                            return 1;
                        case Channel.Ch_2:
                            return 2;
                        case Channel.Ch_3:
                            return 3;
                        case Channel.Ch_4:
                            return 4;
                        case Channel.Ch_5:
                            return 5;
                        case Channel.Ch_6:
                            return 6;
                        case Channel.Ch_7:
                            return 7;
                        case Channel.Ch_8:
                            return 8;
                        case Channel.Ch_9:
                            return 9;
                        case Channel.Ch_10:
                            return 10;
                        case Channel.Ch_11:
                            return 11;
                        case Channel.Ch_12:
                            return 12;
                        case Channel.Ch_13:
                            return 13;
                        case Channel.Ch_14:
                            return 14;
                        case Channel.Ch_15:
                            return 15;
                        case Channel.Ch_16:
                            return 16;
                        case Channel.Ch_17:
                            return 17;
                        case Channel.Ch_18:
                            return 18;
                        case Channel.Ch_19:
                            return 19;
                        case Channel.Ch_20:
                            return 20;
                        default: return 0;
                    }
                }

                public static Channel IntToCh(int channelId)
                {
                    //Cancel if is a invalid channel id
                    if (channelId < 1 || channelId > 20)
                    {
                        Debug.LogError("Unable to convert the integer to a Notification Channel. Please select a valid Channel (1 to 20) in the enum.");
                        return Channel.Ch_20;
                    }

                    //Will return a enum channel corresponding to number
                    switch (channelId)
                    {
                        case 1:
                            return Channel.Ch_1;
                        case 2:
                            return Channel.Ch_2;
                        case 3:
                            return Channel.Ch_3;
                        case 4:
                            return Channel.Ch_4;
                        case 5:
                            return Channel.Ch_5;
                        case 6:
                            return Channel.Ch_6;
                        case 7:
                            return Channel.Ch_7;
                        case 8:
                            return Channel.Ch_8;
                        case 9:
                            return Channel.Ch_9;
                        case 10:
                            return Channel.Ch_10;
                        case 11:
                            return Channel.Ch_11;
                        case 12:
                            return Channel.Ch_12;
                        case 13:
                            return Channel.Ch_13;
                        case 14:
                            return Channel.Ch_14;
                        case 15:
                            return Channel.Ch_15;
                        case 16:
                            return Channel.Ch_16;
                        case 17:
                            return Channel.Ch_17;
                        case 18:
                            return Channel.Ch_18;
                        case 19:
                            return Channel.Ch_19;
                        case 20:
                            return Channel.Ch_20;
                        default: return Channel.Ch_20;
                    }
                }
            }

            public class ScheduledInfo
            {
                //This class stores information about a scheduled notification. Is used to return data in method isNotificationScheduledInChannel.

                public bool isScheduledInThisChannel = false;
                public bool isRepetitiveNotification = false;

                public ScheduledInfo(bool isScheduledInThisChannel, bool isRepetitiveNotification)
                {
                    this.isScheduledInThisChannel = isScheduledInThisChannel;
                    this.isRepetitiveNotification = isRepetitiveNotification;
                }
            }

            public enum IntervalType
            {
                //This enum stores interval types for repetitive notifications

                Minutes,
                Hours,
                Days
            }

            //Core methods

            public static void ShowToast(string text, bool longDuration)
            {
                //Calls Java Side to show a Toast notification

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast(text, longDuration);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("ShowToast", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, text, longDuration);
                }
            }

            public static void SendNotification(string title, string text)
            {
                //Calls Java Side to send a local notification

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.SendNotification(title, text, "", "", "", "", "");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("SendNotification", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, title, text, "", "", "", "", "");
                }
            }

            public static ScheduledInfo isNotificationScheduledInChannel(Channel channel)
            {
                //Calls Java Side to inform if have a scheduled notification in this channel, if is a repetitive notification

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return null;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isNotificationScheduledInChannel(channel);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    string result = javaClass.CallStatic<string>("isNotificationScheduledInChannel", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, ChannelUtils.ChToInt(channel));
                    bool haveNotification = (result.Split(',')[0] == "true") ? true : false;
                    bool isRepetitive = (result.Split(',')[1] == "true") ? true : false;
                    return new ScheduledInfo(haveNotification, isRepetitive);
                }

                //Return null if not run nothing
                return null;
            }

            public class ScheduledNotification
            {
                //This class is a Builder for construct a new scheduled notification, defining time in future

                //Data for a normal notification
                private Channel channel;
                private string title;
                private string text;
                private int years;
                private int months;
                private int days;
                private int hours;
                private int minutes;
                private string onClickAction = "";
                private string button1Txt = "";
                private string onClickButton1Action = "";
                private string button2Txt = "";
                private string onClickButton2Action = "";
                //Data for a repetitive notification
                private bool isRepetitive;
                private IntervalType intervalType;
                private int interval;

                public ScheduledNotification(Channel channel, string title, string text)
                {
                    this.channel = channel;
                    this.title = title;
                    this.text = text;
                    this.years = 0;
                    this.months = 0;
                    this.days = 0;
                    this.hours = 0;
                    this.minutes = 0;
                }

                public ScheduledNotification setYearsInFuture(int years)
                {
                    this.years = years;
                    return this;
                }

                public ScheduledNotification setMonthsInFuture(int months)
                {
                    this.months = months;
                    return this;
                }

                public ScheduledNotification setDaysInFuture(int days)
                {
                    this.days = days;
                    return this;
                }

                public ScheduledNotification setHoursInFuture(int hours)
                {
                    this.hours = hours;
                    return this;
                }

                public ScheduledNotification setMinutesInFuture(int minutes)
                {
                    this.minutes = minutes;
                    return this;
                }

                public ScheduledNotification setAsRepetitiveNotification(IntervalType intervalType, int interval)
                {
                    this.isRepetitive = true;
                    this.intervalType = intervalType;
                    this.interval = interval;
                    return this;
                }

                public ScheduledNotification setOnClickAction(NotificationsActions.Action notificationAction)
                {
                    this.onClickAction = notificationAction.ToString();
                    return this;
                }

                public ScheduledNotification addButton1(string buttonText, NotificationsActions.Action buttonAction)
                {
                    this.button1Txt = buttonText;
                    this.onClickButton1Action = buttonAction.ToString();
                    return this;
                }

                public ScheduledNotification addButton2(string buttonText, NotificationsActions.Action buttonAction)
                {
                    this.button2Txt = buttonText;
                    this.onClickButton2Action = buttonAction.ToString();
                    return this;
                }

                //Calls native code to schedule notification in future
                public void ScheduleThisNotification()
                {
                    //Calls Java Side to schedule a notification to future

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ScheduleNotification(channel, title, text, years, months, days, hours, minutes, isRepetitive, intervalType, interval,
                                                                                                                                                            onClickAction, button1Txt, onClickButton1Action, button2Txt, onClickButton2Action);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                        AndroidJavaObject context = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext;
                        //If is a repetitive notification
                        if (isRepetitive == true)
                        {
                            int intervalTypeId = -1;
                            if (intervalType == IntervalType.Minutes)
                                intervalTypeId = 0;
                            if (intervalType == IntervalType.Hours)
                                intervalTypeId = 1;
                            if (intervalType == IntervalType.Days)
                                intervalTypeId = 2;

                            javaClass.CallStatic("ScheduleNotificationToFuture", context, ChannelUtils.ChToInt(channel), title, text, years, months, days, hours, minutes, true, intervalTypeId, interval,
                                                                                          onClickAction, button1Txt, onClickButton1Action, button2Txt, onClickButton2Action);
                        }
                        //If is not a repetitive notification
                        if (isRepetitive == false)
                            javaClass.CallStatic("ScheduleNotificationToFuture", context, ChannelUtils.ChToInt(channel), title, text, years, months, days, hours, minutes, false, -1, -1,
                                                                                          onClickAction, button1Txt, onClickButton1Action, button2Txt, onClickButton2Action);
                    }
                }
            }

            public static void CancelScheduledNotification(Channel channel)
            {
                //Calls Java Side to cancel a scheduled notification in a channel

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.CancelScheduledNotification(channel);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("CancelScheduledNotification", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, ChannelUtils.ChToInt(channel));
                }
            }

            public static int[] GetListOfFreeNotificationsChannels()
            {
                //Calls Java Side to list all free notifications channels

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new int[0];

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.GetListOfFreeNotificationsChannels();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    string result = javaClass.CallStatic<string>("GetListOfFreeNotificationsChannels", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                    if (string.IsNullOrEmpty(result) == true)
                        return new int[0];
                    string[] resultSplitted = result.Split(',');
                    List<int> allFreeChannels = new List<int>();
                    foreach (string ch in resultSplitted)
                        allFreeChannels.Add(int.Parse(ch));
                    return allFreeChannels.ToArray();
                }

                //Return null if not run nothing
                return new int[0];
            }
        }

        public class Sharing
        {
            //Submethods

            private static IEnumerator TakeScreenshotAndCallTheEventToPassTheTexture2dOfScreenshot()
            {
                //Create the NAT directory, if not exists
                Directory.CreateDirectory(Application.persistentDataPath + "/NAT");

                //Take a screenshot
                string pathOfScreenshot = Application.persistentDataPath + "/NAT/screenshot.png";
                if (Application.isEditor == false)
                    ScreenCapture.CaptureScreenshot("NAT/screenshot.png");
                if (Application.isEditor == true)
                    ScreenCapture.CaptureScreenshot(pathOfScreenshot);

                //Wait a time to png of screenshot be ready in memory
                yield return new WaitForSecondsRealtime(1.5f);

                //Read the texture from memory
                byte[] data = File.ReadAllBytes(pathOfScreenshot);
                Texture2D texture2D = new Texture2D(Screen.width, Screen.height);
                texture2D.LoadImage(data);

                //Call the event for return texture, and clear all registrations
                if (NATEvents.onCompleteScreenshotTexture2dProcessing != null)
                    NATEvents.onCompleteScreenshotTexture2dProcessing(texture2D);
                NATEvents.onCompleteScreenshotTexture2dProcessing = null;
            }

            //Core methods

            public static void ShareTexture2D(string titleOfShareBox, Texture2D texture2D, string messageOfShare)
            {
                //Calls Java Side to share a Texture2D to some other application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShareTexture2D(texture2D, messageOfShare);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("ShareTexture2D", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, titleOfShareBox, messageOfShare, texture2D.EncodeToPNG());
                }
            }

            public static void ShareTextPlain(string titleOfShareBox, string textToShare)
            {
                //Calls Java Side to share a text plain to some other application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShareTextPlain(textToShare);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("ShareTextPlain", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, titleOfShareBox, textToShare);
                }
            }

            public static void CopyTextToClipboard(string textToCopy)
            {
                //Calls Java Side to copy some text to clipboard of device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.CopyTextToClipboard(textToCopy);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("CopyTextToClipBoard", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, textToCopy);
                }
            }

            public static string GetTextFromClipboard()
            {
                //Calls Java Side to get current text of clipboard of device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.GetTextFromClipboard();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    return javaClass.CallStatic<string>("GetTextFromClipBoard", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return empty if not run nothing
                return "";
            }

            public static void TakeScreenshotAndGetTexture2D()
            {
                //Calls Java Side and NAT to take screenshot and get the texture2d of screenshot in a event callback

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(TakeScreenshotAndCallTheEventToPassTheTexture2dOfScreenshot());
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(TakeScreenshotAndCallTheEventToPassTheTexture2dOfScreenshot());
                }
            }
        }

        public class Webview
        {
            //Subclasses

            public enum WebviewMode
            {
                AdaptativePopUp,
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public class WebviewBrowsing
            {
                //This class will store last webview browsing data

                public string webviewType;
                public string[] browsedSites;

                public WebviewBrowsing(string webviewFilePath)
                {
                    //Get content of file "webview.nat" and fill this class instance
                    string[] webviewFileContent = System.IO.File.ReadAllLines(webviewFilePath);

                    //Fill this class
                    webviewType = webviewFileContent[0];
                    List<string> fileBrowsedSites = new List<string>();
                    for (int i = 0; i < webviewFileContent.Length; i++)
                        if (i > 0)
                            fileBrowsedSites.Add(webviewFileContent[i]);
                    browsedSites = fileBrowsedSites.ToArray();

                    //Clear the webview file
                    File.WriteAllText(Application.persistentDataPath + "/NAT/webview.nat", "");
                }
            }

            public class WebviewCookie
            {
                //This class stores data for a Webview cookie
                public string name = "";
                public string content = "";
                public string domain = "";

                public WebviewCookie(string name, string content, string domain)
                {
                    this.name = name;
                    this.content = content;
                    this.domain = domain;
                }
            }

            public class WebviewPostField
            {
                //This class stores data for a post field
                public string fieldName = "";
                public string value = "";

                public WebviewPostField(string fieldName, string value)
                {
                    this.fieldName = fieldName;
                    this.value = value;
                }
            }

            public class WebviewPostData
            {
                //This class stores post data to be used in HTTP request
                public List<WebviewPostField> allPostData = new List<WebviewPostField>();

                public WebviewPostData addField(string fieldName, string fieldValue)
                {
                    allPostData.Add(new WebviewPostField(fieldName, fieldValue));
                    return this;
                }

                public WebviewPostField[] CloseAndGetPostDataArray()
                {
                    return allPostData.ToArray();
                }
            }

            public enum WebviewAccessMethod
            {
                Post,
                Get
            }

            public enum WebviewClearCacheMode
            {
                OnlyWebview,
                OnlyUnityWebRequest
            }

            private static IEnumerator AccessTheURLWithPostDataAndGetAllCookies(WebviewAccessMethod accessMethod, string pageUrl, string pageDomain, WebviewPostField[] webviewPostFields)
            {
                //Create the form to store the post data
                WWWForm form = new WWWForm();
                foreach (WebviewPostField field in webviewPostFields)
                    form.AddField(field.fieldName, field.value);

                //Send the post data to page
                using (UnityWebRequest www = ((accessMethod == WebviewAccessMethod.Post) ? UnityWebRequest.Post(pageUrl, form) : UnityWebRequest.Get(pageUrl)))
                {
                    //Set up the connection
                    www.SetRequestHeader("X-Requested-With", "XMLHttpRequest");
                    //Force unity to clear cache of this domain, to guarantee providation of cookies
                    UnityWebRequest.ClearCookieCache(new Uri(pageDomain));
                    //Start the connection
                    yield return www.SendWebRequest();

                    //If not is completed, wait
                    while (www.isDone == false)
                        yield return new WaitForSeconds(1.0f);

                    //If the request was done successfully
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        //Prepare the store for cookies
                        List<WebviewCookie> allCookies = new List<WebviewCookie>();

                        //Prepare a array of cookies attributes that is not name=value
                        string[] attributesThatNotIsCookieName = new string[] { "domain", "path", "expires", "id", "max-age", "samesite", "secure", "httponly", "custom", "interval", "custom-attribute" };

                        //Handle the Set-Cookie in response header and add to all cookies list
                        foreach (System.Collections.Generic.KeyValuePair<string, string> dict in www.GetResponseHeaders())
                            if (dict.Key == "Set-Cookie")
                            {
                                string fullCookieFormatted = dict.Value.Replace(" ,", " ").Replace(", ", " ").Replace(" , ", " ");
                                string[] cookies = fullCookieFormatted.Split(',');
                                foreach (string cookie in cookies)
                                {
                                    string[] cookieDetails = cookie.Split(';');
                                    string cookieName = "";
                                    string cookieContent = "";
                                    string cookieDomain = "";
                                    foreach (string detail in cookieDetails)
                                        if (detail.Contains("=") == true)
                                        {
                                            string detailAttribute = detail.Split('=')[0].Replace(" ", "");
                                            string detailValue = detail.Split('=')[1].Replace(" ", "");

                                            if (detailAttribute.ToLower() == "domain")
                                            {
                                                cookieDomain = detailValue;
                                                continue;
                                            }
                                            if (attributesThatNotIsCookieName.Any(detailAttribute.ToLower().Equals) == false)
                                            {
                                                cookieName = detailAttribute;
                                                cookieContent = detailValue;
                                                continue;
                                            }
                                        }
                                    WebviewCookie webviewCookie = new WebviewCookie(cookieName, cookieContent, cookieDomain);
                                    allCookies.Add(webviewCookie);
                                }
                            }

                        //Call the event for return all cookies, and clear all registrations
                        if (NATEvents.onWebviewGettedAllCookiesFromURL != null)
                            NATEvents.onWebviewGettedAllCookiesFromURL(true, allCookies.ToArray(), www.downloadHandler.text);
                        NATEvents.onWebviewGettedAllCookiesFromURL = null;
                    }
                    //If have error on request
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        //Call the event for return all cookies, and clear all registrations
                        if (NATEvents.onWebviewGettedAllCookiesFromURL != null)
                            NATEvents.onWebviewGettedAllCookiesFromURL(false, new WebviewCookie[0], "");
                        NATEvents.onWebviewGettedAllCookiesFromURL = null;
                    }
                }
            }

            //Core methods

            public class WebviewChromium
            {
                //This class is a Builder for construct a new webview chromium exibition, while setting useful paramters

                //Data for a webview exibition
                private WebviewMode webviewMode = WebviewMode.AdaptativePopUp;
                private string homeUrl = "https://www.google.com";
                private string title = "";
                private bool showToolbar = true;
                private bool hideRefreshButton = false;
                private bool showControls = true;
                private bool hideBackAndForwardButtons = false;
                private bool hideHomeButton = false;
                private string networkErrorMessage = "There was a network problem. Please check your connection and try again.";
                private string networkErrorButtonMessage = "TRY AGAIN";
                private bool enableCache = false;
                private bool enableJavaScript = true;
                private bool enableZoom = true;
                private bool enableMediaAutoPlay = true;
                private List<WebviewCookie> cookies = new List<WebviewCookie>();

                public WebviewChromium(string homeUrl, WebviewMode webviewMode)
                {
                    this.homeUrl = homeUrl;
                    this.webviewMode = webviewMode;
                }

                public WebviewChromium setTitle(string title)
                {
                    this.title = title;
                    return this;
                }

                public WebviewChromium setShowToolbar(bool showToolbar)
                {
                    this.showToolbar = showToolbar;
                    return this;
                }

                public WebviewChromium setHideRefreshButton(bool hideRefreshButton)
                {
                    this.hideRefreshButton = hideRefreshButton;
                    return this;
                }

                public WebviewChromium setShowControls(bool showControls)
                {
                    this.showControls = showControls;
                    return this;
                }

                public WebviewChromium setHideBackAndForwardButtons(bool hideBackAndForwardButtons)
                {
                    this.hideBackAndForwardButtons = hideBackAndForwardButtons;
                    return this;
                }

                public WebviewChromium setHideHomeButton(bool hideHomeButton)
                {
                    this.hideHomeButton = hideHomeButton;
                    return this;
                }

                public WebviewChromium setNetworkErrorMessage(string networkErrorMessage)
                {
                    this.networkErrorMessage = networkErrorMessage;
                    return this;
                }

                public WebviewChromium setNetworkErrorButtonMessage(string networkErrorButtonMessage)
                {
                    this.networkErrorButtonMessage = networkErrorButtonMessage;
                    return this;
                }

                public WebviewChromium setEnableCache(bool enableCache)
                {
                    this.enableCache = enableCache;
                    return this;
                }

                public WebviewChromium setEnableJavaScript(bool enableJavaScript)
                {
                    this.enableJavaScript = enableJavaScript;
                    return this;
                }

                public WebviewChromium setEnableZoom(bool enableZoom)
                {
                    this.enableZoom = enableZoom;
                    return this;
                }

                public WebviewChromium setEnableMediaAutoPlay(bool enableMediaAutoPlay)
                {
                    this.enableMediaAutoPlay = enableMediaAutoPlay;
                    return this;
                }

                public WebviewChromium addCookie(WebviewCookie cookie)
                {
                    this.cookies.Add(cookie);
                    return this;
                }

                //Calls native code to show the Webview
                public void OpenThisWebview()
                {
                    //Calls Java Side to open the webview chromium

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        if (webviewMode == WebviewMode.AdaptativePopUp)
                            NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenWebviewChromium(false);
                        if (webviewMode != WebviewMode.AdaptativePopUp)
                            NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenWebviewChromium(true);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Get webview mode id
                        int webviewModeId = -1;
                        if (webviewMode == WebviewMode.AdaptativePopUp)
                            webviewModeId = 0;
                        if (webviewMode == WebviewMode.LandscapeFullscreen)
                            webviewModeId = 1;
                        if (webviewMode == WebviewMode.PortraitFullscreen)
                            webviewModeId = 2;
                        //Build the cookies string
                        string allCookies = "";
                        if (cookies.Count > 0)
                            for (int i = 0; i < cookies.Count; i++)
                            {
                                if (i == 0)
                                    allCookies += cookies[i].domain + "!C!" + cookies[i].name + "=" + cookies[i].content;
                                if (i > 0)
                                    allCookies += "\n" + cookies[i].domain + "!C!" + cookies[i].name + "=" + cookies[i].content;
                            }

                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Webview");
                        AndroidJavaObject activity = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity;
                        javaClass.CallStatic("OpenWebview", activity, webviewModeId, homeUrl, title, showToolbar, hideRefreshButton, showControls, hideBackAndForwardButtons, hideHomeButton, networkErrorMessage, networkErrorButtonMessage,
                                                                      enableCache, enableJavaScript, enableZoom, enableMediaAutoPlay, allCookies);
                    }
                }
            }

            public static void AccessSomeURLWithPostDataAndGetAllCookies(WebviewAccessMethod accessMethod, string pageUrl, string pageDomain, WebviewPostField[] webviewPostFields)
            {
                //Calls Java Side and NAT to acces some URL while do post data, and get all cookies from this response URL

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(AccessTheURLWithPostDataAndGetAllCookies(accessMethod, pageUrl, pageDomain, webviewPostFields));
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(AccessTheURLWithPostDataAndGetAllCookies(accessMethod, pageUrl, pageDomain, webviewPostFields));
                }
            }

            public static void ClearAllCookies(WebviewClearCacheMode clearMethod)
            {
                //Calls Java Side to clear all cookies of user

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    if (clearMethod == WebviewClearCacheMode.OnlyUnityWebRequest)
                    {
                        UnityWebRequest.ClearCookieCache();
                        Debug.Log("NAT: All Cookies from Unity side was cleaned.");
                    }
                    if (clearMethod == WebviewClearCacheMode.OnlyWebview)
                        Debug.Log("NAT: All Cookies from Native side was cleaned.");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    if (clearMethod == WebviewClearCacheMode.OnlyWebview)
                    {
                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Webview");
                        javaClass.CallStatic("ClearAllWebviewCookies", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                    }
                    if (clearMethod == WebviewClearCacheMode.OnlyUnityWebRequest)
                        UnityWebRequest.ClearCookieCache();
                }
            }
        }

        public class Permissions
        {
            //Subclasses

            public enum RequesterWizardMode
            {
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public enum AndroidPermission
            {
                Camera,
                AccessCoarseLocation,
                AccessFineLocation,
                RecordAudio,
                AccessFilesAndMedia
            }

            private static string ConvertAndroidPermissionToString(AndroidPermission androidPermission)
            {
                //This method converts Android Permission to a Manifest String Code
                switch (androidPermission)
                {
                    case AndroidPermission.Camera:
                        return "android.permission.CAMERA";
                    case AndroidPermission.AccessCoarseLocation:
                        return "android.permission.ACCESS_COARSE_LOCATION";
                    case AndroidPermission.AccessFineLocation:
                        return "android.permission.ACCESS_FINE_LOCATION";
                    case AndroidPermission.RecordAudio:
                        return "android.permission.RECORD_AUDIO";
                    case AndroidPermission.AccessFilesAndMedia:
                        return ((Utils.GetDeviceAndroidVersionCode() >= 30) ? "android.permission.READ_EXTERNAL_STORAGE" : "android.permission.WRITE_EXTERNAL_STORAGE");
                }

                //Return the default
                return "";
            }

            //Core methods

            public class PermissionRequesterWizard
            {
                //This class is a Builder for open the permissions request wizard

                //Data for a permission request wizard
                public string titleBar = "NAT Permission Requester Wizard";
                public RequesterWizardMode requesterMode = RequesterWizardMode.LandscapeFullscreen;
                public string waitingPermissionMessage = "(Waiting for you to allow)";
                public string givenPermissionMessage = "(Permission granted!)";
                public string warningMessage = "This app needs you to provide some permissions for it to work properly. You can see each of the required permissions here and a short explanation of why it is needed.";
                public string givePermissionButtonMessage = "Allow";
                public string doneButtonMessage = "Done, go back to the app!";
                List<string> listOfPermissionsNameToRequest = new List<string>();
                List<string> listOfPermissionsToRequest = new List<string>();
                List<string> listOfPermissionsExplanations = new List<string>();

                public PermissionRequesterWizard(string titleBar, RequesterWizardMode requesterWizardMode)
                {
                    this.titleBar = titleBar;
                    this.requesterMode = requesterWizardMode;
                }

                public PermissionRequesterWizard setWaitingAndGivenPermissionMessage(string waitingPermissionMessage, string givenPermissionMessage)
                {
                    this.waitingPermissionMessage = waitingPermissionMessage;
                    this.givenPermissionMessage = givenPermissionMessage;
                    return this;
                }

                public PermissionRequesterWizard setWarningMessage(string warningMessage)
                {
                    this.warningMessage = warningMessage;
                    return this;
                }

                public PermissionRequesterWizard setGivePermissionButtonMessage(string givePermissionButtonMessage)
                {
                    this.givePermissionButtonMessage = givePermissionButtonMessage;
                    return this;
                }

                public PermissionRequesterWizard setDoneButtonMessage(string doneButtonMessage)
                {
                    this.doneButtonMessage = doneButtonMessage;
                    return this;
                }

                public PermissionRequesterWizard addPermissionToRequest(string permissionName, AndroidPermission androidPermission, string permissionExplanation)
                {
                    this.listOfPermissionsNameToRequest.Add(permissionName);
                    this.listOfPermissionsToRequest.Add(ConvertAndroidPermissionToString(androidPermission));
                    this.listOfPermissionsExplanations.Add(permissionExplanation);
                    return this;
                }

                //Calls native code to open the permission requester wizard
                public void OpenThisPermissionRequesterWizard()
                {
                    //Calls Java Side to request all desired permissions with permission requester wizard

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        //Build the permissions string
                        string permissionsString = "";
                        //Fill the string
                        for (int i = 0; i < listOfPermissionsToRequest.Count; i++)
                        {
                            if (i > 0)
                                permissionsString += ",";
                            permissionsString += listOfPermissionsToRequest[i];
                        }

                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Permission Requester Wizard (Simulation)\nThe permissions below are being requested from the user...\n\n" + permissionsString.Replace(",", "\n"));
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Get webview mode id
                        int wizardMode = -1;
                        if (requesterMode == RequesterWizardMode.LandscapeFullscreen)
                            wizardMode = 0;
                        if (requesterMode == RequesterWizardMode.PortraitFullscreen)
                            wizardMode = 1;
                        //Build the permissions string
                        string permissionsNameString = "";
                        string permissionsString = "";
                        string explanationsString = "";
                        //Fill the string
                        for (int i = 0; i < listOfPermissionsNameToRequest.Count; i++)
                        {
                            if (i > 0)
                                permissionsNameString += "!N!";
                            permissionsNameString += listOfPermissionsNameToRequest[i];
                        }
                        for (int i = 0; i < listOfPermissionsToRequest.Count; i++)
                        {
                            if (i > 0)
                                permissionsString += ",";
                            permissionsString += listOfPermissionsToRequest[i];
                        }
                        for (int i = 0; i < listOfPermissionsExplanations.Count; i++)
                        {
                            if (i > 0)
                                explanationsString += "!E!";
                            explanationsString += listOfPermissionsExplanations[i];
                        }

                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Permissions");
                        AndroidJavaObject activity = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity;
                        javaClass.CallStatic("OpenPermissionRequesterWizard", activity, wizardMode, titleBar, waitingPermissionMessage, givenPermissionMessage, warningMessage, givePermissionButtonMessage, doneButtonMessage,
                                                                              permissionsNameString, permissionsString, explanationsString);
                    }
                }
            }

            public class PermissionRequester
            {
                //This class is a Builder for construct a permission request dialog

                //Data for a permission request
                List<string> listOfPermissionsToRequest = new List<string>();
                //Data for the wizard

                public PermissionRequester()
                {
                    //Empty initial builder
                }

                public PermissionRequester addPermissionToRequest(AndroidPermission androidPermission)
                {
                    this.listOfPermissionsToRequest.Add(ConvertAndroidPermissionToString(androidPermission));
                    return this;
                }

                //Calls native code to request permission
                public void RequestThisPermissions()
                {
                    //Calls Java Side to request all desired permissions

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        //Build the permissions string
                        string permissionsString = "";
                        //Fill the string
                        for (int i = 0; i < listOfPermissionsToRequest.Count; i++)
                        {
                            if (i > 0)
                                permissionsString += ",";
                            permissionsString += listOfPermissionsToRequest[i];
                        }

                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Requesting the following permissions: [" + permissionsString + "]", true);
                        Debug.Log("NAT: Click on this Log to see the list of permissions requested in this request.\n\nRequested permissions is...\n" + permissionsString.Replace(",", "\n") + "\n\n");
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Build the permissions string
                        string permissionsString = "";
                        //Fill the string
                        for (int i = 0; i < listOfPermissionsToRequest.Count; i++)
                        {
                            if (i > 0)
                                permissionsString += ",";
                            permissionsString += listOfPermissionsToRequest[i];
                        }

                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Permissions");
                        javaClass.CallStatic("RequestPermissions", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, permissionsString);
                    }
                }
            }

            public static bool isPermissionGuaranteed(AndroidPermission androidPermission)
            {
                //Calls Java Side to return if the desired permission is guaranteed

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Permissions");
                    return javaClass.CallStatic<bool>("isPermissionGuaranteed", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, ConvertAndroidPermissionToString(androidPermission));
                }

                //Return empty if not run nothing
                return true;
            }
        }

        public class Utils
        {
            //Subclasses

            public enum RestartInterfaceMode
            {
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public enum TTSEngineLanguage
            {
                DeviceDefault,
                English,
                Chinese,
                Hindi,
                Spanish,
                French,
                Arabic,
                Bengali,
                Russian,
                Portuguese,
                Indonesian
            }

            public enum TTSEngineQueueMode
            {
                AddToQueue,
                FlushQueueAndAdd
            }

            public enum LocaleType
            {
                ISO3Country,
                Country,
                LanguageName,
                LanguageTag,
                ISO3Language,
                CurrencyCode
            }

            public class CanvasSize
            {
                //This class stores size for canvas
                public float unitsWidth = 0;
                public float unitsHeight = 0;

                public CanvasSize(float unitsWidth, float unitsHeight)
                {
                    this.unitsWidth = unitsWidth;
                    this.unitsHeight = unitsHeight;
                }
            }

            private class VibratePlusCache
            {
                //This class store cache references for the VibratePlus method
                public static IntPtr javaClassRaw = IntPtr.Zero;
                public static string vibrateMethodSignature = "";
                public static IntPtr vibrateMethod = IntPtr.Zero;
                public static jvalue[] argumentList = null;
            }

            public class DeviceNotchSize
            {
                //This class stores a notch size
                public int rightPixelsSize = 0;
                public int leftPixelsSize = 0;
                public int topPixelsSize = 0;
                public int bottomPixelsSize = 0;
            }

            //Core methods

            public static void RestartApplication(string restartingMessage, RestartInterfaceMode restartInterfaceMode)
            {
                //Calls Java Side to restar the application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.RestartApplication(restartingMessage);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get restarter mode id
                    int restarterMode = -1;
                    if (restartInterfaceMode == RestartInterfaceMode.LandscapeFullscreen)
                        restarterMode = 0;
                    if (restartInterfaceMode == RestartInterfaceMode.PortraitFullscreen)
                        restarterMode = 1;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("RestartApplication", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, restarterMode, restartingMessage);
                }
            }

            public static void Vibrate(long durationMs)
            {
                //Calls Java Side to vibrate the device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Vibrating device in " + durationMs + "ms", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("Vibrate", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, durationMs);
                }
            }

            public static void VibrateWithPattern(long[] durationPatternMs)
            {
                //Calls Java Side to vibrate the device with pattern

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    string pattern = "";
                    foreach (long millis in durationPatternMs)
                        pattern += millis.ToString() + ",";
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Vibrating device [" + pattern + "] pattern", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("VibrateWithPattern", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, durationPatternMs);
                }
            }

            public static string GetDeviceManufacturer()
            {
                //Calls Java Side to return the device manufacturer

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return "Unity";
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<string>("GetDeviceManufacturer");
                }

                //Return false if not run nothing
                return "";
            }

            public static string GetDeviceModel()
            {
                //Calls Java Side to return the device model

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return "Editor";
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<string>("GetDeviceModel");
                }

                //Return false if not run nothing
                return "";
            }

            public static int GetDeviceAndroidVersionCode()
            {
                //Calls Java Side to return the device android version code

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return -1;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return 19;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<int>("GetDeviceAndroidVersionCode");
                }

                //Return false if not run nothing
                return -1;
            }

            public static void SpeakWithTTS(TTSEngineLanguage languageToSpeak, string textToSpeak, TTSEngineQueueMode queueMode)
            {
                //Calls Java Side to speak with native TTS engine

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Speaking with TTS:\n\"" + textToSpeak + "\"", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");

                    int queueModeCode = 0;
                    if (queueMode == TTSEngineQueueMode.AddToQueue)
                        queueModeCode = 0;
                    if (queueMode == TTSEngineQueueMode.FlushQueueAndAdd)
                        queueModeCode = 1;
                    int laguageToSpeakCode = 0;
                    if (languageToSpeak == TTSEngineLanguage.DeviceDefault)
                        laguageToSpeakCode = 0;
                    if (languageToSpeak == TTSEngineLanguage.English)
                        laguageToSpeakCode = 1;
                    if (languageToSpeak == TTSEngineLanguage.Chinese)
                        laguageToSpeakCode = 2;
                    if (languageToSpeak == TTSEngineLanguage.Hindi)
                        laguageToSpeakCode = 3;
                    if (languageToSpeak == TTSEngineLanguage.Spanish)
                        laguageToSpeakCode = 4;
                    if (languageToSpeak == TTSEngineLanguage.French)
                        laguageToSpeakCode = 5;
                    if (languageToSpeak == TTSEngineLanguage.Arabic)
                        laguageToSpeakCode = 6;
                    if (languageToSpeak == TTSEngineLanguage.Bengali)
                        laguageToSpeakCode = 7;
                    if (languageToSpeak == TTSEngineLanguage.Russian)
                        laguageToSpeakCode = 8;
                    if (languageToSpeak == TTSEngineLanguage.Portuguese)
                        laguageToSpeakCode = 9;
                    if (languageToSpeak == TTSEngineLanguage.Indonesian)
                        laguageToSpeakCode = 10;

                    javaClass.CallStatic("SpeakWithTTS", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, laguageToSpeakCode, textToSpeak, queueModeCode);
                }
            }

            public static string GetDeviceCurrentLocale(LocaleType localeType)
            {
                //Calls Java Side to return the device locale

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //Prepare to return
                    string toReturn = "";

                    switch (localeType)
                    {
                        case LocaleType.ISO3Country:
                            toReturn = "USA";
                            break;
                        case LocaleType.Country:
                            toReturn = "United States";
                            break;
                        case LocaleType.LanguageName:
                            toReturn = "English (United States)";
                            break;
                        case LocaleType.LanguageTag:
                            toReturn = "en_US";
                            break;
                        case LocaleType.ISO3Language:
                            toReturn = "eng";
                            break;
                        case LocaleType.CurrencyCode:
                            toReturn = "USD";
                            break;
                    }

                    //Return
                    return toReturn;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Prepare the locale type
                    int localeTypeInt = -1;
                    if (localeType == LocaleType.ISO3Country)
                        localeTypeInt = 0;
                    if (localeType == LocaleType.Country)
                        localeTypeInt = 1;
                    if (localeType == LocaleType.LanguageName)
                        localeTypeInt = 2;
                    if (localeType == LocaleType.LanguageTag)
                        localeTypeInt = 3;
                    if (localeType == LocaleType.ISO3Language)
                        localeTypeInt = 4;
                    if (localeType == LocaleType.CurrencyCode)
                        localeTypeInt = 5;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<string>("GetDeviceCurrentLocale", localeTypeInt);
                }

                //Return false if not run nothing
                return "";
            }

            public static void EnableAntiScreenshot(bool enable)
            {
                //Calls Java Side to enable/disable anti screenshot

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.EnableAntiScreenshot(enable);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("EnableAntiScreenshot", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, enable);
                }
            }

            public static int ConvertDPToPixels(float dpToBeConverted)
            {
                //Calls Java Side to convert DP to pixels

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return 0;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return (int)(1.8f * dpToBeConverted);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<int>("ConvertDPToPixels", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, dpToBeConverted);
                }

                //Return empty if not run nothing
                return 0;
            }

            public static float ConvertPixelsToDP(int pixelsToBeConverted)
            {
                //Calls Java Side to convert pixels to DP

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return 0;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return (int)((float)pixelsToBeConverted / 1.8f);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<float>("ConvertPixelsToDP", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, pixelsToBeConverted);
                }

                //Return empty if not run nothing
                return 0;
            }

            public static CanvasSize ConvertPixelsSizeToCanvasSize(Canvas targetCanvas, int widthInPixels, int heightInPixels)
            {
                //Calls Java Side to convert pixels to canvas units

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new CanvasSize(0, 0);

                //Prepare to return
                CanvasSize toReturn = new CanvasSize(0, 0);

                //If is screen space, return the converted size
                if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    //Get canvas size
                    float canvasWidth = targetCanvas.GetComponent<RectTransform>().rect.width;
                    float canvasHeight = targetCanvas.GetComponent<RectTransform>().rect.height;

                    //Get screen size
                    float screenWidth = Screen.width;
                    float screenHeight = Screen.height;

                    //Calculate the proportions of the pixels to be converted
                    float proportionWidth = (float)((float)widthInPixels / (float)screenWidth);
                    float proportionHeight = (float)((float)heightInPixels / (float)screenHeight);

                    //Fill to return
                    toReturn = new CanvasSize((canvasWidth * proportionWidth), (canvasHeight * proportionHeight));
                }
                //If is not screen space, return zero size
                if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    toReturn = new CanvasSize(0, 0);

                //Return empty if not run nothing
                return toReturn;
            }

            public static void VibratePlus(long durationMs)
            {
                //Calls Java Side to vibrate the device (with a optimization to be called many times per second)

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Vibrating device in " + durationMs + "ms", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    if (VibratePlusCache.javaClassRaw == IntPtr.Zero)
                        VibratePlusCache.javaClassRaw = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils").GetRawClass();
                    if (VibratePlusCache.vibrateMethodSignature == "")
                        VibratePlusCache.vibrateMethodSignature = AndroidJNIHelper.GetSignature(new object[] { NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, durationMs });
                    if (VibratePlusCache.vibrateMethod == IntPtr.Zero)
                        VibratePlusCache.vibrateMethod = AndroidJNIHelper.GetMethodID(VibratePlusCache.javaClassRaw, "Vibrate", VibratePlusCache.vibrateMethodSignature, true);
                    if (VibratePlusCache.argumentList == null)
                    {
                        VibratePlusCache.argumentList = new jvalue[2];
                        VibratePlusCache.argumentList[0].l = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext.GetRawObject();
                    }

                    VibratePlusCache.argumentList[1].j = durationMs;  //<- "j" indicate that is a LONG type
                    AndroidJNI.CallStaticVoidMethod(VibratePlusCache.javaClassRaw, VibratePlusCache.vibrateMethod, VibratePlusCache.argumentList);
                }
            }

            public static DeviceNotchSize GetDeviceNotchPixelsSize()
            {
                //Calls Java Side to restar the application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new DeviceNotchSize();

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return new DeviceNotchSize();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    string notchSizeResponse = javaClass.CallStatic<string>("GetDeviceNotchPixelsSize", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);

                    //Convert the response and return
                    string[] notchSizes = notchSizeResponse.Split('|');
                    DeviceNotchSize notchSize = new DeviceNotchSize();
                    notchSize.rightPixelsSize = int.Parse(notchSizes[0]);
                    notchSize.leftPixelsSize = int.Parse(notchSizes[1]);
                    notchSize.topPixelsSize = int.Parse(notchSizes[2]);
                    notchSize.bottomPixelsSize = int.Parse(notchSizes[3]);
                    return notchSize;
                }

                //Return empty if not run nothing
                return new DeviceNotchSize();
            }

            public static void OpenPlayStoreInAppReview()
            {
                //Calls Java Side to open review PopUp

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Showing Review PopUp", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("OpenPlayStoreInAppReview", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static bool isVibrationAvailable()
            {
                //Calls Java Side to return if the vibration is available in device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isVibrationAvailable", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isWifiEnabled()
            {
                //Calls Java Side to return if the wifi is enabled in device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isWifiEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isConnectedToWifi()
            {
                //Calls Java Side to return if the device is connected to a wifi

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isConnectedToWifi", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isUsingHeadset()
            {
                //Calls Java Side to return if the device is connected with a headset

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isUsingHeadset", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isInternetAvailable()
            {
                //Calls Java Side to return if the device have internet available

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isInternetAvailable");
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isDeveloperModeEnabled()
            {
                //Calls Java Side to return if the device is with developer mode enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isDeveloperModeEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isGooglePlayServicesAvailable()
            {
                //Calls Java Side to return if the google play services is available in the device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isGooglePlayServicesAvailable", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isDeviceRooted()
            {
                //Calls Java Side to return if the device of user is rooted

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isDeviceRooted");
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isAntiScreenshotEnabled()
            {
                //Calls Java Side to return if the anti screenshot is currently enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isAntiScreenshotEnabled();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isAntiScreenshotEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }

                //Return false if not run nothing
                return false;
            }
        }

        public class Settings
        {
            //Core methods

            public static void OpenGeneralSettings()
            {
                //Calls Java Side to open general settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("General Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenGeneralSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenThisAppSettings()
            {
                //Calls Java Side to open this app settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("This App Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenThisAppSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenWifiSettings()
            {
                //Calls Java Side to open wifi settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Wi-Fi Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenWifiSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenBluetoothSettings()
            {
                //Calls Java Side to open bluetooth settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Bluetooth Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenBluetoothSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenLocationSettings()
            {
                //Calls Java Side to open location settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Location Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenLocationSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenNetworkOperatorSettings()
            {
                //Calls Java Side to open mobile data settings screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Network Operator Settings Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenNetworkOperatorSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void OpenInternetToggleSettings()
            {
                //Calls Java Side to open internet toggle popup screen

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Internet Toggle Settings Popup/Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Settings");
                    javaClass.CallStatic("OpenInternetToggleSettings", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }
        }

        public class Location
        {
            //Subclasses

            public enum LocationProvider
            {
                GPS,
                Network
            }

            public enum LocationUpdateTime
            {
                Each5Seconds,
                Each15Seconds,
                Each30Seconds,
                Each45Seconds,
                Each60Seconds,
                Each120Seconds,
                Each180Seconds,
                Each240Seconds,
                Each300Seconds
            }

            public enum LocationUpdateDistance
            {
                Each5Meters,
                Each10Meters,
                Each25Meters,
                Each50Meters,
                Each100Meters,
                Each150Meters,
                Each300Meters
            }

            public enum LocationRunning
            {
                None,
                GPS,
                Network
            }

            public class LocationData
            {
                //This class holds data for current location info

                //Returned by java
                public float bearing = 0;
                public float bearingAccuracyInDegrees = 0;
                public double longitude = 0;
                public double latitude = 0;
                public float horizontalAccuracyInMeters = 0;
                public float verticalAccuracyInMeters = 0;
                public float speedInMetersPerSecond = 0;
                public float speedAcurracyInMetersPerSecond = 0;
                public long fixTimeNanos = 0;
                public long timeMillis = 0;
                public bool isMock = false;
                public string addressSubLocalityName = "";
                public string address0Name = "";
                public string address1Name = "";
                public string address2Name = "";
                public bool isFirstAndCacheLocation = false;
                //Created on C# receives callback
                public Vector2 coordinates = Vector2.zero;
            }

            public enum GoogleMapsMarker
            {
                None,
                MarkerDefault,
                Marker1,
                Marker2,
                Marker3,
                Marker4,
                Marker5,
                Marker6,
                Marker7,
                Marker8,
                Marker9,
                Marker10,
            }

            public enum GoogleMapsZoom
            {
                World,
                Continent,
                City,
                Streets,
                Buildings
            }

            //Core methods

            public static bool isLocationEnabled(LocationProvider locationProvider)
            {
                //Calls Java Side to return if the location is enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //convert location provider enum to id
                    int locationProviderId = -1;
                    if (locationProvider == LocationProvider.GPS)
                        locationProviderId = 0;
                    if (locationProvider == LocationProvider.Network)
                        locationProviderId = 1;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    return javaClass.CallStatic<bool>("isLocationEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, locationProviderId);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isMockEnabled()
            {
                //Calls Java Side to return if the mock is enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    return javaClass.CallStatic<bool>("isMockEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static LocationRunning isTrackingLocation()
            {
                //Calls Java Side to return if is tracking location

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return LocationRunning.None;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isTrackingLocation();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    int trackResult = javaClass.CallStatic<int>("isTrackingLocation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);

                    //Return the result
                    if (trackResult == -1)
                        return LocationRunning.None;
                    if (trackResult == 0)
                        return LocationRunning.GPS;
                    if (trackResult == 1)
                        return LocationRunning.Network;
                }

                //Return false if not run nothing
                return LocationRunning.None;
            }

            public static bool StartTrackingLocation(LocationProvider locationProvider, bool allowMockProviders, LocationUpdateTime locationUpdateTime, LocationUpdateDistance locationUpdateDistance)
            {
                //Calls Java Side to start tracking location

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    if (locationProvider == LocationProvider.GPS)
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Requested start tracking the GPS Location", true);
                    if (locationProvider == LocationProvider.Network)
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Requested start tracking the Network Location", true);
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StartTrackingLocation(locationProvider);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //convert location provider enum to id
                    int locationProviderId = -1;
                    if (locationProvider == LocationProvider.GPS)
                        locationProviderId = 0;
                    if (locationProvider == LocationProvider.Network)
                        locationProviderId = 1;
                    //convert location update time enum to time
                    int locationUpdateTimeMs = 60000;
                    switch (locationUpdateTime)
                    {
                        case LocationUpdateTime.Each5Seconds:
                            locationUpdateTimeMs = 5000;
                            break;
                        case LocationUpdateTime.Each15Seconds:
                            locationUpdateTimeMs = 15000;
                            break;
                        case LocationUpdateTime.Each30Seconds:
                            locationUpdateTimeMs = 30000;
                            break;
                        case LocationUpdateTime.Each45Seconds:
                            locationUpdateTimeMs = 45000;
                            break;
                        case LocationUpdateTime.Each60Seconds:
                            locationUpdateTimeMs = 60000;
                            break;
                        case LocationUpdateTime.Each120Seconds:
                            locationUpdateTimeMs = 120000;
                            break;
                        case LocationUpdateTime.Each180Seconds:
                            locationUpdateTimeMs = 180000;
                            break;
                        case LocationUpdateTime.Each240Seconds:
                            locationUpdateTimeMs = 240000;
                            break;
                        case LocationUpdateTime.Each300Seconds:
                            locationUpdateTimeMs = 300000;
                            break;
                    }
                    //convert location update distance enum to meters
                    int locationUpdateDistanceM = 50;
                    switch (locationUpdateDistance)
                    {
                        case LocationUpdateDistance.Each5Meters:
                            locationUpdateDistanceM = 5;
                            break;
                        case LocationUpdateDistance.Each10Meters:
                            locationUpdateDistanceM = 10;
                            break;
                        case LocationUpdateDistance.Each25Meters:
                            locationUpdateDistanceM = 25;
                            break;
                        case LocationUpdateDistance.Each50Meters:
                            locationUpdateDistanceM = 50;
                            break;
                        case LocationUpdateDistance.Each100Meters:
                            locationUpdateDistanceM = 100;
                            break;
                        case LocationUpdateDistance.Each150Meters:
                            locationUpdateDistanceM = 150;
                            break;
                        case LocationUpdateDistance.Each300Meters:
                            locationUpdateDistanceM = 300;
                            break;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    return javaClass.CallStatic<bool>("StartTrackingLocation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, locationProviderId, allowMockProviders, locationUpdateTimeMs, locationUpdateDistanceM);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool StopTrackingLocation()
            {
                //Calls Java Side to stop tracking location

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Requested stop track Location.", true);
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StopTrackingLocation();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    bool stopped = javaClass.CallStatic<bool>("StopTrackingLocation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);

                    //If was stopped, clear all events
                    if (stopped == true)
                    {
                        NATEvents.onLocationChanged = null;
                        NATEvents.onLocationProviderChanged = null;
                    }

                    //Return the result
                    return stopped;
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isGoogleMapsOpen()
            {
                //Calls Java Side to return if the google maps is open

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    return javaClass.CallStatic<bool>("isGoogleMapsOpen");
                }

                //Return false if not run nothing
                return false;
            }

            public static void OpenGoogleMaps(string mapsTitle)
            {
                //Calls Java Side to open google maps

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Google Maps Dialog");
                    if (NATEvents.onGoogleMapsLoaded != null)
                        NATEvents.onGoogleMapsLoaded();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    javaClass.CallStatic("OpenGoogleMaps", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, mapsTitle);
                }
            }

            public static void AddMarkerOnGoogleMap(string markerTitle, GoogleMapsMarker markerIcon, double latitude, double longitude, bool showInfo)
            {
                //Calls Java Side to add a marker in google maps

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Added a marker on Google Map\n" + markerTitle, true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get id of the marker
                    int markerIconId = -1;
                    switch (markerIcon)
                    {
                        case GoogleMapsMarker.None:
                            markerIconId = -1;
                            break;
                        case GoogleMapsMarker.MarkerDefault:
                            markerIconId = 0;
                            break;
                        case GoogleMapsMarker.Marker1:
                            markerIconId = 1;
                            break;
                        case GoogleMapsMarker.Marker2:
                            markerIconId = 2;
                            break;
                        case GoogleMapsMarker.Marker3:
                            markerIconId = 3;
                            break;
                        case GoogleMapsMarker.Marker4:
                            markerIconId = 4;
                            break;
                        case GoogleMapsMarker.Marker5:
                            markerIconId = 5;
                            break;
                        case GoogleMapsMarker.Marker6:
                            markerIconId = 6;
                            break;
                        case GoogleMapsMarker.Marker7:
                            markerIconId = 7;
                            break;
                        case GoogleMapsMarker.Marker8:
                            markerIconId = 8;
                            break;
                        case GoogleMapsMarker.Marker9:
                            markerIconId = 9;
                            break;
                        case GoogleMapsMarker.Marker10:
                            markerIconId = 10;
                            break;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    javaClass.CallStatic("AddMarkerOnGoogleMap", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, markerTitle, markerIconId, latitude, longitude, showInfo);
                }
            }

            public static void MoveCameraOfGoogleMap(double latitude, double longitude, GoogleMapsZoom zoom)
            {
                //Calls Java Side to open move camera of google maps

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Moving Map Camera", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Fix the zoom
                    float fixedZoom = 0;
                    switch (zoom)
                    {
                        case GoogleMapsZoom.World:
                            fixedZoom = 1.0f;
                            break;
                        case GoogleMapsZoom.Continent:
                            fixedZoom = 5.0f;
                            break;
                        case GoogleMapsZoom.City:
                            fixedZoom = 10.0f;
                            break;
                        case GoogleMapsZoom.Streets:
                            fixedZoom = 15.0f;
                            break;
                        case GoogleMapsZoom.Buildings:
                            fixedZoom = 20.0f;
                            break;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    javaClass.CallStatic("MoveCameraOfGoogleMap", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, latitude, longitude, fixedZoom);
                }
            }

            public static void RemoveAllMarkersOfGoogleMap()
            {
                //Calls Java Side to remove all markers of google map

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Removing all markers of Google Map", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Location");
                    javaClass.CallStatic("RemoveAllMarkersOfGoogleMap", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }
        }

        public class Camera
        {
            //Subclasses

            public enum CameraMode
            {
                PhotoCamera,
                VideoCamera,
                CodeReader
            }

            public enum CameraType
            {
                BackCamera,
                FrontCamera
            }

            public enum CameraVideoQuality
            {
                SD,
                HD,
                FullHD,
                UltraHD
            }

            //Statics variables
            private static ScreenOrientation appOrientationDefault;
            private static ScreenOrientation appOrientationBeforeOpenCamera;

            //Core methods

            public static bool isCameraSupported()
            {
                //Calls Java Side to return if the camera is supported on this device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Camera");
                    return javaClass.CallStatic<bool>("isCameraSupported", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public class CameraNative
            {
                //This class is a Builder for construct a new camera dialog exibition, while setting useful paramters

                //Data for a camera exibition
                private CameraMode cameraMode = CameraMode.PhotoCamera;
                private ScreenOrientation defaultAppOrientation = ScreenOrientation.AutoRotation;
                private string title = "";
                private bool enableFlash = true;
                private bool enableSwitch = true;
                private string codeReaderMessage = "Point to a Bar/QR Code and center it on the Screen";
                private int defaultCamera = 0;
                private int desiredVideoCameraQuality = 0;
                private int maxRecordingSeconds = 0;

                public CameraNative(CameraMode cameraMode, ScreenOrientation defaultAppOrientation)
                {
                    this.cameraMode = cameraMode;
                    this.defaultAppOrientation = defaultAppOrientation;
                }

                public CameraNative setTitle(string title)
                {
                    this.title = title;
                    return this;
                }

                public CameraNative setEnableFlash(bool enableFlash)
                {
                    this.enableFlash = enableFlash;
                    return this;
                }

                public CameraNative setEnableSwitch(bool enableSwitch)
                {
                    this.enableSwitch = enableSwitch;
                    return this;
                }

                public CameraNative setCodeReaderMessage(string codeReaderMessage)
                {
                    this.codeReaderMessage = codeReaderMessage;
                    return this;
                }

                public CameraNative setDefaultCamera(CameraType defaultCamera)
                {
                    if (defaultCamera == CameraType.BackCamera)
                        this.defaultCamera = 0;
                    if (defaultCamera == CameraType.FrontCamera)
                        this.defaultCamera = 1;

                    return this;
                }

                public CameraNative setVideoCameraDesiredQuality(CameraVideoQuality videoCameraQuality)
                {
                    if (videoCameraQuality == CameraVideoQuality.SD)
                        this.desiredVideoCameraQuality = 0;
                    if (videoCameraQuality == CameraVideoQuality.HD)
                        this.desiredVideoCameraQuality = 1;
                    if (videoCameraQuality == CameraVideoQuality.FullHD)
                        this.desiredVideoCameraQuality = 2;
                    if (videoCameraQuality == CameraVideoQuality.UltraHD)
                        this.desiredVideoCameraQuality = 3;

                    return this;
                }

                public CameraNative setMaxRecordingSeconds(int maxSeconds)
                {
                    if (maxSeconds <= 0)
                        this.maxRecordingSeconds = 0;
                    if (maxSeconds > 0)
                        this.maxRecordingSeconds = maxSeconds;

                    return this;
                }

                //Calls native code to show the Camera
                public void OpenThisCamera()
                {
                    //Calls Java Side to open the camera

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("Camera Dialog");
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Save the default app orientation and force the orientation to landscape
                        appOrientationDefault = defaultAppOrientation;
                        appOrientationBeforeOpenCamera = Screen.orientation;
                        Screen.orientation = ScreenOrientation.LandscapeLeft;

                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Camera");
                        AndroidJavaObject activity = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity;
                        if (cameraMode == CameraMode.PhotoCamera)          //<- If "PhotoCamera" is required
                            javaClass.CallStatic("OpenCamera", activity, 0, title, defaultCamera, enableFlash, enableSwitch, "", desiredVideoCameraQuality, maxRecordingSeconds);
                        if (cameraMode == CameraMode.CodeReader)           //<- If "CodeReader" is required
                            javaClass.CallStatic("OpenCamera", activity, 1, title, defaultCamera, enableFlash, enableSwitch, codeReaderMessage, desiredVideoCameraQuality, maxRecordingSeconds);
                        if (cameraMode == CameraMode.VideoCamera)          //<- If "VideoCamera" is required
                            javaClass.CallStatic("OpenCamera", activity, 2, title, defaultCamera, enableFlash, enableSwitch, "", desiredVideoCameraQuality, maxRecordingSeconds);
                    }
                }
            }

            public static Texture2D GetGeneratedQRCode(string stringToBeEncoded)
            {
                //Calls Java Side to return a bitmap from a encoded string

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new Texture2D(2, 2);

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    Texture2D editorTexture = new Texture2D(128, 128);
                    for (int x = 0; x < editorTexture.width; x++)
                        for (int y = 0; y < editorTexture.width; y++)
                            editorTexture.SetPixel(x, y, Color.gray);
                    return editorTexture;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Camera");
                    byte[] qrCodeGeneratedBytes = javaClass.CallStatic<byte[]>("GetGeneratedQRCode", stringToBeEncoded);

                    //Read bytes encoded and transform into Texture2D
                    Texture2D qrCodeTexture = new Texture2D(2, 2);
                    qrCodeTexture.LoadImage(qrCodeGeneratedBytes);
                    qrCodeTexture.filterMode = FilterMode.Point;
                    return qrCodeTexture;
                }

                //Return false if not run nothing
                return new Texture2D(2, 2);
            }

            //Methods automatically called by the event onCameraClose()

            ///<summary>[WARNING] This method is only available for Internal functions of NAT. Don't use this.</summary> 
            public static void OnCameraClose()
            {
                //Restore default orientation settings of application
                Screen.orientation = appOrientationBeforeOpenCamera;
                Screen.orientation = appOrientationDefault;
            }
        }

        public class Microphone
        {
            //Subclasses

            public enum SpeechToTextResult
            {
                NoError,
                DidntUnderstand,
                AudioRecordingError,
                ClientSideError,
                InsufficientPermissions,
                NetworkError,
                NetworkTimeOut,
                NoMatch,
                RecognitionServiceBusy,
                ErrorFromServer,
                NoSpeechInput,
                LanguageNotSupported,
                LanguageUnvailable,
                ErrorServerDisconnected,
                ErrorServerTooManyRequests
            }

            //Core methods

            public static bool isMicrophoneSupported()
            {
                //Calls Java Side to return if the microphone is supported on this device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    return javaClass.CallStatic<bool>("isMicrophoneSupported", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isRecordingMicrophone()
            {
                //Calls Java Side to return if the microphone is being recorded on this device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isRecordingMicrophone();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    return javaClass.CallStatic<bool>("isRecordingMicrophone");
                }

                //Return false if not run nothing
                return false;
            }

            public static void StartRecordingMicrophone()
            {
                //Calls Java Side to start recording microphone

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StartRecordingMicrophone();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    javaClass.CallStatic("StartRecordingMicrophone", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }
            }

            public static void StopRecordingMicrophone()
            {
                //Calls Java Side to stop recording microphone

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StopRecordingMicrophone();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    javaClass.CallStatic("StopRecordingMicrophone", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }
            }

            public static bool isSpeechToTextSupported()
            {
                //Calls Java Side to return if the microphone supports listening speech to text on this device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    return javaClass.CallStatic<bool>("isListeningSpeechSupported", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }

                //Return false if not run nothing
                return false;
            }

            public static bool isListeningSpeechToText()
            {
                //Calls Java Side to return if the microphone is listening speech to text on this device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isListeningSpeechToText();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    return javaClass.CallStatic<bool>("isListeningSpeechToText");
                }

                //Return false if not run nothing
                return false;
            }

            public static void StartListeningSpeechToText()
            {
                //Calls Java Side to start listening speech to text

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StartListeningSpeechToText();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Microphone");
                    javaClass.CallStatic("StartListeningSpeechToText", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }
        }

        public class Applications
        {
            //Core methods

            public static bool isApplicationInstalled(string packageName)
            {
                //Calls Java Side to return if some application is installed in the device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                    return javaClass.CallStatic<bool>("isApplicationInstalled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, packageName);
                }

                //Return false if not run nothing
                return false;
            }

            public static void OpenApplication(string packageName)
            {
                //Calls Java Side to open a application of device, using package name

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Trying to open an application\n\n\"" + packageName + "\"", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                    javaClass.CallStatic("OpenApplication", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, packageName);
                }
            }

            public class OpenApplicationWithExtras
            {
                //This class is a Builder for construct a new opening application with extras, while setting useful paramters

                //Data for opening other application with extras
                private string packageNameOfTheApplication = "";
                private Dictionary<string, string> intentExtras = new Dictionary<string, string>();

                public OpenApplicationWithExtras(string packageName)
                {
                    this.packageNameOfTheApplication = packageName;
                }

                public OpenApplicationWithExtras putExtra(string key, string value)
                {
                    if (intentExtras.ContainsKey(key) == false)
                        intentExtras.Add(key, value);
                    return this;
                }

                //Calls native code to open the application with extras
                public void OpenTheApplicationNow()
                {
                    //Calls Java Side to open the other application with extras

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Trying to open an application with Extras\n\n\"" + packageNameOfTheApplication + "\"", true);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Build the list of extras
                        string keysExtras = "";
                        string valuesExtras = "";
                        bool firstElementAdded = false;
                        foreach (var key in intentExtras)
                        {
                            if (firstElementAdded == true)
                            {
                                keysExtras += "<§§§§§/>";
                                valuesExtras += "<§§§§§/>";
                            }

                            keysExtras += key.Key;
                            valuesExtras += key.Value;

                            firstElementAdded = true;
                        }

                        //Call the native code
                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                        javaClass.CallStatic("OpenApplicationWithExtra", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, packageNameOfTheApplication, keysExtras, valuesExtras);
                    }
                }
            }

            public static string GetThisApplicationPackageName()
            {
                //Calls Java Side to return the package name of this application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return Application.productName.Replace(" ", "");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                    return javaClass.CallStatic<string>("GetThisApplicationPackageName", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return empty if not run nothing
                return "";
            }

            public static bool isPlayStoreAvailable()
            {
                //Calls Java Side to return if the Play Store is available on device

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                    return javaClass.CallStatic<bool>("isPlayStoreAvailable", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static void OpenApplicationInPlayStore(string packageName)
            {
                //Calls Java Side to open a application page in play store

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Opening application in Play Store\n\n\"" + packageName + "\"", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Applications");
                    javaClass.CallStatic("OpenApplicationInPlayStore", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, packageName);
                }
            }
        }

        public class DateTime
        {
            //Subclasses

            public enum TimeSpanValue
            {
                Days,
                Hours,
                Minutes,
                Seconds
            }

            public enum TimeMode
            {
                UtcTime,
                LocalTime
            }

            public class Calendar
            {
                //This class can storage a date time, and make another functions, like increase, decrease, compare datetimes and etc.

                //Private variables of this class
                private int year = 0;
                private int month = 0;
                private int day = 0;
                private int hour = 0;
                private int minute = 0;
                private int second = 0;

                //Getters for the time

                public int Year
                {
                    get { return year; }
                }

                public int Month
                {
                    get { return month; }
                }

                public int Day
                {
                    get { return day; }
                }

                public int Hour
                {
                    get { return hour; }
                }

                public int Minute
                {
                    get { return minute; }
                }

                public int Second
                {
                    get { return second; }
                }

                public string YearString
                {
                    get { return year.ToString(); }
                }

                public string MonthString
                {
                    get { return (month < 10) ? ("0" + month) : month.ToString(); }
                }

                public string DayString
                {
                    get { return (day < 10) ? ("0" + day) : day.ToString(); }
                }

                public string HourString
                {
                    get { return (hour < 10) ? ("0" + hour) : hour.ToString(); }
                }

                public string MinuteString
                {
                    get { return (minute < 10) ? ("0" + minute) : minute.ToString(); }
                }

                public string SecondString
                {
                    get { return (second < 10) ? ("0" + second) : second.ToString(); }
                }

                //Return value of this class on use Debug.Log();

                public override string ToString()
                {
                    return "Time of this Calendar (M/D/Y H:M:S)\n" + MonthString + "/" + DayString + "/" + YearString + " " + HourString + ":" + MinuteString + ":" + SecondString;
                }

                //Core methods of class

                public Calendar()
                {
                    //Instantiate this class with current time of system
                    System.DateTime currentTimeLocal = System.DateTime.Now;
                    year = currentTimeLocal.Year;
                    month = currentTimeLocal.Month;
                    day = currentTimeLocal.Day;
                    hour = currentTimeLocal.Hour;
                    minute = currentTimeLocal.Minute;
                    second = currentTimeLocal.Second;
                }

                public Calendar(long utcUnixMillisTime)
                {
                    //Instantiate this class with the current UTC Unix time
                    System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    System.DateTime dateTime = epoch.AddMilliseconds(utcUnixMillisTime).ToLocalTime();

                    //Stores the datetime
                    year = dateTime.Year;
                    month = dateTime.Month;
                    day = dateTime.Day;
                    hour = dateTime.Hour;
                    minute = dateTime.Minute;
                    second = dateTime.Second;
                }

                public Calendar(int localYear, int localMonth, int localDay, int localHour, int localMinute, int localSecond)
                {
                    //Instantiate this class with a predefined date and time local
                    if (localHour < 0)
                        localHour = 0;
                    if (localHour > 23)
                        localHour = 23;
                    if (localMinute < 0)
                        localMinute = 0;
                    if (localMinute > 59)
                        localMinute = 59;
                    if (localSecond < 0)
                        localSecond = 0;
                    if (localSecond > 59)
                        localSecond = 59;

                    //Store the time now, local format
                    this.year = localYear;
                    this.month = localMonth;
                    this.day = localDay;
                    this.hour = localHour;
                    this.minute = localMinute;
                    this.second = localSecond;
                }

                public Calendar(Calendar anotherCalendarObject)
                {
                    //Instantiate this class with the time data of other Calendar object
                    this.year = anotherCalendarObject.Year;
                    this.month = anotherCalendarObject.Month;
                    this.day = anotherCalendarObject.Day;
                    this.hour = anotherCalendarObject.Hour;
                    this.minute = anotherCalendarObject.Minute;
                    this.second = anotherCalendarObject.Second;
                }

                public long GetUnixMillisTime(TimeMode timeMode)
                {
                    //Convert the content of this, to DateTime
                    int originalYear = year;
                    int originalMonth = month;
                    int originalDay = day;
                    int rYear = year == 0 ? year + 1 : year;
                    int rMonth = month == 0 ? month + 1 : month;
                    int rDay = day == 0 ? day + 1 : day;
                    System.DateTime timeOfThis = new System.DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local);

                    //Show warning if detect a zero date
                    if (originalYear == 0 || originalMonth == 0 || originalDay == 0)
                    {
                        Debug.LogWarning("Warning, a value of zero was found for Year, Month, or Day when converting this calendar to Unix time. The Unix time returned to you may have been corrected to 1/1/0001 to prevent errors in operations.");
                    }

                    //Convert DateTime to unix time stamp
                    if (timeMode == TimeMode.UtcTime)
                    {
                        System.DateTime unixStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        long unixTimeStampInTicks = (timeOfThis.ToUniversalTime() - unixStart).Ticks;
                        return unixTimeStampInTicks / System.TimeSpan.TicksPerMillisecond;
                    }
                    if (timeMode == TimeMode.LocalTime)
                    {
                        System.DateTime unixStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                        long unixTimeStampInTicks = (timeOfThis.ToLocalTime() - unixStart).Ticks;
                        return unixTimeStampInTicks / System.TimeSpan.TicksPerMillisecond;
                    }
                    return 0;
                }

                public long GetThisCalendarTicks(TimeMode timeMode)
                {
                    //Return the corresponding time of this class into ticks
                    return GetTimeSpan(timeMode).Ticks;
                }

                public System.DateTime GetDateTime(TimeMode timeMode)
                {
                    //Convert the content of this, to DateTime
                    int originalYear = year;
                    int originalMonth = month;
                    int originalDay = day;
                    int rYear = year == 0 ? year + 1 : year;
                    int rMonth = month == 0 ? month + 1 : month;
                    int rDay = day == 0 ? day + 1 : day;

                    //Show warning if detect a zero date
                    if (originalYear == 0 || originalMonth == 0 || originalDay == 0)
                    {
                        Debug.LogWarning("Warning, a value of zero was found for Year, Month, or Day when converting this calendar to Unix time. The Unix time returned to you may have been corrected to 1/1/0001 to prevent errors in operations.");
                    }

                    if (timeMode == TimeMode.LocalTime)
                        return new System.DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local).ToLocalTime();
                    if (timeMode == TimeMode.UtcTime)
                        return new System.DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local).ToUniversalTime();
                    return System.DateTime.Now;
                }

                public System.TimeSpan GetTimeSpan(TimeMode timeMode)
                {
                    //Return the corresponding time of this class, into TimeSpan
                    return new System.TimeSpan(GetDateTime(timeMode).Ticks);
                }

                public string SerializeToString()
                {
                    //Serialize this class with the curent time, to string, to load later
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Calendar[");
                    builder.Append(year.ToString());
                    builder.Append(",");
                    builder.Append(month.ToString());
                    builder.Append(",");
                    builder.Append(day.ToString());
                    builder.Append(",");
                    builder.Append(hour.ToString());
                    builder.Append(",");
                    builder.Append(minute.ToString());
                    builder.Append(",");
                    builder.Append(second.ToString());
                    builder.Append("]");
                    return builder.ToString();
                }

                public void DeserializeFromString(string stringFromSerialization)
                {
                    //Error message
                    string errorMessage = "Could not deserialize a Calendar from a string because the entered string is invalid.";

                    //Verify if contains Calendar word
                    if (stringFromSerialization.Contains("Calendar") == true)
                    {
                        string stringWithoutCalendar = stringFromSerialization.Replace("Calendar", "");

                        //Verify if contains the []
                        if (stringWithoutCalendar.Contains("[") == true && stringWithoutCalendar.Contains("]") == true)
                        {
                            string stringWithoutKeys = stringWithoutCalendar.Replace("[", "").Replace("]", "");
                            string[] arrayOfValues = stringWithoutKeys.Split(',');

                            this.year = int.Parse(arrayOfValues[0]);
                            this.month = int.Parse(arrayOfValues[1]);
                            this.day = int.Parse(arrayOfValues[2]);
                            this.hour = int.Parse(arrayOfValues[3]);
                            this.minute = int.Parse(arrayOfValues[4]);
                            this.second = int.Parse(arrayOfValues[5]);
                        }
                        if (stringWithoutCalendar.Contains("[") == false || stringWithoutCalendar.Contains("]") == false)
                            Debug.LogError(errorMessage);
                    }
                    if (stringFromSerialization.Contains("Calendar") == false)
                        Debug.LogError(errorMessage);
                }

                public Calendar IncreaseThisIn(int valueToIncrease, TimeSpanValue timeSpanValue)
                {
                    //Create the TimeSpan
                    System.TimeSpan timeSpan = new System.TimeSpan(0, 0, 0, 0, 0);
                    switch (timeSpanValue)
                    {
                        case TimeSpanValue.Days:
                            timeSpan = new System.TimeSpan(valueToIncrease, 0, 0, 0, 0);
                            break;
                        case TimeSpanValue.Hours:
                            timeSpan = new System.TimeSpan(0, valueToIncrease, 0, 0, 0);
                            break;
                        case TimeSpanValue.Minutes:
                            timeSpan = new System.TimeSpan(0, 0, valueToIncrease, 0, 0);
                            break;
                        case TimeSpanValue.Seconds:
                            timeSpan = new System.TimeSpan(0, 0, 0, valueToIncrease, 0);
                            break;
                    }

                    //Store the original value of date
                    int originalYear = year;
                    int originalMonth = month;
                    int originalDay = day;

                    //Increase the date in 1 where is zero
                    year = year == 0 ? year + 1 : year;
                    month = month == 0 ? month + 1 : month;
                    day = day == 0 ? day + 1 : day;

                    //Creat the DateTime of this
                    System.DateTime dateTime = new System.DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
                    System.DateTime dateTimeIncreased = dateTime.Add(timeSpan);

                    //Store the result
                    int rYear = dateTimeIncreased.Year;
                    int rMonth = dateTimeIncreased.Month;
                    int rDay = dateTimeIncreased.Day;
                    int rHour = dateTimeIncreased.Hour;
                    int rMinute = dateTimeIncreased.Minute;
                    int rSecond = dateTimeIncreased.Second;

                    //Decreaze the date in 1 where the default is zero
                    rYear = originalYear == 0 ? rYear - 1 : rYear;
                    rMonth = originalMonth == 0 ? rMonth - 1 : rMonth;
                    rDay = originalDay == 0 ? rDay - 1 : rDay;

                    //Stores the values
                    this.year = rYear;
                    this.month = rMonth;
                    this.day = rDay;
                    this.hour = rHour;
                    this.minute = rMinute;
                    this.second = rSecond;

                    return this;
                }

                public Calendar DecreaseThisIn(int valueToDecrease, TimeSpanValue timeSpanValue)
                {
                    //Create the TimeSpan
                    System.TimeSpan timeSpan = new System.TimeSpan(0, 0, 0, 0, 0);
                    switch (timeSpanValue)
                    {
                        case TimeSpanValue.Days:
                            timeSpan = new System.TimeSpan(valueToDecrease, 0, 0, 0, 0);
                            break;
                        case TimeSpanValue.Hours:
                            timeSpan = new System.TimeSpan(0, valueToDecrease, 0, 0, 0);
                            break;
                        case TimeSpanValue.Minutes:
                            timeSpan = new System.TimeSpan(0, 0, valueToDecrease, 0, 0);
                            break;
                        case TimeSpanValue.Seconds:
                            timeSpan = new System.TimeSpan(0, 0, 0, valueToDecrease, 0);
                            break;
                    }

                    //Store the original value of date
                    int originalYear = year;
                    int originalMonth = month;
                    int originalDay = day;

                    //Increase the date in 1 where is zero
                    year = year == 0 ? year + 1 : year;
                    month = month == 0 ? month + 1 : month;
                    day = day == 0 ? day + 1 : day;

                    //Creat the DateTime of this
                    System.DateTime thisDateTime = new System.DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

                    //Decrease the DateTime from timespan desired
                    if (thisDateTime.Ticks > timeSpan.Ticks)
                    {
                        System.DateTime dateTimeDecreased = thisDateTime.Subtract(timeSpan);

                        //Store the result
                        int rYear = dateTimeDecreased.Year;
                        int rMonth = dateTimeDecreased.Month;
                        int rDay = dateTimeDecreased.Day;
                        int rHour = dateTimeDecreased.Hour;
                        int rMinute = dateTimeDecreased.Minute;
                        int rSecond = dateTimeDecreased.Second;

                        //Decreaze the date in 1 where the default is zero
                        rYear = originalYear == 0 ? rYear - 1 : rYear;
                        rMonth = originalMonth == 0 ? rMonth - 1 : rMonth;
                        rDay = originalDay == 0 ? rDay - 1 : rDay;

                        //Stores the values
                        this.year = rYear;
                        this.month = rMonth;
                        this.day = rDay;
                        this.hour = rHour;
                        this.minute = rMinute;
                        this.second = rSecond;
                    }
                    if (thisDateTime.Ticks <= timeSpan.Ticks)
                    {
                        //Stores the zero value
                        this.year = 0;
                        this.month = 0;
                        this.day = 0;
                        this.hour = 0;
                        this.minute = 0;
                        this.second = 0;
                    }

                    return this;
                }

                public Calendar IncreaseThisWithDate(Calendar dateTime)
                {
                    //Validate the DateTime
                    if (dateTime.Year == 0 || dateTime.Month == 0 || dateTime.Day == 0)
                    {
                        Debug.LogError("It is not possible to increase the time of a Calendar if the date to increase is equal to 00/00/0000. Please enter a valid date equal to or greater than 01/01/0001.");
                        return this;
                    }
                    if (year == 0 || month == 0 || day == 0)
                    {
                        Debug.LogError("You cannot increase the Calendar date if the Calendar to be increased has a date of 00/00/0000. Please enter a valid date greater than or equal to 01/01/0001.");
                        return this;
                    }

                    //Create datetime representation of this calendar and another
                    System.DateTime thisDateTime = new System.DateTime(year, month, day, hour, minute, second);
                    System.TimeSpan thisTimeSpan = new System.TimeSpan(thisDateTime.Ticks);
                    System.DateTime dateTimeToIncrement = new System.DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                    System.TimeSpan timeSpanToIncrement = new System.TimeSpan(dateTimeToIncrement.Ticks);

                    //Do the operation of increase
                    System.TimeSpan resultTimeSpan = thisTimeSpan + timeSpanToIncrement;

                    int days = resultTimeSpan.Days;
                    int months = 0;
                    int years = 0;

                    //Calculate the number of monts, with the days
                    while (days >= 30)
                    {
                        days -= 30;
                        months += 1;
                    }

                    //Calculate the number of years, with the months
                    while (months >= 12)
                    {
                        months -= 12;
                        years += 1;
                    }

                    this.year = years;
                    this.month = months;
                    this.day = days;
                    this.hour = resultTimeSpan.Hours;
                    this.minute = resultTimeSpan.Minutes;
                    this.second = resultTimeSpan.Seconds;

                    return this;
                }

                public Calendar DecreaseThisWithDate(Calendar dateTime)
                {
                    //Validate the DateTime
                    if (dateTime.Year == 0 || dateTime.Month == 0 || dateTime.Day == 0)
                    {
                        Debug.LogError("It is not possible to decrease the time of a Calendar if the date to decrease is equal to 00/00/0000. Please enter a valid date equal to or greater than 01/01/0001.");
                        return this;
                    }
                    if (year == 0 || month == 0 || day == 0)
                    {
                        Debug.LogError("You cannot decrease the Calendar date if the Calendar to be decreased has a date of 00/00/0000. Please enter a valid date greater than or equal to 01/01/0001.");
                        return this;
                    }

                    //Create datetime representation of this calendar and another
                    System.DateTime thisDateTime = new System.DateTime(year, month, day, hour, minute, second);
                    System.TimeSpan thisTimeSpan = new System.TimeSpan(thisDateTime.Ticks);
                    System.DateTime dateTimeToSubtract = new System.DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                    System.TimeSpan timeSpanToSubtract = new System.TimeSpan(dateTimeToSubtract.Ticks);

                    //If the datetime to subtract is less than this timespan
                    if (timeSpanToSubtract.Ticks < thisTimeSpan.Ticks)
                    {
                        System.TimeSpan resultTimeSpan = thisTimeSpan - timeSpanToSubtract;

                        int days = resultTimeSpan.Days;
                        int months = 0;
                        int years = 0;

                        //Calculate the number of monts, with the days
                        while (days >= 30)
                        {
                            days -= 30;
                            months += 1;
                        }

                        //Calculate the number of years, with the months
                        while (months >= 12)
                        {
                            months -= 12;
                            years += 1;
                        }

                        this.year = years;
                        this.month = months;
                        this.day = days;
                        this.hour = resultTimeSpan.Hours;
                        this.minute = resultTimeSpan.Minutes;
                        this.second = resultTimeSpan.Seconds;
                    }
                    //If the datetime to subtract is more than this timespan
                    if (timeSpanToSubtract.Ticks >= thisTimeSpan.Ticks)
                    {
                        this.year = 0;
                        this.month = 0;
                        this.day = 0;
                        this.hour = 0;
                        this.minute = 0;
                        this.second = 0;
                    }

                    return this;
                }

                public Calendar SetThisToZero()
                {
                    this.year = 0;
                    this.month = 0;
                    this.day = 0;
                    this.hour = 0;
                    this.minute = 0;
                    this.second = 0;
                    return this;
                }

                public Calendar SetThisToCurrentSystemDateTime()
                {
                    System.DateTime dateTime = System.DateTime.Now;

                    this.year = dateTime.Year;
                    this.month = dateTime.Month;
                    this.day = dateTime.Day;
                    this.hour = dateTime.Hour;
                    this.minute = dateTime.Minute;
                    this.second = dateTime.Second;
                    return this;
                }

                //Comparators methods

                public bool isEqualsToZero()
                {
                    if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0)
                        return true;
                    if (year != 0 || month != 0 || day != 0 || hour != 0 || minute != 0 || second != 0)
                        return false;
                    return false;
                }

                public bool isEqualTo(Calendar calendarToCompare)
                {
                    int cYear = calendarToCompare.Year;
                    int cMonth = calendarToCompare.Month;
                    int cDay = calendarToCompare.Day;
                    int cHour = calendarToCompare.Hour;
                    int cMinute = calendarToCompare.Minute;
                    int cSecond = calendarToCompare.Second;

                    if (year == cYear && month == cMonth && day == cDay && hour == cHour && minute == cMinute && second == cSecond)
                        return true;
                    if (year != cYear || month != cMonth || day != cDay || hour != cHour || minute != cMinute || second != cSecond)
                        return false;

                    return false;
                }

                public bool isNotEqualTo(Calendar calendarToCompare)
                {
                    int cYear = calendarToCompare.Year;
                    int cMonth = calendarToCompare.Month;
                    int cDay = calendarToCompare.Day;
                    int cHour = calendarToCompare.Hour;
                    int cMinute = calendarToCompare.Minute;
                    int cSecond = calendarToCompare.Second;

                    if (year == cYear && month == cMonth && day == cDay && hour == cHour && minute == cMinute && second == cSecond)
                        return false;
                    if (year != cYear || month != cMonth || day != cDay || hour != cHour || minute != cMinute || second != cSecond)
                        return true;

                    return false;
                }

                public bool isGreaterThan(Calendar calendarToCompare)
                {
                    //Calculate this Calendar TimeSpan
                    int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                    System.TimeSpan thisCalendarSpan = new System.TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                    //Calculate another Calendar TimeSpan
                    int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                    System.TimeSpan anotherCalendarSpan = new System.TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                    //Return the result
                    if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                        return true;
                    if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                        return false;

                    return false;
                }

                public bool isLessThan(Calendar calendarToCompare)
                {
                    //Calculate this Calendar TimeSpan
                    int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                    System.TimeSpan thisCalendarSpan = new System.TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                    //Calculate another Calendar TimeSpan
                    int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                    System.TimeSpan anotherCalendarSpan = new System.TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                    //Return the result
                    if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                        return false;
                    if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                        return true;

                    return false;
                }

                public bool isGreaterThanOrEqualTo(Calendar calendarToCompare)
                {
                    //Calculate this Calendar TimeSpan
                    int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                    System.TimeSpan thisCalendarSpan = new System.TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                    //Calculate another Calendar TimeSpan
                    int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                    System.TimeSpan anotherCalendarSpan = new System.TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                    //Return the result
                    if (thisCalendarSpan.Ticks >= anotherCalendarSpan.Ticks)
                        return true;
                    if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                        return false;

                    return false;
                }

                public bool isLessThanOrEqualTo(Calendar calendarToCompare)
                {
                    //Calculate this Calendar TimeSpan
                    int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                    TimeSpan thisCalendarSpan = new TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                    //Calculate another Calendar TimeSpan
                    int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                    TimeSpan anotherCalendarSpan = new TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                    //Return the result
                    if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                        return false;
                    if (thisCalendarSpan.Ticks <= anotherCalendarSpan.Ticks)
                        return true;

                    return false;
                }
            }

            public class TimeElapsedWhileClosed
            {
                //This class stores data about time elapsed while this application is closed
                public Calendar timeElapsed_accordingSystemClock = null;
            }

            public class TimeElapsedWhilePaused
            {
                //This class stores data about time elapsed while this application is paused
                public Calendar timeElapsed_accordingRealtimeClockAfterBoot = null;
            }

            //Core methods

            public static void OpenHourPicker(string title)
            {
                //Calls Java Side to open a native hour picker

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenHourPicker(title);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.DateTime");
                    javaClass.CallStatic("OpenHourPicker", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title);
                }
            }

            public static void OpenDatePicker(string title, Calendar minDate, Calendar maxDate)
            {
                //Calls Java Side to open a native date picker

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenDatePicker(title);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.DateTime");
                    javaClass.CallStatic("OpenDatePicker", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, title, minDate.GetUnixMillisTime(TimeMode.UtcTime), maxDate.GetUnixMillisTime(TimeMode.UtcTime));
                }
            }

            public static void LoadCurrentTimeOfNTP()
            {
                //Calls Java Side to load the time of NTP server

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    if (NATEvents.onDateTimeDoneNTPRequest != null)
                        NATEvents.onDateTimeDoneNTPRequest(true, new Calendar(), "LocalHost");
                    NATEvents.onDateTimeDoneNTPRequest = null;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.DateTime");
                    javaClass.CallStatic("LoadCurrentTimeOfNTP", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }
            }

            public static Calendar GetElapsedRealtimeSinceBoot()
            {
                //Calls Java Side to return the elapsed realtime since boot of system

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new Calendar().SetThisToZero();

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return new Calendar(0, 0, 0, 0, 5, 0);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.DateTime");
                    long elapsedTime = javaClass.CallStatic<long>("GetElapsedRealtimeSinceBoot");
                    Calendar elapsedTimeCalendar = new Calendar(elapsedTime).DecreaseThisWithDate(new Calendar(1969, 12, 31, 21, 0, 0)); //<- Remove 1969 years, 12 months, 31 days and 21 hours to remove the UNIX epoch
                    return elapsedTimeCalendar;
                }

                //Return empty if not run nothing
                return new Calendar().SetThisToZero();
            }
        }

        public class Files
        {
            //Subclasses

            public enum Scope
            {
                AppFiles,
                AppCache,
                DCIM,
                Documents,
                Downloads,
                Movies,
                Music,
                Pictures,
                Podcasts,
                Recordings,
                Ringtones,
                Screenshots
            }

            public enum ScopeStatus
            {
                Unknown,
                AvailableToUse,
                UnvailableMemory,
                UnvailableScope,
                InUseByUSB,
                WithoutPermission
            }

            public enum MediaType
            {
                Screenshot,
                Picture,
                Photo,
                Music,
                Recording,
                Video,
                Document
            }

            public class FilesClassCache
            {
                //This class stores the path of each scope in cache to future queries
                public static string scope_appFilesPath = "";
                public static string scope_appCachePath = "";
                public static string scope_dcimPath = "";
                public static string scope_documentsPath = "";
                public static string scope_downloadsPath = "";
                public static string scope_moviesPath = "";
                public static string scope_musicPath = "";
                public static string scope_picturesPath = "";
                public static string scope_podcastsPath = "";
                public static string scope_recordingsPath = "";
                public static string scope_ringtonesPath = "";
                public static string scope_screenshotsPath = "";

                ///<summary>[WARNING] This method is only available for Internal functions of NAT. Don't use this.</summary> 
                public static void InitializeFilesClassCache()
                {
                    //Calls Java Side to pre-load all data for files class, like path for each scope

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        //Load each scope path
                        scope_appFilesPath = Application.persistentDataPath + "/Files";
                        scope_appCachePath = Application.persistentDataPath + "/Cache";
                        scope_dcimPath = Application.persistentDataPath + "/DCIM";
                        scope_documentsPath = Application.persistentDataPath + "/Documents";
                        scope_downloadsPath = Application.persistentDataPath + "/Download";
                        scope_moviesPath = Application.persistentDataPath + "/Movies";
                        scope_musicPath = Application.persistentDataPath + "/Music";
                        scope_picturesPath = Application.persistentDataPath + "/Pictures";
                        scope_podcastsPath = Application.persistentDataPath + "/Podcasts";
                        scope_recordingsPath = Application.persistentDataPath + "/Recordings";
                        scope_ringtonesPath = Application.persistentDataPath + "/Ringtones";
                        scope_screenshotsPath = Application.persistentDataPath + "/Screenshots";

                        //Create each scope in persistent data path if not exists
                        Directory.CreateDirectory(scope_appFilesPath);
                        Directory.CreateDirectory(scope_appCachePath);
                        Directory.CreateDirectory(scope_dcimPath);
                        Directory.CreateDirectory(scope_documentsPath);
                        Directory.CreateDirectory(scope_downloadsPath);
                        Directory.CreateDirectory(scope_moviesPath);
                        Directory.CreateDirectory(scope_musicPath);
                        Directory.CreateDirectory(scope_picturesPath);
                        Directory.CreateDirectory(scope_podcastsPath);
                        Directory.CreateDirectory(scope_recordingsPath);
                        Directory.CreateDirectory(scope_ringtonesPath);
                        Directory.CreateDirectory(scope_screenshotsPath);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Prepare the java object
                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");

                        //Load each scope path
                        scope_appFilesPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 0);
                        scope_appCachePath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 1);
                        scope_dcimPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 2);
                        scope_documentsPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 3);
                        scope_downloadsPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 4);
                        scope_moviesPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 5);
                        scope_musicPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 6);
                        scope_picturesPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 7);
                        scope_podcastsPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 8);
                        scope_recordingsPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 9);
                        scope_ringtonesPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 10);
                        scope_screenshotsPath = javaClass.CallStatic<string>("GetAbsolutePathForDesiredScope", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, 11);
                    }
                }
            }

            public class Size
            {
                //This class stores a size of a file or size info

                //Private variables
                private long sizeInBytes;

                //Getters for converted size

                public long ToBytes
                {
                    get { return sizeInBytes; }
                }

                public float ToKilobytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1000); }
                }

                public float ToKibibytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1024); }
                }

                public float ToMegabytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1000000); }
                }

                public float ToMebibytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1049000); }
                }

                public float ToGigabytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1000000000); }
                }

                public float ToGibibytes
                {
                    get { return (float)((Double)sizeInBytes / (Double)1074000000); }
                }

                //Core methods

                public Size(long sizeInBytes)
                {
                    this.sizeInBytes = sizeInBytes;
                }
            }

            public class StorageInfo
            {
                //This class stores informations about internal memory

                //Public variables
                public Size freeMemory;
                public Size usedMemory;
                public Size totalMemory;

                //Core methods

                public StorageInfo(Size free, Size used, Size total)
                {
                    this.freeMemory = free;
                    this.usedMemory = used;
                    this.totalMemory = total;
                }
            }

            public class ItemsList
            {
                //This class stores informations about list of files and folders of a directory
                public string[] folders;
                public string[] files;

                public ItemsList(string[] folders, string[] files)
                {
                    this.folders = folders;
                    this.files = files;
                }
            }

            public class Attributes
            {
                //This class stores attributes of a file or folder
                public bool isFile;
                public bool isFolder;
                public Size size;
                public System.DateTime lastModify;
                public string parentPath;
                public bool isHidden;
                public bool isWritable;
                public bool isReadable;
                public string pureName;
                public string extension;
            }

            public enum FilePickerInterfaceMode
            {
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public enum FilePickerAction
            {
                CreateFile,
                OpenFile
            }

            public enum MimeType
            {
                //More MIME Types can be found on: https://android.googlesource.com/platform/external/mime-support/+/9817b71a54a2ee8b691c1dfa937c0f9b16b3473c/mime.types

                All,
                JSON,
                OGG,
                PDF,
                RAR,
                RTF,
                XML,
                ZIP,
                ThreeGP,
                AMR,
                MID,
                WAV,
                MP3,
                WMA,
                TTF,
                OTF,
                GIF,
                JPG,
                PNG,
                SVG,
                TIF,
                ICO,
                BMP,
                PSD,
                CSS,
                CSV,
                HTML,
                MD,
                TXT,
                RTX,
                WML,
                BOO,
                HPP,
                CPP,
                H,
                C,
                D,
                HS,
                JAVA,
                MOC,
                P,
                PL,
                SH,
                MPG,
                MP4,
                QT,
                MOV,
                OGV,
                WEBM,
                FLV,
                WMV,
                AVI,
                MKV
            }

            public enum FilePickerOperationStatus
            {
                Unknown,
                Canceled,
                Successfully,
                InvalidScope
            }

            public class FilePickerOperationResponse
            {
                //This class stores result data after a file picker operation
                public FilePickerAction operationType;
                public Scope scopeOfOperation;
                public string pathFromOperationInScope;
            }

            public enum FilePickerDefaultScope
            {
                LeaveSystemDecide,
                DCIM,
                Documents,
                Movies,
                Music,
                Pictures,
                Podcasts,
                Ringtones
            }

            //Core methods

            public static string GetAbsolutePathForScope(Scope desiredScope)
            {
                //Calls Java Side to get the path for some desired scope

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    switch (desiredScope)
                    {
                        case Scope.AppFiles:
                            return FilesClassCache.scope_appFilesPath;
                        case Scope.AppCache:
                            return FilesClassCache.scope_appCachePath;
                        case Scope.DCIM:
                            return FilesClassCache.scope_dcimPath;
                        case Scope.Documents:
                            return FilesClassCache.scope_documentsPath;
                        case Scope.Downloads:
                            return FilesClassCache.scope_downloadsPath;
                        case Scope.Movies:
                            return FilesClassCache.scope_moviesPath;
                        case Scope.Music:
                            return FilesClassCache.scope_musicPath;
                        case Scope.Pictures:
                            return FilesClassCache.scope_picturesPath;
                        case Scope.Podcasts:
                            return FilesClassCache.scope_podcastsPath;
                        case Scope.Recordings:
                            return FilesClassCache.scope_recordingsPath;
                        case Scope.Ringtones:
                            return FilesClassCache.scope_ringtonesPath;
                        case Scope.Screenshots:
                            return FilesClassCache.scope_screenshotsPath;
                    }
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    switch (desiredScope)
                    {
                        case Scope.AppFiles:
                            return FilesClassCache.scope_appFilesPath;
                        case Scope.AppCache:
                            return FilesClassCache.scope_appCachePath;
                        case Scope.DCIM:
                            return FilesClassCache.scope_dcimPath;
                        case Scope.Documents:
                            return FilesClassCache.scope_documentsPath;
                        case Scope.Downloads:
                            return FilesClassCache.scope_downloadsPath;
                        case Scope.Movies:
                            return FilesClassCache.scope_moviesPath;
                        case Scope.Music:
                            return FilesClassCache.scope_musicPath;
                        case Scope.Pictures:
                            return FilesClassCache.scope_picturesPath;
                        case Scope.Podcasts:
                            return FilesClassCache.scope_podcastsPath;
                        case Scope.Recordings:
                            return FilesClassCache.scope_recordingsPath;
                        case Scope.Ringtones:
                            return FilesClassCache.scope_ringtonesPath;
                        case Scope.Screenshots:
                            return FilesClassCache.scope_screenshotsPath;
                    }
                }

                //Return empty if not run nothing
                return "";
            }

            public static ScopeStatus GetScopeAvailability(Scope desiredScope)
            {
                //Calls Java Side to get a status of current scope availablity

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return ScopeStatus.Unknown;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //Get scope path
                    string scopePath = GetAbsolutePathForScope(desiredScope);

                    //If the scope path is empty, return error
                    if (scopePath == "")
                        return ScopeStatus.UnvailableScope;
                    if (scopePath != "")
                        return ScopeStatus.AvailableToUse;

                    return ScopeStatus.Unknown;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");

                    //Get the scope availability accordint to java side and return
                    int scopeAvailability = -1;
                    if (desiredScope == Scope.AppFiles || desiredScope == Scope.AppCache)
                        scopeAvailability = javaClass.CallStatic<int>("GetScopeAvailability", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, true, GetAbsolutePathForScope(desiredScope));
                    if (desiredScope != Scope.AppFiles && desiredScope != Scope.AppCache)
                        scopeAvailability = javaClass.CallStatic<int>("GetScopeAvailability", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, false, GetAbsolutePathForScope(desiredScope));

                    if (scopeAvailability == -1)
                        return ScopeStatus.Unknown;
                    if (scopeAvailability == 0)
                        return ScopeStatus.AvailableToUse;
                    if (scopeAvailability == 1)
                        return ScopeStatus.UnvailableMemory;
                    if (scopeAvailability == 2)
                        return ScopeStatus.UnvailableScope;
                    if (scopeAvailability == 3)
                        return ScopeStatus.InUseByUSB;
                    if (scopeAvailability == 4)
                        return ScopeStatus.WithoutPermission;

                    return ScopeStatus.Unknown;
                }

                //Return unknown error
                return ScopeStatus.Unknown;
            }

            public static StorageInfo GetInternalMemoryUsageInfo()
            {
                //Calls Java Side to return the size info of internal memory

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return null;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return new StorageInfo(new Size(20000000000), new Size(5000000000), new Size(25000000000));
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    string storageInfo = javaClass.CallStatic<string>("GetInternalMemoryUsageInfo", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);

                    //Convert to storage info object
                    string[] storageInfos = storageInfo.Split(',');
                    return new StorageInfo(new Size(long.Parse(storageInfos[2])), new Size(long.Parse(storageInfos[1])), new Size(long.Parse(storageInfos[0])));
                }

                //Return null
                return null;
            }

            public static void SaveMediaAndMakesItAvailableToGallery(MediaType mediaType, string mediaPath)
            {
                //Calls Java Side to copy a media to a defined collection and register it on MediaStore to be available in Gallery

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Media saved to Gallery!", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get the media type code
                    int mediaTypeInt = -1;
                    switch (mediaType)
                    {
                        case MediaType.Screenshot:
                            mediaTypeInt = 0;
                            break;
                        case MediaType.Picture:
                            mediaTypeInt = 1;
                            break;
                        case MediaType.Photo:
                            mediaTypeInt = 2;
                            break;
                        case MediaType.Music:
                            mediaTypeInt = 3;
                            break;
                        case MediaType.Recording:
                            mediaTypeInt = 4;
                            break;
                        case MediaType.Video:
                            mediaTypeInt = 5;
                            break;
                        case MediaType.Document:
                            mediaTypeInt = 6;
                            break;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    javaClass.CallStatic("SaveMediaAndMakesItAvailableToGallery", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, mediaPath, mediaTypeInt);
                }
            }

            //Private validations methods

            private static bool isThisOperationValid(Scope scope, string pathOfOperation)
            {
                //Store the validation
                bool isValid = true;

                //Check if the scope is available
                if (GetScopeAvailability(scope) != ScopeStatus.AvailableToUse)
                {
                    Debug.LogError("NAT: This file operation could not be performed. Apparently the requested scope is not available (\"" + GetScopeAvailability(scope) + "\").");
                    isValid = false;
                }

                //Check if the path have a up level symbol
                if (pathOfOperation.Contains("..") == true || pathOfOperation.Contains("...") == true)
                {
                    Debug.LogError("NAT: This file operation could not be performed. There is something wrong with the given path. Please do not use \"..\" or \"...\" to level up in the path.");
                    isValid = false;
                }

                //Return the result
                return isValid;
            }

            private static string GetCombinationOfScopeAndPath(Scope scope, string pathOfOperation)
            {
                //Prepare the response
                string response = GetAbsolutePathForScope(scope);

                //If is not empty
                if (pathOfOperation != "")
                    response += "/" + pathOfOperation;

                //Return the response
                return response;
            }

            //Operations methos

            public static bool isFolderScannable(Scope scopeOfOperation, string folderPath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return true;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, folderPath) == false)
                    return true;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, folderPath);

                //------ Do the operation of Files API ------

                //Return if the folder is scannable
                return !File.Exists(absolutePathOfOperation + "/.nomedia");
            }

            public static void SetFolderAsScannable(Scope scopeOfOperation, string folderPath, bool scannableBySystem)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, folderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, folderPath);

                //------ Do the operation of Files API ------

                //If the scope is different from AppFiles or AppCache cancel
                if (scopeOfOperation != Scope.AppFiles && scopeOfOperation != Scope.AppCache)
                {
                    Debug.LogError("NAT: This operation could not be performed. You can only set the folder scan attribute in the \"AppFiles\" and \"AppCache\" scopes.");
                    return;
                }

                //If is desired to set scannable
                if (scannableBySystem == true)
                    if (File.Exists(absolutePathOfOperation + "/.nomedia") == true)
                        File.Delete(absolutePathOfOperation + "/.nomedia");
                //If is desired to set not scannable
                if (scannableBySystem == false)
                    File.WriteAllText(absolutePathOfOperation + "/.nomedia", "");
            }

            public static ItemsList GetListOfFilesAndFolders(Scope scopeOfOperation, string folderPath, bool showFullPath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new ItemsList(new string[] { }, new string[] { });
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, folderPath) == false)
                    return new ItemsList(new string[] { }, new string[] { });
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, folderPath);

                //------ Do the operation of Files API ------

                //If full path is not desired
                if (showFullPath == false)
                {
                    //Prepare the storage
                    ItemsList itemList = new ItemsList(new string[] { }, new string[] { });

                    //Get files
                    string[] folders = Directory.GetDirectories(absolutePathOfOperation);
                    string[] files = Directory.GetFiles(absolutePathOfOperation);

                    //Create new arrays
                    string[] foldersFixed = new string[folders.Length];
                    string[] filesFixed = new string[files.Length];

                    //Fill the fixed arrays
                    for (int i = 0; i < foldersFixed.Length; i++)
                        foldersFixed[i] = Path.GetFileName(folders[i]);
                    for (int i = 0; i < filesFixed.Length; i++)
                        filesFixed[i] = Path.GetFileName(files[i]);

                    //Fill the item list
                    itemList.folders = foldersFixed;
                    itemList.files = filesFixed;

                    //Return the result
                    return itemList;
                }
                //If full path is desired
                if (showFullPath == true)
                    return new ItemsList(Directory.GetDirectories(absolutePathOfOperation), Directory.GetFiles(absolutePathOfOperation));

                //Return empty
                return new ItemsList(new string[] { }, new string[] { });
            }

            public static bool Exists(Scope scopeOfOperation, string path, bool isFolder)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, path) == false)
                    return false;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, path);

                //------ Do the operation of Files API ------

                //Prepare the info
                bool exists = false;

                //If is file
                if (isFolder == false)
                    exists = File.Exists(absolutePathOfOperation);
                //If is folder
                if (isFolder == true)
                    exists = Directory.Exists(absolutePathOfOperation);

                //Return the result
                return exists;
            }

            public static Attributes GetAllAttributes(Scope scopeOfOperation, string path)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return null;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, path) == false)
                    return null;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, path);

                //------ Do the operation of Files API ------

                //Prepare the storage
                Attributes attributes = new Attributes();

                //Fill the attrbutes
                attributes.isFile = File.Exists(absolutePathOfOperation);
                attributes.isFolder = Directory.Exists(absolutePathOfOperation);
                if (attributes.isFile == true)
                    attributes.size = new Size(new FileInfo(absolutePathOfOperation).Length);
                if (attributes.isFolder == true)
                    attributes.size = new Size(GetDirectorySize(new DirectoryInfo(absolutePathOfOperation)));
                attributes.lastModify = File.GetLastWriteTime(absolutePathOfOperation);
                attributes.parentPath = Directory.GetParent(absolutePathOfOperation).FullName;
                attributes.isHidden = ((new FileInfo(absolutePathOfOperation).Attributes.HasFlag(FileAttributes.Hidden) == true) ? true : false);
                attributes.isWritable = ((new FileInfo(absolutePathOfOperation).Attributes.HasFlag(FileAttributes.ReadOnly) == false) ? true : false);
                attributes.isReadable = ((attributes.isFile == true || attributes.isFolder == true) ? true : false);
                attributes.pureName = Path.GetFileNameWithoutExtension(absolutePathOfOperation);
                attributes.extension = Path.GetExtension(absolutePathOfOperation).Replace(".", "");

                //Return the response
                return attributes;
            }

            public static void CopyTo(Scope targetFileOrFolderScope, string targetFileOrFolderPath, Scope destinationFolderScope, string destinationFolderPath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(targetFileOrFolderScope, targetFileOrFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(targetFileOrFolderScope, targetFileOrFolderPath);

                //------ Do the operation of Files API ------

                //Combine the path to get a absolute path to destination
                string absolutePathOfDestination = GetCombinationOfScopeAndPath(destinationFolderScope, destinationFolderPath);

                //If the target path is not a file or folder, cancel
                if (File.Exists(absolutePathOfOperation) == false && Directory.Exists(absolutePathOfOperation) == false)
                {
                    Debug.LogError("Could not perform copy operation. Apparently the target path to be copied does not exist.");
                    return;
                }
                //If the destination folder not exists, cancel
                if (Directory.Exists(absolutePathOfDestination) == false)
                {
                    Debug.LogError("Could not perform copy operation. Apparently the destination folder does not exist.");
                    return;
                }
                //If the target and destination are equals, cancel
                if (absolutePathOfOperation == absolutePathOfDestination)
                {
                    Debug.LogError("Could not perform copy operation. The target path for copying and the target path cannot be the same.");
                    return;
                }

                //Get the file or folder name
                string targetName = Path.GetFileName(absolutePathOfOperation);

                //If the target is a file
                if (File.Exists(absolutePathOfOperation) == true)
                    File.Copy(absolutePathOfOperation, absolutePathOfDestination + "/" + targetName);
                //If the target is a folder
                if (Directory.Exists(absolutePathOfOperation) == true)
                    CopyFilesRecursively(absolutePathOfOperation, absolutePathOfDestination + "/" + targetName);
            }

            public static void MoveTo(Scope targetFileOrFolderScope, string targetFileOrFolderPath, Scope destinationFolderScope, string destinationFolderPath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(targetFileOrFolderScope, targetFileOrFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(targetFileOrFolderScope, targetFileOrFolderPath);

                //------ Do the operation of Files API ------

                //Combine the path to get a absolute path to destination
                string absolutePathOfDestination = GetCombinationOfScopeAndPath(destinationFolderScope, destinationFolderPath);

                //If the target path is not a file or folder, cancel
                if (File.Exists(absolutePathOfOperation) == false && Directory.Exists(absolutePathOfOperation) == false)
                {
                    Debug.LogError("Could not perform move operation. Apparently the target path to be moved does not exist.");
                    return;
                }
                //If the destination folder not exists, cancel
                if (Directory.Exists(absolutePathOfDestination) == false)
                {
                    Debug.LogError("Could not perform move operation. Apparently the destination folder does not exist.");
                    return;
                }
                //If the target and destination are equals, cancel
                if (absolutePathOfOperation == absolutePathOfDestination)
                {
                    Debug.LogError("Could not perform move operation. The target path for moving and the target path cannot be the same.");
                    return;
                }

                //Get the file or folder name
                string targetName = Path.GetFileName(absolutePathOfOperation);

                //If the target is a file
                if (File.Exists(absolutePathOfOperation) == true)
                    File.Move(absolutePathOfOperation, absolutePathOfDestination + "/" + targetName);
                //If the target is a folder
                if (Directory.Exists(absolutePathOfOperation) == true)
                    Directory.Move(absolutePathOfOperation, absolutePathOfDestination + "/" + targetName);
            }

            public static void Rename(Scope scopeOfOperation, string targetFileOrFolderPath, string newNameOfFileOrFolder)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, targetFileOrFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, targetFileOrFolderPath);

                //------ Do the operation of Files API ------

                //If the target path is not a file or folder, cancel
                if (File.Exists(absolutePathOfOperation) == false && Directory.Exists(absolutePathOfOperation) == false)
                {
                    Debug.LogError("Could not perform rename operation. Apparently the target path to be renamed does not exist.");
                    return;
                }

                //Get the parent folder of this file or folder
                string parentFolder = Directory.GetParent(absolutePathOfOperation).FullName;

                //If is a file
                if (File.Exists(absolutePathOfOperation) == true)
                    File.Move(absolutePathOfOperation, parentFolder + "/" + newNameOfFileOrFolder);
                //If is a folder
                if (Directory.Exists(absolutePathOfOperation) == true)
                    Directory.Move(absolutePathOfOperation, parentFolder + "/" + newNameOfFileOrFolder);
            }

            public static void Delete(Scope scopeOfOperation, string targetFileOrFolderPath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, targetFileOrFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, targetFileOrFolderPath);

                //------ Do the operation of Files API ------

                //If is a file
                if (File.Exists(absolutePathOfOperation) == true)
                    File.Delete(absolutePathOfOperation);
                //If is a folder
                if (Directory.Exists(absolutePathOfOperation) == true)
                    Directory.Delete(absolutePathOfOperation, true);
            }

            public static void CreateFolder(Scope scopeOfOperation, string destinationFolderPath, string folderName)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, destinationFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, destinationFolderPath);

                //------ Do the operation of Files API ------

                //Create a new folder
                Directory.CreateDirectory(absolutePathOfOperation + "/" + folderName);
            }

            public static void CreateFile(Scope scopeOfOperation, string destinationFolderPath, string fileName, byte[] fileBytes)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, destinationFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, destinationFolderPath);

                //------ Do the operation of Files API ------

                //Create the new file
                File.WriteAllBytes(absolutePathOfOperation + "/" + fileName, fileBytes);
            }

            public static byte[] LoadAllBytesOfFile(Scope scopeOfOperation, string filePath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return null;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, filePath) == false)
                    return null;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, filePath);

                //------ Do the operation of Files API ------

                //Return the bytes of some file
                return File.ReadAllBytes(absolutePathOfOperation);
            }

            public static void WriteAllText(Scope scopeOfOperation, string destinationFolderPath, string fileName, string text)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, destinationFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, destinationFolderPath);

                //------ Do the operation of Files API ------

                //Write text into a file
                File.WriteAllText(absolutePathOfOperation + "/" + fileName, text);
            }

            public static string ReadAllTextOfFile(Scope scopeOfOperation, string filePath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return "";
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, filePath) == false)
                    return "";
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, filePath);

                //------ Do the operation of Files API ------

                //Read a file and return the text
                return File.ReadAllText(absolutePathOfOperation);
            }

            public static void WriteAllLines(Scope scopeOfOperation, string destinationFolderPath, string fileName, string[] textLines)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, destinationFolderPath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, destinationFolderPath);

                //------ Do the operation of Files API ------

                //Write all lines of text
                File.WriteAllLines(absolutePathOfOperation + "/" + fileName, textLines);
            }

            public static string[] ReadAllLinesOfFile(Scope scopeOfOperation, string filePath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return null;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, filePath) == false)
                    return null;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, filePath);

                //------ Do the operation of Files API ------

                //Read a file and return the text
                return File.ReadAllLines(absolutePathOfOperation);
            }

            public static void OpenWithDefaultApplication(Scope scopeOfOperation, string titleOfOpenBox, string filePath)
            {
                //Calls Files api to do the operation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
                //If this operation is not valid, return
                if (isThisOperationValid(scopeOfOperation, filePath) == false)
                    return;
                //Combine the path to get a absolute path to the operation
                string absolutePathOfOperation = GetCombinationOfScopeAndPath(scopeOfOperation, filePath);

                //------ Do the operation of Files API ------

                //Calls Java Side to open a file of storage, with the default application

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Opening a File With Default Application", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    javaClass.CallStatic("OpenWithDefaultApplication", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, titleOfOpenBox, absolutePathOfOperation);
                }
            }

            public class FilePickerOperation
            {
                //This class is a Builder for construct a operation with file picker

                //Data for operation
                private FilePickerInterfaceMode interfaceMode = FilePickerInterfaceMode.PortraitFullscreen;
                private string title = "";
                private FilePickerAction action = FilePickerAction.OpenFile;
                private string fileName = "";
                private string mimeType = "*/*";
                private FilePickerDefaultScope defaultScope = FilePickerDefaultScope.LeaveSystemDecide;

                public FilePickerOperation(FilePickerInterfaceMode filePickerInterfaceMode, string filerPickerTitle)
                {
                    this.interfaceMode = filePickerInterfaceMode;
                    this.title = filerPickerTitle;
                }

                public FilePickerOperation setCreateFileOperation(FilePickerDefaultScope defaultScope, string fileNameToCreate)
                {
                    this.action = FilePickerAction.CreateFile;
                    if (title == "")
                        title = "Select a Folder";
                    this.defaultScope = defaultScope;
                    this.fileName = (fileNameToCreate != "") ? fileNameToCreate : "fileNameToCreate.bin";
                    return this;
                }

                public FilePickerOperation setOpenFileOperation(FilePickerDefaultScope defaultScope)
                {
                    this.action = FilePickerAction.OpenFile;
                    if (title == "")
                        title = "Select a File";
                    this.defaultScope = defaultScope;
                    return this;
                }

                public FilePickerOperation setCustomMimeType(string type, string subtype)
                {
                    this.mimeType = ((type == "") ? "*" : type.Replace("/", "")) + "/" + ((subtype == "") ? "*" : subtype.Replace("/", ""));
                    return this;
                }

                public FilePickerOperation setMimeType(MimeType mimeType)
                {
                    string mimeTypeToApply = "";

                    switch (mimeType)
                    {
                        case MimeType.All:
                            mimeTypeToApply = "*/*";
                            break;
                        case MimeType.JSON:
                            mimeTypeToApply = "application/json";
                            break;
                        case MimeType.OGG:
                            mimeTypeToApply = "audio/ogg";
                            break;
                        case MimeType.PDF:
                            mimeTypeToApply = "application/pdf";
                            break;
                        case MimeType.RAR:
                            mimeTypeToApply = "application/rar";
                            break;
                        case MimeType.RTF:
                            mimeTypeToApply = "application/rtf";
                            break;
                        case MimeType.XML:
                            mimeTypeToApply = "application/xml";
                            break;
                        case MimeType.ZIP:
                            mimeTypeToApply = "application/zip";
                            break;
                        case MimeType.ThreeGP:
                            mimeTypeToApply = "video/3gpp";
                            break;
                        case MimeType.AMR:
                            mimeTypeToApply = "audio/amr";
                            break;
                        case MimeType.MID:
                            mimeTypeToApply = "audio/midi";
                            break;
                        case MimeType.WAV:
                            mimeTypeToApply = "audio/x-wav";
                            break;
                        case MimeType.MP3:
                            mimeTypeToApply = "audio/mpeg";
                            break;
                        case MimeType.WMA:
                            mimeTypeToApply = "audio/x-ms-wma";
                            break;
                        case MimeType.TTF:
                            mimeTypeToApply = "font/ttf";
                            break;
                        case MimeType.OTF:
                            mimeTypeToApply = "font/otf";
                            break;
                        case MimeType.GIF:
                            mimeTypeToApply = "image/gif";
                            break;
                        case MimeType.JPG:
                            mimeTypeToApply = "image/jpeg";
                            break;
                        case MimeType.PNG:
                            mimeTypeToApply = "image/png";
                            break;
                        case MimeType.SVG:
                            mimeTypeToApply = "image/svg+xml";
                            break;
                        case MimeType.TIF:
                            mimeTypeToApply = "image/tiff";
                            break;
                        case MimeType.ICO:
                            mimeTypeToApply = "image/vnd.microsoft.icon";
                            break;
                        case MimeType.BMP:
                            mimeTypeToApply = "image/x-ms-bmp";
                            break;
                        case MimeType.PSD:
                            mimeTypeToApply = "image/x-photoshop";
                            break;
                        case MimeType.CSS:
                            mimeTypeToApply = "text/css";
                            break;
                        case MimeType.CSV:
                            mimeTypeToApply = "text/csv";
                            break;
                        case MimeType.HTML:
                            mimeTypeToApply = "text/html";
                            break;
                        case MimeType.MD:
                            mimeTypeToApply = "text/markdown";
                            break;
                        case MimeType.TXT:
                            mimeTypeToApply = "text/plain";
                            break;
                        case MimeType.RTX:
                            mimeTypeToApply = "text/richtext";
                            break;
                        case MimeType.WML:
                            mimeTypeToApply = "text/vnd.wap.wml";
                            break;
                        case MimeType.BOO:
                            mimeTypeToApply = "text/x-boo";
                            break;
                        case MimeType.HPP:
                            mimeTypeToApply = "text/x-c++hdr";
                            break;
                        case MimeType.CPP:
                            mimeTypeToApply = "text/x-c++src";
                            break;
                        case MimeType.H:
                            mimeTypeToApply = "text/x-chdr";
                            break;
                        case MimeType.C:
                            mimeTypeToApply = "text/x-csrc";
                            break;
                        case MimeType.D:
                            mimeTypeToApply = "text/x-dsrc";
                            break;
                        case MimeType.HS:
                            mimeTypeToApply = "text/x-haskell";
                            break;
                        case MimeType.JAVA:
                            mimeTypeToApply = "text/x-java";
                            break;
                        case MimeType.MOC:
                            mimeTypeToApply = "text/x-moc";
                            break;
                        case MimeType.P:
                            mimeTypeToApply = "text/x-pascal";
                            break;
                        case MimeType.PL:
                            mimeTypeToApply = "text/x-perl";
                            break;
                        case MimeType.SH:
                            mimeTypeToApply = "text/x-sh";
                            break;
                        case MimeType.MPG:
                            mimeTypeToApply = "video/mpeg";
                            break;
                        case MimeType.MP4:
                            mimeTypeToApply = "video/mp4";
                            break;
                        case MimeType.QT:
                            mimeTypeToApply = "video/quicktime";
                            break;
                        case MimeType.MOV:
                            mimeTypeToApply = "video/quicktime";
                            break;
                        case MimeType.OGV:
                            mimeTypeToApply = "video/ogg";
                            break;
                        case MimeType.WEBM:
                            mimeTypeToApply = "video/webm";
                            break;
                        case MimeType.FLV:
                            mimeTypeToApply = "video/x-flv";
                            break;
                        case MimeType.WMV:
                            mimeTypeToApply = "video/x-ms-wmv";
                            break;
                        case MimeType.AVI:
                            mimeTypeToApply = "video/x-msvideo";
                            break;
                        case MimeType.MKV:
                            mimeTypeToApply = "video/x-matroska";
                            break;
                    }

                    this.mimeType = mimeTypeToApply;
                    return this;
                }

                //Calls native code to open the file picker
                public void OpenFilePicker()
                {
                    //Calls Java Side to open a file picker

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Opening File Picker", false);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Get file picker mode id
                        int filePickerMode = -1;
                        if (interfaceMode == FilePickerInterfaceMode.LandscapeFullscreen)
                            filePickerMode = 0;
                        if (interfaceMode == FilePickerInterfaceMode.PortraitFullscreen)
                            filePickerMode = 1;
                        //Get file picker default scope
                        string uriForDefaultScope = "";
                        switch (defaultScope)
                        {
                            case FilePickerDefaultScope.LeaveSystemDecide:
                                uriForDefaultScope = "";
                                break;
                            case FilePickerDefaultScope.DCIM:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3ADCIM";
                                break;
                            case FilePickerDefaultScope.Documents:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3ADocuments";
                                break;
                            case FilePickerDefaultScope.Movies:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3AMovies";
                                break;
                            case FilePickerDefaultScope.Music:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3AMusic";
                                break;
                            case FilePickerDefaultScope.Pictures:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3APictures";
                                break;
                            case FilePickerDefaultScope.Podcasts:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3APodcasts";
                                break;
                            case FilePickerDefaultScope.Ringtones:
                                uriForDefaultScope = "content://com.android.externalstorage.documents/tree/primary%3ARingtones";
                                break;
                        }

                        //If the desired action is create file
                        if (action == FilePickerAction.CreateFile)
                        {
                            AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                            javaClass.CallStatic("DoFilePickerOperation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, filePickerMode, 0, title, mimeType, fileName, uriForDefaultScope);
                        }
                        //If the desired action is open file
                        if (action == FilePickerAction.OpenFile)
                        {
                            AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                            javaClass.CallStatic("DoFilePickerOperation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, filePickerMode, 1, title, mimeType, fileName, uriForDefaultScope);
                        }
                    }
                }
            }

            //Private auxiliar methods

            private static long GetDirectorySize(DirectoryInfo d)
            {
                long size = 0;
                // Add file sizes.
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += GetDirectorySize(di);
                }
                return size;
            }

            private static void CopyFilesRecursively(string sourcePath, string targetPath)
            {
                //Create the base directory
                Directory.CreateDirectory(targetPath);

                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
            }
        }

        public class Tasks
        {
            //Subclasses

            public enum ExecutionMode
            {
                ForceToExecuteNow,
                ExecuteScheduled
            }

            //Core methods

            public static bool isRunningSomeTaskNow()
            {
                //Calls Java Side to return if the task is enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    return Files.Exists(Files.Scope.AppFiles, "NAT/Tasks/isRunningTask.lock", false);
                }

                //Return empty
                return false;
            }

            public static bool isTaskEnabled()
            {
#pragma warning disable 0162
                //Cancel the execution of this method if not have a Task programmed.
                if (TasksSourceCode.TASK_SOURCE_CODE == "")
                {
                    Debug.LogError("NAT: Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run. Please program what the Task should do through the NAT preferences window before using the Task API. See the documentation for more details.");
                    return false;
                }

                //Calls Java Side to return if the task is enabled

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;
#pragma warning restore 0162

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Tasks");
                    return javaClass.CallStatic<bool>("isTaskEnabled", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return empty
                return false;
            }

            public class ScheduledTask
            {
                //This class is a Builder for construct a task activation, defining time in future

                //Data for a task
                private ExecutionMode executionMode = ExecutionMode.ForceToExecuteNow;
                private string taskSourceCode;
                private Dictionary<string, string> runtimeValues = new Dictionary<string, string>();
                private int years;
                private int months;
                private int days;
                private int hours;
                private int minutes;

                public ScheduledTask(ExecutionMode executionMode)
                {
                    this.executionMode = executionMode;
                    this.taskSourceCode = TasksSourceCode.TASK_SOURCE_CODE;
                }

                public ScheduledTask setYearsInFuture(int years)
                {
                    this.years = years;
                    return this;
                }

                public ScheduledTask setMonthsInFuture(int months)
                {
                    this.months = months;
                    return this;
                }

                public ScheduledTask setDaysInFuture(int days)
                {
                    this.days = days;
                    return this;
                }

                public ScheduledTask setHoursInFuture(int hours)
                {
                    this.hours = hours;
                    return this;
                }

                public ScheduledTask setMinutesInFuture(int minutes)
                {
                    this.minutes = minutes;
                    return this;
                }

                public ScheduledTask setRuntimeValue(string key, string value)
                {
                    if (runtimeValues.ContainsKey(key) == false)
                        runtimeValues.Add(key, value);
                    return this;
                }

                public ScheduledTask setRuntimeValue(string key, int value)
                {
                    if (runtimeValues.ContainsKey(key) == false)
                        runtimeValues.Add(key, value.ToString());
                    return this;
                }

                public ScheduledTask setRuntimeValue(string key, float value)
                {
                    if (runtimeValues.ContainsKey(key) == false)
                        runtimeValues.Add(key, value.ToString());
                    return this;
                }

                public ScheduledTask setRuntimeValue(string key, bool value)
                {
                    if (runtimeValues.ContainsKey(key) == false)
                        runtimeValues.Add(key, value.ToString());
                    return this;
                }

                //Calls native code to schedule task in future
                public void EnableThisTask()
                {
#pragma warning disable 0162
                    //Cancel the execution of this method if not have a Task programmed.
                    if (TasksSourceCode.TASK_SOURCE_CODE == "")
                    {
                        Debug.LogError("NAT: Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run. Please program what the Task should do through the NAT preferences window before using the Task API. See the documentation for more details.");
                        return;
                    }

                    //Calls Java Side to enable the task

                    //If asset is not initialized or have a problem, stop execution
                    if (canContinueToCallJavaSideMethod() == false)
                        return;
#pragma warning restore 0162

                    //If is EDITOR
                    if (Application.isEditor == true)
                    {
                        NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Enabling Task", true);
                    }
                    //If is ANDROID
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        //Get mode
                        int mode = -1;
                        if (executionMode == ExecutionMode.ForceToExecuteNow)
                            mode = 0;
                        if (executionMode == ExecutionMode.ExecuteScheduled)
                            mode = 1;

                        //Get task source code with runtime values applied
                        string taskSourceCodeWithRuntimeValues = taskSourceCode;
                        foreach (var key in runtimeValues)
                            taskSourceCodeWithRuntimeValues = taskSourceCodeWithRuntimeValues.Replace("%" + key.Key + "%", key.Value);

                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Tasks");
                        javaClass.CallStatic("EnableTask", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, mode, taskSourceCodeWithRuntimeValues, years, months, days, hours, minutes);
                    }
                }
            }

            public static void DisableTask()
            {
#pragma warning disable 0162
                //Cancel the execution of this method if not have a Task programmed.
                if (TasksSourceCode.TASK_SOURCE_CODE == "")
                {
                    Debug.LogError("NAT: Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run. Please program what the Task should do through the NAT preferences window before using the Task API. See the documentation for more details.");
                    return;
                }

                //Calls Java Side to disable the task

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;
#pragma warning restore 0162

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Disabling Task", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Tasks");
                    javaClass.CallStatic("DisableTask", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }
            }
        }

        public class AudioPlayer
        {
            //Subclasses

            public class AudioData
            {
                //This class stores a data of a audio

                //Public variables
                public string path = "";
                public bool isAudio = false;
                public string title = "";
                public string artist = "";
                public string album = "";
                public DateTime.Calendar duration;
            }

            //Core methods

            public static AudioData GetAudioDataWithoutPlay(Files.Scope scopeOfOperation, string filePath)
            {
                //Calls Java Side to return the audio data and informations

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return new AudioData();

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //Read a audio file and return the information
                    AudioData audioData = new AudioData();
                    audioData.path = Files.GetAbsolutePathForScope(scopeOfOperation) + "/" + filePath;
                    audioData.isAudio = true;
                    audioData.title = "Demo Name";
                    audioData.artist = "Demo Artist";
                    audioData.album = "Demo Album";
                    audioData.duration = new DateTime.Calendar(0, 0, 0, 0, 1, 30);
                    return audioData;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If the scope is not available, return empty and inform error
                    if (Files.GetScopeAvailability(scopeOfOperation) != Files.ScopeStatus.AvailableToUse || File.Exists(Files.GetAbsolutePathForScope(scopeOfOperation) + "/" + filePath) == false)
                    {
                        Debug.LogError("Unable to get Audio data in \"" + filePath + "\" because scope " + scopeOfOperation + " is not available at the moment or the file not exists in this scope.");
                        return new AudioData();
                    }

                    //Get the audio data
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    string audioDataString = javaClass.CallStatic<string>("GetAudioDataWithoutPlay", (Files.GetAbsolutePathForScope(scopeOfOperation) + "/" + filePath));

                    //Extract audio data and return
                    string[] audioDataParts = audioDataString.Split('§');
                    AudioData audioData = new AudioData();
                    audioData.path = audioDataParts[0];
                    audioData.isAudio = bool.Parse(audioDataParts[1]);
                    audioData.title = (audioDataParts[2].ToLower() == "null") ? "" : audioDataParts[2];
                    audioData.artist = (audioDataParts[3].ToLower() == "null") ? "" : audioDataParts[3];
                    audioData.album = (audioDataParts[4].ToLower() == "null") ? "" : audioDataParts[4];
                    audioData.duration = (audioDataParts[5].ToLower() == "null") ? new DateTime.Calendar(0) : new DateTime.Calendar(long.Parse(audioDataParts[5]));
                    return audioData;
                }

                //Return empty if not run nothing
                return new AudioData();
            }

            public static bool isPlayingAudio()
            {
                //Calls Java Side to return if the audio player is playing

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isPlayingAudio();
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    return javaClass.CallStatic<bool>("isPlayingAudio");
                }

                //Return false if not run nothing
                return false;
            }

            public static void PlayAudio(Files.Scope scopeOfOperation, string filePath)
            {
                //Calls Java Side to play the audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.PlayAudio();
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Playing Audio", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If the scope is not available, return empty and inform error
                    if (Files.GetScopeAvailability(scopeOfOperation) != Files.ScopeStatus.AvailableToUse || File.Exists(Files.GetAbsolutePathForScope(scopeOfOperation) + "/" + filePath) == false)
                    {
                        Debug.LogError("Unable to play Audio in \"" + filePath + "\" because scope " + scopeOfOperation + " is not available at the moment or the file not exists in this scope.");
                        return;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("PlayAudio", (Files.GetAbsolutePathForScope(scopeOfOperation) + "/" + filePath));
                }
            }

            public static void PlayAudio()
            {
                //Calls Java Side to resume audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Resuming Audio", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is not playing audio now, cancel the call
                    if (isPlayingAudio() == false)
                    {
                        Debug.LogError("Unable to resume Audio. There is no audio currently playing.");
                        return;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("PlayAudio", "");
                }
            }

            public static void PauseAudio()
            {
                //Calls Java Side to pause audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    if (NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isPlayingAudio() == false)
                        return;
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Pausing Audio", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("PauseAudio");
                }
            }

            public static float GetAudioPart()
            {
                //Calls Java Side to return the audio part

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return 0;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return 0.35f;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    return javaClass.CallStatic<float>("GetAudioPart");
                }

                //Return false if not run nothing
                return 0;
            }

            public static void SetAudioPart(float audioPart)
            {
                //Calls Java Side to set the part of the audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    Debug.Log("Seeking for " + (audioPart * 100.0f) + "% part of the audio...");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("SetAudioPart", audioPart);
                }
            }

            public static void SetAudioVolume(float leftVolume, float rightVolume)
            {
                //Calls Java Side to set the part of the audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    Debug.Log("Setting volume as " + leftVolume + "/" + rightVolume + "...");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("SetAudioVolume", leftVolume, rightVolume);
                }
            }

            public static void StopAudio()
            {
                //Calls Java Side to stop audio

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    if (NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.isPlayingAudio() == false)
                        return;
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.StopAudio();
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Stopping Audio", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.AudioPlayer");
                    javaClass.CallStatic("StopAudio");
                }
            }
        }

        public class PowerManager
        {
            //Core methods

            public static bool isExactAlarmsAndTasksAllowed()
            {
                //Calls Java Side to return if this app can run accuracy notifications and tasks

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return true;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PowerManager");
                    return javaClass.CallStatic<bool>("isExactAlarmsAndTasksAllowed", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext);
                }

                //Return false if not run nothing
                return false;
            }

            public static void OpenAlarmsAndRemindersAccess()
            {
                //Calls Java Side to open the screen of alarms and reminders access

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.OpenSimulatedActivity("This App Alarms And Reminders Screen");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PowerManager");
                    javaClass.CallStatic("OpenAlarmsAndRemindersAccess", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static bool isThisDeviceProblematic()
            {
                //Calls Java Side to return if this device is considered problematic or not

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //Return that the Editor is not problematic
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Collect the device manufacturer and model information
                    string deviceManufacturer = Utils.GetDeviceManufacturer().ToLower();
                    string deviceModel = Utils.GetDeviceModel().ToLower();

                    //Prepare the response
                    bool isProblematic = false;

                    //Check if is a problematic device
                    if (deviceManufacturer == "samsung")
                        isProblematic = true;
                    if (deviceManufacturer == "oneplus")
                        isProblematic = true;
                    if (deviceManufacturer == "huawei")
                        isProblematic = true;
                    if (deviceManufacturer == "xiaomi")
                    {
                        //Check if this device is from Android One
                        bool isFromAndroidOne = new string[] { "mdg2", "m1804d2sg", "m1804d2si", "m1906f9sh", "m1906f9si", "mzb5645in", "mzb5717in" }.Contains(deviceModel);

                        //If is of Android One
                        if (isFromAndroidOne == true)
                            isProblematic = false;
                        //If is not from Android One
                        if (isFromAndroidOne == false)
                            isProblematic = true;
                    }
                    if (deviceManufacturer == "meizu")
                        isProblematic = true;
                    if (deviceManufacturer == "asus")
                        isProblematic = true;
                    if (deviceManufacturer == "wiko")
                        isProblematic = true;
                    if (deviceManufacturer == "lenovo")
                        isProblematic = true;
                    if (deviceManufacturer == "oppo")
                        isProblematic = true;
                    if (deviceManufacturer == "vivo")
                        isProblematic = true;
                    if (deviceManufacturer == "realme")
                        isProblematic = true;
                    if (deviceManufacturer == "blackview")
                        isProblematic = true;
                    if (deviceManufacturer == "sony")
                        isProblematic = true;
                    if (deviceManufacturer == "unihertz")
                        isProblematic = true;

                    //If this device is android 6.0 or older, set as not problematic automatically, as the older versions not have problems with battery optimzations
                    if (Utils.GetDeviceAndroidVersionCode() <= 23)
                        isProblematic = false;

                    //Return the response
                    return isProblematic;
                }

                //Return false if not run nothing
                return false;
            }

            public static void RequestAutoStartONIfProblematic()
            {
                //Calls Java Side to open the screen of alarms and reminders access

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Requesting Auto Start ON", true);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If this device is not problematic, return
                    if (isThisDeviceProblematic() == false)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PowerManager");
                    javaClass.CallStatic("RequestAutoStartONIfProblematic", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }
        }

        public class PlayGames
        {
            //Subclasses

            public class UserData
            {
                //This class stores all data about a player signed in Play Games in this app
                public NAT.DateTime.Calendar lastUserDataUpdate = new DateTime.Calendar();
                public string displayName = "";
                public string title = "";
                public string playerId = "";
                public bool hasIconImage = false;
                public string iconImagePathInAppFilesScope = "";
                public int currentLevel = 0;
                public long currentLevelMinXp = 0;
                public long currentLevelMaxXp = 0;
                public long currentXpTotal = 0;
                public NAT.DateTime.Calendar lastLevelUp = new DateTime.Calendar();
                public int nextLevel = 0;
                public long nextLevelMinXp = 0;
                public long nextLevelMaxXp = 0;
                public bool isMaxLevel = false;

                //Auxiliar methods

                public Texture2D GetUserIconImage()
                {
                    //If the user not have a user icon, cancel
                    if (hasIconImage == false)
                        return null;

                    //Get play games user icon into a texture 2D
                    Texture2D playGamesUserIcon = new Texture2D(2, 2);
                    playGamesUserIcon.LoadImage(NAT.Files.LoadAllBytesOfFile(Files.Scope.AppFiles, iconImagePathInAppFilesScope));
                    return playGamesUserIcon;
                }
            }

            public class EventData
            {
                //This class stores all data about a event of Play Games in this app
                public string name = "";
                public PlayGamesResources.Event id = PlayGamesResources.Event.NONE;
                public string description = "";
                public string formattedValue = "";
                public long unformattedValue = 0;
                public bool isVisible = false;
                public string iconImagePathInAppFilesScope = "";

                //Auxiliar methods

                public Texture2D GetEventIconImage()
                {
                    //If the event not have a icon, cancel
                    if (Files.Exists(Files.Scope.AppFiles, iconImagePathInAppFilesScope, false) == false)
                        return null;

                    //Get play games event icon into a texture 2D
                    Texture2D playGamesEventIcon = new Texture2D(2, 2);
                    playGamesEventIcon.LoadImage(NAT.Files.LoadAllBytesOfFile(Files.Scope.AppFiles, iconImagePathInAppFilesScope));
                    return playGamesEventIcon;
                }
            }

            public enum FriendListAccessibility
            {
                Accessible,
                Inacessible,
                NowChecking
            }

            public enum FriendAccessInterfaceMode
            {
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public class Friend
            {
                //This class stores all data about a player signed in Play Games in this app
                public NAT.DateTime.Calendar lastUserDataUpdate = new DateTime.Calendar();
                public string displayName = "";
                public string title = "";
                public string playerId = "";
                public int currentLevel = 0;
                public bool hasIconImage = false;
                public string iconImagePathInAppFilesScope = "";

                //Auxiliar methods

                public Texture2D GetFriendIconImage()
                {
                    //If the friend not have a user icon, cancel
                    if (hasIconImage == false)
                        return null;

                    //Get play games user icon into a texture 2D
                    Texture2D playGamesUserIcon = new Texture2D(2, 2);
                    playGamesUserIcon.LoadImage(NAT.Files.LoadAllBytesOfFile(Files.Scope.AppFiles, iconImagePathInAppFilesScope));
                    return playGamesUserIcon;
                }
            }

            public class FriendList
            {
                //This class stores all friend list info
                public Friend[] allUserFriends = null;
            }

            public enum CloudSaveInterfaceMode
            {
                PortraitFullscreen,
                LandscapeFullscreen
            }

            public enum CloudSaveUIResponse
            {
                Error,
                Canceled,
                LoadSave,
                CreateNew
            }

            public enum CloudSaveConflictResolution
            {
                ResolveByMostRecentlyModify,
                ResolveByLongestPlayTime,
                ResolveByHighestProgressValue
            }

            public enum CloudSaveLoadStatus
            {
                Unknown,
                Success,
                ErrorOnFind,
                ErrorOnRead
            }

            //Core methods

            public static bool isSignedIn()
            {
                //Calls Java Side to return if the user is currently signed in play games

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return false;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return false;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    return javaClass.CallStatic<bool>("isSignedIn");
                }

                //Return false if not run nothing
                return false;
            }

            public static void DoManualSignIn()
            {
                //Calls Java Side to do the manual sign in on google play games

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Doing Manual Play Games Sign In...", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("DoManualSignIn", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void ShowAllAchievements()
            {
                //Calls Java Side to do the show all achievements of user

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Showing All Achievements", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("ShowAllAchievements", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity);
                }
            }

            public static void RevealAchievement(PlayGamesResources.Achievement achievementId)
            {
                //Calls Java Side to revel a play games achievement

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Revealing Achievement: " + achievementId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("RevealAchievement", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetAchievementStringID(achievementId));
                }
            }

            public static void UnlockAchievement(PlayGamesResources.Achievement achievementId)
            {
                //Calls Java Side to unlock a play games achievement

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Unlocking Achievement: " + achievementId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("UnlockAchievement", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetAchievementStringID(achievementId));
                }
            }

            public static void IncrementAchievement(PlayGamesResources.Achievement achievementId, int quantityToIncrement)
            {
                //Calls Java Side to increment a play games achievement

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Incrementing Achievement: " + achievementId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (achievementId == PlayGamesResources.Achievement.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("IncrementAchievement", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetAchievementStringID(achievementId), quantityToIncrement);
                }
            }

            public static void LoadEventData(PlayGamesResources.Event eventId, bool clearCacheAndLoad)
            {
                //Calls Java Side to load event data

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (eventId == PlayGamesResources.Event.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Loading Event Data: " + eventId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (eventId == PlayGamesResources.Event.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("LoadEventData", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetEventStringID(eventId), clearCacheAndLoad);
                }
            }

            public static void IncrementEvent(PlayGamesResources.Event eventId, int quantityToIncrement)
            {
                //Calls Java Side to increment a event

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (eventId == PlayGamesResources.Event.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Incrementing Event: " + eventId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (eventId == PlayGamesResources.Event.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("IncrementEvent", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetEventStringID(eventId), quantityToIncrement);
                }
            }

            public static void ShowTheLeaderboard(PlayGamesResources.Leaderboard leaderboardId)
            {
                //Calls Java Side to show a leaderboard

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (leaderboardId == PlayGamesResources.Leaderboard.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Showing Leaderboard: " + leaderboardId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (leaderboardId == PlayGamesResources.Leaderboard.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("ShowTheLeaderboard", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetLeaderboardStringID(leaderboardId));
                }
            }

            public static void SubmitScoreToLeaderboard(PlayGamesResources.Leaderboard leaderboardId, int score)
            {
                //Calls Java Side to submit score to leaderboard

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    //If is NONE, return
                    if (leaderboardId == PlayGamesResources.Leaderboard.NONE)
                        return;

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Submiting Score To Leaderboard: " + leaderboardId.ToString(), false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //If is NONE, return
                    if (leaderboardId == PlayGamesResources.Leaderboard.NONE)
                        return;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("SubmitScoreToLeaderboard", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, GPGSLowLevelResources.GetLeaderboardStringID(leaderboardId), score);
                }
            }

            public static FriendListAccessibility isFriendListAccessible()
            {
                //Calls Java Side to return if the friends list of the user is acessible to this app

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return FriendListAccessibility.Inacessible;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    return FriendListAccessibility.Inacessible;
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    int accessibility = javaClass.CallStatic<int>("isFriendListAccessible");

                    if (accessibility == 0)
                        return FriendListAccessibility.NowChecking;
                    if (accessibility == 1)
                        return FriendListAccessibility.Accessible;
                    if (accessibility == 2)
                        return FriendListAccessibility.Inacessible;

                    return FriendListAccessibility.Inacessible;
                }

                //Return false if not run nothing
                return FriendListAccessibility.Inacessible;
            }

            public static void RequestFriendListAccess(FriendAccessInterfaceMode accessRequestInterfaceMode)
            {
                //Calls Java Side to request access to user friend list

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.RestartApplication("Requesting Access To Friend List");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get restarter mode id
                    int interfaceMode = -1;
                    if (accessRequestInterfaceMode == FriendAccessInterfaceMode.LandscapeFullscreen)
                        interfaceMode = 0;
                    if (accessRequestInterfaceMode == FriendAccessInterfaceMode.PortraitFullscreen)
                        interfaceMode = 1;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("RequestFriendListAccess", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, interfaceMode);
                }
            }

            public static void LoadUserFriendList(bool clearCacheAndLoad)
            {
                //Calls Java Side to load user friend list

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Loading User Friend List", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("LoadUserFriendList", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, clearCacheAndLoad);
                }
            }

            public static void OpenProfileComparation(string theOtherPlayerId, string theOtherPlayerInGameName, string currentPlayerInGameName)
            {
                //Calls Java Side to open profile comparation

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.ShowToast("Opening Profile Comparation", false);
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("OpenProfileComparation", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, theOtherPlayerId, theOtherPlayerInGameName, currentPlayerInGameName);
                }
            }

            public static void OpenCloudSaveUI(CloudSaveInterfaceMode cloudSaveInterfaceMode, string cloudSaveUiTitle, bool allowCreate, bool allowDelete)
            {
                //Calls Java Side to request open cloud save ui

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.RestartApplication("Opening Cloud Save UI");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get restarter mode id
                    int interfaceMode = -1;
                    if (cloudSaveInterfaceMode == CloudSaveInterfaceMode.LandscapeFullscreen)
                        interfaceMode = 0;
                    if (cloudSaveInterfaceMode == CloudSaveInterfaceMode.PortraitFullscreen)
                        interfaceMode = 1;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("OpenCloudSaveUI", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, interfaceMode, cloudSaveUiTitle, allowCreate, allowDelete);
                }
            }

            public static void CreateFileOnCloudSave(string fileNameToBeCreated, string saveDescription, long progressValue, NAT.DateTime.Calendar playedTime, byte[] saveGameBytes, Texture2D saveCover)
            {
                //Calls Java Side to save file to cloud

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.RestartApplication("Saving To Play Games Cloud");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Process the save game bytes and cover
                    Files.CreateFile(Files.Scope.AppFiles, "NAT/GPGS", "tempSaveFile.gps", saveGameBytes);
                    Files.CreateFile(Files.Scope.AppFiles, "NAT/GPGS", "tempCoverFile.gps", saveCover.EncodeToPNG());

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("CreateFileOnCloudSave", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, fileNameToBeCreated, saveDescription, progressValue, playedTime.GetUnixMillisTime(DateTime.TimeMode.LocalTime));
                }
            }

            public static void ReadFileOfCloudSave(string fileNameToBeLoaded, CloudSaveConflictResolution conflictResolutionPolicy)
            {
                //Calls Java Side to load file from cloud

                //If asset is not initialized or have a problem, stop execution
                if (canContinueToCallJavaSideMethod() == false)
                    return;

                //If is EDITOR
                if (Application.isEditor == true)
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.RestartApplication("Loading From Play Games Cloud");
                }
                //If is ANDROID
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    int conflictResolution = 0;
                    if (conflictResolutionPolicy == CloudSaveConflictResolution.ResolveByMostRecentlyModify)
                        conflictResolution = 0;
                    if (conflictResolutionPolicy == CloudSaveConflictResolution.ResolveByLongestPlayTime)
                        conflictResolution = 1;
                    if (conflictResolutionPolicy == CloudSaveConflictResolution.ResolveByHighestProgressValue)
                        conflictResolution = 2;

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.PlayGames");
                    javaClass.CallStatic("ReadFileOfCloudSave", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerActivity, fileNameToBeLoaded, conflictResolution);
                }
            }
        }
    }

    public class NATEvents
    {
        /*
        * This class stores delegates that are used by returns of Java Side of NAT or Interface Emulation
        */

        //---------------- Delegates Container -------------------

        public class NATDelegates
        {
            public delegate void OnInitializeNAT_PostInitialize(bool isSuccessfully);

            public delegate void OnSimpleAlertDialogOk();
            public delegate void OnSimpleAlertDialogCancel();

            public delegate void OnConfirmationDialogYes();
            public delegate void OnConfirmationDialogNo();
            public delegate void OnConfirmationDialogCancel();

            public delegate void OnNeutralDialogYes();
            public delegate void OnNeutralDialogNo();
            public delegate void OnNeutralDialogNeutral();
            public delegate void OnNeutralDialogCancel();

            public delegate void OnRadialListDialogDone(int result);
            public delegate void OnRadialListDialogCancel();

            public delegate void OnCheckboxListDialogDone(bool[] result);
            public delegate void OnCheckboxListDialogCancel();

            public delegate void OnOpenApplicationByNotificationIteraction_PostInitialize(NotificationsActions.Action notificationAction);

            public delegate void OnCompleteScreenshotTexture2dProcessing(Texture2D texture);

            public delegate void OnPopUpWebviewClose(NAT.Webview.WebviewBrowsing webviewBrowsing);
            public delegate void OnResumeApplicationAfterCloseFullscreenWebview_PostInitialize(NAT.Webview.WebviewBrowsing webviewBrowsing);
            public delegate void OnWebviewGettedAllCookiesFromURL(bool successfully, NAT.Webview.WebviewCookie[] cookies, string pageContent);

            public delegate void OnLocationChanged(NAT.Location.LocationProvider provider, NAT.Location.LocationData data);
            public delegate void OnLocationProviderChanged(NAT.Location.LocationProvider provider, bool isEnabledNow);

            public delegate void OnGoogleMapsLoaded();
            public delegate void OnGoogleMapsClosed();
            public delegate void OnGoogleMapsClick(double longitude, double latitude);

            public delegate void OnCameraTakePhoto(Texture2D resultPhoto);
            public delegate void OnCameraReadCode(string resultOfScannedCode);
            public delegate void OnCameraRecorded(string resultVideoPath);
            public delegate void OnCameraClose();

            public delegate void OnMicrophoneStopRecording(string resultRecordPath);

            public delegate void OnMicrophoneSpeechToTextStarted();
            public delegate void OnMicrophoneSpeechToTextFinished(NAT.Microphone.SpeechToTextResult speechToTextResult, string resultText);

            public delegate void OnDateTimeHourPicked(NAT.DateTime.Calendar hourPicked);
            public delegate void OnDateTimeDatePicked(NAT.DateTime.Calendar datePicked);
            public delegate void OnDateTimeDoneNTPRequest(bool success, NAT.DateTime.Calendar responseDateTime, string responseServer);
            public delegate void OnDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize(NAT.DateTime.TimeElapsedWhileClosed timeElapsedWhileClosed);
            public delegate void OnDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume(NAT.DateTime.TimeElapsedWhilePaused timeElapsedWhilePaused);

            public delegate void OnFilesFilePickerOperationFinished(NAT.Files.FilePickerOperationStatus filePickerOperationStatus, NAT.Files.FilePickerOperationResponse filePickerOperationResponse);

            public delegate void OnAudioPlayerFinishedPlaying();

            public delegate void OnPlayGamesInitializationComplete(bool userSignedIn, NAT.PlayGames.UserData userData);
            public delegate void OnPlayGamesEventDataLoaded(bool isSuccessfully, NAT.PlayGames.EventData eventData);
            public delegate void OnPlayGamesFriendListRequestResult(bool isUserFriendListAccessibleNow);
            public delegate void OnPlayGamesUserFriendListLoaded(NAT.PlayGames.FriendList userFriendList);
            public delegate void OnPlayGamesCloudSaveUIResult(NAT.PlayGames.CloudSaveUIResponse cloudSaveResult, string fileNameToBeCreatedOrLoaded);
            public delegate void OnPlayGamesCloudSaveCreateFileResult(bool isSuccessfully);
            public delegate void OnPlayGamesCloudSaveReadFileResult(NAT.PlayGames.CloudSaveLoadStatus loadStatus, byte[] saveGameLoadedBytes);
        }

        //---------------- NAT General -------------------

        //On initialize NAT
        public static NATDelegates.OnInitializeNAT_PostInitialize onInitializeNAT_PostInitialize; //<- Only will be called by the NAT C# code (after NAT initialization), never by Java code

        //---------------- Dialogs -------------------

        //Simple Alert Dialog
        public static NATDelegates.OnSimpleAlertDialogOk onSimpleAlertDialogOk;
        public static NATDelegates.OnSimpleAlertDialogCancel onSimpleAlertDialogCancel;

        //Confirmation Dialog
        public static NATDelegates.OnConfirmationDialogYes onConfirmationDialogYes;
        public static NATDelegates.OnConfirmationDialogNo onConfirmationDialogNo;
        public static NATDelegates.OnConfirmationDialogCancel onConfirmationDialogCancel;

        //Neutral dialog
        public static NATDelegates.OnNeutralDialogYes onNeutralDialogYes;
        public static NATDelegates.OnNeutralDialogNo onNeutralDialogNo;
        public static NATDelegates.OnNeutralDialogNeutral onNeutralDialogNeutral;
        public static NATDelegates.OnNeutralDialogCancel onNeutralDialogCancel;

        //Radial List Dialog
        public static NATDelegates.OnRadialListDialogDone onRadialListDialogDone;
        public static NATDelegates.OnRadialListDialogCancel onRadialListDialogCancel;

        //Checkbox list dialog
        public static NATDelegates.OnCheckboxListDialogDone onCheckboxListDialogDone;
        public static NATDelegates.OnCheckboxListDialogCancel onCheckboxListDialogCancel;

        //---------------- Notifications -------------------

        public static NATDelegates.OnOpenApplicationByNotificationIteraction_PostInitialize onOpenApplicationByNotificationIteraction_PostInitialize; //<- Only will be called by the NAT C# code (after NAT initialization), never by Java code

        //---------------- Sharing -------------------

        //Take screenshot and get texture 2D
        public static NATDelegates.OnCompleteScreenshotTexture2dProcessing onCompleteScreenshotTexture2dProcessing;

        //---------------- Webview -------------------

        //PopUp webview
        public static NATDelegates.OnPopUpWebviewClose onPopUpWebviewClose;
        public static NATDelegates.OnResumeApplicationAfterCloseFullscreenWebview_PostInitialize onResumeApplicationAfterCloseFullscreenWebview_PostInitialize; //<- Only will be called by the NAT C# code (after NAT initialization), never by Java code
        public static NATDelegates.OnWebviewGettedAllCookiesFromURL onWebviewGettedAllCookiesFromURL;

        //---------------- Location -------------------

        //Location Tracking
        public static NATDelegates.OnLocationChanged onLocationChanged;
        public static NATDelegates.OnLocationProviderChanged onLocationProviderChanged;

        //Google Maps
        public static NATDelegates.OnGoogleMapsLoaded onGoogleMapsLoaded;
        public static NATDelegates.OnGoogleMapsClosed onGoogleMapsClosed;
        public static NATDelegates.OnGoogleMapsClick onGoogleMapsClick;

        //---------------- Camera -------------------

        //PopUp Camera
        public static NATDelegates.OnCameraTakePhoto onCameraTakePhoto;
        public static NATDelegates.OnCameraReadCode onCameraReadCode;
        public static NATDelegates.OnCameraRecorded onCameraRecorded;
        public static NATDelegates.OnCameraClose onCameraClose;

        //---------------- Microphone -------------------

        //Microphone recording
        public static NATDelegates.OnMicrophoneStopRecording onMicrophoneStopRecording;

        //Speech To Text
        public static NATDelegates.OnMicrophoneSpeechToTextStarted onMicrophoneSpeechToTextStarted;
        public static NATDelegates.OnMicrophoneSpeechToTextFinished onMicrophoneSpeechToTextFinished;

        //---------------- DateTime -------------------

        //Hour Picker
        public static NATDelegates.OnDateTimeHourPicked onDateTimeHourPicked;

        //Date Picker
        public static NATDelegates.OnDateTimeDatePicked onDateTimeDatePicked;

        //NTP Request
        public static NATDelegates.OnDateTimeDoneNTPRequest onDateTimeDoneNTPRequest;

        //On get time elapsed since last close of this app until this run
        public static NATDelegates.OnDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize; //<- Only will be called by the NAT C# code (after NAT initialization), never by Java code

        //On get time elapsed since last pause of this app until this resume
        public static NATDelegates.OnDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume onDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume; //<- Only will be called by the NAT C# code (after APP resume), never by Java code

        //---------------- Files -------------------

        //On finish the activity of file picker operation
        public static NATDelegates.OnFilesFilePickerOperationFinished onFilesFilePickerOperationFinished;

        //---------------- AudioPlayer -------------------

        //Audio Finished
        public static NATDelegates.OnAudioPlayerFinishedPlaying onAudioPlayerFinishedPlaying;

        //---------------- PlayGames -------------------

        //Play Games initialization complete
        public static NATDelegates.OnPlayGamesInitializationComplete onPlayGamesInitializationComplete;

        //Play Games on event data loaded successfully
        public static NATDelegates.OnPlayGamesEventDataLoaded onPlayGamesEventDataLoaded;

        //Play Games on result of Friend List Access Request
        public static NATDelegates.OnPlayGamesFriendListRequestResult onPlayGamesFriendListRequestResult;

        //Play Games on friend list load finished
        public static NATDelegates.OnPlayGamesUserFriendListLoaded onPlayGamesUserFriendListLoaded;

        //Play Games on cloud save UI result
        public static NATDelegates.OnPlayGamesCloudSaveUIResult onPlayGamesCloudSaveUIResult;

        //Play Games on cloud save create file result
        public static NATDelegates.OnPlayGamesCloudSaveCreateFileResult onPlayGamesCloudSaveCreateFileResult;

        //Play Games on cloud save load file result
        public static NATDelegates.OnPlayGamesCloudSaveReadFileResult onPlayGamesCloudSaveReadFileResult;
    }
}