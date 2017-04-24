using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(GUILayer))]
[RequireComponent(typeof(FlareLayer))]
public class Populate : MonoBehaviour
{
	/// <summary>
	/// How wide should the level be?
	/// </summary>
	public int LevelWidth = 4096;
	/// <summary>
	/// How many revolutions should be made?
	/// </summary>
	public int LevelRevolutions = 10;
	/// <summary>
	/// The default move speed of the world.
	/// </summary>
	public float MoveSpeed = 4;
	/// <summary>
	/// The base move speed of the world.
	/// </summary>
	[System.NonSerialized]
	public static float BaseMoveSpeed = 4;
	/// <summary>
	/// The movement speed of the world.
	/// </summary>
	[System.NonSerialized]
	public static float WorldMoveSpeed = 4;
	/// <summary>
	/// The object that allows tracking of the number of times the player has gone around the world.
	/// </summary>
	public GameObject RevolutionObject = null;
	/// <summary>
	/// The player.
	/// </summary>
	public GameObject Player = null;
	/// <summary>
	/// The force that the player jumps with.
	/// </summary>
	public float PlayerJumpForce = 9.81f;
	/// <summary>
	/// The height that the player is capable of jumping to, with a single jump.
	/// </summary>
	public float PlayerJumpHeight;
	/// <summary>
	/// The distance that the player is capable of jumping.
	/// </summary>
	public float PlayerJumpWidth;
	/// <summary>
	/// The maximum distance the player is capable of jumping.
	/// </summary>
	public float MaxPlayerJumpWidth;
	/// <summary>
	/// What the ground should be.
	/// </summary>
	public GameObject Ground = null;
	/// <summary>
	/// How many extra ground objects should be placed?
	/// </summary>
	[Range(0, 1000)]
	public int Padding = 10;
	/// <summary>
	/// What makes up the left side of a platform?
	/// </summary>
	public GameObject LeftPlat = null;
	/// <summary>
	/// What makes up the middle of a platform?
	/// </summary>
	public GameObject MidPlat = null;
	/// <summary>
	/// What makes up the right of a platform?
	/// </summary>
	public GameObject RightPlat = null;
	/// <summary>
	/// The amount that platform objects after the first should be adjusted.
	/// </summary>
	public float PlatformAdjustment = -1;
	/// <summary>
	/// The list of available enemies.
	/// </summary>
	public GameObject[ ] EnemyObjects = null;
	/// <summary>
	/// How frequently should an attempt to spawn a monster on the ground occur?
	/// </summary>
	public float TimeToSpawnEnemyOnGroundInSeconds = 5;
	/// <summary>
	/// Backgrounds
	/// </summary>
	public GameObject[ ] BackgroundObjects = null;

	[System.NonSerialized]
	public static int RevolutionCount = 0;
	[System.NonSerialized]
	public static GameObject[ ] EnemyObjectList;
	[System.NonSerialized]
	public static int RevolutionsRequired = 0;  


	private Camera MainCamera;
	private float MaxY;
	private float MinX;
	private float MinY;
	private float MaxX;

	private Vector2 GroundBounds;
	private float TimeSinceLastAttempt = 0;

	private GameObject ThePlayer;
	private List<GameObject> AllObjects;
	private bool DisplayRestartGUI;
	private Rect ScreenRect;

	// Use this for initialization
	void Start( )
	{
		RevolutionsRequired = LevelRevolutions;
		ScreenRect = new Rect( );
		DisplayRestartGUI = false;
		AllObjects = new List<GameObject>( );
		MainCamera = GetComponent<Camera>( );
		float ortho = MainCamera.orthographicSize;
		float ratio = Screen.width / Screen.height;
		MinY = transform.position.y + ortho * Vector2.down.y;
		MaxX = transform.position.x + ortho * ratio;
		MaxY = transform.position.y + ortho * Vector2.up.y;
		MinX = transform.position.x - ortho * ratio;
		WorldMoveSpeed = BaseMoveSpeed = MoveSpeed;
		RevolutionCount = 0;
		EnemyObjectList = EnemyObjects;
		PopulateLevel( );
	}

	void Update( )
	{
		if( null != ThePlayer && ( ThePlayer.GetComponent<Player>( ).IsDead || RevolutionCount == LevelRevolutions ) )
		{
			Destroy( ThePlayer );
			for( int i = 0; i < AllObjects.Count; ++i )
			{
				Destroy( AllObjects[ i ] );
			}
			ThePlayer = null;
			AllObjects.Clear( );
			DisplayRestartGUI = true;
		}

		TimeSinceLastAttempt += Time.deltaTime;
		if( TimeToSpawnEnemyOnGroundInSeconds < TimeSinceLastAttempt )
		{
			int Monster = SpawnMonster( );
			if( -1 != Monster )
			{
				GameObject Enemy = Instantiate( EnemyObjects[ Monster ],
				                                Vector2.zero,
				                                Quaternion.identity );
				Vector2 EnemyBounds = Enemy.GetComponent<Renderer>( ).bounds.size;
				Enemy.transform.position = new Vector2( MaxX + 1, MinY + GroundBounds.y + EnemyBounds.y / 2 );
				Enemy.AddComponent<MoveWorld>( );
				AllObjects.Add( Enemy );
			}
			TimeSinceLastAttempt = 0;
		}
	}

