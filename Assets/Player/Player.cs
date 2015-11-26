using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour {
	
	public string username;
	public bool human;

	public HUD hud;

	public WorldObject SelectedObject { get; set; }

	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	private Dictionary< ResourceType, int > resources, resourceLimits, startingResourceAmounts;

	void Awake () {
		resources = InitResourceList();
		resourceLimits = InitResourceList();
		startingResourceAmounts = InitResourceList();
	}

	void Start () {
		this.hud = GetComponentInChildren< HUD >();
		AddStartResourceLimits ();
		AddStartResources();
	}
	
	void Update () {
		if (this.human) hud.SetResourceValues(resources, resourceLimits);
	}

	// init resources
	private Dictionary<ResourceType, int> InitResourceList () {
		Dictionary< ResourceType, int > resourceList = new Dictionary<ResourceType, int>();
		// loop through all resources and set our store of them to 0
		foreach(ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType))) {
			Debug.Log(resourceType);
			resourceList.Add(resourceType, 0);
		}
		return resourceList;
	}

	private void AddStartResources () {
		foreach(ResourceType resource in System.Enum.GetValues(typeof(ResourceType))) {
			this.resources[resource] += this.startingResourceAmounts[resource];
		}
	}

	private void AddStartResourceLimits () {
		foreach(ResourceType resource in System.Enum.GetValues(typeof(ResourceType))) {
			this.resourceLimits[resource] += 100;
		}
	}

	private void IncResource(ResourceType resource, int amount) {
		// make sure we have under the limit
		if (this.resourceLimits [resource] > (this.resources [resource] + amount)) {
			this.resources [resource] += amount;
		}
	}

	private void IncResourceLimit(ResourceType resource, int amount) {
		this.resourceLimits[resource] += amount;
	}

	public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation) {
		// get all our units
		Units units = GetComponentInChildren<Units>();
		// build a new unit from a Prefab
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		// the new unit's parent is this players set of units
		newUnit.transform.parent = units.transform;
		Debug.Log ("added" + unitName + " to player");
	}
}
