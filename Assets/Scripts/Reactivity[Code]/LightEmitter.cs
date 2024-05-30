using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    [SerializeField] private Light lightSource;

    [SerializeField] private float dimmingDuration;
    [SerializeField] private float fullIntensity;

    // Start is called before the first frame update
    void Start()
    {
        if (lightSource == null)
        {
            gameObject.TryGetComponent(out lightSource);
        }

        if (fullIntensity == 0)
        {
            fullIntensity = lightSource.intensity;
        }
    }

    public void DimLight()
    {
        StartCoroutine(ChangeIntensity(lightSource.intensity, 0));
    }

    public void BrightenLight()
    {
        StartCoroutine(ChangeIntensity(lightSource.intensity, fullIntensity));
    }

    private IEnumerator ChangeIntensity(float from, float to)
    {
        float timer = 0;

        while (timer <= dimmingDuration)
        {
            lightSource.intensity = Mathf.Lerp(from, to, timer/dimmingDuration);

            timer += Time.deltaTime;
            yield return null;
        }

        lightSource.intensity = to;
    }
}
