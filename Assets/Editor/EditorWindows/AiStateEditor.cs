using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// [CustomEditor(typeof(AiState))]
public class AiStateEditor : Editor
{

  SerializedProperty actions;
  SerializedProperty transitions;

  // The Reorderable List we will be working with 
  ReorderableList actionsList;
  ReorderableList transitionsList;

  private void OnEnable()
  {
    actions = serializedObject.FindProperty("actions");

    // Set up the reorderable list      
    Debug.Log("actions: " + actions);
    actionsList = new ReorderableList(serializedObject, actions, true, true, true, true);

    actionsList.drawElementCallback = DrawActionsListItems; // Delegate to draw the elements on the list
    actionsList.drawHeaderCallback = DrawActionsHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.

    transitions = serializedObject.FindProperty("transitions");

    // Set up the reorderable list      
    Debug.Log("transitions: " + transitions);
    transitionsList = new ReorderableList(serializedObject, transitions, true, true, true, true);

    transitionsList.drawElementCallback = DrawTransitionsListItems; // delegate to draw the elements on the list
    transitionsList.drawHeaderCallback = DrawTransitionsHeader;
  }

  // Draws the elements on the list
  void DrawActionsListItems(Rect rect, int index, bool isActive, bool isFocused)
  {
    SerializedProperty element = actionsList.serializedProperty.GetArrayElementAtIndex(index); // The element in the list
    // EditorGUI.PropertyField(
    //     new Rect(rect.x, rect.y, 500, EditorGUIUtility.singleLineHeight),
    //     element,
    //     GUIContent.none
    // );
    EditorGUILayout.PropertyField(element);
    // //Create a property field and label field for each property. 
    // EditorGUI.PropertyField(
    //     new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
    //     element.FindPropertyRelative("name"),
    //     GUIContent.none
    // );
    // //The 'mobs' property. Since the enum is self-evident, I am not making a label field for it. 
    // //The property field for mobs (width 100, height of a single line)
    // EditorGUI.PropertyField(
    //     new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), 
    //     element.FindPropertyRelative("mobs"),
    //     GUIContent.none
    // ); 


    // //The 'level' property
    // //The label field for level (width 100, height of a single line)
    // EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Level");

    // //The property field for level. Since we do not need so much space in an int, width is set to 20, height of a single line.
    // EditorGUI.PropertyField(
    //     new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight),
    //     element.FindPropertyRelative("level"),
    //     GUIContent.none
    // ); 

    // //The 'quantity' property
    // //The label field for quantity (width 100, height of a single line)
    // EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Quantity");

    // //The property field for quantity (width 20, height of a single line)
    // EditorGUI.PropertyField(
    //     new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight),
    //     element.FindPropertyRelative("quantity"),
    //     GUIContent.none
    // );        

  }


  //Draws the header
  void DrawActionsHeader(Rect rect)
  {
    string name = "Actions";
    EditorGUI.LabelField(rect, name);
  }

  void DrawTransitionsListItems(Rect rect, int index, bool isActive, bool isFocused)
  {
    SerializedProperty element = transitionsList.serializedProperty.GetArrayElementAtIndex(index); // The element in the list
    EditorGUI.PropertyField(
        new Rect(rect.x, rect.y, 500, EditorGUIUtility.singleLineHeight),
        element,
        GUIContent.none
    );

  }
  //Draws the header
  void DrawTransitionsHeader(Rect rect)
  {
    string name = "Transitions";
    EditorGUI.LabelField(rect, name);
  }

  //This is the function that makes the custom editor work
  public override void OnInspectorGUI()
  {

    serializedObject.Update(); // Update the array property's representation in the inspector

    actionsList.DoLayoutList(); // Have the ReorderableList do its work
    transitionsList.DoLayoutList(); // Have the ReorderableList do its work

    // We need to call this so that changes on the Inspector are saved by Unity.
    serializedObject.ApplyModifiedProperties();

    // Update the array property's representation in the inspector
    // Have the ReorderableList do its work
    // Apply any property modification
  }


}
