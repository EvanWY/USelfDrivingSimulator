using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LidarV2DepthCamera : MonoBehaviour {

    public Material material;
 
	// public void Awake() {
	// 	GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	// 	GetComponent<Camera>().targetTexture.format = RenderTextureFormat.Depth;
	// }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
		material.SetFloat("_Fov", GetComponent<Camera>().fieldOfView);
		Graphics.Blit(source, destination, material);
	}
}