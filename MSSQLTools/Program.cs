using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLTools
{
    partial class Program
    {
        public static string _databaseName = "";
        public static string _serverName = ".";
        public static string _path = @"c:\temp\";
        public static RunTypes _action = RunTypes.NotSet;

        static void Main(string[] args)
        {
            //var a = new System.IO.FileInfo(@"c:\test\");
            //Console.WriteLine(a.Name);
            //Console.ReadKey();
            //return;

            bool show_help = false;

            var p = new OptionSet() {
                { "h|help",  "Show this message and exit", v => show_help = v != null },
                { "g|generate",  "Generate Select/Upsert/Delete scripts", v => _action = RunTypes.Generate },
                { "r|run",  "Run scripts in a directory", v => _action = RunTypes.Run },
                { "b|backup",  "Backup database to location", v => _action = RunTypes.Backup },
                { "p|path=",  "Path to output to or run against", v => _path = v },
                { "d|database=",  "Name of the database", v => _databaseName = v },
                { "s|server=",  "Name of the server", v => _serverName = v }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `mssqltools --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);

                return;
            }

            try
            {
                if (_action == RunTypes.NotSet)
                {
                    GetSettingsFromPrompt();
                }
                else
                {
                    switch (_action)
                    {
                        case RunTypes.Generate:
                            RunGenerate(_serverName, _databaseName, _path);

                            break;
                        case RunTypes.Run:
                            RunRun(_serverName, _databaseName, _path);

                            break;
                        case RunTypes.Backup:
                            RunBackup(_serverName, _databaseName, _path);

                            break;
                        case RunTypes.Exit:
                            return;

                        default:
                            Console.WriteLine($@"Not sure what you're trying to do");

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("MSSQL Tools");
            Console.WriteLine("Small useful tools for use MS SQL.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static System.IO.DirectoryInfo SetupOutputDirectory(string path, string databaseName)
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);

            if (!directory.Exists)
            {
                directory.Create();
            }

            directory = directory.CreateSubdirectory(databaseName);
            return directory;
        }

        private static void GetSettingsFromPrompt()
        {
            Console.Clear();

            switch (GetRunTypes())
            {
                case RunTypes.Generate:
                    RunGenerate(GetServer(), GetDatabase(), GetPath($@"What output directory? (Leave blank for {_path})"));
                    
                    break;
                case RunTypes.Run:
                    RunRun(GetServer(), GetDatabase(), GetPath($@"What scripts directory? (Leave blank for {_path})"));
                    
                    break;
                case RunTypes.Backup:
                    RunBackup(GetServer(), GetDatabase(), GetPath($@"Backup to? (Leave blank for {_path})"));

                    break;
                case RunTypes.Exit:
                    return;

                default:
                    Console.WriteLine($@"Not sure what you're trying to do");

                    break;
            }
            
            Console.Write("Press any key to continue");
            Console.ReadKey();

            GetSettingsFromPrompt();
        }

        private static void RunBackup(string serverName, string databaseName, string path)
        {
            SQLAccess sqlAccess = new SQLAccess($"Server={serverName};Database={databaseName};Trusted_Connection=True;");

            System.IO.FileInfo file = new System.IO.FileInfo(path);

            if (string.IsNullOrEmpty(file.Name))
            {
                file = new System.IO.FileInfo($@"{file.Directory.FullName}\{databaseName}.bak");
            }

            sqlAccess.Backup(databaseName, file.FullName);
        }

        private static void RunRun(string serverName, string databaseName, string path)
        {
            new Runners.RunScripts().Run(path, $"Server={serverName};Database={databaseName};Trusted_Connection=True;");
        }

        private static void RunGenerate(string serverName, string databaseName, string path)
        {
            System.IO.DirectoryInfo directory = SetupOutputDirectory(path, databaseName);

            SQLAccess sqlAccess = new SQLAccess($"Server={serverName};Database={databaseName};Trusted_Connection=True;");

            foreach (var table in sqlAccess.GetTables())
            {
                var columns = sqlAccess.GetColumns(table.TableId).ToList();

                new Creators.SelectCreator().Create(table, columns).Save(directory.FullName);
                new Creators.UpsertCreator().Create(table, columns).Save(directory.FullName);
                new Creators.DeleteCreator().Create(table, columns).Save(directory.FullName);
            }
        }

        private static RunTypes GetRunTypes()
        {
            Console.WriteLine($@"What do you want to do?");
            Console.WriteLine($@"(0) Generate Scripts");
            Console.WriteLine($@"(1) Run Scripts");
            Console.WriteLine($@"(2) Backup");
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

        private static string GetPath(string message)
        {
            Console.WriteLine(message);
            string path = Console.ReadLine();

            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            return _path;
        }

        private static string GetServer()
        {
            Console.WriteLine("Server name? (Leave blank for local(.))");
            string serverName = Console.ReadLine();

            if (!string.IsNullOrEmpty(serverName))
            {
                return serverName;
            }

            return _serverName;
        }

        private static string GetDatabase()
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

                    return GetDatabase();
                }
            }
            else
            {
                Console.WriteLine("Please enter the number");

                return GetDatabase();
            }

            return _databaseName;
        }
    }
}