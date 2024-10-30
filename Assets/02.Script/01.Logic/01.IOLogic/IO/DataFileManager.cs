using System.IO;
using SCC.JsonIO;
using UnityEngine;

namespace SCC.IO
{
    //!<============================================================================

    public class DataFileManager
    {
        //!<========================================================================

        static DataFileManager instance;
        public static DataFileManager Instance => instance ??= new DataFileManager();

        //!<========================================================================

        public DataFileManager()
        {

        }

        public string ReadFile(string filePath)
        {
            filePath = JsonIO.Util.AppDataFilePath(filePath);

            if (System.IO.File.Exists(filePath) == false)
            {
                return null;
            }

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using var reader = new StreamReader(fileStream);
                    return reader.ReadToEnd();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ReadFile failed! {e.Message}");
            }

            return null;
        }
        public bool HasFile(string filePath)
        {
            filePath = JsonIO.Util.AppDataFilePath(filePath);
            return System.IO.File.Exists(filePath);
        }

        public void WriteFile(string filePath, string data)
        {
            filePath = JsonIO.Util.AppDataFilePath(filePath);

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filePath)) == false)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using var writer = new StreamWriter(fileStream);
                writer.Write(data);
                writer.Flush();
            }
        }

        public T ReadJsonFile<T>(string filePath, System.Reflection.BindingFlags bindingAttr) where T : new()
        {
            return JsonReader.ReadFromString<T>(this.ReadFile(filePath), bindingAttr);
        }

        public void WriteJsonFile(string filePath, object obj, System.Reflection.BindingFlags bindingAttr)
        {
            this.WriteFile(filePath, JsonWriter.WriteToString(obj, bindingAttr));
        }

        public void RemoveFile(string name)
        {
            var filePath = JsonIO.Util.AppDataFilePath(name);
            if (name.Contains("*"))
            {
                var directoryPath = System.IO.Path.GetDirectoryName(filePath);
                var searchPattern = System.IO.Path.GetFileName(name);
                if (System.IO.Directory.Exists(directoryPath) == true)
                {
                    foreach (var file in System.IO.Directory.GetFiles(directoryPath, searchPattern))
                    {
                        System.IO.File.Delete(file);
                        Debug.LogFormat("Delete File {0}", file);
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(filePath) == true)
                {
                    System.IO.File.Delete(filePath);
                    Debug.LogFormat("Delete File {0}", filePath);
                }
            }
        }

        public void WriteFilePath(string filePath, string data)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using var writer = new StreamWriter(fileStream);
                writer.Write(data);
                writer.Flush();
            }
        }
        public string ReadFilePath(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
            {
                return null;
            }

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using var reader = new StreamReader(fileStream);
                    return reader.ReadToEnd();
                }
            }

            catch (System.Exception e)
            {
                Debug.LogErrorFormat("ReadFile failed! {0}", e);
            }

            return null;
        }
    }
}