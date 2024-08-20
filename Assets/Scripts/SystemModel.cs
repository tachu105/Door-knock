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

    //センサー値
    public bool vibrationSensorCurrentState { get; set; }
    public bool touchSensorCurrentState { get; set; }
    private bool vibrationSensorPreState;
    private bool touchSensorPreState;

    /// <summary>
    /// システムのフェーズ
    /// </summary>
    [HideInInspector]
    public SystemPhase currentPhase;
    private SystemPhase prePhase;

    /// <summary>
    /// 現在の質問ID
    /// </summary>
    [HideInInspector]
    public uint questionID;

    /// <summary>
    /// ノックの回数
    /// </summary>
    [HideInInspector]
    public int knockCount;

    /// <summary>
    /// オーディオデータフォルダのパス
    /// </summary>
    [HideInInspector]
    public string audioDataFolderPath { get; private set; }

    //録音時間　なぜかSerializeから変更できない
    //なるべく変えない
    //変えたら今までの録音データを削除する
    [HideInInspector]
    public int recordingTime = 10;


    [SerializeField, Tooltip("音声を再生するノックの回数")]
    private int knockThreshold = 3;

    [SerializeField, Tooltip("連続ノックと判定するノック間隔")]
    private float repeatedKnockInterval = 1.0f;

    [SerializeField, Tooltip("ドアノブタッチ待機の受付時間")]
    private float touchWaitTime = 10.0f;

    [SerializeField, Tooltip("ノックを促す音声の再生間隔（分）")]
    private int promptKnockIntervalMinutes = 15;

    [SerializeField, Tooltip("タッチ終了から録音停止処理までのバッファ時間")]
    private float recordingStopBufferTime = 0.5f;

    [SerializeField, Tooltip("AIの音声の音量"), Range(0,1)]
    private float systemAudioVolume = 1.0f;

    [SerializeField, Tooltip("録音した音声の音量"), Range(0, 1)]
    private float recordedAudioVolume = 1.0f;

    /// <summary>
    /// 音声を再生するノックの回数
    /// </summary>
    public int KnockThreshold => knockThreshold;

    /// <summary>
    /// 連続ノックと判定するノック間隔
    /// </summary>
    public float RepeatedKnockInterval => repeatedKnockInterval;

    /// <summary>
    /// ドアノブタッチ待機の受付時間
    /// </summary>
    public float TouchWaitTime => touchWaitTime;

    /// <summary>
    /// ノックを促す音声の再生間隔（分）
    /// </summary>
    public int PromptKnockIntervalMinutes => promptKnockIntervalMinutes;

    /// <summary>
    /// タッチ終了から録音停止処理までのバッファ時間
    /// </summary>
    public float RecordingStopBufferTime => recordingStopBufferTime;

    /// <summary>
    /// AIの音声の音量
    /// </summary>
    public float SystemAudioVolume => systemAudioVolume;

    /// <summary>
    /// 録音した音声の音量
    /// </summary>
    public float RecordedAudioVolume => recordedAudioVolume;


    private void Awake()
    {
        currentPhase = SystemPhase.FileLoading;
        audioDataFolderPath = Application.dataPath + "/Audios/";
    }

    /// <summary>
    /// SystemPhaseの変動有無を確認する
    /// </summary>
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
    /// 振動センサの変動有無を確認する
    /// </summary>
    public bool CheckVibrationSensorChange(out bool currentState)
    {
        if (vibrationSensorCurrentState != vibrationSensorPreState)
        {
            vibrationSensorPreState = vibrationSensorCurrentState;
            currentState = vibrationSensorCurrentState;
            return true;
        }
        
        vibrationSensorPreState = vibrationSensorCurrentState;
        currentState = vibrationSensorCurrentState;
        return false;
    }

    /// <summary>
    /// タッチセンサの変動有無を確認する
    /// </summary>
    public bool CheckTouchSensorChange(out bool currentState)
    {
        if (touchSensorCurrentState != touchSensorPreState)
        {
            touchSensorPreState = touchSensorCurrentState;
            currentState = touchSensorCurrentState;
            return true;
        }
        touchSensorPreState = touchSensorCurrentState;
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
