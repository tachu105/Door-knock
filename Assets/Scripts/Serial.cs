using UnityEngine;
using System.IO.Ports;
using System.Collections;
//エラーが出る場合はProjectSettingsで「.NET Framework」が選択されているか確認

public class Serial : MonoBehaviour
{
    SerialPort serialPort;  //シリアル通信用の変数
    [SerializeField] int comPortNum = 3;    //COMポートの番号
    [SerializeField] int bandRate = 9600;   //通信帯域

    [SerializeField] SystemModel systemModel;

    [SerializeField] bool isDebugSerial = false;

    bool isDoneFirstReceive = false;

    SystemModel.SystemPhase preSystemPhase;


    void Start()
    {
        //COMポート番号とポートレートをArduinoと合わせる
        serialPort = new SerialPort("COM3", 9600);  
        serialPort.Open();  //シリアル通信を開始する
        isDoneFirstReceive = false;

        //1秒ごとにSend()を実行し、Arduinoにデータを送信する
        //InvokeRepeating(nameof(Send), 1, 1);    

        StartCoroutine(Receive());
    }

    void Update()
    {
         //システムのフェーズが変わった場合、バッファを削除する
         //CheckPhaseChange()を参照すると何故か固まる
        if(preSystemPhase != systemModel.currentPhase)
        {
            ClearBuffer();
        }
        preSystemPhase = systemModel.currentPhase;
    }


    /// データを受信する関数
    IEnumerator Receive()
    {
        //シリアル通信が開始されており、かつ、新しい受信データがある場合
        if (serialPort.IsOpen && serialPort.BytesToRead > 0)
        {
            string receivedData = serialPort.ReadLine();  //データを1行読み取る（改行区切り）

            if (isDoneFirstReceive)
            {
                if (isDebugSerial)
                {
                    Debug.Log(receivedData);
                }
                
                SeparateDatas(receivedData);
            }
            else
            {
                isDoneFirstReceive = true;
            }
            
        }

        yield return null;
        StartCoroutine(Receive());
    }

    void SeparateDatas(string originalData)
    {
        string[] separatedData = originalData.Split(" ");
        for(int i = 0; i < separatedData.Length; i++)
        {
            string[] nameAndValue = separatedData[i].Split(":");
            if(nameAndValue.Length != 2 || !(nameAndValue[1] == "0" || nameAndValue[1] == "1"))
            {
                //throw new System.ArgumentOutOfRangeException("シリアル通信で0,1以外のデータを受信しました．");
                return;
            }
            
            switch (nameAndValue[0])
            {
                case "V":
                    systemModel.SetVibrationSensorState(nameAndValue[1] == "1"? true:false);
                    break;
                case "T":
                    systemModel.SetTouchSensorState(nameAndValue[1] == "1" ? true : false);
                    break;
                default:
                    //throw new System.ArgumentOutOfRangeException("シリアル通信で未対応の接頭辞を受信しました．");
                    break;
            }
        }
    }

    /// データを送信する関数
    void Send()
    {
        if (serialPort.IsOpen)  //シリアル通信が開始されている場合
        {
            serialPort.WriteLine("Unity\n");    //データを送信する（改行コードを忘れずに！）
        }
    }

    /// アプリ終了時にシリアル通信を終了する関数
    void OnApplicationQuit()
    {
        serialPort.Close();
    }

    /// <summary>
    /// バッファ削除
    /// </summary>
    void ClearBuffer()
    {
        try
            {
                serialPort.DiscardInBuffer ();
            }
            catch (System.Exception e) {
                Debug.LogWarning (e.Message);
            }
    }
}