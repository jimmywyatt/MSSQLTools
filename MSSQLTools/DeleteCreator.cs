using System;
using System.Collections.Generic;
using System.Linq;
using static MSSQLTools.Program;

namespace MSSQLTools
{
    public class DeleteCreator
    {
        public static void Create(Tables table, List<Columns> columns, string outputDirectory)
        {
            var script = new SQLScripts();
            var identityColumn = columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn == null)
            {
                script.Add("Missing Identity Column");
                script.Save($@"{outputDirectory}\{table.SchemaName}.Delete{table.TableName}Procedure.sql");

                return;
            }

            Header(table, columns, script, identityColumn);

            Select(table, columns, script, identityColumn);

            Footer(script);

            script.Save($@"{outputDirectory}\{table.SchemaName}.Delete{table.TableName}Procedure.sql");
        }

        private static void Header(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add($"Create Proc [{table.SchemaName}].Delete{table.TableName}Procedure");

            script.Add(1, $"@{identityColumn.ColumnName} {SQLHelper.GetTypeWithLength(identityColumn)}");

            script.Add($"As");
            script.Add($"Begin");
        }

        private static void Select(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add(1, $"Delete");
            script.Add(2, $"From");
            script.Add(3, $"[{table.SchemaName}].[{table.TableName}]");
            script.Add(2, $"Where");
            script.Add(3, $"{identityColumn.ColumnName} = @{identityColumn.ColumnName}");
        }

        private static void Footer(SQLScripts script)
        {
            script.Add(0, "End");
        }
    }
}