	void OnGUI( )
	{
		ScreenRect.x = Screen.width / 2 - Screen.width / 4;
		ScreenRect.y = Screen.height / 2 - Screen.height / 4;
		ScreenRect.width = Screen.width / 2;
		ScreenRect.height = Screen.height / 2;
		float Width = ScreenRect.width;
		float Height = ScreenRect.height;
		GUIStyle BoxStyle;
		BoxStyle = GUI.skin.box;
		BoxStyle.fontSize = 48;
		GUI.BeginGroup( ScreenRect );
		if( RevolutionCount == LevelRevolutions )
		{
			GUI.Box( new Rect( 0, 0, Width, Height ), "You win!", BoxStyle );
		}
		else if( DisplayRestartGUI )
		{
			GUI.Box( new Rect( 0, 0, Width, Height ), "You have died.", BoxStyle );
		}
		if( DisplayRestartGUI )
		{
			GUIStyle ButtonStyle = GUI.skin.button;
			ButtonStyle.fontSize = 30;
			if( GUI.Button( new Rect( Width / 2 - Width / 4, Height / 4 + 5, Width / 2, Height / 4 ), "Play again!" ) )
			{
				SceneManager.LoadScene( SceneManager.GetActiveScene( ).name );
			}
			if( GUI.Button( new Rect( Width / 2 - Width / 4, Height / 2 + 5, Width / 2, Height / 4 ),
			                "Quit... :'(",
			                ButtonStyle ) )
			{
				Application.Quit( );
			}
		}
		GUI.EndGroup( );
	}

	void PopulateLevel( )
	{
		GameObject RevolutionCounter = new GameObject( );
		AllObjects.Add( RevolutionCounter );
		RevolutionCounter.AddComponent<RevolutionCounterGUI>( );
		RevolutionCounter.GetComponent<RevolutionCounterGUI>( ).RequiredRevolutions = LevelRevolutions;
		// How much ground can we place?
		GameObject GroundObject = Instantiate( Ground, new Vector2( 0, 0 ), Quaternion.identity );
		Renderer GroundRenderer = GroundObject.GetComponent<Renderer>( );
		GroundBounds = GroundRenderer.bounds.size;
		Destroy( GroundObject );
		int GroundTotal = ( int )( ( MaxX - MinX ) / GroundBounds.x ) + 2 * Padding;
		int MaxPlatforms = ( int )( LevelWidth / 2 );
		float StartX = MinX - GroundBounds.x * Padding;
		GameObject GroundParent = new GameObject( );
		GroundParent.transform.position = new Vector2( StartX, MinY );
		GroundParent.name = "Ground";
		for( int i = 0; i < GroundTotal; ++i )
		{
			GroundObject = Instantiate( Ground, new Vector2( StartX, MinY + GroundBounds.y ), Quaternion.identity );
			StartX += GroundBounds.x;
			GroundObject.transform.parent = GroundParent.transform;
		}
		ThePlayer = Instantiate( Player,
		                        new Vector2( MinX, MinY + GroundBounds.y + 2 ),
		                        Quaternion.identity );
		ThePlayer.GetComponent<Player>( ).DeadX = MinX;
		ThePlayer.GetComponent<Player>( ).JumpForce = PlayerJumpForce;
		AllObjects.Add( GroundParent );
		float PlatformX = MinX + 2 * MaxPlayerJumpWidth;
		bool IsFirstPlatform = true;
		while( MaxPlatforms > 0 && PlatformX < LevelWidth )
		{
			int PlatformSize;
			if( MaxPlatforms > 10 )
			{
				PlatformSize = ( int )Random.Range( 3, 10 );
			}
			else
			{
				PlatformSize = ( int )Random.Range( 2, MaxPlatforms );
			}
			MaxPlatforms -= PlatformSize;
			float PlatformHeight;
			if( IsFirstPlatform )
			{
				// Guarantee the first platform is always within reach of the player.
				PlatformHeight = PlayerJumpHeight;
				IsFirstPlatform = false;
			}
			else
			{
				PlatformHeight = Random.Range( 0.4f, 3f ) * PlayerJumpHeight;
			}
			float PlatformActualSize = MakePlatform( PlatformSize, new Vector2( PlatformX, MinY + PlatformHeight ) );
			MaxPlatforms -= ( int )Mathf.Ceil( PlatformActualSize );
			PlatformX += Random.Range( PlayerJumpWidth, MaxPlayerJumpWidth );
		}
		MoveWorld.WorldEnd = PlatformX;
		GameObject RevolutionCountObject = Instantiate( RevolutionObject, new Vector2( MoveWorld.WorldEnd + 4, 0 ), Quaternion.identity );
		RevolutionCountObject.AddComponent<Platform>( );
		AllObjects.Add( RevolutionCountObject );
		MakeBackground( );
	}

