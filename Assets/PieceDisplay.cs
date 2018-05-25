using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;


// inherits FB login from FB. 
// inherits from FB, used to login to firebase.

public class PieceDisplay : FB {
	
	public GameObject playerPiece;		// attached in editor. player peices.
	public GameObject diceManager;		// holds the dice manager object.

	// Use this for initialization
	protected override void Start () {
		base.Start ();		// inherits from FB, used to login to firebase.

		DisplayPieces ();		// display's game peices from firebase data.

		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("InGame").ChildChanged += NewColorSet;		// event listener at firebase for new children in game.
		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("GameStarted").ValueChanged += GameStartedValueChanged;				// listen for when game starts.
		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").ValueChanged += PlayerTurnChanged;						// listen for when it's now someone's turn.

	}

	// if new player name in PlayerTurn(fb) matches this player, than display the dice roller.
	private void PlayerTurnChanged(object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		//Debug.Log(args.Snapshot.Value);
		if (args.Snapshot.Value.ToString() == PlayerPrefsManager.GetPlayerName ().ToString()) {
			// it's now this player's turn, start the turn.
			//Debug.Log("dice manager should turn on now");
			diceManager.SetActive (true);	// my turn, turn dice on.
		} else {
			diceManager.SetActive (false);	// not my turn, turn dice off.
		}
	}


	private void IsItMyTurn(){
		GameObject firstPiece = this.gameObject.transform.GetChild (0).gameObject;
		Text tempText = firstPiece.GetComponentInChildren<Text> ();
		Debug.Log (PlayerPrefsManager.GetPlayerName ());
		if (tempText.text == PlayerPrefsManager.GetPlayerName ()) {
			Debug.Log ("I am the first player");

			// set this player as the current name in PlayerTurn in fb. 
			reference.Child ("Games").Child (PlayerPrefsManager.GetGameName ()).Child ("PlayerTurn").SetValueAsync (PlayerPrefsManager.GetPlayerName ());
		}
	}


	// when GameStarted's value changes, listen for 1(true), if 1(true) START DA GAME!!!
	public void GameStartedValueChanged(object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		//Debug.Log(args.Snapshot.Value);
		if (System.Convert.ToInt32(args.Snapshot.Value) == 1) {
			// game has been started by host.
			Debug.Log("Game has been started by host");
			//bring player to select game piece, will go to game scene after piece selection
			//levelManager.LoadLevel("SelectPiece");		// take player to select piece scene, will go to game after.
		}
	}


	public void NewColorSet(object sender, ChildChangedEventArgs args){
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		// do something with args.Snapshot
		//Debug.Log(args.Snapshot.Key.ToString());		// name of player

		string newColorName = "";		// string name of the new color that was added in firebase.

		// find the value within child called "GamePiece", set newColorName to be the changed value (a color probably).
		foreach (var child in args.Snapshot.Children) {
		//	Debug.Log (child.Key.ToString());			// should say "GamePiece".
			if (child.Key.ToString() == "GamePiece"){	// only look at children's value if it's GamePiece value.
				//Debug.Log (child.Value);					// should be 0 if color not set, or a string color name.
				newColorName = child.Value.ToString();		// set newColorName as the value of GamePiece from fb. (should be a color name).
			}
		}


		GameObject[] activePieces = GameObject.FindGameObjectsWithTag ("PlayerPiece");	// build array of all game pieces in scene.
		GameObject changedPiece;
		Piece pieceScript;
		foreach (GameObject piece in activePieces) {	// if we found the piece that is also the name of peice changed from firebase, then change that piece's color.
			pieceScript = piece.GetComponent<Piece> ();
			if (pieceScript.pieceName == args.Snapshot.Key.ToString ()) {	// if piece name from scene gameobject and piece name from firebase match.
				SetColor(piece, newColorName);		// set the color of the newly changed piece.
				break;		// break loop, correct piece was just found.
			}
		}


	}
				
