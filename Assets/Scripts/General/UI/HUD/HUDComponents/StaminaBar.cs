using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{

  public CanvasGroup staminaBarContainer;
  public RectTransform staminaBarController;
  public RectTransform staminaBarContentsFill;
  public RectTransform staminaBarMaxContentsFill;
  public Image staminaBarContentsSprite
  {
    get
    {
      return staminaBarContentsFill == null ? null : staminaBarContentsFill.GetComponent<Image>();
    }
  }
  public Image staminaBarMaxContentsSprite
  {
    get
    {
      return staminaBarMaxContentsFill == null ? null : staminaBarMaxContentsFill.GetComponent<Image>();
    }
  }
  public Color lowStaminaColor;
  public Color defaultStaminaColor;
  public Character character;
  public PartStatusInfo partStatusInfo;
  public Vector3 defaultScale;
  public Vector3 defaultMaxScale;
  void Start()
  {
    Init();
    defaultScale = staminaBarContentsFill.localScale;
    defaultMaxScale = staminaBarMaxContentsFill.localScale;
  }
  // Update is called once per frame
  void Update()
  {
    if (character != null)
    {
      HandleStaminaBar();
    }
    else
    {
      Init();
    }
  }

  void Init()
  {
    staminaBarContentsSprite.color = defaultStaminaColor;
    staminaBarContainer.alpha = 1;
  }

  void HandleStaminaBar()
  {
    float currentStamina = Mathf.Max(partStatusInfo.currentExhaustion, 0);
    float maxStamina = 100;
    float currentMaxStamina = partStatusInfo.maxDamage - partStatusInfo.currentDamage;
    // if (!character.blocking && !character.staminaBroken)
    // {
    //   staminaBarContainer.alpha -= Time.deltaTime / staminaBarFadeTime;
    //   staminaBarContainer.alpha = Mathf.Max(.75f, staminaBarContainer.alpha);
    // }
    // else
    // {
    //   staminaBarContainer.alpha += Time.deltaTime / staminaBarFadeTime;
    //   staminaBarContainer.alpha = Mathf.Min(1, staminaBarContainer.alpha);
    // }
    staminaBarContentsFill.localScale = new Vector3(currentStamina / maxStamina * defaultScale.x, defaultScale.y, 0);
    staminaBarMaxContentsFill.localScale = new Vector3((maxStamina - currentMaxStamina) / maxStamina * defaultMaxScale.x, defaultMaxScale.y, 0);
    // if (character.staminaBroken)
    // {
    //   if (staminaBrokenFlashCoroutine == null)
    //   {
    //     staminaBrokenFlashCoroutine = StartCoroutine(StaminaBrokenFlash());
    //   }
    // }
  }

  // public IEnumerator StaminaBrokenFlash()
  // {
  //   while (character.staminaBroken)
  //   {
  //     staminaBarEmptySprite.color = staminaBarEmptySprite.color == brokenStaminaColor ? lowStaminaColor : brokenStaminaColor;
  //     yield return new WaitForSeconds(.2f);
  //   }
  //   staminaBarEmptySprite.color = Color.clear;
  //   staminaBrokenFlashCoroutine = null;
  // }


}
