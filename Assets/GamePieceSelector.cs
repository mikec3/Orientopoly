using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;


// inherits FB login from FB. 
public class GamePieceSelector : FB {


	// override becauase it overides it's inheriting class's Start() method.
	protected override void Start () {
		base.Start ();		// log in to FB from FB class Start() method.

		reference.Child("Games").Child(PlayerPrefsManager.GetGameName()).Child("PiecesUsed").ChildAdded += PieceTaken;
	}

	// if child added is one of available pieces, deactivate that piece. Protects against two people 
	// choosing the same piece.
	public void PieceTaken(object sender, ChildChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		// Do something with args.Snapshot.Key.ToString 
		//loop through all available game pieces, if taken piece is still available, deactivate it.

		Button[] pieces = GameObject.FindObjectsOfType<Button> ();
		foreach (Button piece in pieces) {
			
//			Debug.Log ("PieceButton: " + piece.gameObject.tag);
//			Debug.Log ("child added: " + args.Snapshot.Key.ToString ());
			if (piece.tag == args.Snapshot.Key.ToString ()) {
				piece.gameObject.SetActive (false);
			}
		}


	}

	// takes string name from editor
	public void SelectGamePiece(string name){

		Debug.Log (PlayerPrefsManager.GetGameName());
		Debug.Log (PlayerPrefsManager.GetPlayerName ());

		// calls on public static reference to database. Set the name of the gamepiece in user's name.
		FB.reference.Child ("Games").Child (PlayerPrefsManager.GetGameName()).Child ("InGame").Child (PlayerPrefsManager.GetPlayerName()).Child ("GamePiece").SetValueAsync (name);

		// adds piece name to list of used pieces in DB. childAdded checks "PiecesUsed" to see if pieces are still available for other players.
		FB.reference.Child ("Games").Child (PlayerPrefsManager.GetGameName()).Child ("PiecesUsed").Child(name).SetValueAsync(0);

		// TODO add success message to user.
		Debug.Log(PlayerPrefsManager.GetPlayerName() + " chose " + name + " game piece");

		// Find the level manager and use it to get into the Game scene
		LevelManager levelManager = GameObject.FindObjectOfType<LevelManager> ().GetComponent<LevelManager> ();
		levelManager.LoadLevel ("Game");
	}

}   
