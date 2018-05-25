using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for UI elements

public class Dice : MonoBehaviour {
	private int[] values;	// holds each dice roll value
	private Text diceRollText;		// display's dice total value

	void Start(){
		diceRollText = GameObject.Find ("DiceRollText").GetComponent<Text> ();	
	}



	// roll two dice, save total and check for doubles.
	public void Roll(){
		
		values = new int[2];	// reset to empty new array to hold two dice rolls.

		for (int x = 0; x < 2; x++) {			// two dice rolls
			values[x] = Random.Range (1, 7);	// random number between 1 & 6
			//Debug.Log (x + " roll " + values [x]);	//log each dice roll
		}
		int total = values [0] + values [1];		// total dice roll.
		//Debug.Log ("Total roll: " + total);		// log total.

		diceRollText.text = total.ToString ();		// print dice roll total to UI text.

		if (values [0] == values [1]) {				// check for doubles.
			Debug.Log ("DoubleROLL!!!");
		}
	}
}
