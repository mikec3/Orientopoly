using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for UI elements

// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

// can use all public methods and variables from FB, if base.Start is called somewhere else, no need to call it again.(already logged in).

public class Dice : FB {
	private Text diceRollText;		// display's dice total value

	protected override void Start(){
		
		diceRollText = GameObject.Find ("DiceRollText").GetComponent<Text> ();		// find the dice roll text indicator in scene.

	}



	// roll two dice, save total and check for doubles.
	public int Roll(){
		int total = Random.Range (2, 13);	// random number between 2 and 12.

		diceRollText.text = total.ToString ();		// print dice roll total to UI text.
	
		return total;					// return the total 
	
	}
}
