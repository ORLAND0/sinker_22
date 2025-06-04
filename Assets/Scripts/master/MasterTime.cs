using UnityEngine;
using System;
using System.Collections;

public class MasterTime : MonoBehaviour {

	double debugvar1;
	public double debugvar2;
	//GUIStyle menuStyle = GUIStyle();


	//TIME VARS
	static public double localGameStartTime;
	static public double gameTime;//in-game time
	static public double gameTimeSynced;//The difference between realTime and gameTime;
	static public double realTime;//the time synced between client and server
	static public double localTime;//Client Up Time.. GetUpTime() at beggining of frame
	static public double timeScale=1;
	static public double timeScalePrev=1;
	static public float timeScaleFloat=1f;//avoid re-conversion when sending from server
	static public int frameNo;//physics frame id
	static public int updates;//how many frames game has been updated on

	static public double lastFixedTime; //time.time on frameno calc
	static public double lastUpdateTime;//last update game time.time
	static public double lastUpdateGameTime;
	static public int lastUpdateFrame;

	static private long tickZero;
	static public double timeSyncFrequency = 25; //how often to sync time with server, in seconds
	static public double localTimeRequested = -137d; // the time since sync last sent set it to before so we sync on frame 1
	static public double localTimeSynced = 0; // the time since sync last recieved 
	static public double serverTimeDelta; //Time difference between local reference time and server's
	
	static public int fps=1;
	//public double fpsSampleReset=0.0;
	public double fpsDeltaTime=0;
	//public int fpsSinceReset=0;public float fpsSampleDuration=1f;
	[HideInInspector]
	public TextMesh fpsTextMesh;
	[HideInInspector]
	public UITooltipHover fpsTooltip;
	
	//TODO: make master time scale compliant.
	public void Awake(){
		localGameStartTime=Time.time;
		
	}
	
	public void  Start (){
		//thisNetView=GetComponent<NetworkView>();
		SetZeroTime();

		fpsTextMesh=GameObject.Find("fps_text_mesh").GetComponent<TextMesh>();
		fpsTooltip=fpsTextMesh.gameObject.GetComponent<UITooltipHover>();
		SetTimeScale(1.0f);//SPEED IT UPPP
	}
	
	


	public void  Update (){ updates++;
		//check for sync
		//update local time
		//gameTime = gameTime+(Time.time-lastFixedTime);//
		localTime=GetUpTime();//

		realTime=localTime+serverTimeDelta;

		gameTime = gameTimeSynced+(localTime-localTimeSynced)*timeScale;
		
		debugvar2= gameTime;
		lastUpdateTime = Time.time;
		lastUpdateGameTime = gameTime;
		lastUpdateFrame=frameNo;
		int newFrameNo = (int) Mathf.Ceil((float)(gameTime / Time.fixedDeltaTime));
		if( lastUpdateFrame>newFrameNo){
			frameNo=newFrameNo;
			//Debug.Log("fixed update played an extra "+(lastUpdateFrame-newFrameNo)+" frames");
		}
		if( lastUpdateFrame<newFrameNo-3){
			frameNo=newFrameNo;
			//Debug.Log("fixed update fell behind "+(lastUpdateFrame-frameNo)+" frames");
		}	
		//frameNo=newFrameNo;
		//lastUpdateFrame=frameNo;
		
		fpsDeltaTime += (GetRealDelta() - fpsDeltaTime) * 0.1f;
		fps=(int)Mathf.Floor(1.0f / (float)fpsDeltaTime);
		string newString=fps.ToString();//newString=string.Format("{0:0.##}",gameTime);//
		
		if(newString!=fpsTextMesh.text){fpsTextMesh.text=newString;}
		if(fpsTooltip.tooltipFrame==null){
			fpsTooltip.text=newString+" frames /sec. \n Time Scale: "+string.Format("{0:0.##}",timeScale)+"\n  Game Time: "+string.Format("{0:0}",gameTime);
		}
		//UnityEngine.Random.InitState(MasterTime.updates);
		UnityEngine.Random.seed=updates;//randomize me captain


	}


