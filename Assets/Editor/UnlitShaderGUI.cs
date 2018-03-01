using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitShaderGUI : ShaderGUI
{
	public enum RenderingMode
	{
		Opaque = 0,
		Cutout = 1,
		Transparent = 2,
	}

	bool m_firstTime = false;

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		Material mat = materialEditor.target as Material;

		if(!m_firstTime)
		{
			m_firstTime = true;
			MaterialChanged(mat);
		}

		EditorGUI.BeginChangeCheck();
		base.OnGUI (materialEditor, properties);
		if (EditorGUI.EndChangeCheck())
		{
			MaterialChanged(materialEditor.target as Material);
		}
	}

	void MaterialChanged(Material mat)
	{
		bool noTexture = mat.GetTexture("_MainTex") == null;
		bool noMask = mat.GetTexture("_Mask") == null;
		bool noColor = mat.GetColor("_Color") == Color.white;
		EnableKeyword(mat, "_TEXTURE_OFF", noTexture);
		EnableKeyword(mat, "_MASK_OFF", noMask);
		EnableKeyword(mat, "_COLOR_OFF", noColor);

		RenderingMode renderingMode = (RenderingMode)mat.GetInt("_RenderingMode");

		switch (renderingMode)
		{
			case RenderingMode.Opaque:
				mat.SetOverrideTag("RenderType", "");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				EnableKeyword(mat, "_ALPHATEST_ON", false);
				mat.renderQueue = -1;
				break;

			case RenderingMode.Cutout:
				mat.SetOverrideTag("RenderType", "TransparentCutout");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				EnableKeyword(mat, "_ALPHATEST_ON", true);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
				break;

			case RenderingMode.Transparent:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt("_ZWrite", 0);
				EnableKeyword(mat, "_ALPHATEST_ON", false);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;
		}
	}

	static void EnableKeyword(Material material, string keyword, bool enable)
	{
		if(enable)
			material.EnableKeyword(keyword);
		else
			material.DisableKeyword(keyword);
	}
}
