using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
  EyeLeft,
  EyeRight = 18,
  AntennaRight,
  AntennaLeft,
  Thorax,
  HindwingRight,
  HindwingLeft,
  ForewingRight,
  ForewingLeft,
}

public enum BreakableBugSkeletonPart
{
  HindlegRight = BugSkeletonPart.HindlegRight,
  HindlegLeft = BugSkeletonPart.HindlegLeft,
  MidlegRight = BugSkeletonPart.MidlegRight,
  MidlegLeft = BugSkeletonPart.MidlegLeft,
  ForelegRight = BugSkeletonPart.ForelegRight,
  ForelegLeft = BugSkeletonPart.ForelegLeft,
  AntennaRight = BugSkeletonPart.AntennaRight,
  AntennaLeft = BugSkeletonPart.AntennaLeft,
  HindwingRight = BugSkeletonPart.HindwingRight,
  HindwingLeft = BugSkeletonPart.HindwingLeft,
  ForewingRight = BugSkeletonPart.ForewingRight,
  ForewingLeft = BugSkeletonPart.ForewingLeft

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
  BugSkeletonPart.EyeLeft,
  BugSkeletonPart.EyeRight,
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
    string modifiedPath = path.Substring((Application.dataPath + "/resources/").Length);
    Sprite[] sprites = Resources.LoadAll<Sprite>(modifiedPath);
    BugSkeletonPartToSpriteDictionary images = new BugSkeletonPartToSpriteDictionary();
    foreach (Sprite sprite in sprites)
    {
      string spriteNumberString = System.Text.RegularExpressions.Regex.Match(sprite.name, @"\d+").Value;
      int spriteNumber = System.Int32.Parse(spriteNumberString) - 1;
      Debug.Log(spriteNumber);
      images.Add(bugSkeletonPartOrder[spriteNumber], sprite);
    }
    bugSkeletonPartImages = images;
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