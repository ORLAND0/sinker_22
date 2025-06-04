using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UITextInput : MonoBehaviour {
	
	public TextMesh textMesh;
	public string placeholder="";
	public TextAnchor textAnchor=TextAnchor.MiddleCenter;
	public string inputText="";//internal actuall text value entered
	public int characterMode=1;//1 text; 2 password;3 numeric only;
	[HideInInspector]
	public string textDisplay="";//sent to gen -> mesh
	public int carratIndex=-1;
	public int carratIndexPrev=-1;
	public int selectIndex=-1;//index of selecting (from this to Carrat); -1 not selecting;
	public int selectIndexTo=-1;//index of selecting (from this to Carrat); -1 not selecting;
	public string carratChar ="|";
	public int carratEnabled=-1;
	public double carratShiftPrev=0;
	public Vector2 carratSize= new Vector2(.18f,1f);
	public GameObject carratOb;
	[HideInInspector]
	public double lastCarratTime;
	[HideInInspector]
	public UIMenuItem menuItem;
	[HideInInspector]
	public DelegateString callbackInput;
	public DelegateObject callbackEnter;
	public int maxChars=512;
	
	public int focused=-1;
	public int disableInputOrig=-1;
	
	private Dictionary<int,GameObject> linkObs=new Dictionary<int,GameObject>();
	private Dictionary<int,GameObject> selObs=new Dictionary<int,GameObject>();


	void Awake () {

		UIMenuItem gotMenuItem=gameObject.GetComponent<UIMenuItem>();
		if(textMesh==null&&gotMenuItem!=null){
			SetMenuItem(gotMenuItem);
		}

		if(menuItem==null)enabled=false;
		SetText(inputText);
		Update();
	}
	void Start(){
		SetText(inputText);
		Update();
	}
	
	
	public void Focus(){
		if(focused>1){return;}
		focused=1;
		if(carratIndex==-1)carratIndex=inputText.Length;
		disableInputOrig=MasterUI.disableInput;
		MasterUI.disableInput=2;
		EnableLinkObs();
	}
	
	public void Unfocus(){
		focused=0;
		if(MasterUI.disableInput==2){
			MasterUI.disableInput=disableInputOrig;
		}
		DisableLinkObs();
	}
	
	
	void Update () {
		if(focused>0){
			string inputString=Input.inputString;
			if((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)||(MasterUI.isEditor==1&&Input.GetKey(KeyCode.CapsLock)))){
				if(MasterUI.isEditor==1)inputString="";
				if((Input.GetKeyDown(KeyCode.V))){
					if(selectIndex>-1){
						carratIndex=selectIndex;if(carratIndex>selectIndexTo)carratIndex=selectIndexTo;
						DeleteSelection();
					}
					if(inputString.Length<maxChars-inputText.Length)inputString=GUIUtility.systemCopyBuffer;
				} //PASTE
				if(Input.GetKeyDown(KeyCode.C)&&selectIndex>-1){
					 GUIUtility.systemCopyBuffer = GetSelection();
				}//COPY
				if(Input.GetKeyDown(KeyCode.A)){
					selectIndex=0;selectIndexTo=inputText.Length; HighlightSelection();
				}//select all
			}
			foreach(char c in inputString) {
				if (c == "\b"[0]) {// Backspace - Remove character(s)
					if (selectIndex==-1&&carratIndex>0&&inputText.Length>0){
						inputText = inputText.Substring(0, carratIndex-1) + inputText.Substring(carratIndex,inputText.Length-carratIndex);
						carratIndex+=-1;
					}
					if(selectIndex>-1){DeleteSelection();}
				} else if (c == "\n"[0] || c == "\r"[0]) {//enter submits entry// "\n" for Mac, "\r" for windows.
					if(callbackEnter!=null){callbackEnter(inputText);}//ON ENTER OR RETURN
				}else if(inputText.Length<maxChars){// Normal text input - just append to the end
					if(characterMode==3&&!(char.IsDigit(c)||c=='.'||c=='-')){continue;}
					if(selectIndex>-1){
						carratIndex=selectIndex;if(carratIndex>selectIndexTo)carratIndex=selectIndexTo;
						DeleteSelection();
					}
					inputText = inputText.Substring(0, carratIndex) + c + inputText.Substring(carratIndex,inputText.Length-carratIndex);
					carratIndex+=1;	
				}
				SetText(inputText); 	ShowCarrat();
			}
			if(Input.GetKeyDown(KeyCode.Delete)){
				if(selectIndex>-1){DeleteSelection();}
				else if(carratIndex<inputText.Length){
					inputText = inputText.Substring(0, carratIndex) + inputText.Substring(carratIndex+1,(inputText.Length-carratIndex)-1);
				}
				SetText(inputText); 	ShowCarrat();
			}
				
			if(inputString.Length==0){//&&carratIndexPrev==carratIndex //check if carrat already moved this frame
				if((cInput.GetKeyDown("Left")||Input.GetKeyDown(KeyCode.LeftArrow))){
					CarratShiftLeft();
					carratShiftPrev=carratShiftPrev+.2;
				}else{
					if(Input.GetKey(KeyCode.LeftArrow)&&carratShiftPrev<MasterTime.realTime-.1){//CHECK FOR HOLD
						CarratShiftLeft();
					}
				}
				if((cInput.GetKeyDown("Right")||Input.GetKeyDown(KeyCode.RightArrow))){
					CarratShiftRight();
					carratShiftPrev=carratShiftPrev+.2;
				}else{
					if(Input.GetKey(KeyCode.RightArrow)&&carratShiftPrev<MasterTime.realTime-.1){//CHECK FOR HOLD
						CarratShiftRight();
					}
				}
			}
			if(carratIndexPrev!=carratIndex){
				if(selectIndex>-1){ //MAKE SELECTION AREA VISUAL
					HighlightSelection();
				}else{
					ClearSelectObs();
				}
				
			}
			
			if(lastCarratTime+.5<MasterTime.gameTime){//carrat flashing
				lastCarratTime=MasterTime.gameTime;
				if(carratEnabled<1){carratEnabled=1;}
				else{carratEnabled=0;}
			}
			if(characterMode==2){textDisplay="";
				for (int i = 0; i < carratIndex; i++)	{textDisplay+="*";}
			}else{
				textDisplay = inputText;
			}
			
			if(carratEnabled==1){
				carratOb.SetActive(true);
				int ci=0;
				foreach(UICharInfo charInfo in menuItem.textGenerator.characters){ci++;
					if(ci>carratIndex){
						carratOb.transform.localPosition=new Vector3(charInfo.cursorPos.x,charInfo.cursorPos.y,0f);
						break;
					}
					if(ci==menuItem.textGenerator.characters.Count){
						carratOb.transform.localPosition=new Vector3(charInfo.cursorPos.x+charInfo.charWidth,charInfo.cursorPos.y,0f);
					}
				}
			}else{
				carratOb.SetActive(false);
			}		
			/*
			if(characterMode==2){textDisplay="";
				for (int i = 0; i < carratIndex; i++)	{textDisplay+="*";}
			}else{	textDisplay = inputText.Substring(0, carratIndex);		}
			if(carratEnabled>0){	textDisplay +=carratChar ;		}
			if(characterMode==2){for (int i = 0; i < inputText.Length-carratIndex; i++)	{textDisplay+="*";}
			}else{textDisplay += inputText.Substring(carratIndex,inputText.Length-carratIndex);}
			*/
			if(cInput.GetKeyDown("Escape")){
				Unfocus();
				if(menuItem)menuItem.SetSelected(1);
			}
		}else{//end if focused
			if(inputText==""&&placeholder!=""){
				textDisplay=placeholder;
			}else{
				if(characterMode==2){
					textDisplay="";
					for (int i = 0; i < inputText.Length; i++)	{textDisplay+="*";}
				}else{
					if(menuItem.text!=inputText){textDisplay=inputText;}
				}
			}
			carratOb.SetActive(false);
		}
		if(menuItem.text!=textDisplay){
			menuItem.SetText(textDisplay);
			ClearLinkObs();
			CreateLinkObs();
		}
	}
	public void LateUpdate(){
		carratIndexPrev=carratIndex;
	}
	
	public void CarratShiftRight(){
		if(carratIndex<inputText.Length){
			carratShiftPrev=MasterTime.realTime;
			carratIndex+=1;ShowCarrat();
			if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){selectIndexTo=carratIndex;if(selectIndex==-1){selectIndex=carratIndexPrev;}}
			else{selectIndex=-1;selectIndexTo=-1;}
		}
	}
	public void CarratShiftLeft(){
		if(carratIndex>0){
			carratShiftPrev=MasterTime.realTime;
			carratIndex+=-1;ShowCarrat();
			if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){selectIndexTo=carratIndex;if(selectIndex==-1){selectIndex=carratIndexPrev;}}
			else{selectIndex=-1;selectIndexTo=-1;}
		}
	}
	
	public string GetSelection(){
		if(selectIndex==-1)return "";
		int delStart=selectIndex;
		int delFin=selectIndexTo;
		if(delStart>delFin){delStart=delFin;delFin=selectIndex;}
		return inputText.Substring(delStart, delFin);
	}
	
	public void DeleteSelection(){
		if(selectIndex>-1){
			int delStart=selectIndex;
			int delFin=selectIndexTo;
			if(delStart>delFin){delStart=delFin;delFin=selectIndex;}
			inputText = inputText.Substring(0, delStart) + inputText.Substring(delFin,inputText.Length-delFin);
			carratIndex=delStart;
			if(carratIndex<0)carratIndex=0;
			if(carratIndex>inputText.Length)carratIndex=inputText.Length;
		}
		ClearSelection();
		
	}
	public void HighlightSelection(){ 
		ClearSelectObs(); int ci=0;
		foreach(UICharInfo charInfo in menuItem.textGenerator.characters){ci++;int cci=ci-1;
			if(linkObs.ContainsKey(cci)&&((cci>=selectIndex&&cci<selectIndexTo)||(cci<selectIndex&&cci>=selectIndexTo))){
				UICharInfo thisCharInfo=charInfo;
				selObs[cci]=(GameObject) Instantiate(MasterUI.icon_dict["cube"]);
				selObs[cci].layer=MasterGame.phys_layers["ui"];
				selObs[cci].Localize(linkObs[cci]);
				if(!CharWidthValid(thisCharInfo.charWidth)){thisCharInfo.charWidth=10f;}
				selObs[cci].transform.localScale=new Vector3(thisCharInfo.charWidth,1.2f/menuItem.textOb.transform.localScale.y,.1f);
				//selObs[ci].transform.localPosition=new Vector3(0f,0f,15f);
			}
		}
	}
	public void ClearSelection(){
		selectIndex=-1;selectIndexTo=-1;ClearSelectObs();
	}
	public void ClearSelectObs(){
		List<GameObject> selObsValues = new List<GameObject>(); 
		selObsValues.AddRange(selObs.Values);
		foreach(GameObject gob in selObsValues){Destroy(gob);}
	}
	
	public void CreateLinkObs(){
			if(placeholder==textDisplay)return;//dont make buttons for placeholder
			int ci=0; //iterate through chars and create clickboxes
			UICharInfo thisCharInfo=new UICharInfo();
			foreach(UICharInfo charInfo in menuItem.textGenerator.characters){int cci=ci;ci++;
				if(charInfo.charWidth==0f){continue;} //skip this character (it doesnt take space)
				thisCharInfo=charInfo;	
				GameObject newLinkOb=new GameObject("UITextInput_charbut");
				linkObs[cci]=newLinkOb;
				newLinkOb.Localize(menuItem.textOb.transform); newLinkOb.layer=MasterGame.phys_layers["ui"];
				if(!CharWidthValid(thisCharInfo.charWidth)){thisCharInfo.charWidth=10f;}
				newLinkOb.transform.localPosition=new Vector3(thisCharInfo.cursorPos.x+thisCharInfo.charWidth/2f,thisCharInfo.cursorPos.y-1.15f*(MasterConsole.GetRowHeight()*MasterUI.GetFontScaleFactor()/3f),-10f);
				BoxCollider boxCollider = newLinkOb.AddComponent<BoxCollider>();
				boxCollider.size=new Vector3(thisCharInfo.charWidth,1.3f*(MasterConsole.GetRowHeight()*MasterUI.GetFontScaleFactor()/2f),12f);
				boxCollider.isTrigger=true;
				UITooltipHover tooltipHover = newLinkOb.AddComponent<UITooltipHover>();
				double prevClickTime=0;//double click timer// do you need this?
				tooltipHover.callbackPointerDown=inc_ob=>{ //click 
					if(focused<1){menuItem.Press();}
					if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){selectIndexTo=cci;if(selectIndex==-1){selectIndex=carratIndex;}}
					else{selectIndex=-1;selectIndexTo=-1;ClearSelectObs();}
					carratIndex=cci;
					MasterUI.OverlayClick(this);
					//if(prevClickTime+MasterUI.doubleClickThresh>MasterTime.realTime){ prevClickTime=0;	}else{prevClickTime=MasterTime.realTime;}
				};
				tooltipHover.callbackPointerEnter=inc_ob=>{ //click and hold
					if(Input.touchCount > 0||Input.GetKey(KeyCode.Mouse0)){
						selectIndexTo=cci;
						if(selectIndex==-1){selectIndex=carratIndex;}
						carratIndex=cci;
					}
					else if(Input.GetKey(KeyCode.Mouse1)){tooltipHover.callbackPointerDown(tooltipHover.gameObject);}
				};
			}
			for(int lasi=0;ci<=menuItem.textGenerator.characters.Count;lasi++){int cci=ci;ci++;
				GameObject newLinkOb=new GameObject("UITextInput_charbutlast");
				linkObs[cci]=newLinkOb;
				newLinkOb.Localize(menuItem.textOb.transform); newLinkOb.layer=MasterGame.phys_layers["ui"];
				if(!CharWidthValid(thisCharInfo.charWidth)){thisCharInfo.charWidth=10f;}
				newLinkOb.transform.localPosition=new Vector3(thisCharInfo.cursorPos.x+thisCharInfo.charWidth*1.5f,thisCharInfo.cursorPos.y-1.15f*(MasterConsole.GetRowHeight()*MasterUI.GetFontScaleFactor()/3f),-10f);
				BoxCollider boxCollider = newLinkOb.AddComponent<BoxCollider>();
				boxCollider.size=new Vector3(thisCharInfo.charWidth,1.3f*(MasterConsole.GetRowHeight()*MasterUI.GetFontScaleFactor()/2f),12f);
				boxCollider.isTrigger=true;
				UITooltipHover tooltipHover = newLinkOb.AddComponent<UITooltipHover>();
				double prevClickTime=0;//double click timer// do you need this?
				tooltipHover.callbackPointerDown=inc_ob=>{ //click
					if(focused<1){menuItem.Press();}
					if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){selectIndexTo=cci-1;if(selectIndex==-1){selectIndex=carratIndex;}}
					else{selectIndex=-1;selectIndexTo=-1;ClearSelectObs();}
					carratIndex=cci-1;
					MasterUI.OverlayClick(this);
				};
				tooltipHover.callbackPointerEnter=inc_ob=>{ //click and hold
					if(Input.touchCount > 0||Input.GetKey(KeyCode.Mouse0)){
						selectIndexTo=cci-1;
						if(selectIndex==-1){selectIndex=carratIndex;}
						carratIndex=cci-1;
					}
					else if(Input.GetKey(KeyCode.Mouse1)){tooltipHover.callbackPointerDown(tooltipHover.gameObject);}
				};
			}//1 extra button after last chars
			if(focused<1){DisableLinkObs();}
	}
	public void EnableLinkObs(){
		foreach(KeyValuePair<int,GameObject> entry in linkObs){if(entry.Value!=null)entry.Value.SetActive(true);}
	}
	public void DisableLinkObs(){
		foreach(KeyValuePair<int,GameObject> entry in linkObs){if(entry.Value!=null)entry.Value.SetActive(false);}
	}	
	
	public void ClearLinkObs(){
		List<GameObject> linkObsValues = new List<GameObject>(); 
		linkObsValues.AddRange(linkObs.Values);
		foreach(GameObject gob in linkObsValues){Destroy(gob);}
	}
	
	public void ShowCarrat(){
		carratEnabled=1;
		lastCarratTime=MasterTime.gameTime;
	}
	
	public bool CharWidthValid(float inc_width){
		if(float.IsNaN(inc_width)||float.IsInfinity(inc_width))return false;
		if(inc_width>2220f)return false;
		if(inc_width>0f&&inc_width<.01f)return false;
		return true;
	}
	
	public void SetMenuItem(UIMenuItem inc_item){
		menuItem=inc_item;
		if(textMesh==null){
			if(menuItem.textOb==null){	menuItem.SetText(placeholder);}
			textMesh= menuItem.textOb.GetComponent<TextMesh>();
		}
		if(menuItem.createBg==1){
			menuItem.bgColors[0]=new Color(.01f,.01f,.01f,.33f);
		}
		
		if(menuItem.callbackPress==null){
			menuItem.callbackPress=inc_str => {	
				if(focused<1){
					Focus();
				}else{
					Unfocus();
					if(menuItem!=null){
						menuItem.SetSelected(1);
					}
				}				
			};
		}
		if(menuItem.callbackUnselect==null){
			DelegateString unSelCal= inc_str => {	Unfocus();	};
			menuItem.callbackUnselect=unSelCal;
		}
		menuItem.Refresh();
		
		if(carratOb==null){
			carratOb=new GameObject(gameObject.name+"_carrat");
			carratOb.AddComponent<MeshRenderer>();
			carratOb.AddComponent<MeshFilter>();
			carratOb.layer = MasterGame.GetLayer("ui");
		}
		carratOb.transform.parent=menuItem.textOb.transform;
		carratOb.Localize();
		
		TextGenerationSettings m_TextSettings = MasterUI.GetTextGenSettings();
		m_TextSettings.textAnchor=TextAnchor.UpperCenter;
		m_TextSettings.color = menuItem.textColor;
		TextGenerator textGenerator = new TextGenerator();
		textGenerator.Populate(carratChar, m_TextSettings);
		MeshRenderer carRenderer= carratOb.GetComponent<MeshRenderer>();
		carRenderer.sharedMaterial = MasterUI.GetDefaultFont().material;
		carRenderer.material.color=menuItem.textColor;
		MeshFilter carFilt= carratOb.GetComponent<MeshFilter>();
		carFilt.mesh = textGenerator.GetMesh(carFilt.mesh);
		if(menuItem.selected>1){
			Focus();
		}
		
	}
	
	public void SetText(string inc_text){ //if this is done before start, mesh doesnt get updated until 
		inputText=inc_text;
		if(carratIndex>inputText.Length)carratIndex=inputText.Length;
		ClearLinkObs();
		if(callbackInput!=null)callbackInput(inputText);

	}
	

}

