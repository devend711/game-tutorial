using UnityEngine;
using System.Collections;

public class Soldier : Unit {
	private Animation anim;

	// Use this for initialization
	void Start () {
		base.Start ();
		animation = GetComponentsInChildren<Animation>()[0];
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		if (this.moving) {
			this.animation.CrossFade ("Walk");
		} else {
			this.animation.CrossFade("Idle");
		}
	}
}
