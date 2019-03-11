using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AttackAnimationData : ScriptableObject {
 
    // TODO: This should be a dropdown based on attack animations in the animations folder
    public string attackAnimation;
    public AnimationPhaseInfo windup;
    public AnimationPhaseInfo attack;
    public AnimationPhaseInfo recovery;
    public List<HitboxInfo> hitboxes;

    #if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an AttackAnimation Asset
        [MenuItem("Assets/Create/AttackAnimation")]
        public static void CreateAttackAnimation()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Attack Animation", "New Attack Animation", "Asset", "Save Attack Animation", "Assets/resources/Data/AnimationData/Attack");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AttackAnimationData>(), path);
        }
    #endif
}
