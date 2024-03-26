using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordingController : MonoBehaviour
{
    [SerializeField] SystemModel systemModel;
    [SerializeField] AudioDataBase audioDataBase;
    [SerializeField] RecordingSystem recordingSystem;

    AudioClip recordedClip;

    Coroutine resetCoroutine;
    Coroutine stopRecordingTimerCoroutine;

    bool isCancelRecording = false; //システムリセット
    bool isStopRecording = false;   //録音終了
    
    // Start is called before the first frame update
    void Start()
    {
        isCancelRecording = false;
        isStopRecording = false;

        StartCoroutine(RecordingControl());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RecordingControl()
    {
        bool currentState;

        // 録音待機状態以外のときは再起
        if (systemModel.currentPhase != SystemModel.SystemPhase.WaitTouch)
        {
            yield return null;
            StartCoroutine(RecordingControl());
            yield break;
        }

        //未タッチによるシステムリセットのタイマー作動
        resetCoroutine = StartCoroutine(ResetKnockCountTimer(systemModel.touchResetTime));

        //録音開始を監視
        while (true)
        {
            //タッチセンサー感知
            if(systemModel.CheckTouchSensorChange(out currentState) && currentState)
            {
                StopCoroutine(resetCoroutine);
                break;
            }
            //一定時間センサー未タッチの場合，システムリセット
            if (isCancelRecording)
            {
                isCancelRecording = false;

                yield return null;
                StartCoroutine(RecordingControl());
                yield break;
            }

            yield return null;
        }

        recordingSystem.StartRecording();   //録音開始
        
        //録音フェーズに変更
        systemModel.currentPhase = SystemModel.SystemPhase.AnswerRecord;

        //録音停止タイマー作動
        stopRecordingTimerCoroutine = StartCoroutine(RecordingTimer(systemModel.recordingTime));

        //録音終了を監視
        while (true)
        {
            //タッチセンサーがオフになったことによる録音終了
            if(systemModel.CheckTouchSensorChange(out currentState) && !currentState)
            {
                StopCoroutine(stopRecordingTimerCoroutine);
                Debug.Log("タッチセンサーオフによる録音停止");
                break;
            }
            //時間制限による録音終了
            if (isStopRecording)
            {
                isStopRecording = false;
                Debug.Log("タイマーによる録音停止");
                break;
            }

            yield return null;
        }

        AudioDataBase.AudioData currentAudioData = audioDataBase.GetAudioData(systemModel.questionID);  //現在の質問のオーディオデータベースを取得
        recordingSystem.StopRecording(out recordedClip, ref currentAudioData.allAnswerFileCount, currentAudioData.folderName); //録音終了＆録音データ取得

        //録音データをデータベースに保存
        if(recordedClip!= null)
        {
            audioDataBase.SetAnswerAudioData(systemModel.questionID,recordedClip);
        }

        //フェーズ変更
        systemModel.currentPhase = SystemModel.SystemPhase.PlayAnotherAnswer;

        yield return null;
        StartCoroutine(RecordingControl());
    }




    /// <summary>
    /// 一定時間録音が開始されなかった場合，システムリセット
    /// </summary>
    /// <param name="waitTime">システムリセットまでの制限時間</param>
    /// <returns></returns>
    IEnumerator ResetKnockCountTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isCancelRecording = true;
        systemModel.currentPhase = SystemModel.SystemPhase.SystemReset;
    }

    /// <summary>
    /// 指定時間経過後，録音を終了させる処理
    /// </summary>
    /// <param name="recordTime">録音最長時間</param>
    /// <returns></returns>
    IEnumerator RecordingTimer(float recordTime)
    {
        yield return new WaitForSeconds(recordTime);
        isStopRecording = true;
    }
}
