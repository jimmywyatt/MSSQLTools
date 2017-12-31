using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLTools.Runners
{
    public class RunScripts
    {
        public void Run(string directoryPath, string connectionString)
        {
            var directoryInfo = new System.IO.DirectoryInfo(directoryPath);
            var sqlAccess = new SQLAccess(connectionString);

            foreach (var file in directoryInfo.GetFiles("*.sql", System.IO.SearchOption.AllDirectories))
            {
                try
                {
                    sqlAccess.RunScript(file.FullName);

                    Helpers.LogHelper.Log4Net.Debug($"Run {file.FullName}");
                }
                catch (Exception ex)
                {
                    Helpers.LogHelper.Log4Net.Error(file.FullName, ex);
                }
            }
        }
    }
}
