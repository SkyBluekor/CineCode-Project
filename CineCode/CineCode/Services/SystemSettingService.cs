using System.Data;
using CineCode.Data;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    public class SystemSettingService
    {
        // 프로그램 실행에 필요한 기본 설정값을 DB에 준비
        public void EnsureDefaultSettings()
        {
            EnsureSettingExists("CSV_EXPORT_PATH", @"C:\OTTExport\");
            EnsureSettingExists("DEFAULT_SUBSCRIPTION_DAYS", "30");
            EnsureSettingExists("MIN_RATING_PROGRESS", "50");
        }

        // 설정 키가 없을 때만 기본값을 새로 등록
        private void EnsureSettingExists(string settingKey, string settingValue)
        {
            string sql = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.SystemSetting
                    WHERE Setting_Key = @Setting_Key
                )
                BEGIN
                    INSERT INTO dbo.SystemSetting
                    (
                        Setting_Key,
                        Setting_Value
                    )
                    VALUES
                    (
                        @Setting_Key,
                        @Setting_Value
                    );
                END;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Setting_Key", SqlDbType.VarChar, 50).Value = settingKey;
            command.Parameters.Add("@Setting_Value", SqlDbType.NVarChar, 400).Value = settingValue;

            connection.Open();
            command.ExecuteNonQuery();
        }

        // 구독 목록 CSV 저장에 사용할 폴더 경로를 설정 테이블에서 조회
        public string GetCsvExportPath()
        {
            string sql = @"
                SELECT Setting_Value
                FROM dbo.SystemSetting
                WHERE Setting_Key = 'CSV_EXPORT_PATH';
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            object? result = command.ExecuteScalar();

            // 설정이 없거나 빈 값이면 CSV 저장 위치를 알 수 없으므로 예외 처리
            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException("CSV_EXPORT_PATH 설정값이 없습니다.");
            }

            string path = result.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("CSV_EXPORT_PATH 설정값이 비어있습니다.");
            }

            return path.Trim();
        }

        // 특정 설정값 조회
        public string GetSettingValue(string settingKey)
        {
            string sql = @"
                SELECT Setting_Value
                FROM dbo.SystemSetting
                WHERE Setting_Key = @Setting_Key;
            ";
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Setting_Key", SqlDbType.VarChar, 50).Value = settingKey;
            connection.Open();
            object? result = command.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException($"설정값 '{settingKey}'이(가) 없습니다.");
            }
            return result.ToString() ?? "";
        }

        // 특정 설정값 추출
        public void UpdateSettingValue(string settingKey, string settingValue)
        {
            string sql = @"
                UPDATE dbo.SystemSetting
                SET Setting_Value = @Setting_Value
                WHERE Setting_Key = @Setting_Key;
            ";
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Setting_Key", SqlDbType.VarChar, 50).Value = settingKey;
            command.Parameters.Add("@Setting_Value", SqlDbType.NVarChar, 400).Value = settingValue;
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"설정값 '{settingKey}'이(가) 존재하지 않아 업데이트할 수 없습니다.");
            }
        }

        // 평점 등록 최소 시청률 조회
        public int GetMinRatingProgress()
        {
            string value = GetSettingValue("MIN_RATING_PROGRESS");

            if (!int.TryParse(value, out int minRatingProgress))
            {
                throw new InvalidOperationException("MIN_RATING_PROGRESS 설정값이 숫자가 아닙니다.");
            }

            return minRatingProgress;
        }

        // 기본 구독 기간 조회
        public int GetDefaultSubscriptionDays()
        {
            string value = GetSettingValue("DEFAULT_SUBSCRIPTION_DAYS");

            if (!int.TryParse(value, out int defaultSubscriptionDays))
            {
                throw new InvalidOperationException("DEFAULT_SUBSCRIPTION_DAYS 설정값이 숫자가 아닙니다.");
            }

            return defaultSubscriptionDays;
        }

        // CSV 저장 경로 수정
        public void UpdateCsvExportPath(string csvExportPath)
        {
            if (string.IsNullOrWhiteSpace(csvExportPath))
            {
                throw new InvalidOperationException("CSV 저장 경로는 비워둘 수 없습니다.");
            }

            UpdateSettingValue("CSV_EXPORT_PATH", csvExportPath.Trim());
        }

        // 평점 등록 최소 시청률 수정
        public void UpdateMinRatingProgress(int minRatingProgress)
        {
            if (minRatingProgress < 0 || minRatingProgress > 100)
            {
                throw new InvalidOperationException("평점 등록 최소 시청률은 0~100 사이여야 합니다.");
            }

            UpdateSettingValue("MIN_RATING_PROGRESS", minRatingProgress.ToString());
        }
    }
}
