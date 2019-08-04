using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour {
	//TODO: Use TextMeshProUGUI instead of STM
	private SuperTextMesh healthFieldText;
	private SuperTextMesh healthValueText;
	private SuperTextMesh maxHealthValueText;
	private SuperTextMesh diedText;
	private SuperTextMesh respawnText;

	private SuperTextMesh activatedAbilityText;

  public TextMeshProUGUI skill1TitleText;
  public TextMeshProUGUI skill1ValueText;
  public TextMeshProUGUI skill2TitleText;
  public TextMeshProUGUI skill2ValueText;

	private string diedString = "This body has been destroyed.\nPress Return to be reborn.";
	// Use this for initialization
	// TODO: these should be public properties we set in the inspector
	void Start () {
		healthFieldText = transform.Find("HealthFieldText").GetComponent<SuperTextMesh>();
		healthValueText = transform.Find("HealthValueText").GetComponent<SuperTextMesh>();
		maxHealthValueText = transform.Find("MaxHealthValueText").GetComponent<SuperTextMesh>();
		activatedAbilityText = transform.Find("ActivatedAbilityText").GetComponent<SuperTextMesh>();
		activatedAbilityText.text = " ";
		diedText = transform.Find("DiedText").GetComponent<SuperTextMesh>();
	}

	// Update is called once per frame
	void Update () {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
			healthFieldText.text = "Health: ";
			healthValueText.text = Mathf.Max(Mathf.Round(playerController.vitals[CharacterVital.CurrentHealth]), 0).ToString();
			maxHealthValueText.text = Mathf.Round(playerController.GetStat(CharacterStat.MaxHealth)).ToString();
			if (playerController.vitals[CharacterVital.CurrentHealth] > 0) {
				diedText.text = " ";
			}
			if (playerController.lastActivatedTrait != null && playerController.lastActivatedTrait != "") {
				activatedAbilityText.text = playerController.lastActivatedTrait + " Activated!";
			} else {
				activatedAbilityText.text = " ";
			}
      skill1ValueText.text = playerController.skill1 ? playerController.skill1.GetTraitName() : "[None]";
      skill2ValueText.text = playerController.skill2 ? playerController.skill2.GetTraitName() : "[None]";
		}
		else { //  You Died, presumably
			healthValueText.text = "0";
			if (diedText.text == " ") {
				diedText.text = diedString;
			}
		}
	}

}
