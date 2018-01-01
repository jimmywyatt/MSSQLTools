using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace MSSQLTools
{
    public class SQLAccess
    {
        public string _connectionString = "";

        public SQLAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Columns> GetColumns(int tableId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var results = connection.Query<Columns>(@"
                    Select
		                    so.name As TableName
		                    , sc.name As ColumnName
		                    , st.name As TypeName
		                    , sc.Length
		                    , Cast(Case
			                    When sc.status = 0x80 then 1 else 0
		                    End As Bit) As IsIdentity
		                    , ColOrder As ColumnOrder
	                    From
		                    sysobjects so 
		                    Join syscolumns sc On so.id = sc.id
		                    Join systypes st On sc.xtype = st.xusertype
	                    Where
		                    so.id = @TableId
	                    Order By
		                    ColOrder
                ", new { TableId = tableId });

                return results;
            }
        }

        public IEnumerable<Tables> GetTables()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var results = connection.Query<Tables>(@"
                    Select
		                    so.object_id As TableId
		                    , Schema_Name(Schema_Id) As SchemaName
		                    , Name As TableName
	                    From
		                    sys.Objects so 
	                    Where
		                    so.[Type] = 'U'
                ");

                return results;
            }
        }

        public IEnumerable<Databases> GetDatabases()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var results = connection.Query<Databases>(@"
                    Select
		                    [Name]
		                    , [Dbid]
	                    From
		                    sysdatabases
	                    Where
		                    [Name] Not In ('master', 'tempdb', 'model', 'msdb')
                ");

                return results;
            }
        }

        public int RunScript(string filePath)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.InfoMessage += (s, e) =>
                {
                    Helpers.LogHelper.Log4Net.Debug(e.Message);
                };

                return connection.Execute(System.IO.File.ReadAllText(filePath));
            }
        }

        public int Backup(string databaseName, string filePath)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.InfoMessage += (s, e) =>
                {
                    Helpers.LogHelper.Log4Net.Debug(e.Message);
                };

                return connection.Execute($@"
                    Backup Database [{databaseName}] 
	                    To Disk = N'{filePath}' WITH NOFORMAT
	                    , INIT
	                    , NAME = N'{databaseName}-Full Database Backup'
	                    , SKIP
	                    , STATS = 10
                ");
            }
        }
    }
}
