using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LidarV2DepthCamera : MonoBehaviour {

	public Cubemap cubemap;

	public RawImage ri;
	// public void Awake()
	// {
	// 	GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	// 	GetComponent<Camera>().targetTexture.format = RenderTextureFormat.Depth;
	// }
	Texture2D tex;

	void Start() {
		tex = new Texture2D(128, 128);
		ri.texture = tex;

	}

	void Update()
	{
		GetComponent<Camera>().RenderToCubemap(cubemap);

		Debug.Log(cubemap.texelSize);

		Graphics.CopyTexture(cubemap, 0, 0, 0, 0, 128, 128, tex, 0, 0, 0, 0);
		tex.Apply();
	}
}
