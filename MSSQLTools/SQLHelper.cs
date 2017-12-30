namespace MSSQLTools
{
    partial class Program
    {
        public static class SQLHelper
        {
            public static string GetTypeWithLength(Columns column)
            {
                switch (column.TypeName.ToLower())
                {
                    case "int":
                    case "bit":
                    case "datetime":
                        return column.TypeName;

                    default:
                        if (column.Length == -1)
                        {
                            return $"{column.TypeName}(Max)";
                        }

                        return $"{column.TypeName}({column.Length})";
                }
            }
        }
    }
}
