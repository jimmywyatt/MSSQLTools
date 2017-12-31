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
        public static string _directoryPath = @"c:\temp\";

        static void Main(string[] args)
        {
            try
            {
                GetSettingsFromPrompt();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static System.IO.DirectoryInfo SetupOutputDirectory()
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(_directoryPath);

            if (!directory.Exists)
            {
                directory.Create();
            }

            directory = directory.CreateSubdirectory(_databaseName);
            return directory;
        }

        private static void GetSettingsFromPrompt()
        {
            Console.Clear();

            switch (GetRunTypes())
            {
                case RunTypes.Generate:
                    GetServer();
                    GetDatabase();
                    GetOutputLocation();

                    System.IO.DirectoryInfo directory = SetupOutputDirectory();

                    SQLAccess sqlAccess = new SQLAccess($"Server={_serverName};Database={_databaseName};Trusted_Connection=True;");
                    
                    foreach (var table in sqlAccess.GetTables())
                    {
                        var columns = sqlAccess.GetColumns(table.TableId).ToList();

                        new Creators.SelectCreator().Create(table, columns).Save(directory.FullName);
                        new Creators.UpsertCreator().Create(table, columns).Save(directory.FullName);
                        new Creators.DeleteCreator().Create(table, columns).Save(directory.FullName);
                    }

                    Console.Write("Press any key to continue");
                    Console.ReadKey();
                    
                    break;
                case RunTypes.Run:
                    GetServer();
                    GetDatabase();
                    GetScriptsLocation();

                    new Runners.RunScripts().Run(_directoryPath, $"Server={_serverName};Database={_databaseName};Trusted_Connection=True;");

                    Console.Write("Press any key to continue");
                    Console.ReadKey();

                    break;
                case RunTypes.Exit:
                    return;

                default:
                    Console.WriteLine($@"Not sure what you're trying to do");

                    break;
            }

            GetSettingsFromPrompt();
        }

        private static RunTypes GetRunTypes()
        {
            Console.WriteLine($@"What do you want to do?");
            Console.WriteLine($@"(0) Generate Scripts");
            Console.WriteLine($@"(1) Run Scripts");
            Console.WriteLine($@"(100) Exit");

            if (int.TryParse(Console.ReadLine(), out int result))
            {
                return (RunTypes)result;
            }
            else
            {
                Console.WriteLine($@"Please choose a number?");

                return GetRunTypes();
            }
        }

        private static void GetScriptsLocation()
        {
            Console.WriteLine($@"What scripts directory? (Leave blank for {_directoryPath})");
            string outputDirectory = Console.ReadLine();

            if (!string.IsNullOrEmpty(outputDirectory))
            {
                _directoryPath = outputDirectory;
            }
        }

        private static void GetOutputLocation()
        {
            Console.WriteLine($@"What output directory? (Leave blank for {_directoryPath})");
            string outputDirectory = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                _directoryPath = outputDirectory;
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

            Console.WriteLine("What database?");

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