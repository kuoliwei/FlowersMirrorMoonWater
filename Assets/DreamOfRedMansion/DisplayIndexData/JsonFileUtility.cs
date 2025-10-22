using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class JsonFileUtility
{
    /// <summary>
    /// 儲存任意 Serializable 型別的資料為 JSON 檔
    /// </summary>
    public static async Task SaveToFileAsync<T>(T data, string savePath, string fileName)
    {
        string fullPath = Path.Combine(savePath, fileName);

        string json = JsonUtility.ToJson(data, true);

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        await File.WriteAllTextAsync(fullPath, json);
        Debug.Log($"[Json Save] Data saved to: {fullPath}");
    }
    public static void SaveToFile<T>(T data, string savePath, string fileName)
    {
        string fullPath = Path.Combine(savePath, fileName);
        string json = JsonUtility.ToJson(data, true);

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        File.WriteAllText(fullPath, json);
        Debug.Log($"[Json Save Sync] Saved to: {fullPath}");
    }

    /// <summary>
    /// 從 JSON 檔讀取資料為泛型物件
    /// </summary>
    public static async Task<T?> LoadFromFileAsync<T>(string savePath, string fileName) where T : struct
    {
        string fullPath = Path.Combine(savePath, fileName);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[Json Load] File not found: {fullPath}");
            return null;
        }

        string json = await File.ReadAllTextAsync(fullPath);
        T data = JsonUtility.FromJson<T>(json);
        return data;
    }
}
