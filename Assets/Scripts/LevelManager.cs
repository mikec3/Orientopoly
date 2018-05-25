using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;		// allows for SceneManagement.
public class LevelManager : MonoBehaviour {

	// Use this for initialization
	private static int currentLevel;


	void Start() {



		currentLevel = SceneManager.GetActiveScene ().buildIndex;
	}
		



	// "retry" button on lose scene redirects user back to previously played scene
	public void RetryLevel(){
		Debug.Log ("loading same level again");
		SceneManager.LoadScene (currentLevel);

	}


	// loads level as stated in Unity (menu system)
	public void LoadLevel(string name) {

		Debug.Log ("Loading level " + name);
		SceneManager.LoadScene (name);
	}

	// Quits the game from the quit button
	public void QuitRequest () {
		Debug.Log("quit request");
		Application.Quit ();
	}

	// loads the next level in the game build settings order
	public void LoadNextLevel() {

		Debug.Log ("loading next scene");
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
	}

	public void LoadPreviousLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex - 1);
	}
}
