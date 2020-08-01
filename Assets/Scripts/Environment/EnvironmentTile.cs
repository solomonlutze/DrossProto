using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Interaction tile heights. If characters are in the air they ignore interactions iwth anything on a lower height than them.
public enum FloorLayer { B6, B5, B4, B3, B2, B1, F1, F2, F3, F4, F5, F6 }
public enum TileDurability { Delicate, Soft, Medium, Hard, Indestructable }
public enum TileTag { Ground, Water }
public enum FloorTilemapType { Ground, Object }

[System.Serializable]
public class EnvironmentTileParticleSystemInfo
{
  public ParticleSystem system;

  public Color32 overrideColor = Color.white;
  public int burstCount;
  public float burstCooldown;
}

[System.Serializable]
public class EnvironmentTile : Tile
{

  // Annoying workaround for non-nullable environmentalDamage. Mark True to have tile deal damage.
  public bool dealsDamage = false;
  public List<TileTag> tileTags;
  public DamageData_OLD environmentalDamage_OLD;
  public EnvironmentalDamageInfo environmentalDamageInfo;
  public bool corrodable;
  public List<CharacterMovementAbility> movementAbilitiesWhichBypassDamage;

  [HideInInspector]
  public bool changesFloorLayer;
  // Which floor layer the tile sends the character to
  [HideInInspector]
  [SerializeField]
  public int changesFloorLayerByAmount;
  public TileDurability tileDurability = TileDurability.Indestructable;
  public FloorTilemapType floorTilemapType = FloorTilemapType.Ground;

  public EnvironmentTile autoPlaceTileOnPaint_Above;
  public EnvironmentTile replacedByWhenDestroyed;
  public EnvironmentTile replacedByWhenCorroded;
  //TODO: should we just have statMods?
  public float accelerationMod;
  public Sprite[] maskingSprites;

  // Eventually this'll be a more complex thing, probably
  public bool shouldRespawnPlayer = false;
  // map of attributes which can bypass : the value required to bypass
  // characters can cross tile with ANY of these attributes in the right amount; they do not need all of them
  public CharacterAttributeToIntDictionary attributesWhichBypassRespawn;
  // map of attributes which can bypass : the value required to bypass
  // characters can climb with ANY of these attributes in the right amount; they do not need all of them
  public CharacterAttributeToIntDictionary attributesWhichAllowClimbing;
  public CharacterAttributeToIntDictionary attributesWhichAllowBurrowing;
  public CharacterAttributeToIntDictionary attributesWhichAllowPassingThrough;
  public EnvironmentTileParticleSystemInfo footstepParticleSystemInfo;
  // public ParticleSystem footstepParticleSystem; // used for particles created when something walks on this
  public ParticleSystem.EmitParams footstepSystemParams;
  private string tileType;
  private Renderer _renderer;
  public ShaderData shaderData;
  public GameObject[] interestObjects;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an EnvironmentTile Asset
  [MenuItem("Assets/Create/EnvironmentTile")]
  public static void CreateEnvironmentTile()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Environment Tile", "New Environment Tile", "Asset", "Save Environment Tile", "resources/Art/Tiles");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EnvironmentTile>(), path);
  }
