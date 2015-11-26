using UnityEngine;
using System.Collections.Generic;
using RTS;

public class Soldier : Unit {
	private Animation anim;

	// Use this for initialization
	override protected void Start () {
		base.Start ();
		this.resourceCosts = new Dictionary<ResourceType, int>(){ 
			{ResourceType.Grain, 50} 
		};
		animation = GetComponentsInChildren<Animation>()[0];
	}
	
	// Update is called once per frame
	override protected void Update () {
		base.Update();
		if (this.moving) {
			this.animation.CrossFade ("Walk");
		} else {
			this.animation.CrossFade("Idle");
		}
	}
}
