using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// for Firebase
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

// firebase login class.

public class FB : MonoBehaviour {

	// static because it's the only database reference used in this game.
	// called by inheriting scripts as FB.reference.Child(etc..etc..
	public static DatabaseReference reference;		// holds the database reference, set below
	// Use this for initialization

	public string userIdNum;		// TODO finish this

	// protected virtual allows for inheriting classes to use and modify this method.
	protected virtual void Start () {
		// Set this before calling into the realtime database.
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orientopoly.firebaseio.com/");


		// ONLY FOR EDITOR TO LOGIN TO FIREBASE
		if (Application.isEditor) {
			FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://orientopoly.firebaseio.com/");
			FirebaseApp.DefaultInstance.SetEditorP12FileName ("Orientopoly-2be1799d7251.p12");
			FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail ("editor@orientopoly.iam.gserviceaccount.com");
			FirebaseApp.DefaultInstance.SetEditorP12Password ("notasecret");
		}
		// END EDITOR LOGIN TO FIREBASE


		// Get the root reference location of the database.
		reference = FirebaseDatabase.DefaultInstance.RootReference;

		//reference.Child ("Games").ChildAdded += HandleChildAdded;	// event listener on db for children added below games.

		// anonymous sign in.
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

		// anonymous firebase sign in.
		auth.SignInAnonymouslyAsync().ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInAnonymouslyAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
			userIdNum = newUser.UserId;		// TODO this too.
		});
	}
	

}
