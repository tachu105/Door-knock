using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AudioLoader : MonoBehaviour
{
    private string audiosFolderPath; //audiosのフォルダのパス 

    private List<AudioClip> audioClips = new List<AudioClip>();



    /// <summary>
    /// 指定フォルダから，オーディオファイルをロードして，リストに格納する
    /// </summary>
    /// <param name="folderName">ロードするオーディオフォルダ名</param>
    /// <param name="maxListCount">オーディオクリップリストの最大数</param>
    /// <param name="allFileCount">保存されているオーディオファイルの総数</param>
    public void LoadAudioClips(string folderName, int maxListCount ,ref List<AudioClip> audioClips, out int allFileCount)
    {
        audiosFolderPath = Application.dataPath + "/Audios/"; //Audiosフォルダのパスを取得


        DirectoryInfo directoryInfo = new DirectoryInfo(audiosFolderPath + folderName);
        FileInfo[] files = directoryInfo.GetFiles("*.wav");

        allFileCount = files.Length;

        // 最新のファイルを優先的に取得するために、ファイルを作成日時で並び替えます。
        files = files.OrderByDescending(f => f.CreationTime).ToArray();

        foreach (FileInfo file in files)
        {
            if (audioClips.Count >= maxListCount)
                break; // 最大数に達したらループを抜ける

            // AudioClipを作成し、リストに追加します。
            AudioClip audioClip = LoadAudioClipFromFile(file.FullName);
            if (audioClip != null)
            {
                audioClips.Add(audioClip);
            }
        }
    }

    AudioClip LoadAudioClipFromFile(string filePath)
    {
        // WAVファイルをロードしてAudioClipに変換します。
        WWW www = new WWW("file:///" + filePath);
        while (!www.isDone) { }
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("Failed to load audio file: " + filePath + ", Error: " + www.error);
            return null;
        }
        return www.GetAudioClip(false, false, AudioType.WAV);
    }
}
