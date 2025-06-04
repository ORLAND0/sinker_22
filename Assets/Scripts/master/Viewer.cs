using UnityEngine;
using System.Collections;

public class Viewer: MonoBehaviour {

	public Transform target;

	static public GameObject theViewer;
	static public Transform viewerTrans;
	static public Transform listenerTrans;
	static public Transform particlesTrans;
	static public Viewer thisScript;
	static public int spectateMode=-1;//-1 init,0 not spectating (playing),1 spectating all teams, 2 spectating specific team?
	static public int spectatePlayer=-1;//-1init;-2 wandering;-3 non player;>-1 playerid;
	static public int spectatePlayerCur=0;
	static public double spectatePlayerSent=0;//realtime of previous spectate request. re-zerod in MasterTime.SetZeroTime
	static public Vector3 spectatePosSent=Vector3.zero;
	static public Vector3 viewerBoundsMin=Vector3.zero;
	static public Vector3 viewerBoundsMax=Vector3.zero;
	


	void  Awake (){
		theViewer=gameObject;
		viewerTrans=gameObject.transform;
		listenerTrans=transform.Find("AudioListener");
		particlesTrans=transform.Find("particles float");
		thisScript=this;
	}
	void Update(){
		
	
		viewerBoundsMin.x=viewerTrans.position.x-99f;	//assign the bounds of the 'screen'
		viewerBoundsMax.x=viewerTrans.position.x+99f;	
		viewerBoundsMin.z=viewerTrans.position.z-99f;
		viewerBoundsMax.z=viewerTrans.position.z+99f;
	}

	void  LateUpdate (){

	}

	static public Transform GetTarget(){return thisScript.target;}
	static public void  SetTarget ( Transform incTrans  ){
		thisScript.target = incTrans;

	}
	
	static public void SetPos(Vector3 inc_vec){//defaults at 0,10,0
		theViewer.transform.position = inc_vec;
	}
	
	static public float GetYRot(){
		if(MasterCamera.rotationMode<-1)return MasterCamera.thirdCamera.transform.eulerAngles.y;
		return theViewer.transform.eulerAngles.y;
	}
	
	static public float GetYRotMinimap(){
		return theViewer.transform.eulerAngles.y;
	}
	
	static public Vector3 GetPosition(){
		return theViewer.transform.position;
	}
	
	static public void SpectateMode(int inc_mode){	SetSpectateMode(inc_mode);}
	static public void SetSpectateMode(int inc_mode){
		//Debug.Log("set mode "+inc_mode);
		spectateMode=inc_mode;
		//if(spectateMode<1)SetSpectatePlayer(-1);
		if(spectateMode==0&&IntroScape.introScape!=null){
			IntroScape.Finish();
			ObjectDelegator.DelegateDelay(inc_ob=>{	
				if(Viewer.spectateMode>0&&Viewer.GetTarget()==null){
					Viewer.SpectateFindTargetNext();
				}
			},3f);
		}
		
	}
	
	static public void SpectateFindTarget(){
		
		
	}
	
	static public void SpectateFindTargetNext(){
		
	}	
	static public void SpectateFindTargetPrev(){
			
		
	}

	static public void SetSpectatePlayer(int inc_player_id){
		
	}

	
	static public float  DistanceFromViewer( Vector3 incVec  ){
		return (thisScript.transform.position-incVec).magnitude;
	}	
	static public float  SquareDistanceFromViewer( Vector3 incVec  ){
		return (thisScript.transform.position-incVec).sqrMagnitude;
	}
	
	static public int IsPositionOnScreen(Vector3 incVec){
		if(MasterConnect.isDedicatedServer>0){return 0;}
		//todo: if incvec y is very negative... grow bounds by ratio.
		
		if(incVec.x>viewerBoundsMin.x&&incVec.x<viewerBoundsMax.x&&incVec.z>viewerBoundsMin.z&&incVec.z<viewerBoundsMax.z){
			return 1;
		}
		if(incVec.y>660f&&incVec.y<690f){//UI range
			return 2;	
		}
		return 0;
		incVec.y=10f;//by distance
		if(SquareDistanceFromViewer(incVec )<9000f){return 1;}
		return 0;
	}
	
	
}