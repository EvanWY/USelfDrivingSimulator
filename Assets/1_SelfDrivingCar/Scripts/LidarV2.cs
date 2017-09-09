using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LidarV2 : MonoBehaviour
{
	public Camera DepthCamera;
	public LidarV2DepthCamera LidarV2DepthCameraObject;

	public float RotateFrequency = 20;


	public float SampleFrequency = 20000;

	public int Channels = 64;
	public float MaximalVerticalFOV = +0.2f;
	public float MinimalVerticalFOV = -24.9f;


	public float MeasurementRange = 120f;
	public float MeasurementAccuracy = 0.02f;

	public int SupersampleScale = 2;

	int CloudWidth;

	//public Queue<Texture2D> imageQueue = new Queue<Texture2D>();
	public Texture2D lastImage;
	public Texture2D nextImage;

	public RawImage rawImage;

	public Texture2D scaledImage;

	int nextStartColumns = 0;
	int frameRenderCounter = 0;
	int frameActualRenderTimes = 0;

	float currCamTheta;
	int maxCamRenderWidth;
	int maxCamRenderHeight;

	public bool TryRenderPointCloud(out byte[] image)
	{
		if (lastImage == null) {
			image = null;
			return false;
		}

		image = lastImage.EncodeToJPG();
		lastImage = null;
		return true;
	}

	void Start() {
		
		CloudWidth = Mathf.RoundToInt(SampleFrequency / RotateFrequency);
		lastImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);
		nextImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);

		currCamTheta = Mathf.Rad2Deg * Mathf.Atan((Mathf.Tan(Mathf.Deg2Rad * DepthCamera.fieldOfView / 2) / Mathf.Sqrt(2f)));
		maxCamRenderWidth = Mathf.FloorToInt((DepthCamera.fieldOfView / 360) * CloudWidth);
		maxCamRenderHeight = Mathf.RoundToInt(2 * currCamTheta * Channels / (MaximalVerticalFOV - MinimalVerticalFOV));
		DepthCamera.targetTexture = new RenderTexture(SupersampleScale * maxCamRenderWidth, SupersampleScale * maxCamRenderHeight, 24);
		DepthCamera.targetTexture.Create();
		DepthCamera.aspect = 1;

		LidarV2DepthCameraObject.Fov = DepthCamera.fieldOfView;
		LidarV2DepthCameraObject.SupersampleScale = SupersampleScale;
	}

	void Update()
	{
		int sampleCount = Mathf.FloorToInt(SampleFrequency * Time.deltaTime);

		// theta is the angle of the diag

		frameRenderCounter = 0;
		frameActualRenderTimes = 0;
		Render(ref nextImage, ref nextStartColumns, ref sampleCount);
		
		while (sampleCount > 0) {
			nextImage.Apply();
			lastImage = nextImage;
			rawImage.texture = lastImage;
			nextImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);
			Render(ref nextImage, ref nextStartColumns, ref sampleCount);
		}

		Debug.LogFormat("DeltaTime:{0}, RenderTimes:{1}, ActualRenderTiems:{2}", Time.deltaTime, frameRenderCounter, frameActualRenderTimes);
	}

	// return successfully rendered fragment width
	void Render(ref Texture2D targetImage, ref int imgHorizontalPixelStart, ref int sampleCount) {
		frameRenderCounter++;


		while (maxCamRenderWidth < sampleCount && imgHorizontalPixelStart + maxCamRenderWidth < CloudWidth) {
			// render a whole camera
			ExecuteRender(ref targetImage, maxCamRenderWidth, ref imgHorizontalPixelStart, ref sampleCount);
		}

		int renderWidth = Mathf.Min(sampleCount, CloudWidth - imgHorizontalPixelStart);
		ExecuteRender(ref targetImage, renderWidth, ref imgHorizontalPixelStart, ref sampleCount);

	}


	void ExecuteRender(ref Texture2D targetImage,int renderWidth, ref int imgHorizontalPixelStart, ref int sampleCount) {
		
		// Rotate Camera to target angle and render
		DepthCamera.transform.localEulerAngles = Vector3.up * Mathf.LerpUnclamped(0, 360, (imgHorizontalPixelStart + 0.5f * renderWidth) / (float)(CloudWidth));
		DepthCamera.Render();

		// copy camera render texture to "readRenderTex"
		Texture2D readRenderTex = new Texture2D(maxCamRenderWidth, maxCamRenderHeight, TextureFormat.RGB24, false);
		RenderTexture.active = DepthCamera.targetTexture;
		readRenderTex.ReadPixels(new Rect(maxCamRenderWidth * (SupersampleScale *0.5f - 0.5f), maxCamRenderHeight * (SupersampleScale *0.5f - 0.5f), maxCamRenderWidth, maxCamRenderHeight), 0, 0);
		readRenderTex.Apply();

		// copy texture from "readRenderTex" to related area in "targetImage"
		int srcX = (maxCamRenderWidth - renderWidth) / 2;
		int srcY = Mathf.RoundToInt(maxCamRenderHeight * (MinimalVerticalFOV + currCamTheta) / (currCamTheta + currCamTheta));
		Graphics.CopyTexture(readRenderTex, 0, 0, srcX, srcY, renderWidth, Channels, targetImage, 0, 0, imgHorizontalPixelStart, 0);

		sampleCount -= renderWidth;
		imgHorizontalPixelStart += renderWidth;
		imgHorizontalPixelStart %= CloudWidth;
		frameActualRenderTimes++;
	}

}


