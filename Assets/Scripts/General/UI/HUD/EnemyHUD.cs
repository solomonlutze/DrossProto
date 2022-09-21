using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUD : MonoBehaviour
{
  // public CarapaceBar carapaceBar;
  public HealthBar healthBar;
  public StaminaBar[] skillStaminaBars;
  // public StaminaBar staminaBar;


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

  Character character;
  void Start()
  {
    character = GetComponentInParent<Character>();
  }
  // Update is called once per frame
  void Update()
  {
    if (character != null)
    {
      healthBar.character = character; //todo: anywhere but update please!
      for (int i = 0; i < skillStaminaBars.Length; i++)
      {
        skillStaminaBars[i].character = character;
        skillStaminaBars[i].partStatusInfo = character.partStatusInfos[Trait.slots[i]];
      }
      // carapaceBar.character = character;
      // staminaBar.character = character;
    }
  }

}