#endif

  public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
  {
    tileType = this.name;
    if (go != null)
    {
      SetPropertyBlock(location, tilemap, go);
      go.transform.eulerAngles = new Vector3(90, 90, -90);
      go.transform.position += new Vector3(.5f, .5f, 0);
    }
    return base.StartUp(location, tilemap, go);
  }

  private void SetPropertyBlock(Vector3Int location, ITilemap tilemap, GameObject go)
  {
    if (shaderData == null) { return; }
    _renderer = go.GetComponent<Renderer>();
    AssignMask(location, tilemap, go);
    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
    _renderer.GetPropertyBlock(propBlock);
    Dictionary<string, TextureData> textureDatas = new Dictionary<string, TextureData>() {
            {"_Red0", shaderData.red0TextureData },
            {"_Red50", shaderData.red50TextureData },
            {"_Red100", shaderData.red100TextureData },
            {"_Red150", shaderData.red150TextureData },
            {"_Green0", shaderData.green0TextureData },
            {"_Green50", shaderData.green50TextureData },
            {"_Green100", shaderData.green100TextureData },
            {"_Green150", shaderData.green150TextureData },
            {"_Blue0", shaderData.blue0TextureData },
            {"_Blue50", shaderData.blue50TextureData },
            {"_Blue100", shaderData.blue100TextureData },
            {"_Blue150", shaderData.blue150TextureData },
        };
    foreach (var textureData in textureDatas)
    {
      if (textureData.Value != null && textureData.Value.texture != null)
      {
        propBlock.SetTexture(textureData.Key + "Texture", textureData.Value.texture);
        propBlock.SetVector(textureData.Key + "Scale", textureData.Value.scale);
        propBlock.SetFloat(textureData.Key + "IntensityMin", textureData.Value.intensityMin);
        propBlock.SetFloat(textureData.Key + "IntensityMax", textureData.Value.intensityMax);
        propBlock.SetFloat(textureData.Key + "Opacity", textureData.Value.opacity);
        propBlock.SetVector(textureData.Key + "ScrollSpeed", textureData.Value.scrollSpeed);
      }
    }
    _renderer.SetPropertyBlock(propBlock);

  }
  public override void RefreshTile(Vector3Int location, ITilemap tilemap)
  {
    // Debug.Log("tile " + this.name + " on tilemap " + tilemap);
    for (int yd = -1; yd <= 1; yd++)
      for (int xd = -1; xd <= 1; xd++)
      {
        Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
        if (IsSameTileTypeAs(tilemap, position))
          tilemap.RefreshTile(position);
      }
  }

  public void AssignMask(Vector3Int location, ITilemap tilemap, GameObject go)
  {
    int mask = IsSameTileTypeAs(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;
    int index = GetIndex((byte)mask);
    Texture maskingSpriteTexture = maskingSprites.Length > 0 ? maskingSprites[0].texture : null;
    // TODO? did there used to be an if here??
    if (index >= 0 && index < maskingSprites.Length)
    {
      maskingSpriteTexture = maskingSprites[index].texture;
    }
    else
    {
      Debug.LogWarning("Not enough sprites in " + this.name + " instance");
    }
    if (_renderer != null)
    {
      MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
      _renderer.GetPropertyBlock(propBlock);
      propBlock.SetTexture("_MaskingTexture", maskingSpriteTexture);
      var newRotationAngle = GetRotationAngle((byte)mask);
      propBlock.SetFloat("_MaskRotation", Mathf.Deg2Rad * newRotationAngle);
      _renderer.SetPropertyBlock(propBlock);
    }
    else
    {

      Debug.LogWarning("renderer is null on " + this.name);
    }

  }

  public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
  {
    int mask = IsSameTileTypeAs(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;
    mask += IsSameTileTypeAs(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;
    tileData.sprite = this.sprite;
    tileData.colliderType = this.colliderType;
    tileData.gameObject = this.gameObject;
  }

  // The following determines which sprite to use based on the number of adjacent similar tiles
  private int GetIndex(byte mask)
  {
    switch (mask)
    {
      case 0: return 0; // no adjacent tiles
      case 1:
      case 2:
      case 4:
      case 8: return 1; // one adjacent tile
      case 3:
      case 6:
      case 9:
      case 12: return 2; // two tiles, corner
      case 5:
      case 10: return 3; // two tiles, opposing
      case 7:
      case 11:
      case 13:
      case 14: return 4; // three tiles
      case 15: return 5; // all adjacent tiles
    }
    return -1;
  }

  // The following determines which rotation to use based on the positions of adjacent RoadTiles
  private float GetRotationAngle(byte mask)
  {
    switch (mask)
    {
      case 8:
      case 9:
      case 10:
      case 11:
        return -90f; // rotate clockwise once
      case 1:
      case 3:
      case 7:
        return -180f; // rotate clockwise twice
      case 2:
      case 6:
      case 14:
        return -270f; // rotate clockwise three times
    }
    return 0f;
  }
  public bool ShouldRespawnPlayer()
  {
    return shouldRespawnPlayer;
  }

  // TODO: don't do this
  public bool IsSameTileTypeAs(ITilemap tilemap, Vector3Int position)
  {
    return tilemap.GetTile(position) == this;
  }

  public bool IsInteractable()
  {
    return (changesFloorLayer);
  }

  public float EmitFootstepParticles(Character c)
  {
    if (footstepParticleSystemInfo.system != null)
    {
      ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
      emitParams.applyShapeToPosition = true;
      emitParams.startColor = footstepParticleSystemInfo.overrideColor;
      emitParams.position = c.transform.position;
      emitParams.rotation = Random.Range(0, 360);
      GameMaster.Instance.particleSystemMaster.EmitFootstep(c, this, emitParams, footstepParticleSystemInfo.burstCount);
      return footstepParticleSystemInfo.burstCooldown;
    }
    return 0;
  }
}