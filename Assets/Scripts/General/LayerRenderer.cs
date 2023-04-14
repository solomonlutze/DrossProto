using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using ScriptableObjectArchitecture;

[ExecuteInEditMode]
public class LayerRenderer : MonoBehaviour
{
  public float fadeDampTime = 0.005f;
  public float fadeTime = 0.25f;
  float currentOpacity = 0;
  public bool becomingVisible = false;
  public static bool showFloorAbove = false;
  // public FloorLayer floorLayer;
  public IntVariable currentFloorLayer;
  public bool executeInPlayMode = true;

  bool ShouldExecute()
  {
    return !Application.IsPlaying(gameObject) && executeInPlayMode;
  }
  void Awake()
  {
    if (ShouldExecute())
    {
      Debug.Log("should execute?", gameObject);
      currentOpacity = .001f; // dumb hack; will force a changeOpacity update that will turn off renderers
    }
    else
    {
      ChangeOpacityRecursively(transform, 1);
    }
  }

  void Update()
  {
    Debug.Log("update?");
    if (ShouldExecute())
    {
      Debug.Log("should execute?");
      // ChangeOpacityRecursively(transform, 1);

      // if (!FinishedChangingOpacity(becomingVisible, currentOpacity, GetTargetOpacity()))
      // {
      //   // WARNING: AI and LayerRenderer tightly coupled for camouflage.
      //   // See AiStateController.HandleVisibility. 
      //   // AiStateController ai = GetComponent<AiStateController>();
      //   // if (ai != null && shouldBeVisible) // AI handling opacity
      //   // {
      //   //   return;
      //   // }
      //   currentOpacity += Time.deltaTime / fadeTime * (becomingVisible ? 1 : -1);
      //   if (FinishedChangingOpacity(becomingVisible, currentOpacity, GetTargetOpacity()))
      //   {
      //     currentOpacity = GetTargetOpacity();
      //     ChangeOpacityRecursively(transform, currentOpacity); // prevent overshooting our desired opagcity
      //   }
      //   else
      //   {
      //     Debug.Log("changing opacity?" + currentOpacity, gameObject);
      //     ChangeOpacityRecursively(transform, currentOpacity);
      //   }
      // }
    }
  }

  public float GetTargetOpacity()
  {
    return 1;
    int floorOffsetFromCurrentLayer = (int)WorldObject.GetFloorLayerFromGameObjectLayer(gameObject.layer) - currentFloorLayer.Value;
    if (floorOffsetFromCurrentLayer > 1 || floorOffsetFromCurrentLayer < -3)
    {
      return 1;
    }
    if (floorOffsetFromCurrentLayer == 1)
    {
      if (showFloorAbove)
      {
        return 1f;
      }
      return 1;
    }
    return 1;
  }

  public static bool FinishedChangingOpacity(bool becomingVisible, float currentOpacity, float targetOpacity)
  {
    return (becomingVisible && currentOpacity > targetOpacity) || (!becomingVisible && currentOpacity < targetOpacity);
  }

  // called from event handler. gotta set it up or this script does nothing!
  public void ChangeTargetOpacity()
  {
    int floorOffsetFromCurrentLayer = (int)WorldObject.GetFloorLayerFromGameObjectLayer(gameObject.layer) - currentFloorLayer.Value; // positive means we are above player; negative means we are below

    if (floorOffsetFromCurrentLayer == 1)
    {
      becomingVisible = GetTargetOpacity() > currentOpacity;
    }
    else if (floorOffsetFromCurrentLayer > 1 || floorOffsetFromCurrentLayer < -3)
    {
      becomingVisible = false;
    }
    else
    {
      becomingVisible = true;
    }
  }

  public static void ChangeOpacityRecursively(Transform trans, float currentOpacity)
  {
    Debug.Log("changeOpacityRecursively " + trans.gameObject.name + " " + currentOpacity, trans.gameObject);
    SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
    if (r != null)
    {
      r.color = new Color(r.color.r, r.color.g, r.color.b, currentOpacity);
      if (false) { r.enabled = false; }
      else { r.enabled = true; }
    }
    Tilemap t = trans.gameObject.GetComponent<Tilemap>();
    if (t != null)
    {
      t.color = new Color(t.color.r, t.color.g, t.color.b, currentOpacity);
    }
    TilemapRenderer tr = trans.GetComponent<TilemapRenderer>();
    if (tr != null)
    {
      if (false)
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


#if UNITY_EDITOR
  [MenuItem("CustomTools/ToggleAboveFloorOpacity")]
  public static void ToggleAboveFloorOpacity()
  {
    showFloorAbove = !showFloorAbove;
  }
#endif
}

