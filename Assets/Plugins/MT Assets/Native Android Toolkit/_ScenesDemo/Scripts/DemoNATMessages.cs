using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTAssets.NativeAndroidToolkit
{
    public class DemoNATMessages : MonoBehaviour
    {
        public Text notificatonText = null;
        public Animator notificationsBox = null;

        public void ShowNotificationOnDemoScene(string message)
        {
            notificationsBox.gameObject.SetActive(false);
            notificationsBox.gameObject.SetActive(true);
            notificatonText.text = message;
            notificationsBox.SetTrigger("showNotification");
        }
    }
}