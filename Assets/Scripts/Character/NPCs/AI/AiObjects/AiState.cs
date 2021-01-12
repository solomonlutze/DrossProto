using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "PluggableAi/State")]
public class AiState : ScriptableObject
{
  public AiAction[] actions;
  public AiTransition[] transitions;
  public void UpdateState(AiStateController controller)
  {
    DoActions(controller);
    CheckTransitions(controller);
  }

  void DoActions(AiStateController controller)
  {
    for (int i = 0; i < actions.Length; i++)
    {
      actions[i].Act(controller);
    }
  }

  private void CheckTransitions(AiStateController controller)
  {
    for (int i = 0; i < transitions.Length; i++)
    {
      if (transitions[i].decision.Decide(controller))
      {
        controller.TransitionToState(transitions[i].trueState);
        if (transitions[i].trueState != controller.remainState)
        {
          break;
        }
      }
      else
      {
        controller.TransitionToState(transitions[i].falseState);
        if (transitions[i].falseState != controller.remainState)
        {
          break;
        }
      }
    }
  }

  public bool OnEntry(AiStateController controller)
  {
    foreach (AiAction action in actions)
    {
      if (!action.OnEntry(controller))
      {
        return false;
      }
    }
    return true;
  }


  public void OnExit(AiStateController controller)
  {
    foreach (AiAction action in actions)
    {
      action.OnExit(controller);
    }
  }
}