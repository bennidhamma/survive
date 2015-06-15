using UnityEngine;
using System.Collections;

public class TreeBehavior : MonoBehaviour {
	
	// Use this for initialization
	void Awake () {
		this.transform.localScale = new Vector3(Random.Range(0.45f, 0.65f), Random.Range(0.6f, 0.8f), 1);
	}
	
	// Update is called once per frame
	void Update () {
		var rot = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
		rot.x = 0;
		rot.z = 0;
		this.transform.rotation = rot;
	}
}
