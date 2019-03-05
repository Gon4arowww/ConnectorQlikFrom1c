using System.Collections.Generic;
using System.Windows.Interop;
using QlikView.Qvx.QvxLibrary;

namespace СonnectorQlikFrom1c {
    internal class Connector : QvxServer {
        private QvxConnection go_qvConnector;

        public override QvxConnection CreateConnection() {
            go_qvConnector = new QvConnector();
            return go_qvConnector;
        }

        #region for Qlik View
        public override string CreateConnectionString() {
            QvxLog.SetLogLevels(true, true);
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "QvxServer.CreateConnectionString() avg");

            BaseConnector lo_baseConnector = CreateBaseConnector(go_qvConnector?.MParameters);
            lo_baseConnector.ShowDialog();

            if (lo_baseConnector.DialogResult.Equals(true)) {
                if (lo_baseConnector.GetServerBase()) {
                    if (string.IsNullOrEmpty(lo_baseConnector.GetUser())) {
                        return string.Format("Srvr={0};Ref={1};Query={2};QV_Table={3}",
                            lo_baseConnector.GetBaseLocation(), lo_baseConnector.GetBaseName(),
                            lo_baseConnector.GetQueryText(), lo_baseConnector.GetQvTable()
                        );
                    }
                    else {
                        return string.Format("Srvr={0};Ref={1};UserDB={2};PasswordDB={3};Query={4};QV_Table={5}",
                            lo_baseConnector.GetBaseLocation(), lo_baseConnector.GetBaseName(),
                            lo_baseConnector.GetUser(), lo_baseConnector.GetPassword(),
                            lo_baseConnector.GetQueryText(), lo_baseConnector.GetQvTable()
                        );
                    }
                }
                else {
                    if (string.IsNullOrEmpty(lo_baseConnector.GetUser())) {
                        return string.Format("File={0};Query={1};QV_Table={2}",
                            lo_baseConnector.GetBaseLocation(), lo_baseConnector.GetQueryText(),
                            lo_baseConnector.GetQvTable()
                        );
                    }
                    else {
                        return string.Format("File={0};UserDB={1};PasswordDB={2};Query={3};QV_Table={4}",
                            lo_baseConnector.GetBaseLocation(), lo_baseConnector.GetUser(),
                            lo_baseConnector.GetPassword(), lo_baseConnector.GetQueryText(),
                            lo_baseConnector.GetQvTable()
                        );
                    }
                }

            }
            return string.Empty;
        }

        /*форма для ввода данных подключения*/
        private BaseConnector CreateBaseConnector(Dictionary<string, string> io_params = null) {
            var base_connector = new BaseConnector(io_params);
            var wih = new WindowInteropHelper(base_connector) {
                Owner = MParentWindow
            };
            return base_connector;
        }
        #endregion

        #region for Qlik Sense 
        /**
         * QlikView 12 classes
         */
        public override string HandleJsonRequest(string io_method, string[] io_userParameters, QvxConnection io_connection) {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "HandleJsonRequest, method: " + io_method);
            QvDataContractResponse lo_response;

            switch (io_method) {
                case "getInfo":
                    lo_response = GetInfo();
                    break;
                case "getDatabases":
                    lo_response = GetDatabases();
                    break;
                case "getTables":
                    if ((io_connection.MTables == null) && (io_connection.MParameters != null)) {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "QvxConnection.ReInit()");
                        io_connection.Init();
                    }
                    lo_response = GetTables(io_connection, io_userParameters[0], io_userParameters[1]);
                    break;
                case "getFields":
                    if ((io_connection.MTables == null) && (io_connection.MParameters != null)) {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "QvxConnection.ReInit()");
                        io_connection.Init();
                    }
                    lo_response = GetFields(io_connection, io_userParameters[0], io_userParameters[1], io_userParameters[2]);
                    break;
                default:
                    lo_response = new Info { qMessage = "Unknown command" };
                    break;
            }
            return ToJson(lo_response);    // serializes response into JSON string
        }

        public QvDataContractResponse GetInfo() {
            return new Info {
                qMessage = "Коннектор c# подключен"
            };
        }

        public QvDataContractResponse GetDatabases() {
            return new QvDataContractDatabaseListResponse {
                qDatabases = new Database[] {
                    new Database {
                        qName = "Define DataBase"
                    }
                }
            };
        }

        public QvDataContractResponse GetTables(QvxConnection io_connection, string io_database, string io_owner) {
            return new QvDataContractTableListResponse {
                qTables = io_connection.MTables
            };
        }

        public QvDataContractResponse GetFields(QvxConnection io_connection, string io_database, string io_owner, string io_table) {
            var lo_currentTable = io_connection.FindTable(io_table, io_connection.MTables);
            return new QvDataContractFieldListResponse {
                qFields = (lo_currentTable != null) ? lo_currentTable.Fields : new QvxField[0]
            };
        }
        #endregion
    }
}
