using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class GameBoy : MonoBehaviour
{
    public Material identityMaterial;
    public Material gameBoyMaterial;

    //private RenderTexture _downscaledRenderTexture;

        /*
    private void OnEnable()
    {
        var camera = GetComponent<Camera>();
        int height = 144;
        int width = Mathf.RoundToInt(camera.aspect * height);
        _downscaledRenderTexture = new RenderTexture(width, height, 16);
        _downscaledRenderTexture.filterMode = FilterMode.Point;
    }

    private void OnDisable()
    {
        Destroy(_downscaledRenderTexture);
    }
    */

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, gameBoyMaterial);
        //Graphics.Blit(_downscaledRenderTexture, dst, identityMaterial);
    }
}
