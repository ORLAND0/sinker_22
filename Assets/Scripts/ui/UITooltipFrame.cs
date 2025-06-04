using UnityEngine;
using System.Collections;

public class UITooltipFrame: MonoBehaviour {
	
	public GameObject contentOb;
	public Transform contentTrans;
	
	public Vector3 coord=Vector3.zero;
	[HideInInspector]
	public Vector3 coordOffset=Vector3.zero;
	public float width=-1f;//in pixels
	public float height=-1f;//in pixels
	public int autoSize=1;
	
	//public double minDuration=.5;
	public double maxDuration=-1;
	public double startTime=0f;
	
	public string text="";
	public TextAnchor textAnchor=TextAnchor.UpperLeft;
	public TextGenerator textGenerator;
	public GameObject textOb;
	public Color textColor=Color.white;//deprecating? use richtext (dont use richtext i think)
	public float scale=1f;//
	
	Color bgColor=new Color(.25f,.25f,.25f,.6f);
	GameObject bgOb;
	
	void Awake(){
		gameObject.layer=MasterGame.phys_layers["ui"];
		transform.parent=MasterUI.tooltipTrans;
		gameObject.Localize();
		if(contentOb==null){
			contentOb=new GameObject(gameObject.name+"_ttip_tent");
			contentTrans=contentOb.transform;
			contentTrans.parent=transform;
			contentOb.Localize();
			
		}
	}
	
	// Use this for initialization
	void Start () {
		startTime = MasterTime.gameTime;
		if(maxDuration==-1)maxDuration=3.5f;
		SetWidth(width);
		SetHeight(height);
		//SetCoord(coord);
		Refresh();
		//MasterUI.AddUIListener(gameObject);
		gameObject.SetLayerChildren("ui");
	}
	
	
	void Update () {
		if(maxDuration>0f){
			if(startTime+maxDuration<MasterTime.gameTime)Close();
		}
	}
	
	public void SetWidth(float inc_width){ //in pixels
		if(MasterUI.GetUIWidth()<inc_width)inc_width=MasterUI.GetUIWidth();
		width=inc_width;

	}
	public void SetHeight(float inc_height){ //in pixels
		if(MasterUI.GetUIHeight()<inc_height)inc_height=MasterUI.GetUIHeight();
		height=inc_height;

	}	
	public void Refresh(){
		if(bgOb==null){CreateBgOb();}
		//if(text!=""){
		//	SetText(text);
		//}
		SetCoord(coord);
		bgOb.transform.localScale = new Vector3(width*.1f,height*.1f,1f);
		bgOb.transform.localPosition=new Vector3(width*.05f,-height*.05f,.1f);
		transform.localScale=Vector3.one*scale;
	}

	
	public void SetCoord(Vector3 inc_vec){//in world to screen cords
		coord=inc_vec;
		Vector3 setCoord=inc_vec;
		if(textAnchor==TextAnchor.UpperRight){setCoord.x+=-width;}
		if(setCoord.x>MasterUI.GetUIWidth()){setCoord.x=MasterUI.GetUIWidth();}
		if(setCoord.y>MasterUI.GetUIHeight()){setCoord.y=MasterUI.GetUIHeight();}
		if(setCoord.x+width>MasterUI.GetUIWidth()){
			coordOffset.x=-8f-(setCoord.x+width-MasterUI.GetUIWidth());
		}else{
			if(setCoord.x<0){
				coordOffset.x=-setCoord.x+8f;
			}
		}		
		if(setCoord.y-height<0){
			coordOffset.y=height;
		}
		transform.localPosition=new Vector3(Mathf.Round((setCoord.x+coordOffset.x))*.1f,Mathf.Round((setCoord.y+coordOffset.y))*.1f,-4f);
	}
	
	
	public void CreateBgOb(){
		if(bgOb!=null){Destroy(bgOb);}
		bgOb=new GameObject(gameObject.name+"_ttip_bgob");
		bgOb.layer=LayerMask.NameToLayer("UI");
		bgOb.transform.parent=gameObject.transform;
		bgOb.Localize();
		MeshRenderer bgRend = bgOb.AddComponent<MeshRenderer>();
		MeshFilter bgFilt = bgOb.AddComponent<MeshFilter>();
		bgFilt.mesh=MeshGenPlane.CreatePlane();
		bgRend.material=MaterialHelper.CreateMaterialTransparent();
		bgRend.material.SetColor("_Color", bgColor);
	}
	
	public void SetScale(float inc_scale){ 
		scale=inc_scale;
		Refresh();
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
			textOb.transform.localPosition=new Vector3(0f,0f,-.1f);
		}
		TextGenerationSettings m_TextSettings = MasterUI.GetTextGenSettings();
		m_TextSettings.textAnchor = textAnchor;//m_TextSettings.textAnchor = TextAnchor.MiddleCenter;
		m_TextSettings.richText = true;
		//m_TextSettings.color = textColor;
		float genWidth=300f; float genHeight=0f;
		if(autoSize<1&&width>-1){genWidth=width;}
		if(autoSize<1&&height>-1){genHeight=height;}
		m_TextSettings.generationExtents = new Vector2(genWidth, genHeight);
		m_TextSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
		if(textGenerator==null){textGenerator = new TextGenerator();} 
		textGenerator.Populate(text, m_TextSettings);
		
		textOb.transform.localScale=(new Vector3(.1f,.1f,.1f))/MasterUI.GetFontScaleFactor();
		
		MeshFilter thisFilt = textOb.GetComponent<MeshFilter>();
		thisFilt.mesh = textGenerator.GetMesh(thisFilt.mesh);
		MeshRenderer newButRenderer=	textOb.GetComponent<MeshRenderer>();
		newButRenderer.sharedMaterial = MasterUI.GetDefaultFont().material;
		//if(textColor!=null){
		//	SetTextColor(textColor);
		//}
		float xpad=4f;
		float ypad=2f;
		if(autoSize>0){
			SetHeight(newButRenderer.bounds.extents.z*20f/MasterUI.UIScale+2f*ypad);
			SetWidth(newButRenderer.bounds.extents.x*20f/MasterUI.UIScale+2f*xpad);
		}
		if( textAnchor==TextAnchor.UpperLeft){
			textOb.transform.localPosition=new Vector3(((genWidth/2f)+xpad)*.1f,.2f-ypad*.1f,-.1f);
		}		
		if( textAnchor==TextAnchor.UpperCenter){
			textOb.transform.localPosition=new Vector3((Mathf.Round((width/2f)))*.1f,.2f-ypad*.1f,-.1f);
		}
		if( textAnchor==TextAnchor.UpperRight){
			textOb.transform.localPosition=new Vector3((-((genWidth/2f))+(width-xpad))/10f,.2f-ypad*.1f,-.1f);
		}

		Refresh();
		//SetCoord(coord);//refresh
	}
	public void SetTextColor(Color inc_color){
		textColor=inc_color;
		if(textOb==null)return;
		MeshRenderer textRend=textOb.GetComponent<MeshRenderer>();
		textRend.material.color = inc_color;
	}	
	
	public void SetBGColor(Color inc_color){
		bgColor=inc_color;
		if(textOb==null)return;
		CreateBgOb();
	}
	
	public void Close(){
		Destroy(gameObject);
	}
	
	static public UITooltipFrame CreateFrame(string inc_text=""){
		GameObject newob = new GameObject(inc_text+"_ttip");
		UITooltipFrame ttip = newob.AddComponent<UITooltipFrame>();
		if(inc_text!="")ttip.SetText(inc_text);
		return ttip;
	}
	
}
