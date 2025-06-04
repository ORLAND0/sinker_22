using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlyingText3D;
public class MasterUI : MonoBehaviour {
	public static MasterUI thisScript;
	public static List<GameObject> UIObs =new List<GameObject>();
	public static List<MonoBehaviour> UIScripts =new List<MonoBehaviour>();

	public static GameObject UIOb;
	public static GameObject UICamOb;
	public static Camera UICam;
	

	//public static int chatOpen;
	//public static GameObject chatPanel;
	//public static UIInput chatInput;
	public static GameObject versionLabel;
	public static Transform tooltipTrans;
	public static Transform menuFrameTrans;
	public static Transform affectorsTrans;
	public static Transform consoleTrans;

	public static int disableInput;//0, 1 menuframe, 2 chatting
	
	public Vector3 mousePosPrev;//keeping track of mouse location prev farme
	static public double mouseStillTime=0;
	public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
	public static int cursorAim=0;//0 none,  1 held; 2 locked cursor,3 aim pos; 4 aim ob;
	public static Vector3 cursorAimPos=Vector3.zero;
	public static GameObject cursorAimIndicator;
	public static Transform cursorAimTrans;//transform of indicator
	public static Transform cursorAimTarget;
	private double cursorAimHeldDur=0;
	private double cursorAimPrev=0; //realTime that aim button was previously clicked
	
	public static int cursorMove=0;//0 none, 1 held; 2 to cursor; 3 to pos;4 move to ob
	public static int cursorMovable=1;//0 unable to be used, 1 able
	public static Vector3 cursorMovePos=Vector3.zero;
	public static GameObject cursorMoveIndicator;//indicates cursorMovePos
	public static Transform cursorMoveTrans;//indicates cursorMovePos
	public static Transform cursorMoveTarget;
	private double cursorMoveHeldDur=0;
	private double cursorMovePrev=0;

	public static double cursorFixedTimeout=45;
	public static double doubleClickThresh=.35f;
	private double holdClickThres=2;

	public static int UIHidden=0;
	public static float UIScale=1f;
	public static float pixelsw=0f;
	public static float pixelsh=0f;
	public static int pixelsNextFrame=0;//we are doing this excess resizing because unity just doesnt wanna snap to it
    
	public static float minimapWidth=0f;//in percentage of screen
	public static float minimapHeight=0f;//in percentage of screen
	public static float minimapScale=1.5f;//128 pix * this * uiscale
	
	public static Dictionary<string,GameObject> icon_dict=new Dictionary<string,GameObject>();
	public static Dictionary<string,Font> font_dict=new Dictionary<string,Font>();
	public static Dictionary<string,Material> font_mat_dict=new Dictionary<string,Material>();
	public static Dictionary<string,Texture2D> www_texture_dict=new Dictionary<string,Texture2D>();
	public static  Dictionary<string,AudioClip> audioClips = new Dictionary<string,AudioClip>();

	

	
	
	public static int isEditor=0;//1 editor, 2 dev mode
	void Awake(){
		thisScript=this;
		#if UNITY_EDITOR
		  isEditor=1;
		#endif
		if(isEditor>0||Debug.isDebugBuild)Debug.unityLogger.logEnabled = true;
		else{Debug.unityLogger.logEnabled=false;}
		UIOb = GameObject.Find("UI").gameObject;
		tooltipTrans= UIOb.transform.Find("tooltip");
		menuFrameTrans= UIOb.transform.Find("menuframe");
		affectorsTrans= UIOb.transform.Find("affectorsbar");
		consoleTrans= UIOb.transform.Find("consoleframe");
		UICamOb = GameObject.Find("UICamera").gameObject; UICam=UICamOb.GetComponent<Camera>();
		LoadFonts();
		LoadAudios();
	}
	
