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

	[System.Flags]
	public enum MaterialFlags
	{
		None = 0,
		NoMaskScaleOffset = 1 << 0,
		AdvancedToggle = 1 << 1,
		ForceTexture = 1 << 2,
		ForceMask = 1 << 3,
		ForceMaskScaleOffset = 1 << 4,
		ForceColor = 1 << 5,
		ForceCutoff = 1 << 6,
	}

	public const string RenderingModePropName = "_RenderingMode";
	public const string MainTexPropName = "_MainTex";
	public const string MaskPropName = "_Mask";
	public const string ColorPropName = "_Color";
	public const string CutoffPropName = "_Cutoff";
	public const string SrcBlendPropName = "_SrcBlend";
	public const string DstBlendPropName = "_DstBlend";
	public const string ZWritePropName = "_ZWrite";
	public const string MaterialFlagsPropName = "_MaterialFlags";

	static readonly GUIContent s_noMaskScaleOffsetLabel = new GUIContent("Same Tiling & Offset as Texture");
	static readonly GUIContent s_advancedLabel = new GUIContent("Advanced Options");
	static readonly GUIContent s_forceTextureLabel = new GUIContent("Do not disable Texture");
	static readonly GUIContent s_forceMaskLabel = new GUIContent("Do not disable Mask");
	static readonly GUIContent s_forceMaskScaleOffsetLabel = new GUIContent("Do not disable Mask Tiling & Offset");
	static readonly GUIContent s_forceColorLabel = new GUIContent("Do not disable Color");
	static readonly GUIContent s_forceCutoffLabel = new GUIContent("Do not disable Alpha Cutoff");

			
	bool m_firstTime = false;

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		if(!m_firstTime)
		{
			m_firstTime = true;

			foreach(Material material in materialEditor.targets)
				MaterialChanged(material);
		}

		EditorGUI.BeginChangeCheck();
		DrawProperties (materialEditor, properties);
		if (EditorGUI.EndChangeCheck())
		{
			foreach(Material material in materialEditor.targets)
				MaterialChanged(material);
		}
	}

	static void DrawProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		materialEditor.SetDefaultGUIWidths();
		EditorGUI.BeginChangeCheck();

		MaterialProperty materialFlagesProp = FindProperty(MaterialFlagsPropName, properties);
		MaterialFlags materialFlags = (MaterialFlags)materialFlagesProp.floatValue;
		MaterialFlags mixedValueMaterialFlags = MaterialFlags.None;
		if(materialFlagesProp.hasMixedValue)
			mixedValueMaterialFlags = GetMixedValueMaterialFlags(materialEditor);

		MaterialProperty renderingModeProp = DrawProperty(RenderingModePropName, materialEditor, properties);
		DrawProperty(MainTexPropName, materialEditor, properties);

		bool noMaskScaleOffset = GetMaterialFlag(materialFlags, MaterialFlags.NoMaskScaleOffset);
		bool mixedValueNoMaskScaleOffset = GetMaterialFlag(mixedValueMaterialFlags, MaterialFlags.NoMaskScaleOffset);
		bool maskScaleOffset = !noMaskScaleOffset || mixedValueNoMaskScaleOffset;
		DrawTexure(MaskPropName, maskScaleOffset, materialEditor, properties);
		DrawFlagToggleProperty(s_noMaskScaleOffsetLabel, MaterialFlags.NoMaskScaleOffset, ref materialFlags, ref mixedValueMaterialFlags);

		DrawProperty(ColorPropName, materialEditor, properties);

		if((RenderingMode)renderingModeProp.floatValue == RenderingMode.Cutout && !renderingModeProp.hasMixedValue)
			DrawProperty(CutoffPropName, materialEditor, properties);

		bool advancedToggle = DrawFlagFoldoutProperty(s_advancedLabel, MaterialFlags.AdvancedToggle, ref materialFlags, ref mixedValueMaterialFlags);
		
		if(advancedToggle)
		{
			EditorGUI.indentLevel++;
			DrawFlagToggleProperty(s_forceTextureLabel, MaterialFlags.ForceTexture, ref materialFlags, ref mixedValueMaterialFlags);
			DrawFlagToggleProperty(s_forceMaskLabel, MaterialFlags.ForceMask, ref materialFlags, ref mixedValueMaterialFlags);
			DrawFlagToggleProperty(s_forceMaskScaleOffsetLabel, MaterialFlags.ForceMaskScaleOffset, ref materialFlags, ref mixedValueMaterialFlags);
			DrawFlagToggleProperty(s_forceColorLabel, MaterialFlags.ForceColor, ref materialFlags, ref mixedValueMaterialFlags);
			DrawFlagToggleProperty(s_forceCutoffLabel, MaterialFlags.ForceCutoff, ref materialFlags, ref mixedValueMaterialFlags);
			EditorGUI.indentLevel--;
		}

		if(EditorGUI.EndChangeCheck())
		{
			if(materialFlagesProp.hasMixedValue)
				SetMixedValueMatrialFlags(materialEditor, materialFlags, mixedValueMaterialFlags);
			else
				materialFlagesProp.floatValue = (float)materialFlags;
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		materialEditor.RenderQueueField();
		materialEditor.EnableInstancingField();
		materialEditor.DoubleSidedGIField();
	}

	static MaterialProperty DrawProperty(string propertyName, MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		MaterialProperty property = FindProperty(propertyName, properties);
		materialEditor.ShaderProperty(property, property.displayName);
		return property;
	}

	static MaterialProperty DrawTexure(string propertyName, bool scaleOffset, MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		MaterialProperty property = FindProperty(propertyName, properties);
		materialEditor.TextureProperty(property, property.displayName, scaleOffset);
		return property;
	}

	static void DrawFlagToggleProperty(GUIContent label, MaterialFlags flag, ref MaterialFlags allFlags, ref MaterialFlags mixedValueFlags)
	{
		bool value = GetMaterialFlag(allFlags, flag);
		bool mixedValue = GetMaterialFlag(mixedValueFlags, flag);

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		value = EditorGUILayout.Toggle(label, value);
		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck())
			mixedValueFlags = SetMaterialFlag(mixedValueFlags, flag, false);

		allFlags = SetMaterialFlag(allFlags, flag, value);
	}

	static bool DrawFlagFoldoutProperty(GUIContent label, MaterialFlags flag, ref MaterialFlags allFlags, ref MaterialFlags mixedValueFlags)
	{
		bool value = GetMaterialFlag(allFlags, flag);
		bool mixedValue = GetMaterialFlag(mixedValueFlags, flag);

		EditorGUI.BeginChangeCheck();
		value = EditorGUILayout.Foldout(value || mixedValue, label);
		if(EditorGUI.EndChangeCheck())
			mixedValueFlags = SetMaterialFlag(mixedValueFlags, flag, false);

		allFlags = SetMaterialFlag(allFlags, flag, value);

		return value;
	}

	static MaterialFlags GetMixedValueMaterialFlags(MaterialEditor materialEditor)
	{
		MaterialFlags onFlags = MaterialFlags.None;
		MaterialFlags offFlags = ~MaterialFlags.None;
		
		foreach(Material material in materialEditor.targets)
		{
			MaterialFlags materialFlags = (MaterialFlags)material.GetInt(MaterialFlagsPropName);
			onFlags |= materialFlags;
			offFlags &= materialFlags;
		}

		return onFlags ^ offFlags;
	}
	
	static void SetMixedValueMatrialFlags(MaterialEditor materialEditor, MaterialFlags allFlags, MaterialFlags mixedValueFlags)
	{
		foreach(Material material in materialEditor.targets)
		{
			MaterialFlags materialFlags = (MaterialFlags)material.GetInt(MaterialFlagsPropName);
			materialFlags = (materialFlags & mixedValueFlags) | (allFlags & ~mixedValueFlags);
			material.SetInt(MaterialFlagsPropName, (int) materialFlags);
		}
	}

	static void MaterialChanged(Material mat)
	{
		MaterialFlags materialFlags = (MaterialFlags)mat.GetInt(MaterialFlagsPropName);
		
		bool noTexture = mat.GetTexture(MainTexPropName) == null;
		bool forceTextureFlag = GetMaterialFlag(materialFlags, MaterialFlags.ForceTexture);
		EnableKeyword(mat, "_TEXTURE_OFF", noTexture && !forceTextureFlag);

		Vector2 textureScale = mat.GetTextureScale(MainTexPropName);
		Vector2 textureOffset = mat.GetTextureOffset(MainTexPropName);
		bool noMask = mat.GetTexture(MaskPropName) == null;
		bool forceMaskFlag = GetMaterialFlag(materialFlags, MaterialFlags.ForceMask);
		EnableKeyword(mat, "_MASK_OFF", noMask && !forceMaskFlag);

		Vector2 maskScale = mat.GetTextureScale(MaskPropName);
		Vector2 maskOffset = mat.GetTextureOffset(MaskPropName);
		bool sameMaskScale = maskScale == textureScale;
		bool sameMaskOffset = maskOffset == textureOffset;
		bool noMaskScaleOffsetFlag = GetMaterialFlag(materialFlags, MaterialFlags.NoMaskScaleOffset);
		bool forceMaskScaleOffsetFlag = GetMaterialFlag(materialFlags, MaterialFlags.ForceMaskScaleOffset);
		bool noMaskScaleOffset = noMaskScaleOffsetFlag || sameMaskScale && sameMaskOffset;
		EnableKeyword(mat, "_MASK_SCALE_OFFSET_OFF", noMaskScaleOffset && !noMask && !forceMaskScaleOffsetFlag);
		
		bool noColor = mat.GetColor(ColorPropName) == Color.white;
		bool forceColorFlag = GetMaterialFlag(materialFlags, MaterialFlags.ForceColor);
		EnableKeyword(mat, "_COLOR_OFF", noColor && !forceColorFlag);

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
				bool forceCutoff = GetMaterialFlag(materialFlags, MaterialFlags.ForceCutoff);
				EnableKeyword(mat, "_ALPHATEST_ON", alphaTest || forceCutoff);
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

	static bool GetMaterialFlag(MaterialFlags allFlags, MaterialFlags flag)
	{
		return (allFlags & flag) != MaterialFlags.None;
	}

	static MaterialFlags SetMaterialFlag(MaterialFlags allFlags, MaterialFlags flag, bool enable)
	{
		if(enable)
			return allFlags | flag;
		else
			return allFlags & ~flag;
	}

	static void EnableKeyword(Material material, string keyword, bool enable)
	{
		if(enable)
			material.EnableKeyword(keyword);
		else
			material.DisableKeyword(keyword);
	}
}
