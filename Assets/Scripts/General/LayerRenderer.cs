using UnityEngine;
using UnityEngine.Tilemaps;
using ScriptableObjectArchitecture;

[ExecuteInEditMode]
public class LayerRenderer : MonoBehaviour
{
  public float fadeDampTime = 0.005f;
  public float fadeTime = 0.25f;
  float currentOpacity = 0;
  public bool shouldBeVisible = false;
  public FloorLayer floorLayer;
  public IntVariable currentFloorLayer;

  void Awake()
  {
    currentOpacity = .001f; // dumb hack; will force a changeOpacity update that will turn off renderers
  }

  void Update()
  {
    if (!FinishedChangingOpacity(shouldBeVisible, currentOpacity))
    {
      // WARNING: AI and LayerRenderer tightly coupled for camouflage.
      // See AiStateController.HandleVisibility. 
      AiStateController ai = GetComponent<AiStateController>();
      if (ai != null && shouldBeVisible) // AI handling opacity
      {
        return;
      }
      currentOpacity += Time.deltaTime / fadeTime * (shouldBeVisible ? 1 : -1);
      ChangeOpacityRecursively(transform, currentOpacity);
    }
  }

  public static bool FinishedChangingOpacity(bool shouldBeVisible, float currentOpacity)
  {
    return (shouldBeVisible && currentOpacity >= 1) || (!shouldBeVisible && currentOpacity <= 0);
  }

  // called from event handler. gotta set it up or this script does nothing!
  public void ChangeTargetOpacity()
  {
    int floorOffsetFromCurrentLayer = (int)floorLayer - currentFloorLayer.Value; // positive means we are above player; negative means we are below

    if (floorOffsetFromCurrentLayer > 0 || floorOffsetFromCurrentLayer < -3)
    {
      Debug.Log(gameObject.name + " received event for floor layer " + currentFloorLayer.Value + " - shouldn't be visible; own layer " + floorLayer);
      shouldBeVisible = false;
    }
    else
    {
      Debug.Log(gameObject.name + " received event for changing floor layer - should be visible");
      shouldBeVisible = true;
    }
  }

  public static void ChangeOpacityRecursively(Transform trans, float currentOpacity)
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
      }
      else { tr.enabled = true; }
    }
    CanvasGroup c = trans.GetComponent<CanvasGroup>();
    if (c != null)
    {
      c.alpha = currentOpacity;
    }
    foreach (Transform child in trans)
    {
      ChangeOpacityRecursively(child, currentOpacity);
    }
  }
}