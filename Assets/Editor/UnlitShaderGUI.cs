using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitShaderGUI : ShaderGUI
{
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
		bool noColor = mat.GetColor("_Color") == Color.white;
		EnableKeyword(mat, "_TEXTURE_OFF", noTexture);
		EnableKeyword(mat, "_COLOR_OFF", noColor);
	}

	static void EnableKeyword(Material material, string keyword, bool enable)
	{
		if(enable)
			material.EnableKeyword(keyword);
		else
			material.DisableKeyword(keyword);
	}
}
