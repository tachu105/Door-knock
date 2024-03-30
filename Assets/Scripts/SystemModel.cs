using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// システム全体のデータを管理するクラス
/// </summary>
public class SystemModel : MonoBehaviour
{
    /// <summary>
    /// システムのフェーズ
    /// </summary>
    public enum SystemPhase
    {
        FileLoading,
        WaitKnock,
        PlayQuestion,
        WaitTouch,
        AnswerRecord,
        PlayAnotherAnswer,
        PlayQuestionAgain,
        SystemReset
    }


    /// <summary>
    /// 現在の振動センサの値
    /// </summary>
    private bool vibrationSensorCurrentState;

    /// <summary>
    /// 前回の振動センサの値
    /// </summary>
    private bool preVibrationSensorState;

    /// <summary>
    /// 現在のタッチセンサの値
    /// </summary>
    private bool touchSensorCurrentState;

    /// <summary>
    /// 前回のタッチセンサの値
    /// </summary>
    private bool preTouchSensorState;

    /// <summary>
    /// システムのフェーズ
    /// </summary>
    [HideInInspector]
    public SystemPhase currentPhase;

    /// <summary>
    /// システムのフェーズの変更前のフェーズ
    /// </summary>
    private SystemPhase prePhase;

    /// <summary>
    /// 現在の質問ID
    /// </summary>
    [HideInInspector]
    public int questionID;

    /// <summary>
    /// ノックの回数
    /// </summary>
    [HideInInspector]
    public int knockCount;


    [SerializeField, Tooltip("音声を再生するノックの回数")]
    public int knockThreshold = 3;

    [SerializeField, Tooltip("指定時間以内に連続でノックされない場合はリセット")]
    public float knockResetTime = 1.0f;

    [SerializeField, Tooltip("指定時間以内にドアノブがタッチされない場合はリセット")]
    public float touchResetTime = 10.0f;

    [SerializeField, Tooltip("ノックを促す音声の再生間隔（分）")]
    public int promptKnockIntervalMinutes = 15;

    [SerializeField, Tooltip("この時間以上手が離されていたら録音停止")]
    public float recordingStopBufferTime = 0.5f;

    [SerializeField, Tooltip("AIの音声の音量"), Range(0,1)]
    public float systemAudioVolume = 1.0f;

    [SerializeField, Tooltip("録音した音声の音量"), Range(0, 1)]
    public float recordedAudioVolume = 1.0f;

    //録音時間　なぜかSerializeから変更できない
    //なるべく変えない
    //変えたら今までの録音データを削除する
    [HideInInspector]
    public int recordingTime = 10;



    private void Awake()
    {
        currentPhase = SystemPhase.FileLoading;
    }

    /// <summary>
    /// 振動センサの値を代入
    /// </summary>
    /// <param name="state">センサの値</param>
    public void SetVibrationSensorState(bool state)
    {
        vibrationSensorCurrentState = state;
    }

    /// <summary>
    /// タッチセンサの値を代入
    /// </summary>
    /// <param name="state">センサの値</param>
    public void SetTouchSensorState(bool state)
    {
        touchSensorCurrentState = state;
    }

    public bool GetTouchSensorState()
    {
        return touchSensorCurrentState;
    }


    /// <summary>
    /// SystemPhaseの変更の有無を確認する
    /// </summary>
    /// <returns></returns>
    public bool CheckPhaseChange()
    {
        if (currentPhase != prePhase)
        {
            prePhase = currentPhase;
            Debug.Log($"フェーズ{currentPhase}へ移行");
            return true;
        }
        return false;
    }


    /// <summary>
    /// 振動センサの変化を確認する
    /// </summary>
    /// <returns></returns>
    public bool CheckVibrationSensorChange(out bool currentState)
    {
        if (vibrationSensorCurrentState != preVibrationSensorState)
        {
            preVibrationSensorState = vibrationSensorCurrentState;
            currentState = vibrationSensorCurrentState;
            Debug.Log("振動センサー値変動");
            return true;
        }
        
        preVibrationSensorState = vibrationSensorCurrentState;
        currentState = vibrationSensorCurrentState;
        return false;
    }


    /// <summary>
    /// タッチセンサの変化を確認する
    /// </summary>
    /// <returns></returns>
    public bool CheckTouchSensorChange(out bool currentState)
    {
        if (touchSensorCurrentState != preTouchSensorState)
        {
            preTouchSensorState = touchSensorCurrentState;
            currentState = touchSensorCurrentState;
            Debug.Log("タッチセンサー値変動");
            return true;
        }
        preTouchSensorState = touchSensorCurrentState;
        currentState = touchSensorCurrentState;
        return false;
    }


    /// <summary>
    /// 次のシステムフェーズに変更する
    /// </summary>
    public void ChangeNextSystemPhase()
    {
        switch (currentPhase)
        {
            case SystemPhase.WaitKnock:
                currentPhase = SystemPhase.PlayQuestion;
                break;
            case SystemPhase.PlayQuestion:
                currentPhase = SystemPhase.WaitTouch;
                break;
            case SystemPhase.WaitTouch:
                currentPhase = SystemPhase.AnswerRecord;
                break;
            case SystemPhase.AnswerRecord:
                currentPhase = SystemPhase.PlayAnotherAnswer;
                break;
            case SystemPhase.PlayAnotherAnswer:
                currentPhase = SystemPhase.WaitKnock;
                break;
        }
    }
}
