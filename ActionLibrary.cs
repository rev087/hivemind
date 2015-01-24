using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Hivemind {
	
	public enum GUIField {
		BoundsField, ColorField, CurveField, EnumMaskField, EnumPopup, FloatField, IntField, IntSlider, LayerField,
		RectField, TagField, TextField, Toggle, ToggleLeft, Vector2Field, Vector3Field, Vector4Field
	}

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class ActionAttribute : System.Attribute {
		public ActionAttribute() {}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
	public class ParameterAttribute : System.Attribute {
		public string name;
		public System.Type type;
		public GUIField guiField;
		public ParameterAttribute(string parameterName, System.Type parameterType) {
			name = parameterName;
			type = parameterType;
			throw new System.NotImplementedException("ParameterAttribute not implemented");
		}
		public ParameterAttribute(string parameterName, System.Type parameterType, GUIField parameterGUIField) {
			name = parameterName;
			type = parameterType;
			guiField = parameterGUIField;
			throw new System.NotImplementedException("ParameterAttribute not implemented");
		}
	}
	
	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
	public class ExpectsAttribute : System.Attribute {
		public string key;
		public System.Type type;
		
		public ExpectsAttribute(string inputKey, System.Type inputType) {
			key = inputKey;
			type = inputType;
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
	public class OutputsAttribute : System.Attribute {
		public string key;
		public System.Type type;

		public OutputsAttribute(string outputKey, System.Type outputType) {
			key = outputKey;
			type = outputType;
		}
	}

	public class ActionLibrary {
		public GameObject agent;
		public Context context;
	}

}