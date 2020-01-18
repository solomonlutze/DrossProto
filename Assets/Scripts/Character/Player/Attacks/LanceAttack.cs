using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
public class LanceAttack : CharacterAttack
{

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an TraitItem Asset
    [MenuItem("Assets/Create/CharacterAttack/LanceAttack")]
    public static void CreateCharacterAttack()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Lance Attack", "New Lance Attack", "Asset", "Save Lance Attack", "Assets/resources/Data/CharacterData/AttackData");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LanceAttack>(), path);
    }
#endif

    public override IEnumerator DoAttack(Character owner)
    {
        owner.weaponInstance.gameObject.SetActive(true);
        float t = 0;
        while (t < attackDuration)
        {
            float easedT = Utils.EaseOut(t / attackDuration, 5);
            // float easedTime = 1f + ((normalizedT -= 1f) * normalizedT * normalizedT);
            owner.weaponInstance.transform.localPosition = GetInitialPosition() + new Vector3(easedT * distance, 0, 0);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        owner.weaponInstance.gameObject.SetActive(false);
    }

    public override void InterruptAttack(Character owner)
    {
        owner.weaponInstance.transform.position = GetInitialPosition();
    }
}
