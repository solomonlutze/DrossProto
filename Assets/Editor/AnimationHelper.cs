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
      Debug.Log("data is defined: " + bugAnimationDatas);
      AnimatorController animationController = Resources.Load("Prefabs/Characters/Animation/BugAnimationController") as AnimatorController;
      AnimatorControllerLayer baseLayer = System.Array.Find(animationController.layers, l => l.name == "Base");
      foreach (ChildAnimatorState state in baseLayer.stateMachine.states)
      {
        AnimationName stateName = (AnimationName)System.Enum.Parse(typeof(AnimationName), state.state.name);
        foreach (AnimatorControllerLayer layer in animationController.layers)
        {
          if (layer.name == "Base") { continue; }
          // foreach (ChildAnimatorState state in layer.stateMachine.states)
          // {
          // Debug.Log("Evaluating state name: " + state.state.name + " for layer " + layer.name);
          // AnimationState animationState = state.state.name
          BlendTree tree = new BlendTree();
          foreach (BugSpecies bugSpecies in bugAnimationDatas.bugSpeciesToAnimationData.Keys)
          // for (int i = 0; i < bugAnimationDatas.bugSpeciesToAnimationData.Keys.Count; i++)
          {
            tree.AddChild(bugAnimationDatas.bugSpeciesToAnimationData[bugSpecies].animationStateNamesToClips[stateName], (int)bugSpecies);
          }
          state.state.motion = tree;
        }
      }
    }

  }
}