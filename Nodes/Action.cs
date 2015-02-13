using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using System.ComponentModel;

using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Hivemind {

	[System.Serializable]
	public class Action : Node
	{
		// Target Script
		public Object scriptInstance;
		private string _scriptClass;
		private string _scriptPath;
		public bool HasScript { get { return _scriptClass == null || _scriptClass.Length > 0; } }
		public string ScriptPath { get { return _scriptPath; } }

		public void SetScript(string scriptClass, string scriptPath, Object newScriptInstance) {
			if (scriptClass != _scriptClass) 
				methodName = null;
			_scriptClass = scriptClass;
			_scriptPath = scriptPath;
			scriptInstance = newScriptInstance;
		}
		
		// Target method
		private string _methodName;
		public string methodName {
			get {
				return _methodName;
			}
			set {
				if (_methodName != value) {
					_parameters.Clear ();
					_inputs.Clear ();
					_outputs.Clear ();
					_methodInfo = null ;
					_paramsForMethod = null;
					_inputsForMethod = null;
					_outputsForMethod = null;
				}
				_methodName = value;
			}
		}
		private System.Reflection.MethodInfo _methodInfo;
		public System.Reflection.MethodInfo methodInfo {
			get {
				// Nothing is selected
				if (_scriptClass == null || _scriptClass.Length == 0 || methodName == null)
					return null;

				// Return the cached value if present
				if (_methodInfo != null)
					return _methodInfo;

				// Attempt to retrieve the MethodInfo
				if (_methodInfo == null) {
					System.Type type = System.Type.GetType(_scriptClass);
					if (type == null) {
						Debug.LogWarning(string.Format ("Could not find the class {0} associated with the action", _scriptClass));
						return null;
					}
					_methodInfo = type.GetMethod(methodName);
				}

				return _methodInfo;
			}
		}
		
		// Parameters

		// Class to represent the parameter unit
		public class ActionParameter {
			public System.Type Type;
			public object Value;

			public ActionParameter(System.Type paramType, object paramValue) {
				Type = paramType;
				Value = paramValue;
			}

			public string ValueToString() {
				if (Value != null) return Value.ToString ();
				else return "";
			}
		}
		// This is set after retrieving the parameters for a method, and unset when changing the class or method
		// selected. It is used to verify if the current list of parameters match the current method selection
		// or we need to reflect the class again.
		private string _paramsForMethod;
		private Dictionary<string, ActionParameter> _parameters = new Dictionary<string, ActionParameter>();
		public Dictionary<string, ActionParameter> Parameters {
			get {
				if (_paramsForMethod == methodName)
					return _parameters;

				if (methodName == null || methodInfo == null) {
					_parameters.Clear();
					return _parameters;
				}

				ParameterInfo[] paramsInfo = methodInfo.GetParameters();
				foreach (ParameterInfo parameter in paramsInfo) {
					_parameters[parameter.Name] = new ActionParameter(parameter.ParameterType, GetEmptyValue(parameter.ParameterType));
				}

				_paramsForMethod = methodName;

				return _parameters;
			}
		}
		private object GetEmptyValue(System.Type t)
		{
			if (t.IsValueType)
				return System.Activator.CreateInstance(t);
			
			return null;
		}

		// Contextual inputs and outputs
		private string _inputsForMethod;
		private Dictionary<string, System.Type> _inputs = new Dictionary<string, System.Type>();
		public Dictionary<string, System.Type> Inputs {
			get {
				if (_inputsForMethod == methodName)
					return _inputs;

				if (methodName == null || methodInfo == null) {
					_inputs.Clear();
					return _inputs;
				}

				ExpectsAttribute[] attrs = (ExpectsAttribute[]) methodInfo.GetCustomAttributes(typeof(ExpectsAttribute), false);
				foreach (ExpectsAttribute input in attrs) {
					_inputs[input.key] = input.type;
				}

				return _inputs;
			}
		}
		private string _outputsForMethod;
		private Dictionary<string, System.Type> _outputs = new Dictionary<string, System.Type>();
		public Dictionary<string, System.Type> Outputs {
			get {
				if (_outputsForMethod == methodName)
					return _outputs;
				
				if (methodName == null || methodInfo == null) {
					_outputs.Clear();
					return _outputs;
				}
				
				OutputsAttribute[] attrs = (OutputsAttribute[]) methodInfo.GetCustomAttributes(typeof(OutputsAttribute), false);
				foreach (OutputsAttribute output in attrs) {
					_outputs[output.key] = output.type;
				}
				
				return _outputs;
			}
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
		public void Serialize(ref XmlElement el) {
			el.SetAttribute("script", _scriptClass);
			el.SetAttribute("scriptpath", _scriptPath);
			el.SetAttribute("method", methodName);
			foreach (KeyValuePair<string, ActionParameter> parameter in Parameters)
			{
				XmlElement paramEl = el.OwnerDocument.CreateElement("param");
				paramEl.SetAttribute("key", parameter.Key);
				paramEl.SetAttribute("type", parameter.Value.Type.ToString ());
				paramEl.SetAttribute("value", parameter.Value.ValueToString ());
				el.AppendChild (paramEl);
			}
		}
		
		// Deserialization
		public void Deserialize(XmlElement el) {
			_scriptClass = el.GetAttribute("script");
			_scriptPath = el.GetAttribute("scriptpath");
			methodName = el.GetAttribute ("method");
			if (methodName != null && methodInfo != null && el.HasChildNodes) {
				foreach (XmlNode paramNode in el.ChildNodes) {
					XmlElement paramEl = paramNode as XmlElement;
					if (paramEl != null && paramEl.Name == "param") {
						string key = paramEl.GetAttribute ("key");

						// When a method signature changes, a serialized parameter might be gone, so we check for its existance
						if (Parameters.ContainsKey(key)) {
							System.Type type = System.Type.GetType(paramEl.GetAttribute("type"));
							Parameters[key].Type = type;
							string value = paramEl.GetAttribute("value");
							Parameters[key].Value = TypeDescriptor.GetConverter(type).ConvertFrom(value);
						}

					}
				}
			}
		}
		
		// Runtime
		private static Dictionary<string, ActionLibrary> ActionLibraries = new Dictionary<string, ActionLibrary>();
		public override Status Tick(GameObject agent, Context context) {

			string actionLibID = _scriptClass + "-" + behaviorTree.GetInstanceID();

			ActionLibrary lib;
			if (ActionLibraries.ContainsKey(actionLibID)) {
				lib = ActionLibraries[actionLibID];
			} else {
				System.Type type = System.Type.GetType (_scriptClass);
				if (type == null) {
					Debug.LogWarning("An action node does not have an associated ActionLibrary");
					return Status.Error;
				}

				lib = (ActionLibrary) System.Activator.CreateInstance (type);
				lib.agent = agent;
				lib.context = context;
				MethodInfo actionMethod = type.GetMethod ("Start");
				if (actionMethod != null) actionMethod.Invoke (lib, null);
				ActionLibraries[actionLibID] = (ActionLibrary) lib;
			}
			
			if (methodInfo == null) {
				Debug.LogWarning("An action node does not have an associated ActionLibrary method");
				return Status.Error;
			}

			object[] parameters = new object[Parameters.Count];
			int i = 0;
			foreach (KeyValuePair<string, ActionParameter> parameter in Parameters) {
				parameters[i++] = parameter.Value.Value;
			}

			object result = methodInfo.Invoke(lib, parameters);

			return (Status) result;
		}
	}
}
