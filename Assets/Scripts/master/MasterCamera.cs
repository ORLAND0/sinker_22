// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterCamera : MonoBehaviour {
	static public Viewer tehviewer;
	static public Camera mainCamera;//
	static public Dictionary<int,MasterCamera> masterCameras = new Dictionary<int, MasterCamera>(); //0 - front 1- main 2- back //for the love of frames please make this an array
	static public Camera uiCamera;
	static public Camera thirdCamera;
	static public float pixelsw;
	static public float pixelsh;
	static public float aspect;
	static public float adjustDistance = 6f; //
	static public float adjustDistanceDefault = 6f; //

	public int isMainCamera=0;//0 frontdrop,1 units,2 backdrop, 
	public float adjustSmoothing = 2.7f; //
	public float xVel = 0.0f;
	public float zVel = 0.0f;
	public float baseFov= 19f;
	public float baseOrtho= 20f;
	public bool  orthomode = true;

	public float autoZoomSpeed = 2f;
	public float autoZoomAmount = 1f;

	public Matrix4x4 camMatrix;
	public Matrix4x4 modMatrix;
	public Vector4 matrixMod; //w-skewx x-scalex ,y-skewz, z-scalez
	public Vector4 matrixModPow;

	public Vector4 teleFluxMatrix;

	public Camera thisCamera;
	static public float zoomAdjust = 1.0f;
	static public float defaultZoom =1.09f;//should be a player setting
	private float fovAdjustment;
	static public int rotationMode = 0; //-3 third person,-2 locked to rot, -1 locked to target,0 is normal, 3 is 270 roing CW;
	static public float rotationY=0f;//
	static public float rotationYLock=0f;
	static public float rotationElev=0f;

	[HideInInspector]
	public int fluctuateTeleportStart=0;
	[HideInInspector]
	public int fluctThisTurn=0;
	public float debugvar;


	public void  Awake (){
		//camera.orthographic = true;
		//camera.enabled = true;
		if(thisCamera==null){	thisCamera=GetComponent<Camera>(); 	}
		if(isMainCamera==1&&thisCamera!=null){ mainCamera=thisCamera; }
		if (orthomode ==true){	 thisCamera.orthographic = true; }
		
		tehviewer=transform.parent.GetComponent<Viewer>();
		masterCameras[isMainCamera]=this;
		if(!uiCamera){	uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();	}	
		if(!thirdCamera){	thirdCamera = tehviewer.transform.Find("thirdCamera").GetComponent<Camera>();	}	
	}
	
	public void Start(){
		SetZoom(defaultZoom);
		SetAdjustDistance(adjustDistanceDefault);
	}
	
	public void  Update (){ //this is all happening 3 times.... not right
		if(pixelsw != thisCamera.pixelWidth||pixelsh != thisCamera.pixelHeight){
			
			pixelsw = thisCamera.pixelWidth;
			pixelsh  = thisCamera.pixelHeight;
			aspect = thisCamera.aspect;
			//MasterUI.ScreenResize();
		}

		if( 1 == 0 ){
			thisCamera.ResetProjectionMatrix();
			camMatrix=thisCamera.projectionMatrix;	
			thisCamera.projectionMatrix=modMatrix;
		}
		if(isMainCamera==1&&MasterUI.disableInput==0){
				if((cInput.GetKey("RotateCW")&&cInput.GetKeyDown("RotateCC"))||(cInput.GetKey("RotateCC")&&cInput.GetKeyDown("RotateCW"))){
					int prevRotMode=rotationMode;
					if(rotationMode==-2){rotationMode=-3;

					}
					if(rotationMode==-1){rotationMode=-2;
						EnableThirdPerson();
					}
					if(rotationMode>-1){rotationMode=-1;}
					
					if(prevRotMode==-3){rotationMode=0;
						DisableThirdPerson();
					}
				}else{
					if(cInput.GetKeyDown("RotateCW")){
						RotateCW();
					}	
					if(cInput.GetKeyDown("RotateCC")){
						RotateCC();
					}
				}
				if(!Input.GetKey(KeyCode.LeftAlt)&&!Input.GetKey(KeyCode.RightAlt)){
					if(cInput.GetKey("ZoomIn")&&zoomAdjust>.2f){
						SetZoom(zoomAdjust-.013f);
					}	
					if(cInput.GetKey("ZoomOut")&&zoomAdjust<2f){
						SetZoom(zoomAdjust+.013f);
					}
					if(cInput.GetKey("ZoomOut")&&cInput.GetKey("ZoomIn")){
						SetZoom(defaultZoom);
					}
				}else{
					if(cInput.GetKey("ZoomIn")&&adjustDistance<33f){
						SetAdjustDistance(adjustDistance+Time.deltaTime*7f);
					}	
					if(cInput.GetKey("ZoomOut")&&adjustDistance>0f){
						SetAdjustDistance(adjustDistance-Time.deltaTime*7f);
						if(adjustDistance<0f)SetAdjustDistance(0f);
					}
					if(cInput.GetKey("ZoomOut")&&cInput.GetKey("ZoomIn")){
						SetAdjustDistance(adjustDistanceDefault);
					}
				}
			
		}
		if(isMainCamera==1){UpdateRotation();}
		
	}

	public void  LateUpdate (){
		if(fluctThisTurn!=0){
			FluctuateTeleport();
			fluctThisTurn=0;
		}
		if(fluctuateTeleportStart!=0){
			int framesToFlux=37;
			float fluxPercent= (MasterTime.frameNo-fluctuateTeleportStart)/(framesToFlux+.00f);
			thisCamera.ResetProjectionMatrix();
			Matrix4x4 newMatrix=thisCamera.projectionMatrix;
			//skewh
			float wpowAdjust=Mathf.Pow(Mathf.Abs(matrixMod.w)*(1-fluxPercent),matrixModPow.w);
			if(matrixMod.w<0){wpowAdjust=Mathf.Abs(wpowAdjust)*-1.0f;}
			newMatrix[0,1]=newMatrix[0,1]+wpowAdjust;
			//HORIZ
			newMatrix[0,0]=newMatrix[0,0]+Mathf.Pow(Mathf.Abs(matrixMod.x)*(1-fluxPercent),matrixModPow.x);//Mathf.Pow(matrixMod.x,(1-fluxPercent));
			//SKEWv
			float ypowAdjust=Mathf.Pow(Mathf.Abs(matrixMod.y)*(1-fluxPercent),matrixModPow.y);
			if(matrixMod.y<0){wpowAdjust=Mathf.Abs(ypowAdjust)*-1.0f;}
			newMatrix[1,0]=newMatrix[1,0]+ypowAdjust;
			//VERT
			float zpowAdjust=Mathf.Pow(Mathf.Abs(matrixMod.z)*(1-fluxPercent),matrixModPow.z);
			if(matrixMod.z<0){zpowAdjust=Mathf.Abs(zpowAdjust)*-1.0f;}
			newMatrix[1,1]=newMatrix[1,1]+zpowAdjust;
			thisCamera.projectionMatrix=newMatrix;
			if(fluxPercent>1){fluctuateTeleportStart=0;thisCamera.ResetProjectionMatrix();}
		}
		
		
	}
	public void UpdateRotation(){
		//BEGIN CAMERA ZOOM BY SPEED
		//if(isMainCamera==1){SetZoom(zoomAdjust);}
		//fovAdjustment = Mathf.Lerp(fovAdjustment, (Mathf.Sqrt(Mathf.Pow(tehviewer.target.GetComponent<UnitMovement>().vx,2f)+Mathf.Pow(tehviewer.target.GetComponent<UnitMovement>().vz,2f)))*autoZoomAmount,Time.deltaTime*autoZoomSpeed);
		
		//BEGIN CAMERA PAN ADJUST BY PLAYER ANGLE
		float adjDistance= (adjustDistance) * zoomAdjust;
		if(Viewer.spectateMode>0){adjDistance=0f;}
		//debugvar=Time.deltaTime*adjustSmoothing;
		//float xLerped = Mathf.SmoothDamp(transform.localPosition.x,adjDistance*Mathf.Sin(Mathf.Deg2Rad*target.rotation.eulerAngles.y),xVel,adjustSmoothing);
		//float zLerped = Mathf.SmoothDamp(transform.localPosition.z,adjDistance*Mathf.Cos(Mathf.Deg2Rad*target.rotation.eulerAngles.y),zVel,adjustSmoothing);
		float xLerped = transform.localPosition.x;
		float zLerped = transform.localPosition.z;
		Transform tehTarget=tehviewer.target;
		if(!tehviewer.target){
			tehTarget=Viewer.viewerTrans;
		}else{ //dont do adjusting when no target
			if(rotationMode==0){
				xLerped  = Mathf.Lerp(transform.localPosition.x,adjDistance*Mathf.Sin(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
				zLerped = Mathf.Lerp(transform.localPosition.z,adjDistance*Mathf.Cos(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
			}
			if(rotationMode==1){
				xLerped = Mathf.Lerp(transform.localPosition.x,adjDistance*Mathf.Cos(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
				zLerped = Mathf.Lerp(transform.localPosition.z,adjDistance*-Mathf.Sin(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
			}	
			if(rotationMode==2){
				xLerped = Mathf.Lerp(transform.localPosition.x,adjDistance*-Mathf.Sin(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
				zLerped = Mathf.Lerp(transform.localPosition.z,adjDistance*-Mathf.Cos(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
			}
			if(rotationMode==3){
				xLerped = Mathf.Lerp(transform.localPosition.x,adjDistance*-Mathf.Cos(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
				zLerped = Mathf.Lerp(transform.localPosition.z,adjDistance*Mathf.Sin(Mathf.Deg2Rad*tehTarget.rotation.eulerAngles.y),Time.deltaTime*adjustSmoothing);
			}		
			
			if(rotationMode==-1){
				rotationY=tehTarget.transform.eulerAngles.y;
				xLerped=0f;
				zLerped=adjDistance;
			}	
		}		
			
		if(rotationMode==-2){//third person free
			if(cInput.GetKey("CursorAim")){
				 rotationY=rotationY+Input.GetAxis("Mouse X")*2f;
				 rotationElev=rotationElev-Input.GetAxis("Mouse Y")*2f;
			}
			//thirdCamera.transform.localRotation= Quaternion.AngleAxis(rotationY, Vector3.up) *  Quaternion.AngleAxis(rotationElev, Vector3.right);
			thirdCamera.transform.localRotation=Quaternion.AngleAxis(rotationElev, Vector3.right);
			thirdCamera.transform.localPosition=thirdCamera.transform.localRotation*(Vector3.forward*-zoomAdjust*8f);
		}		
		
		if(rotationMode==-3){//third person LOCKED
			if(cInput.GetKey("CursorAim")){
				rotationYLock+=Input.GetAxis("Mouse X")*2f;
				rotationElev=rotationElev-Input.GetAxis("Mouse Y")*2f;
			}
			if(tehTarget!=Viewer.viewerTrans){
				rotationY=rotationYLock+tehTarget.eulerAngles.y;
			}			
			//thirdCamera.transform.localRotation= tehTarget.rotation*(Quaternion.AngleAxis(rotationY, Vector3.up) *  Quaternion.AngleAxis(rotationElev, Vector3.right));
			thirdCamera.transform.localRotation= Quaternion.AngleAxis(rotationElev, Vector3.right);
			thirdCamera.transform.localPosition=thirdCamera.transform.localRotation*(Vector3.forward*-zoomAdjust*8f);
		}
		

		//smooth rotation
		if(rotationMode>-1){
			transform.parent.rotation=Trans.QuatY(Mathf.MoveTowardsAngle(transform.parent.rotation.eulerAngles.y, rotationY, 600* Time.fixedDeltaTime));
		}else{
			//if(rotationMode==-1){
				transform.parent.rotation=Trans.QuatY(rotationY);
			//}else{
				//transform.parent.rotation=Trans.QuatY(0f);
			//}
		}
		foreach(MasterCamera masCam in masterCameras.Values ){
			masCam.transform.localPosition=new Vector3(xLerped,100f,zLerped);
		}

	}
	static public void SetRotationMode(int inc_mode){
		if(inc_mode<-1){
			EnableThirdPerson();
		}else{
			DisableThirdPerson();
			//rotationY=rotationMode*-90f;			
		}
		rotationMode=inc_mode;
		
		
	}
	
	static public void EnableThirdPerson(){
		foreach(KeyValuePair<int,MasterCamera> pair in masterCameras){
			pair.Value.thisCamera.enabled=false;
		}
		thirdCamera.enabled=true;
	}
	static public void DisableThirdPerson(){
		foreach(KeyValuePair<int,MasterCamera> pair in masterCameras){
			pair.Value.thisCamera.enabled=true;
		}
		thirdCamera.enabled=false;
	}

	public void  RotateCW (){
		if(rotationMode>-1){
			if(rotationMode>0){rotationMode--;}else{rotationMode=3;}
			rotationY=rotationMode*-90f;
		}else{
			rotationY+=-90f;rotationYLock+=-90f;
		}
	}
	public void  RotateCC (){
		if(rotationMode>-1){
			if(rotationMode<3){rotationMode++;}else{rotationMode=0;}
			rotationY=rotationMode*-90f;
		}else{
			rotationY+=90f;rotationYLock+=90f;
		}
	}
	
	static public void SetZoom(float inc_float){
		zoomAdjust=inc_float;
		
		foreach(KeyValuePair<int, MasterCamera> valuePair in masterCameras){
			MasterCamera disMasCamera=(MasterCamera)valuePair.Value;
			if(disMasCamera==null) continue;
			Camera disCamera=disMasCamera.thisCamera;
			disCamera.fieldOfView = (disMasCamera.baseFov*zoomAdjust);//+Mathf.Sqrt(fovAdjustment*autoZoomAmount);
			//todo: someday make this not an ugly shit hack
			if(disMasCamera.isMainCamera==0){
				float divideRange=Mathf.InverseLerp(.698f, 2.01f, zoomAdjust);
				float divideZoomBy=Mathf.LerpUnclamped(4.3f,2.53f,divideRange);
				disCamera.fieldOfView+=(1f-zoomAdjust)/divideZoomBy;
			}
			disCamera.orthographicSize = (disMasCamera.baseOrtho*zoomAdjust);//+Mathf.Sqrt(fovAdjustment*autoZoomAmount);
		}
		
	}
	
	static public void SetAdjustDistance(){SetAdjustDistance(adjustDistanceDefault);}
	static public void SetAdjustDistance(float inc_dist,int inc_default=0){
		adjustDistance=inc_dist;
		if(inc_default!=0)	adjustDistanceDefault=inc_dist;
	}
	
	
	static public void SetBackgroundColor(Color inc_color){
		thirdCamera.backgroundColor=inc_color;
		masterCameras[2].thisCamera.backgroundColor=inc_color;

	}


	static public void  FluctuateTeleport (){
		float plusOrMinusW = (Random.Range(0.0f,1.0f) < 0.5f) ? -1.0f : 1.0f;
		float plusOrMinusY = (Random.Range(0.0f,1.0f) < 0.5f) ? -1.0f : 1.0f;
		float plusOrMinusZ = (Random.Range(0.0f,1.0f) < 0.5f) ? -1.0f : 1.0f;
		Vector4 randomExp=new Vector4(Random.Range(5.1f,7.7f),Random.Range(2.1f,3.7f),Random.Range(5.0f,7.0f),Random.Range(2.0f,3.0f));
		for(int cnt= 0; cnt < masterCameras.Count; cnt++){
			MasterCamera thisMasCam=masterCameras[cnt];
			thisMasCam.fluctuateTeleportStart=MasterTime.frameNo;
			thisMasCam.matrixMod.x=thisMasCam.teleFluxMatrix.x;
			thisMasCam.matrixMod.y=thisMasCam.teleFluxMatrix.y*plusOrMinusY;
			thisMasCam.matrixMod.z=thisMasCam.teleFluxMatrix.z*plusOrMinusZ;
			thisMasCam.matrixMod.w=thisMasCam.teleFluxMatrix.w*plusOrMinusW;
			thisMasCam.matrixModPow=randomExp;
		}
	}
	
	static public void Reset(){
		MasterCamera.SetRotationMode(0);
		MasterCamera.rotationY=0f;
		MasterCamera.SetZoom(MasterCamera.defaultZoom);
	}
	
	static public string SaveScreenshot(){
		// File path
		string folderPath = Application.dataPath + "/screenshots/"; 
		
		string fileName = MasterTime.GetEpochTime() + ".png";
		if(!System.IO.Directory.Exists(folderPath)){System.IO.Directory.CreateDirectory(folderPath);}// Create the folder beforehand if not exists
		ScreenCapture.CaptureScreenshot(folderPath + fileName , 2);// Capture and store the screenshot, param 2 is res scale
				
		return folderPath + fileName;
	}

	static public int  IsPointOnScreen ( Vector3 incVec  ){
		Vector3 viewPoint =mainCamera.WorldToViewportPoint(incVec);
		if(viewPoint.x>1.31f||viewPoint.x<-1.31f){return 0;}
		else if(viewPoint.y>1.31f||viewPoint.y<-1.31f){return 0;}
		return 1;
	}
	
	static public float GetPixelWidth(){
		return uiCamera.pixelWidth;
	}		
	static public float GetPixelHeight(){
		return uiCamera.pixelHeight;
	}	
	
	
}