using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBackground : MonoBehaviour
{

  public SuperTextMesh stm;
  private SpriteRenderer sprite;
  // Use this for initialization
  void Start()
  {
    stm = transform.GetComponentInChildren<SuperTextMesh>();
    if (stm == null)
    {
      // Debug.LogError("No SuperTextMesh in DialogueBackground's children: "+gameObject.name);
    }
    sprite = GetComponent<SpriteRenderer>();
    if (sprite == null)
    {
      Debug.LogError("No sprite on DialogueBackground object: " + gameObject.name);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (stm != null && stm.text != " ")
    {
      sprite.enabled = true;
    }
    else
    {
      sprite.enabled = false;
    }
  }
}
