using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLTools
{
    partial class Program
    {
        public static string _databaseName = "";
        public static string _serverName = ".";
        public static string _outputDirectory = @"c:\temp\";

        static void Main(string[] args)
        {
            try
            {
                GetSettings();

                System.IO.DirectoryInfo directory = SetupOutputDirectory();

                SQLAccess sqlAccess = new SQLAccess($"Server={_serverName};Database={_databaseName};Trusted_Connection=True;");

                foreach (var table in sqlAccess.GetTables())
                {
                    var columns = sqlAccess.GetColumns(table.TableId).ToList();

                    new Creators.SelectCreator().Create(table, columns).Save(directory.FullName);
                    new Creators.UpsertCreator().Create(table, columns).Save(directory.FullName);
                    new Creators.DeleteCreator().Create(table, columns).Save(directory.FullName);
                }

                Console.WriteLine("Complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        private static System.IO.DirectoryInfo SetupOutputDirectory()
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(_outputDirectory);

            if (!directory.Exists)
            {
                directory.Create();
            }

            directory = directory.CreateSubdirectory(_databaseName);
            return directory;
        }

        private static void GetSettings()
        {
            GetServer();
            GetDatabase();
            GetOutputLocation();
        }

        private static void GetOutputLocation()
        {
            Console.WriteLine(@"What output directory? (Leave blank for c:\temp\)");
            string outputDirectory = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                _outputDirectory = outputDirectory;
            }
        }

        private static void GetServer()
        {
            Console.WriteLine("Server name? (Leave blank for local(.))");
            string serverName = Console.ReadLine();

            if (!string.IsNullOrEmpty(serverName))
            {
                _serverName = serverName;
            }
        }

        private static void GetDatabase()
        {
            SQLAccess sqlAccess = new SQLAccess($"Server={_serverName};Database=Master;Trusted_Connection=True;");

            var databases = sqlAccess.GetDatabases();

            Console.WriteLine("Database name?");

            foreach (var database in databases.OrderBy(x => x.Name))
            {
                Console.WriteLine($"({database.Dbid}) {database.Name}");
            }

            if (int.TryParse(Console.ReadLine(), out int dbid))
            {
                _databaseName = databases.FirstOrDefault(x => x.Dbid == dbid)?.Name;

                if (string.IsNullOrEmpty(_databaseName))
                {
                    Console.WriteLine("Failed to find a database with that Id, please enter the number");

                    GetDatabase();
                }
            }
            else
            {
                Console.WriteLine("Please enter the number");

                GetDatabase();
            }
        }
    }
}