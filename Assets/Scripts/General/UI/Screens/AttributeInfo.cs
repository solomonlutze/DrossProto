using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Should have a reference to AttributeValuePip prefab
// Should have references to its AttributeName, Description, and PipsContent children
// Should be passed an Attribute, its Value, and the corresponding AttributeData object
// Populate AttributeName and Description according to ATtributeData
// Init 1 pip for each point of Attribute
[ExecuteAlways]
public class AttributeInfo : MonoBehaviour
{

  public CharacterAttribute attribute;
  public GameObject attributeValuePipPrefab;
  public SuperTextMesh attributeNameText;
  public SuperTextMesh attributeDescriptionText;
  public SuperTextMesh pupaAttributeDescriptionText;
  public Transform pipsContainer;
  public int defaultFontSize;
  public Color32 defaultFontColor;

  private IAttributeDataInterface attributeData;
  private int actualPupaAttributeValue;

  public Image arrowImage;
  void Start()
  {
    defaultFontSize = pupaAttributeDescriptionText.quality;
    defaultFontColor = pupaAttributeDescriptionText.color;
    if (gameObject.name != attribute.ToString() + "_AttributeInfo")
    {
      gameObject.name = attribute.ToString() + "_AttributeInfo";
    }
    UnhighlightDelta();
  }

  void Update()
  {
    if (!Application.IsPlaying(gameObject))
    {
      if (gameObject.name != attribute.ToString() + "_AttributeInfo")
      {
        gameObject.name = attribute.ToString() + "_AttributeInfo";
      }
    }
  }

  public void Init(IAttributeDataInterface data, int value, int nextValue)
  {
    attribute = data.attribute;
    attributeData = data;
    attributeNameText.text = attributeData.displayNameWithGroup;
    attributeDescriptionText.gameObject.SetActive(false);
    pupaAttributeDescriptionText.text = attributeData.attributeTiers[nextValue].attributeTierDescription;
    actualPupaAttributeValue = nextValue; // save to modify when looking at items
    arrowImage.gameObject.SetActive(false);
    DisableAllPips();
  }

  public void HighlightDelta(int proposedChange)
  {
    int desiredFontSize = defaultFontSize;
    Color32 desiredFontColor = defaultFontColor;
    if (proposedChange > 0)
    {
      desiredFontColor = Color.red;
      desiredFontSize = defaultFontSize + 2;
      arrowImage.gameObject.SetActive(true);
      arrowImage.color = Color.red;
      arrowImage.transform.localScale = new Vector3(1, 1, 1);
    }
    else if (proposedChange < 0)
    {
      desiredFontColor = Color.blue;
      desiredFontSize = defaultFontSize - 1;
      arrowImage.gameObject.SetActive(true);
      arrowImage.color = Color.blue;
      arrowImage.SetAllDirty();
      arrowImage.transform.localScale = new Vector3(1, -1, 1);
    }
    else
    {
      arrowImage.gameObject.SetActive(false);
    }
    pupaAttributeDescriptionText.color = desiredFontColor;
    pupaAttributeDescriptionText.quality = desiredFontSize;
    pupaAttributeDescriptionText.text = attributeData.attributeTiers[actualPupaAttributeValue + proposedChange].attributeTierDescription;
  }

  public void UnhighlightDelta()
  {
    pupaAttributeDescriptionText.color = defaultFontColor;
    pupaAttributeDescriptionText.quality = defaultFontSize;
    pupaAttributeDescriptionText.text = attributeData.attributeTiers[actualPupaAttributeValue].attributeTierDescription;
    arrowImage.gameObject.SetActive(false);
  }
  public void InitPips(int numberOfPips)
  {
    while (pipsContainer.childCount < numberOfPips)
    {
      Instantiate(attributeValuePipPrefab).transform.parent = pipsContainer;
    }
    for (int i = 0; i < pipsContainer.childCount; i++)
    {
      if (i < numberOfPips)
      {
        pipsContainer.GetChild(i).gameObject.SetActive(true);
      }
      else
      {
        pipsContainer.GetChild(i).gameObject.SetActive(false);
      }
    }
  }

  void DisableAllPips()
  {
    for (int i = 0; i < pipsContainer.childCount; i++)
    {
      pipsContainer.GetChild(i).gameObject.SetActive(false);
    }
  }
}
