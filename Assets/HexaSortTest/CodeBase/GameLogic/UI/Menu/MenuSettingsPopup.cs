using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public sealed class MenuSettingsPopup : MonoBehaviour
  {
    [SerializeField] private SettingsButton _openButton;
    [SerializeField] private GameObject _popup;
    [SerializeField] private SettingsButton _closeButton;
    [SerializeField] private Slider _master;
    [SerializeField] private Slider _music;
    [SerializeField] private Slider _sfx;
    [SerializeField] private SettingsButton _fps30;
    [SerializeField] private SettingsButton _fps60;
    [SerializeField] private SettingsButton _fpsUnlimited;
    [SerializeField] private TMP_Text _masterValue;
    [SerializeField] private TMP_Text _musicValue;
    [SerializeField] private TMP_Text _sfxValue;

    private void OnEnable()
    {
      _popup.SetActive(false);
      _openButton.OnSettingsButtonClick += Open;
      _closeButton.OnSettingsButtonClick += Close;
      _master.onValueChanged.AddListener(SetMaster);
      _music.onValueChanged.AddListener(SetMusic);
      _sfx.onValueChanged.AddListener(SetSfx);
      _fps30.OnSettingsButtonClick += Set30;
      _fps60.OnSettingsButtonClick += Set60;
      _fpsUnlimited.OnSettingsButtonClick += SetUnlimited;
    }
    private void OnDisable()
    {
      _openButton.OnSettingsButtonClick -= Open;
      _closeButton.OnSettingsButtonClick -= Close;
      _master.onValueChanged.RemoveListener(SetMaster);
      _music.onValueChanged.RemoveListener(SetMusic);
      _sfx.onValueChanged.RemoveListener(SetSfx);
      _fps30.OnSettingsButtonClick -= Set30;
      _fps60.OnSettingsButtonClick -= Set60;
      _fpsUnlimited.OnSettingsButtonClick -= SetUnlimited;
      _popup.SetActive(false);
      PlayerSettings.Save();
    }
    private void Update()
    {
      if (_popup.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close();
    }
    private void OnApplicationPause(bool paused) { if (paused) PlayerSettings.Save(); }
    private void OnApplicationFocus(bool focused) { if (!focused) PlayerSettings.Save(); }
    private void Open()
    {
      _master.SetValueWithoutNotify(PlayerSettings.Master);
      _music.SetValueWithoutNotify(PlayerSettings.Music);
      _sfx.SetValueWithoutNotify(PlayerSettings.Sfx);
      RefreshLabels();
      RefreshFps();
      _popup.SetActive(true);
    }
    private void Close() { _popup.SetActive(false); PlayerSettings.Save(); }
    private void SetMaster(float value) { PlayerSettings.SetVolume("Master", value); RefreshLabels(); }
    private void SetMusic(float value) { PlayerSettings.SetVolume("Music", value); RefreshLabels(); }
    private void SetSfx(float value) { PlayerSettings.SetVolume("Sfx", value); RefreshLabels(); }
    private void RefreshLabels()
    {
      _masterValue.text = Mathf.RoundToInt(PlayerSettings.Master * 100) + "%";
      _musicValue.text = Mathf.RoundToInt(PlayerSettings.Music * 100) + "%";
      _sfxValue.text = Mathf.RoundToInt(PlayerSettings.Sfx * 100) + "%";
    }
    private void Set30() => SetFps(30);
    private void Set60() => SetFps(60);
    private void SetUnlimited() => SetFps(-1);
    private void SetFps(int value) { PlayerSettings.SetFps(value); RefreshFps(); PlayerSettings.Save(); }
    private void RefreshFps()
    {
      _fps30.Button.interactable = PlayerSettings.Fps != 30;
      _fps60.Button.interactable = PlayerSettings.Fps != 60;
      _fpsUnlimited.Button.interactable = PlayerSettings.Fps != -1;
    }
  }
}
