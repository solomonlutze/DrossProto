using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTarget : MonoBehaviour
{
  public Sprite nullSprite;
  public EnvironmentTile tileToReplaceWith;

  void Start() {
    GetComponent<SpriteRenderer>().sprite = null;
  }

  private void OnValidate() {
    Debug.Log("tileToReplaceWith is now " +tileToReplaceWith);
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    sr.sprite = tileToReplaceWith ? tileToReplaceWith.sprite : nullSprite;
  }
}
