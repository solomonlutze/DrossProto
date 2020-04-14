using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterHUD : MonoBehaviour
{
  public GameObject contextualActionObject;
  public TextMeshProUGUI selectedContextualActionText;

  public GameObject nextContextualActionObject;

  public CanvasGroup staminaBarContainer;
  public RectTransform staminaBarController;
  public Image staminaBarContentsSprite;

  public float staminaBarFadeTime = .5f;
  public Color recoveryColor;
  public Color lowStaminaColor;
  public Color defaultStaminaColor;
  private PlayerController pc;
  private Coroutine recoveryFlashCoroutine;
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
    float currentStamina = pc.GetCharacterVital(CharacterVital.RemainingFlightStamina);
    float maxStamina = pc.GetMaxFlightDuration();
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
      if (recoveryFlashCoroutine == null)
      {
        recoveryFlashCoroutine = StartCoroutine(RecoveryFlash());
      }
    }
    else if (currentStamina / maxStamina < .3)
    {
      staminaBarContentsSprite.color = lowStaminaColor;
    }
    else
    {
      StopCoroutine(recoveryFlashCoroutine);
      recoveryFlashCoroutine = null;
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
    while (pc.GetCharacterVital(CharacterVital.RemainingFlightStamina) < pc.GetMaxFlightDuration())
    {
      staminaBarContentsSprite.color = staminaBarContentsSprite.color == recoveryColor ? defaultStaminaColor : recoveryColor;
      yield return new WaitForSeconds(.2f);
    }
    staminaBarContentsSprite.color = defaultStaminaColor;
    recoveryFlashCoroutine = null;
  }
}
