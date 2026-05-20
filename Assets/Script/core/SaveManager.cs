using UnityEngine;
using System.IO;

public static class SaveManager
{
    public static SaveData Data;

    static string savePath =
        Path.Combine(
            Application.persistentDataPath,
            // "save.json"
            #if UNITY_EDITOR
            ParrelSync.ClonesManager.IsClone()
            ?
            "save_clone.json"
            :
            "save.json"
            #else
            "save.json"
            #endif
        );

        

    // ==========================
    // LOAD
    // ==========================
    public static void Load()
    {
        if(File.Exists(savePath))
        {
            string json =
                File.ReadAllText(
                    savePath
                );

            Data =
                JsonUtility
                .FromJson<SaveData>(
                    json
                );

            Debug.Log(
                "LOAD OK"
            );
        }
        else
        {
            Data =
                new SaveData();

            Save();

            Debug.Log(
                "CREATE SAVE"
            );
        }
    }

    // ==========================
    // SAVE
    // ==========================
    public static void Save()
    {
        string json =
            JsonUtility.ToJson(
                Data,
                true
            );

        File.WriteAllText(
            savePath,
            json
        );

        Debug.Log(
            savePath
        );
    }
}