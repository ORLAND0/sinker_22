using UnityEngine;
using System.Collections;

public class TransTween : MonoBehaviour {
	public Vector3[] positions=new Vector3[0];
	public Vector3[] rotations=new Vector3[0];
	public Vector3[] scales=new Vector3[0];
	[HideInInspector]
	public Vector3 posorig;
	[HideInInspector]
	public Quaternion rotorig;
	[HideInInspector]
	public Vector3 scaleorig;
	[HideInInspector]
	public int keyframes;
	public double tweenInit =0;
	public double tweenDuration =1;
	public double tweenFirstStart =0;//the gametime when this tween first plays. use negative to offset time
	public double tweenStartAfter =-1;//0 or greater sets start time at start()
	public double tweenStarted=0; //the time tw
	public float tweenProgress=0;
	public int tweenFrameOnFinish=1;//0 is original pos 1 is last keyframe
	public int updateOn=1;//1 update, 2 fixed
	public int tweenOnAwake=0;//1 on awake
	public int autoplay =2;//0 use tweenStartAfter; 1 = forward loop; 2 = ping pong loop (order of keyframes)
	public int flipFlop=0;//1 do not lerp between frames;2 do not lerp between last and first frame (also happens on autoplay0)
	public int ease=0; //1 ease out, 2 ease in.
	public int synced=1;//0 startime set on finishing frame,1 starttime based off first start
	public int tweening=-1;//-1 not tweened yet. 0 not tweening; 1 forward; 2 backward.
	public int screenOnly=1;//only spin if on screen
	public int tweenLocal=1;//-1 world pos;0  ;1 local rot / pos; scale not implemented
	public int scaleAdditive=0;//0= orig+keyframe*vector.one; 1 is add to original;2 is mult original
	public int posRelative=0;//0 absolute, 1 origpos+rotorig*pos
	public int startOn=0;//1 StartTween on Start;2 OnEnable;


	public int tind=-1;
	[HideInInspector]
	public Transform thisTransform;
	[HideInInspector]
	public Rigidbody thisRigid;
	
	// Use this for initialization

	private int init = 0;
	
	public void Awake(){
		if(tweenOnAwake==1&&init<1){
			Init();
			StartTween();
		}		
	}
	
	public void Start() {
			if(init<1){Init();}
			if(startOn==1){StartTween();}
			TweenFrame();
	}	

	public void Init(){
			thisTransform=transform;
			if(updateOn==2){thisRigid=GetComponent<Rigidbody>();}
			SetOrig();
			scaleorig=thisTransform.localScale;
			if(tweenStartAfter>=0){
				tweenFirstStart=MasterTime.gameTime+tweenStartAfter;
				tweenStarted=tweenFirstStart;
			}
			if(autoplay>0){
				tweenStarted=GetRecentStartTime(MasterTime.gameTime);
			}
			
			SetKeyframes();
			init++;
	}	
	
	// Update is called once per frame
	void Update () {
		if(updateOn!=1) return;
		TweenFrame();	

	}	
	void FixedUpdate () {
		if(updateOn!=2||init<1) return;
		TweenFrame();	

	}
	
	public void OnEnable() {
		if(startOn==2){StartTween();}
	}
	
