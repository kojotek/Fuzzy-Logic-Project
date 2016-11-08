using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour {

    public Camera camera1;
    public Camera camera2;


    // Use this for initialization
    void Start () {
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void OnGUI () {
	    if (GUI.Button(new Rect(0,0, 100, 30), "ChangeCamera")) {
            if (camera1.gameObject.activeSelf) {
                camera1.gameObject.SetActive(false);
                camera2.gameObject.SetActive(true);
            }
            else {
                camera1.gameObject.SetActive(true);
                camera2.gameObject.SetActive(false);
            }
        }
	}
}
