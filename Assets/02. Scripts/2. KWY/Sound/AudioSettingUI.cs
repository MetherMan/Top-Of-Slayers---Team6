using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private Image bgmButtonImage;
    [SerializeField] private Image sfxButtonImage;
    [SerializeField] private Sprite onSoundSprite;
    [SerializeField] private Sprite offSoundSprite;

    private float lastBGMValue = 1f;
    private float lastSFXValue = 1f;

    private bool isBGMMute = false;
    private bool isSFXMute = false;


    private void Start()
    {
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        audioManager.BGMSoundVolume(bgmSlider.value);
        audioManager.SFXSoundVolume(sfxSlider.value);
    }

    private void OnBGMChanged(float val)
    {
        audioManager.BGMSoundVolume(val);
    }
    private void OnSFXChanged(float val)
    {
        audioManager.SFXSoundVolume(val);
    }

    public void ToggleBGM()
    {
        if (!isBGMMute)
        {
            lastBGMValue = bgmSlider.value;
            bgmSlider.value = 0.001f;
            isBGMMute = true;
            bgmButtonImage.sprite = offSoundSprite;
        }
        else
        {
            bgmSlider.value = lastBGMValue;
            isBGMMute = false;
            bgmButtonImage.sprite = onSoundSprite;
        }
    }
    public void ToggleSFX()
    {
        if (!isSFXMute)
        {
            lastSFXValue = sfxSlider.value;
            sfxSlider.value = 0.001f;
            isSFXMute = true;
            sfxButtonImage.sprite = offSoundSprite;
        }
        else
        {
            sfxSlider.value = lastSFXValue;
            isSFXMute = false;
            sfxButtonImage.sprite = onSoundSprite;
        }
    }
}
