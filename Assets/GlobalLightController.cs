using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class GlobalLightController : MonoBehaviour
{
    public static GlobalLightController Instance;

    public Light2D globalLight;
    public float fadeSpeed = 0.5f;
    public float targetIntensity = 1f;

    private bool isFading = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isFading)
        {
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);

            if (Mathf.Abs(globalLight.intensity - targetIntensity) < 0.02f)
                isFading = false;
        }
    }

    public void FadeInLight()
    {
        isFading = true;
    }
}
