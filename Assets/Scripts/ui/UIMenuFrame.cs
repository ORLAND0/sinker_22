using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMenuFrame : MonoBehaviour {
	
	public float gridSizeXMax=-1f;
	public float gridSizeYMax=-1f;
	public float gridSizeX=-1f;//how wide this menuframe is gridSizeX*gridWidth
	public float gridSizeY=-1f;// float seems necessary
	public float gridWidth=360f;
	public float gridHeight=40f;
	public float alignX=.5f;
	public float alignY=.5f;
	public float offsetX=0f;
	public float offsetY=0f;
	public int disableInput=-1;//-1 disables input
	
	public int active=-1;//-1 init, 0 , 1
	public int layer=-1;//first layer is 1

	
	public List<UIMenuItem> menuItems=new List<UIMenuItem>();
	public Dictionary<string,UIMenuItem> itemsDict=new Dictionary<string,UIMenuItem>();
	[HideInInspector]
	public UIMenuItem selectedItem;
	[HideInInspector]
	public GameObject itemsOb;
	[HideInInspector]
	public Transform itemsTrans;
	public Color bgColor=new Color(.1f,.1f,.1f,.88f); //Color bgColor=new Color(.16f,.15f,.15f,.88f);
	[HideInInspector]
	public GameObject bgOb;	
	[HideInInspector]
	public GameObject offclickOb;//todo
	public string audioClose="menu_close"; 
	
	public DelegateGameObject callbackEnable;
	public DelegateGameObject callbackUpdate;
	public IntDelegateGameObject callbackEnter;


	
	static public List<UIMenuFrame> openFrames = new List<UIMenuFrame>();
	
	
	public void Awake(){
		gameObject.layer=MasterGame.phys_layers["ui"];
		transform.parent=MasterUI.menuFrameTrans;
		gameObject.Localize();
		if(itemsOb==null){
			itemsOb=new GameObject(gameObject.name+"_items");
			itemsTrans=itemsOb.transform;
			itemsTrans.parent=transform;
			itemsOb.Localize();
			
		}
		MasterUI.AddUIOb(gameObject);
		
	}
	
	void Start () {
		if(MasterUI.cursorMove<4){MasterUI.cursorMove=0;}
		Refresh(false);
		if(selectedItem==null){
			PressDown();
		}
	}
	
	void Update () {
		if(disableInput<1&&!cInput.scanning){
			if(cInput.GetKeyDown("Up")){		PressUp();		}		
			if(cInput.GetKeyDown("Down")){		PressDown();		}		
			if(cInput.GetKeyDown("Right")&&MasterUI.disableInput!=2){	PressRight();		}		
			if(cInput.GetKeyDown("Left")&&MasterUI.disableInput!=2){	PressLeft();		}
			
			if(Input.GetKeyDown(KeyCode.Tab)){
				UIMenuItem gotItem = FindNextItemRight();
				if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){
					gotItem = FindNextItemLeft();
				}
				UITextInput gotInput = gotItem.gameObject.GetComponent<UITextInput>();
				if(gotInput!=null){
					gotItem.Press();
				}else{
					SetSelectedItem(gotItem,1);
				}
			}
			
			if(cInput.GetKeyDown("Enter")){//||(cInput.GetKeyUp("Use1")&&MasterUI.disableInput==1)){
				PressEnter();
			}
		}
		if(callbackUpdate!=null){callbackUpdate();}
	}
	
	public void OnEnable(){
		if(callbackEnable!=null){
			callbackEnable(gameObject);
		}
	}
	
	
	public void PressUp(){
		if(selectedItem!=null&&selectedItem.callbackUpSelected!=null){if(selectedItem.callbackUpSelected()<1)return;}
		UIMenuItem gotItem = FindNextItemUp();
		SetSelectedItem(gotItem,1);
	}
	
	public void PressDown(){
		if(selectedItem!=null&&selectedItem.callbackDownSelected!=null){if(selectedItem.callbackDownSelected()<1)return;}
		UIMenuItem gotItem = FindNextItemDown();
		SetSelectedItem(gotItem,1);
	}	
	
	public void PressRight(){
		UIMenuItem gotItem = FindNextItemRight();
		SetSelectedItem(gotItem,1);
	}	
	
	public void PressLeft(){
		UIMenuItem gotItem = FindNextItemLeft();
		SetSelectedItem(gotItem,1);
	}
	
	public void PressEnter(){
		if(callbackEnter!=null){if(callbackEnter(gameObject)<1)return;};
		if(selectedItem!=null){
			selectedItem.Press();
		}
	}
	
	public UIMenuItem FindNextItemUp(){
		float selectedX=0f;
		float selectedY=0f;
		if(selectedItem!=null){
			selectedX=selectedItem.gridX; selectedY=selectedItem.gridY;
		}
		UIMenuItem closeItem=default(UIMenuItem);//find closest selectable item with greater y
		UIMenuItem topItem=default(UIMenuItem);//find top most item
		UIMenuItem bottomItem=default(UIMenuItem);//find bottom most item
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.selected==-2){continue;}
			if(closeItem==null){
				bottomItem=mitem;	topItem=mitem;	closeItem=mitem;
				continue;
			}
			if(topItem.gridY>mitem.gridY){	topItem=mitem;		}
			if(bottomItem.gridY<mitem.gridY){		bottomItem=mitem;		}
			if(mitem!=selectedItem&&mitem.gridY<selectedY&&(mitem.gridY>=closeItem.gridY||closeItem.gridY>=selectedY)){
			//if(mitem.gridY<selectedY&&mitem.gridY>=closeItem.gridY){
				if(selectedItem!=null&&(selectedItem.IsItemSameColumn(mitem)<selectedItem.IsItemSameColumn(closeItem))){ 
					continue;
				}
				closeItem=mitem;

			}
		}
		if(bottomItem!=null&&(topItem.gridY==selectedY||closeItem==selectedItem)){closeItem=bottomItem;}
		return closeItem;		
	}
	public UIMenuItem FindNextItemDown(){
		float selectedX=0f;
		float selectedY=-1f;
		if(selectedItem!=null){
			selectedX=selectedItem.gridX; selectedY=selectedItem.gridY;
		}
		UIMenuItem closeItem=default(UIMenuItem);//find closest selectable item with greater y
		UIMenuItem topItem=default(UIMenuItem);//find top most item
		UIMenuItem bottomItem=default(UIMenuItem);//find bottom most item
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.selected==-2){continue;}
			if(closeItem==null){
				bottomItem=mitem;	topItem=mitem;	closeItem=mitem;
				continue;
			}
			if(topItem.gridY>mitem.gridY){	topItem=mitem;		}
			if(bottomItem.gridY<mitem.gridY){bottomItem=mitem;		}
			if(selectedY<mitem.gridY&&(mitem.gridY<=closeItem.gridY||closeItem.gridY<=selectedY)){
				if(selectedItem!=null&&(selectedItem.IsItemSameColumn(mitem)<selectedItem.IsItemSameColumn(closeItem))){ 
					continue;
				}
				closeItem=mitem;
			}
		}
		if(bottomItem!=null&&bottomItem.gridY==selectedY){closeItem=topItem;}
		return closeItem;		
	}
	
	public UIMenuItem FindNextItemRight(){
		float selectedX=0f;
		float selectedY=0f;
		if(selectedItem!=null){
			selectedX=selectedItem.gridX; selectedY=selectedItem.gridY;
		}else{
			selectedItem=new UIMenuItem();
		}
		UIMenuItem closeItem=default(UIMenuItem);//find closest selectable item with greater y
		UIMenuItem leftItem=default(UIMenuItem);//find top most item
		UIMenuItem rightItem=default(UIMenuItem);//find bottom most item
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.selected<0||mitem.IsItemSameRow(selectedItem)<1){continue;}
			if(closeItem==null){
				rightItem=mitem;	leftItem=mitem;	closeItem=mitem;
				continue;
			}
			if(leftItem.gridX>mitem.gridX){	leftItem=mitem;		}
			if(rightItem.gridX<mitem.gridX){rightItem=mitem;		}
			if(mitem==selectedItem)continue;
			if(selectedX<mitem.gridX&&(mitem.gridX<closeItem.gridX||closeItem.gridX<=selectedX)){
				closeItem=mitem;
			}
		}
		if(rightItem!=null&&rightItem.gridX==selectedX&&leftItem!=rightItem){closeItem=leftItem;}
		if(closeItem==null||closeItem==selectedItem){closeItem=FindNextItemDown();}
		return closeItem;		
	}	
	
	public UIMenuItem FindNextItemLeft(){
		float selectedX=0f;
		float selectedY=0f;
		if(selectedItem!=null){
			selectedX=selectedItem.gridX; selectedY=selectedItem.gridY;
		}else{
			selectedItem=new UIMenuItem();
		}
		UIMenuItem closeItem=default(UIMenuItem);//find closest selectable item with greater y
		UIMenuItem leftItem=default(UIMenuItem);//find top most item
		UIMenuItem rightItem=default(UIMenuItem);//find bottom most item
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.selected<0||mitem.IsItemSameRow(selectedItem)<1){continue;}
			if(closeItem==null){
				rightItem=mitem;	leftItem=mitem;	closeItem=mitem;
				continue;
			}
			if(leftItem.gridX>mitem.gridX){	leftItem=mitem;		}
			if(rightItem.gridX<mitem.gridX){rightItem=mitem;		}
			if(mitem==selectedItem)continue;
			if(selectedX>mitem.gridX&&(mitem.gridX>closeItem.gridX||closeItem.gridX>=selectedX)){
				closeItem=mitem;
			}
		}
		if(rightItem!=null&&leftItem.gridX==selectedX&&leftItem!=rightItem){closeItem=rightItem;}
		if(closeItem==null||closeItem==selectedItem){closeItem=FindNextItemUp();}
		return closeItem;		
	}
	
	public void CleanupItems(){
		
	}
	static public void CleanupFrames(){
		openFrames.RemoveAll(item => item == null);
		
	}
	
	public void SetSelectedItem(UIMenuItem inc_item,int inc_selected=1){
		if(inc_selected>0){
			if(selectedItem!=inc_item){
				if(MasterUI.audioClips.ContainsKey(inc_item.audioSelect)){
					MasterAudio.Play(MasterUI.audioClips[inc_item.audioSelect],.15f,1.2f);
				}
			}
			selectedItem=inc_item;
		}
		if(inc_item==null)return;
		inc_item.SetSelected(inc_selected);//
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.selected>0&&mitem!=inc_item){
				mitem.SetSelected(0);
			}
		}
		
	}
	
	public void SetActive(int inc_active){ //'top frame'
		active=inc_active;
		if(inc_active==0){
			gameObject.SetActive(false);
		}
		if(inc_active==1){
			if(!gameObject.active)gameObject.SetActive(true);
		}
		
	}
	
	public void Close(){
		openFrames.Remove(this);
		if(gameObject!=null){
			if(MasterUI.audioClips.ContainsKey(audioClose)){
				MasterAudio.Play(MasterUI.audioClips[audioClose],.04f,1.1f);
			}
			Destroy(gameObject);
		}


		RefreshFrames();
	}
	
	static public void CloseTopFrame(){
		if(openFrames.Count==0){return;}
		openFrames[openFrames.Count-1].Close();
		
	}
	
	static public void CloseFrames(){
		List<UIMenuFrame> openFramesTemp=new List<UIMenuFrame>(openFrames);
		foreach(UIMenuFrame frame in openFramesTemp){
			frame.Close();
		}

		MenuHelpKeys.OpenNoob();//TODO ON CLOSEFRAMES CALLBACKS.
		
		MasterUI.DisableInput(0);
		//openFrames.Clear();
	}
	
	static public UIMenuFrame GetTopFrame(){
		if(openFrames.Count==0){return default(UIMenuFrame);}
		return openFrames[openFrames.Count-1];
		
	}
	
	static public void RefreshFrames(){
		CleanupFrames();
		if(openFrames.Count<1){
			CloseFrames();
			return;
		}
		int li=0;
		foreach(UIMenuFrame fram in new List<UIMenuFrame>(openFrames)){li++;
			fram.layer=li;
			if(li==openFrames.Count){fram.SetActive(1);}
			else{fram.SetActive(0);}
			fram.Refresh();
		}

	}
	
		
	public void RefreshItems(){
		foreach(UIMenuItem mitem in menuItems){
			mitem.Refresh();	
		}
	}
	public void Refresh(bool inc_refresh_items=true){
		RefreshPosition();
		if(bgOb==null){CreateBgOb();}
		bgOb.transform.localScale = new Vector3(gridSizeX*gridWidth*.1f,gridSizeY*gridHeight*.1f,1f);
		bgOb.transform.localPosition=new Vector3(gridSizeX*gridWidth*.05f,-gridSizeY*gridHeight*.05f,.1f);
		if(inc_refresh_items){
			RefreshItems();
		}
	}
	
	public void RefreshPosition(){
		transform.localPosition=new Vector3(
			Mathf.Floor((MasterUI.GetUIWidth()*alignX-(gridSizeX*gridWidth*alignX))+offsetX)*.1f,
			Mathf.Floor((MasterUI.GetUIHeight()*alignY+(gridSizeY*gridHeight*alignY))+offsetY)*.1f,
			-layer*.5f
		);
	}
	public void OnScreenResize(float inc_float){
		RefreshPosition();
	}

	
	public void SetBgColor(Color inc_color){
		bgColor=inc_color;
		CreateBgOb();
		Refresh();
		
	}
	
	public void CreateBgOb(){
		if(bgOb!=null){Destroy(bgOb);}
		bgOb=new GameObject(gameObject.name+"_bgob");
		bgOb.layer=LayerMask.NameToLayer("UI");
		bgOb.transform.parent=gameObject.transform;
		bgOb.Localize();
		MeshRenderer bgRend = bgOb.AddComponent<MeshRenderer>();
		MeshFilter bgFilt = bgOb.AddComponent<MeshFilter>();
		bgFilt.mesh=MeshGenPlane.CreatePlane();
		bgRend.material=MaterialHelper.CreateMaterialTransparent();
		bgRend.material.SetColor("_Color", bgColor);
	}
	
	
	public float GetExtentX(){ //returns the highest gridX from contained menuitems stf
		float gotx=0;
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.gridX+mitem.gridWidth>gotx){gotx=mitem.gridX+mitem.gridWidth;}
		}
		return gotx;
	}	
	public float GetMaxY(){ //returns the highest gridY from contained menuitems stf
		float goty=0;
		foreach(UIMenuItem mitem in menuItems){
			if(mitem.gridY>goty){goty=mitem.gridY;}
		}
		return goty;
	}
	
	
	
	public void AddItem(UIMenuItem inc_mitem){//adds menu item to this frame
		inc_mitem.menuFrame=this;
		if(!menuItems.Contains(inc_mitem)){
			menuItems.Add(inc_mitem);
		}
		inc_mitem.Refresh();
	}
	
	
	public void AddItemBottom(UIMenuItem inc_mitem){//adds menu item to bottom and grows out
		if(gridSizeX==-1f){gridSizeX=1f;}
		if(gridSizeY==-1f){gridSizeY=0f;}
		inc_mitem.gridY=gridSizeY;
		if(inc_mitem.gridHeight>0f){
			gridSizeY+=inc_mitem.gridHeight;
			
		}
		inc_mitem.SetMenuFrame(this);
		
		Refresh(false);
		
	}
	
	
	public UIMenuItem CreateMenuItem(int add_to=1){//1 adds to bottom;
		GameObject newOb=new GameObject(gameObject.name+"_mitem");
		UIMenuItem newItem= newOb.AddComponent<UIMenuItem>();
		if(add_to==1){AddItemBottom(newItem);}
		if(add_to==0){AddItem(newItem);}
		return newItem;
	}
	
	
	static public UIMenuFrame CreateFrame(string inc_name="unnamed",int managed=1){
		//Debug.Log("create frame");
		MasterUI.DisableInput(1);
		GameObject newob = new GameObject("mframe_"+inc_name);
		UIMenuFrame mframe = newob.AddComponent<UIMenuFrame>();
		if(managed>0){
			openFrames.Add(mframe);
			mframe.layer=openFrames.Count;
			RefreshFrames(); 
		}else{ //non-managed... will not be closed by escape or hidden by other menus
			mframe.layer=-1;
		}
		
		return mframe;			
	}
	
	
	
	
	
}
