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
    Coroutine recStopByTouchSensorCoroutine;

    bool isCancelRecording = false; //システムリセット
    bool isStopRecording = false;   //録音終了

    bool isDoneAgainQuestion = false;   //再度質問を再生したかどうか
    
    // Start is called before the first frame update
    void Start()
    {
        isCancelRecording = false;
        isStopRecording = false;
        isDoneAgainQuestion = false;

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
            if(systemModel.GetTouchSensorState())
            {
                isDoneAgainQuestion = false;
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

        recordingSystem.StartRecording(systemModel.recordingTime);   //録音開始
        
        //録音フェーズに変更
        systemModel.currentPhase = SystemModel.SystemPhase.AnswerRecord;

        //録音停止タイマー作動
        stopRecordingTimerCoroutine = StartCoroutine(RecordingTimer(systemModel.recordingTime));

        //録音終了を監視
        while (true)
        {
            //タッチセンサーが変動したとき
            if(systemModel.CheckTouchSensorChange(out currentState))
            {
                //タッチセンサーがオフ→オンになったとき
                if (currentState)
                {
                    //手が離されたことはノイズとして扱い，録音を継続
                    if(recStopByTouchSensorCoroutine != null) StopCoroutine(recStopByTouchSensorCoroutine);
                }
                //タッチセンサーがオン→オフになったとき
                else
                {
                    //手が離されたことを検知し，一定時間再びタッチセンサーがオンにならなかった場合，録音を終了
                    recStopByTouchSensorCoroutine = StartCoroutine(RecordingStopByTouchSensor(systemModel.recordingStopBufferTime));
                }
            }

            //録音終了
            if (isStopRecording)
            {
                if(recStopByTouchSensorCoroutine != null) StopCoroutine(recStopByTouchSensorCoroutine);
                if(stopRecordingTimerCoroutine != null) StopCoroutine(stopRecordingTimerCoroutine);
                isStopRecording = false;
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
    /// 一定時間録音が開始されなかった場合，システムリセットまたは再度質問を再生する処理
    /// </summary>
    /// <param name="waitTime">システムリセットまでの制限時間</param>
    /// <returns></returns>
    IEnumerator ResetKnockCountTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //録音催促2回目
        if (isDoneAgainQuestion)
        {
            isDoneAgainQuestion = false;
            isCancelRecording = true;
            systemModel.currentPhase = SystemModel.SystemPhase.SystemReset;
        }
        //録音催促1回目
        else
        {
            isDoneAgainQuestion = true;
            isCancelRecording = true;
            systemModel.currentPhase = SystemModel.SystemPhase.PlayQuestionAgain;
        }        
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
        Debug.Log("タイマーによる録音停止");
    }


    /// <summary>
    /// タッチセンサーがオフになったとき，一定時間経過後に録音を終了させる処理
    /// </summary>
    /// <returns></returns>
    IEnumerator RecordingStopByTouchSensor(float bufferTime)
    {
        yield return new WaitForSeconds(bufferTime);
        isStopRecording = true;
        Debug.Log("タッチセンサーオフによる録音停止");
    }
}
