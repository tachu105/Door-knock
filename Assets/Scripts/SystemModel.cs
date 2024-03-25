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
        WaitKnock,
        PlayQuestion,
        AnswerRecord,
        PlayAnotherAnswer
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


    /// <summary>
    /// SystemPhaseの変更の有無を確認する
    /// </summary>
    /// <returns></returns>
    public bool CheckPhaseChange()
    {
        if (currentPhase != prePhase)
        {
            prePhase = currentPhase;
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
            return true;
        }
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
            return true;
        }
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
                currentPhase = SystemPhase.AnswerRecord;
                break;
            case SystemPhase.AnswerRecord:
                currentPhase = SystemPhase.PlayAnotherAnswer;
                break;
            case SystemPhase.PlayAnotherAnswer:
                currentPhase = SystemPhase.PlayQuestion;
                break;
        }
    }
}
