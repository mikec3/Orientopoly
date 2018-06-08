using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;		// for UI elements.

public class Board : MonoBehaviour {

	public int[][] boardSpots = new int[42][];	// 42 spots on board



	void Start(){
		BuildBoardPosArr (-850, -450, 100);

	}

	// build board spots array. 42 total pieces. 
	// corners are 0, 10, 21, 31
	// index 1-9 is L side
	// 11-20 is top side
	// 22-30 is R side
	// 32-41 is bottom side.

	public void BuildBoardPosArr(int xPos, int yPos, int increment){

		for (int x = 0; x <= boardSpots.Length - 1; x++) {
			if (x <= 0) {
				// do nothing if x is 0.
			} else if (x <= 10) {
				yPos += increment;		// 0-10 ADD Y 
			} else if (x <= 21) {
				xPos += increment;		// 11-21 ADD X
			} else if (x <= 31) {
				yPos -= increment;		// 22- 31 Subract Y
			} else if (x <= 41) {
				xPos -= increment;		// 32-41 Subtract X
			}

			boardSpots [x] = new int[2]{ xPos, yPos };
		}
	}

}
