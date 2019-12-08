using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ContextualActionHUD : MonoBehaviour
{
  public Canvas contextualActionCanvas;
  public TextMeshProUGUI selectedContextualActionText;

  public Canvas nextContextualActionCanvas;
  private PlayerController pc;
  void Start()
  {
    pc = GetComponent<PlayerController>();
  }
  // Update is called once per frame
  void Update()
  {
    if (pc != null && pc.availableContextualActions != null)
    {
      if (pc.availableContextualActions.Count > 0)
      {
        contextualActionCanvas.enabled = true;
        selectedContextualActionText.text = "e: " + pc.GetSelectedContextualAction().actionName;
      }
      else
      {
        contextualActionCanvas.enabled = false;
      }
      if (pc.availableContextualActions.Count > 1)
      {
        nextContextualActionCanvas.enabled = true;
      }
      else
      {
        nextContextualActionCanvas.enabled = false;
      }
    }
  }

}
