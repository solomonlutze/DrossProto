﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class exists only to map body parts to gameobjects to display visuals.
// Visuals are populated from scriptableobjects....... somehow
public class CharacterVisuals : MonoBehaviour
{
  public BugSkeletonPartToCharacterCharacterBodyPartVisualDictionary skeletonPartToCharacterBodyPartVisual;
  public static Dictionary<BugSkeletonPart, TraitSlot> bugSkeletonPartToTraitSlot = new Dictionary<BugSkeletonPart, TraitSlot>() {
    {BugSkeletonPart.HindlegRight, TraitSlot.Legs},
    {BugSkeletonPart.HindlegLeft, TraitSlot.Legs},
    {BugSkeletonPart.MidlegRight, TraitSlot.Legs},
    {BugSkeletonPart.MidlegLeft, TraitSlot.Legs},
    {BugSkeletonPart.ForelegRight, TraitSlot.Legs},
    {BugSkeletonPart.ForelegLeft, TraitSlot.Legs},
    {BugSkeletonPart.Abdomen, TraitSlot.Abdomen},
    {BugSkeletonPart.MandibleRight, TraitSlot.Head},
    {BugSkeletonPart.MandibleLeft, TraitSlot.Head},
    {BugSkeletonPart.Head, TraitSlot.Head},
    {BugSkeletonPart.Eyes, TraitSlot.Head},
    {BugSkeletonPart.AntennaRight, TraitSlot.Head},
    {BugSkeletonPart.AntennaLeft, TraitSlot.Head},
    {BugSkeletonPart.Thorax, TraitSlot.Thorax},
    {BugSkeletonPart.HindwingRight, TraitSlot.Wings},
    {BugSkeletonPart.HindwingLeft, TraitSlot.Wings},
    {BugSkeletonPart.ForewingRight, TraitSlot.Wings},
    {BugSkeletonPart.ForewingLeft, TraitSlot.Wings},
  };
  public Color32 defaultColor = Color.clear;

  public void SetCharacterVisuals(TraitSlotToTraitDictionary traits)
  {
    foreach (BugSkeletonPart part in (BugSkeletonPart[])Enum.GetValues(typeof(BugSkeletonPart)))
    {
      skeletonPartToCharacterBodyPartVisual[part].spriteRenderer1.sprite = traits[bugSkeletonPartToTraitSlot[part]].imagesData.bugSkeletonPartImages[part];
    }
  }

  public void SetCharacterVisualsFromSkeletonImagesData(BugSkeletonImagesData data)
  {
    foreach (BugSkeletonPart part in (BugSkeletonPart[])Enum.GetValues(typeof(BugSkeletonPart)))
    {
      skeletonPartToCharacterBodyPartVisual[part].spriteRenderer1.sprite = data.bugSkeletonPartImages[part];
    }
  }
  public void SetOverrideColor(Color32 overrideColor)
  {
    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    foreach (CharacterBodyPartVisual visual in skeletonPartToCharacterBodyPartVisual.Values)
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
    }
  }
  public void ClearCharacterVisuals()
  {

    foreach (BugSkeletonPart part in skeletonPartToCharacterBodyPartVisual.Keys)
    {
      CharacterBodyPartVisual visual = skeletonPartToCharacterBodyPartVisual[part];
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
