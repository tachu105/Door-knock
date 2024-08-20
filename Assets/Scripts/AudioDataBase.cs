using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 質問と回答の音声データを管理するクラス
/// </summary>
public class AudioDataBase : MonoBehaviour
{
    /// <summary>
    /// 質問と回答の音声データ型
    /// </summary>
    [Serializable]
    public class AudioData
    {
        [Tooltip("質問の要約")]
        public string questionSummary;

        [Tooltip("音声データのフォルダ名")]
        public string folderName;

        [Tooltip("質問の音声")]
        public AudioClip questionAudio;

        [Tooltip("回答の音声")]
        public List<AudioClip> answerAudios;

        [HideInInspector]
        public int allAnswerFileCount;    //回答ファイルの総数
    }

    [SerializeField] private SystemModel systemModel;

    [SerializeField, Tooltip("質問回答音声のデータベース")]
    private List<AudioData> audioDatas;

    [SerializeField, Tooltip("回答データを保持する個数")]
    private int maxHoldNumOfAudioDatas = 10;

    [SerializeField, Tooltip("システムをリセットするときに流れる音声")]
    private AudioClip systemResetAudio;

    [SerializeField, Tooltip("ノックを促す音声")]
    private AudioClip promptKnockAudio;

    [SerializeField, Tooltip("ドアノブタッチを促す音声")]
    private AudioClip promptTouchAudio;

    [SerializeField, Tooltip("ノック後の音声")]
    private AudioClip afterKnockAudio;

    [SerializeField, Tooltip("録音後の音声")]
    private AudioClip afterRecordingAudio;

    [SerializeField, Tooltip("最後の会話")]
    private AudioClip lastConversationAudio;

    /// <summary>
    /// システムをリセットするときに流れる音声
    /// </summary>
    public AudioClip SystemResetAudio => systemResetAudio;

    /// <summary>
    /// ノックを促す音声
    /// </summary>
    public AudioClip PromptKnockAudio => promptKnockAudio;

    /// <summary>
    /// ドアノブタッチを促す音声
    /// </summary>
    public AudioClip PromptTouchAudio => promptTouchAudio;

    /// <summary>
    /// ノック後の音声
    /// </summary>
    public AudioClip AfterKnockAudio => afterKnockAudio;

    /// <summary>
    /// 録音後の音声
    /// </summary>
    public AudioClip AfterRecordingAudio => afterRecordingAudio;

    /// <summary>
    /// 最後の会話
    /// </summary>
    public AudioClip LastConversationAudio => lastConversationAudio;


    /// <summary>
    /// 過去の回答音声データをランダムで取得する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    public AudioClip GetRandomAnswerAudioData(uint questionID)
    {
        // 質問IDが範囲外の場合は例外を投げる
        if(questionID >= audioDatas.Count)
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");

        List<AudioClip> audios = audioDatas[(int)questionID].answerAudios;

        //ランダムで回答音声データを返す
        int randomIndex = UnityEngine.Random.Range(0, audios.Count - 1);
        return audios[randomIndex];
    }

    /// <summary>
    /// ランダムな質問IDを取得する
    /// </summary>
    public uint GetRandomQuestionID()
    {
        return (uint)UnityEngine.Random.Range(0, audioDatas.Count);
    }

    /// <summary>
    /// 質問の音声データを取得する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    public AudioClip GetQuestionAudioData(uint questionID)
    {
        //　質問IDが範囲外の場合は例外を投げる
        if (questionID >= audioDatas.Count)
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");

        // 質問の音声データを返す
        return audioDatas[(int)questionID].questionAudio;
    }

    /// <summary>
    /// 回答に新しい音声データを追加する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    /// <param name="answerAudio">追加する音声データ</param>
    public void SetAnswerAudioData(uint questionID, AudioClip answerAudio)
    {
        if (questionID >= audioDatas.Count)
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");

        List<AudioClip> audios = audioDatas[(int)questionID].answerAudios;

        // 回答音声データを追加する
        audios.Add(answerAudio);

        // 保持する回答音声データ数が最大数を超えた場合は古いデータを削除する
        if (audios.Count > maxHoldNumOfAudioDatas)  audios.RemoveAt(0);
    }

    /// <summary>
    /// ローカルフォルダからオーディオデータを取得してDataBaseのリストに挿入する
    /// </summary>
    public void LoadAudioFiles()
    {
        Debug.Log("ファイルロード中");
        foreach (AudioData audioData in audioDatas)
        {
            AudioLoader.LoadAudioClips( 
                systemModel.audioDataFolderPath + audioData.folderName,
                maxHoldNumOfAudioDatas,
                ref audioData.answerAudios, 
                out audioData.allAnswerFileCount
                );
        } 
        Debug.Log("ファイルロード完了");
    }

    /// <summary>
    /// 指定質問番号のオーディオデータのゲッター
    /// </summary>
    /// <param name="questionID"> 質問のインデックス </param>
    public AudioData GetAudioData(uint questionID)
    {
        if (questionID >= audioDatas.Count)
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");

        return audioDatas[(int)questionID];
    }
}
