using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for UI elements
using System;				// for DateTime

using UnityEngine.SceneManagement;		// allows for SceneManagement.

// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;


public class FireBase : MonoBehaviour {
	
	public GameObject nameInputPopUp;			// attached in editor.
	private LevelManager levelManager;			// attached below
	public Text validationText;					// for showing errors and success

	// test strings just in case PlayerPrefs isnt' set yet.
	public static string playerName = "testing";		// player's name, unless otherwise posted is testing.
	public static string gameName = "loot";							// holds game name.

	public DatabaseReference reference;		// holds the database reference, set below
	private bool duplicate = false;				// return true if duplicate nameOfGame found.

	public GameObject gameNameButton;			// prefab for gameNameButtons (selecting which game to join).

	private Transform scroller;					// game selection buttons are childed in the scene to this transform for display.

	private Transform debugWindowTransform;
	public Text debugText;				// Text prefab for debugging in scene.

	// Use this for initialization
	void Start () {


		// Set the player and game names if PlayerPrefs has been saved.
		if (PlayerPrefsManager.GetPlayerName() != null) {
			playerName = PlayerPrefsManager.GetPlayerName ();
		}
		if (PlayerPrefsManager.GetGameName() != null) {
			gameName = PlayerPrefsManager.GetGameName ();
		}
		validationText = GameObject.Find ("ValidationText").GetComponent<Text> ();
		if (!validationText)
			Debug.Log ("No ValidationText found!");

		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		if (!levelManager)
			Debug.Log ("No LevelManager found!");


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

		//reference.Child ("Games").ChildAdded += HandleChildAdded;	// event listener on db for children added below games.

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

		// if we're in the "FindGame" scene, display the open games.
		if (SceneManager.GetActiveScene ().name == "FindGame") {

			// game selection buttons are childed in the scene to this transform for display.
			scroller = GameObject.Find("Content").GetComponent<Transform>();

			//debugWindowTransform = GameObject.Find ("DebugWindow").GetComponent<Transform> ();	// for debuging without a console.

			StartCoroutine (DisplayOpenGames (done => {

				if (done) {
					// DO NEXT!!!!!!!!
					// listen for new games added AFTER DisplayOpenGames has been performed, so that same game doesn't show up twice.
					reference.Child ("Games").ChildAdded += NewGameAdded;		// listen if any new games get added; display the newly added game.
					Debug.Log ("Display Open Games Performed");



				} else {
					Debug.Log ("error in coroutine");
				}

			}));
		}

			
	}


	void HandleChildAdded(object sender, ChildChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		// Do something with the data in args.Snapshot
		//Debug.Log(args.Snapshot.Key.ToString());		// display's the newly added child's key.
	}

	// passes text to the DebugWindow in the scene.
	// must activate window in the scene and link it to this script first.
	private void Debugger(string text){
		Text button;		// declare variable to hold buttons as they're being instantiated
		button = Instantiate(debugText, transform.position, Quaternion.identity) as Text;
		button.transform.SetParent (debugWindowTransform, false);	// set parent to scroll view, keep original scale.
		button.text = text;

	}


	// a new game has been added, display it now for the user to select if desired.
	// checks to make sure it's not already displaying a game on screen.
	void NewGameAdded(object sender, ChildChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		bool duplicateFound = false;	// will be true if a button already matches the name of the game added.
		//Debug.Log("new game added: " + args.Snapshot.Key.ToString());
		GameObject[] gameNameButtons = GameObject.FindGameObjectsWithTag ("GameNameButton");	// build array of all game name buttons in scene.
		foreach (GameObject btn in gameNameButtons) {
			Text btnTxt = btn.GetComponentInChildren<Text> ();
			// TODO fix dis shit. For some reason, not seeing duplicates.
			Debug.Log (btnTxt.text);
			Debug.Log (args.Snapshot.Key.ToString ());
			Debug.Log ("break");

			if (btnTxt.text == args.Snapshot.Key.ToString ()) {	// if newly added game name matches a button already on display, don't create a new button.
				duplicateFound = true;

				break;
			}
		}

		// if the newly added game name is NOT already on display as a button, create a button with the name.
		if (!duplicateFound){
		CreateGameSelectButton(args.Snapshot.Key.ToString());	// add the newly added game to the screen: game selection buttons.

		}


	}


