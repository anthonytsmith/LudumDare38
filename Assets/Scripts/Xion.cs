using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Xion : MonoBehaviour
{
	/// <summary>
	/// How much time in between Xion's attacks.
	/// </summary>
	public float AttackFrequency = 3f;

	private float TimeSinceLastAttack = 0f;
	private Animator Anim;

	// Use this for initialization
	void Start( )
	{
		Anim = GetComponent<Animator>( );
	}
	
	// Update is called once per frame
	void Update( )
	{
		if( Anim.GetCurrentAnimatorStateInfo( 0 ).IsName( "Xion_Stand" ) && !Anim.GetBool( "IsAttacking" ) )
		{
			TimeSinceLastAttack += Time.deltaTime;
		}
		if( Anim.GetCurrentAnimatorStateInfo( 0 ).IsName( "Xion_Attack" ) )
		{
			Anim.SetBool( "IsAttacking", false );
		}
		if( TimeSinceLastAttack > AttackFrequency )
		{
			TimeSinceLastAttack = 0;
			Anim.SetBool( "IsAttacking", true );
		}
	}
}
