using UnityEngine;
public class UIBug : MonoBehaviour
{
  public Animator animator;
  public TraitSlotToTraitDictionary traits;
  public TraitSlotToTraitDictionary overriddenTraits;
  public CharacterVisuals characterVisuals;

  public void Init(TraitSlotToTraitDictionary t)
  {
    traits = t;
    PopulateVisuals(t);
  }

  public void PopulateVisuals(TraitSlotToTraitDictionary traitsForVisuals)
  {
    Debug.Log("traits for visuals " + traitsForVisuals);
    Debug.Log("characterVisuals " + characterVisuals);
    characterVisuals.SetCharacterVisuals(traitsForVisuals);
    animator.SetFloat("HeadAnimationType", (int)traitsForVisuals[TraitSlot.Head].bugSpecies);
    animator.SetFloat("ThoraxAnimationType", (int)traitsForVisuals[TraitSlot.Thorax].bugSpecies);
    animator.SetFloat("AbdomenAnimationType", (int)traitsForVisuals[TraitSlot.Abdomen].bugSpecies);
    animator.SetFloat("LegsAnimationType", (int)traitsForVisuals[TraitSlot.Legs].bugSpecies);
    animator.SetFloat("WingsAnimationType", (int)traitsForVisuals[TraitSlot.Wings].bugSpecies);
  }
  public void HighlightSlot(TraitSlot slot, Trait overrideTrait)
  {
    overriddenTraits = new TraitSlotToTraitDictionary(traits);
    overriddenTraits[slot] = overrideTrait;
    PopulateVisuals(overriddenTraits);
  }


  public void UnhighlightSlot()
  {
    PopulateVisuals(traits);
  }
}