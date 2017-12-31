using System.Collections.Generic;
using System.Linq;

namespace MSSQLTools.Creators
{
    public class UpsertCreator
    {
        Tables _table = null;
        SQLScripts _script = new SQLScripts();

        public UpsertCreator Create(Tables table, List<Columns> columns)
        {
            _table = table;
            var identityColumn = columns.FirstOrDefault(x => x.IsIdentity);

            if (identityColumn == null)
            {
                _script.Add("Missing Identity Column");

                Helpers.LogHelper.Log4Net.Debug($"Missing Identity Column ({FileName})");

                return this;
            }

            Header(table, columns, _script);

            Update(table, columns, _script, identityColumn);

            _script.Add(1, "");

            Insert(table, columns, _script, identityColumn);

            _script.Add(1, "");

            Select(table, columns, _script, identityColumn);

            Footer(_script);

            Helpers.LogHelper.Log4Net.Debug($"Created {FileName}");

            return this;
        }

        private void Footer(SQLScripts script)
        {
            script.Add(0, "End");
        }

        private void Select(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
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

        private void Insert(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
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

        private void Update(Tables table, List<Columns> columns, SQLScripts script, Columns identityColumn)
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

        private void Header(Tables table, List<Columns> columns, SQLScripts script)
        {
            script.Add($"Create Proc [{table.SchemaName}].Upsert{table.TableName}Procedure");

            foreach (var column in columns)
            {
                script.Add(1, $"{(columns.IndexOf(column) > 0 ? ", " : "")}@{column.ColumnName} {Helpers.SQLHelper.GetTypeWithLength(column)}");
            }

            script.Add($"As");
            script.Add($"Begin");
        }

        public string FileName => $"Upsert{_table.TableName}Procedure.sql";

        public void Save(string folderPath)
        {
            _script.Save($@"{folderPath}\{FileName}");
        }
    }
}
