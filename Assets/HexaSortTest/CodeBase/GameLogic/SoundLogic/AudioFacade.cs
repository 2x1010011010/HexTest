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
      if (Instance != null) Destroy(gameObject);
      Instance = this;
    }

    public void PlayClick() => PlayFX(_sounds[0]);
    public void PlayOpen() => PlayFX(_sounds[1]);
    public void PlayClose() => PlayFX(_sounds[2]);
    public void PlaySort() => PlayFX(_sounds[3]);
    public void PlaySpawn() => PlayFX(_sounds[4]);

    public void SetMusicEnabled(bool enabled)
    {
      _musicEnabled = enabled;
      _mixer.SetFloat(MusicGroup, enabled ? 0f : -80f);
    }

    public void SetFXEnabled(bool enabled)
    {
      _fxEnabled = enabled;
      _mixer.SetFloat(FxGroup, enabled ? 0f : -80f);
    }


    private void PlayFX(AudioClip clip) => 
      _soundEffectsSource.PlayOneShot(clip);
    
  }
}