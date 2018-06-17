using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for canvas elements.
// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

// TODO make this brief, manage the state of the game,
// hold game state like playerTurn, positions, amounts, stats, etc...
// purpose is to reduce traffic on firebase, get info, update if changed, and use for all calcs.
public class GameManager : FB {	// Inherits from FB class, login to firebase.


	public Dice diceManager;		// attached in editor.
	private Text gameText;			// attached in editor. Display's game info like player turn and dice roll.
	private Board board;			// holds board script.
	public PieceDisplay pieceDisplay;		// pieceDisplay script attached in editor.

	public GameObject endTurnButton;		// button that ends player's turn and tells firebase who next player is. Listeners then see new player's turn name.


	public Slider Xslider;			// for debugging UI stuffs.
	public Slider Yslider;
	public Slider IncSlider;

	public GameObject boardPiece;		// board pieces prefab
	public GameObject cornerPiece;		// corner pieces prefab

	private GameObject[] gameBoardArray = new GameObject[42];		// array filled with board game spots.

	// Not currently using a data struct to hold all players in scene, will just look up all player pieces in scene each time, will optimize later if needed.
	//public Dictionary<string, Piece> allPieces = new Dictionary<string, Piece>();		// Dictionary holding all current Pieces.

	public string playerCurrentTurn;		// player currently taking the turn.

	public int lastRoll;					// last dice roll.

	protected override void Start(){
		base.Start ();

		gameText = GameObject.Find ("GameText").GetComponent<Text> ();	// gameText attached.
		board = GetComponent<Board>();		// board component attached.

		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").Child("name").ValueChanged += PlayerTurnChanged;
		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").Child("uId").ValueChanged += RollValueChanged;

		showBoardPos ();		// initial slider values are x:-514 y:-417 inc:94
	}

	// TODO fix this so that pieces show up on each piece.
	// build board spots array. 42 total pieces. 
	public void showBoardPos(){
		//Debug.Log ("show board pos");

		// rebuild the board poss array with slider values.
		board.BuildBoardPosArr (System.Convert.ToInt32 (Xslider.value), System.Convert.ToInt32 (Yslider.value), System.Convert.ToInt32 (IncSlider.value));

		foreach (Transform child in this.transform) {		// find and kill all children first. (clears board).
			Destroy (child.gameObject);
		}

		// adjust each peice according to it's index.

		Vector3 tempPos = new Vector3();
		for (int x = 0; x <= board.boardSpots.Length - 1; x++) {		// go through each item in boardSpots array.
			tempPos.x = board.boardSpots[x][0];
			tempPos.y = board.boardSpots[x][1];
	
			AdjustPiece(x, tempPos);			// adjust piece created according to it's index. 
		}
	}

	//TODO refactor board creation to be in board script.
	// corners are 0, 10, 21, 31
	// index 1-9 is L side
	// 11-20 is top side
	// 22-30 is R side
	// 32-41 is bottom side.
	public void AdjustPiece(int index, Vector3 tempPos){
		GameObject tempPiece;

		if (index == 0 || index == 10 || index == 21 || index == 31) {		// use index to find corners, instantiate corner piece, otherwise instantiate a normal board piece.
			tempPiece = Instantiate (cornerPiece) as GameObject;
		} else {
		tempPiece = Instantiate(boardPiece) as GameObject;			// instantiate using one parameter, adjusting rest below.
		}

		if (index <= 9) {
			tempPiece.transform.localEulerAngles = new Vector3 (0, 0, 270f);
		} else if (index >= 11 && index <= 20) {
			tempPiece.transform.localEulerAngles = new Vector3 (0, 0, 180f);
		} else if (index >= 22 && index <= 30) {
			tempPiece.transform.localEulerAngles = new Vector3 (0, 0, 90f);
		}

		tempPiece.transform.SetParent (this.transform);	// set parent
		tempPiece.transform.localScale = new Vector2(1f, 1f);		// reset scale, messed up for some reason after instantiating.
		// move pieces according to tempPos
		tempPiece.transform.localPosition = tempPos;
		gameBoardArray [index] = tempPiece;			// set each piece being created to the corresponding index in gameBoardArray
	}

	// if new player name in PlayerTurn(fb) matches this player, than display the dice roller.
	private void PlayerTurnChanged(object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		playerCurrentTurn = args.Snapshot.Value.ToString ();
		//Debug.Log(args.Snapshot.Value);
		if (playerCurrentTurn == PlayerPrefsManager.GetPlayerName ().ToString()) {
			// it's now this player's turn, start the turn.
			//Debug.Log("dice manager should turn on now");
			diceManager.gameObject.SetActive (true);	// my turn, turn dice on.
		} else {
			diceManager.gameObject.SetActive (false);	// not my turn, turn dice off.
		}

		// Display to this user who's turn it currently is.
		Debug.Log("It's " + playerCurrentTurn + "'s turn");
	}

