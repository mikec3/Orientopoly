using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for canvas (text) elements.

public class NameInputPopUp : MonoBehaviour {

	public GameObject popUp;		// this game object pop up
	public GameObject joinGameButton;		// join game button, will deactivate it.

	// Use this for initialization
	void Start () {
		
	}

	// activate text input popup and deactivate join game button.
	public void Activate(){
		popUp.SetActive (true);
		joinGameButton.SetActive (false);
	}

	// save player's name to PlayerPrefs.
	public void SetPlayerName(){
		Text input = GameObject.Find("NameInputText").GetComponent<Text>();
		PlayerPrefsManager.SetPlayerName (input.text);		// save input from text box into PLayerName PlayerPrefs.
		//Debug.Log( "PlayerPrefs playerName: " + PlayerPrefsManager.GetPlayerName());
		LevelManager levelManager = GameObject.FindObjectOfType<LevelManager>().GetComponent<LevelManager>();
		levelManager.LoadLevel ("CreateGame");
	}
}
