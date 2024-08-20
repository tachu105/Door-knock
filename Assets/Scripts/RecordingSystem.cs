using System.IO;
using UnityEngine;

/// <summary>
/// 録音機能を提供するクラス
/// </summary>
public class RecordingSystem : MonoBehaviour
{
    [SerializeField] private SystemModel systemModel;

    [SerializeField, Tooltip("使用するマイクの番号")] 
    private int micNum;
    [SerializeField, Tooltip("録音を無効とする音声データの長さ")] 
    float disableRecordingTime = 1.0f;

    private AudioClip recordedClip;
    private string micName;
    private int samplingFrequency = 44100;
    private float recordingStartTime;

    // Start is called before the first frame update
    void Start()
    {
        var devices = Microphone.devices;

        //マイクデバイスを検索
        for(int i = 0; i < devices.Length; i++)
        {
            Debug.Log($"マイク番号：{i}  マイク名：{devices[i]}");
        }

        micName = devices[micNum];
    }

    /// <summary>
    /// レコーディングを開始する
    /// </summary>
    /// <param name="maxRecordingTime"> 最長録音時間 </param>
    public void StartRecording(int maxRecordingTime)
    {
        Debug.Log("録音開始");

        recordedClip = Microphone.Start(micName,false, maxRecordingTime, samplingFrequency);
        recordingStartTime = Time.time;
    }

    /// <summary>
    /// 録音停止処理
    /// </summary>
    /// <param name="recordedClip">録音したオーディオクリップ</param>
    /// <param name="recordingSessionCount">保存されている音声データの総数</param>
    /// <param name="folderName">録音データを保存するフォルダ名</param>
    public void StopRecording(out AudioClip recordedClip ,ref int recordingSessionCount ,string folderName)
    {
        if (Microphone.IsRecording(micName) == true)
        {
            Debug.Log("録音停止");
            Microphone.End(micName);

            //1秒未満の録音データは無効とする
            if (Time.time - recordingStartTime < disableRecordingTime)
            {
                recordedClip = null;
                return;
            }

            recordingSessionCount++;
            recordedClip = this.recordedClip;
            SaveWav(recordedClip, systemModel.audioDataFolderPath + folderName);
        }
        else
        {
            recordedClip =  null;
        }
    }

    /// <summary>
    /// WAVファイルとして保存する
    /// </summary>
    /// <param name="clip"> 保存するAudioClip </param>
    /// <param name="folderPath"> WAVファイルを保存するフォルダパス </param>
    void SaveWav(AudioClip clip, string folderPath)
    {
        // オーディオデータを取得
        float recordingDuration = Time.time - recordingStartTime;
        int samplesToClip = (int)(recordingDuration * samplingFrequency);
        float[] samples = new float[samplesToClip];
        clip.GetData(samples, 0);

        string filePath = Path.Combine(
            folderPath, 
            "recordedAudio"+ System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".wav"
            );

        // WAVファイルの生成
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fileStream);

        // WAVファイルヘッダの書き込み
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

        // オーディオデータの書き込み
        foreach (float sample in samples)
        {
            writer.Write((short)(sample * 32767f));
        }

        writer.Close();
        fileStream.Close();

        Debug.Log("WAV file saved at: " + filePath);
    }

    void OnApplicationQuit()
    {
        if (Microphone.IsRecording(deviceName: micName) == true)
            Microphone.End(deviceName: micName);
    }
}