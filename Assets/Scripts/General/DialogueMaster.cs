using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueMaster : MonoBehaviour {

	private Dictionary<string, string> variableStorage;
	
	// Use this for initialization
	void Start () {
		variableStorage = new Dictionary<string, string>();
		variableStorage["playerName"] = "Chelsea";
		variableStorage["playerClass"] = "Pilgrim";
	}

	public string GetDialogueVariable(string variableName) {
		return variableStorage[variableName];
	}
	
	public void SetDialogueVariable(string variableName, string variableValue) {
		variableStorage[variableName] = variableValue;
	}
}
