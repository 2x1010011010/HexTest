using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.SoundLogic
{
  // Independent of scene lifetime and gameplay progress resets.
  public static class PlayerSettings
  {
    public static float Master => ReadVolume("Master");
    public static float Music => ReadVolume("Music");
    public static float Sfx => ReadVolume("Sfx");
    public static int Fps
    {
      get
      {
        int value = PlayerPrefs.GetInt("Settings.Fps", 60);
        return value == 30 || value == 60 || value == -1 ? value : 60;
      }
    }
    private static float ReadVolume(string key)
    {
      float value = PlayerPrefs.GetFloat("Settings." + key, 1f);
      return float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
    }
    public static void SetVolume(string channel, float value)
    {
      PlayerPrefs.SetFloat("Settings." + channel, Mathf.Clamp01(value));
      AudioFacade.Instance?.ApplyVolumes();
    }
    public static void SetFps(int value)
    {
      if (value != 30 && value != 60 && value != -1) return;
      PlayerPrefs.SetInt("Settings.Fps", value);
      ApplyFrameRate();
    }
    public static void ApplyFrameRate()
    {
      QualitySettings.vSyncCount = 0;
      int target = Fps;
      // On mobile -1 means the platform default (usually 30), not unlimited.
      if (target == -1 && Application.isMobilePlatform)
      {
        int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        target = refreshRate > 0 ? refreshRate : 60;
      }
      Application.targetFrameRate = target;
    }
    public static void Save() => PlayerPrefs.Save();
  }
}
