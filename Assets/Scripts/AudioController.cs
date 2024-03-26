using System.Collections;
using UnityEngine;

/// <summary>
/// オーディオの再生を管理するクラス
/// </summary>
public class AudioController : MonoBehaviour
{
    [SerializeField]
    private SystemModel systemModel;
    private AudioDataBase audioDataBase;
    private AudioSource audioSource;

    

    // Start is called before the first frame update
    void Start()
    {
        audioDataBase = GetComponent<AudioDataBase>();
        audioSource = GetComponent<AudioSource>();

        audioDataBase.LoadAudioFiles();

        systemModel.currentPhase = SystemModel.SystemPhase.WaitKnock;

        StartCoroutine(PlayAudio());
    }

    /// <summary>
    /// オーディオを再生する
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayAudio()
    {
        // システムのフェーズ変更を監視
        while(!systemModel.CheckPhaseChange())
        {
            yield return null;
        }

        // システムのフェーズによって再生する音声を変更する
        switch(systemModel.currentPhase)
        {
            case SystemModel.SystemPhase.PlayQuestion:
                // 質問の音声を再生する
                yield return StartCoroutine(PlayQuestionAudio());   //音声の再生が終わるまで待機
                systemModel.ChangeNextSystemPhase();  //次のフェーズに変更
                break;
            case SystemModel.SystemPhase.PlayAnotherAnswer:
                // 別の回答の音声を再生する
                yield return StartCoroutine(PlayAnotherAnswerAudio());  //音声の再生が終わるまで待機
                systemModel.ChangeNextSystemPhase();  //次のフェーズに変更
                break;
            case SystemModel.SystemPhase.SystemReset:
                yield return StartCoroutine(PlaySystemResetAudio());  //音声の再生が終わるまで待機
                systemModel.currentPhase = SystemModel.SystemPhase.WaitKnock;
                break;
            default:
                yield return null;
                // それ以外のフェーズの場合は再起
                StartCoroutine(PlayAudio());
                yield break;
        }

        yield return null;
        // 監視を再開
        StartCoroutine(PlayAudio());
    }


    /// <summary>
    /// 質問フェーズの音声を再生する
    /// </summary>
    IEnumerator PlayQuestionAudio()
    {
        // ランダムな質問IDを取得する
        systemModel.questionID = audioDataBase.GetRandomQuestionID();

        // 質問の音声を再生する
        audioSource.clip = audioDataBase.GetQuestionAudioData(systemModel.questionID);
        audioSource.Play();

        Debug.Log("質問音声再生");

        // 音声の再生終了まで待機
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }


    /// <summary>
    /// 他者の回答の音声を再生する
    /// </summary>
    IEnumerator PlayAnotherAnswerAudio()
    {
        // ランダムに別の回答の音声を再生する
        audioSource.clip = audioDataBase.GetRandomAnswerAudioData(systemModel.questionID);
        audioSource.Play();

        Debug.Log("他者回答再生");

        // 音声の再生終了まで待機
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }


    /// <summary>
    /// 時間切れなどでシステムをリセットする際の音声を再生する
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaySystemResetAudio()
    {
        // システムリセットの音声を再生する
        audioSource.clip = audioDataBase.systemResetAudio;
        audioSource.Play();

        Debug.Log("システムリセット音声再生");

        // 音声の再生終了まで待機
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }


    
}
