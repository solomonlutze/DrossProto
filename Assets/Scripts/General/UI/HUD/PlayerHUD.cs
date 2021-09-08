using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
  //TODO: Use TextMeshProUGUI instead of STM
  public IntVariable grubCount;
  public HealthBar healthBar;
  public CarapaceBar carapaceBar;
  public StaminaBar staminaBar;
  public SuperTextMesh grubCountText;
  // private SuperTextMesh healthFieldText;
  // private SuperTextMesh healthValueText;
  // private SuperTextMesh maxHealthValueText;
  private SuperTextMesh diedText;
  private SuperTextMesh respawnText;

  // private SuperTextMesh activatedAbilityText;

  public SuperTextMesh[] selectedSkillTexts;
  public SuperTextMesh selectedAttackSkillText;
  public SuperTextMesh selectedAttackSkillDescriptionText;
  public SuperTextMesh moltDescriptionText;

  bool playerExists;
  // public TextMeshProUGUI skill1ValueText;
  // public TextMeshProUGUI skill2TitleText;
  // public TextMeshProUGUI skill2ValueText;

  private string diedString = "This body has been destroyed.\nPress Return to be reborn.";
  private string obliteratedString = "This body has been swallowed by Oblivion.\nPress Return to be reborn.";
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
      if (!playerExists)
      {
        playerExists = true;
        healthBar.character = playerController;
        moltDescriptionText.text = "Hold button to shed max health. After several seconds, health is fully restored to new maximum.\n" + playerController.characterSkills[TraitSlot.Thorax].description;
        diedText.text = " ";
        for (int i = 0; i < selectedSkillTexts.Length; i++)
        {
          // if (selectedSkill != playerController.characterSkills[playerController.selectedSkillIdx])
          // {
          //   selectedSkill = playerController.characterSkills[playerController.selectedSkillIdx];
          selectedSkillTexts[i].text = playerController.characterSkills[Trait.slots[i]].displayName;
          // }
        }
      }
      // TODO: drive this off an event maybe
      // if (selectedAttack != playerController.characterAttackSkills[playerController.selectedAttackSkillIdx])
      // {
      //   selectedAttack = playerController.characterAttackSkills[playerController.selectedAttackSkillIdx];
      //   selectedAttackSkillText.text = playerController.characterAttackSkills[playerController.selectedAttackSkillIdx].displayName;
      //   selectedAttackSkillDescriptionText.text = playerController.characterAttackSkills[playerController.selectedAttackSkillIdx].description;
      // }
      // if (selectedSkill != playerController.characterSkills[playerController.selectedSkillIdx])
      // {
      //   selectedSkill = playerController.characterSkills[playerController.selectedSkillIdx];
      //   selectedSkillText.text = playerController.characterSkills[playerController.selectedSkillIdx].displayName;
      //   selectedSkillDescriptionText.text = playerController.characterSkills[playerController.selectedSkillIdx].description;
      // }
    }
    else if (playerExists && playerController == null)
    { //  You Died, presumably
      // healthValueText.text = "0";
      playerExists = false;
      if (diedText.text == " ")
      {
        if (GameMaster.Instance.playerObliterated)
        {
          diedText.text = obliteratedString;
        }
        else
        {
          diedText.text = diedString;
        }
      }
    }
  }

  public void SetGrubCount()
  {
    Debug.Log("setting grub count text to " + grubCount.Value);
    grubCountText.text = "" + grubCount.Value;
  }

}
