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

	// defining class for playerTurn data save obj. Use this to store player turn name and dice roll info at the same time.
	public class playerTurnDataObj {
		public string name;
		public int roll;

		public playerTurnDataObj(){
			this.name = " ";
			this.roll = 0;

		}

		public playerTurnDataObj(string name, int roll){
			this.name = name;
			this.roll = roll;
		}
	}
	
	public GameObject playerPiece;		// attached in editor. player peices.
	public GameObject diceManager;		// holds the dice manager object.
	public GameObject gameBoard;		// holds gameboard
	public GameManager gameManager;		// gameManager script attached in editor.

	// Use this for initialization
	protected override void Start () {
		base.Start ();		// inherits from FB, used to login to firebase.

		DisplayPieces ();		// display's game peices from firebase data.

		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("InGame").ChildChanged += NewColorSet;		// event listener at firebase for new children in game.
		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("GameStarted").ValueChanged += GameStartedValueChanged;				// listen for when game starts.

	}

	// when pieces are originally displayed, find first piece in heirarchy in scene and set it's name to be the PlayerTurn in FB. Other scripts will see the
	// newly added name and initiate that player's turn.
	private void IsItMyTurn(){
		GameObject[] firstPieceArr = GameObject.FindGameObjectsWithTag ("PlayerPiece");	

		GameObject firstPiece = firstPieceArr [0];
		Text tempText = firstPiece.GetComponentInChildren<Text> ();
		if (tempText.text == PlayerPrefsManager.GetPlayerName ().ToString()) {		// use the text below the piece to see if it matches with this player.
			Debug.Log ("I am the first player");

			// set this player as the current name in PlayerTurn in fb. 
			// it saves player name AND dice roll. Probably use JsonUtility to save a w/ setrawjsonvalue();
			// saves as playerTurn -> name: value, roll: value
//			int arr = 0;
//			playerTurnDataObj turn = new playerTurnDataObj(PlayerPrefsManager.GetPlayerName(), arr);
//			string json = JsonUtility.ToJson(turn);
//		
			reference.Child ("Games").Child (PlayerPrefsManager.GetGameName ()).Child ("PlayerTurn").Child ("name").SetValueAsync (PlayerPrefsManager.GetPlayerName ().ToString ());
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

	public Piece[] ReturnAllPlayerPiecesInScene(){
		GameObject[] activePieces = GameObject.FindGameObjectsWithTag ("PlayerPiece");	// build array of all game pieces in scene.
		Piece[] allPiecesInScene = new Piece[activePieces.Length];
		for (int x = 0; x <= allPiecesInScene.Length - 1; x++) {
			allPiecesInScene [x] = activePieces [x].GetComponent<Piece> ();	// populate allPiecesInScene array with Piece component from all active PlayerPiece gameobjects in scene.
		}
		return allPiecesInScene;			// return the array of all active Piece scripts in the scene.
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
		tempPiece.transform.SetParent (gameBoard.transform);		// set piece's parent to THIS
		tempPiece.transform.localScale = new Vector2(1f, 1f);		// reset scale, messed up for some reason after instantiating.
		tempPiece.transform.Translate (0, -posPlus, 0);		// move piece after being instantiated, increments as each piece is made.
		Text tempText = tempPiece.GetComponentInChildren<Text>();		// find text componant in children of peice image
		tempText.text = player;										// set piece's text to player's name.

		SetColor (tempPiece, piece);

		// add a way to identify the game piece objects later by name. (script on gamepiece object with a public varaible; and set it here).
		Piece pieceScript = tempPiece.GetComponent<Piece>();
		pieceScript.pieceName = player;		// set pieceName in Piece attached to gameobject to player's name from firebase.

		// Decided to not store game players in an array or dictionary, just going to look it up each time in scene, can optimize later if needed.
		//gameManager.allPieces.Add(player,pieceScript);			// populate the allPieces in gameManager as pieces are being created.
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

