using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUD : MonoBehaviour
{
  public CarapaceBar carapaceBar;
  public HealthBar healthBar;
  public StaminaBar staminaBar;


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
      carapaceBar.character = character;
      staminaBar.character = character;
    }
  }

}
