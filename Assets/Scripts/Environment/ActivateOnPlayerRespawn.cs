using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnPlayerRespawn : MonoBehaviour
{
  // Update is called once per frame
  public virtual void Activate(bool newGame = false)
  {
    Debug.Log("BaseActivate??");
  }
}
