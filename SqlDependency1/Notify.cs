using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.Data;
using System.Collections;

namespace SqlDependency1 {

  public class Notify<T> where T : new() {

    //assign connection string and sql command for listening 
    public Notify(string ConnectionString, string Command, string controlField = null) {
      this.ConnectionString = ConnectionString;
      this._controlField = controlField;
      _controlConsecutive = 0;
      CollectionReturn = new List<T>();
      this.Command = Command;
      this.NotifyNewItem();
    }

    //event handler to notify the calling class
    public event EventHandler ItemReceived;
    private bool isFirst = true;
    private string _controlField;
    private int _controlConsecutive;
    public string ConnectionString { get; set; }
    public string Command { get; set; }
    //rows to return as a collection 
    public List<T> CollectionReturn { get; set; }

    //check if user has permission 
    private bool DoesUserHavePermission() {
      try {
        SqlClientPermission clientPermission =
               new SqlClientPermission(PermissionState.Unrestricted);
        clientPermission.Demand();
        return true;
      }
      catch {
        return false;
      }
    }
    //initiate notification 
    private void NotifyNewItem() {
      if (DoesUserHavePermission()) {
        if (isFirst) {
          SqlDependency.Stop(ConnectionString);
          SqlDependency.Start(ConnectionString);
        }
        try {
          using (SqlConnection conn = new SqlConnection(ConnectionString)) {
            var cmd = Command;
            if (!string.IsNullOrEmpty(_controlField)) {
              cmd += (cmd.Contains(" WHERE ") ? " AND " : " WHERE ") + _controlField + " > " + _controlConsecutive;
            }

            using (SqlCommand com = new SqlCommand(cmd, conn)) {
              com.Notification = null;
              SqlDependency dep = new SqlDependency(com);
              //subscribe to sql dependency event handler
              dep.OnChange += new OnChangeEventHandler(dep_OnChange);
              conn.Open();
              using (var reader = com.ExecuteReader()) {
                //convert reader to list<T> using reflection 
                while (reader.Read()) {
                  var obj = Activator.CreateInstance<T>();
                  var properties = obj.GetType().GetProperties();
                  foreach (var property in properties) {
                    try {
                      if (reader[property.Name] != DBNull.Value) {
                        property.SetValue(obj, reader[property.Name], null);
                      }
                      if (!string.IsNullOrEmpty(_controlField) && reader[_controlField] != DBNull.Value) {
                        int.TryParse(reader[_controlField].ToString(), out _controlConsecutive);
                      }
                    }
                    catch (Exception ex) {
                      Console.WriteLine(ex.Message);
                    }
                  }
                  CollectionReturn.Add(obj);
                }
              }
            }
          }
        }
        catch (Exception ex) {
          Console.WriteLine(ex.Message);
        }
      }
    }

    //event handler
    private void dep_OnChange(object sender, SqlNotificationEventArgs e) {
      isFirst = false;
      var sometype = e.Info;
      //call notify item again 
      NotifyNewItem();

      switch (e.Info) {
        case SqlNotificationInfo.Invalid:
          Console.WriteLine("The above notification query is not valid.");
          break;
        case SqlNotificationInfo.Insert:
          onItemReceived(e);
          Console.WriteLine("Notification Info: " + e.Info);
          Console.WriteLine("Notification source: " + e.Source);
          Console.WriteLine("Notification type: " + e.Type);
          break;
        default:
          Console.WriteLine("Notification Info: " + e.Info);
          Console.WriteLine("Notification source: " + e.Source);
          Console.WriteLine("Notification type: " + e.Type);
          break;
      }

        
      SqlDependency dep = sender as SqlDependency;
      //unsubscribe 
      dep.OnChange -= new OnChangeEventHandler(dep_OnChange);
    }
    private void onItemReceived(SqlNotificationEventArgs eventArgs) {
      EventHandler handler = ItemReceived;
      if (handler != null)
        handler(this, eventArgs);
    }
  }
}
