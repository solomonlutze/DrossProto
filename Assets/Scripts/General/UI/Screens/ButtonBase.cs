using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class ButtonBase : MonoBehaviour, IPointerEnterHandler, IDeselectHandler
{


  public void OnPointerEnter(PointerEventData data)
  {
    if (!EventSystem.current.alreadySelecting)
    {
      EventSystem.current.SetSelectedGameObject(this.gameObject);
    }
  }

  public void OnDeselect(BaseEventData data)
  {
    this.GetComponent<Selectable>().OnPointerExit(null);
  }

}