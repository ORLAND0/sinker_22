using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//testing
//    IBeginDragHandler    ICancelHandler    IDeselectHandler    IDragHandler    IDropHandler    IEndDragHandler  
//IEventSystemHandler    IInitializePotentialDragHandler    IMoveHandler IPointerClickHandler    IPointerDownHandler    IPointerEnterHandler    IPointerExitHandler   IPointerUpHandler    IScrollHandler    ISelectHandler    ISubmitHandler    IUpdateSelectedHandler
public class UIMenuItem : MonoBehaviour, IPointerEnterHandler,IPointerUpHandler, IPointerClickHandler,IPointerDownHandler,IPointerExitHandler{
	
	public UIMenuFrame menuFrame;
	
	public float gridX=0f;//POSITION
	public float gridY=0f;//(top 0 down)
	public float gridWidth=1f;//multiplies with menuFrame.gridwidth
	public float gridHeight=1f;
	public int selected=-1;//-2 not selectable, -1 init, 0 static, 1 hover, 2 down,
	public int selectedPrev=-1;
	public int pressed=-1;//-1 init 0 not pressed wtf
	public int hovered=-1;//
	public int hoverFrames=-1;
	public int firstFrames=0;
	public DelegateObject callbackPress;//runs on PointerClick
	public DelegateObject callbackPointerExit;//runs on PointerClick
	public DelegateString callbackSelect;//runs on PointerClick
	public DelegateString callbackUnselect;//runs on PointerClick
	public IntDelegateGameObject callbackDownSelected;//return >0 to supress menu pressdown
	public IntDelegateGameObject callbackUpSelected;//return >0 to supress menu pressdown
	//public DelegateString callbackPointerUp;public DelegateString callbackPointerDown;	public DelegateString callbackPointerEnter;public DelegateString callbackPointerExit;
	
	public int createBg=-1;//-1 defaults to on if selectable;0 dont create, 1 creates;
	public float bgPadding=8f;
	public Color[] bgColors=new Color[3]{new Color(.23f,.23f,.23f,.55f),new Color(.33f,.33f,.33f,.75f),new Color(.0f,.0f,.0f,.88f)};
	public GameObject bgOb;
	public GameObject contentOb;
	public Transform contentTrans;
	
	
	public string text;
	public TextAnchor textAnchor=TextAnchor.MiddleCenter;
	public int textOverflow=-1;//-1 default->0;0 center;1 todo:hide;2 move up
	public TextGenerator textGenerator;
	public GameObject textOb;
	public Color textColor=Color.white;
	public float textHeight=0f;
	public UITooltipHover tooltip;
	public string tooltipText="";
	public string audioSelect="menu_roll"; //played from UIMenuFrame.SetSelected
	public string audioPress="menu_press"; 
	
	void Awake(){
		gameObject.layer=MasterGame.phys_layers["ui"];
		transform.parent=MasterUI.menuFrameTrans;
		gameObject.Localize();
		if(contentOb==null){
			contentOb=new GameObject(gameObject.name+"_cont");
			contentTrans=contentOb.transform;
			contentTrans.parent=transform;
			contentOb.Localize();
		}
	}
	
	// Use this for initialization
	void Start () {
		if(selected==-1){selected=0;}
		Refresh();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		//if(hovered>0){
		//	hoverFrames++;
		//}else{
		//	if(hoverFrames>0){hoverFrames=0;}
		//	else{hoverFrames+=-1;}		
		//}
		//hovered=0;
		if(firstFrames<1)firstFrames++;
		selectedPrev=selected;
	}
	
	public void Refresh(){
		//if(tooltip==null){tooltip=gameObject.GetComponent<UITooltipHover>();}
		if(gridWidth==-1f){gridWidth=1f;}
		if(gridHeight==-1f){gridHeight=1f;}
		
		
		if(menuFrame!=null&&transform.parent!=menuFrame.itemsTrans.transform){
			transform.parent=menuFrame.itemsTrans.transform;
			gameObject.Localize();
		}
		RefreshPosition();
		SetText(text);
		RefreshBg();
		
	}
	
	public void RefreshPosition(){
		transform.localPosition= new Vector3(Mathf.Round(gridX*menuFrame.gridWidth)*.1f,Mathf.Round(-gridY*menuFrame.gridHeight)*.1f,-.1f);
	}
	
	
	public void OnEnable(){
		firstFrames=-1;
	}
	

	public void OnPointerClick(PointerEventData data){//on up
		if(firstFrames>0){
		Press();
		hovered=1;
		}
	}
	public void OnPointerUp(PointerEventData data){
		if(selected>-1){
			//menuFrame.SetSelectedItem(this,1);
		}
		hovered=1;
		pressed=0;
	}
	public void OnPointerDown(PointerEventData data){
		MasterUI.OverlayClick(this);
		pressed=1;
		if(selected>-1){
			menuFrame.SetSelectedItem(this,2);
		}
		hovered=1;
	}
	
