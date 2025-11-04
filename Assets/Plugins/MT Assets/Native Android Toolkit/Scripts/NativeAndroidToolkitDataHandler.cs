#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTAssets.NativeAndroidToolkit;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

[AddComponentMenu("")] //Hide this script in component menu.
public class NativeAndroidToolkitDataHandler : MonoBehaviour
{
    /*
     * This class is responsible for communicating between your game and Android Native code.
    */

    //Classes of script
    public class EmulatedAndroidInterface
    {
        /*
        * This class store all references, informations and methods to handle emulated Android Interface
        * for Unity Editor.
        */

        //Cache variables
        private Vector2[] notificationChannelsUsage = new Vector2[21]; //<- (1 = used, 1 = haveRepetitive)
        private string currentClipboardContent = "NAT PLACE HOLDER EDITOR - SIMULATED CLIPBOARD CONTENT";
        private NAT.Location.LocationRunning locationBeingTracked = NAT.Location.LocationRunning.None;
        private bool recordingMicrophone = false;
        private bool listeningSpeechToText = false;
        private bool antiScreenshotEnabled = false;
        private bool audioPlayerPlaying = false;

        //Private variables

        private Canvas canvas = null;                                             //<- Canvas start
        private CanvasScaler canvasScaler = null;
        private GraphicRaycaster canvasGraphicRaycaster = null;
        private RectTransform dialogsInterface = null;                            //<- Dialogs start
        private Button dialogsInterfaceCancelationButton = null;
        private Text dialogUiInterfaceTitleText = null;
        private Button[] dialogUiInterfaceButtonsBt = new Button[3];
        private Text[] dialogUiInterfaceButtonsText = new Text[3];
        private RectTransform dialogUiInterfaceContentTextGroup = null;
        private Text dialogUiInterfaceContentText = null;
        private RectTransform dialogUiInterfaceContentChecksGroup = null;
        private Toggle[] dialogUiInterfaceToggles = new Toggle[100];
        private Text[] dialogUiInterfaceToggleTexts = new Text[100];
        private RectTransform notificationsInterface = null;                      //<- Notifications start
        private Coroutine notificationsToastCurrentShowingToast = null;
        private RectTransform notificationsToastGroup = null;
        private Text notificationsToastText = null;
        private Coroutine notificationsPushCurrentShowingPush = null;
        private RectTransform notificationsPushGroup = null;
        private Slider notificationsPushTime = null;
        private Text notificationsPushTitle = null;
        private Text notificationsPushText = null;
        private Button[] notificationsPushButtons = new Button[3];
        private Text[] notificationsButtonsText = new Text[3];
        private RectTransform webviewInterface = null;                             //<- Webview start
        private RectTransform webviewExibition = null;
        private Button webviewExibitionCloseButton = null;
        private RectTransform simulatedActivityInterface = null;                   //<- Simulated Activity
        private RectTransform simulatedActivityExibition = null;
        private Text simulatedActivityExibitionText = null;
        private Button simulatedActivityExibitionCloseButton = null;
        private RectTransform locationInterface = null;                            //<- Location
        private RectTransform usingLocationInterface = null;
        private Coroutine locationTrackingEventSimulator = null;
        private RectTransform microphoneInterface = null;                          //<- Microphone
        private RectTransform usingMicrophoneInterface = null;
        private RectTransform datetimeInterface = null;                            //<- Datetime start
        private Text pickerUiInterfaceTitleText = null;
        private Button pickerUiInterfaceButtonBt = null;
        private InputField[] pickerUiInterfaceInputs = new InputField[3];
        private Text[] pickerUiInterfaceInputsPlaceHolders = new Text[3];

        //Core Methods