	//TODO currently if a player rolls the same thing as the last player, it doesn't trigger this method.
	//TODO fix me first.
	// make players move with dice roll.
	// look at new die roll, tell it to all players.
	private void RollValueChanged(object sender, ValueChangedEventArgs args) {


		StartCoroutine (SearchPlayerTurnAndDiceRoll (done => {

			if (done) {
				// DO NEXT!!!!!!!!
				// TODO(get the roll Value from firebase), update this script, show roll on screen, move piece, find next player.
				//process dice roll data. from args. 
				Debug.Log(playerCurrentTurn + " rolled a " + lastRoll);	// ex. john rolled a 5.
				gameText.text = playerCurrentTurn + " rolled a " + lastRoll;	// show on screen who rolled what.
				//Piece pieceScript = FindThisPlayer(playerCurrentTurn).GetComponent<Piece>();


				MovePiece (lastRoll); // TODO determine next player's turn

			} else {
				Debug.Log ("error in coroutine");
			}

		}));
	
	}

	public Piece MovePiece(int rollV){
		// look at all Piece object types in screen. Loop through each one, if current player turn matches with piece found, then move it. 
		// Increment player position based on roll value, also, account for rolling over past 41.
		Piece[] pieceArray = GameObject.FindObjectsOfType<Piece> ();
		foreach (var p in pieceArray) {
			if (p.pieceName == playerCurrentTurn) {
				p.currentBoardPos += rollV;
				if (p.currentBoardPos > 41) {							// roll over the counter if passes 41.
					p.currentBoardPos = p.currentBoardPos - 42;
				}
				//Debug.Log (p.pieceName + " is now at board pos: " + p.currentBoardPos);

				p.transform.localPosition = gameBoardArray[p.currentBoardPos].transform.localPosition;	// move current piece to board piece that corresponds to newly updated board position.
				return p;
				break;
			}
		}
		return null;
	}

	// find all players in scene, make it the next player's turn.
	public void NewPlayersTurn(){
			// turn OFF the endTurnButton, because in order to get to this method, they must have pressed it.
		endTurnButton.gameObject.SetActive(false);
		Piece[] piecesInScene = pieceDisplay.ReturnAllPlayerPiecesInScene();	// create an array of all active pieces
		for (int x = 0; x <= piecesInScene.Length - 1; x++) {
			if (piecesInScene [x].pieceName == playerCurrentTurn) {			// find the current player in active pieces
				
				int nextIndex = x + 1;									// increment to next player in heirarchy.
				if (nextIndex == piecesInScene.Length) {				// rollover index if at bottom of heirarchy.
					nextIndex = 0;
				}


				// set next player's name in firebase. childUPdated methods will see this and trigger that player's turn.
				reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").Child("name").SetValueAsync(piecesInScene[nextIndex].pieceName);
				Debug.Log ("setting " + piecesInScene [nextIndex].pieceName + " as next player");
				break;					// get out of the loop after setting the next player's name in firebase.
			}
		}
				
			}

		
	

	// perform dice roll and send results to FB.
	public void RollDice(){
		int diceRollResults = diceManager.Roll ();	// initialize diceRollResults to hold the results that will be returned.
		Debug.Log (diceRollResults);			// log dice roll results
		diceManager.gameObject.SetActive (false);			// turn off dice gameobject AFTER player has rolled.
		endTurnButton.gameObject.SetActive(true);			// turn ON the endTurnButton GameObject. Player can end their turn.

		// send dice rresults to fb.
		// TODO player turn isn't working sometimes
		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").Child("roll").SetValueAsync(diceRollResults);

		reference.Child ("Games").Child (PlayerPrefsManager.GetGameName ()).Child ("PlayerTurn").Child ("uId").SetValueAsync(Random.Range(0f,10000f));				// set the key as the value for uId.

		}

	// search for pieces
	public IEnumerator SearchPlayerTurnAndDiceRoll(System.Action<bool> done){
		var task = FirebaseDatabase.DefaultInstance.GetReference ("Games").Child(PlayerPrefsManager.GetGameName()).Child("PlayerTurn").GetValueAsync ();

		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted){
			// handle the error
			Debug.Log("could not read database");
		}
		else {
			DataSnapshot snapshot = task.Result;
			bool nameSame = true;
			bool diceSame = true;
			// loops through all children of "Games" ex. childSnap.Key = "Mike's Game"
			foreach (var childSnap in snapshot.Children) {
				//Debug.Log (childSnap.Key.ToString());		// nameOfGame
				if (childSnap.Key.ToString () == "name" && childSnap.Value.ToString () != playerCurrentTurn) {
					nameSame = false;
				
				} 
				if (childSnap.Key.ToString () == "roll" && System.Convert.ToInt32 (childSnap.Value) != lastRoll) {
					diceSame = false;

				}

				// debug area _________________,,,,

//				if (childSnap.Key.ToString () == "name") Debug.Log (childSnap.Key.ToString () + " " + childSnap.Value.ToString () + " " + playerCurrentTurn);
//				if (childSnap.Key.ToString () == "roll") Debug.Log (childSnap.Key.ToString () + " " + System.Convert.ToInt32 (childSnap.Value) + " " + lastRoll);

				// end debug area ^^^^^^^^^^^^

				if (childSnap.Key.ToString () == "roll") {
					lastRoll = System.Convert.ToInt32 (childSnap.Value);			// set value of lastroll as the value from firebase.
				}
			}
			if (!nameSame && !diceSame) {
				gameText.text = "ON NO!!!!! dice results not uploaded!";
				Debug.Log ("WARNING!!!!! dice results not uploaded!");
			} else {
				Debug.Log ("dice results uploaded");
			}
			done (true);		// signal to startCouritine calling this that processing is done.

		}
	}
}


