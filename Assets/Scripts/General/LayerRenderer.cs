using UnityEngine;
using UnityEngine.Tilemaps;
using ScriptableObjectArchitecture;

[ExecuteInEditMode]
public class LayerRenderer : MonoBehaviour
{
  public float fadeDampTime = 0.005f;
  public float fadeTime = 0.25f;
  float currentOpacity = 0;
  bool shouldBeVisible = false;
  public FloorLayer floorLayer;
  public IntVariable currentFloorLayer;

  void Awake()
  {
    currentOpacity = .001f; // dumb hack; will force a changeOpacity update that will turn off renderers
  }

  void Update()
  {
    if (!FinishedChangingOpacity())
    {
      currentOpacity += Time.deltaTime / fadeTime * (shouldBeVisible ? 1 : -1);
      ChangeOpacityRecursively(transform);
    }
  }

  bool FinishedChangingOpacity()
  {
    return (shouldBeVisible && currentOpacity >= 1) || (!shouldBeVisible && currentOpacity <= 0);
  }

  public void ChangeTargetOpacity()
  {
    int floorOffsetFromCurrentLayer = (int)floorLayer - currentFloorLayer.Value; // positive means we are above player; negative means we are below
    if (floorOffsetFromCurrentLayer > 0 || floorOffsetFromCurrentLayer < -3)
    {
      Debug.Log("received changeTargetOpacity for " + gameObject + "; should no longer be visible");
      shouldBeVisible = false;
    }
    else
    {
      shouldBeVisible = true;
    }
  }

  public void ChangeOpacityRecursively(Transform trans)
  {
    SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
    if (r != null)
    {
      r.color = new Color(r.color.r, r.color.g, r.color.b, currentOpacity);
      if (currentOpacity <= 0) { r.enabled = false; }
      else { r.enabled = true; }
    }
    Tilemap t = trans.gameObject.GetComponent<Tilemap>();
    if (t != null)
    {
      // actualNewOpacity = Mathf.SmoothDamp(t.color.a, targetOpacity, ref _ref, fadeDampTime);
      t.color = new Color(t.color.r, t.color.g, t.color.b, currentOpacity);
    }
    TilemapRenderer tr = trans.GetComponent<TilemapRenderer>();
    if (tr != null)
    {
      // Debug.Log("should be disabling " + tr);
      if (currentOpacity <= 0)
      {
        tr.enabled = false;
        Debug.Log("should be disabling " + tr);
      }
      else { tr.enabled = true; }
    }
    foreach (Transform child in trans)
    {
      ChangeOpacityRecursively(child);
    }
  }
}