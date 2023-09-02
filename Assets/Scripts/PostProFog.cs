using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostProFog : MonoBehaviour
{
	[Range(0.0f, 3.0f)]
	[Header("雾的浓度")] public float FogDensity = 1.0f;
	[Header("雾的起始高度")] public float FogStart = 0.0f;
	[Header("雾的终止高度")] public float FogEnd = 2.0f;
	[Header("雾的颜色")] public Color FogColor = new Color(130 / 255f, 130 / 255f, 130 / 255f, 1.0f);
	[Header("雾的后处理Shader")] public Shader FogShader;
	[Header("雾的噪声图")] public Texture NoiseTexture;
	[Range(-0.5f, 0.5f)]
	[Header("雾X方向流动速度")] public float FogXSpeed = 0.05f;
	[Range(-0.5f, 0.5f)]
	[Header("雾Z方向流动速度")] public float FogZSpeed = 0.05f;
	[Range(0.0f, 3.0f)]
	[Header("雾噪声图参数")] public float NoiseAmount = 1.0f;

	private Camera mCurrentCamera;
	public Camera CurrentCamera
	{
		get
		{
			if (mCurrentCamera == null)
			{
				mCurrentCamera = GetComponent<Camera>();
			}
			return mCurrentCamera;
		}
	}

	private Transform mCameraTransform;
	public Transform CameraTransform
	{
		get
		{
			if (mCameraTransform == null)
			{
				mCameraTransform = GetComponent<Transform>();
			}
			return mCameraTransform;
		}
	}

	private Material mFogMaterial = null;
	public Material FogMaterial
	{
		get
		{
            if (mFogMaterial == null)
            {
				mFogMaterial = CreateMaterial(FogShader);
			}			
			return mFogMaterial;
		}
	}

	void OnEnable()
	{
		CurrentCamera.depthTextureMode |= DepthTextureMode.Depth;
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
        if (FogMaterial == null)
        {
			Graphics.Blit(src, dest);
			return;
		}

		Matrix4x4 frustumCorners = Matrix4x4.identity;

		float fov = CurrentCamera.fieldOfView;
		float near = CurrentCamera.nearClipPlane;
		float aspect = CurrentCamera.aspect;

		float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
		Vector3 toRight = CameraTransform.right * halfHeight * aspect;
		Vector3 toTop = CameraTransform.up * halfHeight;

		Vector3 topLeft = CameraTransform.forward * near + toTop - toRight;
		float scale = topLeft.magnitude / near;

		topLeft.Normalize();
		topLeft *= scale;

		Vector3 topRight = CameraTransform.forward * near + toRight + toTop;
		topRight.Normalize();
		topRight *= scale;

		Vector3 bottomLeft = CameraTransform.forward * near - toTop - toRight;
		bottomLeft.Normalize();
		bottomLeft *= scale;

		Vector3 bottomRight = CameraTransform.forward * near + toRight - toTop;
		bottomRight.Normalize();
		bottomRight *= scale;

		frustumCorners.SetRow(0, bottomLeft);
		frustumCorners.SetRow(1, bottomRight);
		frustumCorners.SetRow(2, topRight);
		frustumCorners.SetRow(3, topLeft);

		FogMaterial.SetMatrix("_FrustumCornersRay", frustumCorners);
		FogMaterial.SetFloat("_FogDensity", FogDensity);
		FogMaterial.SetColor("_FogColor", FogColor);
		FogMaterial.SetFloat("_FogStart", FogStart);
		FogMaterial.SetFloat("_FogEnd", FogEnd);
		FogMaterial.SetTexture("_NoiseTex", NoiseTexture);
		FogMaterial.SetFloat("_FogXSpeed", FogXSpeed);
		FogMaterial.SetFloat("_FogZSpeed", FogZSpeed);
		FogMaterial.SetFloat("_NoiseAmount", NoiseAmount);

		Graphics.Blit(src, dest, FogMaterial);
	}

	private Material CreateMaterial(Shader shader)
	{
		Material material = new Material(shader);
		material.hideFlags = HideFlags.DontSave;
		return material;
	}

}
