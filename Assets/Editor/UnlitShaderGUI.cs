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

	public const string RenderingModePropName = "_RenderingMode";
	public const string MainTexPropName = "_MainTex";
	public const string MaskPropName = "_Mask";
	public const string ColorPropName = "_Color";
	public const string CutoffPropName = "_Cutoff";
	public const string SrcBlendPropName = "_SrcBlend";
	public const string DstBlendPropName = "_DstBlend";
	public const string ZWritePropName = "_ZWrite";

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
		DrawProperties (materialEditor, properties);
		if (EditorGUI.EndChangeCheck())
		{
			MaterialChanged(materialEditor.target as Material);
		}
	}

	void DrawProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		materialEditor.SetDefaultGUIWidths();

		MaterialProperty renderingModeProp = FindProperty(RenderingModePropName, properties);
		materialEditor.ShaderProperty(renderingModeProp, renderingModeProp.displayName);

		MaterialProperty mainTexProp = FindProperty(MainTexPropName, properties);
		materialEditor.ShaderProperty(mainTexProp, mainTexProp.displayName);

		MaterialProperty maskProp = FindProperty(MaskPropName, properties);
		bool maskScaleOffset = true;
		materialEditor.TextureProperty(maskProp, maskProp.displayName, maskScaleOffset);

		MaterialProperty colorProp = FindProperty(ColorPropName, properties);
		materialEditor.ShaderProperty(colorProp, colorProp.displayName);

		if((RenderingMode)renderingModeProp.floatValue == RenderingMode.Cutout)
		{
			MaterialProperty cutoffProp = FindProperty(CutoffPropName, properties);
			materialEditor.ShaderProperty(cutoffProp, cutoffProp.displayName);
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		materialEditor.RenderQueueField();
		materialEditor.EnableInstancingField();
		materialEditor.DoubleSidedGIField();
	}

	void MaterialChanged(Material mat)
	{
		bool noTexture = mat.GetTexture(MainTexPropName) == null;
		Vector2 textureScale = noTexture ? Vector2.one : mat.GetTextureScale(MainTexPropName);
        Vector2 textureOffset = noTexture ? Vector2.zero : mat.GetTextureOffset(MainTexPropName);
		bool noTextureScale = textureScale == Vector2.one;
        bool noTextureOffset = textureOffset == Vector2.zero;
        bool noTextureScaleOffset = noTextureScale && noTextureOffset;
		bool noMask = mat.GetTexture(MaskPropName) == null;
		Vector2 maskScale = mat.GetTextureScale(MaskPropName);
        Vector2 maskOffset = mat.GetTextureOffset(MaskPropName);
		bool noMaskScale = maskScale == textureScale;
		bool noMaskOffset = maskOffset == textureOffset;
        bool noMaskScaleOffset = noMaskScale && noMaskOffset;
		bool noColor = mat.GetColor(ColorPropName) == Color.white;
		EnableKeyword(mat, "_TEXTURE_OFF", noTexture);
		EnableKeyword(mat, "_TEXTURE_SCALE_OFFSET_OFF", noTextureScaleOffset && !noTexture);
		EnableKeyword(mat, "_MASK_OFF", noMask);
		EnableKeyword(mat, "_MASK_SCALE_OFFSET_OFF", noMaskScaleOffset && !noMask);
		EnableKeyword(mat, "_COLOR_OFF", noColor);

		RenderingMode renderingMode = (RenderingMode)mat.GetInt(RenderingModePropName);

		switch (renderingMode)
		{
			case RenderingMode.Opaque:
				mat.SetOverrideTag("RenderType", "");
				mat.SetInt(SrcBlendPropName, (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt(DstBlendPropName, (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt(ZWritePropName, 1);
				EnableKeyword(mat, "_ALPHATEST_ON", false);
				mat.renderQueue = -1;
				break;

			case RenderingMode.Cutout:
				mat.SetOverrideTag("RenderType", "TransparentCutout");
				mat.SetInt(SrcBlendPropName, (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt(DstBlendPropName, (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt(ZWritePropName, 1);
				bool alphaTest = mat.GetFloat(CutoffPropName) > 0f;
				EnableKeyword(mat, "_ALPHATEST_ON", alphaTest);
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
				break;

			case RenderingMode.Transparent:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt(SrcBlendPropName, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mat.SetInt(DstBlendPropName, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt(ZWritePropName, 0);
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
