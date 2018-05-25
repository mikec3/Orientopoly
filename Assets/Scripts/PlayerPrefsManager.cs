using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour {

	const string playerName = "player name";		// string for storing key of PlayerPrefs playerName value. 
	const string gameName = "game name";			// string for storing key of PlayerPrefs gameName value.

	// save player's name to PlayerPrefs for later use in game and db.
	public static void SetPlayerName(string name){
		PlayerPrefs.SetString (playerName, name);
	}

	// return playerName from PlayerPrefs
	public static string GetPlayerName(){
		return PlayerPrefs.GetString (playerName);
	}

	// save name of this game
	public static void SetGameName(string game){
		PlayerPrefs.SetString (gameName, game);
	}

	// return the name of this game 
	public static string GetGameName(){
		return PlayerPrefs.GetString (gameName);	

	}

}
