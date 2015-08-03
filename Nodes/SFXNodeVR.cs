using UnityEngine;
using System.Collections;
using Kaae;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSourceVR))]
public class SFXNodeVR : AudioNode, IPlayable, IPlaybackSettings {

	#region IPlaybackSettings implementation
	public void ApplyPlaybackSettings (float volume, float pitch, float pan, float delay)
	{
		nodevolume *= volume;
		nodePitch *= pitch;
		nodePan += pan;
		_audiosource.volume = nodevolume;
		_audiosource.pitch = nodePitch;
		_audiosource.panStereo = nodePan;
	}
	#endregion

	#region IPlayable implementation
	public void Play (float volume, float pitch, float pan, float delay)
	{
		StartFadeIn();
		_audiosource.clip = clip;
		_audiosource.outputAudioMixerGroup = mixerGroupProperties.mixerGroup;
		_audiosource.volume = volume * nodevolume;
		_audiosource.pitch = pitch * nodePitch;
		_audiosource.panStereo = pan + nodePan;
		//apply randomSampleOffset
		if (randomSampleOffset) _audiosource.timeSamples = Random.Range(0,_clipLength); 
		if (!randomSampleOffset) _audiosource.timeSamples = 0;
		if (oneShot)  
			//this doesn't have VR support..yet?
			_audiosource.PlayOneShot(_audiosource.clip);
		else
		{
			if (Application.isPlaying) 
				_audioSourceVR.PlayScheduled(AudioSettings.dspTime + delay + nodeDelay);
			else
				_audiosource.PlayScheduled(AudioSettings.dspTime + delay + nodeDelay);
		}
	}
	

	public void Stop ()
	{
		StartFadeOut();
		//_audioSourceVR.SetScheduledEndTime(fadeOut);
		_audiosource.SetScheduledEndTime(AudioSettings.dspTime + fadeOut);
	}
	#endregion
	
	public AudioClip clip;
	public bool randomSampleOffset;
	[Range(0,15)]
	public float fadeIn;
	[Range(0,15)]
	public float fadeOut;
	public bool loop;
	public bool oneShot;

	int _clipLength;
	public override void OnValidate ()
	{
		base.OnValidate ();
		_audiosource = GetComponent<AudioSource>();
		_audioSourceVR = GetComponent<AudioSourceVR>();
		_audiosource.outputAudioMixerGroup = mixerGroupProperties.mixerGroup;
		_audiosource.clip = clip;
		_audiosource.volume = nodevolume;
		_audiosource.pitch = nodePitch;
		_audiosource.panStereo = nodePan;
		_audiosource.loop = loop;
		_audiosource.dopplerLevel = 0;
		//get lenghth of audioclip TODO: DO THIS IN CUSTOM INSPECTOR, WHEN SAMPLE GETS CHANGED !!!
		if (clip != null) _clipLength = clip.samples;
	}
	
	AudioSource _audiosource;
	AudioSourceVR _audioSourceVR;
	public override void OnEnable()
	{
		base.OnEnable();
		_audiosource = GetComponent<AudioSource>();
		_audioSourceVR = GetComponent<AudioSourceVR>();
		_audiosource.playOnAwake = false;
		if (clip != null) _clipLength = clip.samples;
		counter =0;
	}
	
	bool fadingIn;
	void StartFadeIn()
	{
		timeInSamples = AudioSettings.outputSampleRate * 2 * fadeIn;
		gain = 0;
		counter =0;
		fadingOut = false;
		fadingIn = true;
	}
	
	bool fadingOut;
	void StartFadeOut()
	{
		timeInSamples = AudioSettings.outputSampleRate * 2 * fadeOut;
		//gain = 1;
		counter =0;
		fadingIn = false;
		fadingOut = true;
	}
	
	float timeInSamples;
	double gain;
	int counter;
	void OnAudioFilterRead(float[] data, int channels)
	{
		for (var i = 0; i < data.Length; ++i)
		{
			if (fadingIn)
			{
				gain += 1/timeInSamples;
				counter++;
				if (gain > 1)
				{
					gain = 1;
					fadingIn = false;
				}
			}
			
			if (fadingOut)
			{
				gain -= 1/timeInSamples;
				counter++;
				if (gain < 0)
				{
					gain = 0;
					fadingOut = false;
				}
			}
			
			data[i] = Mathf.Clamp( data[i] * (float)gain, -1,1) ;			
		}
	}

	void Update()
	{
		if (Application.isPlaying)
		{
			if (fadingOut)
			{
				if (gain <= 0) _audioSourceVR.Stop();
			}
		}
	}
	
}

