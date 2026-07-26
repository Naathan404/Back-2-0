using MeowgaByte.Core;
using UnityEngine;


public class WarmUpBGM : MonoBehaviour
{
    [SerializeField] private AudioClip au;
    [SerializeField] private float volume = 1f;

    private void Start()
    {
        if (AudioManager.Instance?.CurrentBGM == au) return;

        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlayMusic(au, volume);
    }
}
