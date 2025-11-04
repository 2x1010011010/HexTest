using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTAssets.NativeAndroidToolkit
{
    public class DemoNATScheduling : MonoBehaviour
    {
        public int currentSchedulingChannel = 1;
        public Text scheduleToFutureButton;
        public Text scheduleToRepetitiveButton;
        public Text isScheduledToFutureButton;
        public Text cancelScheduledToFutureButton;

        void Start()
        {
            scheduleToFutureButton.text = "(CH " + currentSchedulingChannel + ") Schedule New Notification";
            scheduleToRepetitiveButton.text = "(CH " + currentSchedulingChannel + ") Create Repetitive Notify";
            isScheduledToFutureButton.text = "(CH " + currentSchedulingChannel + ") Have Scheduled Notify? ";
            cancelScheduledToFutureButton.text = "(CH " + currentSchedulingChannel + ") Cancel Scheduled Notify";
        }

        public void ChangeCurrentSchedulingChannel()
        {
            NATEvents.onRadialListDialogDone += (int choosed) =>
            {
                int newChannel = choosed + 1;
                this.GetComponentInParent<DemoNAT>().GetComponentInChildren<DemoNATMessages>().ShowNotificationOnDemoScene("Now you are editing Channel " + newChannel + " for Scheduled Notifications.");
                currentSchedulingChannel = newChannel;
                Start();
            };

            NAT.Dialogs.ShowRadialListDialog("Choose a New Notification Channel", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" }, false, "Done", (currentSchedulingChannel - 1));
        }
    }
}