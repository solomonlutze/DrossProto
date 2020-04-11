using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterHUD : MonoBehaviour
{
  public GameObject contextualActionObject;
  public TextMeshProUGUI selectedContextualActionText;

  public GameObject nextContextualActionObject;

  public GameObject healthBarContainer;
  public GameObject healthBarController;
  public Sprite healthBarContentsSprite;
  private PlayerController pc;
  void Start()
  {
    pc = GetComponent<PlayerController>();
  }
  // Update is called once per frame
  void Update()
  {
    if (pc != null)
    {
      if (pc.availableContextualActions != null)
      {
        if (pc.availableContextualActions.Count > 0)
        {
          contextualActionObject.SetActive(true);
          selectedContextualActionText.text = "e: " + pc.GetSelectedContextualAction().actionName;
        }
        else
        {
          contextualActionObject.SetActive(false);
        }
        if (pc.availableContextualActions.Count > 1)
        {
          nextContextualActionObject.SetActive(true);
        }
        else
        {
          nextContextualActionObject.SetActive(false);
        }
      }
    }
  }

}
