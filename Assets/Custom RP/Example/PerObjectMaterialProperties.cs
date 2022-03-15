
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor"),
    alphaCutOffId = Shader.PropertyToID("_Cutoff");


    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    float alphaCutOff;

    static MaterialPropertyBlock block;

    void OnValidate()
    {

        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        block.SetColor(baseColorId, baseColor);
        block.SetFloat(alphaCutOffId, alphaCutOff);
        GetComponent<Renderer>().SetPropertyBlock(block);

    }


    void Awake()
    {
        OnValidate();
    }


}
