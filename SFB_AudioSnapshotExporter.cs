using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine.Audio;

// This script is designed to export all of the snapshots from an Audio Mixer into individual tracks.
// It could be used, for instance, to export all versions of a song that have been mastered into different
// snapshots.

public class SFB_AudioSnapshotExporter : MonoBehaviour {
    [ContextMenuItem("Export Snapshots", "SaveNow")]
	public AudioMixer audioMixer;
	public AudioMixerSnapshot[] audioSnapshots;
	public AudioMixerGroup[] audioGroups;
	public List<AudioClips> audioClips = new List<AudioClips>();
	public List<string> exposedParams;
	public List<string>[] groupEffects;

    static float rescaleFactor = 32767; //to convert float to Int16

    [ContextMenu("Export Snapshots")]
    public void SaveNow()
    {

		//Debug.Log ("Saving Snapshots from " + audioMixer.name);

		audioGroups = audioMixer.FindMatchingGroups ("Master");

		//Debug.Log ("There are " + audioGroups.Length + " Audio Groups");

		for (int g = 0; g < audioGroups.Length; g++) {
			//Debug.Log(audioGroups[g].name);
		}

		/*
		PropertyInfo[] groupPropInf = audioGroups.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		MemberInfo[] groupMemberInf = audioGroups.GetType().GetMembers(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		FieldInfo[] groupFieldInf = audioGroups.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

		Debug.Log ("groupPropInf: " + groupPropInf.Length);
		for (int pi = 0; pi < groupPropInf.Length; pi++) {
			Debug.Log ("groupPropInfo[" + pi + "]: " + groupPropInf [pi]);
		}
		Debug.Log ("groupMemberInf: " + groupMemberInf.Length);
		for (int mi = 0; mi < groupMemberInf.Length; mi++) {
			Debug.Log ("groupMemberInf[" + mi + "]: " + groupMemberInf [mi]);
		}
		Debug.Log ("groupFieldInf: " + groupFieldInf.Length);
		for (int fi = 0; fi < groupFieldInf.Length; fi++) {
			Debug.Log ("groupFieldInf[" + fi + "]: " + groupFieldInf [fi]);
		}*/


		groupEffects = new List<string>[audioGroups.Length];
		for (int x = 0; x < audioGroups.Length; x++) {
			Debug.Log ("AudioGroup " + audioGroups[x].name);
			groupEffects[x] = new List<string>();
			Array effects = (Array)audioGroups[x].GetType().GetProperty("effects").GetValue(audioGroups[x], null);
			for(int i = 0; i< effects.Length; i++)
			{
				var o = effects.GetValue(i);
				string effect = (string)o.GetType().GetProperty("effectName").GetValue(o, null);
				groupEffects[x].Add(effect);
				Debug.Log(effect);
			}
		}

		LoadAudioClips();

		/*
		PropertyInfo[] PropInf = audioMixer.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		MemberInfo[] MemberInf = audioMixer.GetType().GetMembers(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		FieldInfo[] FieldInf = audioMixer.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

		Debug.Log ("PropInf: " + PropInf.Length);
		for (int pi = 0; pi < PropInf.Length; pi++) {
			Debug.Log ("PropInf[" + pi + "]: " + PropInf [pi]);
		}
		Debug.Log ("MemberInf: " + MemberInf.Length);
		for (int mi = 0; mi < MemberInf.Length; mi++) {
			Debug.Log ("MemberInf[" + mi + "]: " + MemberInf [mi]);
		}
		Debug.Log ("FieldInf: " + FieldInf.Length);
		for (int fi = 0; fi < FieldInf.Length; fi++) {
			Debug.Log ("FieldInf[" + fi + "]: " + FieldInf [fi]);
		}
		*/

		//Exposed Params
		Array parameters = (Array)audioMixer.GetType().GetProperty("exposedParameters").GetValue(audioMixer, null);

		//Debug.Log("----ExposedParams (" + parameters.Length + ")----------------------------------------------------");
		for(int i = 0; i< parameters.Length; i++)
		{
			var o = parameters.GetValue(i);
			string Param = (string)o.GetType().GetField("name").GetValue(o);
			exposedParams.Add(Param);
			//Debug.Log(Param);
		}

		audioSnapshots = (AudioMixerSnapshot[])audioMixer.GetType().GetProperty("snapshots").GetValue(audioMixer, null);

		/*
		PropertyInfo[] snapPropInf = audioSnapshots.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		MemberInfo[] snapMemberInf = audioSnapshots.GetType().GetMembers(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		FieldInfo[] snapFieldInf = audioSnapshots.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

		Debug.Log ("snapPropInf: " + snapPropInf.Length);
		for (int pi = 0; pi < snapPropInf.Length; pi++) {
			Debug.Log ("snapPropInf[" + pi + "]: " + snapPropInf [pi]);
			Debug.Log(System.String.Format("{0} = {1}", snapPropInf [pi].Name, snapPropInf [pi].PropertyType));
		}
		Debug.Log ("snapMemberInf: " + snapMemberInf.Length);
		for (int mi = 0; mi < snapMemberInf.Length; mi++) {
			Debug.Log ("snapMemberInf[" + mi + "]: " + snapMemberInf [mi]);
			Debug.Log(System.String.Format("{0} = {1}", snapMemberInf [mi].Name, snapMemberInf [mi].MemberType));
		}
		Debug.Log ("snapFieldInf: " + snapFieldInf.Length);
		for (int fi = 0; fi < snapFieldInf.Length; fi++) {
			Debug.Log ("snapFieldInf[" + fi + "]: " + snapFieldInf [fi]);
			Debug.Log(System.String.Format("{0} = {1}", snapFieldInf [fi].Name, snapFieldInf [fi].FieldType));
		}
*/

		//Debug.Log ("There are " + audioSnapshots.Length + " Audio Snapshots");

		for(int i = 0; i< audioSnapshots.Length; i++)
		{
			//Debug.Log(audioSnapshots[i].name);
		}

		// EXPORTING
		// 1.  For each snapshot...
		// 2.  Export the track by...
		// 3.  Adding each clip by group, using the volume of that group/snapshot



		for (int s = 0; s < audioSnapshots.Length; s++) {
			if (s == 0) { // TODO:  Remove this once it's all working
				// For each snapshot...
				string fixedSnapshotName = audioSnapshots [s].name.Replace (" (" + audioMixer.name + ")", "");		// Remove the " ([name])" from the snapshot name
				float currentProgress = s / audioSnapshots.Length;
				EditorUtility.DisplayProgressBar ("Exporting " + audioMixer.name + " Snapshots", fixedSnapshotName + " (" + (s + 1) + "/" + audioSnapshots.Length + ")", currentProgress);
				string filename = audioMixer.name + "_" + fixedSnapshotName + ".wav";								// Set up filename
				filename = filename.Replace (" ", "");																// remove spaces
				string filepath	= "Assets/SFBayStudios/Exported Music/" + audioMixer.name + "/" + filename;			// Full Path & Name
				Directory.CreateDirectory (Path.GetDirectoryName (filepath));											// Create Directory if it doesn't exist
				Debug.Log ("Exporting " + filename);

				using (var fileStream = CreateEmpty (filepath)) {														// Create an empty file
					// This is the "ConvertAndWrite()" from the original script

					int mostSamples = 0;																				// Setup variable
					foreach (AudioClips audioClip in audioClips) {
						audioClip.GetSamplesMusic ();																// Run this function from the class 
						mostSamples = Mathf.Max (mostSamples, audioClip.sampleCount);								// Set mostSamples to the greatest one
					}
					Debug.Log ("mostSamples: " + mostSamples);




					Int16[] finalSamples = new Int16[mostSamples];										// The exported clip will have the mostSamples
					for (int i = 0; i < mostSamples; i++) {												// for each sample
						float sampleValue = 0;															// Set variable for exported clip
						int sampleCount = 0;															// Set variable

						foreach (AudioClips audioClip in audioClips) {									// For each clip....
							if (i < audioClip.sampleCount) {												// if we are under the samplecount for the clip
								// Add the value from this layer to the final (sampleValue)
								sampleValue += (audioClip.samples [i] / rescaleFactor);
								sampleCount++;
							}
						}

						if (sampleCount != 0)																// If we have done some samples (keep from dividing by 0)
						sampleValue /= sampleCount;													// compute sampleValue
						finalSamples [i] = (short)(sampleValue * rescaleFactor);
					}


					Byte[] bytesData = ConvertSamplesToBytes (finalSamples);
					fileStream.Write (bytesData, 0, bytesData.Length);

					WriteHeader (fileStream, audioClips [0].clip, mostSamples);

					// End ConvertAndWrite()
				}


				AssetDatabase.ImportAsset (filepath);
			}
		}

		
		EditorUtility.ClearProgressBar();
		Debug.Log ("Done!");
    }

