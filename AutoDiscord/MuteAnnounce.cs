using System.Collections.Generic;
using NAudio.CoreAudioApi;
using UnityEngine;
using UnityEngine.UI;

namespace AutoDiscord;

public class MuteAnnounce {
    public GameObject gameObject;
    public List<AudioSessionControl> mutedSessions;

    public MuteAnnounce(List<AudioSessionControl> mutedSessions) {
        this.mutedSessions = mutedSessions;
        gameObject = new GameObject("MuteAnnounce");
        CreateCanvas();
        gameObject.AddComponent<GraphicRaycaster>();
        CreateBackground();
        GameObject popupObject = CreatePopupObject();
        CreateText(popupObject);
        AddButton(popupObject, "Yes", Yes, -140);
        AddButton(popupObject, "No", No, 140);
    }

    private void CreateCanvas() {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 12345;
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
    }

    private void CreateBackground() {
        GameObject backgroundPanel = new("BackgroundPanel");
        RectTransform panelTransform = backgroundPanel.AddComponent<RectTransform>();
        backgroundPanel.transform.SetParent(gameObject.transform, false);
        panelTransform.sizeDelta = new Vector2(600, 300);
        panelTransform.anchoredPosition = Vector2.zero;
        Image panelImage = backgroundPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
    }

    private GameObject CreatePopupObject() {
        GameObject popupObject = new("Popup");
        RectTransform popupTransform = popupObject.AddComponent<RectTransform>();
        popupObject.transform.SetParent(gameObject.transform, false);
        popupTransform.sizeDelta = new Vector2(600, 300);
        popupTransform.anchoredPosition = Vector2.zero;
        return popupObject;
    }

    private static void CreateText(GameObject popupObject) {
        GameObject textObject = new("Popup Text");
        RectTransform textTransform = textObject.AddComponent<RectTransform>();
        textObject.transform.SetParent(popupObject.transform, false);
        textTransform.anchoredPosition = new Vector2(0, 50);
        textTransform.sizeDelta = new Vector2(600, 300);
        Text text = textObject.AddComponent<Text>();
        text.font = RDString.GetFontDataForLanguage(RDString.language).font;
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = Main.Instance.Localization["MuteAnnounce.Text"];
    }

    private static void AddButton(GameObject popupObject, string buttonText, UnityEngine.Events.UnityAction buttonAction, float x) {
        GameObject buttonObject = new(buttonText + " Button");
        buttonObject.transform.SetParent(popupObject.transform, false);
        RectTransform buttonTransform = buttonObject.AddComponent<RectTransform>();
        Text buttonTextComponent = buttonObject.AddComponent<Text>();
        buttonTextComponent.text = Main.Instance.Localization["MuteAnnounce." + buttonText];
        buttonTextComponent.font = RDString.GetFontDataForLanguage(RDString.language).font;
        buttonTextComponent.fontSize = 40;
        buttonTextComponent.alignment = TextAnchor.MiddleCenter;
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(buttonAction);
        buttonTransform.sizeDelta = new Vector2(150, 50);
        buttonTransform.anchoredPosition = new Vector2(x, -100);
    }

    private void Yes() {
        foreach(AudioSessionControl session in mutedSessions) session.SimpleAudioVolume.Mute = false;
        No();
    }

    private void No() {
        Object.Destroy(gameObject);
    }
}