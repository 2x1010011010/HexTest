using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;

namespace MTAssets.NativeAndroidToolkit
{
    public class DemoNAT : MonoBehaviour
    {
        //Private Variables
        private DemoNATMessages demoNATMessages = null;

        // --- This Script Methods ---//

        private void Start()
        {
            //Get instance of demon NAT messages script
            demoNATMessages = this.gameObject.GetComponentInChildren<DemoNATMessages>();
            //Unlock frame rate of demo scene
            Application.targetFrameRate = 75;




            //Register event that shows that NAT was initialized
            NATEvents.onInitializeNAT_PostInitialize += (bool isSuccessfully) =>
            {
                if (isSuccessfully == true)
                    NAT.Notifications.ShowToast("NAT was successfully initialized on Unity and Native side!", true);
                if (isSuccessfully == false)
                    NAT.Notifications.ShowToast("There was a problem on initializing the NAT! :(", true);
            };
            //Register events that possibelly can be called by NAT after it initialize (events with suffix "PostInitialize" must be registered before call NativeAndroidToolkit.Initialize();)
            NATEvents.onResumeApplicationAfterCloseFullscreenWebview_PostInitialize += (NAT.Webview.WebviewBrowsing webviewBrowsing) =>
            {
                string navigatedSites = "";
                foreach (string str in webviewBrowsing.browsedSites)
                    navigatedSites += "\n" + str;

                NAT.Notifications.SendNotification("Fullscreen Webview", "The application was resumed after closing a Webview in Fullscreen! Browsed sites is...\n" + navigatedSites);
            };
            NATEvents.onOpenApplicationByNotificationIteraction_PostInitialize += (NotificationsActions.Action action) =>
            {
                NAT.Dialogs.ShowSimpleAlertDialog("App Opened By Notification", "The app was opened due to an interaction with a notification sent by this app.\n\nNotification Action: " + action.ToString(), false);
            };
            NATEvents.onDateTimeGetElapsedTimeSinceLastCloseUntilThisRun_PostInitialize += (NAT.DateTime.TimeElapsedWhileClosed timeElapsedWhileClosed) =>
            {
                NAT.DateTime.Calendar time = timeElapsedWhileClosed.timeElapsed_accordingSystemClock;

                demoNATMessages.ShowNotificationOnDemoScene("Welcome back! :)\nSee below how much time passed while the app was closed, provided by DateTime Event!\n" + time.YearString + "y, " + time.MonthString + "m, " + time.DayString + "d, " + time.HourString + "h, " + time.MinuteString + "m and " + time.SecondString + "s");
            };
            NATEvents.onDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume += (NAT.DateTime.TimeElapsedWhilePaused timeElapsedWhilePaused) =>
            {
                StartCoroutine(DateTime_WaitAndRunCodeOfElapsedTimeWhenPaused(timeElapsedWhilePaused));
            };
            NATEvents.onPlayGamesInitializationComplete += (bool userSignedIn, NAT.PlayGames.UserData userData) =>
            {
                if (userSignedIn == true)
                {
                    NAT.Notifications.ShowToast("Hello " + userData.displayName + "! You have been connected to Google Play Games!", true);
                    this.gameObject.GetComponentInChildren<DemoNATDataBase>().playGamesUserData = userData;


                    //UNCOMMENT     NAT.PlayGames.UnlockAchievement(PlayGamesResources.Achievement.internet_explorer);
                }
            };




            //Create the FilesClassDemo folder in the AppFiles scope
            Files_CreateTheNATFilesClassDemoFolderInAppFiles();



            //Initialize the Native Android Toolkit if is not initialized yet. This is necessary to NAT work.
            if (NativeAndroidToolkit.isInitialized == false)
                NativeAndroidToolkit.Initialize();
        }

        // --- NAT Demonstration Methods ---//

        //-- DIALOGS --//