	public void LoadAudioClips(){
		audioClips.Clear();
		foreach (Transform child in transform)
		{
			//Debug.Log ("Child: " + child.gameObject.name);
			AudioSource audioSource = child.gameObject.GetComponent<AudioSource>();
			AudioMixerGroup audioGroup = audioSource.outputAudioMixerGroup;
			//Debug.Log ("Group: " + audioGroup.name);

			for (int g = 0; g < audioGroups.Length; g++) {
				if (audioGroups [g].name == audioGroup.name) {
					//Debug.Log ("Found a Match!");
					//List<AudioClips> audioClips = new List<AudioClips>();
					audioClips.Add(new AudioClips(audioSource.clip, g));
				}
			}
		}
	}

    const int HEADER_SIZE = 44;

	[System.Serializable]
	public class AudioClips
	{
		public AudioClip clip;												// The audio clip we are using
		public int groupID;													// The group ID the clip is attached to
		public float[] snapshotVolume;										// The volume settings for each snapshot (0.0 - 1.0)
		[HideInInspector]
		public Int16[] samples;
		[HideInInspector]
		public Byte[] bytes;
		[HideInInspector]
		public int sampleCount;

		public AudioClips(AudioClip newClip, int newGroup)					// Function to add a new entry
		{
			clip = newClip;
			groupID = newGroup;
		}

		public void GetSamplesMusic()
		{
			// TO DO -- The 2nd value here needs to be the volume.
			samples = GetSamplesFromClip(clip, 1.0f);
			sampleCount = samples.Length;
		}
	}
		
    static Byte[] ConvertSamplesToBytes(Int16[] samples)
    {
        Byte[] bytesData = new Byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(samples[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }

	static Int16[] GetSamplesFromClip(AudioClip clip, float volume)
    {
		Debug.Log ("Getting Samples from clip " + clip.name);
        var samples = new float[clip.samples * clip.channels];
		Debug.Log ("Samples: " + samples.Length);

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        
		Int16 positiveInt = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * volume * rescaleFactor);
			positiveInt += intData [i];
        }
        return intData;
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip, int sampleCount)
    {
        var frequency = clip.frequency;
        var channelCount = clip.channels;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        //UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channelCount);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(frequency);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(frequency * channelCount * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channelCount * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

		//Byte[] subChunk2 = BitConverter.GetBytes(sampleCount * channelCount * 2);
        Byte[] subChunk2 = BitConverter.GetBytes(sampleCount * channelCount * 1);
        fileStream.Write(subChunk2, 0, 4);

        //		fileStream.Close();
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }



}
