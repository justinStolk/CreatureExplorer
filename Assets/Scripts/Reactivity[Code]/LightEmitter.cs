using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    [SerializeField] private Light lightSource;

    [SerializeField] private float dimmingDuration;
    [SerializeField] private float fullIntensity;

    [Header("Timed Lighting")]
    [field: HideArrow, SerializeField] private bool timedLighting;
    [field: ConditionalHide("timedLighting", true), SerializeField] private DistantLands.Cozy.MeridiemTime dimTime;
    [field: ConditionalHide("timedLighting", true), SerializeField] private DistantLands.Cozy.MeridiemTime brightenTime;
    private bool isDimmed;

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

    //TODO: figure out what's wrong with cozyweather events and use events
    private void Update()
    {
        if (timedLighting)
        {
            if (!isDimmed && DistantLands.Cozy.CozyWeather.instance.currentTime.IsBetweenTimes(dimTime, brightenTime))
            {
                DimLight();
            }
            else if (isDimmed && DistantLands.Cozy.CozyWeather.instance.currentTime.IsBetweenTimes(brightenTime, dimTime))
            {
                BrightenLight();
            }
        }
    }

    public void DimLight()
    {
        StartCoroutine(ChangeIntensity(lightSource.intensity, 0));
        isDimmed = true;
    }

    public void BrightenLight()
    {
        StartCoroutine(ChangeIntensity(lightSource.intensity, fullIntensity));
        isDimmed = false;
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