	public void CreateGame(){
		
		duplicate = false;		// declaring false before checking.
		Text input = GameObject.Find ("InputText").GetComponent<Text> ();		// find inputText in scene
		string nameOfGame = input.text;		// save inputText as nameOfGame

		string date = System.DateTime.Now.ToString();	// use date as placeholder data

		StartCoroutine (CheckForDuplicateGames(nameOfGame, done => {

			if (done) {
				// DO NEXT!!!!!!!!

				if (!duplicate){
				// If no duplicates found, save the name of game to db.
				reference.Child("Games").Child(nameOfGame).SetValueAsync (date);

					// set "Started" in firebase as 0. 1 will mean yes game has started.
					reference.Child("Games").Child(nameOfGame).Child("GameStarted").SetValueAsync(0);

					// PlayerTurn's value will hold the name of the player who's turn it currently is.
					reference.Child("Games").Child(nameOfGame).Child("PlayerTurn").Child("roll").SetValueAsync(0);
					reference.Child("Games").Child(nameOfGame).Child("PlayerTurn").Child("name").SetValueAsync(" ");


					// save playerName of host from PLayerPrefs to db. the name was set on the main menu
					// save's gamepiece not set yet also.
					reference.Child ("Games").Child (nameOfGame).Child ("InGame").Child (PlayerPrefsManager.GetPlayerName()).Child("GamePiece").SetValueAsync (0);

					Debug.Log(nameOfGame + " game stored to db.");
					// name of game added to static name of game.
					gameName = nameOfGame;
					PlayerPrefsManager.SetGameName(gameName);	// save player prefs game name.

					levelManager.LoadLevel("Lobby");		// go to lobby scene
	
				} else {
					Debug.Log("duplicate name found: " + nameOfGame);	// duplicate found.

			
				}


			} else {
				Debug.Log ("error in coroutine");
			}

		}));
	}


