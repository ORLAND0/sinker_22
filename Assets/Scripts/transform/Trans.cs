using UnityEngine;
using System.Collections;

public class Trans : MonoBehaviour {
	
	public static void Localize(GameObject inc_child,Transform inc_parent){
		inc_child.transform.parent=inc_parent;
		Localize(inc_child);
	}
	public static void Localize(GameObject inc_child,GameObject inc_parent){
		inc_child.transform.parent=inc_parent.transform;
		Localize(inc_child);
	}
	public static void Localize(GameObject inc_gob){
		Localize(inc_gob.transform);
	}	
	public static void Localize(Transform inc_trans){
		inc_trans.localPosition=Vector3.zero;
		inc_trans.localRotation=Quaternion.identity;
		inc_trans.localScale=Vector3.one;
		//if(inc_trans.parent){
			//inc_trans.gameObject.layer=inc_trans.parent.gameObject.layer;  
		//}
	}
	
	public static  float  RelYRot ( float incYrot  ){ //wtf
		if(incYrot>180){return (-360+incYrot);}
		return incYrot;
	}
	public static float ClampAngle(float angle) {
		 if(angle < 0f)
			 return angle + (360f * Mathf.Ceil(-angle / 360f));
		 else if(angle > 360f)
			 return angle - (360f * (int) (angle / 360f));
		 else
			 return angle;
	}	
	public static float ClampRad(float rad) {
		float twopi=Mathf.PI*2f;
		 if(rad < 0f)
			 return rad + (twopi * Mathf.Ceil(-rad / twopi));
		 else if(rad > twopi)
			 return rad - (twopi * (int) (rad / twopi));
		 else
			 return rad;
	}
	
	 public static Vector3 RotateVector(Vector3 point, float angle){
		 return QuatY(angle) * point;
	 }	 	
	 public static Vector3 RotateVector(Vector3 point, Quaternion angle){
		 return angle * point;
	 }	 
	 public static Vector3 RotateVectorAround(Vector3 point, Quaternion angle , Vector3 pivot){
		 return angle * ( point - pivot) + pivot;
	 }

	public static Quaternion QuatY(float inc_float){
		//if(inc_float>179.99f){inc_float=inc_float*-1f;}
		inc_float=ClampAngle(inc_float);
		Quaternion newQuat=Quaternion.identity;
		newQuat.eulerAngles=new Vector3(0f,inc_float,0f);
		return newQuat;
	}		
	public static Quaternion QuatEul(Vector3 inc_vec){
		return QuatEul(inc_vec.x,inc_vec.y,inc_vec.z);
	}
	public static Quaternion QuatEul(float inc_x,float inc_y,float inc_z){
		Quaternion newQuat=Quaternion.identity;
		newQuat.eulerAngles=new Vector3(inc_x,inc_y,inc_z);
		return newQuat;
	}	
	public static Quaternion QuatAddY(Quaternion inc_quat,float inc_float){
		Quaternion newQuat=inc_quat;
		newQuat.eulerAngles=new Vector3(newQuat.eulerAngles.x,newQuat.eulerAngles.y+inc_float,newQuat.eulerAngles.z);
		return newQuat;
	}
	
	public static Vector3 AngleToVector(float inc_angle){ //convert angle to unit circle coordinates on x/z planes
		return (new Vector3(Mathf.Sin(Mathf.Deg2Rad*inc_angle),0f,Mathf.Cos(Mathf.Deg2Rad*inc_angle)));
	}
	
	public static Vector3 RandomDirection(){ //return random point on sphere
         double x0 = -1.0 + Random.value*2.0;
         double x1 = -1.0 + Random.value*2.0; 
         double x2 = -1.0 + Random.value*2.0;
         double x3 = -1.0 + Random.value*2.0; 
         while(x0*x0 + x1*x1 + x2*x2 + x3*x3 >= 1){
             x0 = -1.0 + Random.value*2.0;
             x1 = -1.0 + Random.value*2.0; 
             x2 = -1.0 + Random.value*2.0;
             x3 = -1.0 + Random.value*2.0;
         } 
         double a = x0*x0+x1*x1+x2*x2+x3*x3;
         double x = 2*(x1* x3+x0*x2)/a;    
         double y = 2*(x2*x3-x0*x1)/a;    
         double z = (x0*x0 + x3*x3 - x1*x1 - x2*x2)/a;
         return new Vector3((float)x, (float)y, (float)z);
     }
	 
	static public int ParseInt(object inc_object){
		if(inc_object is int){	return (int) inc_object;}
		if(inc_object is float){return (int)(float)inc_object;}
		if(inc_object is double){return (int)(double)inc_object;}
		if(inc_object is string){
			int tryInt=0;
			int.TryParse((string)inc_object,out tryInt);
			return tryInt;
		}
		return 0;
	}
	static public float ParseFloat(object inc_object){		
		if(inc_object is float){return (float)inc_object;}	
		if(inc_object is double){return (float)(double)inc_object;}
		if(inc_object is int){	return (float)(int)inc_object;}
		if(inc_object is string){	float tryInt=0;
			float.TryParse((string)inc_object,out tryInt);
			return tryInt;
		}
		return 0f;
	}	
	
	
}

public static class GameObjectExtension {
	 
	public static void SetLayer(this GameObject parent, string layer_name){
		parent.SetLayerChildren(layer_name);
	}
	public static void SetLayerChildren(this GameObject parent, string layer_name){
		parent.SetLayerChildren(MasterGame.GetLayer(layer_name));
	}
    public static void SetLayerChildren(this GameObject parent, int layer)
    {
        parent.layer = layer;
		foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;
		}

    }
	public static void Localize(this GameObject gob,GameObject inc_parent){
		gob.Localize(inc_parent.transform);
	}
	public static void Localize(this GameObject gob,Transform inc_parent){
		gob.transform.parent=inc_parent;
		gob.Localize();
	}
	public static void Localize(this GameObject parent){
		Trans.Localize(parent.transform);
	}
	

	public static bool IsDestroyed(this GameObject go){
		 // UnityEngine overloads the == opeator for the GameObject type
		 // and returns null when the object has been destroyed, but 
		 // actually the object is still there but has not been cleaned up yet
		 // if we test both we can determine if the object has been destroyed.
		 return go == null && !ReferenceEquals(go, null);
	}
	
	public static bool IsNullOrDestroyed(this GameObject gameObject) {object obj=gameObject as object;
		return obj == null || ((obj is UnityEngine.Object) && (UnityEngine.Object)obj == null);
	}
	
	static public int ParseInt(this MonoBehaviour thisMono, object inc_object){
		if(inc_object is int){	return (int) inc_object;}
		if(inc_object is float){return (int)(float)inc_object;}
		if(inc_object is double){return (int)(double)inc_object;}
		if(inc_object is string){
			int tryInt=0;
			int.TryParse((string)inc_object,out tryInt);
			return tryInt;
		}
		return 0;
	}
  
}


public class StringHelp : MonoBehaviour {
    static public string CapitalFirst(string inc_string){
      string str = inc_string;
      if (str.Length == 0){
        return"";
	  }else if (str.Length == 1){
        return (char.ToUpper(str[0])).ToString();
      }else{
        return char.ToUpper(str[0]) + str.Substring(1);
	  }
    }
}

