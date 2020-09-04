using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Base weapon behavior and info.
// Any weapon can specify any animation and will play it when an attack happens.
// Animation system decides when to spawn hitboxes, and which ones to spawn.
// animation system also dictates flow of attacks and combos, as well as what an attack visually looks like.
public class WeaponData : ItemData
{

  public override InventoryItemType type
  {
    get { return InventoryItemType.Weapon; }
    set { }
  }
  public Weapon_SUPEROLD weaponPrefab;
  public Hitbox_OLD hitboxPrefab;
  public Animator animator;

  // Weapon's approximate attack range. used for AI. TODO: better system for AI than this
  public float range;

  // angle within which this weapon can usefully attack. used for AI. TODO: better system for AI than this
  public float attackAngle;

  public AttackAnimation[] attackAnimations;
  [SerializeField]
  private List<string> attackAnimationNames = new List<string>();

  // the length of time in which you can queue your next attack
  // TODO: this should possibly be attack-specific?
  public float attackQueueWindow = .25f;

  // Base damage info, modified by individual hitboxes.
  public DamageData_OLD baseDamage;

  public Transform spawnTransform;
  // Set properties of the base damage object this weapon uses.
  // Hitboxes will modify these damage objects.

  // Set attacking on the animator, and specify which attack animation to use
  // with queueAttack_<animationName>

  void OnValidate()
  {
    //Mostly handle attackanimation stuff
    // ensure attackAnimationNames and attackAnimations are always the same length
    // ensure attackAnimationNames[i] == attackAnimations[i].attackAnimation
    // if it doesn't, set attackAnimationNames[i] = attackAnimations[i].attackAnimation


    while (attackAnimationNames.Count < attackAnimations.Length)
    {
      string nextAttackAnimation = attackAnimations[attackAnimationNames.Count].attackAnimation;
      attackAnimationNames.Add(nextAttackAnimation);
    }
    while (attackAnimationNames.Count > attackAnimations.Length)
    {
      attackAnimationNames.RemoveAt(attackAnimationNames.Count - 1);
    }
    for (int i = 0; i < attackAnimationNames.Count; i++)
    {
      if (attackAnimationNames[i] != attackAnimations[i].attackAnimation)
      {
        attackAnimations[i].ResetDefaults();
        attackAnimationNames[i] = attackAnimations[i].attackAnimation;
      }
    }
  }

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/Item/Weapon")]
  public static void CreateWeaponItem()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Weapon Data", "New Weapon Data", "Asset", "Save Weapon Data", "Assets/resources/Data/ItemData/Weapon");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WeaponData>(), path);
  }
#endif
}
