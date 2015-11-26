using UnityEngine;
using System.Collections;

public class Barracks : Building {
	protected override void Start () {
		base.Start();
		actions = new string[] { "Soldier" };
		this.maxBuildProgress = 20;
	}

	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}
}
