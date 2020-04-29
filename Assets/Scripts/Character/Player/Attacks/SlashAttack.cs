using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
public class SlashAttack : AttackSkillData
{

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/CharacterAttack/SlashAttack")]
  public static void CreateCharacterAttack()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Slash Attack", "New Slash Attack", "Asset", "Save Slash Attack", "Assets/resources/Data/CharacterData/AttackData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SlashAttack>(), path);
  }
#endif

  public override void BeforeAttack(Character owner)
  {
    float startingRot = sweepRadiusInDegrees / 2;
    owner.weaponPivot.transform.localEulerAngles = new Vector3(0, 0, startingRot);
    base.BeforeAttack(owner);
  }

  public override IEnumerator DoAttack(Character owner)
  {
    float startingRot = sweepRadiusInDegrees / 2;
    owner.weaponPivot.transform.localEulerAngles = new Vector3(0, 0, startingRot);
    owner.weaponInstances[name].gameObject.SetActive(true);
    float t = 0;
    while (t < attackDuration)
    {
      float easedT = Utils.EaseOut(t / attackDuration, 3);
      float desiredAngle = startingRot - (easedT * sweepRadiusInDegrees);
      owner.weaponPivot.transform.localEulerAngles = new Vector3(0, 0, desiredAngle);
      // owner.weaponPivot.transform.localEulerAngles = new Vector3(0, 0, startingRot - (t * sweepRadiusInDegrees / attackDuration));
      t += Time.deltaTime;
      yield return new WaitForFixedUpdate();
    }
    owner.weaponInstances[name].gameObject.SetActive(false);
  }
}
