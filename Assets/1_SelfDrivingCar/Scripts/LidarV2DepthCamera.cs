using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LidarV2DepthCamera : MonoBehaviour {

    public Material material;
	public RawImage ri;
 
	public void Awake() {
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		GetComponent<Camera>().targetTexture.format = RenderTextureFormat.Depth;
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }

	void Update()
	{
		//Graphics.CopyTexture(cubemap, 0, 0, 0, 0, 128, 128, tex, 0, 0, 0, 0);
		//tex.Apply();
	}
}

public class CRTEffect : MonoBehaviour
{
}