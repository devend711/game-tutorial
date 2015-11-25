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

	public void SetSelection (bool selected) {
		this.currentlySelected = selected;
	}

	public string[] GetActions() {
		return this.actions;
	}
	
	public virtual void PerformAction (string actionToPerform) {
	}

	public virtual void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller) {
		//only handle input if currently selected
		if(currentlySelected && hitObject && hitObject.name != "Ground") {
			WorldObject worldObject = hitObject.transform.root.GetComponent<WorldObject>();
			//clicked on another selectable object
			if(worldObject) ChangeSelection(worldObject, controller);
		}
	}

	private void ChangeSelection(WorldObject worldObject, Player player) {
		//this should be called by the following line, but there is an outside chance it will not
		SetSelection(false);
		if(player.SelectedObject) player.SelectedObject.SetSelection(false);
		player.SelectedObject = worldObject;
		worldObject.SetSelection(true);
	}
}