	// call with StartCor
	public IEnumerator CheckForDuplicateGames(string child, System.Action<bool> done){
		var task = FirebaseDatabase.DefaultInstance.GetReference ("Games").GetValueAsync ();

		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted){
			// handle the error
			Debug.Log("could not read database");
		}
		else {
			// handle data
			DataSnapshot snapshot = task.Result;
			// loops through all children of "Games" ex. childSnap.Key = "Mike's Game"
			foreach (var childSnap in snapshot.Children) {
				//Debug.Log (childSnap.Key.ToString());		// nameOfGame
				if (childSnap.Key.ToString () == child) {
					duplicate = true;	// duplicate found.
				}


			}
			done (true);		// signal to startCouritine calling this that processing is done.

		}
	}

	// queries all children of "Games", attaches names to buttons and fills scrollView with nameOfGameButtons.
	public IEnumerator DisplayOpenGames(System.Action<bool> done){	// trigger done bool TRUE when finished.
		var task = FirebaseDatabase.DefaultInstance.GetReference ("Games").GetValueAsync ();

		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted){
			// handle the error
			Debug.Log("could not read database");
		}
		else {
			// handle data
			DataSnapshot snapshot = task.Result;
			// loops through all children of "Games" ex. childSnap.Key = "Mike's Game"
			foreach (var childSnap in snapshot.Children) {
				//Debug.Log (childSnap.Key.ToString());		// nameOfGame

				// display nameOfGame buttons 
				 CreateGameSelectButton(childSnap.Key.ToString());

			}
			done (true);		// signal to startCouritine calling this that processing is done.

		}
	}

	public void CreateGameSelectButton(string nameOfTheGameAdded){
		GameObject button;		// declare variable to hold buttons as they're being instantiated
		button = Instantiate(gameNameButton, transform.position, Quaternion.identity) as GameObject;
		button.transform.SetParent (scroller, false);	// set parent to scroll view, keep original scale.
		Text buttonText = button.GetComponentInChildren<Text>();	// find text component of button in children
		buttonText.text = nameOfTheGameAdded;				// make text appear as nameOfGame

		Button btn = button.GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			AskForName (buttonText.text);		// attach onClick event, pop up asking for player's name.
		});

	}

	// popup asking for player's name becomes active.
	// buttonText param holds the nameof the game.
	public void AskForName(string buttonText){
		nameInputPopUp.SetActive (true);
		gameName = buttonText;	// set gameName for all further lookups and game requests.
	}


	// perform check for duplicate names.
	public void SubmitNameToGame(){
		duplicate = false;		// reset duplicate to false, will go true if duplicate name found
		Text input = GameObject.Find("NameInputText").GetComponent<Text>();
		playerName = input.text;
		StartCoroutine (CheckForDuplicateNames(playerName, "Requests", done => {

			if (done) {
				// DO NEXT!!!!!!!!
				if (!duplicate){
			// no duplicates found in "Requests", search "InGame" now.

					CheckDuplicatesAgain(playerName);	// check 'InGame' for duplicate names.


				} else{
					Debug.Log("duplicate name found in 'Requests': " + playerName);	// duplicate found.
					playerName = "";		// reset playerName to blank becuase it's a duplicate.
				}



			} else {
				Debug.Log ("error in coroutine inside 'Requests' duplicate check");
			}

		}));
	}


	public void CheckDuplicatesAgain(string name){
		StartCoroutine (CheckForDuplicateNames(name, "InGame", done => {

			if (done) {
				// DO NEXT!!!!!!!!
				if (!duplicate){
					// If no duplicates found
					Debug.Log(name + " submitted to 'Requests'");
					// submit name to requests.
					reference.Child("Games").Child(gameName).Child("Requests").Child(name).SetValueAsync(0);

					// save playerName (name in this method) into playerPrefs
					PlayerPrefsManager.SetPlayerName(name);
						
					validationText.text = "Success! Wait for the host to accept your request.";
					// listen to "inGame" when names added, check to see if it's this.playerName
					reference.Child("Games").Child(gameName).Child("InGame").ChildAdded += ChildAddedToInGame;

					// save gameName to playerPrefs now that this player is added to "InGame".
					PlayerPrefsManager.SetGameName(gameName);

				} else{ 
					Debug.Log("duplicate name found in 'InGame': " + name);	// duplicate found.
					validationText.text = "Duplicate name found, please choose another name.";
				}




			} else {
				Debug.Log ("error in coroutine inside 'InGame' duplicate check");
			}

		}));
		nameInputPopUp.SetActive (false);		// deactivate nameInput pop up window.
	}


	public IEnumerator CheckForDuplicateNames(string name, string directory, System.Action<bool> done){
		var task = FirebaseDatabase.DefaultInstance.GetReference ("Games").Child(gameName).Child(directory).GetValueAsync ();

		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted){
			// handle the error
			Debug.Log("could not read database");
		}
		else {
			// handle data
			DuplicateChecker (name, task.Result);		// check for duplicates.

			done (true);		// signal to startCouritine calling this that processing is done.

		}
	}

	public void DuplicateChecker(string name, DataSnapshot snapshot){
		// loops through all children of "Games" ex. childSnap.Key = "Mike's Game"
		foreach (var childSnap in snapshot.Children) {
			//Debug.Log (childSnap.Key.ToString());		// nameOfGame
			if (childSnap.Key.ToString () == name) {
				duplicate = true;	// duplicate found.
			}


		}
	}

	// is called whenever a child is added to "inGame", logs success if this playerName is the same as childAdded.
	public void ChildAddedToInGame(object sender, ChildChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		//Debug.Log ("child added to 'inGame'");
		// add listener to "GameStarted" on db to let player know game has started.
		if (args.Snapshot.Key.ToString () == playerName) {
			Debug.Log (args.Snapshot.Key.ToString () + " has been approved to enter " + gameName);
			validationText.text = "You've been approved to play in " + gameName + ". Wait for the host to start the game.";

			Invoke ("GoToPieceSelectorScene", 2f);		// take player to piece selector scene after delay.


		}


	}

	public void GoToPieceSelectorScene(){
		levelManager.LoadLevel("SelectPiece");		// take player to select piece scene, will go to game after.
	}



}
