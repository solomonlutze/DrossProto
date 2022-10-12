using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class exists only to map body parts to gameobjects to display visuals.
// Visuals are populated from scriptableobjects....... somehow
public class CharacterVisuals : MonoBehaviour
{
  public BugSkeletonPartToCharacterCharacterBodyPartVisualDictionary skeletonPartToCharacterBodyPartVisual;
  public List<BugSkeletonPart> unbrokenBugParts;
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
    {BugSkeletonPart.EyeLeft, TraitSlot.Head},
    {BugSkeletonPart.EyeRight, TraitSlot.Head},
    {BugSkeletonPart.AntennaRight, TraitSlot.Head},
    {BugSkeletonPart.AntennaLeft, TraitSlot.Head},
    {BugSkeletonPart.Thorax, TraitSlot.Thorax},
    {BugSkeletonPart.HindwingRight, TraitSlot.Wings},
    {BugSkeletonPart.HindwingLeft, TraitSlot.Wings},
    {BugSkeletonPart.ForewingRight, TraitSlot.Wings},
    {BugSkeletonPart.ForewingLeft, TraitSlot.Wings},
  };

  public TraitSlotToTransformDictionary bugParts;
  public Dictionary<TraitSlot, GameObject> bugPartPrefabs;
  public Color32 defaultColor = Color.clear;
  public Animator animator;

  public void Awake()
  {
    // Debug.Log("Awake??? ");
    unbrokenBugParts = new List<BugSkeletonPart>((BugSkeletonPart[])Enum.GetValues(typeof(BreakableBugSkeletonPart)));
    // bugParts = new Dictionary<TraitSlot, Transform>();
  }
  public void SetCharacterVisuals(TraitSlotToTraitDictionary traits, bool forUi = false)
  {
    Debug.Log("bugParts: " + bugParts);
    foreach (TraitSlot slot in traits.Keys)
    {
      if (bugParts.ContainsKey(slot) && bugParts[slot] != null)
      {
        Destroy(bugParts[slot].gameObject);
      }

      if (traits[slot].visualPrefab == null)
      {
        Debug.Log("failed to instantiate visualPrefab for trait " + traits[slot].traitName + ", slot " + slot);
        return;
      }
      BugPartVisual bugPartVisual = Instantiate(traits[slot].visualPrefab, transform);
      bugParts[slot] = bugPartVisual.transform;
      foreach (BugSkeletonPart part in bugParts[slot].GetComponent<BugPartVisual>().visualParts.Keys)
      {
        skeletonPartToCharacterBodyPartVisual[part] = bugPartVisual.visualParts[part];
      }
      bugPartVisual.gameObject.name = slot.ToString();
      if (forUi) { bugParts[slot].GetComponent<BugPartVisual>().InitForUI(); }
    }
    unbrokenBugParts = new List<BugSkeletonPart>((BugSkeletonPart[])Enum.GetValues(typeof(BreakableBugSkeletonPart)));
    StartCoroutine(RebindAnimator());
  }

  public IEnumerator RebindAnimator()
  {
    yield return new WaitForEndOfFrame();
    animator.Rebind();
  }
  // public void SetCharacterVisualsFromSkeletonImagesData(BugSkeletonImagesData data)
  // {
  //   foreach (BugSkeletonPart part in (BugSkeletonPart[])Enum.GetValues(typeof(BugSkeletonPart)))
  //   {
  //     skeletonPartToCharacterBodyPartVisual[part].spriteRenderer1.sprite = data.bugSkeletonPartImages[part];
  //   }
  // }
  public void SetOverrideColor(Color32 overrideColor)
  {
    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    // foreach (CharacterBodyPartVisual_OLD visual in skeletonPartToCharacterBodyPartVisual.Values)
    // {
    //   if (visual.spriteRenderer1 != null)
    //   {
    //     visual.spriteRenderer1.GetPropertyBlock(mpb);
    //     mpb.SetColor("_OverrideColor", overrideColor);
    //     visual.spriteRenderer1.SetPropertyBlock(mpb);
    //   }
    //   if (visual.spriteRenderer2 != null)
    //   {
    //     visual.spriteRenderer2.GetPropertyBlock(mpb);
    //     mpb.SetColor("_OverrideColor", overrideColor);
    //     visual.spriteRenderer1.SetPropertyBlock(mpb);
    //     visual.spriteRenderer2.SetPropertyBlock(mpb);
    //   }
    // }
  }
  // public void ClearCharacterVisuals()
  // {

  //   foreach (BugSkeletonPart part in skeletonPartToCharacterBodyPartVisual.Keys)
  //   {
  //     CharacterBodyPartVisual_OLD visual = skeletonPartToCharacterBodyPartVisual[part];
  //     visual.ClearSprites();
  //     visual.gameObject.SetActive(false);
  //   }
  // }

  public void DamageFlash(Color32 damageFlashColor)
  {
    StartCoroutine(DoDamageFlash(damageFlashColor));
  }

  IEnumerator DoDamageFlash(Color32 damageFlashColor)
  {
    SetOverrideColor(damageFlashColor);
    yield return new WaitForSeconds(.1f);
    SetOverrideColor(defaultColor);
    yield return new WaitForSeconds(.1f);
    SetOverrideColor(damageFlashColor);
    yield return new WaitForSeconds(.1f);
    SetOverrideColor(defaultColor);
  }

  public float linearDrag = 3;
  // public float linearDrag = 3;
  public float knockbackMult = 3.5f;
  public float knockbackMultJitter = .5f;
  public float knockbackAngleJitter = 25f;
  public float linearDragJitter = .5f;
  public float angularVelocity = 3000;
  public float angularDrag = 3;
  public float angularDragJitter = .5f;
  public float angularVelocityJitter = 500;
  public void BreakOffRandomBodyPart(Vector2 knockback)
  {
    int partToBreakIndex = UnityEngine.Random.Range(0, unbrokenBugParts.Count);
    BugSkeletonPartVisual partToBreakVisual = skeletonPartToCharacterBodyPartVisual[unbrokenBugParts[partToBreakIndex]];
    BreakOffBodyPart(knockback, partToBreakVisual);
    unbrokenBugParts.RemoveAt(partToBreakIndex);
  }

  public void BreakRandomBodyPartFromSlot(Vector2 knockback, TraitSlot slot, int count = 1)
  {
    List<BugSkeletonPart> unbrokenPartsOfType = new List<BugSkeletonPart>();
    foreach (BugSkeletonPart part in unbrokenBugParts)
    {
      if (bugSkeletonPartToTraitSlot[part] == slot)
      {
        unbrokenPartsOfType.Add(part);
      }
    }
    for (int i = 0; i < count; i++)
    {
      if (i < unbrokenPartsOfType.Count)
      {
        int partToBreakIndex = UnityEngine.Random.Range(0, unbrokenPartsOfType.Count);
        BugSkeletonPartVisual partToBreakVisual = skeletonPartToCharacterBodyPartVisual[unbrokenPartsOfType[partToBreakIndex]];
        BreakOffBodyPart(knockback, partToBreakVisual);
        unbrokenBugParts.RemoveAt(unbrokenBugParts.IndexOf(unbrokenPartsOfType[partToBreakIndex]));
        unbrokenPartsOfType.RemoveAt(partToBreakIndex);
      }
    }
  }
  public void BreakOffBodyPart(Vector2 knockback, BugSkeletonPartVisual partToBreakVisual)
  {
    GameObject brokenPart = partToBreakVisual.gameObject; //Instantiate(partToBreakVisual.gameObject, partToBreakVisual.transform.position, partToBreakVisual.transform.rotation);
    brokenPart.transform.parent = null;
    brokenPart.AddComponent<DestroyOnPlayerRespawn>();
    Rigidbody2D rb = brokenPart.AddComponent<Rigidbody2D>();
    float randomAngleMod = UnityEngine.Random.Range(-knockbackAngleJitter, knockbackAngleJitter);
    Vector2 modifiedKnockback = Quaternion.Euler(0, 0, randomAngleMod) * knockback;
    rb.velocity = modifiedKnockback * (knockbackMult + UnityEngine.Random.Range(-knockbackMultJitter, knockbackMultJitter));
    rb.drag = linearDrag + UnityEngine.Random.Range(-linearDragJitter, linearDragJitter);
    rb.angularVelocity = angularVelocity + UnityEngine.Random.Range(-angularVelocityJitter, angularVelocityJitter);
    rb.angularDrag = angularDrag + UnityEngine.Random.Range(-angularDragJitter, angularDragJitter);
    // partToBreakVisual.spriteRenderer1.sprite = null;
  }

  public void BreakOffRemainingBodyParts(Vector2 knockback)
  {
    foreach (BugSkeletonPartVisual visual in skeletonPartToCharacterBodyPartVisual.Values)
    {
      if (visual.transform.parent != null) // part isn't broken
      {
        BreakOffBodyPart(knockback, visual);
      }
    }
  }
}
