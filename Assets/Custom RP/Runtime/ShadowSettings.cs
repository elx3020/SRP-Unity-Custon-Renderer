using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSettings
{

    // max distance to draw shadows
    [Min(0f)]
    public float maxDistance = 100f;

    // enum containing the texture resolution for the shadow map 
    public enum TextureSize
    {
        _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8192 = 8192
    }

    // atlas size regarding only direction lights

    [System.Serializable]
    public struct Directional
    {
        public TextureSize atlasSize;

    }

    // shadow map entry point 

    public Directional directional = new Directional { atlasSize = TextureSize._1024 };




}
