using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitShaderGUI : ShaderGUI
{
	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		EditorGUI.BeginChangeCheck();
		base.OnGUI (materialEditor, properties);
		if (EditorGUI.EndChangeCheck())
		{
			Material mat = materialEditor.target as Material;
			bool noTexture = mat.GetTexture("_MainTex") == null;
			bool noColor = mat.GetColor("_Color") == Color.white;
			EnableKeyword(mat, "NO_TEXTURE", noTexture);
			EnableKeyword(mat, "NO_COLOR", noColor);
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