	public void TweenFrame(){
		if(screenOnly==1){
			if(Viewer.IsPositionOnScreen(thisTransform.position)<1)return;
		}
		int wasTweening=tweening;
		tweenProgress= GetTweenProgress();
		
		if(tweenProgress>=1f){			
			if(autoplay==0){
				if(tweening>0){
					FinishTween();
				}
				return;
			}
			if(autoplay>0){
				tweening=1;
				tweenStarted=GetRecentStartTime(MasterTime.gameTime);
				tweenProgress= GetTweenProgress();
			}

		}
		if(tweenProgress<0f){
			if(autoplay>0){
				tweening=1;
				tweenStarted=GetRecentStartTime(MasterTime.gameTime);
				tweenProgress= GetTweenProgress();
			}
		}
		if(tweenFirstStart>MasterTime.gameTime){
			tweenProgress=-1; //CANCEL
		}
		
		if(tweenProgress<1f&&tweenProgress>=0f){ //tweening 
			tweening=1;
			if(wasTweening==-1){ //2024 big change
				SetOrig();
			}
			if(ease==2){tweenProgress=tweenProgress * tweenProgress;}
			if(ease==1){tweenProgress=2 * tweenProgress * (1f - (tweenProgress * 0.5f));}
			//if(tweenProgress>1f){tweenProgress=1f;}
			//if(tweenProgress<0f){tweenProgress=0f;}	
			tind=0;int tindb=1;int step=0;int steps=1;
			if(autoplay!=2){
				steps=keyframes;
				if(flipFlop==2||autoplay==0){steps=keyframes-1;}
				step=tind = (int) Mathf.Floor((float)(tweenProgress*steps));
				tindb=tind+1;
				if(tindb>=keyframes){tindb=0;}//this shouldnt happen.. idk used to be >=steps
			}
			if(autoplay==2){
				steps=((keyframes-1)*2);

				step=tind= (int) Mathf.Floor((float)(tweenProgress*(float)steps));
				if(step>=keyframes){//looping backwards
					tind=(keyframes-1)-(step-(keyframes-1));
					tindb=tind-1;
				}
				if(step==keyframes-1){
					tindb=tind-1;
				}
				if(step<keyframes-1){
					tindb=tind+1;
				}
			}
			if(flipFlop==1){
				tindb=tind;
			}
			//Debug.Log("s:"+step+" ta:"+tind+" tb:"+tindb);
			float percenteachiteration=1f/steps;
			float percenta=((float)step)*percenteachiteration;//the percent progress of tinda
			//float percentb=tindb*percenteachiteration;
			float stepProgress= (tweenProgress-percenta) / percenteachiteration;
			//Debug.Log(stepProgress);
			
			if(keyframes==positions.Length){
				Vector3 tweenedPos = Vector3.Lerp(positions[tind],positions[tindb],stepProgress);
				if(posRelative==1){tweenedPos=transform.rotation*tweenedPos;}
				if(tweenLocal==1){
					tweenedPos=(posorig+tweenedPos);
					transform.localPosition=tweenedPos;
				}else{
					if(tweenLocal==-1){tweenedPos=tweenedPos;}
					else{tweenedPos=posorig+tweenedPos;}	
					if(thisRigid!=null&&!thisRigid.isKinematic){thisRigid.position=tweenedPos;}
					else{transform.position=tweenedPos;}
				}
			
			}				
			if(keyframes==rotations.Length){
				Quaternion tweenedRot;
				if(tind!=tindb){
					tweenedRot=Quaternion.Lerp(UnitRotation.QuatEul(rotations[tind]),UnitRotation.QuatEul(rotations[tindb]),stepProgress);
				}else{
					tweenedRot=UnitRotation.QuatEul(rotations[tind]);
				}
				Quaternion newQuat=new Quaternion();
				newQuat.eulerAngles=rotorig.eulerAngles+tweenedRot.eulerAngles;
				if(tweenLocal==1){
					if(thisRigid!=null){
						thisRigid.rotation=thisTransform.parent.localRotation*newQuat;
					}else{
						thisTransform.localRotation=newQuat;
					}
				}else{
					if(thisRigid!=null){
						thisRigid.rotation=newQuat;
					}else{
						thisTransform.rotation=newQuat;
					}
				}
			}						
			if(keyframes==scales.Length){
					Vector3 tweenedScale = Vector3.Lerp(scales[tind],scales[tindb],stepProgress);
					if(scaleAdditive==1){//add scales to original scale
						thisTransform.localScale=new Vector3(scaleorig.x+tweenedScale.x,scaleorig.y+tweenedScale.y,scaleorig.z+tweenedScale.z);
					}
					if(scaleAdditive==0){//multiply original scale by Vector.one + keyframe vector
						thisTransform.localScale=new Vector3(scaleorig.x*(tweenedScale.x+1f),scaleorig.y*(tweenedScale.y+1f),scaleorig.z*(tweenedScale.z+1f));
					}
					if(scaleAdditive==2){//multiply original scale by orig * keyframe vector
						thisTransform.localScale=new Vector3(scaleorig.x*tweenedScale.x,scaleorig.y*tweenedScale.y,scaleorig.z*tweenedScale.z);
					}
			}
		}

	}
	
	public void StartTween(){
		tweening=1;
		tweenStarted=GetRecentStartTime(MasterTime.gameTime);
		TweenFrame();
	}
	
	public void FinishTween(){
		if(tweenFrameOnFinish==0){
			if(keyframes==positions.Length){
				thisTransform.localPosition=posorig;
			}				
			if(keyframes==rotations.Length){
				Quaternion newQuat=new Quaternion();
				newQuat=rotorig;
				thisTransform.localRotation=newQuat;
			}			
			if(keyframes==scales.Length){
				thisTransform.localScale=scaleorig;
			}
			enabled=false;
		}
		tweening=0;
	}
	
	public void SetKeyframes(){ //whichever type of modifier array has most keys will become the keyframe amount, lessers will be ignored. 
		if(positions!=null&&positions.Length>keyframes){keyframes=positions.Length;}
		if(rotations!=null&&rotations.Length>keyframes){keyframes=rotations.Length;}
		if(scales!=null&&scales.Length>keyframes){keyframes=scales.Length;}
		if(keyframes==0){keyframes=-1;}//prevent errors for uninitialized instances
		
	}
	
	public void SetOrig(){
			if(tweenLocal==1){
				posorig=thisTransform.localPosition;
				rotorig=thisTransform.localRotation;
			}else{
				posorig=thisTransform.position;
				rotorig=thisTransform.rotation;
			}
		
	}
	
	
	public float GetTweenProgress(){
		float percent = (float)((MasterTime.gameTime-tweenStarted)/(tweenDuration));
	
		return percent;
	}
	
	public double GetRecentStartTime(double inc_time){
		if(synced==0){return MasterTime.gameTime;}
		int loops = (int) Mathf.Floor((float)((inc_time-tweenFirstStart)/(tweenDuration)));//how many times has it already played
		return (loops*tweenDuration)+tweenFirstStart;//timescal
	}
}
