using System;
using System.Collections.Generic;
using System.Linq;
using static MSSQLTools.Program;

namespace MSSQLTools
{
    public class SelectCreator
    {
        public static void Create(Tables table, List<Columns> columns, string outputDirectory)
        {
            var script = new SQLScripts();
            var identityColumn = columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn == null)
            {
                script.Add("Missing Identity Column");
                script.Save($@"{outputDirectory}\{table.SchemaName}.Select{table.TableName}Procedure.sql");

                return;
            }

            Header(table, columns, script, identityColumn);

            Select(table, columns, script, identityColumn);

            Footer(script);

            script.Save($@"{outputDirectory}\{table.SchemaName}.Select{table.TableName}Procedure.sql");
        }

        private static void Header(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add($"Create Proc [{table.SchemaName}].Select{table.TableName}Procedure");

            script.Add(1, $"@{identityColumn.ColumnName} {SQLHelper.GetTypeWithLength(identityColumn)}");
            
            script.Add($"As");
            script.Add($"Begin");
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
            script.Add(3, $"(");
            script.Add(4, $"t.{identityColumn.ColumnName} = @{identityColumn.ColumnName}");
            script.Add(4, $"Or @{identityColumn.ColumnName} Is Null");
            script.Add(3, $")");
        }

        private static void Footer(SQLScripts script)
        {
            script.Add(0, "End");
        }
    }
}