	void MakeBackground( )
	{
		if( null == BackgroundObjects )
		{
			return;
		}

		GameObject TheBackground = new GameObject( "TheBackground" );

		GameObject GetWidthFromThisObject = Instantiate( BackgroundObjects[ 0 ], new Vector2( 0, 0 ), Quaternion.identity );
		float Width = GetWidthFromThisObject.GetComponent<Renderer>( ).bounds.size.x;
		Destroy( GetWidthFromThisObject );

		// assume 128x128 because.
		float NumWidth = Mathf.Ceil( ( float )LevelWidth / Width );
		// hardcoded because permanent window size.
		float NumHeight = 8;
		Vector2 Pos = new Vector2( MinX - Width, MaxY );
		for( int i = 0; i < NumWidth * NumHeight; ++i )
		{
			GameObject Background = Instantiate( BackgroundObjects[ ( int )Random.Range( 0, BackgroundObjects.Length ) ],
			                                     Pos,
			                                     Quaternion.identity );
			Background.AddComponent<MoveWorld>( );
			Background.transform.parent = TheBackground.transform;
			Pos.x += Background.GetComponent<Renderer>( ).bounds.size.x;
			if( Pos.x >= ( float )MoveWorld.WorldEnd )
			{
				Pos.x = MinX;
				Pos.y -= Background.GetComponent<Renderer>( ).bounds.size.y;
			}
		}
	}

	float MakePlatform( int Count, Vector2 StartPos )
	{
		float PlatformSize = 0;
		float Left = 0;
		GameObject Platform = new GameObject( );
		AllObjects.Add( Platform );
		Platform.transform.position = StartPos;
		Platform.AddComponent<Platform>( );
		Platform.name = "Platform";
		Platform.layer = LayerMask.NameToLayer( "Platform" );
		GameObject PlatformObject = Instantiate( LeftPlat, StartPos, Quaternion.identity );
		PlatformObject.transform.parent = Platform.transform;
		Renderer PlatRenderer = PlatformObject.GetComponent<Renderer>( );
		Transform PlatTrans = PlatformObject.transform;
		Vector2 Bounds = new Vector2( );
		Bounds.x = PlatRenderer.bounds.size.x;
		Left = Bounds.x;
		Bounds.y = PlatRenderer.bounds.size.y * PlatTrans.localScale.y;
		PlatformSize += Bounds.x;
		StartPos.x += Bounds.x + PlatformAdjustment;
		for( int i = 1; i < Count - 1; ++i )
		{
			PlatformObject = Instantiate( MidPlat, StartPos, Quaternion.identity );
			PlatRenderer = PlatformObject.GetComponent<Renderer>( );
			Bounds.x = PlatRenderer.bounds.size.x;
			PlatformSize += Bounds.x;
			StartPos.x += Bounds.x + PlatformAdjustment;
			PlatformObject.transform.parent = Platform.transform;
		}

		PlatformObject = Instantiate( RightPlat, StartPos, Quaternion.identity );
		PlatformObject.transform.parent = Platform.transform;
		PlatformSize += PlatformObject.GetComponent<Renderer>( ).bounds.size.x;
		Platform.AddComponent<BoxCollider2D>( );
		BoxCollider2D Collider = Platform.GetComponent<BoxCollider2D>( );
		Collider.size = new Vector2( PlatformSize + PlatformAdjustment * ( Count - 1 ), Bounds.y / PlatformObject.transform.localScale.y );
		Collider.offset = new Vector2( PlatformSize / 2 - Left / 2 + PlatformAdjustment * ( Count / 2 ), 0 );
		Platform.GetComponent<Platform>( ).Size = PlatformSize;
		Platform.GetComponent<Platform>( ).Bounds = Bounds;
		if( null != EnemyObjects )
		{
			int Monster = SpawnMonster( );
			if( -1 != Monster )
			{
				//TODO(anthony): Spawn the monster
				GameObject MonsterObject = Instantiate( EnemyObjects[ Monster ], new Vector2( StartPos.x - PlatformSize / 2, StartPos.y + Bounds.y ), Quaternion.identity );
				MonsterObject.transform.parent = Platform.transform;
				Platform.GetComponent<Platform>( ).HasMonster = true;
			}
		}
		return PlatformSize;
	}

	public static int SpawnMonster( )
	{
		int Result = -1;
		int ToSpawn = ( int )Random.Range( 0, RevolutionsRequired );
		ToSpawn -= RevolutionCount;
		if( ToSpawn > RevolutionsRequired / 2 - RevolutionCount )
		{
			Result = ( int )Random.Range( 0, EnemyObjectList.Length );
		}
		return Result;
	}
}
