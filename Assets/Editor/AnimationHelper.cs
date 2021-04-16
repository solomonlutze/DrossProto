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
      BugSpeciesToAnimationData bugAnimationDatas = Resources.Load("Prefabs/Characters/Animation/BugAnimationData/BugSpeciesToAnimationData") as BugSpeciesToAnimationData;
      AnimatorController animationController = Resources.Load("Prefabs/Characters/Animation/BugAnimationController") as AnimatorController;
      AnimatorControllerLayer baseLayer = System.Array.Find(animationController.layers, l => l.name == "Base");
      AnimatorControllerLayer[] layers = animationController.layers;
      foreach (ChildAnimatorState state in baseLayer.stateMachine.states)
      {
        AnimationName stateName = (AnimationName)System.Enum.Parse(typeof(AnimationName), state.state.name);
        for (int i = 0; i < layers.Length; i++)
        {
          if (layers[i].name == "Base") { continue; }
          BlendTree tree = new BlendTree();
          tree.blendParameter = layers[i].name + "AnimationType";
          tree.useAutomaticThresholds = false;
          foreach (BugSpecies bugSpecies in bugAnimationDatas.bugSpeciesToAnimationData.Keys)
          {
            tree.AddChild(bugAnimationDatas.bugSpeciesToAnimationData[bugSpecies].animationStateNamesToClips[stateName], (int)bugSpecies);
          }
          layers[i].SetOverrideMotion(state.state, tree);
        }
      }
      animationController.layers = layers;
    }

  }
}