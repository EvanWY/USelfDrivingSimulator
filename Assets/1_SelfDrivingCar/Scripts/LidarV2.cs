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
	
	int CloudWidth;

	//public Queue<Texture2D> imageQueue = new Queue<Texture2D>();
	public Texture2D lastImage;
	public Texture2D nextImage;

	public RawImage rawImage;

	public Texture2D scaledImage;

	void Start()
	{
		CloudWidth = Mathf.RoundToInt(SampleFrequency / RotateFrequency);
		lastImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);
		nextImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);

		rawImage.texture = lastImage;
	}

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

	int nextStartColumns = 0;
	int frameRenderCounter = 0;
	int frameActualRenderTimes = 0;

	void Update()
	{
		int sampleCount = Mathf.FloorToInt(SampleFrequency * Time.deltaTime);

		// theta is the angle of the diag
		float currCamTheta = Mathf.Rad2Deg * Mathf.Atan((Mathf.Tan(Mathf.Deg2Rad * DepthCamera.fieldOfView / 2) / Mathf.Sqrt(2f)));

		frameRenderCounter = 0;
		frameActualRenderTimes = 0;
		Render(ref nextImage, ref nextStartColumns, ref sampleCount, currCamTheta);
		
		while (sampleCount > 0) {
			nextImage.Apply();
			lastImage = nextImage;
			nextImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);
			Render(ref nextImage, ref nextStartColumns, ref sampleCount, currCamTheta);
		}

		Debug.LogFormat("DeltaTime:{0}, RenderTimes:{1}, ActualRenderTiems:{2}", Time.deltaTime, frameRenderCounter, frameActualRenderTimes);
	}

	// return successfully rendered fragment width
	void Render(ref Texture2D targetImage, ref int imgHorizontalPixelStart, ref int sampleCount, float currCamTheta) {
		frameRenderCounter++;
		float maxCamRenderHorizontalAngle = DepthCamera.fieldOfView;
		int maxCamRenderWidth = Mathf.FloorToInt((maxCamRenderHorizontalAngle / 360) * CloudWidth);


		while (maxCamRenderWidth < sampleCount && imgHorizontalPixelStart + maxCamRenderWidth < CloudWidth) {
			// render a whole camera
			ExecuteRender(ref targetImage, maxCamRenderWidth, ref imgHorizontalPixelStart, ref sampleCount, maxCamRenderWidth, currCamTheta);
		}

		int renderWidth = Mathf.Min(sampleCount, CloudWidth - imgHorizontalPixelStart);
		ExecuteRender(ref targetImage, renderWidth, ref imgHorizontalPixelStart, ref sampleCount, maxCamRenderWidth, currCamTheta);

	}

	void ExecuteRender(ref Texture2D targetImage,int renderWidth, ref int imgHorizontalPixelStart, ref int sampleCount, int maxCamRenderWidth, float currCamTheta) {
		frameActualRenderTimes++;
		DepthCamera.transform.localEulerAngles = Vector3.up * Mathf.LerpUnclamped(0, 360, (imgHorizontalPixelStart + 0.5f * renderWidth) / (float)(CloudWidth));
		DepthCamera.Render();
		Texture2D readRenderTex = new Texture2D(DepthCamera.targetTexture.width, DepthCamera.targetTexture.height, TextureFormat.RGB24, false);
		
		RenderTexture.active = DepthCamera.targetTexture;
		readRenderTex.ReadPixels(new Rect(0, 0, DepthCamera.targetTexture.width, DepthCamera.targetTexture.height), 0, 0);
		readRenderTex.Apply();

		int maxCamRenderHeight = Mathf.RoundToInt(2 * currCamTheta * Channels / (MaximalVerticalFOV - MinimalVerticalFOV));
		//Debug.Log(maxCamRenderWidth + " " + maxCamRenderHeight + " " + currCamTheta);
		scaledImage = TextureScaler.scaled(readRenderTex, maxCamRenderWidth, maxCamRenderHeight);
		scaledImage.Apply();

		int srcX = (maxCamRenderWidth - renderWidth) / 2;
		int srcY = Mathf.RoundToInt(maxCamRenderHeight * (MinimalVerticalFOV + currCamTheta) / (currCamTheta + currCamTheta));
		Graphics.CopyTexture(scaledImage, 0, 0, srcX, srcY, renderWidth, Channels, targetImage, 0, 0, imgHorizontalPixelStart, 0);
		
		//Debug.Log(srcX + " " + srcY + " " + renderWidth + " " + imgHorizontalPixelStart);

		sampleCount -= renderWidth;
		imgHorizontalPixelStart += renderWidth;
		imgHorizontalPixelStart %= CloudWidth;
	}

}


