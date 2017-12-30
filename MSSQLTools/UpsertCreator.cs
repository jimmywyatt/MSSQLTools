using System.Collections.Generic;
using System.Linq;
using static MSSQLTools.Program;

namespace MSSQLTools
{
    public class UpsertCreator
    {
        public static void Create(Tables table, List<Columns> columns, string outputDirectory)
        {
            var script = new SQLScripts();
            var identityColumn = columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn == null)
            {
                script.Add("Missing Identity Column");
                script.Save($@"{outputDirectory}\{table.SchemaName}.Update{table.TableName}Procedure.sql");

                return;
            }

            Header(table, columns, script);

            Update(table, columns, script, identityColumn);

            script.Add(1, "");

            Insert(table, columns, script, identityColumn);

            script.Add(1, "");

            Select(table, columns, script, identityColumn);

            Footer(script);

            script.Save($@"{outputDirectory}\{table.SchemaName}.Upsert{table.TableName}Procedure.sql");
        }

        private static void Footer(SQLScripts script)
        {
            script.Add(0, "End");
        }

        private static void Select(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add(1, $"Select");

            foreach (var column in columns)
            {
                script.Add(3, $"{(columns.IndexOf(column) > 0 ? ", " : "")}t.[{column.ColumnName}]");
            }

            script.Add(2, $"From");
            script.Add(3, $"[{table.SchemaName}].[{table.TableName}] t");
            script.Add(2, $"Where");
            script.Add(3, $"t.{identityColumn.ColumnName} = @{identityColumn.ColumnName}");
        }

        private static void Insert(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add(1, "If (@@RowCount = 0)");
            script.Add(1, "Begin");
            script.Add(2, "Insert Into");
            script.Add(3, $"[{table.SchemaName}].[{table.TableName}]");
            script.Add(3, $"(");

            foreach (var column in columns.Where(x => !x.IsIdentity))
            {
                script.Add(4, $"{(columns.IndexOf(column) > 1 ? ", " : "")}[{column.ColumnName}]");
            }

            script.Add(3, $")");
            script.Add(3, $"Values");
            script.Add(3, $"(");

            foreach (var column in columns.Where(x => !x.IsIdentity))
            {
                script.Add(4, $"{(columns.IndexOf(column) > 1 ? ", " : "")}@{column.ColumnName}");
            }

            script.Add(3, $")");
            script.Add(3, $"");

            script.Add(2, $"Select");
            script.Add(3, $"@{identityColumn.ColumnName} = Scope_Identity()");

            script.Add(1, "End");
        }

        private static void Update(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add(1, "Update");
            script.Add(3, "t");
            script.Add(2, "Set");

            foreach (var column in columns.Where(x => !x.IsIdentity))
            {
                script.Add(3, $"{(columns.IndexOf(column) > 1 ? ", " : "")}t.[{column.ColumnName}] = @{column.ColumnName}");
            }

            script.Add(2, "From");
            script.Add(3, $"[{table.SchemaName}].[{table.TableName}] t");
            script.Add(2, "Where");
            script.Add(3, $"t.[{identityColumn.ColumnName}] = @{identityColumn.ColumnName}");
        }

        private static void Header(Tables table, List<Columns> columns, SQLScripts script)
        {
            script.Add($"Create Proc [{table.SchemaName}].Upsert{table.TableName}Procedure");

            foreach (var column in columns)
            {
                script.Add(1, $"{(columns.IndexOf(column) > 0 ? ", " : "")}@{column.ColumnName} {SQLHelper.GetTypeWithLength(column)}");
            }

            script.Add($"As");
            script.Add($"Begin");
        }
    }
}
