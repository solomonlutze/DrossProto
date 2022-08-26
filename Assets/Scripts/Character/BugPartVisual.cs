
using UnityEngine;
using UnityEngine.UI;
public class BugPartVisual : MonoBehaviour
{
  public TraitSlot slot;
  public BugSkeletonPartToVisualDictionary visualParts;

  public void InitForUI()
  {
    foreach (BugSkeletonPartVisual visual in visualParts.Values)
    {
      visual.gameObject.layer = LayerMask.NameToLayer("UiRenderTexture");
      visual.GetComponent<SpriteRenderer>().sortingLayerName = "UiRenderTexture";
    }
  }
}