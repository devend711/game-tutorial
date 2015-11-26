using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class HUD : MonoBehaviour {

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	private WorldObjectType orderBarState;

	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;
	public Texture2D buttonHover, buttonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D healthIcon;
	public Texture2D[] resourceIcons;

	private Dictionary< ResourceType, Texture2D > resourceImages;

	private CursorState activeCursorState;
	private int currentFrame = 0;

	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int LABEL_HEIGHT = 24;
	private const int PADDING = 5;
	private const int ICON_WIDTH = 16, ICON_HEIGHT = 16, TEXT_WIDTH = 128, TEXT_HEIGHT = 24;

	private Player player;
	private Dictionary< ResourceType, int > resourceValues, resourceLimits;

	private WorldObject lastSelectedObject;
	private float sliderValue;

	private int buildAreaHeight = 0;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
	private const int SELECTION_NAME_HEIGHT = 10;
	private const int BUTTON_SPACING = 10;
	private const int SCROLL_BAR_WIDTH = 22;

	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent< Player >();
		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		resourceValues = new Dictionary< ResourceType, int >();
		resourceLimits = new Dictionary< ResourceType, int >();
		orderBarState = WorldObjectType.None;
		SetupResourceIcons();
		SetCursorState(CursorState.Select);
		ResourceManager.StoreSelectBoxItems(this.selectBoxSkin);
	}
	
	// Update is called once per frame
	void OnGUI () {
		if(player && player.human) {
			DrawOrdersBar();
			DrawResourceBar();
			DrawMouseCursor();
		}
	}

	public bool MouseInBounds() {
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		return insideWidth && insideHeight;
	}

	private Rect GetCursorDrawPosition() {
		//set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
		//adjust position base on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}

	public void SetCursorState(CursorState newState) {
		activeCursorState = newState;
		switch(newState) {
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		default: break;
		}
	}

	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;

		if(mouseOverHud) {
			//show system cursor
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			GUI.skin = this.mouseCursorSkin;
			GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height));
			UpdateCursorAnimation();
			Rect cursorPosition = GetCursorDrawPosition();
			GUI.Label(cursorPosition, this.activeCursor);
			GUI.EndGroup();
		}
	}

	private void UpdateCursorAnimation() {
		//sequence animation for cursor (based on more than one image for the cursor)
		//change once per second, loops through array of images
		if(this.activeCursorState == CursorState.Move) {
			this.currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[this.currentFrame];
		} else if(activeCursorState == CursorState.Attack) {
			this.currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[this.currentFrame];
		} else if(activeCursorState == CursorState.Harvest) {
			this.currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[this.currentFrame];
		}
	}

	private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
		for(int i = 0; i < buildQueue.Length; i++) {
			float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * PADDING;
			Rect buildPos = new Rect(PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos, buildFrame);
			topPos += PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * PADDING;
			if(i==0) {
				//shrink the build mask on the item currently being built to give an idea of progress
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * PADDING, topPos, width, height), buildMask);
		}
	}

	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH,RESOURCE_BAR_HEIGHT,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");
		if (this.orderBarState == WorldObjectType.None) {
			GUI.EndGroup();
			return;
		}
		// draw the info
		int row = 0;
		// write the name of the currently selected object
		if(player.SelectedObject) {
			string selectionName = player.SelectedObject.objectName;
			if(selectionName != "") {
				int topPos = buildAreaHeight + BUTTON_SPACING;	
				GUI.Label(new Rect(PADDING,row*PADDING,ORDERS_BAR_WIDTH - 2*PADDING,LABEL_HEIGHT), selectionName);
			}
			if(player.SelectedObject.BelongsToPlayer(this.player)) {
				row++;
				GUI.Label(new Rect(PADDING,PADDING + row*LABEL_HEIGHT,ORDERS_BAR_WIDTH - 2*PADDING,LABEL_HEIGHT), "You own this");
				//reset slider value if the selected object has changed
				if(this.lastSelectedObject && this.lastSelectedObject != player.SelectedObject) this.sliderValue = 0.0f;
				DrawActions(player.SelectedObject.GetActions());
				//store the current selection
				this.lastSelectedObject = player.SelectedObject;
				if(this.orderBarState == WorldObjectType.Building) {
					// draw build status
					Building selectedBuilding = lastSelectedObject.GetComponent<Building>();
					if(selectedBuilding) {
						DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					}
				} else if(this.orderBarState == WorldObjectType.Unit) {
					Unit selectedUnit = lastSelectedObject.GetComponent<Unit>();
					if(selectedUnit) { 
						row++;
						Texture2D icon = this.healthIcon;
						string healthString = selectedUnit.currentHealth.ToString() + "/" + selectedUnit.maxHealth.ToString();
						DrawIconWithText(icon, PADDING, PADDING, PADDING + row*LABEL_HEIGHT, healthString);
					}
				}
			}
		}

		GUI.EndGroup();
	}

	private int MaxNumRows(int areaHeight) {
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}

	private Rect GetScrollPos(int groupHeight) {
		return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
	}
	
	private Rect GetButtonPos(int row, int column) {
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}
	
	private void DrawSlider(int groupHeight, float numRows) {
		//slider goes from 0 to the number of rows that do not fit on screen
		sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
	}

	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		int columnsPerRow = 2;

		//define the area to draw the actions inside
		GUI.BeginGroup(new Rect(0, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
		//draw scroll bar for the list of actions if need be
		if(numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
		//display possible actions as buttons and handle the button click for each
		for(int i = 0; i < numActions; i++) {
			int column = i % columnsPerRow;
			int row = i / columnsPerRow;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				//create the button and handle the click of that button
				if(GUI.Button(pos, action)) {
					Debug.Log ("clicked action " + actions[i]);
					if(player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
				}
			} else {
				Debug.Log("Couldn't find build image for " + actions[i]);
			}
		}
		GUI.EndGroup();
	}

	private void DrawIcon (Texture2D icon, int iconLeft, int topPos) {
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
	}

	private void DrawIconWithText(Texture2D icon, int iconLeft, int textLeft, int topPos, string text) {
		DrawIcon (icon, iconLeft, topPos);
		GUI.Label (new Rect(iconLeft + ICON_WIDTH, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}



	// RESOURCES

	private void DrawResourceIcon (ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
		DrawIconWithText (icon, iconLeft, textLeft, topPos, text);
	}

	private void DrawResourceBar() {
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
		// draw resource icons
		int topPos = 4, iconLeft = 4, textLeft = 20;
		foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType))) {
			DrawResourceIcon(resource, iconLeft, textLeft, topPos);
			iconLeft += TEXT_WIDTH;
			textLeft += TEXT_WIDTH;
		}
		GUI.EndGroup();
	}

	public Rect GetPlayingArea () {
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}

	public void SetResourceValues (Dictionary< ResourceType, int > resourceValues, Dictionary< ResourceType, int > resourceLimits) {
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}

	private void SetupResourceIcons () {
		this.resourceImages = new Dictionary<ResourceType, Texture2D>();

		// fake a foreach with index
		int i = 0;
		foreach(ResourceType resource in System.Enum.GetValues(typeof(ResourceType))) {
			this.resourceImages.Add(resource, resourceIcons[i]);
			// init the stores/limits of this to be 0
			this.resourceValues[resource] = 0;
			this.resourceLimits[resource] = 0;
			i++;
		};
	}

	public void setOrderBarState (WorldObjectType objectType) {
		this.orderBarState = objectType;
	}
}
