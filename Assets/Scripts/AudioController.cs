using System.Collections;
using UnityEngine;

/// <summary>
/// オーディオの再生を管理するクラス
/// </summary>
public class AudioController : MonoBehaviour
{
    [SerializeField] private SystemModel systemModel;
    [SerializeField] private AudioDataBase audioDataBase;
    [SerializeField] private AudioSource audioSource;

    private AudioClip currentQuestionClip;

    private WaitUntil waitUntil_AudioStop;
    private WaitUntil waitUntil_SystemInactive;
    private WaitUntil waitUntil_PhaseChange;
    private WaitForSeconds waitSecond_PromptKnockInterval;


    void Start()
    {
        waitUntil_AudioStop = new WaitUntil(() => !audioSource.isPlaying);
        waitUntil_SystemInactive = new WaitUntil(() => systemModel.currentPhase == SystemModel.SystemPhase.WaitKnock);
        waitUntil_PhaseChange = new WaitUntil(() => systemModel.CheckPhaseChange());
        waitSecond_PromptKnockInterval = new WaitForSeconds(systemModel.PromptKnockIntervalMinutes * 60);

        // オーディオファイルをロード
        audioDataBase.LoadAudioFiles();

        // システムをアクション待機状態にする
        systemModel.currentPhase = SystemModel.SystemPhase.WaitKnock;

        StartCoroutine(ActiveAudioPlayer());
        StartCoroutine(PromptKnockAudioPlayer());
    }

    /// <summary>
    /// アクティブ中，システムフェーズを監視し，オーディオを制御するコルーチン
    /// </summary>
    IEnumerator ActiveAudioPlayer()
    {
        yield return waitUntil_PhaseChange;

        switch (systemModel.currentPhase)
        {
            // 質問の音声を再生する
            case SystemModel.SystemPhase.PlayQuestion:
                yield return StartCoroutine(PlayQuestionAudio());
                systemModel.ChangeNextSystemPhase();
                break;
            // 他者の回答の音声を再生する
            case SystemModel.SystemPhase.PlayAnotherAnswer:
                yield return StartCoroutine(PlayAnotherAnswerAudio());
                systemModel.ChangeNextSystemPhase();
                break;
            // システムリセット時の音声を再生する
            case SystemModel.SystemPhase.SystemReset:
                yield return StartCoroutine(PlaySystemResetAudio());
                systemModel.currentPhase = SystemModel.SystemPhase.WaitKnock;
                break;
            // 再質問音声を再生する
            case SystemModel.SystemPhase.PlayQuestionAgain:
                yield return StartCoroutine(PlayQuestionAudioAgain());
                systemModel.currentPhase = SystemModel.SystemPhase.WaitTouch;
                break;
            default:
                break;
        }

        StartCoroutine(ActiveAudioPlayer());
    }


    /// <summary>
    /// 質問フェーズの音声を再生する
    /// </summary>
    IEnumerator PlayQuestionAudio()
    {
        //質問前導入音声
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.AfterKnockAudio;
        audioSource.Play();
        Debug.Log("質問前導入音声再生");

        yield return waitUntil_AudioStop;

        // ランダムに質問音声を再生する
        systemModel.questionID = audioDataBase.GetRandomQuestionID();
        currentQuestionClip = audioDataBase.GetQuestionAudioData((uint)systemModel.questionID);
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = currentQuestionClip;
        audioSource.Play();
        Debug.Log("質問音声再生");

        yield return waitUntil_AudioStop;
    }


    /// <summary>
    /// 再度質問して録音を促す音声を再生する
    /// </summary>
    IEnumerator PlayQuestionAudioAgain()
    {
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.PromptTouchAudio;
        audioSource.Play();
        Debug.Log("タッチを促す音声を再生");

        yield return waitUntil_AudioStop;

        //同じ質問を再度再生
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = currentQuestionClip;
        audioSource.Play();
        Debug.Log("二度目の質問音声再生");

        yield return waitUntil_AudioStop;
    }


    /// <summary>
    /// 他者の回答の音声を再生する
    /// </summary>
    IEnumerator PlayAnotherAnswerAudio()
    {
        //録音後の他者回答までの導入再生
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.AfterRecordingAudio;
        audioSource.Play();

        yield return waitUntil_AudioStop;

        // ランダムに別の回答の音声を再生する
        // なぜか音声が長時間読み込まれてしまうものがあるので，それを避ける
        // 時間はMicrophone.StartのlengthSec引数の値に依存している
        while (true)
        {
            audioSource.clip = audioDataBase.GetRandomAnswerAudioData(systemModel.questionID);

            // 音声の長さがピッタリ整数値の場合はエラーの可能性があるので再取得
            if (audioSource.clip.length != Mathf.Floor(audioSource.clip.length)) break;
        }
        audioSource.volume = systemModel.RecordedAudioVolume;
        audioSource.Play();
        
        yield return waitUntil_AudioStop;

        //別れの会話を再生
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.LastConversationAudio;
        audioSource.Play();

        yield return waitUntil_AudioStop;
    }


    /// <summary>
    /// 時間切れなどでシステムをリセットする際の音声を再生する
    /// </summary>
    IEnumerator PlaySystemResetAudio()
    {
        // システムリセットの音声を再生する
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.SystemResetAudio;
        audioSource.Play();
        Debug.Log("システムリセット音声再生");

        yield return waitUntil_AudioStop;
    }


    /// <summary>
    /// 待機状態時，一定時間置きにノックを促す音声を再生するコルーチン
    /// </summary>
    IEnumerator PromptKnockAudioPlayer()
    {
        yield return waitUntil_SystemInactive;
        yield return waitSecond_PromptKnockInterval;

        if(systemModel.currentPhase != SystemModel.SystemPhase.WaitKnock)
        {
            StartCoroutine(PromptKnockAudioPlayer());
            yield break;
        }

        // ノックを促す音声を再生する
        audioSource.volume = systemModel.SystemAudioVolume;
        audioSource.clip = audioDataBase.PromptKnockAudio;
        audioSource.Play();
        Debug.Log("ノック促し音声再生");

        StartCoroutine(PromptKnockAudioPlayer());
    }
    
}
