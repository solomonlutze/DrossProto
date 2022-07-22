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
    characterVisuals.SetCharacterVisuals(traitsForVisuals);
    animator.SetFloat("HeadAnimationType", (int)traitsForVisuals[TraitSlot.Head].bugSpecies_DEPRECATED);
    animator.SetFloat("ThoraxAnimationType", (int)traitsForVisuals[TraitSlot.Thorax].bugSpecies_DEPRECATED);
    animator.SetFloat("AbdomenAnimationType", (int)traitsForVisuals[TraitSlot.Abdomen].bugSpecies_DEPRECATED);
    animator.SetFloat("LegsAnimationType", (int)traitsForVisuals[TraitSlot.Legs].bugSpecies_DEPRECATED);
    animator.SetFloat("WingsAnimationType", (int)traitsForVisuals[TraitSlot.Wings].bugSpecies_DEPRECATED);
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