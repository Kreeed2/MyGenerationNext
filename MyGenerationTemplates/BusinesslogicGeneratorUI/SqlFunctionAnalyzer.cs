namespace EHA.ZESA.Util.GeneratorUI
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class FunctionGenInfo
    {
        public FunctionGenInfo(string pFunctionName)
        {
            FunctionName = pFunctionName;
        }
        public string FunctionName { get; private set; }
        public List<TvfColumnInfo> FunctionReturnValueColumns { get; set; }
        public List<TvfColumnInfo> FunctionInputParameterColumns { get; set; }
    }

    [Serializable]
    public class TvfColumnInfo
    {
        public string Name { get; set; }

        public string LanguageType { get; set; }

        public bool IsNullable { get; set; }
    }

    public class SqlFunctionAnalyzer
    {
        private string connectionString;

        public SqlFunctionAnalyzer(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<TvfColumnInfo> GetTvfReturnColumns(string functionName = null)
        {
            var columnList = new List<TvfColumnInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                 SELECT 
                    obj.name AS FunctionName,
                    cols.name AS ColumnName,
                    typ.name AS DataType,
                    cols.max_length AS MaxLength,
                    cols.is_nullable AS IsNullable
                FROM sys.objects obj
                INNER JOIN sys.columns cols ON obj.object_id = cols.object_id
                INNER JOIN sys.types typ ON cols.user_type_id = typ.user_type_id
                WHERE obj.type IN ('TF', 'IF')";

                if (!string.IsNullOrEmpty(functionName))
                {
                    query += " AND obj.name = @FunctionName";
                }

                query += " ORDER BY obj.name, cols.column_id;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(functionName))
                    {
                        command.Parameters.AddWithValue("@FunctionName", functionName);
                    }

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnInfo = new TvfColumnInfo
                            {
                                //FunctionName = reader["FunctionName"].ToString(),
                                Name = reader["ColumnName"].ToString(),
                                LanguageType = reader["DataType"].ToString(),
                                //max = Convert.ToInt32(reader["MaxLength"]),
                                IsNullable = Convert.ToBoolean(reader["IsNullable"])
                            };

                            columnList.Add(columnInfo);
                        }
                    }
                }
            }

            return columnList;
        }
    }

}
