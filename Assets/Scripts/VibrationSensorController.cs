using System.Collections;
using UnityEngine;

/// <summary>
/// 振動センサのコントローラー
/// </summary>
public class VibrationSensorController : MonoBehaviour
{
    private SystemModel systemModel;
    private Coroutine resetCountTimer;

    private void Start()
    {
        systemModel = GetComponent<SystemModel>();

        StartCoroutine(CountKnock());
    }


    /// <summary>
    /// ノックの検知を行う
    /// </summary>
    /// <returns></returns>
    IEnumerator CountKnock()
    {
        bool currentState;

        // システムのフェーズがWaitKnockでない場合は再起
        if(systemModel.currentPhase != SystemModel.SystemPhase.WaitKnock) yield break;


        // 振動センサの値の変化を監視
        while(!systemModel.CheckVibrationSensorChange(out currentState))
        {
            yield return null;
        }


        if(currentState)
        {
            systemModel.knockCount++;

            // 一定時間経過後にノックカウントをリセットする機能を更新
            if(resetCountTimer != null) StopCoroutine(resetCountTimer);
            StartCoroutine(ResetKnockCountTimer(systemModel.knockResetTime));

            // 指定回数ノックされた場合は次のフェーズに移行
            if(systemModel.knockCount >= systemModel.knockThreshold)
            {
                systemModel.ChangeNextSystemPhase();    // 次のフェーズに移行
                systemModel.knockCount = 0;
            }
        }
    }


    /// <summary>
    /// 指定時間以内にノックがない場合にノックカウントをリセットする
    /// </summary>
    /// <param name="waitTime">リセット時間</param>
    /// <returns></returns>
    IEnumerator ResetKnockCountTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        systemModel.knockCount = 0;
    }


}
