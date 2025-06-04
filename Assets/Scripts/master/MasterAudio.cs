// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class MasterAudio: MonoBehaviour {

	//play an audio clip child of viewer transform
	static public AudioSource Play(AudioClip clip ,   float volume=.5f  ,    float pitch=1f ,   int prior=130,Vector3 incPos=default(Vector3) ){
			if(clip==null||MasterConnect.isDedicatedServer>0)return default(AudioSource);
			
			GameObject go= new GameObject("MasterAudioSpawn");
			go.transform.parent=Viewer.listenerTrans;
			AudioSource source= go.AddComponent<AudioSource>();//Create the source
			source.transform.localPosition = incPos;
			source.gameObject.name = clip.name;
			source.rolloffMode=AudioRolloffMode.Linear;
			//source.minDistance=4.667f;
			//source.maxDistance=57f;
			source.clip = clip;
			source.volume = volume;
			source.pitch = pitch;
			source.spatialBlend=1f;
			source.dopplerLevel=0f;
			source.Play();
			Destroy(source.gameObject, ((clip.length)/Mathf.Abs(pitch))+.333f);//destroy after length of clip is played
			return source;
		
	}



	//TODO. array of parameters
	static public AudioSource  PlayAt ( AudioClip clip  ,   Vector3 incPos=default(Vector3)  ,   float volume=1f  ,    float pitch=1f ,   int prior=132  ){
			//hack to help sound performance. should really check BEFORE calling this script though. ?
			if(clip==null||Viewer.IsPositionOnScreen(incPos)<1||MasterConnect.isDedicatedServer>0){return default(AudioSource);}
			
			//Create the source
			AudioSource source = NewAudioSource();
			source.transform.localPosition = incPos;
			source.gameObject.name = clip.name;
			source.rolloffMode=AudioRolloffMode.Linear;
			source.minDistance=4.667f;
			source.maxDistance=57f;
			source.clip = clip;
			source.volume = volume;
			source.pitch = pitch;
			source.spatialBlend=1f;
			source.dopplerLevel=0f;
			source.Play();
			Destroy(source.gameObject, ((clip.length)/Mathf.Abs(pitch))+.337f);//destroy after length of clip is played
			return source;
		
	}
	

	static public AudioSource  NewAudioSource (){
			//Create an empty game object
			GameObject go= new GameObject("MasterAudioSpawn");
			go.transform.parent=MasterGame.soundsTrans;
	 
			//Create the source
			AudioSource source= go.AddComponent<AudioSource>();
			return source;
		
	}
}