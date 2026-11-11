using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace HexaSortTest.CodeBase.GameLogic.SoundLogic
{
  public class AudioFacade : MonoBehaviour
  {
    public static AudioFacade Instance { get; private set; }
    
    [Header( "Audio Sources" )]
    [SerializeField, BoxGroup("MUSIC SOURCE")] private AudioSource _musicSource;
    [SerializeField, BoxGroup("SOUND EFFECTS SOURCE")] private AudioSource _soundEffectsSource;
    [SerializeField, BoxGroup("MIXER")] private AudioMixer _mixer;
    
    [SerializeField, BoxGroup("SOUNDS LIST")] private AudioClip[] _sounds;

    private bool _musicEnabled = true;
    private bool _fxEnabled = true;
    
    private const string MusicGroup = "Music";
    private const string FxGroup = "FX";
    
    public bool IsFXEnabled => _fxEnabled;
    public bool IsMusicEnabled => _musicEnabled;

    private void Awake()
    {
      if (Instance != null && Instance != this) { Destroy(gameObject); return; }
      Instance = this;
    }

    private void Start() => ApplyVolumes();

    private void OnDestroy()
    {
      if (Instance == this) Instance = null;
    }

    public void ApplyVolumes()
    {
      AudioListener.volume = PlayerSettings.Master;
      float music = _musicEnabled ? PlayerSettings.Music : 0f;
      float fx = _fxEnabled ? PlayerSettings.Sfx : 0f;
      if (_mixer != null)
      {
        _mixer.SetFloat(MusicGroup, ToDecibels(music));
        _mixer.SetFloat(FxGroup, ToDecibels(fx));
      }
      else
      {
        if (_musicSource != null) _musicSource.volume = music;
        if (_soundEffectsSource != null) _soundEffectsSource.volume = fx;
      }
    }

    private static float ToDecibels(float volume) => volume <= 0f ? -80f : Mathf.Log10(volume) * 20f;

    public void PlayClick() => PlayFX(_sounds[0]);
    public void PlayOpen() => PlayFX(_sounds[1]);
    public void PlayClose() => PlayFX(_sounds[2]);
    public void PlaySort() => PlayFX(_sounds[3]);
    public void PlaySpawn() => PlayFX(_sounds[4]);

    public void SetMusicEnabled(bool enabled)
    {
      _musicEnabled = enabled;
      ApplyVolumes();
    }

    public void SetFXEnabled(bool enabled)
    {
      _fxEnabled = enabled;
      ApplyVolumes();
    }
    
    private void PlayFX(AudioClip clip) => 
      _soundEffectsSource.PlayOneShot(clip);
  }
}