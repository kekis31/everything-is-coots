using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            PlayerPrefs.SetFloat("MasterVolume", 1);
            GameObject.Find("VolumeSlider").GetComponent<Slider>().value = 1;
        }
        else
        {
            GameObject.Find("VolumeSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVolume");
        }

        SoundManager.instance.PlaySound("miukumauku_main", 0.15f, 1, true, SoundManager.SoundType.Music);
    }

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");

        SoundManager.instance.StopAllSounds();
    }

    public void UpdateVolume()
    {
        PlayerPrefs.SetFloat("MasterVolume", GameObject.Find("VolumeSlider").GetComponent<Slider>().value);

        SoundManager.instance.UpdateVolume();
    }
}
