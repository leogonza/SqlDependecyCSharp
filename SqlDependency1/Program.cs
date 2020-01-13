using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependency1 {
  class Program {
    static void Main(string[] args) {

      var opt = "";
      SqlDepManager sqlDepManager = new SqlDepManager();

      do {

        sqlDepManager.StartNotifications();

        var k = Console.ReadKey();
        opt = k.KeyChar.ToString();

      } while (opt != "x");


    }

    private class LineChange {
      public int ChangeId { get; set; }
      public int GameNum { get; set; }
      public int PeriodNumber { get; set; }
      public int Store { get; set; }

      public override string ToString() {
        return "Line Change: " + ChangeId + ", GameNum: " + GameNum + ", PeriodNumber: " + PeriodNumber + ", Store: " + Store;
      }

    }

    public class SqlDepManager {

      Notify<LineChange> notifier;

      private void ItemReceived_Event(object sender, EventArgs args) {
        Console.WriteLine(args);
        foreach (var c in notifier.CollectionReturn) {
          Console.WriteLine(c.ToString());
        }
      }

      public void StartNotifications() {
        if (notifier != null) return;
        notifier = new Notify<LineChange>("Server=GBSADMIN;Database=TestBase;User Id=TestBaseUser;Password=gbs4dm1n@DBM;", 
          "SELECT ChangeId, GameNum, PeriodNumber, Store FROM dbo.tbLastLineChange", "ChangeId");

        notifier.ItemReceived += ItemReceived_Event;

      }
    }



  }
}
