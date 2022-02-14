using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnPlayerRespawn : MonoBehaviour
{
  // Update is called once per frame
  public void Awake()
  {
    GameMaster.Instance.RegisterObjectToDestroyOnRespawn(gameObject);
  }
}
