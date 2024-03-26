using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
//元はネット上に上がっていたMITライセンスというオープンソースソフトウェアライセンスのコードなので、一応コピー元のURLをはっておきます
//:https://kato-robotics.hatenablog.com/entry/2018/07/07/142156

public class RecordingSystem : MonoBehaviour
{
    AudioClip myclip;
    AudioSource audioSource;
    [SerializeField] string micName;   //マイクデバイスの名前 nullを入れるとデフォルトのマイクを使用する
    [SerializeField] int micNum;    //マイクの配列要素番号
    int samplingFrequency = 44100;  //サンプリング周波数
    int maxTime = 10;//最大録音時間
    private bool isRecording = false;
    private float recordingStartTime;

    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        //マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"マイク番号：{i}  マイク名：{device}");
            i++;
        }

        micName = Microphone.devices[micNum];
    }


    public void StartRecording()
    {
        Debug.Log("recording start!");
        isRecording = true;
        //deviceName => "null" デフォルトのマイクを指定
        //Microphone.Startで録音を開始（マイクデバイスの名前、ループするかどうか、録音時間[s], サンプリング周波数）
        //録音データはAudioClip変数に保存される
        myclip = Microphone.Start(deviceName: micName, loop: false, lengthSec: maxTime, frequency: samplingFrequency);
        recordingStartTime = Time.time;
    }

    void Update()
    {
        /*
        if (isRecording && Time.time >= recordingStartTime + maxTime)
        {
            EndButton();
        }*/
    }

    /// <summary>
    /// 録音停止処理
    /// </summary>
    /// <param name="recordedClip">録音したオーディオクリップ</param>
    /// <param name="recordingSessionCount">保存されている音声データの総数</param>
    /// <param name="folderName">録音データを保存するフォルダ名</param>
    public void StopRecording(out AudioClip recordedClip ,ref int recordingSessionCount,string folderName)
    {
        if (Microphone.IsRecording(deviceName: micName) == true)
        {
            Debug.Log("recording stoped");
            Microphone.End(deviceName: micName);
            isRecording = false;
            recordingSessionCount++;
            recordedClip = myclip;
            SaveWav(ref recordingSessionCount, folderName);
        }
        else
        {
            recordedClip =  null;
        }
        
    }

    public void PlayButton()
    {
        Debug.Log("play");
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = myclip;
        audioSource.Play();
    }
    void SaveWav(ref int recordingSessionCount,string fileName)
    {
        // Calculate recording duration
        float recordingDuration = Time.time - recordingStartTime;

        // Clip the recorded audio
        int samplesToClip = (int)(recordingDuration * samplingFrequency);
        float[] samples = new float[samplesToClip];
        myclip.GetData(samples, 0);

        // Set the file path
        string filePath = Path.Combine(Application.dataPath + "/Audios/" + fileName, "recordedAudio"+recordingSessionCount+".wav");
        //保存するwavファイルの名前、保存場所はこの関数の引数で制御できます。

        // Create WAV file
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fileStream);

        // Write WAV header
        writer.Write(0x46464952); // "RIFF" in ASCII
        writer.Write(samples.Length * 2 + 36); // file size - 8
        writer.Write(0x45564157); // "WAVE" in ASCII
        writer.Write(0x20746D66); // "fmt " in ASCII
        writer.Write(16); // size of fmt chunk
        writer.Write((ushort)1); // audio format (PCM)
        writer.Write((ushort)1); // number of channels
        writer.Write(samplingFrequency); // sample rate
        writer.Write(samplingFrequency * 2); // byte rate
        writer.Write((ushort)2); // block align
        writer.Write((ushort)16); // bits per sample
        writer.Write(0x61746164); // "data" in ASCII
        writer.Write(samples.Length * 2); // data size

        // Write audio data
        foreach (float sample in samples)
        {
            writer.Write((short)(sample * 32767f));
        }

        // Close writer and file stream
        writer.Close();
        fileStream.Close();

        Debug.Log("WAV file saved at: " + filePath);
    }

}