	public void OnPointerEnter(PointerEventData data){		
		if(selected>-1&&selectedPrev==0&&firstFrames>0&&MasterUI.disableInput!=2){
			menuFrame.SetSelectedItem(this,1);
		}
		hovered=1;
		
	}	
	public void OnPointerExit(PointerEventData data){
		//if(pressed==1){SetSelected(1);pressed=-2;}
		hovered=0;
		//Refresh();
		if(callbackPointerExit!=null)callbackPointerExit(data);	
	}
	
	public void Press(){//press this item
		if(selected>-1){
			menuFrame.SetSelectedItem(this,2);
		}
		if(MasterUI.audioClips.ContainsKey(audioPress)){
			MasterAudio.Play(MasterUI.audioClips[audioPress],.2f,1.4f);
		}
		if(callbackPress!=null)callbackPress("");	
	}
	
	public void AddToBottom(){
		menuFrame.AddItemBottom(this);
	}

	public void SetMenuFrame(UIMenuFrame inc_menu_frame){
		inc_menu_frame.AddItem(this);		
		//menuFrame=inc_menu_frame;
		//menuFrame.AddItem(this);
		//Refresh();
	}
	
	
	
	public void AddContent(GameObject inc_ob){
		inc_ob.transform.parent=contentTrans;
		inc_ob.SetLayerChildren(MasterGame.GetLayer("ui"));
		inc_ob.Localize();
	}
	
	public void SetTextMesh(string inc_text){
		text=inc_text;
		//if(textOb!=null){Destroy(textOb);}
		//if(inc_text=="")return;
		if(textOb==null){
			textOb=new GameObject(gameObject.name+"txt");
			textOb.layer=MasterGame.GetLayer("ui");
			TextMesh newButText=textOb.AddComponent<TextMesh>();
			textOb.transform.parent=contentTrans;
			textOb.Localize();
		}
		textOb.transform.localPosition=new Vector3(gridWidth*menuFrame.gridWidth*.05f,gridHeight*menuFrame.gridHeight*-.05f,0f);
		TextMesh textMesh=textOb.GetComponent<TextMesh>();
		MeshRenderer newButRenderer=	textMesh.GetComponent<MeshRenderer>();
		textMesh.font=MasterUI.GetDefaultFont();
		textMesh.fontSize=MasterUI.GetBaseFontSize();
		newButRenderer.sharedMaterial =  MasterUI.GetDefaultFontMaterial();
		textMesh.text=inc_text;
		textMesh.alignment=TextAlignment.Center;
		textMesh.anchor=TextAnchor.MiddleCenter;
		SetTextColor(textColor);
	}
	
	public void SetTextAnchor(TextAnchor inc_anchor){
		textAnchor=inc_anchor;
		if(textOb!=null){SetText(text);}
	}
	
	public void SetText(string inc_text){
		text=inc_text;
		//if(textOb!=null){Destroy(textOb);}//if(inc_text=="")return;
		if(textOb==null){
			textOb=new GameObject(gameObject.name+"txt");
			textOb.layer=MasterGame.GetLayer("ui");
			textOb.AddComponent<MeshRenderer>();
			textOb.AddComponent<MeshFilter>();
			textOb.transform.parent=contentTrans;
			textOb.Localize();
		}
		
		TextGenerationSettings m_TextSettings = MasterUI.GetTextGenSettings();
		m_TextSettings.textAnchor = textAnchor;//m_TextSettings.textAnchor = TextAnchor.MiddleCenter;
		m_TextSettings.color = Color.white;//textColor;
		m_TextSettings.richText = true;//textColor;
		m_TextSettings.generationExtents = new Vector2(gridWidth*menuFrame.gridWidth-bgPadding*2f, gridHeight*menuFrame.gridHeight-bgPadding*2f);
		m_TextSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
		if(textGenerator==null){textGenerator = new TextGenerator();} 
		textGenerator.Populate(text, m_TextSettings);

		textOb.transform.localPosition=new Vector3(gridWidth*menuFrame.gridWidth*.05f,gridHeight*menuFrame.gridHeight*-.05f,0f);
		textOb.transform.localScale=new Vector3(.1f,.1f,.1f)/MasterUI.GetFontScaleFactor();
		
		MeshFilter thisFilt = textOb.GetComponent<MeshFilter>();
		thisFilt.mesh = textGenerator.GetMesh(thisFilt.mesh);
		MeshRenderer newButRenderer=	textOb.GetComponent<MeshRenderer>();
		newButRenderer.sharedMaterial = MasterUI.GetDefaultFont().material;
		SetTextColor(textColor);
		textHeight=	(newButRenderer.bounds.extents.z/MasterUI.UIScale);
		if(textOverflow==2&&textHeight>1f){
			textOb.transform.localPosition=new Vector3(gridWidth*menuFrame.gridWidth*.05f,textHeight+(gridHeight*menuFrame.gridHeight*-.1f)+.5F,0f);
		}
		
	}
	