        public void Dialog_SimpleAlertDialog()
        {
            NATEvents.onSimpleAlertDialogOk += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Ok\" button!");
            };
            NATEvents.onSimpleAlertDialogCancel += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You have canceled the simple alert dialog.");
            };

            NAT.Dialogs.ShowSimpleAlertDialog("Simple Alert Dialog", "This is a simple alert dialog.", true);



            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogues_opened, 1);
            //UNCOMMENT     NAT.PlayGames.SubmitScoreToLeaderboard(PlayGamesResources.Leaderboard.those_who_dialogs_the_most, Random.Range(1, 100));
        }

        public void Dialog_ConfirmationAlertDialog()
        {
            NATEvents.onConfirmationDialogYes += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Yes\" button!");
            };
            NATEvents.onConfirmationDialogNo += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"No\" button!");
            };
            NATEvents.onConfirmationDialogCancel += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You have canceled the confirmation alert dialog.");
            };

            NAT.Dialogs.ShowConfirmationDialog("Confirmation Alert Dialog", "This is a confirmation alert dialog. Ok?", true, "Yes", "No");



            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogues_opened, 1);
        }

        public void Dialog_NeutralAlertDialog()
        {
            NATEvents.onNeutralDialogYes += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Yes\" button!");
            };
            NATEvents.onNeutralDialogNo += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"No\" button!");
            };
            NATEvents.onNeutralDialogNeutral += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Neutral\" button!");

                //UNCOMMENT     NAT.PlayGames.UnlockAchievement(PlayGamesResources.Achievement.speech_100);
            };
            NATEvents.onNeutralDialogCancel += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You have canceled the neutral alert dialog.");
            };

            NAT.Dialogs.ShowNeutralDialog("Neutral Alert Dialog", "This is a neutral alert dialog. Ok?", true, "Yes", "No", "Neutral");



            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogues_opened, 1);
        }

        public void Dialog_RadialListDialog()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Done\" button. You have choosed option \"" + choosed.ToString() + "\".");
            };
            NATEvents.onRadialListDialogCancel += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You have canceled the radial list dialog.");
            };

            NAT.Dialogs.ShowRadialListDialog("Radial List Dialog", new string[] { "Ferrari", "Audi", "BMW", "Honda", "Ford" }, true, "Done", 1);



            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogues_opened, 1);
        }

        public void Dialog_CheckListDialog()
        {
            NATEvents.onCheckboxListDialogDone += (bool[] chooseds) =>
            {
                string str = chooseds[0].ToString();
                for (int i = 0; i < chooseds.Length; i++)
                {
                    if (i == 0)
                        continue;
                    str += "," + chooseds[i].ToString();
                }

                demoNATMessages.ShowNotificationOnDemoScene("You hit the \"Done\" button. You have choosed options\n\n\"" + str + "\"");
            };
            NATEvents.onCheckboxListDialogCancel += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("You have canceled the check list dialog.");
            };

            NAT.Dialogs.ShowCheckListDialog("Check List Dialog", new string[] { "Ferrari", "Audi", "BMW", "Honda", "Ford" }, true, "Done", new bool[] { false, false, true, false, false });



            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogues_opened, 1);
        }

        //-- NOTIFICATIONS --//

        public void Notification_ShowToast()
        {
            NAT.Notifications.ShowToast("This is a Toast notification!", true);
        }

        public void Notification_SendNotification()
        {
            NAT.Notifications.SendNotification("Demonstration notification", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            demoNATMessages.ShowNotificationOnDemoScene("The notification has been delivered.");
        }

        public void Notification_ScheduleNewNotification()
        {
            //Get the current channel in edition
            int currentChannelSelected = this.GetComponentInChildren<DemoNATScheduling>().currentSchedulingChannel;

            new NAT.Notifications.ScheduledNotification(NAT.Notifications.ChannelUtils.IntToCh(currentChannelSelected),
                                                        "NAT Scheduled Notification (CH " + currentChannelSelected + ")",
                                                        "This is a demonstration of Scheduled Notification. This Notification was scheduled in Channel " + currentChannelSelected + ".")
                                                        .setMinutesInFuture(3)
                                                        .setOnClickAction(NotificationsActions.Action.ClickAction1)
                                                        .addButton1("Action 1", NotificationsActions.Action.ButtonAction1)
                                                        .addButton2("Action 2", NotificationsActions.Action.ButtonAction2)
                                                        .ScheduleThisNotification();

            demoNATMessages.ShowNotificationOnDemoScene("A new notification has been scheduled on the Channel " + currentChannelSelected + " for 3 minutes in the future.");
        }

        public void Notification_CreateRepetitiveNotification()
        {
            //Get the current channel in edition
            int currentChannelSelected = this.GetComponentInChildren<DemoNATScheduling>().currentSchedulingChannel;

            new NAT.Notifications.ScheduledNotification(NAT.Notifications.ChannelUtils.IntToCh(currentChannelSelected),
                                                        "NAT Repetitive Notification (CH " + currentChannelSelected + ")",
                                                        "This is a demonstration of Repetitive Notification. This Notification was created in Channel " + currentChannelSelected + ".")
                                                        .setMinutesInFuture(3)
                                                        .setAsRepetitiveNotification(NAT.Notifications.IntervalType.Minutes, 3)
                                                        .setOnClickAction(NotificationsActions.Action.ClickAction1)
                                                        .addButton1("Action 1", NotificationsActions.Action.ButtonAction1)
                                                        .addButton2("Action 2", NotificationsActions.Action.ButtonAction2)
                                                        .ScheduleThisNotification();

            demoNATMessages.ShowNotificationOnDemoScene("A new repetitive notification has been created on the Channel " + currentChannelSelected + " with interval of 3 minutes.");
        }

        public void Notification_isNotificationScheduled()
        {
            //Get the current channel in edition
            int currentChannelSelected = this.GetComponentInChildren<DemoNATScheduling>().currentSchedulingChannel;

            NAT.Notifications.ScheduledInfo si = NAT.Notifications.isNotificationScheduledInChannel(NAT.Notifications.ChannelUtils.IntToCh(currentChannelSelected));

            demoNATMessages.ShowNotificationOnDemoScene("Channel " + currentChannelSelected + " Information\n\nHave Notification Scheduled: " + si.isScheduledInThisChannel.ToString() + "\nHave Repetitive Notification:  " + si.isRepetitiveNotification.ToString());
        }

        public void Notification_CancelScheduledNotification()
        {
            //Get the current channel in edition
            int currentChannelSelected = this.GetComponentInChildren<DemoNATScheduling>().currentSchedulingChannel;

            NAT.Notifications.ScheduledInfo si = NAT.Notifications.isNotificationScheduledInChannel(NAT.Notifications.ChannelUtils.IntToCh(currentChannelSelected));
            if (si.isScheduledInThisChannel == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("There are no scheduled notifications on Channel " + currentChannelSelected + ".");
                return;
            }
            NAT.Notifications.CancelScheduledNotification(NAT.Notifications.ChannelUtils.IntToCh(currentChannelSelected));

            demoNATMessages.ShowNotificationOnDemoScene("Channel " + currentChannelSelected + " Scheduled Notification was cancelled");
        }

        public void Notification_GetListOfFreeNotifyChannels()
        {
            int[] freeChannels = NAT.Notifications.GetListOfFreeNotificationsChannels();

            string freeList = "\n\n";
            int linebreak = 0;
            for (int i = 0; i < freeChannels.Length; i++)
            {
                linebreak += 1;

                if (i == 0)
                    freeList += freeChannels[i];
                if (i > 0)
                {
                    if (linebreak > 1)
                        freeList += ",";
                    freeList += freeChannels[i];
                }

                if (linebreak >= 12)
                {
                    freeList += "\n";
                    linebreak = 0;
                }
            }

            demoNATMessages.ShowNotificationOnDemoScene("There are " + freeChannels.Length + " Notification Channels without scheduled notifications." + freeList);
        }

        //-- SHARING --//

        public void Sharing_ShareTexture2D()
        {
            NAT.Sharing.ShareTexture2D("Sharing Texture2D", this.gameObject.GetComponentInChildren<DemoNATDataBase>().exampleTexture2dForShare, "This is the example message of share!");
        }

        public void Sharing_ShareTextPlain()
        {
            NAT.Sharing.ShareTextPlain("Sharing Text Plain", "This a simple text to be shared with other application!");
        }

        public void Sharing_CopyToClipboard()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Something was copied to your Clipboard!");
            NAT.Sharing.CopyTextToClipboard("Some text from Native Android Toolkit.");
        }

        public void Sharing_GetFromClipboard()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Current text from your Clipboard is...\n\"" + NAT.Sharing.GetTextFromClipboard() + "\"");
        }

        public void Sharing_TakeScreenshotAndGetTexture2D()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Screenshot captured, generating Texture2D...");

            //Register callback to show texture 2D of screenshot, after processing complete
            NATEvents.onCompleteScreenshotTexture2dProcessing += (Texture2D texture) =>
            {
                this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is the Texture2D of Screenshot captured!", texture, 820, 450);
            };

            NAT.Sharing.TakeScreenshotAndGetTexture2D();
        }

        //-- WEBVIEW --//

        public void Webview_OpenWebview(string webviewParameters)
        {
            //Demo for Open Webview

            //Get desired webview mode
            NAT.Webview.WebviewMode webviewMode = NAT.Webview.WebviewMode.AdaptativePopUp;

            if (webviewParameters == "popup") //<- If is desired a popup webview
                webviewMode = NAT.Webview.WebviewMode.AdaptativePopUp;
            if (webviewParameters == "fullscreen") //If is desired a fullscreen webview
                webviewMode = NAT.Webview.WebviewMode.LandscapeFullscreen;




            //Finally, open the webview
            NATEvents.onPopUpWebviewClose += (NAT.Webview.WebviewBrowsing webviewBrowsing) =>
            {
                string navigatedSites = "";
                foreach (string str in webviewBrowsing.browsedSites)
                    navigatedSites += "\n" + str;

                demoNATMessages.ShowNotificationOnDemoScene("You have just closed a PopUp Webview! Browsed sites is...\n" + navigatedSites);
            };
            new NAT.Webview.WebviewChromium("https://google.com/", webviewMode)
                                            .setTitle("NAT Webview")
                                            .OpenThisWebview();




            //UNCOMMENT     if (webviewParameters == "popup")
            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.dialogs_webviews_opened, 1);
            //UNCOMMENT     if (webviewParameters == "fullscreen")
            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.fullscreen_webviews_opened, 1);
        }

        public void Webview_GetAllCookiesFromURL()
        {
            NATEvents.onWebviewGettedAllCookiesFromURL += (bool succesfully, NAT.Webview.WebviewCookie[] cookies, string pageContent) =>
            {
                if (succesfully == true)
                {
                    string allCookies = "";
                    foreach (NAT.Webview.WebviewCookie cookie in cookies)
                        allCookies += "\n" + cookie.name;

                    demoNATMessages.ShowNotificationOnDemoScene("These are the names of all Cookies obtained on Google.com page....\n" + allCookies);
                }
                if (succesfully == false)
                    demoNATMessages.ShowNotificationOnDemoScene("There was a network error while trying to read all Cookies on the Google.com page.");
            };
            NAT.Webview.AccessSomeURLWithPostDataAndGetAllCookies(NAT.Webview.WebviewAccessMethod.Get, "https://google.com/", "https://google.com", new NAT.Webview.WebviewPostData().CloseAndGetPostDataArray());
        }

        public void Webview_ClearAllCookies()
        {
            demoNATMessages.ShowNotificationOnDemoScene("All Webview Cookies have been cleared.");
            NAT.Webview.ClearAllCookies(NAT.Webview.WebviewClearCacheMode.OnlyWebview);
        }

        //-- PERMISSIONS --//

        public void Permissions_OpenPermissionRequesterWizard()
        {
            new NAT.Permissions.PermissionRequesterWizard("NAT Permission Requester Wizard", NAT.Permissions.RequesterWizardMode.LandscapeFullscreen)
                               .addPermissionToRequest("Camera", NAT.Permissions.AndroidPermission.Camera, "Required for NAT Camera functions to work in this demo.")
                               .addPermissionToRequest("Access Coarse Location", NAT.Permissions.AndroidPermission.AccessCoarseLocation, "Required for NAT Location functions to work in this demo.")
                               .addPermissionToRequest("Access Fine Location", NAT.Permissions.AndroidPermission.AccessFineLocation, "Required for NAT Location functions to work in this demo.")
                               .addPermissionToRequest("Record Audio", NAT.Permissions.AndroidPermission.RecordAudio, "Required for NAT Recording functions to work in this demo.")
                               .addPermissionToRequest("Access Files And Media", NAT.Permissions.AndroidPermission.AccessFilesAndMedia, "Required for NAT Files functions to work in this demo.")
                               .OpenThisPermissionRequesterWizard();
        }

        public void Permissions_RequestCameraPermission()
        {
            new NAT.Permissions.PermissionRequester()
                               .addPermissionToRequest(NAT.Permissions.AndroidPermission.Camera)
                               .RequestThisPermissions();
        }

        public void Permissions_RequestLocationPermission()
        {
            new NAT.Permissions.PermissionRequester()
                               .addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessCoarseLocation)
                               .addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessFineLocation)
                               .RequestThisPermissions();
        }

        public void Permissions_RequestMicrophonePermission()
        {
            new NAT.Permissions.PermissionRequester()
                               .addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                               .RequestThisPermissions();
        }

        public void Permissions_RequestFilesPermission()
        {
            new NAT.Permissions.PermissionRequester()
                               .addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessFilesAndMedia)
                               .RequestThisPermissions();
        }

        public void Permissions_isCameraPermissionGuaranteed()
        {
            bool permissionGuaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.Camera);
            demoNATMessages.ShowNotificationOnDemoScene("Is Camera permission guaranteed?\n" + permissionGuaranteed);
        }

        public void Permissions_isLocationPermissionGuaranteed()
        {
            bool permission0Guaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.AccessCoarseLocation);
            bool permission1Guaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.AccessFineLocation);
            demoNATMessages.ShowNotificationOnDemoScene("Is Location permission guaranteed?\n" + ((permission0Guaranteed == true && permission1Guaranteed == true) ? true : false));
        }

        public void Permissions_isMicrophonePermissionGuaranteed()
        {
            bool permissionGuaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.RecordAudio);
            demoNATMessages.ShowNotificationOnDemoScene("Is Microphone permission guaranteed?\n" + permissionGuaranteed);
        }

        public void Permissions_isFilesPermissionGuaranteed()
        {
            bool permissionGuaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.AccessFilesAndMedia);
            demoNATMessages.ShowNotificationOnDemoScene("Is Files permission guaranteed?\n" + permissionGuaranteed);
        }

        //-- UTILS --//

        public void Utils_RestartApplication()
        {
            NAT.Utils.RestartApplication("The application is restarting. Just a moment...", NAT.Utils.RestartInterfaceMode.LandscapeFullscreen);
        }

        public void Utils_Vibrate()
        {
            NAT.Utils.Vibrate(300);
            demoNATMessages.ShowNotificationOnDemoScene("Vibrating device");
        }

        public void Utils_VibrateWithPattern()
        {
            NAT.Utils.VibrateWithPattern(new long[] { 0, 300, 100, 300, 100, 300 });
            demoNATMessages.ShowNotificationOnDemoScene("Vibrating device with pattern");
        }

        public void Utils_GetDeviceManufacturer()
        {
            string deviceInfo = NAT.Utils.GetDeviceManufacturer();
            demoNATMessages.ShowNotificationOnDemoScene("Device manufacturer\n" + deviceInfo);
        }

        public void Utils_GetDeviceModel()
        {
            string deviceInfo = NAT.Utils.GetDeviceModel();
            demoNATMessages.ShowNotificationOnDemoScene("Device model\n" + deviceInfo);
        }

        public void Utils_GetDeviceAndroidVersionCode()
        {
            int deviceInfo = NAT.Utils.GetDeviceAndroidVersionCode();
            demoNATMessages.ShowNotificationOnDemoScene("Device Android Version Code\n" + deviceInfo);
        }

        public void Utils_SpeakWithTTS()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //Speak with TTS
                NAT.Utils.TTSEngineLanguage language = NAT.Utils.TTSEngineLanguage.DeviceDefault;

                if (choosed == 0)
                    language = NAT.Utils.TTSEngineLanguage.DeviceDefault;
                if (choosed == 1)
                    language = NAT.Utils.TTSEngineLanguage.English;
                if (choosed == 2)
                    language = NAT.Utils.TTSEngineLanguage.Spanish;
                if (choosed == 3)
                    language = NAT.Utils.TTSEngineLanguage.Portuguese;
                if (choosed == 4)
                    language = NAT.Utils.TTSEngineLanguage.French;

                NAT.Utils.SpeakWithTTS(language, "Hello! I'm Native Android Toolkit!", NAT.Utils.TTSEngineQueueMode.FlushQueueAndAdd);
                demoNATMessages.ShowNotificationOnDemoScene("Speaking with TTS:\n\nHello! I'm Native Android Toolkit!");
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a language to Speak", new string[] { "DeviceDefault", "English", "Spanish", "Portuguese", "French" }, false, "Speak Now", 0);
        }

        public void Utils_GetDeviceLocale()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                NAT.Utils.LocaleType localeType = NAT.Utils.LocaleType.Country;

                switch (choosed)
                {
                    case 0:
                        localeType = NAT.Utils.LocaleType.ISO3Country;
                        break;
                    case 1:
                        localeType = NAT.Utils.LocaleType.Country;
                        break;
                    case 2:
                        localeType = NAT.Utils.LocaleType.LanguageName;
                        break;
                    case 3:
                        localeType = NAT.Utils.LocaleType.LanguageTag;
                        break;
                    case 4:
                        localeType = NAT.Utils.LocaleType.ISO3Language;
                        break;
                    case 5:
                        localeType = NAT.Utils.LocaleType.CurrencyCode;
                        break;
                }

                string locale = NAT.Utils.GetDeviceCurrentLocale(localeType);
                demoNATMessages.ShowNotificationOnDemoScene("The current \"" + localeType + "\" locale is\n" + locale);
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a Locale", new string[] { "ISO3Country", "Country", "LanguageName", "LanguageTag", "ISO3Language", "CurrencyCode" }, false, "Choose", 0);
        }

        public void Utils_EnableAntiScreenshot()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                bool enable = false;

                switch (choosed)
                {
                    case 0:
                        enable = true;
                        break;
                    case 1:
                        enable = false;
                        break;
                }

                NAT.Utils.EnableAntiScreenshot(enable);
                if (enable == true)
                    demoNATMessages.ShowNotificationOnDemoScene("The Anti Screenshot was enabled!");
                if (enable == false)
                    demoNATMessages.ShowNotificationOnDemoScene("The Anti Screenshot was disabled!");
            };

            NAT.Dialogs.ShowRadialListDialog("Enable or Disable Anti Screenshot?", new string[] { "Enable", "Disable" }, false, "Choose", 0);
        }

        public void Utils_ConvertDPToPixels()
        {
            int pixels = NAT.Utils.ConvertDPToPixels(50.0f);
            demoNATMessages.ShowNotificationOnDemoScene("50dp is equivalent to " + pixels + "pixels");
        }

        public void Utils_ConvertPixelsToDP()
        {
            float dp = NAT.Utils.ConvertPixelsToDP(250);
            demoNATMessages.ShowNotificationOnDemoScene("250pixels is equivalent to " + dp + "dp");
        }

        public void Utils_ConvertPixelsSizeToCanvasSize()
        {
            int pixelsWidth = NAT.Utils.ConvertDPToPixels(320);
            int pixelsHeight = NAT.Utils.ConvertDPToPixels(50);
            NAT.Utils.CanvasSize size = NAT.Utils.ConvertPixelsSizeToCanvasSize(GameObject.Find("Canvas").GetComponent<Canvas>(), pixelsWidth, pixelsHeight);
            demoNATMessages.ShowNotificationOnDemoScene("Converting Pixels Size to Canvas Size\nDP Size: 320dp x 50dp\nPixels Size: " + pixelsWidth + " x " + pixelsHeight + "\nCanvas Size: " + size.unitsWidth.ToString("F1") + "units x " + size.unitsHeight.ToString("F1") + "units");
        }

        public void Utils_VibratePlus()
        {
            NAT.Utils.VibratePlus(300);
            demoNATMessages.ShowNotificationOnDemoScene("Vibrating Device (Plus Optimized)");
        }

        public void Utils_GetDeviceNotchPixelsSize()
        {
            NAT.Utils.DeviceNotchSize notchSize = NAT.Utils.GetDeviceNotchPixelsSize();
            demoNATMessages.ShowNotificationOnDemoScene("Device Notch Size\nRight: " + notchSize.rightPixelsSize + "px\nLeft: " + notchSize.leftPixelsSize + "px\nTop: " + notchSize.topPixelsSize + "px\nBottom: " + notchSize.bottomPixelsSize + "px");
        }

        public void Utils_OpenPlayStoreInAppReview()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Requesting Play Store Review Pop-Up...");
            NAT.Utils.OpenPlayStoreInAppReview();
        }

        public void Utils_isVibrationAvailable()
        {
            bool vibrationAvailable = NAT.Utils.isVibrationAvailable();
            demoNATMessages.ShowNotificationOnDemoScene("Is Vibration Available?\n" + vibrationAvailable);
        }

        public void Utils_isWifiEnabled()
        {
            bool info = NAT.Utils.isWifiEnabled();
            demoNATMessages.ShowNotificationOnDemoScene("Is Wi-Fi Enabled?\n" + info);
        }

        public void Utils_isConnectedToWifi()
        {
            bool info = NAT.Utils.isConnectedToWifi();
            demoNATMessages.ShowNotificationOnDemoScene("Is connected to a Wi-Fi?\n" + info);
        }

        public void Utils_isUsingHeadset()
        {
            bool info = NAT.Utils.isUsingHeadset();
            demoNATMessages.ShowNotificationOnDemoScene("Is using a Headset?\n" + info);
        }

        public void Utils_isInternetAvailable()
        {
            bool info = NAT.Utils.isInternetAvailable();
            demoNATMessages.ShowNotificationOnDemoScene("Is Internet available?\n" + info);
        }

        public void Utils_isDeveloperModeEnabled()
        {
            bool info = NAT.Utils.isDeveloperModeEnabled();
            demoNATMessages.ShowNotificationOnDemoScene("Is Developer Mode enabled?\n" + info);
        }

        public void Utils_isGooglePlayServicesAvailable()
        {
            bool info = NAT.Utils.isGooglePlayServicesAvailable();
            demoNATMessages.ShowNotificationOnDemoScene("Is Google Play Services Available?\n" + info);
        }

        public void Utils_isDeviceRooted()
        {
            bool info = NAT.Utils.isDeviceRooted();
            demoNATMessages.ShowNotificationOnDemoScene("Is Device Rooted?\n" + info);
        }

        public void Utils_isAntiScreenshotEnabled()
        {
            bool info = NAT.Utils.isAntiScreenshotEnabled();
            demoNATMessages.ShowNotificationOnDemoScene("Is Anti Screenshot enabled?\n" + info);
        }

        //-- SETTINGS --//

        public void Settings_OpenGeneralSettings()
        {
            NAT.Settings.OpenGeneralSettings();
        }

        public void Settings_OpenThisAppSettings()
        {
            NAT.Settings.OpenThisAppSettings();
        }

        public void Settings_OpenWifiSettings()
        {
            NAT.Settings.OpenWifiSettings();
        }

        public void Settings_OpenBluetoothSettings()
        {
            NAT.Settings.OpenBluetoothSettings();
        }

        public void Settings_OpenLocationSettings()
        {
            NAT.Settings.OpenLocationSettings();
        }

        public void Settings_OpenNetworkOperatorSettings()
        {
            NAT.Settings.OpenNetworkOperatorSettings();
        }

        public void Settings_OpenInternetToggleSettings()
        {
            NAT.Settings.OpenInternetToggleSettings();
        }

        //-- LOCATION --//

        public void Location_isLocationEnabled(int locationProvider)
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessCoarseLocation)
                                                     .addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessFineLocation)
                                                     .RequestThisPermissions();

            //GPS
            if (locationProvider == 0)
            {
                bool info = NAT.Location.isLocationEnabled(NAT.Location.LocationProvider.GPS);
                demoNATMessages.ShowNotificationOnDemoScene("Is GPS Location enabled?\n" + info);
            }
            //Network
            if (locationProvider == 1)
            {
                bool info = NAT.Location.isLocationEnabled(NAT.Location.LocationProvider.Network);
                demoNATMessages.ShowNotificationOnDemoScene("Is Network Location enabled?\n" + info);
            }
        }

        public void Location_isMockEnabled()
        {
            bool info = NAT.Location.isMockEnabled();
            demoNATMessages.ShowNotificationOnDemoScene("Is Mock Location enabled?\n" + info);
        }

        public void Location_isTrackingLocation()
        {
            NAT.Location.LocationRunning running = NAT.Location.isTrackingLocation();

            if (running == NAT.Location.LocationRunning.None)
                demoNATMessages.ShowNotificationOnDemoScene("Location is not being tracked.");
            if (running == NAT.Location.LocationRunning.GPS)
                demoNATMessages.ShowNotificationOnDemoScene("GPS Location is being tracked!");
            if (running == NAT.Location.LocationRunning.Network)
                demoNATMessages.ShowNotificationOnDemoScene("Network Location is being tracked!");
        }

        public void Location_StartTrackingLocation(int locationProvider)
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessCoarseLocation)
                                                     .addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessFineLocation)
                                                     .RequestThisPermissions();

            //Register callback events if this is the first call of start tracking location
            if (NAT.Location.isTrackingLocation() == NAT.Location.LocationRunning.None)
            {
                NATEvents.onLocationChanged += (NAT.Location.LocationProvider provider, NAT.Location.LocationData data) =>
                {
                    demoNATMessages.ShowNotificationOnDemoScene("New current Location\n" + data.address0Name + "\n" + data.longitude.ToString() + "," + data.latitude + "\nIs Cache: " + data.isFirstAndCacheLocation);
                };
                NATEvents.onLocationProviderChanged += (NAT.Location.LocationProvider provider, bool isEnabledNow) =>
                {
                    demoNATMessages.ShowNotificationOnDemoScene(provider.ToString() + " Provider changed\nIs Enabled: " + isEnabledNow);
                };
            }

            bool started = false;
            if (locationProvider == 0)
                started = NAT.Location.StartTrackingLocation(NAT.Location.LocationProvider.GPS, false, NAT.Location.LocationUpdateTime.Each5Seconds, NAT.Location.LocationUpdateDistance.Each5Meters);
            if (locationProvider == 1)
                started = NAT.Location.StartTrackingLocation(NAT.Location.LocationProvider.Network, false, NAT.Location.LocationUpdateTime.Each5Seconds, NAT.Location.LocationUpdateDistance.Each5Meters);

            if (started == true && locationProvider == 0)
                demoNATMessages.ShowNotificationOnDemoScene("Began tracking the GPS location!");
            if (started == true && locationProvider == 1)
                demoNATMessages.ShowNotificationOnDemoScene("Began tracking the Network location!");
            if (started == false)
                demoNATMessages.ShowNotificationOnDemoScene("Unable to start tracking of Location.\nCheck whether the application has permission or whether another tracking has been started before.");
        }

        public void Location_StopTrackingLocation()
        {
            bool stopped = NAT.Location.StopTrackingLocation();

            if (stopped == true)
                demoNATMessages.ShowNotificationOnDemoScene("Location has stopped being tracked.");
            if (stopped == false)
                demoNATMessages.ShowNotificationOnDemoScene("Unable to stop tracking location. It appears that there is no longer any tracking running, Location is disabled, or there are no permissions.");
        }

        public void Location_isGoogleMapsOpen()
        {
            bool info = NAT.Location.isGoogleMapsOpen();
            demoNATMessages.ShowNotificationOnDemoScene("Is Google Maps open?\n" + info);
        }

        public void Location_OpenGoogleMaps()
        {
            NATEvents.onGoogleMapsLoaded += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("Google Maps has been loaded.");

                //Add the marker
                NAT.Location.AddMarkerOnGoogleMap("COVID 19 Vaccine Location Googleplex (by NAT)", NAT.Location.GoogleMapsMarker.Marker10, 37.422329719012104, -122.0845403129736, false);
                NAT.Location.AddMarkerOnGoogleMap("Googleplex (by NAT)", NAT.Location.GoogleMapsMarker.MarkerDefault, 37.42212710483604f, -122.0841061387679f, true);
                //Move the camera to Googleplex
                NAT.Location.MoveCameraOfGoogleMap(37.42212710483604f, -122.0841061387679f, NAT.Location.GoogleMapsZoom.Streets);
            };
            NATEvents.onGoogleMapsClick += (double longitude, double latitude) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("Clicked on a map position\n" + longitude + "," + latitude);

                //Remove all markers
                NAT.Location.RemoveAllMarkersOfGoogleMap();
                //Add marker on click
                NAT.Location.AddMarkerOnGoogleMap("Your Marker", NAT.Location.GoogleMapsMarker.Marker1, latitude, longitude, true);
            };
            NATEvents.onGoogleMapsClosed += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("Google Maps has been closed.");
            };

            NAT.Location.OpenGoogleMaps("NAT Map");
        }

        //-- CAMERA --//

        public void Camera_isCameraSupported()
        {
            bool info = NAT.Camera.isCameraSupported();
            demoNATMessages.ShowNotificationOnDemoScene("Is Camera supported?\n" + info);
        }

        public void Camera_OpenPhotoCamera()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.Camera)
                                                     .RequestThisPermissions();

            //Open camera code
            NATEvents.onCameraTakePhoto += (Texture2D photo) =>
            {
                this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is the Photo taked!", photo, 820, 450);

                //UNCOMMENT      NAT.PlayGames.IncrementAchievement(PlayGamesResources.Achievement.cameraman, 1);
            };
            NATEvents.onCameraClose += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The Camera has been closed by the user!");
            };

            new NAT.Camera.CameraNative(NAT.Camera.CameraMode.PhotoCamera, ScreenOrientation.AutoRotation)
                                        .setTitle("NAT Photo Camera")
                                        .OpenThisCamera();
        }

        public void Camera_OpenCodeReader()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.Camera)
                                                     .RequestThisPermissions();

            //Open camera code
            NATEvents.onCameraReadCode += (string result) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The result of Scanned Code is\n\n" + result);
            };
            NATEvents.onCameraClose += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The Camera has been closed by the user!");
            };

            new NAT.Camera.CameraNative(NAT.Camera.CameraMode.CodeReader, ScreenOrientation.AutoRotation)
                                        .setTitle("NAT Code Reader")
                                        .setCodeReaderMessage("Point to a Bar/QR Code and center it on the Screen to read it!")
                                        .OpenThisCamera();
        }

        public void Camera_OpenVideoCamera()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.Camera)
                                                     .addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            //Open camera code
            NATEvents.onCameraRecorded += (string videoPath) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The path for recorded video is\n" + videoPath);
            };
            NATEvents.onCameraClose += () =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The Camera has been closed by the user!");
            };

            new NAT.Camera.CameraNative(NAT.Camera.CameraMode.VideoCamera, ScreenOrientation.AutoRotation)
                                        .setTitle("NAT Video Camera")
                                        .OpenThisCamera();
        }

        public void Camera_GetQRCodeFromString()
        {
            Texture2D qrCodeTexture = NAT.Camera.GetGeneratedQRCode("Native Android Toolkit is Cool!");

            demoNATMessages.ShowNotificationOnDemoScene("Generating a QR Code for the string\n\"Native Android Toolkit is Cool!\"");
            this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is the Generated QR Code!", qrCodeTexture, 400, 400);
        }

        //-- MICROPHONE --//

        public void Microphone_isMicrophoneSupported()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            bool info = NAT.Microphone.isMicrophoneSupported();
            demoNATMessages.ShowNotificationOnDemoScene("Is Microphone supported?\n" + info);
        }

        public void Microphone_isRecordingMicrophone()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            bool info = NAT.Microphone.isRecordingMicrophone();
            demoNATMessages.ShowNotificationOnDemoScene("Is recording Microphone?\n" + info);
        }

        public void Microphone_StartRecordingMicrophone()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            bool info = NAT.Microphone.isRecordingMicrophone();

            if (info == false)
            {
                NATEvents.onMicrophoneStopRecording += (string audioPath) =>
                {
                    demoNATMessages.ShowNotificationOnDemoScene("Stopped Microphone recording. The path for recorded audio is\n" + audioPath);
                };

                demoNATMessages.ShowNotificationOnDemoScene("Starting to record Microphone...");
                NAT.Microphone.StartRecordingMicrophone();
            }
            if (info == true)
                demoNATMessages.ShowNotificationOnDemoScene("The Microphone is already being recorded!");
        }

        public void Microphone_StopRecordingMicrophone()
        {
            bool info = NAT.Microphone.isRecordingMicrophone();

            if (info == true)
                NAT.Microphone.StopRecordingMicrophone();
            if (info == false)
                demoNATMessages.ShowNotificationOnDemoScene("The Microphone is not being recorded!");
        }

        public void Microphone_isSpeechToTextSupported()
        {
            bool info = NAT.Microphone.isSpeechToTextSupported();
            demoNATMessages.ShowNotificationOnDemoScene("Is Speech To Text supported on this device?\n" + info);
        }

        public void Microphone_isListeningSpeechToText()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            bool info = NAT.Microphone.isListeningSpeechToText();
            demoNATMessages.ShowNotificationOnDemoScene("Is listening Speech To Text?\n" + info);
        }

        public void Microphone_StartListeningSpeechToText()
        {
            //Permissions code
            new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
                                                     .RequestThisPermissions();

            bool info = NAT.Microphone.isListeningSpeechToText();

            if (info == false)
            {
                NATEvents.onMicrophoneSpeechToTextStarted += () =>
                {
                    NAT.Notifications.ShowToast("Please say something close to the microphone!", true);
                };
                NATEvents.onMicrophoneSpeechToTextFinished += (NAT.Microphone.SpeechToTextResult result, string textResult) =>
                {
                    //If error
                    if (result != NAT.Microphone.SpeechToTextResult.NoError)
                        demoNATMessages.ShowNotificationOnDemoScene("Error on listen: \n\"" + result + "\"");
                    //If success
                    if (result == NAT.Microphone.SpeechToTextResult.NoError)
                        demoNATMessages.ShowNotificationOnDemoScene("Listened is: \n\"" + textResult + "\"");
                };

                demoNATMessages.ShowNotificationOnDemoScene("Starting to listen Speech To Text...");
                NAT.Microphone.StartListeningSpeechToText();
            }
            if (info == true)
                demoNATMessages.ShowNotificationOnDemoScene("The Microphone is already listening Speech To Text!");
        }

        //-- APPLICATIONS --//

        public void Applications_isApplicationInstalled()
        {
            bool info = NAT.Applications.isApplicationInstalled("com.google.android.gm");
            demoNATMessages.ShowNotificationOnDemoScene("Checking if Gmail is installed, to example...\n\nIs installed: " + info);
        }

        public void Applications_OpenApplication()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Trying to open Gmail application, for example...");
            NAT.Applications.OpenApplication("com.google.android.gm");
        }

        public void Applications_OpenApplicationWithExtra()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Trying to open Gmail application putting Extras, for example...");
            new NAT.Applications.OpenApplicationWithExtras("com.google.android.gm")
                                                           .putExtra("testKey0", "this is the string value 0 to be passed and will be acessible to the app2!")
                                                           .putExtra("testKey1", "this is the string value 1 to be passed and will be acessible to the app2!")
                                                           .putExtra("testKey2", "this is the string value 2 to be passed and will be acessible to the app2!")
                                                           .OpenTheApplicationNow();
        }

        public void Applications_GetThisApplicationPackageName()
        {
            string info = NAT.Applications.GetThisApplicationPackageName();
            demoNATMessages.ShowNotificationOnDemoScene("The Package Name of this app is \"" + info + "\"");
        }

        public void Applications_isPlayStoreAvailable()
        {
            bool info = NAT.Applications.isPlayStoreAvailable();
            demoNATMessages.ShowNotificationOnDemoScene("Is Play Store available in this device?\n\n" + info);
        }

        public void Applications_OpenApplicationInPlayStore()
        {
            demoNATMessages.ShowNotificationOnDemoScene("Trying to open Unity Remote application in Play Store, for example...");
            NAT.Applications.OpenApplicationInPlayStore("com.unity3d.mobileremote");
        }

        //-- DATETIME --//

        public void DateTime_OpenHourPicker()
        {
            NATEvents.onDateTimeHourPicked += (NAT.DateTime.Calendar hourPicked) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The Hour picked is\n" + hourPicked.HourString + ":" + hourPicked.MinuteString + "\n\n(HH:MM)");
            };

            NAT.DateTime.OpenHourPicker("Select a Hour");
        }

        public void DateTime_OpenDatePicker()
        {
            NATEvents.onDateTimeDatePicked += (NAT.DateTime.Calendar datePicked) =>
            {
                demoNATMessages.ShowNotificationOnDemoScene("The Date picked is\n" + datePicked.MonthString + "/" + datePicked.DayString + "/" + datePicked.YearString + "\n\n(MM:DD:YYYY)");
            };

            NAT.DateTime.OpenDatePicker("Select a Date", new NAT.DateTime.Calendar(2022, 1, 1, 0, 0, 0), new NAT.DateTime.Calendar(2022, 12, 31, 0, 0, 0));
        }

        public void DateTime_LoadCurrentNTPTime()
        {
            NATEvents.onDateTimeDoneNTPRequest += (bool succesfully, NAT.DateTime.Calendar timeReceived, string responseServer) =>
            {
                if (succesfully == false)
                    demoNATMessages.ShowNotificationOnDemoScene("There was an error getting the time from the NTP server");
                if (succesfully == true)
                    demoNATMessages.ShowNotificationOnDemoScene("This was the time taken from the NTP server\n" + timeReceived.MonthString + "/" + timeReceived.DayString + "/" + timeReceived.YearString + " " + timeReceived.HourString + ":" + timeReceived.MinuteString + "\n(MM/DD/YYYY HH:MM)\nFrom: " + responseServer);
            };

            NAT.DateTime.LoadCurrentTimeOfNTP();
        }

        public void DateTime_GetElapsedRealtimeSinceBoot()
        {
            NAT.DateTime.Calendar elapsedTime = NAT.DateTime.GetElapsedRealtimeSinceBoot();
            demoNATMessages.ShowNotificationOnDemoScene("Elapsed time since boot is...\n" + elapsedTime.YearString + "y, " + elapsedTime.MonthString + "m, " + elapsedTime.DayString + "d, " + elapsedTime.HourString + "h, " + elapsedTime.MinuteString + "m and " + elapsedTime.SecondString + "s");
        }

        //-- FILES --//

        public void Files_GetAbsolutePathForScope()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                NAT.Files.Scope scope = NAT.Files.Scope.AppFiles;

                switch (choosed)
                {
                    case 0:
                        scope = NAT.Files.Scope.AppFiles;
                        break;
                    case 1:
                        scope = NAT.Files.Scope.AppCache;
                        break;
                    case 2:
                        scope = NAT.Files.Scope.DCIM;
                        break;
                    case 3:
                        scope = NAT.Files.Scope.Documents;
                        break;
                    case 4:
                        scope = NAT.Files.Scope.Downloads;
                        break;
                    case 5:
                        scope = NAT.Files.Scope.Movies;
                        break;
                    case 6:
                        scope = NAT.Files.Scope.Music;
                        break;
                    case 7:
                        scope = NAT.Files.Scope.Pictures;
                        break;
                    case 8:
                        scope = NAT.Files.Scope.Podcasts;
                        break;
                    case 9:
                        scope = NAT.Files.Scope.Recordings;
                        break;
                    case 10:
                        scope = NAT.Files.Scope.Ringtones;
                        break;
                    case 11:
                        scope = NAT.Files.Scope.Screenshots;
                        break;
                }

                demoNATMessages.ShowNotificationOnDemoScene("The path for Scope \"" + scope + "\" is\n\n" + NAT.Files.GetAbsolutePathForScope(scope).Replace("/storage/emulated/0", "<storage>"));
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a Scope", new string[] { "AppFiles", "AppCache", "DCIM", "Documents", "Downloads", "Movies", "Music", "Pictures", "Podcasts", "Recordings", "Ringtones", "Screenshots" }, false, "Choose", 0);
        }

        public void Files_GetScopeAvailability()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                NAT.Files.Scope scope = NAT.Files.Scope.AppFiles;

                switch (choosed)
                {
                    case 0:
                        scope = NAT.Files.Scope.AppFiles;
                        break;
                    case 1:
                        scope = NAT.Files.Scope.AppCache;
                        break;
                    case 2:
                        scope = NAT.Files.Scope.DCIM;
                        break;
                    case 3:
                        scope = NAT.Files.Scope.Documents;
                        break;
                    case 4:
                        scope = NAT.Files.Scope.Downloads;
                        break;
                    case 5:
                        scope = NAT.Files.Scope.Movies;
                        break;
                    case 6:
                        scope = NAT.Files.Scope.Music;
                        break;
                    case 7:
                        scope = NAT.Files.Scope.Pictures;
                        break;
                    case 8:
                        scope = NAT.Files.Scope.Podcasts;
                        break;
                    case 9:
                        scope = NAT.Files.Scope.Recordings;
                        break;
                    case 10:
                        scope = NAT.Files.Scope.Ringtones;
                        break;
                    case 11:
                        scope = NAT.Files.Scope.Screenshots;
                        break;
                }

                demoNATMessages.ShowNotificationOnDemoScene("The Scope \"" + scope + "\" availability is\n\n" + NAT.Files.GetScopeAvailability(scope));
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a Scope", new string[] { "AppFiles", "AppCache", "DCIM", "Documents", "Downloads", "Movies", "Music", "Pictures", "Podcasts", "Recordings", "Ringtones", "Screenshots" }, false, "Choose", 0);
        }

        public void Files_GetInternalMemoryUsageInfo()
        {
            NAT.Files.StorageInfo info = NAT.Files.GetInternalMemoryUsageInfo();

            demoNATMessages.ShowNotificationOnDemoScene("This is the usage of Internal Memory\n\nTotal: " + info.totalMemory.ToGigabytes.ToString("F1") + " GB\nUsed: " + info.usedMemory.ToGigabytes.ToString("F1") + " GB\nFree: " + info.freeMemory.ToGigabytes.ToString("F1") + " GB");
        }

        public void Files_SaveMediaAndMakesItAvailableToGallery()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //screenshot
                if (choosed == 0)
                {
                    string originalMediaPath = Application.persistentDataPath + "/NAT/screenshot.png";
                    if (System.IO.File.Exists(originalMediaPath) == true)
                    {
                        NAT.Files.SaveMediaAndMakesItAvailableToGallery(NAT.Files.MediaType.Screenshot, originalMediaPath);
                        demoNATMessages.ShowNotificationOnDemoScene("The media \"screenshot.png\" was copied from \"AppFiles/NAT\" into the \"Screenshots\" scope and was scanned by the Android system! Try viewing your gallery! :)");
                    }
                    else
                    {
                        NAT.Dialogs.ShowSimpleAlertDialog("Error", "Please create a media using function \"Take Screenshot And Get Texture2D\" from class \"Sharing\" and then the media can be saved to appear in the gallery!", false);
                    }
                }
                //photo
                if (choosed == 1)
                {
                    string originalMediaPath = Application.persistentDataPath + "/NAT/last-photo.jpg";
                    if (System.IO.File.Exists(originalMediaPath) == true)
                    {
                        NAT.Files.SaveMediaAndMakesItAvailableToGallery(NAT.Files.MediaType.Photo, originalMediaPath);
                        demoNATMessages.ShowNotificationOnDemoScene("The media \"last-photo.jpg\" was copied from \"AppFiles/NAT\" into the \"DCIM\" scope and was scanned by the Android system! Try viewing your gallery! :)");
                    }
                    else
                    {
                        NAT.Dialogs.ShowSimpleAlertDialog("Error", "Please create a media using function \"Open Photo Camera\" from class \"Camera\" and then the media can be saved to appear in the gallery!", false);
                    }
                }
                //video
                if (choosed == 2)
                {
                    string originalMediaPath = Application.persistentDataPath + "/NAT/last-video.mp4";
                    if (System.IO.File.Exists(originalMediaPath) == true)
                    {
                        NAT.Files.SaveMediaAndMakesItAvailableToGallery(NAT.Files.MediaType.Video, originalMediaPath);
                        demoNATMessages.ShowNotificationOnDemoScene("The media \"last-video.mp4\" was copied from \"AppFiles/NAT\" into the \"Movies\" scope and was scanned by the Android system! Try viewing your gallery! :)");
                    }
                    else
                    {
                        NAT.Dialogs.ShowSimpleAlertDialog("Error", "Please create a media using function \"Open Video Camera\" from class \"Camera\" and then the media can be saved to appear in the gallery!", false);
                    }
                }
                //recording
                if (choosed == 3)
                {
                    string originalMediaPath = Application.persistentDataPath + "/NAT/last-audio.aac";
                    if (System.IO.File.Exists(originalMediaPath) == true)
                    {
                        NAT.Files.SaveMediaAndMakesItAvailableToGallery(NAT.Files.MediaType.Recording, originalMediaPath);
                        demoNATMessages.ShowNotificationOnDemoScene("The media \"last-audio.aac\" was copied from \"AppFiles/NAT\" into the \"Recordings\" scope and was scanned by the Android system!");
                    }
                    else
                    {
                        NAT.Dialogs.ShowSimpleAlertDialog("Error", "Please create a media using function \"Start Recording Microphone\" from class \"Microphone\" and then the media can be saved to appear in the gallery!", false);
                    }
                }
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a Media Type To Save", new string[] { "Screenshot", "Photo", "Video", "Recording" }, false, "Done", 0);
        }

        public void Files_isFolderScannable()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppCache) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }




            bool info = NAT.Files.isFolderScannable(NAT.Files.Scope.AppCache, "");
            demoNATMessages.ShowNotificationOnDemoScene("Checking if \"<root>/AppCache\" folder is scannable by system...\n\nIs scannable: " + info);
        }

        public void Files_SetFolderScannable()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppCache) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }




            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //Yes
                if (choosed == 0)
                {
                    NAT.Files.SetFolderAsScannable(NAT.Files.Scope.AppCache, "", true);

                    demoNATMessages.ShowNotificationOnDemoScene("The folder under \"<root>/AppCache\" has been set to \"Scannable\".");
                }

                //No
                if (choosed == 1)
                {
                    NAT.Files.SetFolderAsScannable(NAT.Files.Scope.AppCache, "", false);

                    demoNATMessages.ShowNotificationOnDemoScene("The folder under \"<root>/AppCache\" has been set to \"Not Scannable\".");
                }
            };

            NAT.Dialogs.ShowRadialListDialog("Set \"AppCache\" As Scannable?", new string[] { "Yes", "No" }, false, "Done", 0);
        }

        public void Files_GetListOfFilesAndFolders()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //NATFilesClassDemo
                if (choosed == 0)
                {
                    string files = "";
                    NAT.Files.ItemsList items = NAT.Files.GetListOfFilesAndFolders(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", false);
                    foreach (string str in items.folders)
                        files += "NATFilesClassDemo/" + str + "\n";
                    foreach (string str in items.files)
                        files += "NATFilesClassDemo/" + str + "\n";

                    demoNATMessages.ShowNotificationOnDemoScene("These are the files present in the \"<root>/AppFiles/NATFilesClassDemo\"...\n\n" + files);
                }

                //folder1
                if (choosed == 1)
                {
                    string files = "";
                    NAT.Files.ItemsList items = NAT.Files.GetListOfFilesAndFolders(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder1", false);
                    foreach (string str in items.folders)
                        files += "NATFilesClassDemo/folder1/" + str + "\n";
                    foreach (string str in items.files)
                        files += "NATFilesClassDemo/folder1/" + str + "\n";

                    demoNATMessages.ShowNotificationOnDemoScene("These are the files present in the \"<root>/AppFiles/NATFilesClassDemo/folder1\"...\n\n" + files);
                }

                //folder2
                if (choosed == 2)
                {
                    string files = "";
                    NAT.Files.ItemsList items = NAT.Files.GetListOfFilesAndFolders(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2", false);
                    foreach (string str in items.folders)
                        files += "NATFilesClassDemo/folder2/" + str + "\n";
                    foreach (string str in items.files)
                        files += "NATFilesClassDemo/folder2/" + str + "\n";

                    demoNATMessages.ShowNotificationOnDemoScene("These are the files present in the \"<root>/AppFiles/NATFilesClassDemo/folder2\"...\n\n" + files);
                }
            };

            NAT.Dialogs.ShowRadialListDialog("Select a Folder To List", new string[] { "AppFiles \"NATFilesClassDemo\"", "AppFiles \"NATFilesClassDemo/folder1\"", "AppFiles \"NATFilesClassDemo/folder2\"" }, false, "List", 0);
        }

        public void Files_Exists()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //fileTwo
                if (choosed == 0)
                {
                    bool info = NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/fileTwo.txt", false);
                    demoNATMessages.ShowNotificationOnDemoScene("Checking if \"<root>/AppFiles/NATFilesClassDemo/fileTwo.txt\" file exists...\n\nExists: " + info);
                }

                //NATFilesClassDemo
                if (choosed == 1)
                {
                    bool info = NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", true);
                    demoNATMessages.ShowNotificationOnDemoScene("Checking if \"<root>/AppFiles/NATFilesClassDemo\" folder exists...\n\nExists: " + info);
                }
            };

            NAT.Dialogs.ShowRadialListDialog("Select a File To Check If Exists", new string[] { "file \"NATFilesClassDemo/fileTwo.txt\"", "folder \"NATFilesClassDemo\"" }, false, "Check", 0);
        }

        public void Files_GetAllAttributes()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.Attributes info = NAT.Files.GetAllAttributes(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/fileOne.txt");
            demoNATMessages.ShowNotificationOnDemoScene("Showing some attributes of \"<root>/AppFiles/NATFilesClassDemo/fileOne.txt\"\n\nisFile: " + info.isFile + "\nisFolder: " + info.isFolder + "\nSize: " + info.size.ToKilobytes + "kb\nLastModify: " + info.lastModify);

            string allInfo = "";
            allInfo += "isFile: " + info.isFile + "\n";
            allInfo += "isFolder: " + info.isFolder + "\n";
            allInfo += "Size: " + info.size.ToKilobytes + "kb\n";
            allInfo += "LastModify: " + info.lastModify + "\n";
            allInfo += "Parent: " + info.parentPath.Replace("/storage/emulated/0", "<root>").Replace(NAT.Applications.GetThisApplicationPackageName(), "<thisApp>") + "\n";
            allInfo += "isHidden: " + info.isHidden + "\n";
            allInfo += "isWritable: " + info.isWritable + "\n";
            allInfo += "isReadable: " + info.isReadable + "\n";
            allInfo += "PureName: " + info.pureName + "\n";
            allInfo += "Extension: " + info.extension + "\n";

            NAT.Dialogs.ShowSimpleAlertDialog("All Attributes Of \"<root>/AppFiles/NATFilesClassDemo/fileOne.txt\"", allInfo, false);
        }

        public void Files_CopyTo()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }


            if (NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileTwo.txt", false) == true)
                NAT.Files.Delete(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileTwo.txt");

            NAT.Files.CopyTo(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/fileTwo.txt", NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/fileTwo.txt\" has been copied to \"AppFiles/NATFilesClassDemo/folder2/fileTwo.txt\".");
        }

        public void Files_MoveTo()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }


            if (NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileTwo.txt", false) == true)
                NAT.Files.Delete(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileTwo.txt");

            NAT.Files.MoveTo(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/fileTwo.txt", NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/fileTwo.txt\" has been moved to \"AppFiles/NATFilesClassDemo/folder2/fileTwo.txt\".");
        }

        public void Files_Rename()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.Rename(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileOne.txt", "fileRenamed.txt");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/folder2/fileOne.txt\" has been renamed to \"AppFiles/NATFilesClassDemo/folder2/fileRenamed.txt\".");
        }

        public void Files_Delete()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.Delete(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/fileTwo.txt");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/fileTwo.txt\" has been deleted!");
        }

        public void Files_CreateFolder()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.CreateFolder(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", "NewFolderCreated");
            demoNATMessages.ShowNotificationOnDemoScene("The folder \"AppFiles/NATFilesClassDemo/NewFolderCreated\" has been created!");
        }

        public void Files_CreateFile()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }




            NAT.Files.CreateFile(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", "NewFileCreated.bin", new byte[48]);
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/NewFileCreated.bin\" has been created!");
        }

        public void Files_LoadAllBytesOfFile()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            byte[] bytes = NAT.Files.LoadAllBytesOfFile(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder2/fileTwo.txt");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/folder2/fileTwo.txt\" has been loaded!\n\nThe size is " + new NAT.Files.Size(bytes.Length).ToKilobytes + " kb!");
        }

        public void Files_WriteAllText()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.WriteAllText(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", "TextWritedFile.txt", "This is a text file! :)");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/TextWritedFile.txt\" has been writed!");
        }

        public void Files_ReadAllTextOfFile()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            string fileStr = NAT.Files.ReadAllTextOfFile(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder1/fileTwo.txt");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/folder1/fileTwo.txt\" has been readed!\n\nThe content is: \"" + fileStr + "\"!");
        }

        public void Files_WriteAllLines()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.WriteAllLines(NAT.Files.Scope.AppFiles, "NATFilesClassDemo", "LinesWritedFile.txt", new string[] { "LineOne", "LineTwo", "LineThree" });
            demoNATMessages.ShowNotificationOnDemoScene("The lines \"AppFiles/NATFilesClassDemo/LinesWritedFile.txt\" has been writed!");
        }

        public void Files_ReadAllLinesOfFile()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            string[] fileStrs = NAT.Files.ReadAllLinesOfFile(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/folder1/fileTwo.txt");
            demoNATMessages.ShowNotificationOnDemoScene("The file \"AppFiles/NATFilesClassDemo/folder1/fileTwo.txt\" has been readed!\n\nThe count of lines is: \"" + fileStrs.Length + "\"!");
        }

        public void Files_OpenWithDefaultApplication()
        {
            //If the scope is not available, cancel
            if (NAT.Files.GetScopeAvailability(NAT.Files.Scope.AppFiles) != NAT.Files.ScopeStatus.AvailableToUse)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Could not perform this task! It looks like the scope is unavailable, memory is inaccessible, or there are no file permissions to be able to do this.");
                return;
            }



            NAT.Files.OpenWithDefaultApplication(NAT.Files.Scope.AppFiles, "Open File With...", "NATFilesClassDemo/fileOne.txt");
            demoNATMessages.ShowNotificationOnDemoScene("Opening the file \"AppFiles/NATFilesClassDemo/fileOne.txt\"...");
        }

        public void Files_DoFilePickerOperation()
        {
            //If not have files permission, cancel
            if (NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.AccessFilesAndMedia) == false)
            {
                new NAT.Permissions.PermissionRequester().addPermissionToRequest(NAT.Permissions.AndroidPermission.AccessFilesAndMedia).RequestThisPermissions();
                return;
            }






            NATEvents.onFilesFilePickerOperationFinished += (NAT.Files.FilePickerOperationStatus status, NAT.Files.FilePickerOperationResponse response) =>
            {
                if (status == NAT.Files.FilePickerOperationStatus.Successfully)
                {
                    string operationMade = "";

                    if (response.operationType == NAT.Files.FilePickerAction.CreateFile)
                    {
                        NAT.Files.WriteAllText(response.scopeOfOperation, Path.GetDirectoryName(response.pathFromOperationInScope), Path.GetFileName(response.pathFromOperationInScope), "This is a text file created by Native Android Toolkit File Picker!");
                        operationMade = "The file \"" + Path.GetFileName(response.pathFromOperationInScope) + "\" was created on scope \"" + response.scopeOfOperation + "\"!";
                    }
                    if (response.operationType == NAT.Files.FilePickerAction.OpenFile)
                    {
                        float kbSize = NAT.Files.GetAllAttributes(response.scopeOfOperation, response.pathFromOperationInScope).size.ToKibibytes;
                        operationMade = "The file \"" + Path.GetFileName(response.pathFromOperationInScope) + "\" of scope \"" + response.scopeOfOperation + "\" have " + kbSize.ToString("F2") + "kb!";
                    }

                    NAT.Dialogs.ShowSimpleAlertDialog("File Picker Operation Result", "Scope Selected: " + response.scopeOfOperation.ToString() + "\nPath: \"" + response.pathFromOperationInScope + "\" \nOperation Type: " + response.operationType + "\n\n" + operationMade, false);
                }
                if (status == NAT.Files.FilePickerOperationStatus.InvalidScope)
                    NAT.Dialogs.ShowSimpleAlertDialog("File Picker Operation Result", "Invalid scope! Please select another location!", false);
                if (status == NAT.Files.FilePickerOperationStatus.Canceled)
                    NAT.Dialogs.ShowSimpleAlertDialog("File Picker Operation Result", "The File Picker operation has been canceled by the user!", false);
                if (status == NAT.Files.FilePickerOperationStatus.Unknown)
                    NAT.Dialogs.ShowSimpleAlertDialog("File Picker Operation Result", "An internal error occurred in File Picker!", false);
            };


            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //If choose "create file"
                if (choosed == 0)
                {
                    new NAT.Files.FilePickerOperation(NAT.Files.FilePickerInterfaceMode.LandscapeFullscreen, "Select a Place To Create File")
                                    .setCreateFileOperation(NAT.Files.FilePickerDefaultScope.Documents, "textFileCreated.txt")
                                    .setMimeType(NAT.Files.MimeType.TXT)
                                    .OpenFilePicker();
                }
                //If choose "open file"
                if (choosed == 1)
                {
                    new NAT.Files.FilePickerOperation(NAT.Files.FilePickerInterfaceMode.LandscapeFullscreen, "Select a File To Be Openned")
                                    .setOpenFileOperation(NAT.Files.FilePickerDefaultScope.Documents)
                                    .setMimeType(NAT.Files.MimeType.All)
                                    .OpenFilePicker();
                }
            };

            NAT.Dialogs.ShowRadialListDialog("What Operation Do You Want To Do?", new string[] { "Create File", "Open File" }, false, "Choose", 0);
        }

        //-- TASKS --//

        public void Tasks_ForceTaskExecution()
        {
#pragma warning disable 0162

            if (TasksSourceCode.TASK_SOURCE_CODE == "")
            {
                demoNATMessages.ShowNotificationOnDemoScene("Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run.");
                return;
            }

            new NAT.Tasks.ScheduledTask(NAT.Tasks.ExecutionMode.ForceToExecuteNow)
                         .EnableThisTask();
            demoNATMessages.ShowNotificationOnDemoScene("Running the programmed task...");

#pragma warning restore 0162
        }

        public void Tasks_isRunningSomeTaskNow()
        {
            bool isRunning = NAT.Tasks.isRunningSomeTaskNow();
            demoNATMessages.ShowNotificationOnDemoScene("Is Running Some Task Now?\n" + isRunning);
        }

        public void Tasks_isTaskEnabled()
        {
#pragma warning disable 0162

            if (TasksSourceCode.TASK_SOURCE_CODE == "")
            {
                demoNATMessages.ShowNotificationOnDemoScene("Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run.");
                return;
            }

            bool info = NAT.Tasks.isTaskEnabled();
            demoNATMessages.ShowNotificationOnDemoScene("Is Task Enabled?\n" + info);

#pragma warning restore 0162
        }

        public void Tasks_EnableTask()
        {
#pragma warning disable 0162

            if (TasksSourceCode.TASK_SOURCE_CODE == "")
            {
                demoNATMessages.ShowNotificationOnDemoScene("Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run.");
                return;
            }

            new NAT.Tasks.ScheduledTask(NAT.Tasks.ExecutionMode.ExecuteScheduled)
                         .setMinutesInFuture(3)
                         .EnableThisTask();
            demoNATMessages.ShowNotificationOnDemoScene("Enabling Task To Be Runned In 3 Minutes In The Future...");

#pragma warning restore 0162
        }

        public void Tasks_DisableTask()
        {
#pragma warning disable 0162

            if (TasksSourceCode.TASK_SOURCE_CODE == "")
            {
                demoNATMessages.ShowNotificationOnDemoScene("Unable to proceed with the operation. There is no programming done in NAT preferences for a task to run.");
                return;
            }

            NAT.Tasks.DisableTask();
            demoNATMessages.ShowNotificationOnDemoScene("Disabling Task...");

#pragma warning restore 0162
        }

        //-- AUDIO PLAYER --//

        public void AudioPlayer_GetAudioDataWithoutPlay()
        {
            //If the WAV sound is not available, create the file
            if (NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/exampleMusic.wav", false) == false)
                Files_ExportTheExampleMusicToAppFilesEscopeInNATFilesClassDemoFolder();

            //Get audio data
            NAT.AudioPlayer.AudioData audioData = NAT.AudioPlayer.GetAudioDataWithoutPlay(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/exampleMusic.wav");
            demoNATMessages.ShowNotificationOnDemoScene("Audio data for \"exampleMusic.wav\"\n\nis Audio Supported: " + audioData.isAudio + "\nDuration: " + audioData.duration.MinuteString + ":" + audioData.duration.SecondString);
        }

        public void AudioPlayer_isPlayingAudio()
        {
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            demoNATMessages.ShowNotificationOnDemoScene("Is Playing Audio?\n" + isPlaying);
        }

        public void AudioPlayer_PlayAudio()
        {
            //If the WAV sound is not available, create the file
            if (NAT.Files.Exists(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/exampleMusic.wav", false) == false)
                Files_ExportTheExampleMusicToAppFilesEscopeInNATFilesClassDemoFolder();

            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Resuming Audio...");
                NAT.AudioPlayer.PlayAudio();
            }
            if (isPlaying == false)
            {
                NATEvents.onAudioPlayerFinishedPlaying += () =>
                {
                    demoNATMessages.ShowNotificationOnDemoScene("The audio was finished!");

                    //UNCOMMENT     NAT.PlayGames.UnlockAchievement(PlayGamesResources.Achievement.music_lover);
                };

                demoNATMessages.ShowNotificationOnDemoScene("Playing Audio...");
                NAT.AudioPlayer.PlayAudio(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/exampleMusic.wav");
            }
        }

        public void AudioPlayer_PauseAudio()
        {
            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Pausing Audio...");
                NAT.AudioPlayer.PauseAudio();
            }
            if (isPlaying == false)
                demoNATMessages.ShowNotificationOnDemoScene("There is no audio being played!");
        }

        public void AudioPlayer_GetAudioPart()
        {
            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                float audioPart = NAT.AudioPlayer.GetAudioPart();
                NAT.AudioPlayer.AudioData audioData = NAT.AudioPlayer.GetAudioDataWithoutPlay(NAT.Files.Scope.AppFiles, "NATFilesClassDemo/exampleMusic.wav");
                NAT.DateTime.Calendar audioPartToTime = new NAT.DateTime.Calendar((long)((double)audioData.duration.GetUnixMillisTime(NAT.DateTime.TimeMode.UtcTime) * (double)audioPart));
                demoNATMessages.ShowNotificationOnDemoScene("Current part is\n" + audioPartToTime.MinuteString + ":" + audioPartToTime.SecondString);
            }
            if (isPlaying == false)
                demoNATMessages.ShowNotificationOnDemoScene("There is no audio being played!");
        }

        public void AudioPlayer_SetAudioPart()
        {
            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Defining part to 00:15...");
                NAT.AudioPlayer.SetAudioPart(0.5f);
            }
            if (isPlaying == false)
                demoNATMessages.ShowNotificationOnDemoScene("There is no audio being played!");
        }

        public void AudioPlayer_SetAudioVolume()
        {
            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Defining volume to 10%...");
                NAT.AudioPlayer.SetAudioVolume(10.0f, 10.0f);
            }
            if (isPlaying == false)
                demoNATMessages.ShowNotificationOnDemoScene("There is no audio being played!");
        }

        public void AudioPlayer_StopAudio()
        {
            //Get if is playing audio
            bool isPlaying = NAT.AudioPlayer.isPlayingAudio();
            if (isPlaying == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Stopping Audio...");
                NAT.AudioPlayer.StopAudio();
            }
            if (isPlaying == false)
                demoNATMessages.ShowNotificationOnDemoScene("There is no audio being played!");
        }

        //-- POWER MANAGER --//

        public void PowerManager_isExactAlarmsAndTasksAllowed()
        {
            bool isAllowed = NAT.PowerManager.isExactAlarmsAndTasksAllowed();
            demoNATMessages.ShowNotificationOnDemoScene("Is Exact Alarms And Tasks Allowed?\n" + isAllowed);
        }

        public void PowerManager_OpenAlarmsAndRemindersAccess()
        {
            NAT.PowerManager.OpenAlarmsAndRemindersAccess();
        }

        public void PowerManager_isThisDeviceProblematic()
        {
            bool isProblematic = NAT.PowerManager.isThisDeviceProblematic();
            demoNATMessages.ShowNotificationOnDemoScene("Is This Device Problematic?\n" + isProblematic);
        }

        public void PowerManager_RequestAutoStartONIfProblematic()
        {
            NAT.PowerManager.RequestAutoStartONIfProblematic();
        }

        //-- PLAY GAMES --//

        public void PlayGames_isSignedIn()
        {
            bool isSigned = NAT.PlayGames.isSignedIn();
            demoNATMessages.ShowNotificationOnDemoScene("Is Signed In Play Games Now?\n" + isSigned);
        }

        public void PlayGames_DoManualSignIn()
        {
            //Check if is already signed in
            if (NAT.PlayGames.isSignedIn() == true)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You are already signed in on Google Play Games!");
                return;
            }







            NATEvents.onPlayGamesInitializationComplete += (bool userSignedIn, NAT.PlayGames.UserData userData) =>
            {
                if (userSignedIn == true)
                {
                    NAT.Notifications.ShowToast("Hello " + userData.displayName + "! You have been connected to Google Play Games!", true);
                    this.gameObject.GetComponentInChildren<DemoNATDataBase>().playGamesUserData = userData;
                }
                if (userSignedIn == false)
                {
                    demoNATMessages.ShowNotificationOnDemoScene("It was not possible to sign in to Google Play Games!");
                }
            };

            NAT.PlayGames.DoManualSignIn();
        }

        public void PlayGames_ShowUserIcon()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }





            NAT.PlayGames.UserData userData = this.gameObject.GetComponentInChildren<DemoNATDataBase>().playGamesUserData;

            if (userData.hasIconImage == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Oops! You don't have an icon image on Google Play Games!");
                return;
            }

            Texture2D userIcon = userData.GetUserIconImage();
            this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is your icon, " + userData.displayName + "!", userIcon, 360, 360);
        }

        public void PlayGames_ShowAllAchievements()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            NAT.PlayGames.ShowAllAchievements();
        }

        public void PlayGames_RevealAchievement()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            demoNATMessages.ShowNotificationOnDemoScene("Achievement Revealed! Check your achievements panel!");
            NAT.PlayGames.RevealAchievement(PlayGamesResources.Achievement.NONE);
            //UNCOMMENT     NAT.PlayGames.RevealAchievement(PlayGamesResources.Achievement.speech_100);
        }

        public void PlayGames_UnlockAchievement()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            demoNATMessages.ShowNotificationOnDemoScene("Achievement unlocked!");
            NAT.PlayGames.UnlockAchievement(PlayGamesResources.Achievement.NONE);
            //UNCOMMENT     NAT.PlayGames.UnlockAchievement(PlayGamesResources.Achievement.toss_a_achievement_to_your_dev);
        }

        public void PlayGames_IncrementAchievement()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }





            demoNATMessages.ShowNotificationOnDemoScene("Increasing achievement in +1 point...");
            NAT.PlayGames.IncrementAchievement(PlayGamesResources.Achievement.NONE, 1);
            //UNCOMMENT     NAT.PlayGames.IncrementAchievement(PlayGamesResources.Achievement._3_tests, 1);
        }

        public void PlayGames_LoadEventData()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }





            NATEvents.onPlayGamesEventDataLoaded += (bool isSuccessfully, NAT.PlayGames.EventData eventData) =>
            {
                //On receive callback of event data loaded
                if (isSuccessfully == true)
                {
                    //Show event icon
                    NATEvents.onSimpleAlertDialogOk += () =>
                    {
                        Texture2D eventIcon = eventData.GetEventIconImage();
                        if (eventIcon != null)
                            this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is the Event icon!", eventIcon, 360, 360);
                    };

                    //Show event data loaded
                    NAT.Dialogs.ShowSimpleAlertDialog("Event Data Loaded", "Name: \"" + eventData.name + "\"\nID: " + eventData.id + "\nUnformatted Value: " + eventData.unformattedValue + "\nIs Visible: " + eventData.isVisible, false);
                }
                if (isSuccessfully == false)
                    demoNATMessages.ShowNotificationOnDemoScene("There was a problem on Loading the Event data!");
            };

            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //If choose "Dialogues Opened"
                if (choosed == 0)
                {
                    NAT.PlayGames.LoadEventData(PlayGamesResources.Event.NONE, true);
                    //UNCOMMENT     NAT.PlayGames.LoadEventData(PlayGamesResources.Event.dialogues_opened, true);
                }
                //If choose "Fullscreen Webviews Opened"
                if (choosed == 1)
                {
                    NAT.PlayGames.LoadEventData(PlayGamesResources.Event.NONE, true);
                    //UNCOMMENT     NAT.PlayGames.LoadEventData(PlayGamesResources.Event.fullscreen_webviews_opened, true);
                }
                //If choose "Dialogs Webviews Opened"
                if (choosed == 2)
                {
                    NAT.PlayGames.LoadEventData(PlayGamesResources.Event.NONE, true);
                    //UNCOMMENT     NAT.PlayGames.LoadEventData(PlayGamesResources.Event.dialogs_webviews_opened, true);
                }
                //If choose "Event Increments"
                if (choosed == 3)
                {
                    NAT.PlayGames.LoadEventData(PlayGamesResources.Event.NONE, true);
                    //UNCOMMENT     NAT.PlayGames.LoadEventData(PlayGamesResources.Event.event_increments, true);
                }

                demoNATMessages.ShowNotificationOnDemoScene("Loading Event data...");
            };

            NAT.Dialogs.ShowRadialListDialog("Which Event do you want to see data from?", new string[] { "Dialogues Opened", "Fullscreen Webviews Opened", "Dialogs Webviews Opened", "Event Increments" }, false, "Choose", 3);
        }

        public void PlayGames_IncrementEvent()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }





            demoNATMessages.ShowNotificationOnDemoScene("Increasing Event in +1 point...");
            NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.NONE, 1);
            //UNCOMMENT     NAT.PlayGames.IncrementEvent(PlayGamesResources.Event.event_increments, 1);
        }

        public void PlayGames_ShowTheLeaderboard()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }







            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                //If choose "Those Who "Dialogs" The Most"
                if (choosed == 0)
                {
                    NAT.PlayGames.ShowTheLeaderboard(PlayGamesResources.Leaderboard.NONE);
                    //UNCOMMENT     NAT.PlayGames.ShowTheLeaderboard(PlayGamesResources.Leaderboard.those_who_dialogs_the_most);
                }
                //If choose "Those Who Send Their Scores"
                if (choosed == 1)
                {
                    NAT.PlayGames.ShowTheLeaderboard(PlayGamesResources.Leaderboard.NONE);
                    //UNCOMMENT     NAT.PlayGames.ShowTheLeaderboard(PlayGamesResources.Leaderboard.those_who_send_their_scores);
                }
            };

            NAT.Dialogs.ShowRadialListDialog("Which Leaderboard you want to see?", new string[] { "Those Who \"Dialogs\" The Most", "Those Who Send Their Scores" }, false, "Choose", 1);
        }

        public void PlayGames_SubmitScoreToLeaderboard()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }





            demoNATMessages.ShowNotificationOnDemoScene("Submiting score to Leaderboard...");
            NAT.PlayGames.SubmitScoreToLeaderboard(PlayGamesResources.Leaderboard.NONE, 1);
            //UNCOMMENT     NAT.PlayGames.SubmitScoreToLeaderboard(PlayGamesResources.Leaderboard.those_who_send_their_scores, Random.Range(1, 100));
        }

        public void PlayGames_isFriendListAccessible()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            demoNATMessages.ShowNotificationOnDemoScene("Is User Friend List Accessible?\n" + NAT.PlayGames.isFriendListAccessible().ToString());
        }

        public void PlayGames_RequestFriendListAccess()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            NATEvents.onPlayGamesFriendListRequestResult += (bool isUserFriendListAccessibleNow) =>
            {
                //If is accessible
                if (isUserFriendListAccessibleNow == true)
                {
                    NAT.Dialogs.ShowSimpleAlertDialog("Friend List Access", "The user friends list is now accessible!", false);
                }
                //If is not accessible
                if (isUserFriendListAccessibleNow == false)
                {
                    NAT.Dialogs.ShowSimpleAlertDialog("Friend List Access", "User friends list is NOT accessible right now.", false);
                }
            };

            NAT.PlayGames.RequestFriendListAccess(NAT.PlayGames.FriendAccessInterfaceMode.LandscapeFullscreen);
        }

        public void PlayGames_LoadUserFriendList()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }




            //Check if friends list is inaccessible cancel
            if (NAT.PlayGames.isFriendListAccessible() != NAT.PlayGames.FriendListAccessibility.Accessible)
            {
                demoNATMessages.ShowNotificationOnDemoScene("Access to the friends list was Blocked by the player! Please request access to the friends list!");
                return;
            }






            NATEvents.onPlayGamesUserFriendListLoaded += (NAT.PlayGames.FriendList userFriendList) =>
            {
                //If have friends
                if (userFriendList.allUserFriends.Length > 0)
                {
                    //Create a array with the nick name of all friends
                    List<string> allFriendsNicknames = new List<string>();
                    foreach (NAT.PlayGames.Friend friend in userFriendList.allUserFriends)
                        allFriendsNicknames.Add(friend.displayName);

                    //Show a dialog to choose a friend to view
                    NATEvents.onRadialListDialogDone += (int choosed) =>
                    {
                        //Get choosed friend
                        NAT.PlayGames.Friend friend = userFriendList.allUserFriends[choosed];

                        //Show friend informations
                        NATEvents.onSimpleAlertDialogOk += () =>
                        {
                            if (friend.hasIconImage == true)
                                this.gameObject.GetComponentInChildren<DemoNATTexture2D>().OpenTexture2DViewer("This is the icon of \"" + friend.displayName + "\"!", friend.GetFriendIconImage(), 360, 360);
                        };
                        NAT.Dialogs.ShowSimpleAlertDialog("Friend Information", "Display Name: " + friend.displayName + "\nLevel: " + friend.currentLevel + "\nHas Icon Image: " + friend.hasIconImage, false);
                    };
                    NAT.Dialogs.ShowRadialListDialog("Choose a Friend to see!", allFriendsNicknames.ToArray(), false, "Choose", 0);
                }
                //If not have friends
                if (userFriendList.allUserFriends.Length == 0)
                    demoNATMessages.ShowNotificationOnDemoScene("You don't have any friends on Google Play Games :(");
            };

            demoNATMessages.ShowNotificationOnDemoScene("Loading Friend List...");
            NAT.PlayGames.LoadUserFriendList(true);
        }

        public void PlayGames_OpenProfileComparation()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }







            NATEvents.onPlayGamesUserFriendListLoaded += (NAT.PlayGames.FriendList userFriendList) =>
            {
                //If have friends
                if (userFriendList.allUserFriends.Length > 0)
                {
                    //Create a array with the nick name of all friends
                    List<string> allFriendsNicknames = new List<string>();
                    foreach (NAT.PlayGames.Friend friend in userFriendList.allUserFriends)
                        allFriendsNicknames.Add(friend.displayName);

                    //Show a dialog to choose a friend to view
                    NATEvents.onRadialListDialogDone += (int choosed) =>
                    {
                        //Get choosed friend
                        NAT.PlayGames.Friend friend = userFriendList.allUserFriends[choosed];

                        //OPEN THE PROFILE COMPARATION
                        NAT.PlayGames.OpenProfileComparation(friend.playerId, "Your Friend", "You");
                    };
                    NAT.Dialogs.ShowRadialListDialog("Choose a Friend to Profile compare!", allFriendsNicknames.ToArray(), false, "Choose", 0);
                }
                //If not have friends
                if (userFriendList.allUserFriends.Length == 0)
                    demoNATMessages.ShowNotificationOnDemoScene("You don't have any friends on Google Play Games, to compare! :(");
            };

            demoNATMessages.ShowNotificationOnDemoScene("Loading Friend List...");
            NAT.PlayGames.LoadUserFriendList(false);
        }

        public void PlayGames_OpenCloudSaveUI()
        {
            //Check if is signed in
            if (NAT.PlayGames.isSignedIn() == false)
            {
                demoNATMessages.ShowNotificationOnDemoScene("You need to be signed into Google Play Games, to do this!");
                return;
            }






            NATEvents.onPlayGamesCloudSaveUIResult += (NAT.PlayGames.CloudSaveUIResponse cloudSaveResult, string fileNameToBeCreatedOrLoaded) =>
            {
                //If is desired to create a save
                if (cloudSaveResult == NAT.PlayGames.CloudSaveUIResponse.CreateNew)
                {
                    NAT.Notifications.ShowToast("Creating new Save...", true);

                    //Create the file in cloud
                    NATEvents.onPlayGamesCloudSaveCreateFileResult += (bool isSuccessfully) =>
                    {
                        if (isSuccessfully == true)
                            NAT.Dialogs.ShowSimpleAlertDialog("Play Games Cloud Save", "Save file was writed to the cloud of Play Games!", false);
                        if (isSuccessfully == false)
                            NAT.Dialogs.ShowSimpleAlertDialog("Play Games Cloud Save", "It was not possible to save the file to the cloud of Play Games!", false);
                    };

                    //Create a save file and cover
                    byte[] saveGameBytes = Encoding.UTF8.GetBytes("This is a example save game of text created on " + new NAT.DateTime.Calendar().ToString().Replace("Time of this Calendar ", "") + ".");

                    //Save file to cloud (informing that was played 5 minutes)
                    NAT.PlayGames.CreateFileOnCloudSave(fileNameToBeCreatedOrLoaded, "This is a NAT Save!", 0, new NAT.DateTime.Calendar(1970, 1, 1, 0, 5, 0), saveGameBytes, ScreenCapture.CaptureScreenshotAsTexture());
                }
                //If is desired to load
                if (cloudSaveResult == NAT.PlayGames.CloudSaveUIResponse.LoadSave)
                {
                    NAT.Notifications.ShowToast("Loading Save...", true);

                    //Process the returned file from the cloud
                    NATEvents.onPlayGamesCloudSaveReadFileResult += (NAT.PlayGames.CloudSaveLoadStatus loadStatus, byte[] saveGameLoadedBytes) =>
                    {
                        if (loadStatus == NAT.PlayGames.CloudSaveLoadStatus.Success)
                        {
                            string saveFileLoadedContent = Encoding.UTF8.GetString(saveGameLoadedBytes);

                            NAT.Dialogs.ShowSimpleAlertDialog("Play Games Cloud Save", "The Save file has been successfully loaded from the Play Games cloud! This is the content of the loaded file...\n\n\"" + saveFileLoadedContent + "\"", false);
                        }
                        if (loadStatus == NAT.PlayGames.CloudSaveLoadStatus.ErrorOnRead)
                            NAT.Dialogs.ShowSimpleAlertDialog("Play Games Cloud Save", "There was a problem when reading the saved file in the Play Games cloud.", false);
                        if (loadStatus == NAT.PlayGames.CloudSaveLoadStatus.ErrorOnFind)
                            NAT.Dialogs.ShowSimpleAlertDialog("Play Games Cloud Save", "There was a problem when trying to find for the saved file in the Play Games cloud.", false);
                    };

                    //Load save file from cloud
                    NAT.PlayGames.ReadFileOfCloudSave(fileNameToBeCreatedOrLoaded, NAT.PlayGames.CloudSaveConflictResolution.ResolveByMostRecentlyModify);
                }
                //If have a error
                if (cloudSaveResult == NAT.PlayGames.CloudSaveUIResponse.Error)
                    NAT.Notifications.ShowToast("There was an error processing your Play Games Cloud Save request!", true);
                //If was canceled
                if (cloudSaveResult == NAT.PlayGames.CloudSaveUIResponse.Canceled)
                    NAT.Notifications.ShowToast("The Play Games Cloud Save UI was just closed.", true);
            };

            NAT.PlayGames.OpenCloudSaveUI(NAT.PlayGames.CloudSaveInterfaceMode.LandscapeFullscreen, "Native Android Toolkit App Cloud Saving", true, true);
        }

















        /*
        * 
        * 
        * The methods below should not be used. They are for demonstration purposes or testing in Demo Scene only.
        * 
        * 
        */

        public void Notification_ForceSendNotificationWithActions()
        {
            /*
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *  WARNING: This method should not be used in your game. This method only exists here for use in the demo scene and for testing purposes.
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            */

            NATEvents.onCheckboxListDialogDone += (bool[] chooseds) =>
            {
                if (Application.isEditor == true)
                {
                    string clickAction = "";
                    string bt1Txt = "";
                    string bt1Action = "";
                    string bt2Txt = "";
                    string bt2Action = "";
                    if (chooseds[0] == true)
                        clickAction = NotificationsActions.Action.ClickAction1.ToString();
                    if (chooseds[1] == true)
                    {
                        bt1Txt = "Button 1";
                        bt1Action = NotificationsActions.Action.ButtonAction1.ToString();
                    }
                    if (chooseds[2] == true)
                    {
                        bt2Txt = "Button 2";
                        bt2Action = NotificationsActions.Action.ButtonAction2.ToString();
                    }

                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.emulatedAndroidInterface.SendNotification("Notification with Actions", "This is a notification with Actions.", clickAction, bt1Txt, bt1Action, bt2Txt, bt2Action);
                }
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    string clickAction = "";
                    string bt1Txt = "";
                    string bt1Action = "";
                    string bt2Txt = "";
                    string bt2Action = "";
                    if (chooseds[0] == true)
                        clickAction = NotificationsActions.Action.ClickAction1.ToString();
                    if (chooseds[1] == true)
                    {
                        bt1Txt = "Button 1";
                        bt1Action = NotificationsActions.Action.ButtonAction1.ToString();
                    }
                    if (chooseds[2] == true)
                    {
                        bt2Txt = "Button 2";
                        bt2Action = NotificationsActions.Action.ButtonAction2.ToString();
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("SendNotification", NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().unityPlayerContext, "Notification with Actions", "This is a notification with Actions.", clickAction, bt1Txt, bt1Action, bt2Txt, bt2Action);
                }

                demoNATMessages.ShowNotificationOnDemoScene("The notification has been delivered.");
            };
            NAT.Dialogs.ShowCheckListDialog("What actions to add to the Notification?", new string[] { "OnClick", "Button1", "Button2" }, false, "Send", new bool[] { false, false, false });
        }

        public IEnumerator DateTime_WaitAndRunCodeOfElapsedTimeWhenPaused(NAT.DateTime.TimeElapsedWhilePaused timeElapsedWhilePaused)
        {
            /*
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *  WARNING: This method should not be used in your game. This method only exists here for use in the demo scene and for testing purposes.
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            *
            */

            //Wait a time before show elapsed paused time, to prevent spam
            yield return new WaitForSeconds(1.5f);

            NAT.DateTime.Calendar time = timeElapsedWhilePaused.timeElapsed_accordingRealtimeClockAfterBoot;

            demoNATMessages.ShowNotificationOnDemoScene("Hi again! :)\nSee below how much time passed with app paused, provided by DateTime Event!\n" + time.YearString + "y, " + time.MonthString + "m, " + time.DayString + "d, " + time.HourString + "h, " + time.MinuteString + "m and " + time.SecondString + "s");
        }

        public void Files_CreateTheNATFilesClassDemoFolderInAppFiles()
        {
            //If is Android
            if (Application.isEditor == false)
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/NATFilesClassDemo");
                Directory.CreateDirectory(Application.persistentDataPath + "/NATFilesClassDemo/folder1");
                Directory.CreateDirectory(Application.persistentDataPath + "/NATFilesClassDemo/folder2");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/fileTwo.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/folder1/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/folder1/fileTwo.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/folder2/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/NATFilesClassDemo/folder2/fileTwo.txt", "TestFile");
            }
            //If is Editor
            if (Application.isEditor == true)
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Files/NATFilesClassDemo");
                Directory.CreateDirectory(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder1");
                Directory.CreateDirectory(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder2");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/fileTwo.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder1/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder1/fileTwo.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder2/fileOne.txt", "TestFile");
                File.WriteAllText(Application.persistentDataPath + "/Files/NATFilesClassDemo/folder2/fileTwo.txt", "TestFile");
            }
        }

        public void Files_ExportTheExampleMusicToAppFilesEscopeInNATFilesClassDemoFolder()
        {
            //Prepare the path for the music file
            string musicPath = NAT.Files.GetAbsolutePathForScope(NAT.Files.Scope.AppFiles) + "/NATFilesClassDemo/exampleMusic";

            //If the file not exists
            if (File.Exists(musicPath) == false)
            {
                //Copy the example music to internal memory
                AudioClipToWav.SavWav.Save(NAT.Files.GetAbsolutePathForScope(NAT.Files.Scope.AppFiles) + "/NATFilesClassDemo/exampleMusic", this.gameObject.GetComponentInChildren<DemoNATDataBase>().exampleMusicToPlay);
            }
        }
    }
}