	private void  FixedUpdate (){
		
		//lastFixedTime = Time.fixedTime;
		//update local time
		//gameTime = gameTime+Time.fixedDeltaTime;
		gameTime = lastUpdateGameTime + (Time.fixedTime - lastUpdateTime);//*timeScale;//
		frameNo++;
		//gameTime=frameNo*Time.fixedDeltaTime;
		//lastUpdateGameTime + (Time.time - lastUpdateTime);//-localGameStartTime;
	}
	

	
	static public void SetZeroTime(){
		frameNo=0;
		localTime=0;gameTime=0;gameTimeSynced=0;localTimeSynced = 0;lastUpdateGameTime=0;lastUpdateTime=0;
		tickZero = DateTime.Now.Ticks;
		//localGameStartTime=Time.time;
		serverTimeDelta = -GetUpTime();
		realTime=GetUpTime() +serverTimeDelta;
		Viewer.spectatePlayerSent=0;
	}

	static public void SendTimeSyncAll(){	}
	
	static public void SendTimeSyncPlayer(){ 	}
	
	//[RPC] //called on server by client
	public void  requestTime ( ){
		SendTimeSyncPlayer(info.sender);
	}
	//[RPC] //called on client by server
	public void  sendTime ( float incDecimalGameTime ,   float incTensGameTime , float incDecimalTime ,   float incTensTime ,float incTimeScale){
		double rpcTransitTime  = 0;
		double currentServerRealTime = incDecimalTime+incTensTime+rpcTransitTime;
		serverTimeDelta = currentServerRealTime-(GetUpTime());
		SetTimeScale(incTimeScale);
		gameTimeSynced=incDecimalGameTime+incTensGameTime+(rpcTransitTime*timeScaleFloat);
		localTimeSynced = GetUpTime();
		//Debug.Log(gameTime+"-"+gameTimeSynced+"="+(gameTime-gameTimeSynced));
		//gameTime=gameTimeSynced+(gameTime-gameTimeSynced)/2;//half way between both 
		//lastUpdateGameTime = gameTime;
		//frameNo = (int) Mathf.Floor((float)(gameTime / Time.fixedDeltaTime));
	}	
	
	
	static public void SetTimeScale(float inc_scale){ //(float)
		timeScaleFloat=inc_scale;
		timeScalePrev=timeScale;
		timeScale=(double)inc_scale;
		//Debug.Log("time scale set to:" + timeScale);
		Time.timeScale=timeScaleFloat;
		if(MasterConnect.isServer){
			gameTimeSynced=gameTime;
			localTimeSynced = localTime;
			SendTimeSyncAll();
		}
	}
	
	static public double GetTimeScale(){
		return timeScale;
	}
	
	static public double GetTimeScalePrev(){
		return timeScalePrev;
	}
	
	/*public void  OnGUI (){
		//GUILayout.Label("Cwfwfted");
		string dbugstring= frameNo.ToString();
		string dbugstring2= gameTime.ToString();
		
		float frametimeoffset= (frameNo*Time.fixedDeltaTime) - (float)gameTime;
		string dbugstring3=  frametimeoffset.ToString();

		//menuStyle.fontSize = 12;
		GUI.Label( new Rect(Screen.width - 40,Screen.height-51,200,80),dbugstring);
		GUI.Label( new Rect(Screen.width - 40,Screen.height-18,200,80),dbugstring3);
		GUI.Label( new Rect(Screen.width - 40,Screen.height-35,200,80),dbugstring2);

	}//end on GUI*/
	
	static public int GetEpochTime(){
		 System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
	}
	
	static public double GameTime(){
		return GetGameTime();
	}
	static public double GetGameTime(){
		return gameTime;
	}	
	
	static public double GetRealTime(){
		return realTime;
	}
	
	static public double GetUpTime(){
		//return Network.time;
		long elapsedTicks = DateTime.Now.Ticks - tickZero;
		double secondsTime=(double)elapsedTicks/10000000.0;
		return secondsTime;
	}

	static public double GetRealDelta(){
		return Time.unscaledDeltaTime;
		
	}
	
	static public double GetFrameTime(int inc_frame){
		return inc_frame * Time.fixedDeltaTime;
	}
	
	static public int GetUnixTime(){
		TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
		TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
		double unixTime = unixTicks.TotalSeconds;
		return Trans.ParseInt(unixTime);
	}

}