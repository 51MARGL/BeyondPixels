using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace BeyondPixels.ECS.Components.SaveGame
{
    public interface ISaveData { }

    public class SaveGameManager
    {
        public static string SaveFolder =
            Path.Combine(Application.persistentDataPath, "SaveGame");

        public static string SaveFile =
            Path.Combine(SaveFolder, "savegame.save");

        public static bool SaveExists => File.Exists(SaveFile);

        public static void SaveData(ISaveData saveData)
        {
            var saveBckpPath = SaveFile + Guid.NewGuid() + ".bckp";
            try
            {
                Directory.CreateDirectory(SaveFolder);

                if (File.Exists(SaveFile))
                {
                    File.Move(SaveFile, saveBckpPath);
                }

                var binaryFormatter = new BinaryFormatter();

                using (var fileStream = new FileStream(SaveFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    binaryFormatter.Serialize(fileStream, saveData);
                }

                File.Delete(saveBckpPath);
            }
            catch (Exception)
            {
                if (File.Exists(saveBckpPath))
                {
                    File.Move(saveBckpPath, SaveFile);
                }
            }
        }

        public static object LoadData()
        {
            if (!SaveExists)
            {
                return null;
            }

            try
            {
                var binaryFormatter = new BinaryFormatter();

                using (var fileStream = new FileStream(SaveFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return binaryFormatter.Deserialize(fileStream);
                }
            }
            catch (Exception) { }

            return null;
        }

        public static void DeleteSave()
        {
            if (SaveExists)
            {
                try
                {
                    File.Delete(SaveFile);
                }
                catch (Exception) { }
            }
        }
    }
}
