using UnityEngine;
using System.Collections;

public class AudioSourceVR : OSPAudioSource {

	

	/// <summary>
	/// Stop this instance after time.
	/// </summary>
	public void SetScheduledEndTime(double time)
	{
		StartCoroutine(WaitForScheduledEndTime((float)time));
	}
	
	IEnumerator WaitForScheduledEndTime(float scheduledEndTime)
	{
		yield return new WaitForSeconds(scheduledEndTime);
		Stop ();
	}
}
