using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class ControllerGlyph : MonoBehaviour
{
  public string actionName;
  public Image glyphImage;
  Sprite _glyphSprite;
  void Update()
  {
    Sprite glyphSprite = GameMaster.Instance.inputGlyphHelper.GetInputGlyphForAction(actionName);
    Debug.Log("sprite: " + glyphSprite);
    if (glyphSprite != _glyphSprite)
    {
      _glyphSprite = glyphSprite;
      Debug.Log("Setting glyph sprite to " + _glyphSprite);
      glyphImage.sprite = _glyphSprite;
    }
  }
}