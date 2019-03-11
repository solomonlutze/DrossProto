using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using SimpleJSON;
using Yarn.Unity;

// How to organize this class...
// We could parse the json into classes that have functions,
// like "canSay" or whatever.
// We could just have this function own its big ol dialogue json,
// keep track of internal state and requirements,
// and calculate what to say on the fly, every time.
// That's p r o b a b l y the most straightforward way to do this.

// some nomenclature:
// a "dialogue" is a thing a character can say.
// a "line" is a subdivision of a dialogue that requires a new interaction.
// eg, you talk to a character once and they say "Move along".
// talk to them again, they say "I said, move along."
// Those could be 2 lines in the same dialgoue.

public class DialogueHandler : Interactable {
    public TextAsset scriptToLoad;
    private CharacterAI character;
    private SuperTextMesh activeStm;
    private JSONNode dialogueJson;
    
    private bool[] readDialogues;
    private int currentLineIndex = 0;
    private bool continueRequired = false;

    public string startNode;
    // public TextMeshPro tmpScript;

    void Start() {
        if (scriptToLoad != null) {
            FindObjectOfType<Yarn.Unity.DialogueRunner>().AddScript(scriptToLoad);
        }
        character = gameObject.GetComponent<CharacterAI>();
        // dialogueJson = JSON.Parse(dialogueText.text);
        // readDialogues = new bool[dialogueJson["dialogues"].AsArray.Count];
    }
   
    // Go through each of our dialogues in order.
    // Check all of the requirements for that dialogue.
    // If they're met, say the dialogue, then mark the dialogue line as said (which by default makes it unsayable again).
    void PlayerActivate(PlayerController player) {
        if (character.aiState == AiStates.PlayerAggro) { return; }
        if (startNode != null) {
            GameMaster.Instance.StartDialogue(startNode);
        }

        // if (activeStm != null) {
        //     continueRequired = activeStm.Continue();
        //     if (continueRequired) {
        //         return;
        //     } else if (currentLineIndex == 0 && activeStm.text != " ") {
        //         activeStm.text = " ";
        //         return;
        //     }
        // }
        // JSONNode currentDialogue = null;
        // int i; // hang on to this so we can mark this dialogue as completed if need be
        // for (i = 0; i < dialogueJson["dialogues"].AsArray.Count; i++) {
        //     if (!readDialogues[i] && MeetsRequirements(player, dialogueJson["dialogues"][i])) {
        //         currentDialogue = dialogueJson["dialogues"][i];
        //         break;
        //     }
        // }
        // SuperTextMesh oldStm = activeStm;
        // if (currentDialogue != null && currentLineIndex < currentDialogue["lines"].AsArray.Count) {
        //     activeStm = null;
        //     string currentDialogueLine = currentDialogue["lines"][currentLineIndex];
        //     Regex speakerRgx = new Regex(@"\|.*?\|");
        //     Match speakerMatch = speakerRgx.Match(currentDialogueLine);
        //     if (speakerMatch.Value != "") {
        //         string speakerName = speakerMatch.Value.Substring(1, speakerMatch.Value.Length-2);
        //         if (speakerName == "player") {
        //             activeStm = player.stm;
        //         }
        //         currentDialogueLine = currentDialogueLine.Replace(speakerMatch.Value, "");
        //     }
        //     if (activeStm == null) {
        //         activeStm = character.stm;
        //     }
        //     if (oldStm != null && oldStm != activeStm) {
        //         oldStm.text = " ";
        //     }
        //     Regex rgx = new Regex(@"{.*?}");
        //     // Replace variables in the string
        //     foreach (Match match in rgx.Matches(currentDialogueLine) ) {
        //         string variableName = match.Value.Substring(1, match.Value.Length-2);
        //         string replaceValue = "";
        //         char[] splitchar = { '.' };
        //         string[] variableStrings = variableName.Split(splitchar);
        //         if (variableStrings.Length == 2) {
        //             if (variableStrings[0] == "player") {
        //                 var field = player.GetType().GetField(variableStrings[1]);
        //                 replaceValue = (string)field.GetValue(player);
        //             }
        //             else if (variableStrings[0] == "self") {
        //                 var field = character.GetType().GetField(variableStrings[1]);
        //                 replaceValue = (string)field.GetValue(gameObject.GetComponent<Character>());
        //             }
        //         } else {
        //             replaceValue = dialogueMaster.GetDialogueVariable(variableName);
        //         }
        //         currentDialogueLine = currentDialogueLine.Replace(match.Value, replaceValue);
        //     }
        //     activeStm.text = currentDialogueLine;
        //     currentLineIndex++;
        //     if (currentLineIndex >= currentDialogue["lines"].AsArray.Count) {
        //         currentLineIndex = 0;
        //         if (!currentDialogue["repeats"]) {
        //             readDialogues[i] = true;
        //         }
        //     }
        // }
    }

    bool MeetsRequirements(PlayerController player, JSONNode dialogueNode) {
        JSONNode requirements = dialogueNode["requirements"];

        foreach(JSONNode statRequirement in requirements["stats"]) {
            if (!MeetsStatRequirement(player, statRequirement)) {
                return false;
            }
        }
        foreach(JSONNode classRequirement in requirements["isClass"]) {
            if (!MeetsIsClassRequirement(player, classRequirement)) {
                return false;
            }
        }
        foreach(JSONNode inventoryRequirement in requirements["hasInInventory"]) {
            if (!MeetsHasInInventoryRequirement(player, inventoryRequirement)) {
                return false;
            }
        }
        foreach(JSONNode equippedRequirement in requirements["hasEquipped"]) {
            if (!MeetsHasEquippedRequirement(player, equippedRequirement)) {
                return false;
            }
        }
        return true;
    }
    
    bool MeetsIsClassRequirement(PlayerController player, string characterClass) {
        return player.characterClass == characterClass;
    }

    // TODO: have multiple equipped items
    bool MeetsHasEquippedRequirement(PlayerController player, string itemId) {
        return player.equippedWeaponId == itemId;
    }

    // TODO: implement this better
    bool MeetsHasInInventoryRequirement(PlayerController player, string itemId) {
        return true;
    }

    bool MeetsStatRequirement(PlayerController player, JSONNode statRequirement) {
        var stat = player.GetType().GetField((string)statRequirement["stat"]);
        if (stat != null) {
            string comparisonOperator = statRequirement["operator"];
            if (comparisonOperator != null) {
                switch(comparisonOperator) {
                    case "greaterthan":
                        return (float)stat.GetValue(player) > int.Parse(statRequirement["value"]);
                    case "lessthan":
                        return (float)stat.GetValue(player) < int.Parse(statRequirement["value"]);
                }
            }
        }
        return false;
    }
}