        public void CreateFullInterfaceNow(GameObject natDataBridge)
        {
            //Create a 3D Text to get font
            GameObject temp3dTextObj = new GameObject("Temp GameObject");
            TextMesh tempText = temp3dTextObj.AddComponent<TextMesh>();
            Font basicTextFont = tempText.font;
            Destroy(temp3dTextObj);

            //Create the Canvas GameObject
            GameObject canvasRoot = new GameObject("Emulated Android Interface");
            canvasRoot.transform.position = Vector3.zero;
            canvasRoot.transform.eulerAngles = Vector3.zero;
            canvasRoot.transform.localScale = Vector3.one;
            canvasRoot.transform.SetParent(natDataBridge.transform);
            canvasRoot.AddComponent<RectTransform>();
            canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 100;
            canvasScaler = canvasRoot.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0;
            canvasScaler.referencePixelsPerUnit = 100;
            canvasGraphicRaycaster = canvasRoot.AddComponent<GraphicRaycaster>();
            canvasGraphicRaycaster.ignoreReversedGraphics = true;

            //Create dialogs interface
            GameObject dialogsInterfaceRoot = new GameObject("Dialogs Interface");
            dialogsInterfaceRoot.transform.position = Vector3.zero;
            dialogsInterfaceRoot.transform.eulerAngles = Vector3.zero;
            dialogsInterfaceRoot.transform.localScale = Vector3.one;
            dialogsInterfaceRoot.transform.SetParent(canvasRoot.transform);
            dialogsInterfaceRoot.SetActive(false);
            dialogsInterface = dialogsInterfaceRoot.AddComponent<RectTransform>();
            dialogsInterface.pivot = new Vector2(0.5f, 0.5f);
            dialogsInterface.anchorMin = new Vector2(0, 0);
            dialogsInterface.anchorMax = new Vector2(1, 1);
            dialogsInterface.offsetMin = new Vector2(0, 0);
            dialogsInterface.offsetMax = new Vector2(0, 0);
            dialogsInterface.anchoredPosition = new Vector2(0, 0);
            dialogsInterfaceRoot.AddComponent<CanvasRenderer>();
            Image dialogsInterfaceImg = dialogsInterfaceRoot.AddComponent<Image>();
            dialogsInterfaceImg.color = new Color(0, 0, 0, 0.5f);
            dialogsInterfaceCancelationButton = dialogsInterfaceRoot.AddComponent<Button>();
            dialogsInterfaceCancelationButton.interactable = true;
            dialogsInterfaceCancelationButton.transition = Selectable.Transition.ColorTint;
            dialogsInterfaceCancelationButton.targetGraphic = dialogsInterfaceImg;
            GameObject dialogUiInterfaceRoot = new GameObject("Dialog UI");
            dialogUiInterfaceRoot.transform.position = Vector3.zero;
            dialogUiInterfaceRoot.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceRoot.transform.localScale = Vector3.one;
            dialogUiInterfaceRoot.transform.SetParent(dialogsInterfaceRoot.transform);
            RectTransform dialogUiInterface = dialogUiInterfaceRoot.AddComponent<RectTransform>();
            dialogUiInterface.pivot = new Vector2(0.5f, 0.5f);
            dialogUiInterface.anchorMin = new Vector2(0.5f, 0.5f);
            dialogUiInterface.anchorMax = new Vector2(0.5f, 0.5f);
            dialogUiInterface.offsetMin = new Vector2(0, 0);
            dialogUiInterface.offsetMax = new Vector2(550, 350);
            dialogUiInterface.anchoredPosition = new Vector2(0, 0);
            dialogUiInterfaceRoot.AddComponent<CanvasRenderer>();
            Image dialogUiInterfaceImg = dialogUiInterfaceRoot.AddComponent<Image>();
            GameObject dialogUiInterfaceTitleRoot = new GameObject("Title");
            dialogUiInterfaceTitleRoot.transform.position = Vector3.zero;
            dialogUiInterfaceTitleRoot.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceTitleRoot.transform.localScale = Vector3.one;
            dialogUiInterfaceTitleRoot.transform.SetParent(dialogUiInterfaceRoot.transform);
            RectTransform dialogUiInterfaceTitle = dialogUiInterfaceTitleRoot.AddComponent<RectTransform>();
            dialogUiInterfaceTitle.pivot = new Vector2(0.5f, 1);
            dialogUiInterfaceTitle.anchorMin = new Vector2(0, 1);
            dialogUiInterfaceTitle.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceTitle.offsetMin = new Vector2(16, 0);
            dialogUiInterfaceTitle.offsetMax = new Vector2(0, 50);
            dialogUiInterfaceTitle.anchoredPosition = new Vector2(0, -6);
            dialogUiInterfaceTitleRoot.AddComponent<CanvasRenderer>();
            dialogUiInterfaceTitleText = dialogUiInterfaceTitleRoot.AddComponent<Text>();
            dialogUiInterfaceTitleText.text = "Title";
            dialogUiInterfaceTitleText.font = basicTextFont;
            dialogUiInterfaceTitleText.fontStyle = FontStyle.Bold;
            dialogUiInterfaceTitleText.fontSize = 22;
            dialogUiInterfaceTitleText.alignment = TextAnchor.MiddleLeft;
            dialogUiInterfaceTitleText.color = Color.black;
            GameObject dialogUiInterfaceButtonsGroupRoot = new GameObject("Buttons Root");
            dialogUiInterfaceButtonsGroupRoot.transform.position = Vector3.zero;
            dialogUiInterfaceButtonsGroupRoot.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceButtonsGroupRoot.transform.localScale = Vector3.one;
            dialogUiInterfaceButtonsGroupRoot.transform.SetParent(dialogUiInterfaceRoot.transform);
            RectTransform dialogUiInterfaceButtonsGroup = dialogUiInterfaceButtonsGroupRoot.AddComponent<RectTransform>();
            dialogUiInterfaceButtonsGroup.pivot = new Vector2(0.5f, 0);
            dialogUiInterfaceButtonsGroup.anchorMin = new Vector2(0, 0);
            dialogUiInterfaceButtonsGroup.anchorMax = new Vector2(1, 0);
            dialogUiInterfaceButtonsGroup.offsetMin = new Vector2(16, 0);
            dialogUiInterfaceButtonsGroup.offsetMax = new Vector2(0, 30);
            dialogUiInterfaceButtonsGroup.anchoredPosition = new Vector2(0, 8);
            HorizontalLayoutGroup dialogUiInterfaceButtonsGroupLayout = dialogUiInterfaceButtonsGroupRoot.AddComponent<HorizontalLayoutGroup>();
            dialogUiInterfaceButtonsGroupLayout.spacing = 10;
            dialogUiInterfaceButtonsGroupLayout.childAlignment = TextAnchor.MiddleCenter;
            dialogUiInterfaceButtonsGroupLayout.childForceExpandHeight = true;
            dialogUiInterfaceButtonsGroupLayout.childForceExpandWidth = true;
            for (int i = 0; i < 3; i++)
            {
                GameObject dialogUiInterfaceButtonRoot = new GameObject("Button " + i.ToString());
                dialogUiInterfaceButtonRoot.transform.position = Vector3.zero;
                dialogUiInterfaceButtonRoot.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceButtonRoot.transform.localScale = Vector3.one;
                dialogUiInterfaceButtonRoot.transform.SetParent(dialogUiInterfaceButtonsGroupRoot.transform);
                RectTransform dialogUiInterfaceButton = dialogUiInterfaceButtonRoot.AddComponent<RectTransform>();
                dialogUiInterfaceButton.pivot = new Vector2(0.5f, 0.5f);
                dialogUiInterfaceButton.anchorMin = new Vector2(0, 1);
                dialogUiInterfaceButton.anchorMax = new Vector2(0, 1);
                dialogUiInterfaceButton.offsetMin = new Vector2(0, 0);
                dialogUiInterfaceButton.offsetMax = new Vector2(0, 0);
                dialogUiInterfaceButton.anchoredPosition = new Vector2(0, 0);
                dialogUiInterfaceButtonRoot.AddComponent<CanvasRenderer>();
                Image dialogUiInterfaceButtonImg = dialogUiInterfaceButtonRoot.AddComponent<Image>();
                dialogUiInterfaceButtonImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                dialogUiInterfaceButtonsBt[i] = dialogUiInterfaceButtonRoot.AddComponent<Button>();
                dialogUiInterfaceButtonsBt[i].interactable = true;
                dialogUiInterfaceButtonsBt[i].transition = Selectable.Transition.ColorTint;
                dialogUiInterfaceButtonsBt[i].targetGraphic = dialogUiInterfaceButtonImg;
                GameObject dialogUiInterfaceButtonTextRoot = new GameObject("Text");
                dialogUiInterfaceButtonTextRoot.transform.position = Vector3.zero;
                dialogUiInterfaceButtonTextRoot.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceButtonTextRoot.transform.localScale = Vector3.one;
                dialogUiInterfaceButtonTextRoot.transform.SetParent(dialogUiInterfaceButtonRoot.transform);
                RectTransform buttonRectTransform = dialogUiInterfaceButtonTextRoot.AddComponent<RectTransform>();
                buttonRectTransform.pivot = new Vector2(0.5f, 0.5f);
                buttonRectTransform.anchorMin = new Vector2(0, 0);
                buttonRectTransform.anchorMax = new Vector2(1, 1);
                buttonRectTransform.offsetMin = new Vector2(0, 0);
                buttonRectTransform.offsetMax = new Vector2(0, 0);
                buttonRectTransform.anchoredPosition = new Vector2(0, 0);
                dialogUiInterfaceButtonsText[i] = dialogUiInterfaceButtonTextRoot.AddComponent<Text>();
                dialogUiInterfaceButtonsText[i].text = "Button";
                dialogUiInterfaceButtonsText[i].font = basicTextFont;
                dialogUiInterfaceButtonsText[i].fontStyle = FontStyle.Normal;
                dialogUiInterfaceButtonsText[i].fontSize = 12;
                dialogUiInterfaceButtonsText[i].alignment = TextAnchor.MiddleCenter;
                dialogUiInterfaceButtonsText[i].color = Color.black;
            }
            GameObject dialogUiInterfaceContentTextRoot = new GameObject("Content Text");
            dialogUiInterfaceContentTextRoot.transform.position = Vector3.zero;
            dialogUiInterfaceContentTextRoot.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentTextRoot.transform.localScale = Vector3.one;
            dialogUiInterfaceContentTextRoot.transform.SetParent(dialogUiInterfaceRoot.transform);
            dialogUiInterfaceContentTextRoot.SetActive(false);
            dialogUiInterfaceContentTextGroup = dialogUiInterfaceContentTextRoot.AddComponent<RectTransform>();
            dialogUiInterfaceContentTextGroup.pivot = new Vector2(0.5f, 0.5f);
            dialogUiInterfaceContentTextGroup.anchorMin = new Vector2(0, 0);
            dialogUiInterfaceContentTextGroup.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentTextGroup.offsetMin = new Vector2(16, 120);
            dialogUiInterfaceContentTextGroup.offsetMax = new Vector2(0, 0);
            dialogUiInterfaceContentTextGroup.anchoredPosition = new Vector2(0, 0);
            dialogUiInterfaceContentText = dialogUiInterfaceContentTextRoot.AddComponent<Text>();
            dialogUiInterfaceContentText.text = "Text Content";
            dialogUiInterfaceContentText.font = basicTextFont;
            dialogUiInterfaceContentText.fontStyle = FontStyle.Normal;
            dialogUiInterfaceContentText.fontSize = 12;
            dialogUiInterfaceContentText.alignment = TextAnchor.UpperLeft;
            dialogUiInterfaceContentText.color = Color.black;
            GameObject dialogUiInterfaceContentChecksRoot = new GameObject("Content Checks");
            dialogUiInterfaceContentChecksRoot.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksRoot.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksRoot.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksRoot.transform.SetParent(dialogUiInterfaceRoot.transform);
            dialogUiInterfaceContentChecksRoot.SetActive(false);
            dialogUiInterfaceContentChecksGroup = dialogUiInterfaceContentChecksRoot.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksGroup.pivot = new Vector2(0.5f, 0.5f);
            dialogUiInterfaceContentChecksGroup.anchorMin = new Vector2(0, 0);
            dialogUiInterfaceContentChecksGroup.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentChecksGroup.offsetMin = new Vector2(16, 120);
            dialogUiInterfaceContentChecksGroup.offsetMax = new Vector2(0, 0);
            dialogUiInterfaceContentChecksGroup.anchoredPosition = new Vector2(0, 0);
            dialogUiInterfaceContentChecksRoot.AddComponent<CanvasRenderer>();
            Image scrollviewBackgroundImg = dialogUiInterfaceContentChecksRoot.AddComponent<Image>();
            scrollviewBackgroundImg.color = new Color(1, 1, 1, 0.0f);
            ScrollRect scrollviewRect = dialogUiInterfaceContentChecksRoot.AddComponent<ScrollRect>();
            scrollviewRect.content = null; //<--
            scrollviewRect.horizontal = false;
            scrollviewRect.vertical = true;
            scrollviewRect.movementType = ScrollRect.MovementType.Elastic;
            scrollviewRect.elasticity = 0.1f;
            scrollviewRect.inertia = true;
            scrollviewRect.decelerationRate = 0.135f;
            scrollviewRect.scrollSensitivity = 1;
            scrollviewRect.viewport = null; //<--
            scrollviewRect.verticalScrollbar = null; //<--
            scrollviewRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollviewRect.verticalScrollbarSpacing = -3;
            GameObject dialogUiInterfaceContentChecksScrollviewViewport = new GameObject("Viewport");
            dialogUiInterfaceContentChecksScrollviewViewport.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewViewport.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewViewport.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksScrollviewViewport.transform.SetParent(dialogUiInterfaceContentChecksRoot.transform);
            RectTransform dialogUiInterfaceContentChecksScrollviewViewportRt = dialogUiInterfaceContentChecksScrollviewViewport.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksScrollviewViewport.AddComponent<CanvasRenderer>();
            Image dialogUiInterfaceContentChecksScrollviewViewportImg = dialogUiInterfaceContentChecksScrollviewViewport.AddComponent<Image>();
            Mask dialogUiInterfaceContentChecksScrollviewViewportM = dialogUiInterfaceContentChecksScrollviewViewport.AddComponent<Mask>();
            dialogUiInterfaceContentChecksScrollviewViewportM.showMaskGraphic = false;
            scrollviewRect.viewport = dialogUiInterfaceContentChecksScrollviewViewportRt;
            GameObject dialogUiInterfaceContentChecksScrollviewContent = new GameObject("Content");
            dialogUiInterfaceContentChecksScrollviewContent.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewContent.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewContent.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksScrollviewContent.transform.SetParent(dialogUiInterfaceContentChecksScrollviewViewport.transform);
            RectTransform dialogUiInterfaceContentChecksScrollviewContentRt = dialogUiInterfaceContentChecksScrollviewContent.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksScrollviewContentRt.pivot = new Vector2(0.5f, 1);
            dialogUiInterfaceContentChecksScrollviewContentRt.anchorMin = new Vector2(0, 1);
            dialogUiInterfaceContentChecksScrollviewContentRt.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentChecksScrollviewContentRt.offsetMin = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewContentRt.offsetMax = new Vector2(0, 50);
            dialogUiInterfaceContentChecksScrollviewContentRt.anchoredPosition = new Vector2(0, -6);
            VerticalLayoutGroup dialogUiInterfaceContentChecksScrollviewContentLg = dialogUiInterfaceContentChecksScrollviewContent.AddComponent<VerticalLayoutGroup>();
            dialogUiInterfaceContentChecksScrollviewContentLg.spacing = 4;
            dialogUiInterfaceContentChecksScrollviewContentLg.childAlignment = TextAnchor.UpperCenter;
            dialogUiInterfaceContentChecksScrollviewContentLg.childControlWidth = true;
            dialogUiInterfaceContentChecksScrollviewContentLg.childControlHeight = false;
            dialogUiInterfaceContentChecksScrollviewContentLg.childForceExpandWidth = true;
            dialogUiInterfaceContentChecksScrollviewContentLg.childForceExpandHeight = true;
            ContentSizeFitter dialogUiInterfaceContentChecksScrollviewContentSf = dialogUiInterfaceContentChecksScrollviewContent.AddComponent<ContentSizeFitter>();
            dialogUiInterfaceContentChecksScrollviewContentSf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            dialogUiInterfaceContentChecksScrollviewContentSf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollviewRect.content = dialogUiInterfaceContentChecksScrollviewContentRt;
            GameObject dialogUiInterfaceContentChecksScrollviewScrollbar = new GameObject("Scrollbar");
            dialogUiInterfaceContentChecksScrollviewScrollbar.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbar.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbar.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksScrollviewScrollbar.transform.SetParent(dialogUiInterfaceContentChecksRoot.transform);
            RectTransform dialogUiInterfaceContentChecksScrollviewScrollbarRt = dialogUiInterfaceContentChecksScrollviewScrollbar.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.pivot = new Vector2(1, 0.5f);
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.anchorMin = new Vector2(1, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.offsetMin = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.offsetMax = new Vector2(20, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarRt.anchoredPosition = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbar.AddComponent<CanvasRenderer>();
            Image dialogUiInterfaceContentChecksScrollviewScrollbarBg = dialogUiInterfaceContentChecksScrollviewScrollbar.AddComponent<Image>();
            dialogUiInterfaceContentChecksScrollviewScrollbarBg.color = new Color(0.7f, 0.7f, 0.7f, 1.0f);
            Scrollbar dialogUiInterfaceContentChecksScrollviewScrollbarSb = dialogUiInterfaceContentChecksScrollviewScrollbar.AddComponent<Scrollbar>();
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.interactable = true;
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.transition = Selectable.Transition.ColorTint;
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.handleRect = null; //<--
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.direction = Scrollbar.Direction.BottomToTop;
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.value = 1.0f;
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.size = 0.66f;
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.numberOfSteps = 0;
            scrollviewRect.verticalScrollbar = dialogUiInterfaceContentChecksScrollviewScrollbarSb;
            GameObject dialogUiInterfaceContentChecksScrollviewScrollbarSa = new GameObject("Sliding Area");
            dialogUiInterfaceContentChecksScrollviewScrollbarSa.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbarSa.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbarSa.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksScrollviewScrollbarSa.transform.SetParent(dialogUiInterfaceContentChecksScrollviewScrollbar.transform);
            RectTransform dialogUiInterfaceContentChecksScrollviewScrollbarSaRt = dialogUiInterfaceContentChecksScrollviewScrollbarSa.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.pivot = new Vector2(0.5f, 0.5f);
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.anchorMin = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.offsetMin = new Vector2(20, 20);
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.offsetMax = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarSaRt.anchoredPosition = new Vector2(0, 0);
            GameObject dialogUiInterfaceContentChecksScrollviewScrollbarSh = new GameObject("Sliding Handle");
            dialogUiInterfaceContentChecksScrollviewScrollbarSh.transform.position = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbarSh.transform.eulerAngles = Vector3.zero;
            dialogUiInterfaceContentChecksScrollviewScrollbarSh.transform.localScale = Vector3.one;
            dialogUiInterfaceContentChecksScrollviewScrollbarSh.transform.SetParent(dialogUiInterfaceContentChecksScrollviewScrollbarSa.transform);
            RectTransform dialogUiInterfaceContentChecksScrollviewScrollbarShRt = dialogUiInterfaceContentChecksScrollviewScrollbarSh.AddComponent<RectTransform>();
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.pivot = new Vector2(0.5f, 0.5f);
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.anchorMin = new Vector2(0, 0.3333333f);
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.anchorMax = new Vector2(1, 1);
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.offsetMin = new Vector2(-20, -20);
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.offsetMax = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarShRt.anchoredPosition = new Vector2(0, 0);
            dialogUiInterfaceContentChecksScrollviewScrollbarSh.AddComponent<CanvasRenderer>();
            Image dialogUiInterfaceContentChecksScrollviewScrollbarShFg = dialogUiInterfaceContentChecksScrollviewScrollbarSh.AddComponent<Image>();
            dialogUiInterfaceContentChecksScrollviewScrollbarShFg.color = new Color(0, 0, 0, 1.0f);
            dialogUiInterfaceContentChecksScrollviewScrollbarSb.handleRect = dialogUiInterfaceContentChecksScrollviewScrollbarShRt;
            for (int i = 0; i < 100; i++)
            {
                GameObject dialogUiInterfaceToggleRoot = new GameObject("Toggle " + i.ToString());
                dialogUiInterfaceToggleRoot.transform.position = Vector3.zero;
                dialogUiInterfaceToggleRoot.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceToggleRoot.transform.localScale = Vector3.one;
                dialogUiInterfaceToggleRoot.transform.SetParent(dialogUiInterfaceContentChecksScrollviewContent.transform);
                RectTransform dialogUiInterfaceToggleRt = dialogUiInterfaceToggleRoot.AddComponent<RectTransform>();
                dialogUiInterfaceToggleRt.pivot = new Vector2(0.5f, 0.5f);
                dialogUiInterfaceToggleRt.anchorMin = new Vector2(0, 0);
                dialogUiInterfaceToggleRt.anchorMax = new Vector2(0, 0);
                dialogUiInterfaceToggleRt.offsetMin = new Vector2(0, 0);
                dialogUiInterfaceToggleRt.offsetMax = new Vector2(0, 30);
                dialogUiInterfaceToggleRt.anchoredPosition = new Vector2(0, 0);
                dialogUiInterfaceToggles[i] = dialogUiInterfaceToggleRoot.AddComponent<Toggle>();
                dialogUiInterfaceToggles[i].interactable = true;
                dialogUiInterfaceToggles[i].transition = Selectable.Transition.ColorTint;
                dialogUiInterfaceToggles[i].isOn = false;
                dialogUiInterfaceToggles[i].toggleTransition = Toggle.ToggleTransition.Fade;
                dialogUiInterfaceToggles[i].graphic = null; //<--
                dialogUiInterfaceToggles[i].group = null;
                GameObject dialogUiInterfaceToggleBackground = new GameObject("Background");
                dialogUiInterfaceToggleBackground.transform.position = Vector3.zero;
                dialogUiInterfaceToggleBackground.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceToggleBackground.transform.localScale = Vector3.one;
                dialogUiInterfaceToggleBackground.transform.SetParent(dialogUiInterfaceToggleRoot.transform);
                RectTransform dialogUiInterfaceToggleBackgroundRt = dialogUiInterfaceToggleBackground.AddComponent<RectTransform>();
                dialogUiInterfaceToggleBackgroundRt.pivot = new Vector2(0.5f, 0.5f);
                dialogUiInterfaceToggleBackgroundRt.anchorMin = new Vector2(0, 1);
                dialogUiInterfaceToggleBackgroundRt.anchorMax = new Vector2(0, 1);
                dialogUiInterfaceToggleBackgroundRt.offsetMin = new Vector2(0, 0);
                dialogUiInterfaceToggleBackgroundRt.offsetMax = new Vector2(20, 20);
                dialogUiInterfaceToggleBackgroundRt.anchoredPosition = new Vector2(10, -10);
                dialogUiInterfaceToggleBackground.AddComponent<CanvasRenderer>();
                Image dialogUiInterfaceToggleBackgroundImg = dialogUiInterfaceToggleBackground.AddComponent<Image>();
                dialogUiInterfaceToggleBackgroundImg.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                GameObject dialogUiInterfaceToggleCheckmark = new GameObject("Checkmark");
                dialogUiInterfaceToggleCheckmark.transform.position = Vector3.zero;
                dialogUiInterfaceToggleCheckmark.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceToggleCheckmark.transform.localScale = Vector3.one;
                dialogUiInterfaceToggleCheckmark.transform.SetParent(dialogUiInterfaceToggleBackground.transform);
                RectTransform dialogUiInterfaceToggleCheckmarkRt = dialogUiInterfaceToggleCheckmark.AddComponent<RectTransform>();
                dialogUiInterfaceToggleCheckmarkRt.pivot = new Vector2(0.5f, 0.5f);
                dialogUiInterfaceToggleCheckmarkRt.anchorMin = new Vector2(0, 1);
                dialogUiInterfaceToggleCheckmarkRt.anchorMax = new Vector2(0, 1);
                dialogUiInterfaceToggleCheckmarkRt.offsetMin = new Vector2(0, 0);
                dialogUiInterfaceToggleCheckmarkRt.offsetMax = new Vector2(20, 20);
                dialogUiInterfaceToggleCheckmarkRt.anchoredPosition = new Vector2(10, -10);
                dialogUiInterfaceToggleCheckmark.AddComponent<CanvasRenderer>();
                Image dialogUiInterfaceToggleCheckmarkImg = dialogUiInterfaceToggleCheckmark.AddComponent<Image>();
                dialogUiInterfaceToggleCheckmarkImg.color = new Color(0, 0, 0, 1.0f);
                dialogUiInterfaceToggles[i].graphic = dialogUiInterfaceToggleCheckmarkImg;
                GameObject dialogUiInterfaceToggleLabel = new GameObject("Label");
                dialogUiInterfaceToggleLabel.transform.position = Vector3.zero;
                dialogUiInterfaceToggleLabel.transform.eulerAngles = Vector3.zero;
                dialogUiInterfaceToggleLabel.transform.localScale = Vector3.one;
                dialogUiInterfaceToggleLabel.transform.SetParent(dialogUiInterfaceToggleRoot.transform);
                RectTransform dialogUiInterfaceToggleLabelRt = dialogUiInterfaceToggleLabel.AddComponent<RectTransform>();
                dialogUiInterfaceToggleLabelRt.pivot = new Vector2(0.5f, 1);
                dialogUiInterfaceToggleLabelRt.anchorMin = new Vector2(0, 1);
                dialogUiInterfaceToggleLabelRt.anchorMax = new Vector2(1, 1);
                dialogUiInterfaceToggleLabelRt.offsetMin = new Vector2(0, 0);
                dialogUiInterfaceToggleLabelRt.offsetMax = new Vector2(0, 17);
                dialogUiInterfaceToggleLabelRt.anchoredPosition = new Vector2(25, -1);
                dialogUiInterfaceToggleLabel.AddComponent<CanvasRenderer>();
                dialogUiInterfaceToggleTexts[i] = dialogUiInterfaceToggleLabel.AddComponent<Text>();
                dialogUiInterfaceToggleTexts[i].text = "Item " + i.ToString();
                dialogUiInterfaceToggleTexts[i].font = basicTextFont;
                dialogUiInterfaceToggleTexts[i].fontStyle = FontStyle.Normal;
                dialogUiInterfaceToggleTexts[i].fontSize = 14;
                dialogUiInterfaceToggleTexts[i].alignment = TextAnchor.MiddleLeft;
                dialogUiInterfaceToggleTexts[i].color = Color.black;
            }

            //Create notifications interface
            GameObject notificationsInterfaceRoot = new GameObject("Notifications Interface");
            notificationsInterfaceRoot.transform.position = Vector3.zero;
            notificationsInterfaceRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfaceRoot.transform.localScale = Vector3.one;
            notificationsInterfaceRoot.transform.SetParent(canvasRoot.transform);
            notificationsInterface = notificationsInterfaceRoot.AddComponent<RectTransform>();
            notificationsInterface.pivot = new Vector2(0.5f, 0.5f);
            notificationsInterface.anchorMin = new Vector2(0, 0);
            notificationsInterface.anchorMax = new Vector2(1, 1);
            notificationsInterface.offsetMin = new Vector2(0, 0);
            notificationsInterface.offsetMax = new Vector2(0, 0);
            notificationsInterface.anchoredPosition = new Vector2(0, 0);
            GameObject notificationsInterfaceToastRoot = new GameObject("Toast UI");
            notificationsInterfaceToastRoot.transform.position = Vector3.zero;
            notificationsInterfaceToastRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfaceToastRoot.transform.localScale = Vector3.one;
            notificationsInterfaceToastRoot.transform.SetParent(notificationsInterfaceRoot.transform);
            notificationsInterfaceToastRoot.SetActive(false);
            notificationsToastGroup = notificationsInterfaceToastRoot.AddComponent<RectTransform>();
            notificationsToastGroup.pivot = new Vector2(0.5f, 0.5f);
            notificationsToastGroup.anchorMin = new Vector2(0, 0);
            notificationsToastGroup.anchorMax = new Vector2(1, 1);
            notificationsToastGroup.offsetMin = new Vector2(0, 0);
            notificationsToastGroup.offsetMax = new Vector2(0, 0);
            notificationsToastGroup.anchoredPosition = new Vector2(0, 0);
            notificationsInterfaceToastRoot.AddComponent<CanvasRenderer>();
            Image notificationsInterfaceToastImg = notificationsInterfaceToastRoot.AddComponent<Image>();
            notificationsInterfaceToastImg.color = new Color(0, 0, 0, 0.8f);
            notificationsInterfaceToastImg.raycastTarget = false;
            GameObject notificationsInterfaceToastTextRoot = new GameObject("Content");
            notificationsInterfaceToastTextRoot.transform.position = Vector3.zero;
            notificationsInterfaceToastTextRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfaceToastTextRoot.transform.localScale = Vector3.one;
            notificationsInterfaceToastTextRoot.transform.SetParent(notificationsInterfaceToastRoot.transform);
            RectTransform notificationsInterfaceToastTextRt = notificationsInterfaceToastTextRoot.AddComponent<RectTransform>();
            notificationsInterfaceToastTextRt.pivot = new Vector2(0.5f, 0.5f);
            notificationsInterfaceToastTextRt.anchorMin = new Vector2(0, 0);
            notificationsInterfaceToastTextRt.anchorMax = new Vector2(1, 1);
            notificationsInterfaceToastTextRt.offsetMin = new Vector2(0, 0);
            notificationsInterfaceToastTextRt.offsetMax = new Vector2(0, 0);
            notificationsInterfaceToastTextRt.anchoredPosition = new Vector2(0, 0);
            notificationsInterfaceToastTextRoot.AddComponent<CanvasRenderer>();
            notificationsToastText = notificationsInterfaceToastTextRoot.AddComponent<Text>();
            notificationsToastText.text = "Toast Content";
            notificationsToastText.font = basicTextFont;
            notificationsToastText.fontStyle = FontStyle.Bold;
            notificationsToastText.fontSize = 14;
            notificationsToastText.alignment = TextAnchor.MiddleCenter;
            notificationsToastText.color = Color.white;
            notificationsToastText.resizeTextForBestFit = true;
            notificationsToastText.resizeTextMinSize = 1;
            notificationsToastText.resizeTextMaxSize = 22;
            notificationsToastText.raycastTarget = false;
            GameObject notificationsInterfacePushRoot = new GameObject("Push UI");
            notificationsInterfacePushRoot.transform.position = Vector3.zero;
            notificationsInterfacePushRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushRoot.transform.SetParent(notificationsInterfaceRoot.transform);
            notificationsInterfacePushRoot.SetActive(false);
            notificationsPushGroup = notificationsInterfacePushRoot.AddComponent<RectTransform>();
            notificationsPushGroup.pivot = new Vector2(0.5f, 1);
            notificationsPushGroup.anchorMin = new Vector2(0, 1);
            notificationsPushGroup.anchorMax = new Vector2(1, 1);
            notificationsPushGroup.offsetMin = new Vector2(0, 0);
            notificationsPushGroup.offsetMax = new Vector2(-500, 120);
            notificationsPushGroup.anchoredPosition = new Vector2(0, 0);
            notificationsInterfacePushRoot.AddComponent<CanvasRenderer>();
            Image notificationsInterfacePushImg = notificationsInterfacePushRoot.AddComponent<Image>();
            notificationsInterfacePushImg.color = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            notificationsInterfacePushImg.raycastTarget = true;
            notificationsPushButtons[0] = notificationsInterfacePushRoot.AddComponent<Button>();
            notificationsPushButtons[0].interactable = true;
            notificationsPushButtons[0].transition = Selectable.Transition.ColorTint;
            notificationsPushButtons[0].targetGraphic = notificationsInterfacePushImg;
            GameObject notificationsInterfacePushTimeRoot = new GameObject("Time");
            notificationsInterfacePushTimeRoot.transform.position = Vector3.zero;
            notificationsInterfacePushTimeRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushTimeRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushTimeRoot.transform.SetParent(notificationsInterfacePushRoot.transform);
            RectTransform notificationsPushTimeRt = notificationsInterfacePushTimeRoot.AddComponent<RectTransform>();
            notificationsPushTimeRt.pivot = new Vector2(0.5f, 0);
            notificationsPushTimeRt.anchorMin = new Vector2(0, 0);
            notificationsPushTimeRt.anchorMax = new Vector2(1, 0);
            notificationsPushTimeRt.offsetMin = new Vector2(0, 0);
            notificationsPushTimeRt.offsetMax = new Vector2(0, 4);
            notificationsPushTimeRt.anchoredPosition = new Vector2(0, -1);
            notificationsPushTime = notificationsInterfacePushTimeRoot.AddComponent<Slider>();
            notificationsPushTime.interactable = false;
            notificationsPushTime.transition = Selectable.Transition.None;
            notificationsPushTime.direction = Slider.Direction.LeftToRight;
            notificationsPushTime.minValue = 0;
            notificationsPushTime.maxValue = 1;
            notificationsPushTime.wholeNumbers = false;
            notificationsPushTime.value = 0;
            GameObject notificationsInterfacePushTimeSliderFaRoot = new GameObject("Fill Area");
            notificationsInterfacePushTimeSliderFaRoot.transform.position = Vector3.zero;
            notificationsInterfacePushTimeSliderFaRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushTimeSliderFaRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushTimeSliderFaRoot.transform.SetParent(notificationsInterfacePushTimeRoot.transform);
            RectTransform notificationsInterfacePushTimeSliderFaRt = notificationsInterfacePushTimeSliderFaRoot.AddComponent<RectTransform>();
            notificationsInterfacePushTimeSliderFaRt.pivot = new Vector2(0.5f, 0.5f);
            notificationsInterfacePushTimeSliderFaRt.anchorMin = new Vector2(0, 0.25f);
            notificationsInterfacePushTimeSliderFaRt.anchorMax = new Vector2(1, 0.75f);
            notificationsInterfacePushTimeSliderFaRt.offsetMin = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFaRt.offsetMax = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFaRt.anchoredPosition = new Vector2(0, 0);
            GameObject notificationsInterfacePushTimeSliderFRoot = new GameObject("Fill");
            notificationsInterfacePushTimeSliderFRoot.transform.position = Vector3.zero;
            notificationsInterfacePushTimeSliderFRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushTimeSliderFRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushTimeSliderFRoot.transform.SetParent(notificationsInterfacePushTimeSliderFaRoot.transform);
            RectTransform notificationsInterfacePushTimeSliderFRt = notificationsInterfacePushTimeSliderFRoot.AddComponent<RectTransform>();
            notificationsInterfacePushTimeSliderFRt.pivot = new Vector2(0.5f, 0.5f);
            notificationsInterfacePushTimeSliderFRt.anchorMin = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFRt.anchorMax = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFRt.offsetMin = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFRt.offsetMax = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFRt.anchoredPosition = new Vector2(0, 0);
            notificationsInterfacePushTimeSliderFRoot.AddComponent<CanvasRenderer>();
            Image notificationsInterfacePushTimeSliderFImg = notificationsInterfacePushTimeSliderFRoot.AddComponent<Image>();
            notificationsInterfacePushTimeSliderFImg.color = new Color(0, 0, 0, 1.0f);
            notificationsInterfacePushTimeSliderFImg.raycastTarget = false;
            notificationsPushTime.fillRect = notificationsInterfacePushTimeSliderFRt;
            GameObject notificationsInterfacePushIconRoot = new GameObject("Icon");
            notificationsInterfacePushIconRoot.transform.position = Vector3.zero;
            notificationsInterfacePushIconRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushIconRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushIconRoot.transform.SetParent(notificationsInterfacePushRoot.transform);
            RectTransform notificationsPushIconRt = notificationsInterfacePushIconRoot.AddComponent<RectTransform>();
            notificationsPushIconRt.pivot = new Vector2(0, 0.5f);
            notificationsPushIconRt.anchorMin = new Vector2(0, 0.5f);
            notificationsPushIconRt.anchorMax = new Vector2(0, 0.5f);
            notificationsPushIconRt.offsetMin = new Vector2(0, 0);
            notificationsPushIconRt.offsetMax = new Vector2(100, 100);
            notificationsPushIconRt.anchoredPosition = new Vector2(10, 0);
            notificationsInterfacePushIconRoot.AddComponent<CanvasRenderer>();
            Image notificationsPushIconImg = notificationsInterfacePushIconRoot.AddComponent<Image>();
            notificationsPushIconImg.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            notificationsPushIconImg.raycastTarget = false;
            GameObject notificationsInterfacePushTitleRoot = new GameObject("Title");
            notificationsInterfacePushTitleRoot.transform.position = Vector3.zero;
            notificationsInterfacePushTitleRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushTitleRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushTitleRoot.transform.SetParent(notificationsInterfacePushRoot.transform);
            RectTransform notificationsPushTitleRt = notificationsInterfacePushTitleRoot.AddComponent<RectTransform>();
            notificationsPushTitleRt.pivot = new Vector2(0.5f, 1);
            notificationsPushTitleRt.anchorMin = new Vector2(0, 1);
            notificationsPushTitleRt.anchorMax = new Vector2(1, 1);
            notificationsPushTitleRt.offsetMin = new Vector2(0, 0);
            notificationsPushTitleRt.offsetMax = new Vector2(0, 30);
            notificationsPushTitleRt.anchoredPosition = new Vector2(120, -10);
            notificationsInterfacePushTitleRoot.AddComponent<CanvasRenderer>();
            notificationsPushTitle = notificationsInterfacePushTitleRoot.AddComponent<Text>();
            notificationsPushTitle.text = "Notification Title";
            notificationsPushTitle.font = basicTextFont;
            notificationsPushTitle.fontStyle = FontStyle.Bold;
            notificationsPushTitle.fontSize = 14;
            notificationsPushTitle.alignment = TextAnchor.MiddleLeft;
            notificationsPushTitle.color = Color.black;
            notificationsPushTitle.raycastTarget = false;
            GameObject notificationsInterfacePushTextRoot = new GameObject("Text");
            notificationsInterfacePushTextRoot.transform.position = Vector3.zero;
            notificationsInterfacePushTextRoot.transform.eulerAngles = Vector3.zero;
            notificationsInterfacePushTextRoot.transform.localScale = Vector3.one;
            notificationsInterfacePushTextRoot.transform.SetParent(notificationsInterfacePushRoot.transform);
            RectTransform notificationsPushTextRt = notificationsInterfacePushTextRoot.AddComponent<RectTransform>();
            notificationsPushTextRt.pivot = new Vector2(0.5f, 0);
            notificationsPushTextRt.anchorMin = new Vector2(0, 0);
            notificationsPushTextRt.anchorMax = new Vector2(1, 0);
            notificationsPushTextRt.offsetMin = new Vector2(120, 0);
            notificationsPushTextRt.offsetMax = new Vector2(0, 60);
            notificationsPushTextRt.anchoredPosition = new Vector2(60, 10);
            notificationsInterfacePushTextRoot.AddComponent<CanvasRenderer>();
            notificationsPushText = notificationsInterfacePushTextRoot.AddComponent<Text>();
            notificationsPushText.text = "Notification Text";
            notificationsPushText.font = basicTextFont;
            notificationsPushText.fontStyle = FontStyle.Normal;
            notificationsPushText.fontSize = 14;
            notificationsPushText.alignment = TextAnchor.UpperLeft;
            notificationsPushText.color = Color.black;
            notificationsPushText.resizeTextForBestFit = true;
            notificationsPushText.resizeTextMinSize = 1;
            notificationsPushText.resizeTextMaxSize = 14;
            notificationsPushText.raycastTarget = false;
            for (int i = 1; i < 3; i++)
            {
                GameObject notificationButtonRoot = new GameObject("Action " + i.ToString());
                notificationButtonRoot.transform.position = Vector3.zero;
                notificationButtonRoot.transform.eulerAngles = Vector3.zero;
                notificationButtonRoot.transform.localScale = Vector3.one;
                notificationButtonRoot.transform.SetParent(notificationsInterfacePushRoot.transform);
                RectTransform notificationButton = notificationButtonRoot.AddComponent<RectTransform>();
                notificationButton.pivot = new Vector2(0.5f, 0.5f);
                notificationButton.anchorMin = new Vector2(0.5f, 0.5f);
                notificationButton.anchorMax = new Vector2(0.5f, 0.5f);
                notificationButton.offsetMin = new Vector2(0, 0);
                notificationButton.offsetMax = new Vector2(160, 30);
                if (i == 1)
                    notificationButton.anchoredPosition = new Vector2(-310, -76);
                if (i == 2)
                    notificationButton.anchoredPosition = new Vector2(-146, -76);
                notificationButtonRoot.AddComponent<CanvasRenderer>();
                Image notificationButtonImg = notificationButtonRoot.AddComponent<Image>();
                notificationButtonImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                notificationsPushButtons[i] = notificationButtonRoot.AddComponent<Button>();
                notificationsPushButtons[i].interactable = true;
                notificationsPushButtons[i].transition = Selectable.Transition.ColorTint;
                notificationsPushButtons[i].targetGraphic = notificationButtonImg;
                GameObject notificationButtonTextRoot = new GameObject("Text");
                notificationButtonTextRoot.transform.position = Vector3.zero;
                notificationButtonTextRoot.transform.eulerAngles = Vector3.zero;
                notificationButtonTextRoot.transform.localScale = Vector3.one;
                notificationButtonTextRoot.transform.SetParent(notificationButtonRoot.transform);
                RectTransform notificationbuttonRectTransform = notificationButtonTextRoot.AddComponent<RectTransform>();
                notificationbuttonRectTransform.pivot = new Vector2(0.5f, 0.5f);
                notificationbuttonRectTransform.anchorMin = new Vector2(0, 0);
                notificationbuttonRectTransform.anchorMax = new Vector2(1, 1);
                notificationbuttonRectTransform.offsetMin = new Vector2(0, 0);
                notificationbuttonRectTransform.offsetMax = new Vector2(0, 0);
                notificationbuttonRectTransform.anchoredPosition = new Vector2(0, 0);
                notificationsButtonsText[i] = notificationButtonTextRoot.AddComponent<Text>();
                notificationsButtonsText[i].text = "Button";
                notificationsButtonsText[i].font = basicTextFont;
                notificationsButtonsText[i].fontStyle = FontStyle.Normal;
                notificationsButtonsText[i].fontSize = 12;
                notificationsButtonsText[i].alignment = TextAnchor.MiddleCenter;
                notificationsButtonsText[i].color = Color.black;
            }

            //Create the webview interface
            GameObject webviewInterfaceRoot = new GameObject("Webview Interface");
            webviewInterfaceRoot.transform.position = Vector3.zero;
            webviewInterfaceRoot.transform.eulerAngles = Vector3.zero;
            webviewInterfaceRoot.transform.localScale = Vector3.one;
            webviewInterfaceRoot.transform.SetParent(canvasRoot.transform);
            webviewInterface = webviewInterfaceRoot.AddComponent<RectTransform>();
            webviewInterface.pivot = new Vector2(0.5f, 0.5f);
            webviewInterface.anchorMin = new Vector2(0, 0);
            webviewInterface.anchorMax = new Vector2(1, 1);
            webviewInterface.offsetMin = new Vector2(0, 0);
            webviewInterface.offsetMax = new Vector2(0, 0);
            webviewInterface.anchoredPosition = new Vector2(0, 0);
            GameObject webviewExibitionRoot = new GameObject("Webview");
            webviewExibitionRoot.transform.position = Vector3.zero;
            webviewExibitionRoot.transform.eulerAngles = Vector3.zero;
            webviewExibitionRoot.transform.localScale = Vector3.one;
            webviewExibitionRoot.transform.SetParent(webviewInterfaceRoot.transform);
            webviewExibition = webviewExibitionRoot.AddComponent<RectTransform>();
            webviewExibition.pivot = new Vector2(0.5f, 0.5f);
            webviewExibition.anchorMin = new Vector2(0, 0);
            webviewExibition.anchorMax = new Vector2(1, 1);
            webviewExibition.offsetMin = new Vector2(0, 0);
            webviewExibition.offsetMax = new Vector2(0, 0);
            webviewExibition.anchoredPosition = new Vector2(0, 0);
            webviewExibitionRoot.AddComponent<CanvasRenderer>();
            Image webviewExibitionImg = webviewExibitionRoot.AddComponent<Image>();
            webviewExibitionImg.color = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            Outline webviewExibitionOutline = webviewExibitionRoot.AddComponent<Outline>();
            webviewExibitionOutline.effectDistance = new Vector2(4, 4);
            webviewExibition.gameObject.SetActive(false);
            GameObject webviewExibitionTextRoot = new GameObject("Text");
            webviewExibitionTextRoot.transform.position = Vector3.zero;
            webviewExibitionTextRoot.transform.eulerAngles = Vector3.zero;
            webviewExibitionTextRoot.transform.localScale = Vector3.one;
            webviewExibitionTextRoot.transform.SetParent(webviewExibitionRoot.transform);
            RectTransform webviewExibitionTextRt = webviewExibitionTextRoot.AddComponent<RectTransform>();
            webviewExibitionTextRt.pivot = new Vector2(0.5f, 0.5f);
            webviewExibitionTextRt.anchorMin = new Vector2(0, 0);
            webviewExibitionTextRt.anchorMax = new Vector2(1, 1);
            webviewExibitionTextRt.offsetMin = new Vector2(0, 0);
            webviewExibitionTextRt.offsetMax = new Vector2(0, 0);
            webviewExibitionTextRt.anchoredPosition = new Vector2(0, 0);
            webviewExibitionTextRoot.AddComponent<CanvasRenderer>();
            Text webviewExibitionText = webviewExibitionTextRoot.AddComponent<Text>();
            webviewExibitionText.text = "Showing Webview";
            webviewExibitionText.font = basicTextFont;
            webviewExibitionText.fontStyle = FontStyle.Normal;
            webviewExibitionText.fontSize = 20;
            webviewExibitionText.alignment = TextAnchor.MiddleCenter;
            webviewExibitionText.color = Color.black;
            webviewExibitionText.raycastTarget = false;
            GameObject webviewExibitionButtonRoot = new GameObject("Close");
            webviewExibitionButtonRoot.transform.position = Vector3.zero;
            webviewExibitionButtonRoot.transform.eulerAngles = Vector3.zero;
            webviewExibitionButtonRoot.transform.localScale = Vector3.one;
            webviewExibitionButtonRoot.transform.SetParent(webviewExibitionRoot.transform);
            RectTransform webviewExibitionButton = webviewExibitionButtonRoot.AddComponent<RectTransform>();
            webviewExibitionButton.pivot = new Vector2(0.5f, 0);
            webviewExibitionButton.anchorMin = new Vector2(0.5f, 0);
            webviewExibitionButton.anchorMax = new Vector2(0.5f, 0);
            webviewExibitionButton.offsetMin = new Vector2(-160, 0);
            webviewExibitionButton.offsetMax = new Vector2(0, 40);
            webviewExibitionButton.anchoredPosition = new Vector2(0, 48);
            webviewExibitionButtonRoot.AddComponent<CanvasRenderer>();
            Image webviewExibitionButtonImg = webviewExibitionButtonRoot.AddComponent<Image>();
            webviewExibitionButtonImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            webviewExibitionCloseButton = webviewExibitionButtonRoot.AddComponent<Button>();
            webviewExibitionCloseButton.interactable = true;
            webviewExibitionCloseButton.transition = Selectable.Transition.ColorTint;
            webviewExibitionCloseButton.targetGraphic = webviewExibitionButtonImg;
            GameObject webviewExibitionButtonTextRoot = new GameObject("Text");
            webviewExibitionButtonTextRoot.transform.position = Vector3.zero;
            webviewExibitionButtonTextRoot.transform.eulerAngles = Vector3.zero;
            webviewExibitionButtonTextRoot.transform.localScale = Vector3.one;
            webviewExibitionButtonTextRoot.transform.SetParent(webviewExibitionButtonRoot.transform);
            RectTransform webviewExibitionButtonRt = webviewExibitionButtonTextRoot.AddComponent<RectTransform>();
            webviewExibitionButtonRt.pivot = new Vector2(0.5f, 0.5f);
            webviewExibitionButtonRt.anchorMin = new Vector2(0, 0);
            webviewExibitionButtonRt.anchorMax = new Vector2(1, 1);
            webviewExibitionButtonRt.offsetMin = new Vector2(0, 0);
            webviewExibitionButtonRt.offsetMax = new Vector2(0, 0);
            webviewExibitionButtonRt.anchoredPosition = new Vector2(0, 0);
            Text webviewExibitionButtonText = webviewExibitionButtonTextRoot.AddComponent<Text>();
            webviewExibitionButtonText.text = "Close";
            webviewExibitionButtonText.font = basicTextFont;
            webviewExibitionButtonText.fontStyle = FontStyle.Normal;
            webviewExibitionButtonText.fontSize = 12;
            webviewExibitionButtonText.alignment = TextAnchor.MiddleCenter;
            webviewExibitionButtonText.color = Color.black;

            //Create the permissions requester wizard interface
            GameObject permissionRequesterInterfaceRoot = new GameObject("Activity Interface");
            permissionRequesterInterfaceRoot.transform.position = Vector3.zero;
            permissionRequesterInterfaceRoot.transform.eulerAngles = Vector3.zero;
            permissionRequesterInterfaceRoot.transform.localScale = Vector3.one;
            permissionRequesterInterfaceRoot.transform.SetParent(canvasRoot.transform);
            simulatedActivityInterface = permissionRequesterInterfaceRoot.AddComponent<RectTransform>();
            simulatedActivityInterface.pivot = new Vector2(0.5f, 0.5f);
            simulatedActivityInterface.anchorMin = new Vector2(0, 0);
            simulatedActivityInterface.anchorMax = new Vector2(1, 1);
            simulatedActivityInterface.offsetMin = new Vector2(0, 0);
            simulatedActivityInterface.offsetMax = new Vector2(0, 0);
            simulatedActivityInterface.anchoredPosition = new Vector2(0, 0);
            GameObject permissionRequesterExibitionRoot = new GameObject("Simulated Activity");
            permissionRequesterExibitionRoot.transform.position = Vector3.zero;
            permissionRequesterExibitionRoot.transform.eulerAngles = Vector3.zero;
            permissionRequesterExibitionRoot.transform.localScale = Vector3.one;
            permissionRequesterExibitionRoot.transform.SetParent(permissionRequesterInterfaceRoot.transform);
            simulatedActivityExibition = permissionRequesterExibitionRoot.AddComponent<RectTransform>();
            simulatedActivityExibition.pivot = new Vector2(0.5f, 0.5f);
            simulatedActivityExibition.anchorMin = new Vector2(0, 0);
            simulatedActivityExibition.anchorMax = new Vector2(1, 1);
            simulatedActivityExibition.offsetMin = new Vector2(0, 0);
            simulatedActivityExibition.offsetMax = new Vector2(0, 0);
            simulatedActivityExibition.anchoredPosition = new Vector2(0, 0);
            permissionRequesterExibitionRoot.AddComponent<CanvasRenderer>();
            Image permissionRequesterExibitionImg = permissionRequesterExibitionRoot.AddComponent<Image>();
            permissionRequesterExibitionImg.color = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            Outline permissionRequesterExibitionOutline = permissionRequesterExibitionRoot.AddComponent<Outline>();
            permissionRequesterExibitionOutline.effectDistance = new Vector2(4, 4);
            simulatedActivityExibition.gameObject.SetActive(false);
            GameObject permissionRequesterExibitionTextRoot = new GameObject("Text");
            permissionRequesterExibitionTextRoot.transform.position = Vector3.zero;
            permissionRequesterExibitionTextRoot.transform.eulerAngles = Vector3.zero;
            permissionRequesterExibitionTextRoot.transform.localScale = Vector3.one;
            permissionRequesterExibitionTextRoot.transform.SetParent(permissionRequesterExibitionRoot.transform);
            RectTransform permissionRequesterExibitionTextRt = permissionRequesterExibitionTextRoot.AddComponent<RectTransform>();
            permissionRequesterExibitionTextRt.pivot = new Vector2(0.5f, 0.5f);
            permissionRequesterExibitionTextRt.anchorMin = new Vector2(0, 0);
            permissionRequesterExibitionTextRt.anchorMax = new Vector2(1, 1);
            permissionRequesterExibitionTextRt.offsetMin = new Vector2(0, 0);
            permissionRequesterExibitionTextRt.offsetMax = new Vector2(0, 0);
            permissionRequesterExibitionTextRt.anchoredPosition = new Vector2(0, 0);
            permissionRequesterExibitionTextRoot.AddComponent<CanvasRenderer>();
            simulatedActivityExibitionText = permissionRequesterExibitionTextRoot.AddComponent<Text>();
            simulatedActivityExibitionText.text = "Temp";
            simulatedActivityExibitionText.font = basicTextFont;
            simulatedActivityExibitionText.fontStyle = FontStyle.Normal;
            simulatedActivityExibitionText.fontSize = 20;
            simulatedActivityExibitionText.alignment = TextAnchor.MiddleCenter;
            simulatedActivityExibitionText.color = Color.black;
            simulatedActivityExibitionText.raycastTarget = false;
            GameObject permissionsRequesterExibitionButtonRoot = new GameObject("Close");
            permissionsRequesterExibitionButtonRoot.transform.position = Vector3.zero;
            permissionsRequesterExibitionButtonRoot.transform.eulerAngles = Vector3.zero;
            permissionsRequesterExibitionButtonRoot.transform.localScale = Vector3.one;
            permissionsRequesterExibitionButtonRoot.transform.SetParent(permissionRequesterExibitionRoot.transform);
            RectTransform permissionsRequesterExibitionButton = permissionsRequesterExibitionButtonRoot.AddComponent<RectTransform>();
            permissionsRequesterExibitionButton.pivot = new Vector2(0.5f, 0);
            permissionsRequesterExibitionButton.anchorMin = new Vector2(0.5f, 0);
            permissionsRequesterExibitionButton.anchorMax = new Vector2(0.5f, 0);
            permissionsRequesterExibitionButton.offsetMin = new Vector2(-160, 0);
            permissionsRequesterExibitionButton.offsetMax = new Vector2(0, 40);
            permissionsRequesterExibitionButton.anchoredPosition = new Vector2(0, 48);
            permissionsRequesterExibitionButtonRoot.AddComponent<CanvasRenderer>();
            Image permissionsRequesterExibitionButtonImg = permissionsRequesterExibitionButtonRoot.AddComponent<Image>();
            permissionsRequesterExibitionButtonImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            simulatedActivityExibitionCloseButton = permissionsRequesterExibitionButtonRoot.AddComponent<Button>();
            simulatedActivityExibitionCloseButton.interactable = true;
            simulatedActivityExibitionCloseButton.transition = Selectable.Transition.ColorTint;
            simulatedActivityExibitionCloseButton.targetGraphic = permissionsRequesterExibitionButtonImg;
            GameObject permissionRequesterExibitionButtonTextRoot = new GameObject("Text");
            permissionRequesterExibitionButtonTextRoot.transform.position = Vector3.zero;
            permissionRequesterExibitionButtonTextRoot.transform.eulerAngles = Vector3.zero;
            permissionRequesterExibitionButtonTextRoot.transform.localScale = Vector3.one;
            permissionRequesterExibitionButtonTextRoot.transform.SetParent(permissionsRequesterExibitionButtonRoot.transform);
            RectTransform permissionsRequesterExibitionButtonRt = permissionRequesterExibitionButtonTextRoot.AddComponent<RectTransform>();
            permissionsRequesterExibitionButtonRt.pivot = new Vector2(0.5f, 0.5f);
            permissionsRequesterExibitionButtonRt.anchorMin = new Vector2(0, 0);
            permissionsRequesterExibitionButtonRt.anchorMax = new Vector2(1, 1);
            permissionsRequesterExibitionButtonRt.offsetMin = new Vector2(0, 0);
            permissionsRequesterExibitionButtonRt.offsetMax = new Vector2(0, 0);
            permissionsRequesterExibitionButtonRt.anchoredPosition = new Vector2(0, 0);
            Text permissionRequesterExibitionButtonText = permissionRequesterExibitionButtonTextRoot.AddComponent<Text>();
            permissionRequesterExibitionButtonText.text = "Close";
            permissionRequesterExibitionButtonText.font = basicTextFont;
            permissionRequesterExibitionButtonText.fontStyle = FontStyle.Normal;
            permissionRequesterExibitionButtonText.fontSize = 12;
            permissionRequesterExibitionButtonText.alignment = TextAnchor.MiddleCenter;
            permissionRequesterExibitionButtonText.color = Color.black;

            //Create location interface
            GameObject locationInterfaceRoot = new GameObject("Location Interface");
            locationInterfaceRoot.transform.position = Vector3.zero;
            locationInterfaceRoot.transform.eulerAngles = Vector3.zero;
            locationInterfaceRoot.transform.localScale = Vector3.one;
            locationInterfaceRoot.transform.SetParent(canvasRoot.transform);
            locationInterface = locationInterfaceRoot.AddComponent<RectTransform>();
            locationInterface.pivot = new Vector2(0.5f, 0.5f);
            locationInterface.anchorMin = new Vector2(0, 0);
            locationInterface.anchorMax = new Vector2(1, 1);
            locationInterface.offsetMin = new Vector2(0, 0);
            locationInterface.offsetMax = new Vector2(0, 0);
            locationInterface.anchoredPosition = new Vector2(0, 0);
            GameObject usingLocationInterfaceRoot = new GameObject("Location");
            usingLocationInterfaceRoot.transform.position = Vector3.zero;
            usingLocationInterfaceRoot.transform.eulerAngles = Vector3.zero;
            usingLocationInterfaceRoot.transform.localScale = Vector3.one;
            usingLocationInterfaceRoot.transform.SetParent(locationInterfaceRoot.transform);
            usingLocationInterface = usingLocationInterfaceRoot.AddComponent<RectTransform>();
            usingLocationInterface.pivot = new Vector2(1, 1);
            usingLocationInterface.anchorMin = new Vector2(1, 1);
            usingLocationInterface.anchorMax = new Vector2(1, 1);
            usingLocationInterface.offsetMin = new Vector2(0, 0);
            usingLocationInterface.offsetMax = new Vector2(36, 36);
            usingLocationInterface.anchoredPosition = new Vector2(-8, -8);
            usingLocationInterfaceRoot.AddComponent<CanvasRenderer>();
            Image usingLocationBg = usingLocationInterfaceRoot.AddComponent<Image>();
            usingLocationBg.color = new Color(0.7f, 0.7f, 0.7f);
            Outline usingLocationOutline = usingLocationInterfaceRoot.AddComponent<Outline>();
            usingLocationOutline.effectColor = new Color(0, 0, 0, 1);
            usingLocationOutline.effectDistance = new Vector2(2, -2);
            usingLocationInterfaceRoot.SetActive(false);
            GameObject usingLocationInterfaceFgRoot = new GameObject("Icon");
            usingLocationInterfaceFgRoot.transform.position = Vector3.zero;
            usingLocationInterfaceFgRoot.transform.eulerAngles = Vector3.zero;
            usingLocationInterfaceFgRoot.transform.localScale = Vector3.one;
            usingLocationInterfaceFgRoot.transform.SetParent(usingLocationInterfaceRoot.transform);
            RectTransform usingLocationInterfaceFg = usingLocationInterfaceFgRoot.AddComponent<RectTransform>();
            usingLocationInterfaceFg.pivot = new Vector2(0.5f, 0.5f);
            usingLocationInterfaceFg.anchorMin = new Vector2(0, 0);
            usingLocationInterfaceFg.anchorMax = new Vector2(1, 1);
            usingLocationInterfaceFg.offsetMin = new Vector2(0, 0);
            usingLocationInterfaceFg.offsetMax = new Vector2(-8, -8);
            usingLocationInterfaceFg.anchoredPosition = new Vector2(0, 0);
            usingLocationInterfaceFgRoot.AddComponent<CanvasRenderer>();
            Image usingLocationFg = usingLocationInterfaceFgRoot.AddComponent<Image>();
#if UNITY_EDITOR
            Texture locationIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Location.png", typeof(Texture));
            usingLocationFg.sprite = Sprite.Create((Texture2D)locationIcon, new Rect(0, 0, locationIcon.width, locationIcon.height), new Vector2(0.5f, 0.5f));
#endif

            //Create microphone interface
            GameObject microphoneInterfaceRoot = new GameObject("Microphone Interface");
            microphoneInterfaceRoot.transform.position = Vector3.zero;
            microphoneInterfaceRoot.transform.eulerAngles = Vector3.zero;
            microphoneInterfaceRoot.transform.localScale = Vector3.one;
            microphoneInterfaceRoot.transform.SetParent(canvasRoot.transform);
            microphoneInterface = microphoneInterfaceRoot.AddComponent<RectTransform>();
            microphoneInterface.pivot = new Vector2(0.5f, 0.5f);
            microphoneInterface.anchorMin = new Vector2(0, 0);
            microphoneInterface.anchorMax = new Vector2(1, 1);
            microphoneInterface.offsetMin = new Vector2(0, 0);
            microphoneInterface.offsetMax = new Vector2(0, 0);
            microphoneInterface.anchoredPosition = new Vector2(0, 0);
            GameObject usingMicrophoneInterfaceRoot = new GameObject("Microphone");
            usingMicrophoneInterfaceRoot.transform.position = Vector3.zero;
            usingMicrophoneInterfaceRoot.transform.eulerAngles = Vector3.zero;
            usingMicrophoneInterfaceRoot.transform.localScale = Vector3.one;
            usingMicrophoneInterfaceRoot.transform.SetParent(microphoneInterfaceRoot.transform);
            usingMicrophoneInterface = usingMicrophoneInterfaceRoot.AddComponent<RectTransform>();
            usingMicrophoneInterface.pivot = new Vector2(1, 1);
            usingMicrophoneInterface.anchorMin = new Vector2(1, 1);
            usingMicrophoneInterface.anchorMax = new Vector2(1, 1);
            usingMicrophoneInterface.offsetMin = new Vector2(0, 0);
            usingMicrophoneInterface.offsetMax = new Vector2(36, 36);
            usingMicrophoneInterface.anchoredPosition = new Vector2(-54, -8);
            usingMicrophoneInterfaceRoot.AddComponent<CanvasRenderer>();
            Image usingMicrophoneBg = usingMicrophoneInterfaceRoot.AddComponent<Image>();
            usingMicrophoneBg.color = new Color(0.7f, 0.7f, 0.7f);
            Outline usingMicrophoneOutline = usingMicrophoneInterfaceRoot.AddComponent<Outline>();
            usingMicrophoneOutline.effectColor = new Color(0, 0, 0, 1);
            usingMicrophoneOutline.effectDistance = new Vector2(2, -2);
            usingMicrophoneInterfaceRoot.SetActive(false);
            GameObject usingMicrophoneInterfaceFgRoot = new GameObject("Icon");
            usingMicrophoneInterfaceFgRoot.transform.position = Vector3.zero;
            usingMicrophoneInterfaceFgRoot.transform.eulerAngles = Vector3.zero;
            usingMicrophoneInterfaceFgRoot.transform.localScale = Vector3.one;
            usingMicrophoneInterfaceFgRoot.transform.SetParent(usingMicrophoneInterfaceRoot.transform);
            RectTransform usingMicrophoneInterfaceFg = usingMicrophoneInterfaceFgRoot.AddComponent<RectTransform>();
            usingMicrophoneInterfaceFg.pivot = new Vector2(0.5f, 0.5f);
            usingMicrophoneInterfaceFg.anchorMin = new Vector2(0, 0);
            usingMicrophoneInterfaceFg.anchorMax = new Vector2(1, 1);
            usingMicrophoneInterfaceFg.offsetMin = new Vector2(0, 0);
            usingMicrophoneInterfaceFg.offsetMax = new Vector2(-8, -8);
            usingMicrophoneInterfaceFg.anchoredPosition = new Vector2(0, 0);
            usingMicrophoneInterfaceFgRoot.AddComponent<CanvasRenderer>();
            Image usingMicrophoneFg = usingMicrophoneInterfaceFgRoot.AddComponent<Image>();
#if UNITY_EDITOR
            Texture microphoneIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/MT Assets/Native Android Toolkit/Editor/Images/Microphone.png", typeof(Texture));
            usingMicrophoneFg.sprite = Sprite.Create((Texture2D)microphoneIcon, new Rect(0, 0, microphoneIcon.width, microphoneIcon.height), new Vector2(0.5f, 0.5f));
#endif

            //Create datetime interface
            GameObject datetimeInterfaceRoot = new GameObject("DateTime Interface");
            datetimeInterfaceRoot.transform.position = Vector3.zero;
            datetimeInterfaceRoot.transform.eulerAngles = Vector3.zero;
            datetimeInterfaceRoot.transform.localScale = Vector3.one;
            datetimeInterfaceRoot.transform.SetParent(canvasRoot.transform);
            datetimeInterfaceRoot.SetActive(false);
            datetimeInterface = datetimeInterfaceRoot.AddComponent<RectTransform>();
            datetimeInterface.pivot = new Vector2(0.5f, 0.5f);
            datetimeInterface.anchorMin = new Vector2(0, 0);
            datetimeInterface.anchorMax = new Vector2(1, 1);
            datetimeInterface.offsetMin = new Vector2(0, 0);
            datetimeInterface.offsetMax = new Vector2(0, 0);
            datetimeInterface.anchoredPosition = new Vector2(0, 0);
            datetimeInterfaceRoot.AddComponent<CanvasRenderer>();
            Image datetimeInterfaceImg = datetimeInterfaceRoot.AddComponent<Image>();
            datetimeInterfaceImg.color = new Color(0, 0, 0, 0.5f);
            GameObject pickerUiInterfaceRoot = new GameObject("Picker UI");
            pickerUiInterfaceRoot.transform.position = Vector3.zero;
            pickerUiInterfaceRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceRoot.transform.SetParent(datetimeInterfaceRoot.transform);
            RectTransform pickerUiInterface = pickerUiInterfaceRoot.AddComponent<RectTransform>();
            pickerUiInterface.pivot = new Vector2(0.5f, 0.5f);
            pickerUiInterface.anchorMin = new Vector2(0.5f, 0.5f);
            pickerUiInterface.anchorMax = new Vector2(0.5f, 0.5f);
            pickerUiInterface.offsetMin = new Vector2(0, 0);
            pickerUiInterface.offsetMax = new Vector2(550, 350);
            pickerUiInterface.anchoredPosition = new Vector2(0, 0);
            pickerUiInterfaceRoot.AddComponent<CanvasRenderer>();
            Image pickerUiInterfaceImg = pickerUiInterfaceRoot.AddComponent<Image>();
            GameObject pickerUiInterfaceTitleRoot = new GameObject("Title");
            pickerUiInterfaceTitleRoot.transform.position = Vector3.zero;
            pickerUiInterfaceTitleRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceTitleRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceTitleRoot.transform.SetParent(pickerUiInterfaceRoot.transform);
            RectTransform pickerUiInterfaceTitle = pickerUiInterfaceTitleRoot.AddComponent<RectTransform>();
            pickerUiInterfaceTitle.pivot = new Vector2(0.5f, 1);
            pickerUiInterfaceTitle.anchorMin = new Vector2(0, 1);
            pickerUiInterfaceTitle.anchorMax = new Vector2(1, 1);
            pickerUiInterfaceTitle.offsetMin = new Vector2(16, 0);
            pickerUiInterfaceTitle.offsetMax = new Vector2(0, 50);
            pickerUiInterfaceTitle.anchoredPosition = new Vector2(0, -6);
            pickerUiInterfaceTitleRoot.AddComponent<CanvasRenderer>();
            pickerUiInterfaceTitleText = pickerUiInterfaceTitleRoot.AddComponent<Text>();
            pickerUiInterfaceTitleText.text = "Title";
            pickerUiInterfaceTitleText.font = basicTextFont;
            pickerUiInterfaceTitleText.fontStyle = FontStyle.Bold;
            pickerUiInterfaceTitleText.fontSize = 22;
            pickerUiInterfaceTitleText.alignment = TextAnchor.MiddleLeft;
            pickerUiInterfaceTitleText.color = Color.black;
            GameObject pickerUiInterfaceButtonsGroupRoot = new GameObject("Buttons Root");
            pickerUiInterfaceButtonsGroupRoot.transform.position = Vector3.zero;
            pickerUiInterfaceButtonsGroupRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceButtonsGroupRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceButtonsGroupRoot.transform.SetParent(pickerUiInterfaceRoot.transform);
            RectTransform pickerUiInterfaceButtonsGroup = pickerUiInterfaceButtonsGroupRoot.AddComponent<RectTransform>();
            pickerUiInterfaceButtonsGroup.pivot = new Vector2(0.5f, 0);
            pickerUiInterfaceButtonsGroup.anchorMin = new Vector2(0, 0);
            pickerUiInterfaceButtonsGroup.anchorMax = new Vector2(1, 0);
            pickerUiInterfaceButtonsGroup.offsetMin = new Vector2(16, 0);
            pickerUiInterfaceButtonsGroup.offsetMax = new Vector2(0, 30);
            pickerUiInterfaceButtonsGroup.anchoredPosition = new Vector2(0, 8);
            HorizontalLayoutGroup pickerUiInterfaceButtonsGroupLayout = pickerUiInterfaceButtonsGroupRoot.AddComponent<HorizontalLayoutGroup>();
            pickerUiInterfaceButtonsGroupLayout.spacing = 10;
            pickerUiInterfaceButtonsGroupLayout.childAlignment = TextAnchor.MiddleCenter;
            pickerUiInterfaceButtonsGroupLayout.childForceExpandHeight = true;
            pickerUiInterfaceButtonsGroupLayout.childForceExpandWidth = true;
            GameObject pickerUiInterfaceButtonRoot = new GameObject("Button");
            pickerUiInterfaceButtonRoot.transform.position = Vector3.zero;
            pickerUiInterfaceButtonRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceButtonRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceButtonRoot.transform.SetParent(pickerUiInterfaceButtonsGroupRoot.transform);
            RectTransform pickerUiInterfaceButton = pickerUiInterfaceButtonRoot.AddComponent<RectTransform>();
            pickerUiInterfaceButton.pivot = new Vector2(0.5f, 0.5f);
            pickerUiInterfaceButton.anchorMin = new Vector2(0, 1);
            pickerUiInterfaceButton.anchorMax = new Vector2(0, 1);
            pickerUiInterfaceButton.offsetMin = new Vector2(0, 0);
            pickerUiInterfaceButton.offsetMax = new Vector2(0, 0);
            pickerUiInterfaceButton.anchoredPosition = new Vector2(0, 0);
            pickerUiInterfaceButtonRoot.AddComponent<CanvasRenderer>();
            Image pickerUiInterfaceButtonImg = pickerUiInterfaceButtonRoot.AddComponent<Image>();
            pickerUiInterfaceButtonImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            pickerUiInterfaceButtonBt = pickerUiInterfaceButtonRoot.AddComponent<Button>();
            pickerUiInterfaceButtonBt.interactable = true;
            pickerUiInterfaceButtonBt.transition = Selectable.Transition.ColorTint;
            pickerUiInterfaceButtonBt.targetGraphic = pickerUiInterfaceButtonImg;
            GameObject pickerUiInterfaceButtonTextRoot = new GameObject("Text");
            pickerUiInterfaceButtonTextRoot.transform.position = Vector3.zero;
            pickerUiInterfaceButtonTextRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceButtonTextRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceButtonTextRoot.transform.SetParent(pickerUiInterfaceButtonRoot.transform);
            RectTransform pickerButtonRectTransform = pickerUiInterfaceButtonTextRoot.AddComponent<RectTransform>();
            pickerButtonRectTransform.pivot = new Vector2(0.5f, 0.5f);
            pickerButtonRectTransform.anchorMin = new Vector2(0, 0);
            pickerButtonRectTransform.anchorMax = new Vector2(1, 1);
            pickerButtonRectTransform.offsetMin = new Vector2(0, 0);
            pickerButtonRectTransform.offsetMax = new Vector2(0, 0);
            pickerButtonRectTransform.anchoredPosition = new Vector2(0, 0);
            Text pickerUiInterfaceButtonText = pickerUiInterfaceButtonTextRoot.AddComponent<Text>();
            pickerUiInterfaceButtonText.text = "Done";
            pickerUiInterfaceButtonText.font = basicTextFont;
            pickerUiInterfaceButtonText.fontStyle = FontStyle.Normal;
            pickerUiInterfaceButtonText.fontSize = 12;
            pickerUiInterfaceButtonText.alignment = TextAnchor.MiddleCenter;
            pickerUiInterfaceButtonText.color = Color.black;
            GameObject pickerUiInterfaceContentTextRoot = new GameObject("Content Pick");
            pickerUiInterfaceContentTextRoot.transform.position = Vector3.zero;
            pickerUiInterfaceContentTextRoot.transform.eulerAngles = Vector3.zero;
            pickerUiInterfaceContentTextRoot.transform.localScale = Vector3.one;
            pickerUiInterfaceContentTextRoot.transform.SetParent(pickerUiInterfaceRoot.transform);
            RectTransform pickerUiInterfaceContentTextGroup = pickerUiInterfaceContentTextRoot.AddComponent<RectTransform>();
            pickerUiInterfaceContentTextGroup.pivot = new Vector2(0.5f, 0.5f);
            pickerUiInterfaceContentTextGroup.anchorMin = new Vector2(0, 0);
            pickerUiInterfaceContentTextGroup.anchorMax = new Vector2(1, 1);
            pickerUiInterfaceContentTextGroup.offsetMin = new Vector2(16, 120);
            pickerUiInterfaceContentTextGroup.offsetMax = new Vector2(0, 0);
            pickerUiInterfaceContentTextGroup.anchoredPosition = new Vector2(0, 0);
            VerticalLayoutGroup pickerUiInterfaceInputsGroupLayout = pickerUiInterfaceContentTextRoot.AddComponent<VerticalLayoutGroup>();
            pickerUiInterfaceInputsGroupLayout.spacing = 10;
            pickerUiInterfaceInputsGroupLayout.childAlignment = TextAnchor.MiddleCenter;
            pickerUiInterfaceInputsGroupLayout.childForceExpandWidth = true;
            pickerUiInterfaceInputsGroupLayout.childForceExpandHeight = false;
            pickerUiInterfaceInputsGroupLayout.childControlWidth = true;
            pickerUiInterfaceInputsGroupLayout.childControlHeight = false;
            for (int i = 0; i < 3; i++)
            {
                GameObject pickerUiInterfaceInputRoot = new GameObject("Input " + i.ToString());
                pickerUiInterfaceInputRoot.transform.position = Vector3.zero;
                pickerUiInterfaceInputRoot.transform.eulerAngles = Vector3.zero;
                pickerUiInterfaceInputRoot.transform.localScale = Vector3.one;
                pickerUiInterfaceInputRoot.transform.SetParent(pickerUiInterfaceContentTextRoot.transform);
                RectTransform pickerUiInterfaceInput = pickerUiInterfaceInputRoot.AddComponent<RectTransform>();
                pickerUiInterfaceInput.pivot = new Vector2(0.5f, 0.5f);
                pickerUiInterfaceInput.anchorMin = new Vector2(0, 1);
                pickerUiInterfaceInput.anchorMax = new Vector2(0, 1);
                pickerUiInterfaceInput.offsetMin = new Vector2(0, 0);
                pickerUiInterfaceInput.offsetMax = new Vector2(0, 30);
                pickerUiInterfaceInput.anchoredPosition = new Vector2(0, 0);
                pickerUiInterfaceInputRoot.AddComponent<CanvasRenderer>();
                Image pickerUiInterfaceInputImg = pickerUiInterfaceInputRoot.AddComponent<Image>();
                pickerUiInterfaceInputImg.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                pickerUiInterfaceInputs[i] = pickerUiInterfaceInputRoot.AddComponent<InputField>();
                pickerUiInterfaceInputs[i].interactable = true;
                pickerUiInterfaceInputs[i].transition = Selectable.Transition.ColorTint;
                pickerUiInterfaceInputs[i].targetGraphic = pickerUiInterfaceInputImg;
                pickerUiInterfaceInputs[i].contentType = InputField.ContentType.IntegerNumber;
                GameObject pickerUiInterfaceInputPlaceHolderRoot = new GameObject("Place Holder");
                pickerUiInterfaceInputPlaceHolderRoot.transform.position = Vector3.zero;
                pickerUiInterfaceInputPlaceHolderRoot.transform.eulerAngles = Vector3.zero;
                pickerUiInterfaceInputPlaceHolderRoot.transform.localScale = Vector3.one;
                pickerUiInterfaceInputPlaceHolderRoot.transform.SetParent(pickerUiInterfaceInputRoot.transform);
                RectTransform placeHolderRectTransform = pickerUiInterfaceInputPlaceHolderRoot.AddComponent<RectTransform>();
                placeHolderRectTransform.pivot = new Vector2(0.5f, 0.5f);
                placeHolderRectTransform.anchorMin = new Vector2(0, 0);
                placeHolderRectTransform.anchorMax = new Vector2(1, 1);
                placeHolderRectTransform.offsetMin = new Vector2(0, 0);
                placeHolderRectTransform.offsetMax = new Vector2(0, 0);
                placeHolderRectTransform.anchoredPosition = new Vector2(0, 0);
                pickerUiInterfaceInputsPlaceHolders[i] = pickerUiInterfaceInputPlaceHolderRoot.AddComponent<Text>();
                pickerUiInterfaceInputsPlaceHolders[i].text = "Type Here";
                pickerUiInterfaceInputsPlaceHolders[i].font = basicTextFont;
                pickerUiInterfaceInputsPlaceHolders[i].fontStyle = FontStyle.Normal;
                pickerUiInterfaceInputsPlaceHolders[i].fontSize = 12;
                pickerUiInterfaceInputsPlaceHolders[i].alignment = TextAnchor.MiddleCenter;
                pickerUiInterfaceInputsPlaceHolders[i].color = Color.white;
                pickerUiInterfaceInputs[i].placeholder = pickerUiInterfaceInputsPlaceHolders[i];
                GameObject pickerUiInterfaceInputValueRoot = new GameObject("Text");
                pickerUiInterfaceInputValueRoot.transform.position = Vector3.zero;
                pickerUiInterfaceInputValueRoot.transform.eulerAngles = Vector3.zero;
                pickerUiInterfaceInputValueRoot.transform.localScale = Vector3.one;
                pickerUiInterfaceInputValueRoot.transform.SetParent(pickerUiInterfaceInputRoot.transform);
                RectTransform valueRectTransform = pickerUiInterfaceInputValueRoot.AddComponent<RectTransform>();
                valueRectTransform.pivot = new Vector2(0.5f, 0.5f);
                valueRectTransform.anchorMin = new Vector2(0, 0);
                valueRectTransform.anchorMax = new Vector2(1, 1);
                valueRectTransform.offsetMin = new Vector2(0, 0);
                valueRectTransform.offsetMax = new Vector2(0, 0);
                valueRectTransform.anchoredPosition = new Vector2(0, 0);
                Text pickerUiInterfaceInputValue = pickerUiInterfaceInputValueRoot.AddComponent<Text>();
                pickerUiInterfaceInputValue.text = "";
                pickerUiInterfaceInputValue.font = basicTextFont;
                pickerUiInterfaceInputValue.fontStyle = FontStyle.Normal;
                pickerUiInterfaceInputValue.fontSize = 12;
                pickerUiInterfaceInputValue.alignment = TextAnchor.MiddleCenter;
                pickerUiInterfaceInputValue.color = Color.black;
                pickerUiInterfaceInputs[i].textComponent = pickerUiInterfaceInputValue;
            }

            //Create the Input System
            GameObject canvasInputSystem = new GameObject("Event System");
            canvasInputSystem.transform.position = Vector3.zero;
            canvasInputSystem.transform.eulerAngles = Vector3.zero;
            canvasInputSystem.transform.localScale = Vector3.one;
            canvasInputSystem.transform.SetParent(canvasRoot.transform);
            canvasInputSystem.AddComponent<EventSystem>();
            canvasInputSystem.AddComponent<StandaloneInputModule>();
        }

        //Dialogs

        public void ShowSimpleAlertDialog(string title, string text, bool isCancelable)
        {
            //Show simple alert dialog in emulated interface
            dialogsInterface.gameObject.SetActive(true);
            dialogUiInterfaceContentTextGroup.gameObject.SetActive(true);
            dialogUiInterfaceTitleText.text = title;
            dialogUiInterfaceContentText.text = text;
            dialogUiInterfaceButtonsBt[0].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[1].gameObject.SetActive(false);
            dialogUiInterfaceButtonsBt[2].gameObject.SetActive(false);
            dialogUiInterfaceButtonsText[0].text = "Ok";
            if (isCancelable == true)
                dialogsInterfaceCancelationButton.onClick.AddListener(() =>
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.SimpleAlertDialog_Cancel();

                    dialogsInterface.gameObject.SetActive(false);
                    dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                    dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                    dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                });
            dialogUiInterfaceButtonsBt[0].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.SimpleAlertDialog_OkButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
        }

        public void ShowConfirmationDialog(string title, string text, bool isCancelable, string yesButton, string noButton)
        {
            //Show simple alert dialog in emulated interface
            dialogsInterface.gameObject.SetActive(true);
            dialogUiInterfaceContentTextGroup.gameObject.SetActive(true);
            dialogUiInterfaceTitleText.text = title;
            dialogUiInterfaceContentText.text = text;
            dialogUiInterfaceButtonsBt[0].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[1].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[2].gameObject.SetActive(false);
            dialogUiInterfaceButtonsText[0].text = yesButton;
            dialogUiInterfaceButtonsText[1].text = noButton;
            if (isCancelable == true)
                dialogsInterfaceCancelationButton.onClick.AddListener(() =>
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.ConfirmationDialog_Cancel();

                    dialogsInterface.gameObject.SetActive(false);
                    dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                    dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                    dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                    dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                });
            dialogUiInterfaceButtonsBt[0].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.ConfirmationDialog_YesButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
            dialogUiInterfaceButtonsBt[1].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.ConfirmationDialog_NoButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
        }

