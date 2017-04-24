using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWorld : MonoBehaviour
{
	public static float WorldEnd = 0;
	protected Vector2 Position;
	// Use this for initialization
	protected virtual void Start( )
	{
		Position = transform.position;
	}
	
	// Update is called once per frame
	protected virtual void Update( )
	{
		Position.x -= Populate.WorldMoveSpeed * Time.deltaTime;
		transform.position = Position;
		if( transform.position.x < -10 )
		{
			Position.x = WorldEnd + transform.position.x;
			transform.position = Position;
		}
	}
}
