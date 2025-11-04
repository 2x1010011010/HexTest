using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTAssets.NativeAndroidToolkit
{
    public class DemoNATTexture2D : MonoBehaviour
    {
        public GameObject texture2dViewerObj;
        public Text titleShower;
        public Image texture2dShower;

        public void OpenTexture2DViewer(string title, Texture2D texture2D, int widthOfShower, int heightOfShower)
        {
            titleShower.text = title;
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texture2dShower.sprite = sprite;
            texture2dShower.GetComponent<RectTransform>().sizeDelta = new Vector2(widthOfShower, heightOfShower);

            texture2dViewerObj.SetActive(true);
        }

        public void CloseTexture2DViewer()
        {
            texture2dViewerObj.SetActive(false);
        }
    }
}