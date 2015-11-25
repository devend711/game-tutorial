using UnityEngine;
using System.Collections;

public class WorldObject : MonoBehaviour {

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;

	// this is called before Start ()
	protected virtual void Awake () {

	}

	protected virtual void Start () {
		this.player = transform.root.GetComponentInChildren<Player>();
		this.cost = this.sellValue = 0;
		this.hitPoints = this.maxHitPoints = 0;
	}
	
	protected virtual void Update () {
		
	}
	
	protected virtual void OnGUI() {
	}

	public void SetSelection(bool selected) {
		this.currentlySelected = selected;
	}

	public string[] GetActions() {
		return this.actions;
	}
	
	public virtual void PerformAction(string actionToPerform) {
	}
}
