using UnityEngine;
using UnityEngine.UI;


public class ElementalBuildupBar : MonoBehaviour
{

  public CanvasGroup buildupBarContainer;
  public RectTransform buildupBarController;
  public RectTransform buildupBarContentsFill;
  public DamageType damageType;
  public Image buildupBarContentsSprite
  {
    get
    {
      return buildupBarContentsFill == null ? null : buildupBarContentsFill.GetComponent<Image>();
    }
  }

  public RectTransform buildupBarMaxFill;
  public Image buildupBarMaxSprite
  {
    get
    {
      return buildupBarMaxFill == null ? null : buildupBarMaxFill.GetComponent<Image>();
    }
  }

  public float buildupBarFadeTime = .5f;
  public Color defaultBuildupColor;

  public Character character;
  void Start()
  {
    if (character == null)
    {
      character = GetComponentInParent<Character>();
    }
    buildupBarContainer.alpha = 0;
  }

  void Update()
  {
    if (character != null)
    {
      HandleBuildupBar();
    }
  }

  void HandleBuildupBar()
  {
    ElementalDamageBuildup buildup = character.GetElementalDamageBuildup(damageType);
    float currentBuildup = 0;
    if (buildup != null)
    {
      currentBuildup = Mathf.Max(buildup.remainingMagnitude, 0);
    }
    if (currentBuildup == 0)
    {
      buildupBarContainer.alpha -= Time.deltaTime / buildupBarFadeTime;
      buildupBarContainer.alpha = Mathf.Max(0, buildupBarContainer.alpha);
      return;
    }
    float trueMaxHealth = character.GetTrueMaxHealth();
    buildupBarContainer.alpha += Time.deltaTime / buildupBarFadeTime;
    buildupBarContainer.alpha = Mathf.Min(1, buildupBarContainer.alpha);
    buildupBarMaxFill.localScale = new Vector3(0, buildupBarMaxFill.localScale.y, 0);
    buildupBarContentsFill.localScale = new Vector3(currentBuildup / trueMaxHealth, buildupBarContentsFill.localScale.y, 0);
  }

}
