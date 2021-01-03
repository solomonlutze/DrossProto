using UnityEngine;
using System.Collections;

// This class exists only to map body parts to gameobjects to display visuals.
// Visuals are populated from scriptableobjects....... somehow
public class CharacterVisuals : MonoBehaviour
{
    public TraitSlotToCharacterCharacterBodyPartVisualDictionary traitSlotsToVisuals;
    public Color32 defaultColor = Color.clear;

    public void SetCharacterVisuals(TraitSlotToTraitDictionary traits)
    {
        foreach (TraitSlot slot in traits.Keys)
        {
            if (!traitSlotsToVisuals.ContainsKey(slot))
            {
                Debug.LogError("Need to assign body part transforms to character visuals on character " + gameObject.name);
            }
            else
            {
                CharacterBodyPartVisual visual = traitSlotsToVisuals[slot];
                visual.SetSpritesFromTrait(traits[slot]);
                visual.gameObject.SetActive(true);
            }
        }
    }

    public void SetOverrideColor(Color32 overrideColor)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        foreach (CharacterBodyPartVisual visual in traitSlotsToVisuals.Values)
        {
            if (visual.spriteRenderer1 != null)
            {
                visual.spriteRenderer1.GetPropertyBlock(mpb);
                mpb.SetColor("_OverrideColor", overrideColor);
                visual.spriteRenderer1.SetPropertyBlock(mpb);
            }
            if (visual.spriteRenderer2 != null)
            {
                visual.spriteRenderer2.GetPropertyBlock(mpb);
                mpb.SetColor("_OverrideColor", overrideColor);
                visual.spriteRenderer1.SetPropertyBlock(mpb);
                visual.spriteRenderer2.SetPropertyBlock(mpb);
            }
            // visual.spriteRenderer2.SetPropertyBlock(mpb);
        }
    }
    public void ClearCharacterVisuals()
    {

        foreach (TraitSlot slot in traitSlotsToVisuals.Keys)
        {
            CharacterBodyPartVisual visual = traitSlotsToVisuals[slot];
            visual.ClearSprites();
            visual.gameObject.SetActive(false);
        }
    }

    public void DamageFlash(Color32 damageFlashColor)
    {
        StartCoroutine(DoDamageFlash(damageFlashColor));
    }

    IEnumerator DoDamageFlash(Color32 damageFlashColor)
    {
        Debug.Log("Doing damage flash");
        SetOverrideColor(damageFlashColor);
        yield return new WaitForSeconds(.1f);
        SetOverrideColor(defaultColor);
        yield return new WaitForSeconds(.1f);
        SetOverrideColor(damageFlashColor);
        yield return new WaitForSeconds(.1f);
        SetOverrideColor(defaultColor);
    }
}
