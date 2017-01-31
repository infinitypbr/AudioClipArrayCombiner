using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class SFB_AudioClipArrayCombiner : MonoBehaviour {
    [ContextMenuItem("Export Combined Audio Clips", "SaveNow")]
    public string outputName;
    public AudioLayer[] audioLayers;

    static float rescaleFactor = 32767; //to convert float to Int16

    // Use this for initialization
    void Start () {
        //Save(outputName, audioLayers);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [ContextMenu("Export now")]
    void SaveNow()
    {
		int totalLayers		= audioLayers.Length;
		int totalClips		= 0;
		int totalExports	= 1;

		for(int i = 0; i < totalLayers; i++)
		{
			totalClips 		+= audioLayers[i].clip.Length;
			totalExports 	*= audioLayers [i].clip.Length;
		}

		Debug.Log ("Total Layers / Clips / Exports: " + totalLayers + " / " + totalClips + " / " + totalExports);

		// TO DO:
		// Create an array that has every combination possible.
		// I'm thinking a String[] with a comma dilimitaor, such as "0,0,0", "0,3,1", "3,2,0,1,4" etc.
		// Then "SaveClip" (* Needs to be modified), can be called for each entry in String[]
		// Explode can be used to get the ID# for the clip to use in each layer.

		// WHAT I DO NOT KNOW:
		// I'm not sure how to get every combination, with an arbitrary number of layers and clips per layer.

        //Save(outputName, audioLayers);
    }

    const int HEADER_SIZE = 44;

    [System.Serializable]
    public class AudioLayer
    {
        public string name;
        public AudioClip[] clip;
		public int clipNumber = 0;
        public bool record = true;
        public float volume = 1;
        public float delay;
        [HideInInspector]
        public Int16[] samples;
        [HideInInspector]
        public Byte[] bytes;
        [HideInInspector]
        public int sampleCount;
        [HideInInspector]
        public int delayCount;

		public void GetSamples(int clipNumber)
        {
            samples =  GetSamplesFromClip(clip[clipNumber], volume);
			delayCount = (int)(delay * clip[clipNumber].frequency * clip[clipNumber].channels);
            sampleCount = delayCount + samples.Length;
        }
    }

	public static bool SaveClip(string filename, AudioLayer[] audioLayers, int layerNumber, int clipNumber, int exportNumber)
	{
		if (filename.Length <= 0)
			filename = "OutputAudio" + exportNumber;
		else {
			filename = filename + "" + exportNumber;
		}
		if (!filename.ToLower().EndsWith(".wav"))
		{
			filename += ".wav";
		}

		var filepath	= "Assets/SFBayStudios/Exported Audio Files/" + filename;

		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));

		using (var fileStream = CreateEmpty(filepath))
		{
			int sampleCount = ConvertAndWrite(fileStream, audioLayers);

			//	 ClIP NUMBER CHANGE HERE
			WriteHeader(fileStream, audioLayers[0].clip[0], sampleCount);
		}
		AssetDatabase.ImportAsset(filepath);
		return true; // TODO: return false if there's a failure saving the file
	}

    public static bool Save(string filename, AudioLayer[] audioLayers)
    {
        if (filename.Length <= 0)
            filename = "OutputAudio";
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

		//var filepath = Path.Combine(Application.dataPath + "/SFBayStudios/Exported Audio Files/", filename);
		var filepath	= "Assets/SFBayStudios/Exported Audio Files/" + filename;
        Debug.Log(filepath);

        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {

            int sampleCount = ConvertAndWrite(fileStream, audioLayers);

			//	 ClIP NUMBER CHANGE HERE
            WriteHeader(fileStream, audioLayers[0].clip[0], sampleCount);
        }
		AssetDatabase.ImportAsset(filepath);
        return true; // TODO: return false if there's a failure saving the file
    }

    static int ConvertAndWrite(FileStream fileStream, AudioLayer[] audioLayers)
    {
        int mostSamples = 0;

        foreach(var audioLayer in audioLayers)
        {
            if (!audioLayer.record)
                continue;
            audioLayer.GetSamples(0);
            mostSamples = Mathf.Max(mostSamples, audioLayer.sampleCount);
        }
        
        Int16[] finalSamples = new Int16[mostSamples];
        Debug.Log("Most sample: " + mostSamples);
        for(int i = 0; i < mostSamples; i++)
        {
            float sampleValue = 0;
            int sampleCount = 0;

            foreach (var audioLayer in audioLayers)
            {
                if (!audioLayer.record)
                    continue;
                if(i > audioLayer.delayCount && i < audioLayer.sampleCount)
                {
                    sampleValue += (audioLayer.samples[i - audioLayer.delayCount] / rescaleFactor);
                    sampleCount++;
                }
            }
            
            if(sampleCount!=0)
                sampleValue /= sampleCount;
            finalSamples[i] = (short)(sampleValue * rescaleFactor);
        }


        Byte[] bytesData = ConvertSamplesToBytes(finalSamples);
        fileStream.Write(bytesData, 0, bytesData.Length);

        return mostSamples;
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


    static Int16[] GetSamplesFromClip(AudioClip clip, float volume = 1)
    {
        var samples = new float[clip.samples * clip.channels];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * volume * rescaleFactor);
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
