using UnityEngine;
using Rewired;

//lifted a lot of this from the rewired docs
//https://guavaman.com/projects/rewired/docs/HowTos.html#display-glyph-for-action
public class InputGlyphHelper : MonoBehaviour
{
  public int rewiredPlayerId = 0;
  private Rewired.Player rewiredPlayer;

  void Awake()
  {
    rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
  }

  public Sprite GetInputGlyphForAction(string actionName)
  {
    Controller activeController = rewiredPlayer.controllers.GetLastActiveController();
    if (activeController == null)
    { // player hasn't used any controllers yet
      // No active controller, set a default
      if (rewiredPlayer.controllers.joystickCount > 0)
      { // try to get the first joystick in player
        activeController = rewiredPlayer.controllers.Joysticks[0];
      }
      else
      { // no joysticks assigned, just get keyboard
        activeController = rewiredPlayer.controllers.Keyboard;
      }
    }
    if (!string.IsNullOrEmpty(actionName))
    {
      InputAction action = ReInput.mapping.GetAction(actionName); // get the Action for the current string
      if (action != null)
      { // make sure this is a valid action
        return GetGlyph(rewiredPlayer, activeController, action);
      }
      else
      {
        Debug.Log("action is null");
      }
    }
    return null;
  }

  Sprite GetGlyph(Player p, Controller controller, InputAction action)
  {
    if (p == null || controller == null || action == null) return null;

    // Find the first element mapped to this Action on this controller
    ActionElementMap aem = p.controllers.maps.GetFirstElementMapWithAction(controller, action.id, true);
    if (aem == null) return null; // nothing was mapped on this controller for this Action
    Sprite glyph = null;
    if (controller.type == ControllerType.Joystick)
    {
      // Find the glyph for the element on the controller
      glyph = ControllerGlyphs.GetGlyph((controller as Joystick).hardwareTypeGuid, aem.elementIdentifierId, aem.axisRange);
    }
    else if (controller.type == ControllerType.Keyboard)
    {
      glyph = ControllerGlyphs.GetKeyGlyph(aem.elementIdentifierId, aem.axisRange);
    }
    return glyph; // this example only supports joystick glyphs
    // // Draw the glyph to the screen
    // Rect rect = new Rect(0, 30, glyph.textureRect.width, glyph.textureRect.height);
    // GUI.Label(new Rect(rect.x, rect.y + rect.height + 20, rect.width, rect.height), action.descriptiveName);
    // GUI.DrawTexture(rect, glyph.texture);
  }
}