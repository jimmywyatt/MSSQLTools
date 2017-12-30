namespace MSSQLTools
{
    public class Columns
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string TypeName { get; set; }
        public bool IsIdentity { get; set; }
        public int ColumnOrder { get; set; }
        public int Length { get; set; }
    }
}