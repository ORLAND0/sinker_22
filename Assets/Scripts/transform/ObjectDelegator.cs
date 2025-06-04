using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//InputsDelegateOutput
public delegate void DelegateInt(int incInt);
public delegate void DelegateObject(object incOb=default(object));
public delegate void DelegateGameObject(GameObject incOb=default(GameObject));
public delegate void DelegateString(string incString);//why would params need a name here?
public delegate void DelegateStringInt(string incString, int incInt);//why would params need a name here?
public delegate void DelegateBool(bool incBool = false);
public delegate void DelegateDictionary(Dictionary<string,object> incDict = default(Dictionary<string,object>));

//public delegate int DelegateGameObjectInt(GameObject incOb);
public delegate int IntDelegateGameObject(GameObject incOb=default(GameObject));
public delegate float FloatDelegateGameObjectFloatString(GameObject incOb=default(GameObject),float incFloat=0f,string incString ="");//
public delegate Vector3 VectorDelegateVector(Vector3 incVec=new Vector3());
public delegate Vector3 VectorDelegateVectorVector(Vector3 incVec=new Vector3(),Vector3 incVecB=new Vector3());
public delegate Vector3 VectorDelegateGameObject(GameObject incOb=default(GameObject));
public delegate Vector3 VectorDelegateObject(object incOb=default(object));

public class ObjectDelegator : MonoBehaviour {
	
	//static public List<IEnumerator> delayCoroutines=new List<IEnumerator>();
	//static public List<ObjectDelegator> delegators=new List<ObjectDelegator>();
	
	public DelegateGameObject callback;
	
	public int invokeOnStart=0;
	public int invokeOnDestroy=0;//1 OnDest, 2 Destroying
	public int invokeOnCollision=0;//1 is enter,2 is enter and stay
	[HideInInspector]
	public int passCollider=1;//1 is passes the collider, 0 passes this object
	
	
	// Use this for initialization
	
	//void Awake(){
	//	delegators.Add(this);
	//}
	void Start () {
		if(invokeOnStart>0)
			InvokeMe(gameObject);

	}

	void Destroying(){
		if(invokeOnDestroy==2){
			if(	MasterGame.isQuitting>0){return;}
			InvokeMe(gameObject);
		}
	}	
	
	void OnDestroy(){
		if(invokeOnDestroy==1){
			if(	MasterGame.isQuitting>0){return;}
			InvokeMe(gameObject);
		}
		//delegators.Remove(this);
	}
	
	
	// Update is called once per frame
	//void Update () {	}
	
	public void StopAll(){
		//MasterGame.thisScript.StopAllCoroutines();
		//foreach(ObjectDelegator deleg in delegators){if(deleg==null)continue;
		//	deleg.StopAllCoroutines();
		//}
	}
	
	void InvokeMe(GameObject incOb){
		if(callback!=null){
			callback(incOb);
		}
	}
	static public Coroutine DelegateDelay(DelegateGameObject inc_callback,float inc_sec){
		return MasterGame.thisScript.StartCoroutine(ObjectDelegator.DelegateDelayer(inc_callback,inc_sec));

	}	
	static public Coroutine DelegateDelay(DelegateGameObject inc_callback,float inc_sec,GameObject inc_object){//default seemed to work on zone but not in standard assets folder
		return MasterGame.thisScript.StartCoroutine(ObjectDelegator.DelegateDelayer(inc_callback,inc_sec,inc_object ));
	}

	static public IEnumerator DelegateDelayer(DelegateGameObject inc_callback,float inc_sec,GameObject inc_object = default(GameObject)) {
		yield return new WaitForSeconds(inc_sec);
		inc_callback(inc_object);		
	}
}
