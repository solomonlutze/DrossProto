using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class EnvironmentTileWindow : EditorWindow {
	Texture2D headerSectionTexture;
	Texture2D section1Texture;
	Texture2D section2Texture;
	Rect headerSection;
	Rect section1;
	Rect section2;

	[MenuItem("Window/Environment Tile Editor")]
	static void OpenWindow() {
		EnvironmentTileWindow window = (EnvironmentTileWindow)GetWindow(typeof(EnvironmentTileWindow));
		window.minSize = new Vector2(600,300);
		window.Show();
	}

	void OnEnable()
	{
		InitTextures();
	}

	void OnGUI()
	{
		DrawLayouts();
		DrawHeader();
		DrawSection1();
	}

	void InitTextures() {
		headerSectionTexture = new Texture2D(1,1);
		headerSectionTexture.SetPixel(0,0, Color.gray);
		headerSectionTexture.Apply();
		section1Texture = new Texture2D(1,1);
		section1Texture.SetPixel(0,0, Color.cyan);
		section1Texture.Apply();
		section2Texture = new Texture2D(1,1);
		section2Texture.SetPixel(0,0, Color.magenta);
		section2Texture.Apply();
	}

	void DrawLayouts() {
		headerSection.x = 0;
		headerSection.y = 0;
		headerSection.width = Screen.width;
		headerSection.height = 40;

		section1.x = 0;
		section1.y = 40;
		section1.width = Screen.width / 2f;
		section1.height = Screen.height - 40;

		section2.x = Screen.width / 2f;
		section2.y = 40;
		section2.width = Screen.width / 2f;
		section2.height = Screen.height - 40;

		GUI.DrawTexture(headerSection, headerSectionTexture);
		GUI.DrawTexture(section1, section1Texture);
		GUI.DrawTexture(section2, section2Texture);
	}
	void DrawHeader() {
		GUILayout.BeginArea(headerSection);
		GUILayout.Label("Create new Environment Tile");
		GUILayout.EndArea();
	}


	void DrawSection1() {
		GUILayout.BeginArea(section1);
		GUILayout.Label("Here's some Environment Tile text");
		GUILayout.EndArea();
	}
}
