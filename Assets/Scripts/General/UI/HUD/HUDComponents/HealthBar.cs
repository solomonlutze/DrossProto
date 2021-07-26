using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{

  // STAMINA
  public CanvasGroup healthBarContainer;
  public RectTransform healthBarController;
  public RectTransform healthBarContentsFill;
  public Image healthBarContentsSprite
  {
    get
    {
      return healthBarContentsFill == null ? null : healthBarContentsFill.GetComponent<Image>();
    }
  }

  public RectTransform healthBarMaxFill;
  public Image healthBarMaxSprite
  {
    get
    {
      return healthBarMaxFill == null ? null : healthBarMaxFill.GetComponent<Image>();
    }
  }

  public float healthBarFadeTime = .5f;
  // public Color healthRecoveryColor;
  // public Color lowHealthColor;
  public Color defaultHealthColor;
  public Color defaultMaxHealthColor;
  private Coroutine healthRecoveryFlashCoroutine;

  public Character character;
  void Start()
  {
    if (character == null)
    {
      character = GetComponentInParent<Character>();
    }
    healthBarContentsSprite.color = defaultHealthColor;
    healthBarMaxSprite.color = defaultMaxHealthColor;
    healthBarContainer.alpha = 0;
  }
  // Update is called once per frame
  void Update()
  {
    if (character != null)
    {
      HandleHealthBar();
    }
  }

  void HandleHealthBar()
  {
    float currentHealth = Mathf.Max(character.GetCharacterVital(CharacterVital.CurrentHealth), 0);
    float maxHealth = character.GetCurrentMaxHealth();
    float trueMaxHealth = character.GetTrueMaxHealth();
    // if (!character.blocking && !character.healthBroken)
    // {
    //   healthBarContainer.alpha -= Time.deltaTime / healthBarFadeTime;
    //   healthBarContainer.alpha = Mathf.Max(0, healthBarContainer.alpha);
    //   return;
    // }
    // if (currentHealth / maxHealth < .3)
    // {
    //   healthBarContentsSprite.color = lowHealthColor;
    // }
    healthBarContainer.alpha += Time.deltaTime / healthBarFadeTime;
    healthBarContainer.alpha = Mathf.Min(1, healthBarContainer.alpha);
    healthBarMaxFill.localScale = new Vector3((trueMaxHealth - maxHealth) / trueMaxHealth, healthBarMaxFill.localScale.y, 0);
    healthBarContentsFill.localScale = new Vector3(currentHealth / trueMaxHealth, healthBarContentsFill.localScale.y, 0);
  }

}
