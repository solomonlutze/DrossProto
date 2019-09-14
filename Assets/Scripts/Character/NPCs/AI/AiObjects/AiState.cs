using UnityEngine;
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
      }
      else
      {
        controller.TransitionToState(transitions[i].falseState);
      }
    }
  }
}