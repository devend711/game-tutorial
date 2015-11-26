using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : WorldObject {
	public float maxBuildProgress;
	protected Queue< string > buildQueue;
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	private int buildSpeed;

	public static int SPAWN_DISTANCE_FROM_BUILDING = 10;
	public static int DEFAULT_BUILD_SPEED = 10;
	
	protected override void Awake () {
		base.Awake();
		buildQueue = new Queue< string >();
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * SPAWN_DISTANCE_FROM_BUILDING;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * SPAWN_DISTANCE_FROM_BUILDING;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		buildSpeed = DEFAULT_BUILD_SPEED;
	}
	
	protected override void Start () {
		base.Start();
	}
	
	protected override void Update () {
		base.Update();
		ProcessBuildQueue();
	}
	
	protected override void OnGUI () {
		base.OnGUI();
	}

	protected void CreateUnit(string unitName) {
		buildQueue.Enqueue(unitName);
	}

	protected void ProcessBuildQueue() {
		if(buildQueue.Count > 0) {
			currentBuildProgress += Time.deltaTime * DEFAULT_BUILD_SPEED;
			if(currentBuildProgress > maxBuildProgress) {
				if(player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation);
				currentBuildProgress = 0.0f;
			}
		}
	}

	public string[] getBuildQueueValues() {
		string[] values = new string[buildQueue.Count];
		int pos = 0;
		foreach(string unit in buildQueue) values[pos++] = unit;
		return values;
	}
	
	public float getBuildPercentage() {
		return currentBuildProgress / maxBuildProgress;
	}
}
