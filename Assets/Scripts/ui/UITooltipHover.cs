using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UITooltipHover : MonoBehaviour , IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerClickHandler{

	public string text="";
	private string textPrev="";//previous frame
	public UITooltipFrame tooltipFrame;
	
	public int relativeTo=1;//1 transform; 2 cursor
	public Vector3 relativePos=new Vector3(1f,0f,0f);//(x,ydown,)
	public TextAnchor textAnchor=TextAnchor.UpperLeft;

	
	public int closeOnClick=1;
	
	
	public DelegateObject callbackClick;
	public DelegateGameObject callbackPress;
	public DelegateGameObject callbackPointerEnter;
	public DelegateObject callbackPointerEnterData;
	public DelegateGameObject callbackPointerExit;
	public DelegateGameObject callbackPointerStay;
	public DelegateGameObject callbackPointerDown;

	//void Start () {	}
	public void Update(){
		if(tooltipFrame!=null&&textPrev!=text){
			tooltipFrame.SetText(text);
			textPrev=text;
		}
		
	}
	

	//REQUIRES COLLIDER + TRIGGER checked
	public void OnPointerEnter(PointerEventData data){
		if(callbackPointerEnter!=null){callbackPointerEnter(gameObject);}
		if(callbackPointerEnterData!=null){callbackPointerEnterData(data);}
		Open();
	}	
	public void OnPointerExit(PointerEventData data){
		if(tooltipFrame!=null){tooltipFrame.Close();}
		if(callbackPointerExit!=null){callbackPointerExit(gameObject);}
		
	}	
	public void OnPointerStay(PointerEventData data){
		if(tooltipFrame!=null){tooltipFrame.startTime=MasterTime.gameTime;}
		if(callbackPointerStay!=null){callbackPointerStay(gameObject);}
		
	}
	
	public void OnPointerDown(PointerEventData data) {
		if(callbackPointerDown!=null){callbackPointerDown(gameObject);}
		MasterUI.OverlayClick(this);	
		
	}
	public void OnPointerClick(PointerEventData data) {
		MasterUI.OverlayClick(this);
		if(closeOnClick==1&&tooltipFrame!=null){tooltipFrame.Close();}
		if(callbackClick!=null){callbackClick(data);}
		Press();

    }
	
	public void Press(){
		if(callbackPress!=null){callbackPress(gameObject);}
	}
	
	public void Open(){
		if(tooltipFrame!=null){tooltipFrame.Close();}
		tooltipFrame=UITooltipFrame.CreateFrame(text);
		tooltipFrame.textAnchor=textAnchor;
		Vector3 tipPos=transform.position+relativePos*MasterUI.UIScale;
		if(relativeTo==2){tipPos=Input.mousePosition+relativePos*MasterUI.UIScale;}//not workin?
		tooltipFrame.coord=MasterCamera.uiCamera.WorldToScreenPoint(tipPos)/MasterUI.UIScale;
		textPrev=text;//putting here to avoiding late update
	}
	public void Close(){
		if(tooltipFrame!=null){tooltipFrame.Close();}
	}
}
