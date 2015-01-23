using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using System.ComponentModel;

namespace Hivemind {

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class ActionAttribute : System.Attribute {
		public string[] editorParameters;
		public ActionAttribute(params string[] parameters) {
			editorParameters = parameters;
		}
	}
	
	[System.Serializable]
	public class Action : Node
	{

		// Target MonoScript
		[SerializeField]
		private MonoScript _monoScriptInstance;
		[SerializeField]
		private int _monoScriptID;
		[SerializeField]
		private string _monoScriptClass;
		public MonoScript monoScript {
			get {
				// No class means no selection, return null
				if (_monoScriptClass == null ||_monoScriptClass == "")
					return null;
				
				// Return object reference if present
				if (_monoScriptInstance != null)
					return _monoScriptInstance;

				// First attempt to retrieve the object reference from the object ID
				if (_monoScriptID != 0) {
					_monoScriptInstance = EditorUtility.InstanceIDToObject(_monoScriptID) as MonoScript;

					if (_monoScriptInstance != null)
						return _monoScriptInstance;
				}

				// Lastly, attempt to retrieve the object reference from the class name
				_monoScriptInstance = MonoScript.FindObjectOfType<MonoScript>();

				// Failing that, the script was either renamed or removed, so we disassociate and log a warning
				if (_monoScriptInstance == null) {
					Debug.LogWarning(string.Format ("Could not find the class {0} associated with the action", _monoScriptClass));
					_monoScriptID = 0;
					_monoScriptClass = "";
					_monoScriptInstance = null;
				} else {
					// Store the ID for cheaper retrieval
					_monoScriptID = _monoScriptInstance.GetInstanceID();
				}


				return _monoScriptInstance;
			}
			set {
				if (value != _monoScriptInstance) {
					if (value != null) {
						_monoScriptClass = value.GetClass ().ToString();
						_monoScriptID = value.GetInstanceID();
						_monoScriptInstance = value;
					}
					else {
						_monoScriptClass = "";
						_monoScriptID = 0;
						_monoScriptInstance = null;
					}
					methodName = null;
				}
			}
		}

		// Class instance (runtime)
		// - IMPLEMENT -
		
		// Target method
		private string _methodName;
		public string methodName {
			get {
				return _methodName;
			}
			set {
				if (_methodName != value) {
					_editorParameters.Clear ();
					_methodInfo = null ;
					_editorParamsForMethod = null;
				}
				_methodName = value;
			}
		}
		private System.Reflection.MethodInfo _methodInfo;
		public System.Reflection.MethodInfo methodInfo {
			get {
				// Nothing is selected
				if (monoScript == null || methodName == null)
					return null;

				// Return the cached value if present
				if (_methodInfo != null)
					return _methodInfo;

				// Attempt to retrieve the MethodInfo
				if (_methodInfo == null) {
					System.Type type = System.Type.GetType(_monoScriptClass);
					if (type == null) return null;
					_methodInfo = type.GetMethod(methodName);
				}

				return _methodInfo;
			}
		}
		
		// Parameters

		// Class to represent the parameter unit
		public class ActionParameter {
			public object Value;
			public System.Type Type;
		}

