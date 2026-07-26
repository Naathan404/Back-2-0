using UnityEngine;
using UnityEngine.Audio;

namespace MeowgaByte.Core
{
    public class AudioManager : MonoSingleton<AudioManager>
    {   
        [Header("Audio Sources")]
        [SerializeField] private AudioMixer _audioMixer;
        public AudioSource SFX;
        public AudioSource Music;
        public AudioSource TimerSource;


        [Header("Gameplay SFX")]
        public AudioClip SelectCommandSFX;
        public AudioClip DropFailCommandSFX;
        public AudioClip DropSuccessCommandSFX;
        public AudioClip HitCommandSFX;
        public AudioClip JumpSFX;
        public AudioClip SignSFX;
        public AudioClip FootstepSFX;

        public AudioClip GameStartSFX;
        public AudioClip WinSFX;
        public AudioClip FailSFX;


        [Header("Music")]
        public AudioClip MenuBGM;
        public AudioClip GameplayBGM;

        [Header("UI Interact")]
        public AudioClip ButtonClick;
        public AudioClip SceenTransition;
        public AudioClip Hover;
        public AudioClip InputFieldClick;

        [HideInInspector] public AudioClip CurrentBGM; 
        [HideInInspector] public AudioClip CurrentSFX; 

        public bool IsSFXMuted = false;
        public bool IsBGMMuted = false;

        public const string PREF_SFX_MUTED = "Audio_IsSFXMuted";
        public const string PREF_BGM_MUTED = "Audio_IsBGMMuted";

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
            LoadAudioSettings();
        }

        private void LoadAudioSettings()
        {
            IsSFXMuted = PlayerPrefs.GetInt(PREF_SFX_MUTED, 0) == 1;
            IsBGMMuted = PlayerPrefs.GetInt(PREF_BGM_MUTED, 0) == 1;

            ApplyBGMMuteState();
        }

        private void ApplyBGMMuteState()
        {
            Music.volume = IsBGMMuted ? 0f : 0.1f;
        }

        public void PlaySFX(AudioClip sfx, bool randomPitch = false, bool isOverrided = false, float volume = 1f)
        {
            SFX.volume = volume;
            if (IsSFXMuted) SFX.volume = 0;

            if(randomPitch)
            {
                SFX.pitch = Random.Range(0.8f, 1.2f);
            }
            else
            {
                SFX.pitch = 1f;
            }

            if(isOverrided)
            {
                SFX.Stop();
                // play sfx
                SFX.clip = sfx;
                SFX.Play();
            }
            else
            {
                SFX.PlayOneShot(sfx);
            }
        }

        public void StopSFX()
        {
            if(SFX.isPlaying)
                SFX.Stop();
        }

        public void PlayMusic(AudioClip music, float volume = 1f)
        {
            Music.volume = volume;
            CurrentBGM = music;
            if (IsBGMMuted) Music.volume = 0;

            if(Music.clip == music) return;
            Music.clip = music;
            Music.Play();
        }

        public void StopMusic()
        {
            Music.Stop();
            CurrentBGM = null;
        }

        public void PlayTimerTick(AudioClip sfx)
        {
            if (TimerSource.isPlaying) return; 

            TimerSource.clip = sfx;
            TimerSource.loop = true; 
            TimerSource.Play();
        }

        public void StopTimerTick()
        {
            if (TimerSource.isPlaying)
            {
                TimerSource.Stop();
            }
        }

        public void ToggleMuteSFX()
        {
            IsSFXMuted = !IsSFXMuted;
            PlayerPrefs.SetInt(PREF_SFX_MUTED, IsSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ToggleMuteBGM()
        {
            IsBGMMuted = !IsBGMMuted;
            PlayerPrefs.SetInt(PREF_BGM_MUTED, IsBGMMuted ? 1 : 0);
            PlayerPrefs.Save();

            ApplyBGMMuteState();
        }
    }    
}
