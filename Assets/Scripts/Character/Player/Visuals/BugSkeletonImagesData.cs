using UnityEngine;
using UnityEditor;


public enum BugSkeletonPart
{
  HindlegRight,
  HindlegLeft,
  MidlegRight,
  MidlegLeft,
  ForelegRight,
  ForelegLeft,
  Abdomen,
  MandibleRight,
  MandibleLeft,
  Head,
  Eyes,
  AntennaRight,
  AntennaLeft,
  Thorax,
  HindwingRight,
  HindwingLeft,
  ForewingRight,
  ForewingLeft

}

public class BugSkeletonImagesData : ScriptableObject
{
  public static BugSkeletonPart[] bugSkeletonPartOrder = new BugSkeletonPart[]{
  BugSkeletonPart.HindlegRight,
  BugSkeletonPart.HindlegLeft,
  BugSkeletonPart.MidlegRight,
  BugSkeletonPart.MidlegLeft,
  BugSkeletonPart.ForelegRight,
  BugSkeletonPart.ForelegLeft,
  BugSkeletonPart.Abdomen,
  BugSkeletonPart.MandibleRight,
  BugSkeletonPart.MandibleLeft,
  BugSkeletonPart.Head,
  BugSkeletonPart.Eyes,
  BugSkeletonPart.AntennaRight,
  BugSkeletonPart.AntennaLeft,
  BugSkeletonPart.Thorax,
  BugSkeletonPart.HindwingRight,
  BugSkeletonPart.HindwingLeft,
  BugSkeletonPart.ForewingRight,
  BugSkeletonPart.ForewingLeft
};
  public BugSkeletonPartToSpriteDictionary bugSkeletonPartImages;

  public void LoadAndAssignSprites(string path)
  { // assumes a path to a folder full of sprites, all of whicah

    Debug.Log("hi??? ");
    string modifiedPath = path.Substring((Application.dataPath + "/resources/").Length);
    Debug.Log("modifiedPath: " + modifiedPath);
    Sprite[] sprites = Resources.LoadAll<Sprite>(modifiedPath);
    // Sprite[] sprites = Resources.LoadAll<Sprite>("Art/Characters/Sprites/Wasp");// as Sprite[];
    Debug.Log("sprites: " + sprites.Length);
    BugSkeletonPartToSpriteDictionary images = new BugSkeletonPartToSpriteDictionary();
    foreach (Sprite sprite in sprites)
    {
      string spriteNumberString = System.Text.RegularExpressions.Regex.Match(sprite.name, @"\d+").Value;
      int spriteNumber = System.Int32.Parse(spriteNumberString) - 1;
      Debug.Log(spriteNumber);
      bugSkeletonPartImages.Add(bugSkeletonPartOrder[spriteNumber], sprite);
    }
  }
#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create a BugSkeletonImagesData Asset
  [MenuItem("Assets/Create/BugSkeletonImagesData")]
  public static void CreateBugSkeletonImagesData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save BugSkeletonImagesData", "New BugSkeletonImagesData", "Asset", "Save BugSkeletonImagesData", "Assets/resources/Data/ArtData/CharacterArt");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BugSkeletonImagesData>(), path);
  }
#endif
}