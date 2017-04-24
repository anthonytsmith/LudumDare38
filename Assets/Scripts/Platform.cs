using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MoveWorld
{
	public bool HasMonster = false;
	public float Size = 0;
	public Vector2 Bounds = new Vector2( );
	// Use this for initialization
	protected override void Start( )
	{
		base.Start( );
	}
	
	// Update is called once per frame
	protected override void Update( )
	{
		if( !HasMonster && transform.position.x < -9 )
		{
			Debug.Log( "Could spawn" );
			int Monster = Populate.SpawnMonster( );
			if( -1 != Monster )
			{
				Debug.Log( "Should spawn" );
				GameObject SpawnedMonster = Instantiate( Populate.EnemyObjectList[ Monster ], new Vector2( transform.position.x + Size / 2, transform.position.y + Bounds.y ), Quaternion.identity );
				HasMonster = true;
				SpawnedMonster.transform.parent = transform;
			}
		}
		base.Update( );
	}
}
