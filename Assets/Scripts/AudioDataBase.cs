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

    [SerializeField, Tooltip("質問回答音声のデータベース")]
    private List<AudioData> audioDatas;

    [SerializeField, Tooltip("回答データを保持する個数")]
    private int maxHoldNumOfAudioDatas = 10;

    [SerializeField, Tooltip("システムをリセットするときに流れる音声")]
    public AudioClip systemResetAudio;

    AudioLoader audioLoader;

    /// <summary>
    /// 過去の回答音声データをランダムで取得する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public AudioClip GetRandomAnswerAudioData(int questionID)
    {
        // 質問IDが範囲外の場合は例外を投げる
        if(questionID < 0 || questionID >= audioDatas.Count)
        {
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");
        }

        //ランダムで回答音声データを返す
        int randomIndex = UnityEngine.Random.Range(0, audioDatas[questionID].answerAudios.Count-1);
        return audioDatas[questionID].answerAudios[randomIndex];
    }


    /// <summary>
    /// ランダムな質問IDを取得する
    /// </summary>
    /// <returns></returns>
    public int GetRandomQuestionID()
    {
        return UnityEngine.Random.Range(0, audioDatas.Count);
    }


    /// <summary>
    /// 質問の音声データを取得する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public AudioClip GetQuestionAudioData(int questionID)
    {
        //　質問IDが範囲外の場合は例外を投げる
        if (questionID < 0 || questionID >= audioDatas.Count)
        {
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");
        }

        // 質問の音声データを返す
        return audioDatas[questionID].questionAudio;
    }


    /// <summary>
    /// 回答に新しい音声データを追加する
    /// </summary>
    /// <param name="questionID">質問のID番号（要素番号）</param>
    /// <param name="answerAudio">追加する音声データ</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void SetAnswerAudioData(int questionID, AudioClip answerAudio)
    {
        // 質問IDが範囲外の場合は例外を投げる
        if (questionID < 0 || questionID >= audioDatas.Count)
        {
            throw new ArgumentOutOfRangeException("質問IDが範囲外です");
        }

        // 回答音声データを追加する
        audioDatas[questionID].answerAudios.Add(answerAudio);

        // 保持する回答音声データ数が最大数を超えた場合は古いデータを削除する
        if (audioDatas[questionID].answerAudios.Count > maxHoldNumOfAudioDatas)
        {
            audioDatas[questionID].answerAudios.RemoveAt(0);
        }
    }

    /// <summary>
    /// ローカルフォルダからオーディオデータを取得してDataBaseのリストに挿入する関数
    /// </summary>
    public void LoadAudioFiles()
    {
        Debug.Log("ファイルロード中");
        audioLoader = GetComponent<AudioLoader>();
        foreach (AudioData audioData in audioDatas)
        {
            audioLoader.LoadAudioClips(audioData.folderName, maxHoldNumOfAudioDatas,ref audioData.answerAudios, out audioData.allAnswerFileCount);
        } 
        Debug.Log("ファイルロード完了");
    }

    /// <summary>
    /// 指定の質問番号のオーディオデータベースを取得する関数
    /// </summary>
    /// <param name="questionID"></param>
    /// <returns></returns>
    public AudioData GetAudioData(int questionID)
    {
        return audioDatas[questionID];
    }
}
