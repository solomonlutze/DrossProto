using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handles UI info to be displayed near the player character, such as contextual actions
// for general HUD please see PlayerHUD
public class PlayerCharacterHUD : MonoBehaviour
{
  public GameObject contextualActionObject;
  public TextMeshProUGUI selectedContextualActionText;

  public GameObject nextContextualActionObject;


  // STAMINA
  public CanvasGroup staminaBarContainer;
  public RectTransform staminaBarController;
  public Image staminaBarContentsSprite;

  public float staminaBarFadeTime = .5f;
  public Color staminaRecoveryColor;
  public Color lowStaminaColor;
  public Color defaultStaminaColor;
  private Coroutine staminaRecoveryFlashCoroutine;
  // END STAMINA

  private PlayerController pc;
  void Start()
  {
    pc = GetComponent<PlayerController>();
    staminaBarContentsSprite.color = defaultStaminaColor;
    staminaBarContainer.alpha = 0;
  }
  // Update is called once per frame
  void Update()
  {
    if (pc != null)
    {
      HandleContextualActions();
      HandleStaminaBar();
    }
  }

  void HandleContextualActions()
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

  void HandleStaminaBar()
  {
    float currentStamina = Mathf.Max(pc.GetCharacterVital(CharacterVital.RemainingStamina), 0);
    float maxStamina = pc.GetMaxStamina();
    if (currentStamina >= maxStamina)
    {
      staminaBarContainer.alpha -= Time.deltaTime / staminaBarFadeTime;
      staminaBarContainer.alpha = Mathf.Max(0, staminaBarContainer.alpha);
      return;
    }
    staminaBarContainer.alpha += Time.deltaTime / staminaBarFadeTime;
    staminaBarContainer.alpha = Mathf.Min(1, staminaBarContainer.alpha);
    staminaBarController.localScale = new Vector3(currentStamina / maxStamina, staminaBarController.localScale.y, 0);
    if (!pc.flying)
    {
      if (staminaRecoveryFlashCoroutine == null)
      {
        staminaRecoveryFlashCoroutine = StartCoroutine(RecoveryFlash());
      }
    }
    else if (currentStamina / maxStamina < .3)
    {
      staminaBarContentsSprite.color = lowStaminaColor;
    }
    else
    {
      if (staminaRecoveryFlashCoroutine != null)
      {
        StopCoroutine(staminaRecoveryFlashCoroutine);
        staminaRecoveryFlashCoroutine = null;
      }
      staminaBarContentsSprite.color = defaultStaminaColor;
    }
    // if the player is at full stamina: hide the bar
    // if the player is not at full stamina, 
    //   show the bar and adjust its scale to be stamina remaining/stamina total
    //   if the bar is white, change it to green
    //   if the player is not flying and the bar is green: change it to white
  }

  public IEnumerator RecoveryFlash()
  {
    Debug.Log("starting recovery flash!");
    while (pc.GetCharacterVital(CharacterVital.RemainingStamina) < pc.GetMaxStamina())
    {
      staminaBarContentsSprite.color = staminaBarContentsSprite.color == staminaRecoveryColor ? defaultStaminaColor : staminaRecoveryColor;
      yield return new WaitForSeconds(.2f);
    }
    staminaBarContentsSprite.color = defaultStaminaColor;
    staminaRecoveryFlashCoroutine = null;
  }
}
