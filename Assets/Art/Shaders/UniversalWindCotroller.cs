using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalWindCotroller : MonoBehaviour
{
    public Texture2D WindTexture;
    public float variableMultiplier = 1f;
    public float WindStrength;
    float offsetX = 0f;
    float offsetY = 0f;

    void Update()
    {
        // Calculate texture offset based on time or other factor
        //float offsetX = Time.time * scrollSpeedX;
        //float offsetY = Time.time * scrollSpeedY;

        // Sample the texture at the calculated offset
        Color sampledColor = WindTexture.GetPixelBilinear(offsetX, offsetY);
        float WindStrength = sampledColor.grayscale * variableMultiplier;
    }
}
