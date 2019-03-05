using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace СonnectorQlikFrom1c {
    [Serializable]
    public class ConnectData {

        public string ConnectionString { get; set; }

        public string Query { get; set; }

        /*название таблицы => колонки*/
        public Dictionary<string, List<string>> go_tablesFields;

        public void SaveToJson(string io_fileName) {
            string lo_fileName = (io_fileName.EndsWith(".json")) ? io_fileName : io_fileName + ".json";
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(ConnectData));
            using (FileStream fs = new FileStream(lo_fileName, FileMode.OpenOrCreate)) {
                jsonFormatter.WriteObject(fs, this);
            }
        }

        public static ConnectData LoadFromJson(string io_fileName) {
            string lo_fileName = (io_fileName.EndsWith(".json")) ? io_fileName : io_fileName + ".json";
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(ConnectData));
            using (FileStream fs = new FileStream(lo_fileName, FileMode.OpenOrCreate)) {
                return (ConnectData)jsonFormatter.ReadObject(fs);
            }
        }

        public static bool IsExistJson(string io_fileName) {
            string lo_fileName = (io_fileName.EndsWith(".json")) ? io_fileName : io_fileName + ".json";
            return File.Exists(lo_fileName);
        }

    }
}
