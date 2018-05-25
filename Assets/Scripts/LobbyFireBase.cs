using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for UI elements
using System;				// for DateTime

// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;


public class LobbyFireBase : MonoBehaviour {

	public DatabaseReference reference;		// holds the database reference, set below

	private LevelManager levelManager;		// for level manager.

	public GameObject gameNameButton;			// prefab for gameNameButtons (selecting which game to join).

	private string thisGameName;

	// Use this for initialization
	void Start () {

		levelManager = GameObject.FindObjectOfType<LevelManager> ().GetComponent<LevelManager> ();		// level manager attached.

		// thisGameName initialized from PlayerPrefs.
		thisGameName = PlayerPrefsManager.GetGameName();

		
		// Set this before calling into the realtime database.
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orientopoly.firebaseio.com/");


		// ONLY FOR EDITOR TO LOGIN TO FIREBASE
		if (Application.isEditor) {
			FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://orientopoly.firebaseio.com/");
			FirebaseApp.DefaultInstance.SetEditorP12FileName ("Orientopoly-2be1799d7251.p12");
			FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail ("editor@orientopoly.iam.gserviceaccount.com");
			FirebaseApp.DefaultInstance.SetEditorP12Password ("notasecret");
		}
		// END EDITOR LOGIN TO FIREBASE


		// Get the root reference location of the database.
		reference = FirebaseDatabase.DefaultInstance.RootReference;

		reference.Child ("Games").Child(thisGameName).Child("Requests").ChildAdded += HandleChildAdded;	// event listener on db for children added below games.

		reference.Child("Games").Child(thisGameName).Child("InGame").ChildAdded += NewChildInGame;		// event listener at firebase for new children in game.
		// anonymous sign in.
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

		// anonymous firebase sign in.
		auth.SignInAnonymouslyAsync().ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInAnonymouslyAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
		});

	}

	// when a name is added to "Requests", create button with listeners and add it to "RequestContent" scroll view, so host
	// can tap names and add players to "InGame"
	void HandleChildAdded(object sender, ChildChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		GameObject button;		// declare variable to hold buttons as they're being instantiated
		Transform scroller = GameObject.Find("RequestContent").GetComponent<Transform>();
		// Do something with the data in args.Snapshot
		//Debug.Log(args.Snapshot);

		//Debug.Log("child added");
		// display nameOfGame buttons 
		button = Instantiate(gameNameButton, transform.position, Quaternion.identity) as GameObject;
		button.transform.SetParent (scroller, false);	// set parent to scroll view, keep original scale.
		Text buttonText = button.GetComponentInChildren<Text>();	// find text component of button in children
		buttonText.text = args.Snapshot.Key.ToString ();				// make text appear as nameOfGame

		Button btn = button.GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			GameAccessGranted (args.Snapshot.Key.ToString());		// attach method to move players name from "Requests" to "InGame"
		});

		// destroy the button after it's pressed.
		btn.onClick.AddListener(delegate{
			Destroy(button);
		});

	}

	// give access to game to the name passed in. Pass the name to "InGame" and delete it from "Reqeusts".
	public void GameAccessGranted(string nameToEnterGame){
		Debug.Log (name + " game access granted");

		// move name to "InGame"
		// also sets gamepeice as not set yet.
		reference.Child ("Games").Child (thisGameName).Child ("InGame").Child (nameToEnterGame).Child("GamePiece").SetValueAsync (0);
		// remove name from "Requests"
		reference.Child ("Games").Child (thisGameName).Child ("Requests").Child (nameToEnterGame).RemoveValueAsync ();

	}


	// shows names when added to "InGame" of firebase. Find's scroll view titled "InGameContent" and adds buttons with 
	// names of players on them. Buttons have NO onclick.
	 void NewChildInGame(object sender, ChildChangedEventArgs args){
				if (args.DatabaseError != null) {
					Debug.LogError(args.DatabaseError.Message);
					return;
				}
				// Do something with the data in args.Snapshot
				GameObject button;		// declare variable to hold buttons as they're being instantiated
				Transform scroller = GameObject.Find("InGameContent").GetComponent<Transform>();
				//Debug.Log("child added");
				// display nameOfGame buttons 
				button = Instantiate(gameNameButton, transform.position, Quaternion.identity) as GameObject;
				button.transform.SetParent (scroller, false);	// set parent to scroll view, keep original scale.
				Text buttonText = button.GetComponentInChildren<Text>();	// find text component of button in children
				buttonText.text = args.Snapshot.Key.ToString ();				// make text appear as nameOfGame
			}


	// change value of "GameStarted" in fB to 1(true). Value changed events on all players will signal game has begun.
	public void StartGame(){
		Debug.Log ("Start da game!");
		reference.Child ("Games").Child (thisGameName).Child ("GameStarted").SetValueAsync (1);
		// add this player (host) into game, BUT FIRST, go to select peice scene
		levelManager.LoadLevel("SelectPiece");		// bring this host to SelectPiece scene, THEN game scene after.

	}

}
