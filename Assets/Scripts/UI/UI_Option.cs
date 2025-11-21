using UnityEngine;
using UnityEngine.UI;

public class UI_Option : MonoBehaviour
{
    [SerializeField] Slider MasterSlider;
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SFXSlider;


    public void SetMasterVolume()
    {
        AudioManager.Instance.SetMasterVolume(MasterSlider.value);
    }
    public void SetBGMVolume()
    {
        AudioManager.Instance.SetBGMVolume(BGMSlider.value);
    }
    public void SetSFXVolume()
    {
        AudioManager.Instance.SetSFXVolume(SFXSlider.value);
    }
    
}
