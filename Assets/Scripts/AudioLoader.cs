using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class AudioLoader
{
    /// <summary>
    /// 指定フォルダから，オーディオファイルをロードして，リストに格納する
    /// </summary>
    /// <param name="folderPath">ロードするオーディオフォルダ名</param>
    /// <param name="maxListCount">オーディオクリップリストの最大数</param>
    /// <param name="allFileCount">保存されているオーディオファイルの総数</param>
    public static void LoadAudioClips(string folderPath, int maxListCount ,ref List<AudioClip> audioClips, out int allFileCount)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = directoryInfo.GetFiles("*.wav");

        allFileCount = files.Length;

        // 最新のファイルを優先的に取得するためにファイルを作成日時で並び替え
        files = files.OrderByDescending(f => f.CreationTime).ToArray();

        // ファイルから指定数のAudioClipをロード
        foreach (FileInfo file in files)
        {
            if (audioClips.Count >= maxListCount)   break;

            AudioClip audioClip = LoadAudioClipFromFile(file.FullName);
            if (audioClip != null)  audioClips.Add(audioClip);
        }
    }

    /// <summary>
    /// 指定フォルダから，WAVファイルをロードしてAudioClipに変換するメソッド
    /// </summary>
    /// <param name="filePath"> WAVファイルのパス </param>
    static AudioClip LoadAudioClipFromFile(string filePath)
    {
        WWW www = new WWW("file:///" + filePath);
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("Failed to load audio file: " + filePath + ", Error: " + www.error);
            return null;
        }
        return www.GetAudioClip(false, false, AudioType.WAV);
    }
}
