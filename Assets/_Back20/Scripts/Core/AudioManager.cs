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

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }

        public void PlaySFX(AudioClip sfx, bool randomPitch = false, bool isOverrided = false, float volume = 1f)
        {
            SFX.volume = volume;

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

            SFX.volume = 1f;
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

            if(Music.clip == music) return;
            Music.clip = music;
            Music.Play();
        }

        public void StopMusic()
        {
            if (Music.isPlaying)
            {
                Music.Stop();
                CurrentBGM = null;
            }
        }

        public void PlayTimerTick(AudioClip sfx)
        {
            if (TimerSource.isPlaying) return; // Nếu đang chạy thì không bắt đầu lại

            TimerSource.clip = sfx;
            TimerSource.loop = true; // 🌟 QUAN TRỌNG: để nó lặp lại liên tục
            TimerSource.Play();
        }

        public void StopTimerTick()
        {
            if (TimerSource.isPlaying)
            {
                TimerSource.Stop();
            }
        }
    }    
}
