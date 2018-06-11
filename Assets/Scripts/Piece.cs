using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {

	//TODO have this find all of it's own data and hold it here. position, name, stats, etc...

	// Player's name. set by data from firebase when the piece is created. Looked at by many scripts for game play.
	public string pieceName;
	public int currentBoardPos;			// hold player's position
}
