using System;
using System.Collections.Generic;
using System.Linq;

namespace MSSQLTools.Creators
{
    public class DeleteCreator
    {
        Tables _table = null;
        SQLScripts _script = new SQLScripts();

        public DeleteCreator Create(Tables table, List<Columns> columns)
        {
            _table = table;
            var identityColumn = columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn == null)
            {
                _script.Add("Missing Identity Column");

                Helpers.LogHelper.Log4Net.Debug($"Missing Identity Column ({FileName})");

                return this;
            }

            Header(table, columns, _script, identityColumn);

            Select(table, columns, _script, identityColumn);

            Footer(_script);

            Helpers.LogHelper.Log4Net.Debug($"Created {FileName}");

            return this;
        }

        private void Header(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add($"Create Proc [{table.SchemaName}].Delete{table.TableName}Procedure");

            script.Add(1, $"@{identityColumn.ColumnName} {Helpers.SQLHelper.GetTypeWithLength(identityColumn)}");

            script.Add($"As");
            script.Add($"Begin");
        }

        private void Select(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
        {
            script.Add(1, $"Delete");
            script.Add(2, $"From");
            script.Add(3, $"[{table.SchemaName}].[{table.TableName}]");
            script.Add(2, $"Where");
            script.Add(3, $"{identityColumn.ColumnName} = @{identityColumn.ColumnName}");
        }

        private void Footer(SQLScripts script)
        {
            script.Add(0, "End");
        }

        public string FileName => $"Delete{_table.TableName}Procedure.sql";

        public void Save(string folderPath)
        {
            _script.Save($@"{folderPath}\{FileName}");
        }
    }
}