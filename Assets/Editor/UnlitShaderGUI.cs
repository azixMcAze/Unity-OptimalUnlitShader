using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitShaderGUI : ShaderGUI
{
	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		base.OnGUI (materialEditor, properties);
	}
}
