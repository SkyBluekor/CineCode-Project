using Microsoft.Data.SqlClient;

namespace CineCode.Data
{
    // SqlDataReader의 컬럼 값을 애플리케이션에서 사용할 형식으로 변환합니다.
    public static class DbReaderHelper
    {
        // DB 값이 NULL이면 0을 반환합니다.
        public static int GetInt(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value);
        }

        // DB 값이 NULL이면 빈 문자열을 반환합니다.
        public static string GetString(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            return value.ToString() ?? "";
        }

        // DB 값이 NULL이면 DateTime의 최솟값을 반환합니다.
        public static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            return Convert.ToDateTime(value);
        }

        // 아래 메서드는 DB의 NULL 값을 nullable 형식의 null로 유지합니다.
        public static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(value);
        }

        public static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(value);
        }

        public static bool? GetNullableBool(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(index))
            {
                return null;
            }

            return reader.GetBoolean(index);
        }

        public static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }

        // 날짜 값을 화면 표시 형식(yyyy-MM-dd)의 문자열로 변환합니다.
        public static string GetDateText(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            DateTime date = Convert.ToDateTime(value);

            return date.ToString("yyyy-MM-dd");
        }

        // 소수 값을 소수점 한 자리의 숫자 문자열로 변환합니다.
        public static string GetDecimalText(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            decimal number = Convert.ToDecimal(value);

            return number.ToString("N1");
        }
    }
}
