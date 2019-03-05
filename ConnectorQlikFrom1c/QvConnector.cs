using QlikView.Qvx.QvxLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace СonnectorQlikFrom1c {

    [ComVisible(true)]
    class QvConnector : QvxConnection {

        private string go_connString;
        private string go_query, go_qvTableName;

        public override void Init() {
            QvxLog.SetLogLevels(true, true);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "QvxConnection.Init() avg");

            if (MParameters == null) {
                return;
            }

            MParameters.TryGetValue("QV_Table", out go_qvTableName);
            MParameters.TryGetValue("Query", out go_query);
            MParameters.TryGetValue("name_connect", out string lo_connectionName);

            if (string.IsNullOrEmpty(go_qvTableName)) {
                return;
            }

            if (string.IsNullOrEmpty(go_query)) {
                return;
            }

            go_connString = ConnectString();
            List<string> lo_fieldsName = new List<string>();
            QvxField[] v8Fileds = null;


            /*смотрим, если есть сохраненные данные о подключении - грузим*/
            if (string.IsNullOrEmpty(lo_connectionName)) {
                lo_connectionName = "QlikView";
            }
            
            ConnectData lo_connectData = null;
            if (ConnectData.IsExistJson(Properties.Settings.Default.QlikConnectJsonFolder + lo_connectionName)) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Load Json file");
                lo_connectData = ConnectData.LoadFromJson(Properties.Settings.Default.QlikConnectJsonFolder + lo_connectionName);

                /**/
                if (lo_connectData.ConnectionString.Equals(go_connString)) {
                    go_query = lo_connectData.Query;

                    go_qvTableName = lo_connectData.go_tablesFields.Keys.ElementAt(0);
                    lo_fieldsName = lo_connectData.go_tablesFields[go_qvTableName];
                }
            }

            /*если сохраненных данных нет - подключаемся и тащим*/
            if (lo_fieldsName.Count == 0) {

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Connect 1c COM");
                Type oType = Type.GetTypeFromProgID("V83.COMConnector");
                using (Object1C lo_v8 = (oType == null) ? null : new Object1C(Activator.CreateInstance(oType))) {
                    if (lo_v8 == null) {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Невозможно создать объект типа V83.COMConnector");
                        Debug.WriteLine("Невозможно создать объект типа V83.COMConnector");
                        return;
                    }

                    /*соединяемся с базой данных*/
                    using (Object1C lo_connection = lo_v8.ExecuteFunction("Connect", new object[] { go_connString })) {
                        if (lo_connection == null) {
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Невозможно подключиться к системе: " + go_connString);
                            Debug.WriteLine("Невозможно подключиться к системе: " + go_connString);
                            return;
                        }

                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Подключение к 1с выполнено успешно: ");
                        try {
                            using (Object1C lo_query = lo_connection.ExecuteFunctionEx("NewObject", new object[] { "ПостроительЗапроса" })) {
                                lo_query.SetProperty("Текст", new object[] { go_query });

                                using (Object1C lo_queryRes = lo_query.GetPropertyEx("Результат")) {

                                    using (Object1C lo_queryResColumn = lo_queryRes.GetPropertyEx("Колонки")) {

                                        /*количество колонок в таблице, которую вернет запрос*/
                                        int lv_countColumn = 0;
                                        using (Object1C lo_cntColumn = lo_queryResColumn.ExecuteFunctionEx("Количество")) {
                                            lv_countColumn = Convert.ToInt32(lo_cntColumn.ToString());
                                        }

                                        /*названия колонок*/
                                        for (int lv_i = 0; lv_i < lv_countColumn; ++lv_i) {
                                            string lo_columnName = string.Empty;

                                            using (Object1C lo_arrayField = lo_queryResColumn.ExecuteFunction("Получить", new object[] { lv_i })) {
                                                using (Object1C lo_fieldName = lo_arrayField.GetProperty("Имя")) {
                                                    lo_columnName = lo_fieldName.ToString();
                                                }
                                            }

                                            lo_fieldsName.Add(lo_columnName);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception lo_ex) {
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.ToString());
                            Debug.WriteLine(lo_ex.ToString());
                        }
                        finally {
                            if (lo_connection != null)
                                lo_connection.Dispose();
                        }
                    }
                }

                try {
                    lo_connectData = new ConnectData {
                        ConnectionString = go_connString,
                        Query = go_query,
                        go_tablesFields = new Dictionary<string, List<string>> {
                            [go_qvTableName] = lo_fieldsName
                        }
                    };
                    lo_connectData.SaveToJson(Properties.Settings.Default.QlikConnectJsonFolder + lo_connectionName);
                } 
                catch(Exception lo_ex) {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.ToString());
                    Debug.WriteLine(lo_ex.ToString());
                }
            }


            /*добавляем поля в таблицу Qlik View*/
            v8Fileds = new QvxField[lo_fieldsName.Count];
            for (int lv_i = 0; lv_i < lo_fieldsName.Count; ++lv_i) {
                v8Fileds.SetValue(
                    new QvxField(
                        lo_fieldsName[lv_i],
                        QvxFieldType.QVX_TEXT,
                        QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA,
                        FieldAttrType.ASCII
                    ), lv_i
                );
            }

            MTables = new List<QvxTable> {
                new QvxTable {
                    TableName = go_qvTableName,
                    GetRows = GetData,
                    Fields = v8Fileds
                }
            };
        }

        private string ConnectString() {
            /*путь к файловой базе данных*/
            MParameters.TryGetValue( "File", out string lo_file );

            /*данные к серверной базе данных*/
            MParameters.TryGetValue( "Srvr", out string lo_srvr );
            MParameters.TryGetValue( "Ref",  out string lo_ref  );

            /*если задана доменная авторизация, то поля логин и пароль пустые, в строку подключения не передаем*/
            MParameters.TryGetValue( "UserDB",     out string lo_usr );
            MParameters.TryGetValue( "PasswordDB", out string lo_pwd );

            if (!string.IsNullOrEmpty(lo_usr) && lo_usr.Equals("undefined")) {
                lo_usr = string.Empty;
                lo_pwd = string.Empty;
            }

            /*файловая база данных*/
            if (!string.IsNullOrEmpty(lo_file)) {
                if (string.IsNullOrEmpty(lo_usr))
                    return string.Format("File='{0}';", lo_file);
                else
                    return string.Format("File='{0}';Usr='{1}';Pwd='{2}';", lo_file, lo_usr, lo_pwd);
            }

            /*серверная база данных*/
            if (string.IsNullOrEmpty(lo_usr))
                return string.Format("Srvr='{0}';Ref='{1}';", lo_srvr, lo_ref);
            else
                return string.Format("Srvr='{0}';Ref='{1}';Usr='{2}';Pwd='{3}';", lo_srvr, lo_ref, lo_usr, lo_pwd);

        }

        private IEnumerable<QvxDataRow> GetData() {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "GetData() avg");

            QvxTable table = FindTable(go_qvTableName, MTables);

            if (table == null)
                yield return null;

            Type oType = Type.GetTypeFromProgID("V83.COMConnector");
            using (Object1C lo_v8 = (oType == null) ? null : new Object1C(Activator.CreateInstance(oType))) {
                if (lo_v8 == null) {
                    Debug.WriteLine("Невозможно создать объект типа V83.COMConnector");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Невозможно создать объект типа V83.COMConnector");
                    yield return new QvxDataRow();
                }

                /*соединяемся с базой данных*/
                using (Object1C lo_connection = lo_v8.ExecuteFunction("Connect", new object[] { go_connString })) {
                    if (lo_connection == null) {
                        Debug.WriteLine("Невозможно подключиться к системе: " + go_connString);
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Невозможно подключиться к системе: " + go_connString);
                        yield return new QvxDataRow();
                    }

                    /*получаем результат запроса, возвращаем построчно*/
                    using (Object1C lo_query = lo_connection.ExecuteFunctionEx("NewObject", new object[] { "Query" })) {
                        lo_query.SetProperty("Текст", new object[] { go_query });

                        using (Object1C lo_queryResult = lo_query.ExecuteFunctionEx("Выполнить")) {
                            using (Object1C lo_queryResSelection = lo_queryResult.ExecuteFunctionEx("Выбрать")) {

                                while ((bool)lo_queryResSelection.ExecuteFunctionEx("Следующий").CurrentObject) {
                                    QvxDataRow lo_row = new QvxDataRow();
                                    foreach (var tField in table.Fields) {
                                        lo_row[tField] = lo_queryResSelection.GetPropertyEx(tField.FieldName).ToString();
                                    }
                                    yield return lo_row;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> qvxTables) {
            /* Make sure to remove your quotesuffix, quoteprefix, 
             * quotesuffixfordoublequotes, quoteprefixfordoublequotes
             * as defined in selectdialog.js somewhere around here.
             * 
             * In this example it is an escaped double quote that is
             * the quoteprefix/suffix
             */
            query = Regex.Replace(query, "\\\"", "");

            return base.ExtractQuery(query, qvxTables);
        }

    }
}
