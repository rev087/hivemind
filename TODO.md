# TODO

## ActionLibrary

- <del>Remove parameters from the _ActionAttribute_. All parameters in the action method signature will be considered editor parameters.</del>
- Implement the ParameterAttribute to allow using a different GUI field, eg:  
	`[Hivemind.Parameter("tag", Hivemind.GUIFields.TagField)]`
- <del>Implement the _Outputs_ and _Expects_ attributes so action methods can specify context variables they expect or populate. Follows the format:</del>  
	`[Hivemind.Outputs("gameObject", typeof(GameObject))]`  
	`[Hivemind.Expects("position", typeof(Vector3))]`
- When a node is selected in the _BTEditorWindow_, the inspector should display all its inputs and outputs.