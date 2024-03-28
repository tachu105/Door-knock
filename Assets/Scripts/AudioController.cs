using System.Collections;
using Unity.VisualScripting;
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

    private AudioClip currentQuestionClip;

    

    // Start is called before the first frame update
    void Start()
    {
        audioDataBase = GetComponent<AudioDataBase>();
        audioSource = GetComponent<AudioSource>();

        audioDataBase.LoadAudioFiles();

        systemModel.currentPhase = SystemModel.SystemPhase.WaitKnock;

        StartCoroutine(PlayAudio());
        StartCoroutine(PlayPromptKnockAudio());
    }

    /// <summary>
    /// オーディオを再生する
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayAudio()
    {
        // システムのフェーズ変更を監視
        while (true)
        {
            if (systemModel.CheckPhaseChange()) break;
            yield return null;
        }

        // システムのフェーズによって再生する音声を変更する
        switch (systemModel.currentPhase)
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
            case SystemModel.SystemPhase.PlayQuestionAgain:
                yield return StartCoroutine(PlayQuestionAudioAgain());
                systemModel.currentPhase = SystemModel.SystemPhase.WaitTouch;
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
        currentQuestionClip = audioDataBase.GetQuestionAudioData(systemModel.questionID);
        audioSource.clip = currentQuestionClip;
        audioSource.Play();

        Debug.Log("質問音声再生");
        Debug.Log(audioSource.clip.length);

        // 音声の再生終了まで待機
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }


    /// <summary>
    /// 再度質問して録音を促す音声を再生する
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayQuestionAudioAgain()
    {
        //同じ質問を再度再生
        audioSource.clip = currentQuestionClip;
        audioSource.Play();

        Debug.Log("二度目の質問音声再生");

        yield return new WaitUntil(() => !audioSource.isPlaying);
    }


    /// <summary>
    /// 他者の回答の音声を再生する
    /// </summary>
    IEnumerator PlayAnotherAnswerAudio()
    {
        // ランダムに別の回答の音声を再生する
        // なぜか音声が長時間読み込まれてしまうものがあるので，それを避ける
        // 時間はMicrophone.StartのlengthSec引数の値に依存している
        while (true)
        {
            audioSource.clip = audioDataBase.GetRandomAnswerAudioData(systemModel.questionID);
            Debug.Log(audioSource.clip.length);

            // 音声の長さがピッタリ整数値の場合はエラーの可能性があるので再取得
            if (audioSource.clip.length != Mathf.Floor(audioSource.clip.length)) break;
        }
        
        audioSource.Play();

        

        // 音声の再生終了まで待機
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }

    private void Update()
    {
        //Debug.Log(!audioSource.isPlaying);
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


    /// <summary>
    /// 一定時間置きに，ノックを促す音声を再生する
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayPromptKnockAudio()
    {
        yield return new WaitUntil(() => systemModel.currentPhase == SystemModel.SystemPhase.WaitKnock);
        yield return new WaitForSeconds(systemModel.promptKnockIntervalMinutes * 60);

        if(systemModel.currentPhase != SystemModel.SystemPhase.WaitKnock)
        {
            yield return null;
            StartCoroutine(PlayPromptKnockAudio());
            yield break;
        }

        audioSource.clip = audioDataBase.promptKnockAudio;
        audioSource.Play();

        Debug.Log("ノック促し音声再生");

        yield return null;
        StartCoroutine(PlayPromptKnockAudio());
    }
    
}
