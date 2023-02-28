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
  public ElementalBuildupBar heatBuildupBar;
  public ElementalBuildupBar acidBuildupBar;
  public ElementalBuildupBar fungalBuildupBar;
  // public StaminaBar staminaBar;
  public SuperTextMesh grubCountText;
  public SuperTextMesh dietText;
  public SuperTextMesh eggProgressText;
  private SuperTextMesh diedText;
  private SuperTextMesh victoryText;
  private SuperTextMesh respawnText;

  public StaminaBar[] skillStaminaBars;
  public SuperTextMesh[] selectedSkillTexts;
  public SuperTextMesh selectedAttackSkillText;
  public SuperTextMesh selectedAttackSkillDescriptionText;
  public SuperTextMesh moltDescriptionText;
  public SuperTextMesh areaNameText;
  public CanvasGroup areaNameCanvasGroup;
  public SuperTextMesh itemPickupNameText;
  public CanvasGroup itemPickupCanvasGroup;

  bool playerExists;

  float areaNameDisplayTime;
  float itemPickupDisplayTime;
  public float itemPickupDisplayDuration;
  public float itemPickupFadeOutDuration;
  public float areaNameDisplayDuration;
  public float areaNameFadeOutDuration;
  private string diedString = "This body has been destroyed.\nPress Return/Start to be reborn.";
  private string victoryString = "All the other bugs are dead.\nYou win!";
  private string obliteratedString = "This body has been swallowed by Oblivion.\nPress Return to be reborn.";
  // Use this for initialization
  // TODO: these should be public properties we set in the inspector
  void Start()
  {
    diedText = transform.Find("DiedText").GetComponent<SuperTextMesh>();
    victoryText = transform.Find("VictoryText").GetComponent<SuperTextMesh>();
    itemPickupNameText.text = "";
    ClearVictoryText();
  }

  // Update is called once per frame
  void Update()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    FadeOutAreaName();
    FadeOutPickupItem();
    if (playerController != null)
    {
      if (!playerExists)
      {
        playerExists = true;
        healthBar.character = playerController;
        heatBuildupBar.character = playerController;
        acidBuildupBar.character = playerController;
        fungalBuildupBar.character = playerController;
        for (int i = 0; i < skillStaminaBars.Length; i++)
        {
          skillStaminaBars[i].character = playerController;
          skillStaminaBars[i].partStatusInfo = playerController.partStatusInfos[Trait.slots[i]];
        }
        moltDescriptionText.text = "Hold button to shed max health. After several seconds, health is fully restored to new maximum.\n" + playerController.characterSkills[TraitSlot.Thorax].description;
        diedText.text = " ";
        for (int i = 0; i < selectedSkillTexts.Length; i++)
        {
          selectedSkillTexts[i].text = playerController.characterSkills[Trait.slots[i]].displayName;
        }
      }
      List<string> dietString = new List<string>() { "Diet: " };
      foreach (FoodType food in GameMaster.Instance.GetDiet().Keys)
      {
        dietString.Add(food.ToString());
        dietString.Add(GameMaster.Instance.GetEatenFoodOfType(food).ToString());
        dietString.Add("/");
        dietString.Add(GameMaster.Instance.GetDiet()[food].ToString());
        dietString.Add(", ");
      }
      UnityEngine.Debug.Log(System.String.Join("", dietString));
      dietText.text = System.String.Join("", dietString);
      eggProgressText.text = "Food for egg: " + GameMaster.Instance.collectedFood.Count + "/" + GameMaster.Instance.foodRequiredForEgg;
    }
    else if (playerExists && playerController == null)
    { //  Character just died (presumably)
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

  public void SetVictoryText()
  {
    victoryText.text = victoryString;
  }

  public void ClearVictoryText()
  {
    victoryText.text = " ";
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
    areaNameDisplayTime += Time.deltaTime;
    float fadeOutProgress = Mathf.Clamp((areaNameDisplayTime - areaNameDisplayDuration) / areaNameFadeOutDuration, 0, 1);
    areaNameCanvasGroup.alpha = 1 - fadeOutProgress;
  }

  public void SetPickupItem(string itemName)
  {
    itemPickupNameText.text = itemName;
    itemPickupDisplayTime = 0;
  }

  public void FadeOutPickupItem()
  {
    itemPickupDisplayTime += Time.deltaTime;
    float fadeOutProgress = Mathf.Clamp((itemPickupDisplayTime - itemPickupDisplayDuration) / itemPickupFadeOutDuration, 0, 1);
    itemPickupCanvasGroup.alpha = 1 - fadeOutProgress;
  }
}
