using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
  //TODO: Use TextMeshProUGUI instead of STM
  public IntVariable grubCount;
  public StringVariable areaName;
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
  public SuperTextMesh areaNameText;
  public CanvasGroup areaNameCanvasGroup;

  bool playerExists;
  // public TextMeshProUGUI skill1ValueText;
  // public TextMeshProUGUI skill2TitleText;
  // public TextMeshProUGUI skill2ValueText;

  float areaNameDisplayTime;
  public float areaNameDisplayDuration;
  public float areaNameFadeOutDuration;
  private string diedString = "This body has been destroyed.\nPress Return to be reborn.";
  private string obliteratedString = "This body has been swallowed by Oblivion.\nPress Return to be reborn.";
  // Use this for initialization
  // TODO: these should be public properties we set in the inspector
  void Start()
  {
    diedText = transform.Find("DiedText").GetComponent<SuperTextMesh>();
  }

  // Update is called once per frame
  void Update()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    // if (areaNameDisplayTime < areaNameDisplayDuration + areaNameFadeOutDuration)
    // {
    areaNameDisplayTime += Time.deltaTime;
    FadeOutAreaName();
    // }
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
          selectedSkillTexts[i].text = playerController.characterSkills[Trait.slots[i]].displayName;
        }
      }
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
    grubCountText.text = "" + grubCount.Value;
  }

  public void SetAreaName()
  {
    areaNameText.text = areaName.Value;
    areaNameDisplayTime = 0;
  }

  public void FadeOutAreaName()
  {
    float fadeOutProgress = Mathf.Clamp((areaNameDisplayTime - areaNameDisplayDuration) / areaNameFadeOutDuration, 0, 1);
    areaNameCanvasGroup.alpha = 1 - fadeOutProgress;
  }
}
