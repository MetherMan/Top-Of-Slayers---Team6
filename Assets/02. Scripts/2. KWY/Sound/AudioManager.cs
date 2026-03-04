using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSound;
    [SerializeField] private AudioSource bgmSound;
    [SerializeField] private AudioMixer mixer;

    public void SFXSoundVolume(float val)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(val) * 20);
    }
    public void PlaySFX(AudioClip clip)
    {
        sfxSound.Stop();
        sfxSound.clip = clip;
        sfxSound.Play();
    }
    public void BGMSoundVolume(float val)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(val) * 20);
    }

    public void BGMSoundPlay(AudioClip clip)
    {
        bgmSound.clip = clip;
        bgmSound.loop = true;
        bgmSound.Play();
    }

}
