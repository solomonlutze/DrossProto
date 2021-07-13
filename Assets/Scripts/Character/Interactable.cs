using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// At some point this may contain shared logic; for now it's enough
// that it identifies an object as being something the player can interact with
public class Interactable : WorldObject
{

  public virtual string interactableText { get; set; }
  public virtual void OnRemove()
  {

  }
  public bool isInteractable = true;
}