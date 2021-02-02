using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaBar : MonoBehaviour
{

  // CARAPACE
  public CanvasGroup staminaBarContainer;
  public RectTransform staminaBarController;
  public RectTransform staminaBarContentsFill;
  public Image staminaBarContentsSprite
  {
    get
    {
      Debug.Log("staminaBarContentsFill " + staminaBarContentsFill);
      return staminaBarContentsFill == null ? null : staminaBarContentsFill.GetComponent<Image>();
    }
  }

  // public RectTransform staminaBarEmpty;
  // public Image staminaBarEmptySprite
  // {
  //   get
  //   {
  //     return staminaBarEmpty == null ? null : staminaBarEmpty.GetComponent<Image>();
  //   }
  // }

  // public float staminaBarFadeTime = .5f;
  public Color lowStaminaColor;
  public Color defaultStaminaColor;
  // private Coroutine staminaBrokenFlashCoroutine;

  // END CARAPACE
  public Character character;
  void Start()
  {
    Init();
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
    float currentStamina = Mathf.Max(character.GetCharacterVital(CharacterVital.CurrentStamina), 0);
    float maxStamina = character.GetMaxStamina();
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
    staminaBarContentsFill.localScale = new Vector3(currentStamina / maxStamina, staminaBarContentsFill.localScale.y, 0);
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
