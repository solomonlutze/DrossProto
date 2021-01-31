using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
  //TODO: Use TextMeshProUGUI instead of STM
  public HealthBar healthBar;
  public CarapaceBar carapaceBar;
  public StaminaBar staminaBar;
  // private SuperTextMesh healthFieldText;
  // private SuperTextMesh healthValueText;
  // private SuperTextMesh maxHealthValueText;
  private SuperTextMesh diedText;
  private SuperTextMesh respawnText;

  // private SuperTextMesh activatedAbilityText;

  // public TextMeshProUGUI selectedSkillText;
  // public TextMeshProUGUI skill1ValueText;
  // public TextMeshProUGUI skill2TitleText;
  // public TextMeshProUGUI skill2ValueText;

  private string diedString = "This body has been destroyed.\nPress Return to be reborn.";
  // Use this for initialization
  // TODO: these should be public properties we set in the inspector
  void Start()
  {
    // healthFieldText = transform.Find("HealthFieldText").GetComponent<SuperTextMesh>();
    // healthValueText = transform.Find("HealthValueText").GetComponent<SuperTextMesh>();
    // maxHealthValueText = transform.Find("MaxHealthValueText").GetComponent<SuperTextMesh>();
    // activatedAbilityText = transform.Find("ActivatedAbilityText").GetComponent<SuperTextMesh>();
    // activatedAbilityText.text = " ";
    diedText = transform.Find("DiedText").GetComponent<SuperTextMesh>();
  }

  // Update is called once per frame
  void Update()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      healthBar.character = playerController; //todo: anywhere but update please!
      carapaceBar.character = playerController;
      staminaBar.character = playerController;
      if (playerController.GetCharacterVital(CharacterVital.CurrentHealth) > 0)
      {
        diedText.text = " ";
      }
    }
    else
    { //  You Died, presumably
      // healthValueText.text = "0";
      if (diedText.text == " ")
      {
        diedText.text = diedString;
      }
    }
  }

}