        public void ShowNeutralDialog(string title, string text, bool isCancelable, string yesButton, string noButton, string neutralButton)
        {
            //Show simple alert dialog in emulated interface
            dialogsInterface.gameObject.SetActive(true);
            dialogUiInterfaceContentTextGroup.gameObject.SetActive(true);
            dialogUiInterfaceTitleText.text = title;
            dialogUiInterfaceContentText.text = text;
            dialogUiInterfaceButtonsBt[0].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[1].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[2].gameObject.SetActive(true);
            dialogUiInterfaceButtonsText[0].text = yesButton;
            dialogUiInterfaceButtonsText[1].text = noButton;
            dialogUiInterfaceButtonsText[2].text = neutralButton;
            if (isCancelable == true)
                dialogsInterfaceCancelationButton.onClick.AddListener(() =>
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.NeutralDialog_CancelButton();

                    dialogsInterface.gameObject.SetActive(false);
                    dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                    dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                    dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                    dialogUiInterfaceButtonsBt[2].onClick.RemoveAllListeners();
                    dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                });
            dialogUiInterfaceButtonsBt[0].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.NeutralDialog_YesButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[2].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
            dialogUiInterfaceButtonsBt[1].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.NeutralDialog_NoButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[2].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
            dialogUiInterfaceButtonsBt[2].onClick.AddListener(() =>
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.NeutralDialog_NeutralButton();

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentTextGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[1].onClick.RemoveAllListeners();
                dialogUiInterfaceButtonsBt[2].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
            });
        }

        public void ShowRadialListDialog(string title, string[] options, bool isCancelable, string doneButton, int defaultCheckedOption)
        {
            //Show simple alert dialog in emulated interface
            dialogsInterface.gameObject.SetActive(true);
            dialogUiInterfaceContentChecksGroup.gameObject.SetActive(true);
            dialogUiInterfaceTitleText.text = title;
            dialogUiInterfaceButtonsBt[0].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[1].gameObject.SetActive(false);
            dialogUiInterfaceButtonsBt[2].gameObject.SetActive(false);
            dialogUiInterfaceButtonsText[0].text = doneButton;
            foreach (Toggle toggle in dialogUiInterfaceToggles)
                toggle.gameObject.SetActive(false);
            for (int i = 0; i < options.Length; i++)
            {
                dialogUiInterfaceToggles[i].gameObject.SetActive(true);
                dialogUiInterfaceToggleTexts[i].text = options[i];
            }
            foreach (Toggle toggle in dialogUiInterfaceToggles)
            {
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((bool isOn) =>
                {
                    foreach (Toggle toggle2 in dialogUiInterfaceToggles)
                    {
                        if (toggle2 != toggle)
                            toggle2.isOn = false;
                        if (toggle2 == toggle)
                            toggle2.isOn = isOn;
                    }
                });
            }
            for (int i = 0; i < dialogUiInterfaceToggles.Length; i++)
                if (i == defaultCheckedOption)
                    dialogUiInterfaceToggles[i].isOn = true;
            if (isCancelable == true)
                dialogsInterfaceCancelationButton.onClick.AddListener(() =>
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.RadialListDialog_Cancel();

                    dialogsInterface.gameObject.SetActive(false);
                    dialogUiInterfaceContentChecksGroup.gameObject.SetActive(false);
                    dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                    dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                    foreach (Toggle toggle in dialogUiInterfaceToggles)
                    {
                        toggle.isOn = false;
                        toggle.onValueChanged.RemoveAllListeners();
                    }
                });
            dialogUiInterfaceButtonsBt[0].onClick.AddListener(() =>
            {
                int selected = 0;
                for (int i = 0; i < dialogUiInterfaceToggles.Length; i++)
                    if (dialogUiInterfaceToggles[i].isOn == true)
                    {
                        selected = i;
                        break;
                    }

                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.RadialListDialog_Done(selected.ToString());

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentChecksGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                foreach (Toggle toggle in dialogUiInterfaceToggles)
                {
                    toggle.isOn = false;
                    toggle.onValueChanged.RemoveAllListeners();
                }
            });
        }

        public void ShowCheckListDialog(string title, string[] options, bool isCancelable, string doneButton, bool[] defaultCheckedOptions)
        {
            //Show simple alert dialog in emulated interface
            dialogsInterface.gameObject.SetActive(true);
            dialogUiInterfaceContentChecksGroup.gameObject.SetActive(true);
            dialogUiInterfaceTitleText.text = title;
            dialogUiInterfaceButtonsBt[0].gameObject.SetActive(true);
            dialogUiInterfaceButtonsBt[1].gameObject.SetActive(false);
            dialogUiInterfaceButtonsBt[2].gameObject.SetActive(false);
            dialogUiInterfaceButtonsText[0].text = doneButton;
            foreach (Toggle toggle in dialogUiInterfaceToggles)
                toggle.gameObject.SetActive(false);
            for (int i = 0; i < options.Length; i++)
            {
                dialogUiInterfaceToggles[i].gameObject.SetActive(true);
                dialogUiInterfaceToggleTexts[i].text = options[i];
            }
            if (isCancelable == true)
                dialogsInterfaceCancelationButton.onClick.AddListener(() =>
                {
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.CheckboxListDialog_Cancel();

                    dialogsInterface.gameObject.SetActive(false);
                    dialogUiInterfaceContentChecksGroup.gameObject.SetActive(false);
                    dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                    dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                    foreach (Toggle toggle in dialogUiInterfaceToggles)
                    {
                        toggle.isOn = false;
                        toggle.onValueChanged.RemoveAllListeners();
                    }
                });
            dialogUiInterfaceButtonsBt[0].onClick.AddListener(() =>
            {
                bool[] selecteds = new bool[options.Length];
                for (int i = 0; i < selecteds.Length; i++)
                    selecteds[i] = dialogUiInterfaceToggles[i].isOn;
                string result = selecteds[0].ToString();
                for (int ii = 0; ii < selecteds.Length; ii++)
                {
                    if (ii == 0)
                        continue;
                    result += "," + selecteds[ii].ToString();
                }

                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.CheckboxListDialog_Done(result);

                dialogsInterface.gameObject.SetActive(false);
                dialogUiInterfaceContentChecksGroup.gameObject.SetActive(false);
                dialogUiInterfaceButtonsBt[0].onClick.RemoveAllListeners();
                dialogsInterfaceCancelationButton.onClick.RemoveAllListeners();
                foreach (Toggle toggle in dialogUiInterfaceToggles)
                {
                    toggle.isOn = false;
                    toggle.onValueChanged.RemoveAllListeners();
                }
            });
        }

        //Notifications

        public void ShowToast(string text, bool longDuration)
        {
            if (notificationsToastCurrentShowingToast != null)
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StopCoroutine(notificationsToastCurrentShowingToast);
                notificationsToastCurrentShowingToast = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(ShowToastByTime(text, longDuration));
            }
            if (notificationsToastCurrentShowingToast == null)
                notificationsToastCurrentShowingToast = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(ShowToastByTime(text, longDuration));
        }

        private IEnumerator ShowToastByTime(string text, bool longDuration)
        {
            notificationsToastGroup.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            notificationsToastText.text = text;
            notificationsToastGroup.gameObject.SetActive(true);

            notificationsToastGroup.pivot = new Vector2(0.5f, 0);
            notificationsToastGroup.anchorMin = new Vector2(0.5f, 0);
            notificationsToastGroup.anchorMax = new Vector2(0.5f, 0);
            notificationsToastGroup.offsetMin = new Vector2(0, 0);
            notificationsToastGroup.offsetMax = new Vector2(550, 100);
            notificationsToastGroup.anchoredPosition = new Vector2(0, 48);

            if (longDuration == false)
                yield return new WaitForSeconds(2f);
            if (longDuration == true)
                yield return new WaitForSeconds(3.5f);
            notificationsToastGroup.gameObject.SetActive(false);
        }

        public void SendNotification(string title, string text, string clickAction, string button1Txt, string button1Action, string button2Txt, string button2Action)
        {
            if (notificationsPushCurrentShowingPush != null)
            {
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StopCoroutine(notificationsPushCurrentShowingPush);
                notificationsPushCurrentShowingPush = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(SendNotificationByTime(title, text, clickAction, button1Txt, button1Action, button2Txt, button2Action));
            }
            if (notificationsPushCurrentShowingPush == null)
                notificationsPushCurrentShowingPush = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(SendNotificationByTime(title, text, clickAction, button1Txt, button1Action, button2Txt, button2Action));
        }

        private IEnumerator SendNotificationByTime(string title, string text, string clickAction, string button1Txt, string button1Action, string button2Txt, string button2Action)
        {
            Debug.Log("NAT Notification: " + text + "\n\nNotifications Actions - OnClick: \"" + clickAction + "\", Button1: \"" + button1Action + "\", Button2: \"" + button2Action + "\"\n\n");
            notificationsPushGroup.anchoredPosition = new Vector2(0, 120);
            notificationsPushGroup.gameObject.SetActive(false);
            notificationsPushTime.value = 1.0f;
            yield return new WaitForSeconds(0.05f);
            notificationsPushTitle.text = title;
            notificationsPushText.text = text + ((string.IsNullOrEmpty(clickAction) == false) ? "\n[With ClickAction]" : "");
            if (string.IsNullOrEmpty(clickAction) == true)
                notificationsPushButtons[0].enabled = false;
            if (string.IsNullOrEmpty(clickAction) == false)
            {
                notificationsPushButtons[0].enabled = true;
                notificationsPushButtons[0].onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    string warningString = "You've just interacted with a Action notification action. The game was closed to correctly simulate the behavior of a Notification Action on an Android device, this is not an error or crash.\n\nWhen the user interacts with a notification that has actions included, your app opens with NAT's Notification Interact Receiver, which records and archives the user's interaction, and then opens your app normally. Play Mode was stopped.\n\nSee the NAT documentation for more details.";
                    Debug.LogWarning("Native Android Toolkit Notifications: " + warningString + "\n\n");
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/NAT/notify.nat", clickAction); //<- Create a fake notify file, to indicate a notification interaction
                    EditorApplication.ExecuteMenuItem("Edit/Play"); //<- force the play mode end in Editor
                    EditorUtility.DisplayDialog("NAT Notifications", warningString, "Got It!");
#endif
                });
            }
            if (string.IsNullOrEmpty(button1Action) == true)
                notificationsPushButtons[1].gameObject.SetActive(false);
            if (string.IsNullOrEmpty(button1Action) == false)
            {
                notificationsPushButtons[1].gameObject.SetActive(true);
                notificationsButtonsText[1].text = button1Txt;
                notificationsPushButtons[1].onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    string warningString = "You've just interacted with a Action notification action. The game was closed to correctly simulate the behavior of a Notification Action on an Android device, this is not an error or crash.\n\nWhen the user interacts with a notification that has actions included, your app opens with NAT's Notification Interact Receiver, which records and archives the user's interaction, and then opens your app normally. Play Mode was stopped.\n\nSee the NAT documentation for more details.";
                    Debug.LogWarning("Native Android Toolkit Notifications: " + warningString + "\n\n");
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/NAT/notify.nat", button1Action); //<- Create a fake notify file, to indicate a notification interaction
                    EditorApplication.ExecuteMenuItem("Edit/Play"); //<- force the play mode end in Editor
                    EditorUtility.DisplayDialog("NAT Notifications", warningString, "Got It!");
#endif
                });
            }
            if (string.IsNullOrEmpty(button2Action) == true)
                notificationsPushButtons[2].gameObject.SetActive(false);
            if (string.IsNullOrEmpty(button2Action) == false)
            {
                notificationsPushButtons[2].gameObject.SetActive(true);
                notificationsButtonsText[2].text = button2Txt;
                notificationsPushButtons[2].onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    string warningString = "You've just interacted with a Action notification action. The game was closed to correctly simulate the behavior of a Notification Action on an Android device, this is not an error or crash.\n\nWhen the user interacts with a notification that has actions included, your app opens with NAT's Notification Interact Receiver, which records and archives the user's interaction, and then opens your app normally. Play Mode was stopped.\n\nSee the NAT documentation for more details.";
                    Debug.LogWarning("Native Android Toolkit Notifications: " + warningString + "\n\n");
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/NAT/notify.nat", button2Action); //<- Create a fake notify file, to indicate a notification interaction
                    EditorApplication.ExecuteMenuItem("Edit/Play"); //<- force the play mode end in Editor
                    EditorUtility.DisplayDialog("NAT Notifications", warningString, "Got It!");
#endif
                });
            }
            notificationsPushGroup.gameObject.SetActive(true);
            while (notificationsPushGroup.anchoredPosition.y > 0)
            {
                notificationsPushGroup.anchoredPosition -= new Vector2(0, 10);
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1);
            int timeToRemove = 9;
            int timeElapsed = 0;
            while (timeElapsed < timeToRemove)
            {
                notificationsPushTime.value -= 0.1111111f;

                yield return new WaitForSeconds(1);

                timeElapsed += 1;
            }
            while (notificationsPushGroup.anchoredPosition.y != 120)
            {
                notificationsPushGroup.anchoredPosition += new Vector2(0, 10);
                yield return new WaitForSeconds(0.005f);
            }
            foreach (Button bt in notificationsPushButtons)
                bt.onClick.RemoveAllListeners();
            notificationsPushGroup.gameObject.SetActive(false);
        }

        public NAT.Notifications.ScheduledInfo isNotificationScheduledInChannel(NAT.Notifications.Channel channel)
        {
            //Return if a channel is used, and if have a repetitive notification
            bool haveNotification = false;
            bool isRepetitive = false;

            //Check if the channel have a notification
            if (notificationChannelsUsage[NAT.Notifications.ChannelUtils.ChToInt(channel)].x == 1)
            {
                haveNotification = true;
                if (notificationChannelsUsage[NAT.Notifications.ChannelUtils.ChToInt(channel)].y == 1)
                    isRepetitive = true;
            }

            return new NAT.Notifications.ScheduledInfo(haveNotification, isRepetitive);
        }

        public void ScheduleNotification(NAT.Notifications.Channel channel, string title, string text, int years, int months, int days, int hours, int minutes, bool isRepetitive, NAT.Notifications.IntervalType intervalType, int interval, string clickAction, string button1Txt, string button1Action, string button2Txt, string button2Action)
        {
            //Emulate the notification scheduled
            if (isRepetitive == false)
            {
                notificationChannelsUsage[NAT.Notifications.ChannelUtils.ChToInt(channel)] = new Vector2(1, 0);
                //Show the schedulation
                SendNotification("A new Notification has been Scheduled in Channel " + NAT.Notifications.ChannelUtils.ChToInt(channel), "<size=12>A Notification was Scheduled for " + minutes + " min, " + hours + " hours, " + days + " days, " + months + " months and " + years + " years in the future in CH " + NAT.Notifications.ChannelUtils.ChToInt(channel) + ". The content is...</size>\n<color=green>" + text + "</color>", clickAction, button1Txt, button1Action, button2Txt, button2Action);
            }
            if (isRepetitive == true)
            {
                notificationChannelsUsage[NAT.Notifications.ChannelUtils.ChToInt(channel)] = new Vector2(1, 1);
                //Show the creation
                SendNotification("A new Repetitive Notification has been Created in Channel " + NAT.Notifications.ChannelUtils.ChToInt(channel), "<size=12>A Repetitive Notification was Created to be repeated every " + interval + " " + intervalType.ToString() + " in CH " + NAT.Notifications.ChannelUtils.ChToInt(channel) + ". The content is...</size>\n<color=purple>" + text + "</color>", clickAction, button1Txt, button1Action, button2Txt, button2Action);
            }
        }

        public void CancelScheduledNotification(NAT.Notifications.Channel channel)
        {
            //Cancel execution of this method, if not exist notification scheduled
            if (isNotificationScheduledInChannel(channel).isScheduledInThisChannel == false)
                return;

            //Emulate the notification scheduled
            notificationChannelsUsage[NAT.Notifications.ChannelUtils.ChToInt(channel)] = new Vector2(0, 0);

            //Show the schedulation
            ShowToast("Channel " + NAT.Notifications.ChannelUtils.ChToInt(channel) + " Scheduled Notification was cancelled.", false);
        }

        public int[] GetListOfFreeNotificationsChannels()
        {
            //This method will return a array of notifications channels free
            List<int> allFreeNotificationChannels = new List<int>();

            //Add all free notification channels to list
            for (int i = 1; i < notificationChannelsUsage.Length; i++)
                if (notificationChannelsUsage[i].x != 1)
                    allFreeNotificationChannels.Add(i);

            return allFreeNotificationChannels.ToArray();
        }

        //Sharing

        public void ShareTexture2D(Texture2D texture2D, string messageOfShare)
        {
            //This method simulates a texture2d sharing
            SendNotification("Sharing a Texture 2D", "NAT \"ShareTexture2D()\" method was called. The Texture2D \"" + texture2D.name + "\" is being shared. This is a simulation of Share, for Editor.\nMessage of share: " + messageOfShare, "", "", "", "", "");
        }

        public void ShareTextPlain(string textToShare)
        {
            //This method simulates a texture2d sharing
            SendNotification("Sharing a Text Plain", "NAT \"ShareTextPlain()\" method was called. The text below was shared. This is a simulation of this function for Editor.\n\n" + textToShare, "", "", "", "", "");
        }

        public void CopyTextToClipboard(string textToCopy)
        {
            //Set the new clipboard content
            currentClipboardContent = textToCopy;
        }

        public string GetTextFromClipboard()
        {
            //Return the current content of clipboard of this device
            return currentClipboardContent;
        }

        //Webview

        public void OpenWebviewChromium(bool isFullscreen)
        {
            //Configure the webview simulated
            if (isFullscreen == true)
            {
                webviewExibition.pivot = new Vector2(0.5f, 0.5f);
                webviewExibition.anchorMin = new Vector2(0, 0);
                webviewExibition.anchorMax = new Vector2(1, 1);
                webviewExibition.offsetMin = new Vector2(0, 0);
                webviewExibition.offsetMax = new Vector2(0, 0);
                webviewExibition.anchoredPosition = new Vector2(0, 0);
            }
            if (isFullscreen == false)
            {
                webviewExibition.pivot = new Vector2(0.5f, 0.5f);
                webviewExibition.anchorMin = new Vector2(0, 0);
                webviewExibition.anchorMax = new Vector2(1, 1);
                webviewExibition.offsetMin = new Vector2(160, 160);
                webviewExibition.offsetMax = new Vector2(0, 0);
                webviewExibition.anchoredPosition = new Vector2(0, 0);
            }

            //Configure the button callback
            webviewExibitionCloseButton.onClick.AddListener(() =>
            {
                if (isFullscreen == false)
                {
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/NAT/webview.nat", "EditorWebview\nhttps://editorsite1.com\nhttps://editorsite2.com\nhttps://editorsite3.com"); //<- Create a fake fullscreen webview file, to indicate that the fullscreen webview was closed
                    NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.WebviewChromium_OnPopUpWebviewClose();
                }

#if UNITY_EDITOR
                if (isFullscreen == true)
                {
                    string warningString = "You have just closed a Fullscreen Webview (simulated) in the Editor. The game was closed to correctly simulate the behavior of a Fullscreen Webview on an Android device, this is not an error or crash.\n\nAs Webview in Fullscreen runs in a different activity than UnityPlayer on Android, when you close Webview in Fullscreen, your game is re-opened, so when you closed Webview in Fullscreen here in the Editor, Play Mode was stopped.\n\nSee the NAT documentation for more details.";
                    Debug.LogWarning("Native Android Toolkit Webview: " + warningString + "\n\n");
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/NAT/webview.nat", "EditorWebview\nhttps://editorsite1.com\nhttps://editorsite2.com\nhttps://editorsite3.com"); //<- Create a fake fullscreen webview file, to indicate that the fullscreen webview was closed
                    EditorApplication.ExecuteMenuItem("Edit/Play"); //<- force the play mode end in Editor
                    EditorUtility.DisplayDialog("NAT Fullscreen Webview", warningString, "Got It!");
                }
#endif

                webviewExibition.gameObject.SetActive(false);
                webviewExibitionCloseButton.onClick.RemoveAllListeners();
            });

            //Show the webview
            webviewExibition.gameObject.SetActive(true);
        }

        //Utils

        public void RestartApplication(string restartingMessage)
        {
#if UNITY_EDITOR
            EditorApplication.ExecuteMenuItem("Edit/Play"); //<- force the play mode end in Editor
            EditorUtility.DisplayDialog("NAT Utils", "NAT method \"RestartApplication()\" was called. This is a simulation of this method. Play Mode is being stopped. Below is your application restart message.\n\n" + restartingMessage, "Ok");
#endif
        }

        public void EnableAntiScreenshot(bool enable)
        {
            //Simulate the antiscreenshot enabled or disabled
            antiScreenshotEnabled = enable;
        }

        public bool isAntiScreenshotEnabled()
        {
            //Return if the anti screenshot is enabled
            return antiScreenshotEnabled;
        }

        //Location

        public NAT.Location.LocationRunning isTrackingLocation()
        {
            //Return the result
            return locationBeingTracked;
        }

        public bool StartTrackingLocation(NAT.Location.LocationProvider locationProvider)
        {
            //Show that is tracking location
            usingLocationInterface.gameObject.SetActive(true);
            if (locationBeingTracked == NAT.Location.LocationRunning.None)
            {
                if (locationProvider == NAT.Location.LocationProvider.GPS)
                    locationBeingTracked = NAT.Location.LocationRunning.GPS;
                if (locationProvider == NAT.Location.LocationProvider.Network)
                    locationBeingTracked = NAT.Location.LocationRunning.Network;

                locationTrackingEventSimulator = NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(StartTrackingLocationEventSimulation());

                return true;
            }
            return false;
        }

        private IEnumerator StartTrackingLocationEventSimulation()
        {
            yield return new WaitForSecondsRealtime(5.0f);

            while (true)
            {
                if (NATEvents.onLocationChanged != null)
                {
                    if (locationBeingTracked == NAT.Location.LocationRunning.GPS)
                    {
                        NAT.Location.LocationData data = new NAT.Location.LocationData();
                        data.longitude = 37.42212710483604f;
                        data.latitude = -122.0841061387679f;
                        data.addressSubLocalityName = "Googleplex";
                        data.address0Name = "Mountain View";

                        NATEvents.onLocationChanged(NAT.Location.LocationProvider.GPS, data);
                    }
                    if (locationBeingTracked == NAT.Location.LocationRunning.Network)
                    {
                        NAT.Location.LocationData data = new NAT.Location.LocationData();
                        data.longitude = 37.42212710483604f;
                        data.latitude = -122.0841061387679f;
                        data.addressSubLocalityName = "Googleplex";
                        data.address0Name = "Mountain View";

                        NATEvents.onLocationChanged(NAT.Location.LocationProvider.Network, data);
                    }
                }

                yield return new WaitForSecondsRealtime(5.0f);
            }
        }

        public bool StopTrackingLocation()
        {
            //Show that is not tracking location
            usingLocationInterface.gameObject.SetActive(false);
            if (locationBeingTracked != NAT.Location.LocationRunning.None)
            {
                locationBeingTracked = NAT.Location.LocationRunning.None;

                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StopCoroutine(locationTrackingEventSimulator);

                NATEvents.onLocationChanged = null;
                NATEvents.onLocationProviderChanged = null;

                return true;
            }
            return false;
        }

        //Microphone

        public bool isRecordingMicrophone()
        {
            //Return the result
            return recordingMicrophone;
        }

        public void StartRecordingMicrophone()
        {
            if (recordingMicrophone == true || listeningSpeechToText == true)
                return;

            usingMicrophoneInterface.gameObject.SetActive(true);
            recordingMicrophone = true;
        }

        public void StopRecordingMicrophone()
        {
            if (recordingMicrophone == false)
                return;

            usingMicrophoneInterface.gameObject.SetActive(false);
            recordingMicrophone = false;

            if (NATEvents.onMicrophoneStopRecording != null)
                NATEvents.onMicrophoneStopRecording(Application.persistentDataPath + "/NAT/fake-path.aac");
            NATEvents.onMicrophoneStopRecording = null;
        }

        public bool isListeningSpeechToText()
        {
            //Return the result
            return listeningSpeechToText;
        }

        public void StartListeningSpeechToText()
        {
            if (recordingMicrophone == true || listeningSpeechToText == true)
                return;

            listeningSpeechToText = true;

            if (NATEvents.onMicrophoneSpeechToTextStarted != null)
                NATEvents.onMicrophoneSpeechToTextStarted();
            if (NATEvents.onMicrophoneSpeechToTextFinished != null)
                NATEvents.onMicrophoneSpeechToTextFinished(NAT.Microphone.SpeechToTextResult.NoError, "This is a test speech!");

            NATEvents.onMicrophoneSpeechToTextStarted = null;
            NATEvents.onMicrophoneSpeechToTextFinished = null;

            listeningSpeechToText = false;
        }

        //DateTime

        public void OpenHourPicker(string title)
        {
            //Show a hour picker dialog in emulated interface
            datetimeInterface.gameObject.SetActive(true);
            pickerUiInterfaceTitleText.text = title;
            pickerUiInterfaceInputsPlaceHolders[0].text = "type a hour...";
            pickerUiInterfaceInputsPlaceHolders[1].text = "type a minute...";
            pickerUiInterfaceInputsPlaceHolders[2].text = "type a second...";
            pickerUiInterfaceButtonBt.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[0].text) == true)
                    pickerUiInterfaceInputs[0].text = "0";
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[1].text) == true)
                    pickerUiInterfaceInputs[1].text = "0";
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[2].text) == true)
                    pickerUiInterfaceInputs[2].text = "0";

                int hour = int.Parse(pickerUiInterfaceInputs[0].text);
                int minute = int.Parse(pickerUiInterfaceInputs[1].text);
                int second = int.Parse(pickerUiInterfaceInputs[2].text);

                if (hour < 0)
                    hour = 0;
                if (hour > 23)
                    hour = 23;
                if (minute < 0)
                    minute = 0;
                if (minute > 59)
                    minute = 59;
                if (second < 0)
                    second = 0;
                if (second > 59)
                    second = 59;

                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.DateTime_HourPickerDone(hour + "," + minute);

                pickerUiInterfaceInputs[0].text = "";
                pickerUiInterfaceInputs[1].text = "";
                pickerUiInterfaceInputs[2].text = "";

                datetimeInterface.gameObject.SetActive(false);
                pickerUiInterfaceButtonBt.onClick.RemoveAllListeners();
            });
        }

        public void OpenDatePicker(string title)
        {
            //Show a hour picker dialog in emulated interface
            datetimeInterface.gameObject.SetActive(true);
            pickerUiInterfaceTitleText.text = title;
            pickerUiInterfaceInputsPlaceHolders[0].text = "type a year...";
            pickerUiInterfaceInputsPlaceHolders[1].text = "type a month...";
            pickerUiInterfaceInputsPlaceHolders[2].text = "type a day...";
            pickerUiInterfaceButtonBt.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[0].text) == true)
                    pickerUiInterfaceInputs[0].text = "0";
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[1].text) == true)
                    pickerUiInterfaceInputs[1].text = "0";
                if (string.IsNullOrEmpty(pickerUiInterfaceInputs[2].text) == true)
                    pickerUiInterfaceInputs[2].text = "0";

                int year = int.Parse(pickerUiInterfaceInputs[0].text);
                int month = int.Parse(pickerUiInterfaceInputs[1].text);
                int day = int.Parse(pickerUiInterfaceInputs[2].text);

                if (year < 0)
                    year = 0;
                if (month < 1)
                    month = 1;
                if (month > 12)
                    month = 12;
                if (day < 1)
                    day = 1;
                if (day > 31)
                    day = 31;

                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.DateTime_DatePickerDone(year + "," + month + "," + day);

                pickerUiInterfaceInputs[0].text = "";
                pickerUiInterfaceInputs[1].text = "";
                pickerUiInterfaceInputs[2].text = "";

                datetimeInterface.gameObject.SetActive(false);
                pickerUiInterfaceButtonBt.onClick.RemoveAllListeners();
            });
        }

        //Audio Player

        public bool isPlayingAudio()
        {
            return audioPlayerPlaying;
        }

        public void PlayAudio()
        {
            if (audioPlayerPlaying == false)
                NativeAndroidToolkit.GetNativeAndroidToolkitInitBaseParameters().generatedDataHandler.StartCoroutine(SimulateEventAudioPlayerFinishedAudio());
            audioPlayerPlaying = true;
        }

        public void StopAudio()
        {
            audioPlayerPlaying = false;
        }

        private IEnumerator SimulateEventAudioPlayerFinishedAudio()
        {
            yield return new WaitForSeconds(10.0f);

            StopAudio();
            if (NATEvents.onAudioPlayerFinishedPlaying != null)
                NATEvents.onAudioPlayerFinishedPlaying();
        }

        //Simulated Activity

        public void OpenSimulatedActivity(string activityContent)
        {
            //Configure the permissions requester simulated
            simulatedActivityExibitionText.text = activityContent;

            //Configure the button callback
            simulatedActivityExibitionCloseButton.onClick.AddListener(() =>
            {
                simulatedActivityExibition.gameObject.SetActive(false);
                simulatedActivityExibitionCloseButton.onClick.RemoveAllListeners();
            });

            //Show the permission requester
            simulatedActivityExibition.gameObject.SetActive(true);
        }
    }

    //Cache variables
    private float monitorTimingOfTimeOfLastCloseForThisApp = 10.5f;

    //Private variables
    private NAT.DateTime.Calendar calendarOfTimeOfLastCloseForThisApp = new NAT.DateTime.Calendar();
    private NAT.DateTime.Calendar calendarOfTimeOfLastPauseForThisApp = new NAT.DateTime.Calendar();

    //Public variables
    public bool canMonitorTimeOfLastCloseForThisApp = false;
    public EmulatedAndroidInterface emulatedAndroidInterface = new EmulatedAndroidInterface();

