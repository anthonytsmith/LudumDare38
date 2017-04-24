using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolutionCounterGUI : MonoBehaviour
{
	[System.NonSerialized]
	public int RequiredRevolutions = 0;

	private Rect Location;
	private GUIStyle Style;

	// Use this for initialization
	void Start( )
	{
		Location = new Rect( 0, 0, 200, 50 );
	}
	
	// Update is called once per frame
	void Update( )
	{
		
	}

	void OnGUI( )
	{
		Style = GUI.skin.label;
		Style.fontSize = 20;
		GUI.Label( Location, "Revolutions: " + Populate.RevolutionCount + "/" + RequiredRevolutions, Style );
	}
}
