using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor.Build;
using System;
using System.Collections;
using System.Collections.Generic;


public class MasterGame : MonoBehaviour { //manages Zone loading and general game functions


	static public string mapID = "";

	static public GameObject zoneObject;
	static public int zoneIncrement;

	
	static public int version=2501110;//
	static public int isQuitting = 0;//1 quiting;2 ending game/zone;

	//Hierarchy management
	static public Transform masterTrans;
	static public GameObject effectsOb;
	static public Transform effectsTrans;
	static public GameObject effectsDebrisOb;
	static public Transform effectsDebrisTrans;
	static public Transform effectsLabelsTrans;

	static public Transform soundsTrans;
	static public Transform utilsTrans;
	static public Transform astarTrans;
	static public Transform astarGroundTrans;
	
	static public Dictionary<string,int> phys_layers;
	
	public PhysicMaterial matBounce;
	public PhysicMaterial matNoBounce;
	
	public GameObject introOceanFab;
	public GameObject introScapeFab;
	public GameObject introScape2Fab;
	static public GameObject introScape;
	
	//static Exploder.ExploderObject exploder;

	string enterDebugVar;
	static string debugVar;
	

	static public GameObject masterOb;
	static public MasterGame thisScript;
	
	//static public int disableInput=0;

	public void Awake(){
		thisScript=this;
		masterOb=gameObject;

		//Scene gotScene=GetSceneByName("steam_scene");
		//if(gotScene.name!="steam_scene"){Debug.Log("ceere");
		//	SceneManager.LoadScene("steam_scene", LoadSceneMode.Additive);
		//}

		InitThis();	
		//MasterUtils.Init();
		//MasterUnits.Init();
	}
	public void  Start (){
		//mapID=PlayerPrefs.GetString("set_host_zone");
		//IntroOcean.Begin();
	}



	public void  Update(){

	}
	public void LateUpdate(){
		if(false && cInput.GetKeyUp("Screenshot")){
			string filePath = MasterCamera.SaveScreenshot();
			DelegateGameObject delayLog= inc_ob =>{
				MasterConsole.Log("Screenshot saved as " + filePath);
			};
			ObjectDelegator.DelegateDelay(delayLog,.01f);
		
		}
		
		//UnitDebris.HandleCuts();

	}

	static public Vector3  GetOrigin(){ //to be deprecated
		return new Vector3(0f,10f,0f);
	}

	



	
	static public void ServerSetupZone(){
	
		
	}
	



	//server code to send zone message to client
	static public void SendZoneMessage(Dictionary<string,object> inc_params,int inc_player=-2){
		SendZoneMessage(MasterConnect.DictToJson(inc_params),inc_player);
	}
	static public void SendZoneMessage(string inc_message ,int inc_player=-2){//-2 all; 0 server only

	}
	
	//[RPC] 
	public void RpcZoneMessage(string inc_message){ //Client code to receive zone message from server
		if(loadedZone!=null&&loadedZone.zoneMessageHandler!=null){loadedZone.zoneMessageHandler(inc_message);}
	}
	
	static public void ZoneTrigger(string inc_trigger,object inc_ob){
		if(loadedZone!=null){loadedZone.ZoneTrigger(inc_trigger,inc_ob);	}
	}


	static public void  InitServer (){

	}
	
	static public void InitClient(){

	}
	public void CreateDefaultTeams(){

	}