#if UNITY_EDITOR
    //Public variables of Interface
    private bool gizmosOfThisComponentIsDisabled = false;

    //The UI of this component
    #region INTERFACE_CODE
    [UnityEditor.CustomEditor(typeof(NativeAndroidToolkitDataHandler))]
    public class CustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            NativeAndroidToolkitDataHandler script = (NativeAndroidToolkitDataHandler)target;
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(target, "Undo Event");
            script.gizmosOfThisComponentIsDisabled = MTAssetsEditorUi.DisableGizmosInSceneView("NativeAndroidDataHandler", script.gizmosOfThisComponentIsDisabled);

            //Warning
            EditorGUILayout.HelpBox("This GameObject is responsible for receiving and transfer responses to Android system, and communicating between your game and the Android system, with the active Native Android Toolkit. Please do not destroy this.", MessageType.Info);

            GUILayout.Space(10);

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                EditorUtility.SetDirty(script);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }
    }
    #endregion
#endif

    //Core methods of this Data Handler. This methods are called by the Unity and provide data for NAT

    public void Update()
    {
        //If can monitor the time of last close of this app, start to monitor
        if (canMonitorTimeOfLastCloseForThisApp == true)
        {
            /*
            / If has passed 10 seconds since the last iteration, store the current time on a file
            / because if the application is closed now, the date and time of the last time that the application
            / was closed will already be recorded and will be read the next time that the NAT is initialized
            */
            if (monitorTimingOfTimeOfLastCloseForThisApp >= 10.0f)
            {
                //Prepare the lines
                string[] lastCloseLines = new string[1];

                //Update the time of calender for time of last close with the current date
                calendarOfTimeOfLastCloseForThisApp.SetThisToCurrentSystemDateTime();

                //Get the time and multiply to 7 to add a salt and store in the file
                lastCloseLines[0] = (calendarOfTimeOfLastCloseForThisApp.GetUnixMillisTime(NAT.DateTime.TimeMode.UtcTime) * 7).ToString();

                //Write the lines and save on the file
                File.WriteAllLines(Application.persistentDataPath + "/NAT/last-close.alc", lastCloseLines);

                //Reset the monitor timing
                monitorTimingOfTimeOfLastCloseForThisApp = 0;
            }

            //Increase the counter with a unscaled delta time to prevent bugs with Time.timeScale
            monitorTimingOfTimeOfLastCloseForThisApp += Time.unscaledDeltaTime;
        }
    }

    public void OnApplicationPause(bool isPaused)
    {
        //When the app is being paused
        if (isPaused == true)
        {
            //Save the time of the app is being paused
            NAT.DateTime.Calendar elapsedRealtimeSinceBoot = NAT.DateTime.GetElapsedRealtimeSinceBoot();
            calendarOfTimeOfLastPauseForThisApp = new NAT.DateTime.Calendar(elapsedRealtimeSinceBoot.Year + 1970, elapsedRealtimeSinceBoot.Month + 1, elapsedRealtimeSinceBoot.Day + 1,
                                                                            elapsedRealtimeSinceBoot.Hour, elapsedRealtimeSinceBoot.Minute, elapsedRealtimeSinceBoot.Second);
            //Force the next iteration of monitor of this app last close, to better accuracy if the app was closed while is paused
            monitorTimingOfTimeOfLastCloseForThisApp = 10.5f;
            Update();
        }
        //When the app is being resumed
        if (isPaused == false)
        {
            //Get the times and do the calcs
            NAT.DateTime.Calendar timeBeforePause = calendarOfTimeOfLastPauseForThisApp;
            NAT.DateTime.Calendar elapsedRealtimeSinceBoot = NAT.DateTime.GetElapsedRealtimeSinceBoot();
            NAT.DateTime.Calendar timeAfterPause = new NAT.DateTime.Calendar(elapsedRealtimeSinceBoot.Year + 1970, elapsedRealtimeSinceBoot.Month + 1, elapsedRealtimeSinceBoot.Day + 1,
                                                                             elapsedRealtimeSinceBoot.Hour, elapsedRealtimeSinceBoot.Minute, elapsedRealtimeSinceBoot.Second);
            NAT.DateTime.Calendar timeDecreaseOperation = new NAT.DateTime.Calendar((timeAfterPause.GetUnixMillisTime(NAT.DateTime.TimeMode.UtcTime) - timeBeforePause.GetUnixMillisTime(NAT.DateTime.TimeMode.UtcTime)))
                                                                            .IncreaseThisIn((System.DateTimeOffset.Now.Offset.Hours * -1), NAT.DateTime.TimeSpanValue.Hours);
            NAT.DateTime.Calendar timeElapsedPaused = timeDecreaseOperation.DecreaseThisWithDate(new NAT.DateTime.Calendar(1970, 1, 1, 0, 0, 0));

            //Create the TimeElapsedWhilePaused object
            NAT.DateTime.TimeElapsedWhilePaused timeElapsedWhilePaused = new NAT.DateTime.TimeElapsedWhilePaused();
            timeElapsedWhilePaused.timeElapsed_accordingRealtimeClockAfterBoot = timeElapsedPaused;

            //If have a listeners registered, call the event now
            if (NATEvents.onDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume != null)
                NATEvents.onDateTimeGetElapsedTimeSinceLastPauseUntilThisResume_PostAppResume(timeElapsedWhilePaused);
        }
    }

    /*
    * Below are methods that work as response receivers on the Java side of the Native Android Toolkit.
    * The methods below are called only by the Java side of the Native Android Toolkit AAR.
    * These methods below usually have the function of calling Native Android Toolkit events, so these
    * events run code of the end user (Native Android Toolkit client developer).
    *
    * All the methods below, after being called by the native Java side, call the NAT Events so that the
    * code registered by the developer is executed.
    * For performance purposes, after the NAT Events are called and the code registered by the developer
    * is executed, the NAT Events are cleared and any code registered in the NAT Events are forgotten.
    */

    //----------------          Alert Dialogs Java Response Receivers         --------------------

    private void SimpleAlertDialog_OkButton()
    {
        if (NATEvents.onSimpleAlertDialogOk != null)
            NATEvents.onSimpleAlertDialogOk();

        //Reset all events after interact with this
        NATEvents.onSimpleAlertDialogCancel = null;
        NATEvents.onSimpleAlertDialogOk = null;
    }
    private void SimpleAlertDialog_Cancel()
    {
        if (NATEvents.onSimpleAlertDialogCancel != null)
            NATEvents.onSimpleAlertDialogCancel();

        //Reset all events after interact with this
        NATEvents.onSimpleAlertDialogCancel = null;
        NATEvents.onSimpleAlertDialogOk = null;
    }

    private void ConfirmationDialog_YesButton()
    {
        if (NATEvents.onConfirmationDialogYes != null)
            NATEvents.onConfirmationDialogYes();

        //Reset all events after interact with this
        NATEvents.onConfirmationDialogYes = null;
        NATEvents.onConfirmationDialogNo = null;
        NATEvents.onConfirmationDialogCancel = null;
    }
    private void ConfirmationDialog_NoButton()
    {
        if (NATEvents.onConfirmationDialogNo != null)
            NATEvents.onConfirmationDialogNo();

        //Reset all events after interact with this
        NATEvents.onConfirmationDialogYes = null;
        NATEvents.onConfirmationDialogNo = null;
        NATEvents.onConfirmationDialogCancel = null;
    }
    private void ConfirmationDialog_Cancel()
    {
        if (NATEvents.onConfirmationDialogCancel != null)
            NATEvents.onConfirmationDialogCancel();

        //Reset all events after interact with this
        NATEvents.onConfirmationDialogYes = null;
        NATEvents.onConfirmationDialogNo = null;
        NATEvents.onConfirmationDialogCancel = null;
    }

    private void NeutralDialog_YesButton()
    {
        if (NATEvents.onNeutralDialogYes != null)
            NATEvents.onNeutralDialogYes();

        //Reset all events after interact with this
        NATEvents.onNeutralDialogYes = null;
        NATEvents.onNeutralDialogNo = null;
        NATEvents.onNeutralDialogNeutral = null;
        NATEvents.onNeutralDialogCancel = null;
    }
    private void NeutralDialog_NoButton()
    {
        if (NATEvents.onNeutralDialogNo != null)
            NATEvents.onNeutralDialogNo();

        //Reset all events after interact with this
        NATEvents.onNeutralDialogYes = null;
        NATEvents.onNeutralDialogNo = null;
        NATEvents.onNeutralDialogNeutral = null;
        NATEvents.onNeutralDialogCancel = null;
    }
    private void NeutralDialog_NeutralButton()
    {
        if (NATEvents.onNeutralDialogNeutral != null)
            NATEvents.onNeutralDialogNeutral();

        //Reset all events after interact with this
        NATEvents.onNeutralDialogYes = null;
        NATEvents.onNeutralDialogNo = null;
        NATEvents.onNeutralDialogNeutral = null;
        NATEvents.onNeutralDialogCancel = null;
    }
    private void NeutralDialog_CancelButton()
    {
        if (NATEvents.onNeutralDialogCancel != null)
            NATEvents.onNeutralDialogCancel();

        //Reset all events after interact with this
        NATEvents.onNeutralDialogYes = null;
        NATEvents.onNeutralDialogNo = null;
        NATEvents.onNeutralDialogNeutral = null;
        NATEvents.onNeutralDialogCancel = null;
    }

    private void RadialListDialog_Done(string result)
    {
        if (NATEvents.onRadialListDialogDone != null)
            NATEvents.onRadialListDialogDone(int.Parse(result));

        //Reset all events after interact with this
        NATEvents.onRadialListDialogDone = null;
        NATEvents.onRadialListDialogCancel = null;
    }
    private void RadialListDialog_Cancel()
    {
        if (NATEvents.onRadialListDialogCancel != null)
            NATEvents.onRadialListDialogCancel();

        //Reset all events after interact with this
        NATEvents.onRadialListDialogDone = null;
        NATEvents.onRadialListDialogCancel = null;
    }

    private void CheckboxListDialog_Done(string result)
    {
        string[] resultsStr = result.Split(',');
        bool[] resultsBool = new bool[resultsStr.Length];
        for (int i = 0; i < resultsBool.Length; i++)
            resultsBool[i] = bool.Parse(resultsStr[i]);

        if (NATEvents.onCheckboxListDialogDone != null)
            NATEvents.onCheckboxListDialogDone(resultsBool);

        //Reset all events after interact with this
        NATEvents.onCheckboxListDialogDone = null;
        NATEvents.onCheckboxListDialogCancel = null;
    }
    private void CheckboxListDialog_Cancel()
    {
        if (NATEvents.onCheckboxListDialogCancel != null)
            NATEvents.onCheckboxListDialogCancel();

        //Reset all events after interact with this
        NATEvents.onCheckboxListDialogDone = null;
        NATEvents.onCheckboxListDialogCancel = null;
    }

    //----------------             Webview Java Response Receivers            --------------------

    private void WebviewChromium_OnPopUpWebviewClose()
    {
        if (NATEvents.onPopUpWebviewClose != null)
            NATEvents.onPopUpWebviewClose(new NAT.Webview.WebviewBrowsing(Application.persistentDataPath + "/NAT/webview.nat")); //<- It is not necessary to check the file existence or if it has content as this event will always
                                                                                                                                 //   be called after the popup webview closes (and having created a webview file)

        //Reset all events after interact with this
        NATEvents.onPopUpWebviewClose = null;
    }

    //----------------             Location Java Response Receivers            --------------------

    private void Location_Changed(string result)
    {
        if (NATEvents.onLocationChanged != null)
        {
            //Read the result
            string[] resultSplitted = result.Split('|');
            string provider = resultSplitted[0];

            //Create the data object
            NAT.Location.LocationData data = new NAT.Location.LocationData();
            data.bearing = float.Parse(resultSplitted[1]);
            data.bearingAccuracyInDegrees = float.Parse(resultSplitted[2]);
            data.longitude = double.Parse(resultSplitted[3]);
            data.latitude = double.Parse(resultSplitted[4]);
            data.horizontalAccuracyInMeters = float.Parse(resultSplitted[5]);
            data.verticalAccuracyInMeters = float.Parse(resultSplitted[6]);
            data.speedInMetersPerSecond = float.Parse(resultSplitted[7]);
            data.speedAcurracyInMetersPerSecond = float.Parse(resultSplitted[8]);
            data.fixTimeNanos = long.Parse(resultSplitted[9]);
            data.timeMillis = long.Parse(resultSplitted[10]);
            data.isMock = bool.Parse(resultSplitted[11]);
            data.addressSubLocalityName = resultSplitted[12];
            data.address0Name = resultSplitted[13];
            data.address1Name = resultSplitted[14];
            data.address2Name = resultSplitted[15];
            data.isFirstAndCacheLocation = bool.Parse(resultSplitted[16]);
            //Create others variables
            data.coordinates = new Vector2((float)data.longitude, (float)data.latitude);

            if (provider == "gps")
                NATEvents.onLocationChanged(NAT.Location.LocationProvider.GPS, data);
            if (provider == "network")
                NATEvents.onLocationChanged(NAT.Location.LocationProvider.Network, data);
        }

        //The events of Location class will be reseted on StopTrackingLocation() call
    }
    private void Location_EnabledDisabled(string result)
    {
        if (NATEvents.onLocationProviderChanged != null)
        {
            string[] resultSplitted = result.Split('|');

            if (resultSplitted[0] == "gps")
                NATEvents.onLocationProviderChanged(NAT.Location.LocationProvider.GPS, bool.Parse(resultSplitted[1]));
            if (resultSplitted[0] == "network")
                NATEvents.onLocationProviderChanged(NAT.Location.LocationProvider.Network, bool.Parse(resultSplitted[1]));
        }

        //The events of Location class will be reseted on StopTrackingLocation() call
    }

    private void Location_GoogleMapsLoaded()
    {
        if (NATEvents.onGoogleMapsLoaded != null)
            NATEvents.onGoogleMapsLoaded();

        //The events of Location class will be reseted on map be closed
    }
    private void Location_GoogleMapsClick(string result)
    {
        if (NATEvents.onGoogleMapsClick != null)
        {
            string[] splittedResult = result.Split('|');

            NATEvents.onGoogleMapsClick(double.Parse(splittedResult[0]), double.Parse(splittedResult[1]));
        }

        //The events of Location class will be reseted on map be closed
    }
    private void Location_GoogleMapsClosed()
    {
        if (NATEvents.onGoogleMapsClosed != null)
            NATEvents.onGoogleMapsClosed();

        //Reset all google maps events on close the map
        NATEvents.onGoogleMapsLoaded = null;
        NATEvents.onGoogleMapsClick = null;
        NATEvents.onGoogleMapsClosed = null;
    }

    //----------------             Camera Java Response Receivers            --------------------

    private void Camera_CameraTakePhoto(string result)
    {
        if (NATEvents.onCameraTakePhoto != null)
        {
            Texture2D texture = null;
            byte[] fileData;
            fileData = System.IO.File.ReadAllBytes(result);
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            NATEvents.onCameraTakePhoto(texture);
        }

        //Reset all camera events on close the camera popup
        NATEvents.onCameraTakePhoto = null;
        NATEvents.onCameraReadCode = null;
        NATEvents.onCameraRecorded = null;
        NATEvents.onCameraClose = null;
    }
    private void Camera_CameraReadCode(string result)
    {
        if (NATEvents.onCameraReadCode != null)
            NATEvents.onCameraReadCode(result);

        //Reset all camera events on close the camera popup
        NATEvents.onCameraTakePhoto = null;
        NATEvents.onCameraReadCode = null;
        NATEvents.onCameraRecorded = null;
        NATEvents.onCameraClose = null;
    }
    private void Camera_CameraRecorded(string result)
    {
        if (NATEvents.onCameraRecorded != null)
            NATEvents.onCameraRecorded(result);

        //Reset all camera events on close the camera popup
        NATEvents.onCameraTakePhoto = null;
        NATEvents.onCameraReadCode = null;
        NATEvents.onCameraRecorded = null;
        NATEvents.onCameraClose = null;
    }
    private void Camera_CameraClosed()
    {
        if (NATEvents.onCameraClose != null)
        {
            NAT.Camera.OnCameraClose(); //<- Run internal pre-events on close camera, before call the event
            NATEvents.onCameraClose();
        }

        //Reset all camera events on close the camera popup
        NATEvents.onCameraTakePhoto = null;
        NATEvents.onCameraReadCode = null;
        NATEvents.onCameraRecorded = null;
        NATEvents.onCameraClose = null;
    }

    //----------------             Microphone Java Response Receivers            --------------------

    private void Microphone_MicrophoneRecordingStopped(string result)
    {
        if (NATEvents.onMicrophoneStopRecording != null)
            NATEvents.onMicrophoneStopRecording(result);

        //Reset all microphone events on stop the audio recording
        NATEvents.onMicrophoneStopRecording = null;
    }

    private void Microphone_MicrophoneSpeechToTextStarted()
    {
        if (NATEvents.onMicrophoneSpeechToTextStarted != null)
            NATEvents.onMicrophoneSpeechToTextStarted();

        //Reset the microphone event
        NATEvents.onMicrophoneSpeechToTextStarted = null;
    }
    private void Microphone_MicrophoneSpeechToTextFinished(string result)
    {
        if (NATEvents.onMicrophoneSpeechToTextFinished != null)
        {
            //Get the response
            string[] resultParts = result.Split(new string[] { "||" }, System.StringSplitOptions.None);

            //If have error
            if (int.Parse(resultParts[0]) >= 0)
            {
                //Prepare the variables to store error data
                int errorCode = int.Parse(resultParts[0]);
                NAT.Microphone.SpeechToTextResult error = NAT.Microphone.SpeechToTextResult.NoError;

                //Get the error
                switch (errorCode)
                {
                    case 0:
                        error = NAT.Microphone.SpeechToTextResult.DidntUnderstand;
                        break;
                    case 1:
                        error = NAT.Microphone.SpeechToTextResult.AudioRecordingError;
                        break;
                    case 2:
                        error = NAT.Microphone.SpeechToTextResult.ClientSideError;
                        break;
                    case 3:
                        error = NAT.Microphone.SpeechToTextResult.InsufficientPermissions;
                        break;
                    case 4:
                        error = NAT.Microphone.SpeechToTextResult.NetworkError;
                        break;
                    case 5:
                        error = NAT.Microphone.SpeechToTextResult.NetworkTimeOut;
                        break;
                    case 6:
                        error = NAT.Microphone.SpeechToTextResult.NoMatch;
                        break;
                    case 7:
                        error = NAT.Microphone.SpeechToTextResult.RecognitionServiceBusy;
                        break;
                    case 8:
                        error = NAT.Microphone.SpeechToTextResult.ErrorFromServer;
                        break;
                    case 9:
                        error = NAT.Microphone.SpeechToTextResult.NoSpeechInput;
                        break;
                    case 10:
                        error = NAT.Microphone.SpeechToTextResult.LanguageNotSupported;
                        break;
                    case 11:
                        error = NAT.Microphone.SpeechToTextResult.LanguageUnvailable;
                        break;
                    case 12:
                        error = NAT.Microphone.SpeechToTextResult.ErrorServerDisconnected;
                        break;
                    case 13:
                        error = NAT.Microphone.SpeechToTextResult.ErrorServerTooManyRequests;
                        break;
                }

                //Return the result calling the event
                NATEvents.onMicrophoneSpeechToTextFinished(error, "");
            }
            //If not have error, call the event returning the result
            if (int.Parse(resultParts[0]) == -1)
                NATEvents.onMicrophoneSpeechToTextFinished(NAT.Microphone.SpeechToTextResult.NoError, resultParts[1]);
        }

        //Reset the microphone event
        NATEvents.onMicrophoneSpeechToTextFinished = null;
    }

    //----------------             DateTime Java Response Receivers            --------------------

    private void DateTime_HourPickerDone(string result)
    {
        string[] resultSplited = result.Split(',');

        if (NATEvents.onDateTimeHourPicked != null)
            NATEvents.onDateTimeHourPicked(new NAT.DateTime.Calendar(0, 0, 0, int.Parse(resultSplited[0]), int.Parse(resultSplited[1]), 0));

        //Reset all events after interact with this
        NATEvents.onDateTimeHourPicked = null;
    }
    private void DateTime_DatePickerDone(string result)
    {
        string[] resultSplited = result.Split(',');

        if (NATEvents.onDateTimeDatePicked != null)
            NATEvents.onDateTimeDatePicked(new NAT.DateTime.Calendar(int.Parse(resultSplited[0]), int.Parse(resultSplited[1]), int.Parse(resultSplited[2]), 0, 0, 0));

        //Reset all events after interact with this
        NATEvents.onDateTimeDatePicked = null;
    }

    private void DateTime_DoneNTPRequest(string result)
    {
        string[] resultParts = result.Split(',');
        long resultTime = long.Parse(resultParts[0]);
        string resultServer = resultParts[1];

        if (resultTime == -1)
            if (NATEvents.onDateTimeDoneNTPRequest != null)
                NATEvents.onDateTimeDoneNTPRequest(false, new NAT.DateTime.Calendar().SetThisToZero(), "");
        if (resultTime != -1)
            if (NATEvents.onDateTimeDoneNTPRequest != null)
                NATEvents.onDateTimeDoneNTPRequest(true, new NAT.DateTime.Calendar(resultTime), resultServer);

        //Reset all events after interact with this
        NATEvents.onDateTimeDoneNTPRequest = null;
    }

    //----------------             Files Java Response Receivers            --------------------

    private void Files_FilePickerOperationFinished(string result)
    {
        //Read the result
        string[] resultSplitted = result.Split('<');

        //Create the response
        NAT.Files.FilePickerOperationStatus statusCode = NAT.Files.FilePickerOperationStatus.Unknown;
        NAT.Files.FilePickerOperationResponse response = null;
        if (resultSplitted[0] == "-1")
            statusCode = NAT.Files.FilePickerOperationStatus.Unknown;
        if (resultSplitted[0] == "0")
            statusCode = NAT.Files.FilePickerOperationStatus.Successfully;
        if (resultSplitted[0] == "1")
            statusCode = NAT.Files.FilePickerOperationStatus.Canceled;
        if (resultSplitted[0] == "2")
            statusCode = NAT.Files.FilePickerOperationStatus.InvalidScope;
        if (statusCode == NAT.Files.FilePickerOperationStatus.Successfully)
        {
            response = new NAT.Files.FilePickerOperationResponse();
            if (resultSplitted[1] == "0")
                response.operationType = NAT.Files.FilePickerAction.CreateFile;
            if (resultSplitted[1] == "1")
                response.operationType = NAT.Files.FilePickerAction.OpenFile;
            switch (resultSplitted[2])
            {
                case "0":
                    response.scopeOfOperation = NAT.Files.Scope.AppFiles;
                    break;
                case "1":
                    response.scopeOfOperation = NAT.Files.Scope.AppCache;
                    break;
                case "2":
                    response.scopeOfOperation = NAT.Files.Scope.DCIM;
                    break;
                case "3":
                    response.scopeOfOperation = NAT.Files.Scope.Documents;
                    break;
                case "4":
                    response.scopeOfOperation = NAT.Files.Scope.Downloads;
                    break;
                case "5":
                    response.scopeOfOperation = NAT.Files.Scope.Movies;
                    break;
                case "6":
                    response.scopeOfOperation = NAT.Files.Scope.Music;
                    break;
                case "7":
                    response.scopeOfOperation = NAT.Files.Scope.Pictures;
                    break;
                case "8":
                    response.scopeOfOperation = NAT.Files.Scope.Podcasts;
                    break;
                case "9":
                    response.scopeOfOperation = NAT.Files.Scope.Recordings;
                    break;
                case "10":
                    response.scopeOfOperation = NAT.Files.Scope.Ringtones;
                    break;
                case "11":
                    response.scopeOfOperation = NAT.Files.Scope.Screenshots;
                    break;
            }
            response.pathFromOperationInScope = resultSplitted[3].Replace("\r", "").Replace("\n", "");
        }

        if (NATEvents.onFilesFilePickerOperationFinished != null)
            NATEvents.onFilesFilePickerOperationFinished(statusCode, response);

        //Reset the file picker operation result event
        NATEvents.onFilesFilePickerOperationFinished = null;
    }

    //----------------             AudioPlayer Java Response Receivers            --------------------

    private void AudioPlayer_AudioPlayerFinishedPlaying()
    {
        if (NATEvents.onAudioPlayerFinishedPlaying != null)
            NATEvents.onAudioPlayerFinishedPlaying();

        //Reset the audio player event
        NATEvents.onAudioPlayerFinishedPlaying = null;
    }

    //----------------             PlayGames Java Response Receivers            --------------------

    private void PlayGames_InitializationComplete(string result)
    {
        //Read the result
        string[] resultSplitted = result.Split('₢');

        //Create the response
        bool userSignedIn = (resultSplitted[0] == "true") ? true : false;
        NAT.PlayGames.UserData userData = null;
        if (userSignedIn == true)
        {
            userData = new NAT.PlayGames.UserData();
            userData.lastUserDataUpdate = new NAT.DateTime.Calendar(long.Parse(resultSplitted[1]));
            userData.displayName = resultSplitted[2];
            userData.title = resultSplitted[3];
            userData.playerId = resultSplitted[4];
            userData.hasIconImage = bool.Parse(resultSplitted[5]);
            userData.iconImagePathInAppFilesScope = resultSplitted[6];
            userData.currentLevel = int.Parse(resultSplitted[7]);
            userData.currentLevelMinXp = long.Parse(resultSplitted[8]);
            userData.currentLevelMaxXp = long.Parse(resultSplitted[9]);
            userData.currentXpTotal = long.Parse(resultSplitted[10]);
            userData.lastLevelUp = new NAT.DateTime.Calendar(long.Parse(resultSplitted[11]));
            userData.nextLevel = int.Parse(resultSplitted[12]);
            userData.nextLevelMinXp = long.Parse(resultSplitted[13]);
            userData.nextLevelMaxXp = long.Parse(resultSplitted[14]);
            userData.isMaxLevel = bool.Parse(resultSplitted[15]);
        }

        if (NATEvents.onPlayGamesInitializationComplete != null)
            NATEvents.onPlayGamesInitializationComplete(userSignedIn, userData);

        //Reset the play games event
        NATEvents.onPlayGamesInitializationComplete = null;
    }

    private void PlayGames_EventDataLoaded(string result)
    {
        //Read the result
        string[] resultSplitted = result.Split('₢');

        //Create the response
        bool isSuccessfully = (resultSplitted[0] == "true") ? true : false;
        NAT.PlayGames.EventData eventData = null;
        if (isSuccessfully == true)
        {
            eventData = new NAT.PlayGames.EventData();
            eventData.name = resultSplitted[1];
            string eventIdString = resultSplitted[2];
            int correspondingEventIdInPlayGamesResources = 0;
            foreach (string eventIdInResources in GPGSLowLevelResources.events)
            {
                //If is equal, save as this index
                if (eventIdInResources == eventIdString)
                    break;

                //Increase the index
                correspondingEventIdInPlayGamesResources += 1;
            }
            try { eventData.id = (PlayGamesResources.Event)System.Enum.ToObject(typeof(PlayGamesResources.Event), correspondingEventIdInPlayGamesResources); }
            catch (System.Exception e) { Debug.LogError("Error on convert ID of Play Games Event... The Play Games Resources XML of NAT is updated?\n" + e.ToString()); }
            eventData.description = resultSplitted[3];
            eventData.formattedValue = resultSplitted[4];
            eventData.unformattedValue = long.Parse(resultSplitted[5]);
            eventData.isVisible = bool.Parse(resultSplitted[6]);
            eventData.iconImagePathInAppFilesScope = resultSplitted[7];
        }


        if (NATEvents.onPlayGamesEventDataLoaded != null)
            NATEvents.onPlayGamesEventDataLoaded(isSuccessfully, eventData);

        //Reset the play games event
        NATEvents.onPlayGamesEventDataLoaded = null;
    }

    private void PlayGames_DoneFriendListAccessRequest(string result)
    {
        if (NATEvents.onPlayGamesFriendListRequestResult != null)
            NATEvents.onPlayGamesFriendListRequestResult((result == "1") ? true : false);

        //Reset the play games event
        NATEvents.onPlayGamesFriendListRequestResult = null;
    }
    private void PlayGames_UserFriendListLoaded(string result)
    {
        //Read the result
        string[] friendsStrings = new string[0];
        if (result != "")
            friendsStrings = result.Split('|');

        //Create the response
        List<NAT.PlayGames.Friend> allFriends = new List<NAT.PlayGames.Friend>();
        foreach (string friendString in friendsStrings)
        {
            //Split values
            string[] friendInfo = friendString.Split('₢');

            //Create the friend
            NAT.PlayGames.Friend friend = new NAT.PlayGames.Friend();
            friend.lastUserDataUpdate = new NAT.DateTime.Calendar(long.Parse(friendInfo[0]));
            friend.displayName = friendInfo[1];
            friend.title = friendInfo[2];
            friend.playerId = friendInfo[3];
            friend.currentLevel = int.Parse(friendInfo[4]);
            friend.hasIconImage = bool.Parse(friendInfo[5]);
            friend.iconImagePathInAppFilesScope = friendInfo[6];
            allFriends.Add(friend);
        }
        NAT.PlayGames.FriendList friendList = new NAT.PlayGames.FriendList();
        friendList.allUserFriends = allFriends.ToArray();



        if (NATEvents.onPlayGamesUserFriendListLoaded != null)
            NATEvents.onPlayGamesUserFriendListLoaded(friendList);

        //Reset the play games event
        NATEvents.onPlayGamesUserFriendListLoaded = null;
    }

    private void PlayGames_CloudSaveUIResult(string result)
    {
        //Read the result
        string[] resultSplitted = result.Split('₢');

        NAT.PlayGames.CloudSaveUIResponse resultStatus = NAT.PlayGames.CloudSaveUIResponse.Error;
        switch (resultSplitted[0])
        {
            case "0":
                resultStatus = NAT.PlayGames.CloudSaveUIResponse.Error;
                break;
            case "1":
                resultStatus = NAT.PlayGames.CloudSaveUIResponse.Canceled;
                break;
            case "2":
                resultStatus = NAT.PlayGames.CloudSaveUIResponse.LoadSave;
                break;
            case "3":
                resultStatus = NAT.PlayGames.CloudSaveUIResponse.CreateNew;
                break;
        }

        if (NATEvents.onPlayGamesCloudSaveUIResult != null)
            NATEvents.onPlayGamesCloudSaveUIResult(resultStatus, resultSplitted[1]);

        //Reset the play games event
        NATEvents.onPlayGamesCloudSaveUIResult = null;
    }
    private void PlayGames_CloudSaveCreateFileResult(string result)
    {
        if (NATEvents.onPlayGamesCloudSaveCreateFileResult != null)
            NATEvents.onPlayGamesCloudSaveCreateFileResult(bool.Parse(result));

        //Reset the play games event
        NATEvents.onPlayGamesCloudSaveCreateFileResult = null;
    }
    private void PlayGames_CloudSaveReadFileResult(string result)
    {
        NAT.PlayGames.CloudSaveLoadStatus loadStatus = NAT.PlayGames.CloudSaveLoadStatus.Unknown;
        switch (result)
        {
            case "0":
                loadStatus = NAT.PlayGames.CloudSaveLoadStatus.ErrorOnFind;
                break;
            case "1":
                loadStatus = NAT.PlayGames.CloudSaveLoadStatus.ErrorOnRead;
                break;
            case "2":
                loadStatus = NAT.PlayGames.CloudSaveLoadStatus.Success;
                break;
        }

        byte[] saveLoadedBytes = NAT.Files.LoadAllBytesOfFile(NAT.Files.Scope.AppFiles, "NAT/GPGS/tempLoadFile.gps");
        NAT.Files.Delete(NAT.Files.Scope.AppFiles, "NAT/GPGS/tempLoadFile.gps");

        if (NATEvents.onPlayGamesCloudSaveReadFileResult != null)
            NATEvents.onPlayGamesCloudSaveReadFileResult(loadStatus, saveLoadedBytes);

        //Reset the play games event
        NATEvents.onPlayGamesCloudSaveReadFileResult = null;
    }
}