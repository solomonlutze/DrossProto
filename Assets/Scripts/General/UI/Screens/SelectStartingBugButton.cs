using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
[RequireComponent(typeof(Selectable))]
public class SelectStartingBugButton : ButtonBase, ISelectHandler
{
  public StartingBugSelectScreen parentScreen;
  public int idx;
  public SuperTextMesh text;
  public Button button;
  public void Awake()
  {
    button.onClick.AddListener(ConfirmBug);
  }

  public void ConfirmBug()
  {
    parentScreen.SelectBug(idx);
  }

  public void OnSelect(BaseEventData data)
  {
    parentScreen.HighlightBug(idx);
  }

}