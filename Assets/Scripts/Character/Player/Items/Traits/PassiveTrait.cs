using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PassiveTrait : Trait_OLD
{

    public override TraitType traitType
    {
        get { return TraitType.Passive; }
        set { }
    }


#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an TraitItem Asset
    [MenuItem("Assets/Create/Trait/PassiveTrait")]
    public static void CreatePassiveTrait()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Passive Trait", "New Passive Trait", "Asset", "Save Passive Trait", "Assets/resources/Data/TraitData/PassiveTraits");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PassiveTrait>(), path);
    }
#endif
}


// public class PassiveTraitMono : TraitMono {

// 	public override TraitType traitType {
// 		get { return TraitType.Passive; }
// 		set { }
// 	}


// 	// Called when the trait is applied to the player (usually on spawn)
// 	public virtual void OnTraitAdded (Character owner) {

// 	}

// 	// Called when the trait is removed from the character (usually on death)
// 	public virtual void OnTraitRemoved (Character owner) {

// 	}
// }
