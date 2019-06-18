using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntialiasingScript : MonoBehaviour
{
    public Material mat;
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //draws the pixels from the source texture to the destination texture
        var temporaryTexture = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, temporaryTexture, mat, 0);
        Graphics.Blit(temporaryTexture, destination, mat, 1);
        RenderTexture.ReleaseTemporary(temporaryTexture);
    }
}
