using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EPlayerState
{
	Jumping,
	Falling,
	Grounded,
	Attacking
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
	/// <summary>
	/// The amount of force that the player jumps off a platform or the ground with.
	/// </summary>
	public float JumpForce = 9.81f;
	/// <summary>
	/// Maximum number of jumps the player is allowed to make from the ground.
	/// </summary>
	public int MaxJumps = 2;
	[System.NonSerialized]
	public bool IsDead = false;
	[System.NonSerialized]
	public float DeadX = -5;

	private EPlayerState State = EPlayerState.Grounded;
	private int JumpCount = 0;
	private bool IsAxisDown = false;
	private float MoveSpeed = 0;
	private Rigidbody2D Rigid;
	private Animator Anim;
	private EPlayerState PreviousState = EPlayerState.Grounded;
	private bool Transitioning = false;

	// Use this for initialization
	void Start( )
	{
		Anim = GetComponent<Animator>( );
		Rigid = GetComponent<Rigidbody2D>( );
	}
	
	// Update is called once per frame
	void Update( )
	{
		if( Input.GetAxisRaw( "Jump" ) != 0 )
		{
			if( !IsAxisDown )
			{
				IsAxisDown = true;
			}
		}
		if( EPlayerState.Attacking == State && Anim.GetCurrentAnimatorStateInfo( 0 ).IsName( "PlayerAttack" ) && Transitioning )
		{
			Transitioning = false;
		}
		else if( EPlayerState.Attacking == State && !Anim.GetCurrentAnimatorStateInfo( 0 ).IsName( "PlayerAttack" ) && !Transitioning )
		{
			State = PreviousState;
		}
		if( Input.GetAxisRaw( "Jump" ) == 0 && IsAxisDown )
		{
			// Just released.
			if( MaxJumps > JumpCount )
			{
				if( EPlayerState.Attacking != State )
				{
					State = EPlayerState.Jumping;
				}
				Rigid.velocity = new Vector2( Rigid.velocity.x, JumpForce * Vector2.up.y );
				JumpCount++;
			}
			IsAxisDown = false;
		}
		if( Input.GetAxisRaw( "Fire1" ) != 0 && !Anim.GetCurrentAnimatorStateInfo( 0 ).IsName( "PlayerAttack" ) )
		{
			if( EPlayerState.Attacking != State )
			{
				PreviousState = State;
			}
			State = EPlayerState.Attacking;
			Transitioning = true;
			Anim.SetTrigger( "IsAttacking" );
		}
		if( transform.position.x < DeadX )
		{
			//The player is now dead.
			IsDead = true;
		}
		Vector2 Position = transform.position;
		Position.x += Time.deltaTime * MoveSpeed;
	}

	void OnCollisionEnter2D( Collision2D Collision )
	{
		if( Collision.gameObject.layer == LayerMask.NameToLayer( "Ground" ) )
		{
			if( EPlayerState.Attacking != State )
			{
				State = EPlayerState.Grounded;
			}
			JumpCount = 0;
		}
		if( Collision.gameObject.layer == LayerMask.NameToLayer( "Platform" ) )
		{
			if( EPlayerState.Attacking != State )
			{
				State = EPlayerState.Grounded;
			}
			MoveSpeed = Populate.WorldMoveSpeed;
			JumpCount = 0;
		}
	}

	void OnCollisionExit2D( Collision2D Collision )
	{
		if( Collision.gameObject.layer == LayerMask.NameToLayer( "Platform" ) )
		{
			MoveSpeed = 0;
			if( EPlayerState.Jumping != State )
			{
				if( EPlayerState.Attacking != State )
				{
					State = EPlayerState.Falling;
				}
				JumpCount = 1;
			}
		}
	}

	void OnTriggerEnter2D( Collider2D Collider )
	{
		if( Collider.gameObject.layer == LayerMask.NameToLayer( "Enemy" ) )
		{
			if( EPlayerState.Attacking != State )
			{
				IsDead = true;
			}
			else
			{
				GameObject Object = Collider.gameObject;
				if( null != Object.transform.parent )
				{
					Platform StandingOn = Object.transform.parent.gameObject.GetComponent<Platform>( );
					if( null != StandingOn )
					{
						StandingOn.HasMonster = false;
					}
				}
				Destroy( Collider.gameObject );
			}
		}
	}

	void OnTriggerExit2D( Collider2D Collider )
	{
		if( Collider.gameObject.layer == LayerMask.NameToLayer( "RevolutionCounter" ) )
		{
			Populate.RevolutionCount++;
			Populate.WorldMoveSpeed += Populate.BaseMoveSpeed * 0.5f * ( Populate.RevolutionCount % 2 );
		}
	}
}