	public void  InitThis (){//Debug.Log("master game initthis");
		masterTrans=transform;
		//FIXME_VAR_TYPE exploderOb= new GameObject("Exploder");
		//exploderTrans=exploderOb.transform;
		
		phys_layers=new Dictionary<string,int>();
		phys_layers["wall"]=LayerMask.NameToLayer ("wall");		
		phys_layers["util"]=LayerMask.NameToLayer ("util");
		phys_layers["unit"]=LayerMask.NameToLayer ("unit");
		phys_layers["debris"]=LayerMask.NameToLayer("debris");
		phys_layers["only_debris"]=LayerMask.NameToLayer("only_debris");
		phys_layers["only_wall"]=LayerMask.NameToLayer("only_wall");
		phys_layers["map"]=LayerMask.NameToLayer("mapsystem");
		phys_layers["ui"]=LayerMask.NameToLayer("UI");
		phys_layers["non"]=LayerMask.NameToLayer("non");
		UnitDebrisCollider.unitDebrisLayer = LayerMask.NameToLayer("only_debris");
		
		//for in-game organization / clearing.
		if(effectsOb){Destroy(effectsOb);}
		effectsOb= new GameObject("Effects");
		effectsTrans = effectsOb.transform;		
		//if(effectsDebrisTrans){Destroy(effectsDebrisTrans.gameObject);}
		effectsDebrisOb= new GameObject("Debris");
		effectsDebrisTrans = effectsDebrisOb.transform;	
		effectsDebrisTrans.parent = effectsTrans;
		UnitDebris.effectsDebrisTrans=effectsDebrisTrans;
		GameObject effectsLabelsOb= new GameObject("Labels");
		effectsLabelsTrans = effectsLabelsOb.transform;	
		effectsLabelsTrans.parent = effectsTrans;
		UnitLabel.nameTain=effectsLabelsTrans;
	
		if(soundsTrans){Destroy(soundsTrans.gameObject);}
		GameObject soundsObj= new GameObject("Sounds");
		soundsTrans = soundsObj.transform;
		if(utilsTrans){Destroy(utilsTrans.gameObject);}
		GameObject utilsObj= new GameObject("Utils");
		utilsTrans = utilsObj.transform;	
			

		MasterSettings.LoadAudio();
		MasterTime.SetZeroTime();
		
	}
	
	static public int GetLayer(string inc_string){
		return phys_layers[inc_string];
	}
	
	static public int GetLayerMask(string inc_string){
		return (1 << phys_layers[inc_string]);//wat
		Debug.Log("layer mask"+ inc_string+" not set");
		return 0;
		
	}

	static public void  StartGame(){ //for both clients and server
		masterOb.SendMessage("InitGame", 1.0f);
		if(MasterConnect.isServer){MasterCamera.Reset();}
		//MasterConsole.ShowTip();
	}

	static public void  EndGame(){
		
		isQuitting=1;
		Network.Disconnect(200);
		//masterOb.SendMessage("ExitGame", 1.0f);
		//MasterUtils.ClearUtils();
		//MasterUnits.ClearUnits(); 
		if(zoneObject!=null)Destroy(zoneObject);
		thisScript.StopAllCoroutines();
		DestroyWithZone.DestroyAll();
		//	return;
		//SceneManager.UnloadScene("SubScene");//Application.LoadLevel(Application.loadedLevel);
		
		isQuitting=0;
		thisScript.InitThis();
		
		//MasterUtils.Init();
		//MasterUnits.Init();
		//MasterPlayers.Init();
		if(UIMenuFrame.openFrames.Count==0){
			MenuMain.Open();	
		}
		


		//SceneManager.LoadScene("SubScene",LoadSceneMode.Additive);
		
	}
	
	static public void PauseGame(){// TOGGLE pause
		if(MasterTime.GetTimeScale()>0.001){
			MasterPlayers.RequestTimeScale(0.0001f);
		}else{
			MasterPlayers.RequestTimeScale(Trans.ParseFloat(MasterTime.GetTimeScalePrev()));
		}
		
	}
	
	public static void IntroScapeEnd(){
		//if(introScape!=null)introScape.SendMessage("end");
	}
	public static void IntroScapeStart(){
		//IntroScape.Begin();
	}
	
	 public static Scene GetSceneByName(string inc_name){
        if (SceneManager.sceneCount > 0){
            for (int n = 0; n < SceneManager.sceneCount; ++n){
                Scene scene = SceneManager.GetSceneAt(n);
				if(inc_name==scene.name){
					return scene;
				}//output += scene.isLoaded ? " (Loaded, " : " (Not Loaded, "; output += scene.isDirty ? "Dirty, " : "Clean, "; output += scene.buildIndex >= 0 ? " in build)\n" : " NOT in build)\n";
            }
        }
        return default(Scene);
    }

	

	public void  OnApplicationQuit(){
		//MasterLogin.EndServerToken();
		EndGame();
		//SteamManager.OnQuit();
		MasterSettings.SetSetting("quit_success","1");
	}
}

