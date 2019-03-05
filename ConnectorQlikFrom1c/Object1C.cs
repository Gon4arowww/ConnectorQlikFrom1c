using QlikView.Qvx.QvxLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace СonnectorQlikFrom1c {
    public class Object1C : IDisposable {

        [MarshalAs(UnmanagedType.IDispatch)] private object go_currentObject;
        public object CurrentObject {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            get { return go_currentObject; }
            set { go_currentObject = value; }
        }

        #region Init 
        public Object1C() {

        }

        public Object1C(object io_object) {
            if (io_object is Object1C) {
                Debug.WriteLine("ERROR! io_object is Object1c");
                return;
            }
            CurrentObject = io_object;
        }

        ~Object1C() {
            try {
                Clear();
            }
            catch (Exception lo_ex) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.Message);
                Debug.WriteLine(lo_ex.Message);
            }
        }

        //internal void Clear() {
        //    if (CurrentObject == null) return;
        //    Marshal.Release(Marshal.GetIDispatchForObject(CurrentObject));
        //    Marshal.ReleaseComObject(CurrentObject);
        //    CurrentObject = null;

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //}

        internal void Clear() {
            if (CurrentObject == null)
                return;

            if (Marshal.IsComObject(CurrentObject))
                Marshal.ReleaseComObject(CurrentObject);

            CurrentObject = null;
        }

        public void Dispose() {
            Clear();
        }
        #endregion

        public string Type {
            get {
                return ExecuteFunction("Метаданные").ExecuteFunction("ПолноеИмя").ToString();
            }
        }

        #region GetProperty
        public Object1C GetProperty(string io_nameParam) {
            object lo_resObject = null;
            try {
                lo_resObject = CurrentObject.GetType().InvokeMember(io_nameParam, BindingFlags.GetProperty, null, CurrentObject, null);
            }
            catch (Exception lo_ex) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.Message);
                Debug.WriteLine(lo_ex.Message);
            }

            return new Object1C(lo_resObject);
        }

        public Object1C GetPropertyEx(string io_nameParam) {
            return new Object1C(CurrentObject.GetType().InvokeMember(io_nameParam, BindingFlags.GetProperty, null, CurrentObject, null));
        }
        #endregion

        #region SetProperty
        public void SetProperty(string io_nameProperty, object[] io_params) {
            try {
                CurrentObject.GetType().InvokeMember(io_nameProperty, BindingFlags.Public | BindingFlags.SetProperty, null, CurrentObject, io_params);
            }
            catch (Exception lo_ex) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.Message);
                Debug.WriteLine(lo_ex.Message);
            }
        }

        public void SetPropertyEx(string io_nameProperty, object[] io_params) {
            CurrentObject.GetType().InvokeMember(io_nameProperty, BindingFlags.Public | BindingFlags.SetProperty, null, CurrentObject, io_params);
        }
        #endregion

        #region ExecuteFunction
        public Object1C ExecuteFunction(string io_nameFunction, object[] io_params = null) {
            try {
                return new Object1C(CurrentObject.GetType().InvokeMember(io_nameFunction, BindingFlags.Public | BindingFlags.InvokeMethod, null, CurrentObject, io_params));
            }
            catch (Exception lo_ex) {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, lo_ex.Message);
                Debug.WriteLine(lo_ex.Message);
            }
            return null;
        }

        public Object1C ExecuteFunctionEx(string io_nameFunction, object[] io_params = null) {
            return new Object1C(CurrentObject.GetType().InvokeMember(io_nameFunction, BindingFlags.Public | BindingFlags.InvokeMethod, null, CurrentObject, io_params));
        }
        #endregion

        #region ExecuteProcedure
        public void ExecuteProcedure(string io_nameProcedure) {
            try {
                CurrentObject.GetType().InvokeMember(io_nameProcedure, BindingFlags.InvokeMethod, null, CurrentObject, null);
            }
            catch (Exception lo_ex) {
                Debug.WriteLine(lo_ex.Message);
            }
        }

        public void ExecuteProcedureEx(string io_nameProcedure) {
            CurrentObject.GetType().InvokeMember(io_nameProcedure, BindingFlags.InvokeMethod, null, CurrentObject, null);
        }
        #endregion

        public override string ToString() {
            return CurrentObject != null ? CurrentObject.ToString() : "avg null Object1С";
        }
    }
}
