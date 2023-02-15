using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodType { Mushroom = 0, Lymph = 1 }

public class FoodInfo
{
  public FoodType foodType;
  public TraitSlotToTraitDictionary traits;
  public FoodInfo(FoodType ft, TraitSlotToTraitDictionary t = null)
  {
    foodType = ft;
    traits = t;
  }
}
public class FoodPickupItem : PickupItem
{

  // public TraitSlotToTraitDictionary traits;
  public FoodType foodType;
  public TraitSlotToTraitDictionary traits = null;

  public override string interactableText
  {
    get { return "Eat"; }
    set { }
  }

  public override void PlayerActivate()
  {
    Debug.Log("activate food?");
    if (GameMaster.Instance.collectedFood.Count < GameMaster.Instance.maxFood)
    {
      GameMaster.Instance.AddCollectedFoodItem(new FoodInfo(foodType, traits));
      Destroy(gameObject);
    }
    else
    {
      Debug.Log("Could not collect food, max already reached");
    }
  }
}