		// This is set after retrieving the parameters for a method, and unset when changing the class or method
		// selected. It is used to verify if the current list of parameters reflects the current method selection
		// or we need to reflect the class again.
		private string _editorParamsForMethod;
		private Dictionary<string, ActionParameter> _editorParameters = new Dictionary<string, ActionParameter>();
		public Dictionary<string, ActionParameter> EditorParameters {
			get {
				if (_editorParamsForMethod == methodName)
					return _editorParameters;

				if (methodName == null || methodInfo == null) {
					_editorParameters.Clear();
					return _editorParameters;
				}

				// Retrieve the parameters which we want to populate during edit time 
				ActionAttribute[] attrs = (ActionAttribute[]) methodInfo.GetCustomAttributes(typeof(ActionAttribute), false);
				if (attrs.Length > 0) {

					// The first parameters of ActionAttribute is of type "params object[]", so there is only one element in the `attrs` array
					foreach (string paramName in attrs[0].editorParameters) {

						// To retrieve the type, we need to look into the method signature for a parameter of name `paramName`, and retrieve its type
						// GetDefaultValue is used to obtain an appropriate default value that won't cause deserialization errors
						System.Type type = GetParamType (paramName);
						_editorParameters[paramName] = new ActionParameter {Type = type, Value = GetDefaultValue(type)};

					}
				}
				_editorParamsForMethod = methodName;

				return _editorParameters;
			}
		}
		private System.Type GetParamType(string paramName) {
			ParameterInfo[] paramsInfo = methodInfo.GetParameters();
			foreach (ParameterInfo parameter in paramsInfo) {
				if (parameter.Name == paramName) return parameter.ParameterType;
			}
			return null;
		}
		private object GetDefaultValue(System.Type type) {
			// This method is necessary because string does not have a constructor that takes 0 parameters, so
			// Activator.CreateInstance will throw an exception
			if (type == typeof(string)) return "";
			else return System.Activator.CreateInstance(type);
		}

		// Child connections
		public override void ConnectChild(Node child) {
			throw new System.InvalidOperationException(string.Format ("{0} cannot have child connections", this));
		}
		public override void DisconnectChild(Node child) {
			throw new System.InvalidOperationException(string.Format ("{0} cannot have child connections", this));
		}
		public override List<Node> Children {
			get {
				throw new System.InvalidOperationException(string.Format ("{0} cannot have child connections", this));
			}
		}
		public override int ChildCount { get { return 0; } }
		public override bool CanConnectChild { get { return false; } }
		public override bool ContainsChild (Node child)
		{
			throw new System.InvalidOperationException(string.Format ("{0} cannot have child connections", this));
		}
		
		// Serialization
		public XmlElement Serialize(XmlDocument doc) {
			XmlElement el = doc.CreateElement("action");
			el.SetAttribute("editorx", editorPosition.x.ToString());
			el.SetAttribute("editory", editorPosition.y.ToString());
			el.SetAttribute("script", _monoScriptClass);
			el.SetAttribute("scriptid", _monoScriptID.ToString());
			el.SetAttribute("method", methodName);
			foreach (KeyValuePair<string, ActionParameter> parameter in EditorParameters)
			{
				XmlElement paramEl = doc.CreateElement("param");
				paramEl.SetAttribute("key", parameter.Key);
				paramEl.SetAttribute("type", parameter.Value.Type.ToString ());
				paramEl.SetAttribute("value", parameter.Value.Value.ToString ());
				el.AppendChild (paramEl);
			}
			return el;
		}
		
		// Deserialization
		public void Deserialize(XmlElement el) {
			_monoScriptClass = el.GetAttribute("script");
			if (el.GetAttribute ("scriptid") != null && el.GetAttribute ("scriptid").Length > 0)
				_monoScriptID = int.Parse (el.GetAttribute ("scriptid"));
			methodName = el.GetAttribute ("method");
			if (methodName != null && methodInfo != null && el.HasChildNodes) {
				foreach (XmlNode paramNode in el.ChildNodes) {
					XmlElement paramEl = paramNode as XmlElement;
					if (paramEl != null && paramEl.Name == "param") {
						string key = paramEl.GetAttribute ("key");
						System.Type type = System.Type.GetType(paramEl.GetAttribute("type"));
						EditorParameters[key].Type = type;
						string value = paramEl.GetAttribute("value");
						EditorParameters[key].Value = TypeDescriptor.GetConverter(type).ConvertFrom(value);
					}
				}
			}
		}
		
		// Runtime
		public override Result Run(BehaviorTreeAgent agent) {
			throw new System.NotImplementedException ();
		}
	}
}