	public void LoadAudios(){
		audioClips["c"]=(AudioClip) Resources.Load("sounds/ui/chat1"); //c for chat
		audioClips["menu_roll"]=(AudioClip) Resources.Load("sounds/ui/menrollover"); 
		audioClips["menu_press"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_flappybeep1"); 
		audioClips["menu_close"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_flappybeep1_rev"); 
		audioClips["error_reload"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_reloadclick_1");
		audioClips["error_overheat"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_overheatwind1"); 
		audioClips["error_lowenergy"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_flappyerror1"); 
		audioClips["chime1"]=(AudioClip) Resources.Load("sounds/ui/jfxr_chime1"); //chime1
		audioClips["chime2"]=(AudioClip) Resources.Load("sounds/ui/jsfxr_chime2"); //chime2
		audioClips["lap_complete"]=(AudioClip) Resources.Load("sounds/ui/lap_complete");
		audioClips["def_bass1"]=(AudioClip) Resources.Load("sounds/ui/def_bass1");

		
	}
	public void LoadFonts(){
		font_dict["atomics"]=(Font) Resources.Load("text/fonts/sub/atomics");
		font_dict["atomicsc"]=(Font) Resources.Load("text/fonts/sub/atomicsc");
		font_mat_dict["atomicsc"]=font_dict["atomicsc"].material;//(Material) Resources.Load("text/fonts/sub/atomicsc_3Dmat");
		font_dict["quer"]=(Font) Resources.Load("text/fonts/QUER__");
		font_dict["hansel"]=(Font) Resources.Load("text/fonts/HANZEEXR");
		font_dict["quadrangle"]=(Font) Resources.Load("text/fonts/quadrangle");
		font_dict["welbut"]=(Font) Resources.Load("text/fonts/Welbut__");
		font_dict["crux"]=(Font) Resources.Load("text/fonts/coders_crux");
		icon_dict["steam"]=(GameObject) Resources.Load("1prefabs/ui/icon/ico_steam");
		icon_dict["cube"]=(GameObject) Resources.Load("1prefabs/ui/icon/ico_square");
		icon_dict["consoleimg"]=(GameObject) Resources.Load("1prefabs/ui/icon/consoleimg");
		icon_dict["cursor_aim"]=(GameObject) Resources.Load("1prefabs/ui/icon/AimIndicator");
		icon_dict["cursor_move"]=(GameObject) Resources.Load("1prefabs/ui/icon/MoveIndicator");
		icon_dict["keycap"]=(GameObject) Resources.Load("1prefabs/ui/icon/keycap");
		icon_dict["mousebot"]=(GameObject) Resources.Load("1prefabs/ui/icon/mousebot");
		icon_dict["mousebutl"]=(GameObject) Resources.Load("1prefabs/ui/icon/mousebutl");
		icon_dict["mousebutr"]=(GameObject) Resources.Load("1prefabs/ui/icon/mousebutr");
		icon_dict["arrow"]=(GameObject) Resources.Load("1prefabs/ui/icon/arrow");
		icon_dict["arrowrot"]=(GameObject) Resources.Load("1prefabs/ui/icon/arrowrot");
	}
	
	
	void Start() {		
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
		CheckDPIScale();
		ScreenResize();
		CreateCursorIndicators();
		ClickGator gator=GameObject.Find("vlogo").GetComponent<ClickGator>();
		gator.del = inc_ob => {	
			if(UIMenuFrame.openFrames.Count>0){
				UIMenuFrame.CloseFrames();
			}else{
				MenuMain.Open();	
			}
		};
		MenuMain.Open();
		if(MasterSettings.launchTimePrev<1736616000){
			MenuGuide.OpenWelcome();
		}
    }
	
	void Update(){
		if(pixelsNextFrame>1){
			ScreenResize();
			pixelsNextFrame--;
		}
		if(pixelsw != UICam.pixelWidth||pixelsh != UICam.pixelHeight){
			
			pixelsw = UICam.pixelWidth;
			pixelsh  = UICam.pixelHeight;
			ScreenResize();
			pixelsNextFrame+=1;
		}

		CursorAim();
		CursorMove();
		CheckCursorHide(); 

		if(cInput.GetKeyDown("Escape")&&disableInput!=2){
			if(UIMenuFrame.openFrames.Count<1){
				MenuMain.Open();
			}else{
				UIMenuFrame.CloseTopFrame();	
			}
			cGUI._HideGUI();//deprecated
		}
		if (cInput.GetKeyDown("TeamMenu")&&MasterUI.disableInput!=2){
			if(MasterConnect.isConnected){MenuTeam.Toggle();}
			else{MenuMain.Open();}
		}		
		if (cInput.GetKeyDown("ClassMenu")&&MasterUI.disableInput!=2){
			//if(MasterConnect.isConnected){
				MenuClass.Toggle();
			//}else{MenuMain.Open();}
			
		}
		if (cInput.GetKeyDown("Enter")&&UIMenuFrame.openFrames.Count==0&&MasterUI.disableInput==0){
			MenuConsole.Open();
		}	
		if(UIHidden==1){HideUI();}
	}
	public void LateUpdate(){
		cursorMovable=1;	
		if(UIHidden==1){HideUI();}
	}

	public void CursorMove(){
		if(cursorMoveIndicator==null)return;
		if(disableInput<1&&cursorMovable>0&&cInput.GetKey("CursorMove")&&MasterCamera.rotationMode>-3){
			Unit gotUnit = MasterUnits.GetMyUnit();
			if(gotUnit==null)return;
			cursorMovePos=MasterCamera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			cursorMovePos.y=10f;
			if(cInput.GetKeyDown("CursorMove")){
				cursorMoveHeldDur=0;
				if(cursorMove==0&&cursorMovePrev+doubleClickThresh>MasterTime.realTime){
					cursorMovePrev=MasterTime.realTime-doubleClickThresh;
					cursorMove=3;
				}else{
					cursorMove=1;
					cursorMovePrev=MasterTime.realTime;
				}
				
			}else{
				cursorMoveHeldDur+=MasterTime.GetRealDelta();
				//if(cursorMoveHeldDur>holdClickThres){		//cursorMove=2;	//}
			}
			cursorMoveIndicator.transform.position=cursorMovePos;
			cursorMoveIndicator.transform.rotation=gotUnit.GetRotation();
			if(!cursorMoveIndicator.active){cursorMoveIndicator.SetActive(true);}
		}else{
			cursorMoveHeldDur=0;
			if(cursorMove==1||cInput.GetAxisRaw("Vertical")!=0f||cInput.GetAxisRaw("Strafe")!=0f||cursorMovePrev<MasterTime.realTime-cursorFixedTimeout){
				cursorMove=0;
			}
			if(cursorMoveIndicator.active&&cursorMove==0){
				cursorMoveIndicator.SetActive(false);
			}
		}
		
	}
	static public void CursorMoveStop(){
		MasterUI.cursorMove=0;if(!cursorMoveIndicator.active){cursorMoveIndicator.SetActive(false);}
	}
	
	static public void OverlayClick(object inc_ob){
		if(MasterUI.cursorMove==1||MasterUI.cursorMove==2){CursorMoveStop();}
		cursorMovable=0;
	}
	
	public void CursorAim(){
		if(cursorAimIndicator==null)return;
		if(disableInput<1&&cInput.GetKey("CursorAim")&&MasterCamera.rotationMode>-2){ //aim being clicked
			if(cInput.GetKeyDown("CursorAim")){
				cursorAimHeldDur=0;
				if(cursorAim==2&&cursorAimPrev+doubleClickThresh>MasterTime.realTime){//double clicked aim
					cursorAimPrev=MasterTime.realTime-doubleClickThresh;
					cursorAim=3;
					cursorAimTrans.localScale=Vector3.one;
					cursorAimPos=MasterCamera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
					cursorAimPos.y=15f;// RAYCAST TO CHECK FOR TARGET LOCK
					RaycastHit hitUnit=new RaycastHit();
					if (Physics.Raycast (cursorAimPos, Vector3.down, out hitUnit,6f,MasterGame.GetLayerMask("unit"))) {
						Unit gotUnit=hitUnit.collider.transform.GetComponent<Unit>();
						if(gotUnit!=null){//&&gotUnit.GetID()!=MasterUnits.GetMyUnitID()
							cursorAim=4;
							cursorAimTarget=gotUnit.transform;
							cursorAimTrans.localScale=Vector3.one*Mathf.Max(1f,gotUnit.colliderRadius*1.25f);
						}
					}
					cursorAimPos.y=10f;
				}else{ //aim was just single clicked
					cursorAimPrev=MasterTime.realTime;
					if(cursorAim==2){ //clicked while locked on to cursor
						cursorAim=0; 
					}else{
						cursorAim=2; 
					}
					cursorAimTrans.localScale=Vector3.one;
				}
			}else{  //aim is being heled
				cursorAimHeldDur+=MasterTime.GetRealDelta();
				if(cursorAimHeldDur>doubleClickThresh){
					cursorAim=1; //unlock cursor permanent aim
				}
				//if(cursorAimHeldDur>holdClickThres){
					//cursorAim=2;
				//}
			}
		}else{ 		//aim is NOT being clicked
			if(cursorAim==1||cInput.GetAxisRaw("Horizontal")!=0||cursorAimPrev<MasterTime.realTime-cursorFixedTimeout){
				if(cursorAimTarget==null){cursorAim=0;}
				else{Unit gotUnit = cursorAimTarget.GetComponent<Unit>(); if(gotUnit==null||gotUnit.IsControlledByMe()<1)cursorAim=0;}
			}
			cursorAimHeldDur=0f;
			if(cursorAimIndicator.active&&cursorAim==0){
				cursorAimIndicator.SetActive(false);
			}
		}
		
		if(disableInput<1&&(cInput.GetKey("CursorAim")||cursorAim==2)){ //aim locked to cursor
			Unit gotUnit = MasterUnits.GetMyUnit();
			//
				cursorAimPos=MasterCamera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
				cursorAimPos.y=10f;
				cursorAimTrans.position=cursorAimPos;
				if(gotUnit!=null){
					cursorAimTrans.rotation=gotUnit.GetRotation();
				}
				if(cursorAim!=0&&!cursorAimIndicator.active){cursorAimIndicator.SetActive(true);}
			//}else{
				//cursorAim=0;
			//}
		}
		if(cursorAim==4){ //aim at object lock
			if(cursorAimTarget==null||!cursorAimTarget.gameObject.active){ //target gone
				cursorAim=0;
				cursorAimTrans.localScale=Vector3.one;
			}else{ //set aim to target position
				cursorAimPos=cursorAimTarget.position;
				cursorAimTrans.position=cursorAimTarget.position;
				cursorAimTrans.rotation=cursorAimTarget.rotation;
				if(!cursorAimIndicator.active){cursorAimIndicator.SetActive(true);}
			}
		}else{
			if(cursorAimTarget!=null){	cursorAimTarget=default(Transform);}
		}
	}	
	
	public void CheckCursorHide(){
		if(mousePosPrev==Input.mousePosition){
			mouseStillTime+=MasterTime.GetRealDelta();
		}else{
			mouseStillTime=0;
			if(disableInput>0||(cursorAim!=2&&cursorAim!=1)){
				Cursor.visible = true;
			}
		}
		mousePosPrev=Input.mousePosition;
		if(disableInput==0&&(mouseStillTime>6||cursorAim==2||cursorAim==1)){
			if(isEditor!=1)//disabled for debuging//comment this for release
				Cursor.visible = false;
		}
	}
	
	static public void DisableInput (int inc_setting){
		disableInput=inc_setting;
		//if(disableInput==1){cursorAim=0;cursorMove=0;}
	}
	static public void NewPrimary(){
		MasterUI.cursorMove=0;
		MasterUI.cursorAim=0;
	}
	
	static public void CleanUIScripts(){
		List<int> keysToRemove=new List<int>();
		int lpos=-1;
		foreach(MonoBehaviour iOb in UIScripts){lpos++;
			if(iOb==null)keysToRemove.Add(lpos);
		}		
		keysToRemove.Reverse();
		foreach(int iKey in keysToRemove){
			UIScripts.RemoveAt(iKey);
		}
	}
	static public void AddUIScript(MonoBehaviour inc_mono){
		UIScripts.Add(inc_mono);
		
	}
	static public void AddUIOb(GameObject inc_ob){
		UIObs.Add(inc_ob);
	}
	static public void AddUIListener(GameObject inc_ob){AddUIOb(inc_ob);}
	
	static public void CleanUIObs(){
		List<int> keysToRemove=new List<int>();
		int lpos=-1;
		foreach(GameObject iOb in UIObs){lpos++;
			if(iOb==null)keysToRemove.Add(lpos);
		}		
		keysToRemove.Reverse();
		foreach(int iKey in keysToRemove){
			UIObs.RemoveAt(iKey);
		}
	}
	
	static public void SetPlayerColor(){
		CleanUIObs();
		for(int i = 0; i < UIObs.Count; i++){
			GameObject uiob= UIObs[i];
			//bool obActive=uiob.active;
			//if(!obActive){uiob.SetActive(true);}
			uiob.SendMessage("OnTeamSet",MasterPlayers.myTeam,SendMessageOptions.DontRequireReceiver);
			//if(!obActive){uiob.SetActive(false);}
		}
		CleanUIScripts();
		for(int i = 0; i < UIScripts.Count; i++){
			MonoBehaviour uiScript= UIScripts[i];
			MethodInfo theMethod = uiScript.GetType().GetMethod("OnTeamSet");
			if(theMethod!=null){
				theMethod.Invoke(uiScript, new object[1]{MasterPlayers.myTeam});
			}
		}
		
	}
	
	static public int GetBaseFontSize(){
		return 12;
	}
	
	static public float GetFontScaleFactor(){
		return 3f;
	}
	
	
	static public Font GetDefaultFont(){
		return GetFont("atomicsc");
	}	
	static public Material GetDefaultFontMaterial(){
		return font_mat_dict["atomicsc"];
	}
	
	static public Font GetFont(string inc_font_label){
		return font_dict[inc_font_label];
	}
	static public void FlyingTextDefaults(){
		FlyingText.defaultFont = 0;
		FlyingText.anchor = TextAnchor.UpperLeft;
		FlyingText.defaultJustification = Justify.Left;
		FlyingText.colliderType = ColliderType.None;
		FlyingText.verticalLayout = false;
		FlyingText.defaultLetterSpacing = 1f;
	}
	static public void FlyingTextCenter(){
		FlyingText.anchor = TextAnchor.MiddleCenter;
		FlyingText.defaultJustification = Justify.Center;
	}
	
	static public void ScreenResize(){
		UICam.orthographicSize = (UICam.pixelHeight)/20f; //this way .1 in UI space = 1 pixel. (at 1 uiscale) however, when odd number of pixels for height, .05's become the fixed placements. may want to think about checking for odd no of pixels and hiding one if so
		
		RefreshObs();
		UIMinimapCamera.ScreenReize();
	}
	
	static public void RefreshObs(){
		UIMenuFrame.RefreshFrames();
		for(int i = 0; i < UIObs.Count; i++){
			GameObject uiob= UIObs[i];
			if(uiob==null){UIObs.RemoveAt(i);	i--;}
			else{
				uiob.SendMessage("OnScreenResize",0f,SendMessageOptions.DontRequireReceiver);
			}
		}
		
		
	}
	
	public void InitGame(float inc_float){
		UIMenuFrame.CloseFrames();
		CreateCursorIndicators();
	}
	
	static public void ToggleUI(){
		if(UIHidden>0){ShowUI();}
		else{HideUI();}
	}
	static public void ShowUI(){
		UIHidden=0;
		UIOb.SetLayerChildren(MasterGame.GetLayer("ui"));
	}
	static public void HideUI(){
		UIHidden=1;
		UIOb.SetLayerChildren(MasterGame.GetLayer("non"));
	}
	
	static public GameObject GetIcon(string inc_key){
		if(icon_dict.ContainsKey(inc_key)){
			return (GameObject) Instantiate(icon_dict[inc_key]);
			
		}
		return default(GameObject);
	}
	
	public void CreateCursorIndicators(){
		if(cursorAimIndicator!=null){UIObs.Remove(cursorAimIndicator);Destroy(cursorAimIndicator);}
		cursorAimIndicator=(GameObject) Instantiate(icon_dict["cursor_aim"]);
		cursorAimIndicator.SetActive(false);		
		cursorAimTrans=cursorAimIndicator.transform;
		cursorAimTrans.parent=MasterGame.effectsTrans;
		if(cursorMoveIndicator!=null){UIObs.Remove(cursorMoveIndicator);Destroy(cursorMoveIndicator);}
		cursorMoveIndicator=(GameObject) Instantiate(icon_dict["cursor_move"]);
		cursorMoveIndicator.SetActive(false);
		cursorMoveTrans=cursorMoveIndicator.transform;
		cursorMoveTrans.parent=MasterGame.effectsTrans;
		
	}
	
	static public void CheckDPIScale(){ //really should be SETdpiscale
		float curUIScale=UIScale;
		float newScale=1f;
		if(Screen.height>950){	newScale=1.5f;}
		if(Screen.height>1081){	newScale=2f;}
		if(Screen.height>1801){	newScale=3f;}
		
		SetUIScale(newScale);
		

	}
	
	static public void InitUIScale(){
		
		
		
	}
	
	static public float GetUIScale(){
		return UIScale;
	}
	
	static public void SetUIScale(float inc_float){
		if(inc_float<.1f||inc_float>5f){CheckDPIScale();return;}
		if(UIScale!=inc_float){
			UIScale=inc_float;
			UIOb.transform.localScale=new Vector3(UIScale,1f,UIScale);
			ScreenResize();
		}
	}
	
	static public float GetUIWidth(){
		return MasterCamera.GetPixelWidth()/UIScale;
	}	
	static public float GetUIHeight(){
		return MasterCamera.GetPixelHeight()/UIScale;
	}
	
	static public float GetMinimapScale(){
		return minimapScale;
	}
	static public void SetMinimapScale(float inc_float){
		minimapScale = inc_float;
		ScreenResize();
	}
	/*
	static public float GetAndroidDPI(){
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
     
        AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
        activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);
     
        return (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
    }*/
	
	static public void  LoadWWWTexture(string inc_url,DelegateObject inc_del=default(DelegateObject)){ if(!thisScript)return;
		thisScript.StartCoroutine( LoadWWWTextureCo(inc_url,inc_del));
	}
	static public IEnumerator LoadWWWTextureCo(string inc_url,DelegateObject inc_del=default(DelegateObject)){
		if(www_texture_dict.ContainsKey(inc_url)){
			if(inc_del!=null){inc_del(www_texture_dict[inc_url]);}
			yield return true;
		}
		using (WWW www = new WWW(inc_url)){ // Start a download of the given URL
            yield return www; // Wait for download to complete
			www_texture_dict[inc_url]=www.texture; // assign texture
			if(inc_del!=null){inc_del(www_texture_dict[inc_url]);}
        }
		
	}

	
	static public GameObject CreateWorldText(string inc_rich_text,Vector3 inc_pos,float inc_scale=1f){
		//if(textOb!=null){Destroy(textOb);}//if(inc_text=="")return;
		//if(textOb==null){
		GameObject textOb=new GameObject("worldtxt");
		//textOb.layer=MasterGame.GetLayer("ui");
		MeshRenderer newButRenderer=	textOb.AddComponent<MeshRenderer>();
		MeshFilter thisFilt = textOb.AddComponent<MeshFilter>();
		textOb.Localize(MasterGame.effectsTrans);
		//}
		TextGenerationSettings m_TextSettings = MasterUI.GetTextGenSettings();
		m_TextSettings.generationExtents = new Vector2(MasterConsole.GetWidth(), 0f);
		m_TextSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
		TextGenerator textGenerator = new TextGenerator(); 
		textGenerator.Populate(inc_rich_text, m_TextSettings);
	
		//MeshFilter thisFilt = textOb.GetComponent<MeshFilter>();
		thisFilt.mesh = textGenerator.GetMesh(thisFilt.mesh);
		//MeshRenderer newButRenderer=	textOb.GetComponent<MeshRenderer>();
		//height=(int) Mathf.Ceil((newButRenderer.bounds.extents.z/MasterUI.UIScale)/(MasterConsole.GetRowHeight()/2f));
		newButRenderer.sharedMaterial = MasterUI.GetDefaultFontMaterial();		

		float textScale=(0.05f*MasterUI.UIScale*inc_scale* MasterCamera.mainCamera.orthographicSize)/ (MasterCamera.uiCamera.orthographicSize*MasterUI.GetFontScaleFactor());//float textScale=0.0909f;
		textOb.transform.localScale = new Vector3(textScale,textScale ,textScale);
		textOb.transform.localRotation=Trans.QuatEul(90f,Viewer.GetYRot(),0f);
		textOb.transform.position=inc_pos;
		Destroy(textOb,5f);
		return textOb;
	}
	
	static public TextGenerationSettings GetTextGenSettings(){
			TextGenerationSettings m_TextSettings = new TextGenerationSettings();
			m_TextSettings.textAnchor =TextAnchor.MiddleCenter;//m_TextSettings.textAnchor = TextAnchor.MiddleCenter;
			m_TextSettings.color = Color.white;
			m_TextSettings.generationExtents = new Vector2(0f,0f);
			m_TextSettings.pivot = new Vector2(0.5f, 0.5f);
			m_TextSettings.richText = true;
			m_TextSettings.font = MasterUI.GetDefaultFont();
			m_TextSettings.fontSize = 0;//MasterUI.GetBaseFontSize();
			m_TextSettings.fontStyle = FontStyle.Normal;
			m_TextSettings.verticalOverflow = VerticalWrapMode.Overflow;
			m_TextSettings.horizontalOverflow = HorizontalWrapMode.Overflow;//m_TextSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
			m_TextSettings.lineSpacing = 1;
			m_TextSettings.generateOutOfBounds = true;
			m_TextSettings.resizeTextForBestFit = false;
			m_TextSettings.scaleFactor = GetFontScaleFactor();
			return m_TextSettings;
		 
	 }
}


public static class TextExtensions{
 
         static public Mesh GetMesh(this TextGenerator i_Generator, Mesh o_Mesh){
             if (o_Mesh == null){o_Mesh=new Mesh();}
			 else{ o_Mesh.Clear();}
			
             int vertSize = i_Generator.vertexCount;
             Vector3[] tempVerts = new Vector3[vertSize];
             Color32[] tempColours = new Color32[vertSize];
             Vector2[] tempUvs = new Vector2[vertSize];
             IList<UIVertex> generatorVerts = i_Generator.verts;
             for (int i = 0; i < vertSize; ++i){
                 tempVerts[i] = generatorVerts[i].position;
                 tempColours[i] = generatorVerts[i].color;
                 tempUvs[i] = generatorVerts[i].uv0;
             }
             o_Mesh.vertices = tempVerts;
             o_Mesh.colors32 = tempColours;
             o_Mesh.uv = tempUvs;
 
             int characterCount = vertSize / 4;
             int[] tempIndices = new int[characterCount * 6];
             for(int i = 0; i < characterCount; ++i)
             {
                 int vertIndexStart = i * 4;
                 int trianglesIndexStart = i * 6;
                 tempIndices[trianglesIndexStart++] = vertIndexStart;
                 tempIndices[trianglesIndexStart++] = vertIndexStart + 1;
                 tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                 tempIndices[trianglesIndexStart++] = vertIndexStart;
                 tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                 tempIndices[trianglesIndexStart] = vertIndexStart + 3;
             }
             o_Mesh.triangles = tempIndices;
             //TODO: setBounds manually
             o_Mesh.RecalculateBounds();
			 return o_Mesh;
         }
}

