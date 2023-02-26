using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    public CinemachineVirtualCamera cam;
    public Volume postProcessing;
    private float tempNoise;
    private float constantNoise;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        tempNoise -= Time.deltaTime;
        tempNoise = Mathf.Clamp(tempNoise, 0, Mathf.Infinity);

        cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = tempNoise + constantNoise;
    }

    public void SetTempNoise(float value)
    {
        if (tempNoise > value)
            return;

        tempNoise = value;
    }

    public void SetConstantNoise(float value)
    {
        constantNoise = value;
    }

    public void SetLensDistortion(float value)
    {
        UnityEngine.Rendering.Universal.LensDistortion distortion;

        if (!postProcessing.profile.TryGet(out distortion)) throw new System.NullReferenceException(nameof(distortion));

        distortion.intensity.value = value;
    }
}
