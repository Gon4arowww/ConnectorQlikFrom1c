using QlikView.Qvx.QvxLibrary;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace СonnectorQlikFrom1c {

    [ComVisible(true)]
    static class Program {
 
        // <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread] /*[MTAThread]*/
        static void Main(string[] args) {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, string.Format("========== start QvxServer: args0='{0}' args1='{1}' ==========", args[0], args[1]));

            /*директория для сохранения Json с данными о подключениях*/
            if (!Directory.Exists(Properties.Settings.Default.QlikConnectJsonFolder)) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, string.Format("create folder from Json", Properties.Settings.Default.QlikConnectJsonFolder));
                Directory.CreateDirectory(Properties.Settings.Default.QlikConnectJsonFolder);
            }

            /*запускаем коннектор*/
            if (args != null && args.Length >= 2) {
                new Connector().Run(args[0], args[1]);
            }
        }
    }
}
