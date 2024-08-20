using System.Collections;
using UnityEngine;

public class RecordingController : MonoBehaviour
{
    [SerializeField] private SystemModel systemModel;
    [SerializeField] private AudioDataBase audioDataBase;
    [SerializeField] private RecordingSystem recordingSystem;

    private Coroutine resetCoroutine;
    private Coroutine stopRecordingTimerCoroutine;
    private Coroutine recStopByTouchSensorCoroutine;
    private WaitUntil waitUntil_RecordingReadyState;

    private bool isCancelRecording;
    private bool isStopRecording;
    private bool isDoneSecondTimeQuestion;
    

    void Start()
    {
        isCancelRecording = false;
        isStopRecording = false;
        isDoneSecondTimeQuestion = false;

        waitUntil_RecordingReadyState = new WaitUntil(() => systemModel.currentPhase == SystemModel.SystemPhase.WaitTouch);

        StartCoroutine(RecordingControl());
    }

    IEnumerator RecordingControl()
    {
        yield return waitUntil_RecordingReadyState;

        //未タッチによるシステムリセットのタイマー作動
        resetCoroutine = StartCoroutine(ResetKnockCountTimer(systemModel.TouchWaitTime));

        //録音開始を監視
        while (true)
        {
            //タッチセンサー感知
            if(systemModel.touchSensorCurrentState)
            {
                isDoneSecondTimeQuestion = false;
                StopCoroutine(resetCoroutine);
                break;
            }
            //一定時間センサー未タッチの場合，システムリセット
            if (isCancelRecording)
            {
                isCancelRecording = false;

                StartCoroutine(RecordingControl());
                yield break;
            }

            yield return null;
        }

        //録音開始（録音ミスを防ぐため，録音時間にバッファを持たせる）
        recordingSystem.StartRecording(systemModel.recordingTime * 2);   
        systemModel.currentPhase = SystemModel.SystemPhase.AnswerRecord;

        //録音停止タイマー作動
        stopRecordingTimerCoroutine = StartCoroutine(RecordingTimer(systemModel.recordingTime));

        //録音終了を監視
        while (true)
        {
            if (systemModel.CheckTouchSensorChange(out bool newSensorState))
            {
                //タッチセンサーがオフ→オンになったとき
                if (newSensorState)
                {
                    //手が離されたことはノイズとして扱い，録音を継続
                    if(recStopByTouchSensorCoroutine != null) StopCoroutine(recStopByTouchSensorCoroutine);
                }
                //タッチセンサーがオン→オフになったとき
                else
                {
                    //手が離されたことを検知し，一定時間再びタッチセンサーがオンにならなかった場合，録音を終了
                    recStopByTouchSensorCoroutine = StartCoroutine(RecordingStopByTouchSensor(systemModel.RecordingStopBufferTime));
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

        //録音停止＆録音データ取得
        AudioDataBase.AudioData currentAudioData = audioDataBase.GetAudioData(systemModel.questionID);
        recordingSystem.StopRecording(out AudioClip recordedClip, ref currentAudioData.allAnswerFileCount, currentAudioData.folderName);

        //録音データをデータベースに保存
        if(recordedClip!= null)
            audioDataBase.SetAnswerAudioData(systemModel.questionID,recordedClip);

        systemModel.currentPhase = SystemModel.SystemPhase.PlayAnotherAnswer;

        StartCoroutine(RecordingControl());
    }




    /// <summary>
    /// 一定時間録音が開始されなかった場合，システムリセットまたは再度質問を再生する処理
    /// </summary>
    /// <param name="waitTime">システムリセットまでの制限時間</param>
    IEnumerator ResetKnockCountTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //録音催促2回目
        if (isDoneSecondTimeQuestion)
        {
            isDoneSecondTimeQuestion = false;
            isCancelRecording = true;
            systemModel.currentPhase = SystemModel.SystemPhase.SystemReset;
        }
        //録音催促1回目
        else
        {
            isDoneSecondTimeQuestion = true;
            isCancelRecording = true;
            systemModel.currentPhase = SystemModel.SystemPhase.PlayQuestionAgain;
        }        
    }

    /// <summary>
    /// 指定時間経過後，録音を終了させる処理
    /// </summary>
    /// <param name="recordTime">録音最長時間</param>
    IEnumerator RecordingTimer(float recordTime)
    {
        yield return new WaitForSeconds(recordTime);
        isStopRecording = true;
        Debug.Log("タイマーによる録音停止");
    }


    /// <summary>
    /// タッチセンサーがオフになったとき，一定時間経過後に録音を終了させる処理
    /// </summary>
    IEnumerator RecordingStopByTouchSensor(float bufferTime)
    {
        yield return new WaitForSeconds(bufferTime);
        isStopRecording = true;
        Debug.Log("タッチセンサーオフによる録音停止");
    }
}
