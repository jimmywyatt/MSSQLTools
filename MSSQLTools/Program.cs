using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLTools
{
    partial class Program
    {
        static void Main(string[] args)
        {
            string connectionString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
            string outputDirectory = System.Configuration.ConfigurationManager.AppSettings["OutputDirectory"];

            SQLAccess sqlAccess = new SQLAccess(connectionString);
            
            foreach (var table in sqlAccess.GetTables())
            {
                var columns = sqlAccess.GetColumns(table.TableId).ToList();

                SelectCreator.Create(table, columns, outputDirectory);
                UpsertCreator.Create(table, columns, outputDirectory);
                DeleteCreator.Create(table, columns, outputDirectory);
            }
        }

    }
}
