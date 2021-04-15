using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handles UI info to be displayed near the player character, such as contextual actions
public class PlayerCharacterHUD : MonoBehaviour
{
  public GameObject contextualActionObject;
  public TextMeshProUGUI selectedContextualActionText;

  public GameObject nextContextualActionObject;


  // STAMINA
  // public CanvasGroup staminaBarContainer;
  // public RectTransform staminaBarController;
  // public Image staminaBarContentsSprite;

  // public float staminaBarFadeTime = .5f;
  // public Color staminaRecoveryColor;
  // public Color lowStaminaColor;
  // public Color defaultStaminaColor;
  // private Coroutine staminaRecoveryFlashCoroutine;
  // END STAMINA

  PlayerController pc;
  void Start()
  {
    pc = GetComponentInParent<PlayerController>();
  }
  // Update is called once per frame
  void Update()
  {
    if (pc != null)
    {
      HandleContextualActions();
    }
  }

  void HandleContextualActions()
  {
    if (pc != null && pc.availableContextualActions != null)
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
