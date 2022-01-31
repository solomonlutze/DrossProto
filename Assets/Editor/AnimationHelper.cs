using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;

namespace Infrastructure.Editor
{
  public class AnimationHelper
  {
    [MenuItem("CustomTools/RebuildAnimationData")]
    private static void RebuildAnimationData()
    {
      BugSpeciesToAnimationDataMap bugAnimationDatas = AssetDatabase.LoadAssetAtPath<BugSpeciesToAnimationDataMap>("Assets/Art/Animation/BugAnimationData/BugSpeciesToAnimationData.asset");
      Debug.Log(bugAnimationDatas);
      BugAnimationData defaultAnimations = bugAnimationDatas.bugSpeciesToAnimationData[BugSpecies.Default];
      //AnimatorController animationController = Resources.Load("Prefabs/Characters/Animation/BugAnimationController") as AnimatorController;
      AnimatorController animationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Art/Animation/BugAnimationController.controller");
      AnimatorControllerLayer baseLayer = System.Array.Find(animationController.layers, l => l.name == "Base");
      AnimatorControllerLayer[] layers = animationController.layers;
      foreach (ChildAnimatorState state in baseLayer.stateMachine.states)
      {
        AnimationName stateName = (AnimationName)System.Enum.Parse(typeof(AnimationName), state.state.name);
        for (int i = 0; i < layers.Length; i++)
        {
          if (layers[i].name == "Base") { continue; }
          BlendTree tree = layers[i].GetOverrideMotion(state.state) as BlendTree;
          if (tree != null)
          {
            AssetDatabase.RemoveObjectFromAsset(tree);
          }
          tree = new BlendTree();
          tree.blendParameter = layers[i].name + "AnimationType";
          tree.useAutomaticThresholds = false;
          foreach (BugSpecies bugSpecies in bugAnimationDatas.bugSpeciesToAnimationData.Keys)
          {
            if (bugSpecies == BugSpecies.Default) { continue; }
            Debug.Log("species: " + bugSpecies + ", state: " + stateName);
            Motion motion = defaultAnimations.animationStateNamesToClips[stateName];
            if (bugAnimationDatas.bugSpeciesToAnimationData[bugSpecies].animationStateNamesToClips[stateName] != null)
            {
              motion = bugAnimationDatas.bugSpeciesToAnimationData[bugSpecies].animationStateNamesToClips[stateName];

            }
            Debug.Log("motion for species: " + motion);
            tree.AddChild(motion, (int)bugSpecies);
          }
          layers[i].SetOverrideMotion(state.state, tree);
          AssetDatabase.AddObjectToAsset(tree, "Assets/Art/Animation/BugAnimationController.controller");
          // animationController.SetStateEffectiveMotion(state.state, tree, i);
        }
      }
      animationController.layers = layers;
      EditorUtility.SetDirty(animationController);
      AssetDatabase.SaveAssets();
    }
  }
}