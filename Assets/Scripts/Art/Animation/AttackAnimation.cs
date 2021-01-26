using System;
using System.Collections.Generic;
using UnityEngine;

// Custom serializable class
[Serializable]
public class AnimationPhaseInfo
{
  public Vector2 animationInput;
  // how long, in seconds, the animation phase lasts
  public float phaseDuration;
  public float cancelWindow;
  public float rotationSpeed;
}

// Reads from AttackAnimationData scriptableobjects.
// Basically, populates weapons with default animation data,
// then allows modification of that data on a per-weapon basis.

[Serializable]
public class AttackAnimation
{
  [StringInList(typeof(PropertyDrawerHelpers), "AllAttackAnimationNames", new object[] { false })] public string attackAnimation;

  public AnimationPhaseInfo windup;
  public AnimationPhaseInfo attack;
  public AnimationPhaseInfo recovery;
  public List<HitboxData> hitboxes;

  public void ResetDefaults()
  {
    hitboxes.Clear();
    ReadFromScriptableObject();
  }

  private void ReadFromScriptableObject()
  {
    var datavar = Resources.Load("Data/AnimationData/Attack/" + attackAnimation);
    AttackAnimationData data = Resources.Load("Data/AnimationData/Attack/" + attackAnimation) as AttackAnimationData;
    this.windup = data.windup;
    this.attack = data.attack;
    this.recovery = data.recovery;
    foreach (HitboxData hb in data.hitboxes)
    {
      hitboxes.Add(hb);
    }
  }

}
// public class Ingredient
// {
//     public string name;
//     public int amount = 1;
//     public IngredientUnit unit;
// }

// public class Recipe : MonoBehaviour
// {
//     public Ingredient potionResult;
//     public Ingredient[] potionIngredients;
// }