	// search for pieces in firebase. 
	public void DisplayPieces(){
		StartCoroutine (SearchForPieces(done => {

			if (done) {
				// DO NEXT!!!!!!!! search is done.
				//Debug.Log("Pieces Searched");
				// IsItMyTurn happens here so that pieces can be displayed before searching for them.
				IsItMyTurn();		// look at first child of piecedisplay, if it matches this user, it's your turn.
			} else {
				Debug.Log ("error in coroutine inside SearchForPieces()");
			}

		}));
	}

	// search for pieces
	public IEnumerator SearchForPieces(System.Action<bool> done){
		var task = FirebaseDatabase.DefaultInstance.GetReference ("Games").Child(PlayerPrefsManager.GetGameName()).Child("InGame").GetValueAsync ();

		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted){
			// handle the error
			Debug.Log("could not read database");
		}
		else {
			// handle data
			LogData(task.Result);		// check for duplicates.

			done (true);		// signal to startCouritine calling this that processing is done.

		}
	}


	// loop through data snapshot to extract player name and gamePiece name.
	public void LogData(DataSnapshot snapshot){
		// loops through all children of "Games" -> "InGame"
		float PosCounter = 0;	// increments the position of the instantiated game pieces.
		Vector3 pos = transform.position;
		foreach (var player in snapshot.Children) {
			//Debug.Log (player.Key.ToString());		// logs player's name
			foreach (var piece in player.Children) {
				if (piece.Key.ToString () == "GamePiece") {
					//Debug.Log (piece.Value.ToString ());	// logs game piece name
					//Debug.Log(player.Key.ToString() + " " + piece.Value.ToString());	// log player name and game peice name.

					CreatePieces(player.Key.ToString(), piece.Value.ToString(), PosCounter);	// pass player name and piece name into CreatePieces 
					PosCounter = PosCounter + 2;	// incrementally moves pieces as they are instantiated.
				} 
			}
			}


		}

	public void CreatePieces(string player, string piece, float posPlus){

		// instantiate a new game piece.
		GameObject tempPiece = Instantiate(playerPiece, transform.position, Quaternion.identity) as GameObject;
		tempPiece.transform.SetParent (this.transform);		// set piece's parent to THIS
		tempPiece.transform.localScale = new Vector2(1f, 1f);		// reset scale, messed up for some reason after instantiating.
		tempPiece.transform.Translate (0, -posPlus, 0);		// move piece after being instantiated, increments as each piece is made.
		Text tempText = tempPiece.GetComponentInChildren<Text>();		// find text componant in children of peice image
		tempText.text = player;										// set piece's text to player's name.

		SetColor (tempPiece, piece);

		// add a way to identify the game piece objects later by name. (script on gamepiece object with a public varaible; and set it here).
		Piece pieceScript = tempPiece.GetComponent<Piece>();
		pieceScript.pieceName = player;		// set pieceName in Piece attached to gameobject to player's name from firebase.
		//Debug.Log(pieceScript.pieceName);		// logs the player's name saved in variable pieceName in script called piece which is attached to gameobject.
	}

	public void SetColor(GameObject obj, string color){
		string rgbC;
		switch (color) {
		case "LightBlue":
			rgbC = "#07C5FFFF";
			break;
		case "DarkGreen":
			rgbC = "#2AE80AFF";
			break;
		case "Gold":
			rgbC = "#FFC717FF";
			break;
		case "Red":
			rgbC = "#E82B0EFF";
			break;
		case "Purple":
			rgbC = "#7900FFFF";
			break;
		case "Pink":
			rgbC = "#D211FFFF";
			break;
		case "DarkBlue":
			rgbC = "#0C77E8FF";
			break;
		case "LightGreen":
			rgbC = "#07FF2EFF";
			break;
		case "Yellow":
			rgbC = "#E8CA07FF";
			break;
		case "Orange":
			rgbC = "#FF5917FF";
			break;
		default:
			rgbC = "#000000";	// make default pink
			break;				
		}


		Image coloredPiece = obj.GetComponentInChildren<Image> ();
		Color myColor = new Color ();
		ColorUtility.TryParseHtmlString(rgbC, out myColor);
		coloredPiece.color = myColor;
	}
	}