	public void SetTextColor(Color inc_color){ //
		textColor=inc_color;
		if(textOb==null)return;
		MeshRenderer textRend=textOb.GetComponent<MeshRenderer>();
		textRend.material.color = inc_color;
	}
	
	public void SetTooltip(string inc_tip){
		tooltipText=inc_tip;
		RefreshBg();
	}
	
	public void SetSelected(int inc_selected=1){//you should probably call the menuframe.SetSelectedItem();
		selected=inc_selected;
		if(inc_selected>0&&menuFrame.selectedItem!=this){
			menuFrame.selectedItem=this;
		}
		if(tooltip!=null){if(inc_selected==1)tooltip.Open();if(inc_selected<1)tooltip.Close();}
		if(inc_selected>0&&selectedPrev==0&&callbackSelect!=null)callbackSelect("");
		if(inc_selected==0&&callbackUnselect!=null)callbackUnselect("");
		
		Refresh();
	}
	
	public Vector3 GetLocalCenter(){//returns the position of the center of this item
		if(menuFrame==null)return Vector3.zero;
		Vector3 pos = new Vector3((gridWidth/2f)*menuFrame.gridWidth*.1f,-(gridHeight/2f)*menuFrame.gridHeight*.1f,0f);
		return pos;
	}
	
	public float GetWidth(){
		return gridWidth*menuFrame.gridWidth;
	}	
	public float GetHeight(){
		return gridHeight*menuFrame.gridHeight;
	}
	
	public int IsItemSameRow(UIMenuItem inc_item){
		if(gridY<inc_item.gridY+inc_item.gridHeight&&gridY>=inc_item.gridY){
			return 1;} //if the top of this item is within that extent
		if(inc_item.gridY<gridY+gridHeight&&inc_item.gridY>=gridY){
			return 1;} //if the top of that item is within this extent
		//if(inc_item.gridY==gridY){	return 1;	}
		return 0;
	}
	
	public int IsItemSameColumn(UIMenuItem inc_item){
		if(gridX<inc_item.gridX+inc_item.gridWidth&&gridX>=inc_item.gridX){
			return 1;} //if the top of this item is within that extent
		if(inc_item.gridX<gridX+gridWidth&&inc_item.gridX>=gridX){
			return 1;} //if the top of that item is within this extent
		//if(inc_item.gridY==gridY){	return 1;	}
		return 0;
	}
	
	public void SetBgColor(Color inc_color){
		if(bgOb==null)return;
		MeshRenderer bgRend = bgOb.GetComponent<MeshRenderer>();
		bgRend.material.SetColor("_Color", inc_color);
	}
		
	public void RefreshBg(){
		if(createBg==0||(selected<-1&&createBg<1&&tooltipText=="")){
			//Debug.Log(text+" selected:"+selected+" create:"+createBg);
			if(bgOb!=null){DestroyImmediate(bgOb);}
			return;
		}
		if(bgOb==null){createBgOb();}
		if(bgOb!=null){
			bgOb.transform.localScale = new Vector3(((menuFrame.gridWidth*gridWidth)-bgPadding)*.1f,(menuFrame.gridHeight*gridHeight-bgPadding)*.1f,1f);
			bgOb.transform.localPosition=new Vector3(menuFrame.gridWidth*gridWidth*.05f,menuFrame.gridHeight*-gridHeight*.05f,.05f);
			if(selected==0){SetBgColor(bgColors[0]);}
			else{
				if(selected==1){	SetBgColor(bgColors[1]);}		
				if(selected>1){	SetBgColor(bgColors[2]);}
			}
			if(tooltipText!=""){
				if(tooltip==null)tooltip=gameObject.AddComponent<UITooltipHover>();
				tooltip.text=tooltipText;
			}
		}		
	}
	
	public void createBgOb(){
		if(bgOb!=null){DestroyImmediate(bgOb);}
		bgOb=new GameObject(gameObject.name+"_mitem_bgob");
		bgOb.layer=MasterGame.GetLayer("ui");
		bgOb.transform.parent=gameObject.transform;
		bgOb.Localize();
		MeshRenderer bgRend = bgOb.AddComponent<MeshRenderer>();
		MeshFilter bgFilt = bgOb.AddComponent<MeshFilter>();
		bgFilt.mesh=MeshGenPlane.CreatePlane();
		bgRend.material=MaterialHelper.CreateMaterialTransparent();
		bgRend.material.SetColor("_Color", bgColors[0]);
		if(selected>-3){
			BoxCollider clickCollider=bgOb.AddComponent<BoxCollider>();
			clickCollider.isTrigger=true;
			clickCollider.size=new Vector3(1f,1f,1f);

		}
	}
	

	
}
