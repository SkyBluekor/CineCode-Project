using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CineCode.Data;
using Microsoft.Data.SqlClient;

namespace CineCode.Views
{
    public partial class CsvExportWindow : Window
    {
        public CsvExportWindow()
        {
            InitializeComponent();
        }

        // 버튼 Tag 값에 따라 추출할 CSV 종류를 선택합니다.
        private void CsvExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            string exportType = button.Tag?.ToString() ?? "";

            try
            {
                // XAML 버튼의 Tag 값과 SQL 추출 대상을 1:1로 연결합니다.
                switch (exportType)
                {
                    case "TotalUsers":
                        ExportCsv(
                            "전체사용자",
                            @"
                                SELECT
                                    User_ID,
                                    USER_NAME AS User_Name,
                                    User_BirthDate,
                                    User_PhoneNumber,
                                    User_Sex
                                FROM dbo.AppUser
                                ORDER BY User_ID;
                            "
                        );
                        break;

                    case "TotalContent":
                        ExportCsv(
                            "전체콘텐츠",
                            @"
                                SELECT
                                    Content_ID,
                                    TMDB_ID,
                                    Content_Type,
                                    Title,
                                    Release_Date,
                                    TMDB_Average_Rating,
                                    Running_Time,
                                    Age_Rating,
                                    Poster_URL
                                FROM dbo.Content
                                ORDER BY Content_ID;
                            "
                        );
                        break;

                    case "Genre":
                        ExportCsv(
                            "장르",
                            @"
                                SELECT
                                    Genre_ID,
                                    TMDB_Genre_ID,
                                    Content_Type,
                                    Genre_Name
                                FROM dbo.Genre
                                ORDER BY Content_Type, Genre_Name;
                            "
                        );
                        break;

                    case "OttProvider":
                        ExportCsv(
                            "OTT제공자",
                            @"
                                SELECT
                                    OTT_ID,
                                    OTT_Name,
                                    Monthly_Price
                                FROM dbo.OTTProvider
                                ORDER BY OTT_Name;
                            "
                        );
                        break;

                    case "WatchHistory":
                        ExportCsv(
                            "시청기록",
                            @"
                                SELECT
                                    WH.Watch_ID,
                                    WH.User_ID,
                                    AU.USER_NAME AS User_Name,
                                    WH.Content_ID,
                                    C.Title,
                                    C.Content_Type,
                                    WH.OTT_ID,
                                    OP.OTT_Name,
                                    WH.Watch_Started_At,
                                    WH.Watch_Ended_At,
                                    WH.Watch_Minutes,
                                    WH.Progress_Percentage,
                                    WH.Is_Completed
                                FROM dbo.WatchHistory WH
                                INNER JOIN dbo.AppUser AU
                                    ON WH.User_ID = AU.User_ID
                                INNER JOIN dbo.Content C
                                    ON WH.Content_ID = C.Content_ID
                                INNER JOIN dbo.OTTProvider OP
                                    ON WH.OTT_ID = OP.OTT_ID
                                ORDER BY WH.Watch_ID DESC;
                            "
                        );
                        break;

                    default:
                        MessageBox.Show("알 수 없는 CSV 추출 타입입니다.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "CSV 추출 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "CSV 추출 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // CSV 저장 경로를 확인하고, SQL 조회 결과를 파일로 저장합니다.
        private void ExportCsv(string filePrefix, string sql)
        {
            string exportFolderPath = GetCsvExportPath();

            if (!Directory.Exists(exportFolderPath))
            {
                Directory.CreateDirectory(exportFolderPath);
            }

            string fileName =
                filePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            string filePath =
                Path.Combine(exportFolderPath, fileName);

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            WriteReaderToCsv(reader, filePath);

            MessageBox.Show(
                "CSV 추출이 완료되었습니다.\n\n" + filePath,
                "CSV 추출 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // 시스템 설정 테이블에서 CSV 파일을 저장할 폴더 경로를 가져옵니다.
        private string GetCsvExportPath()
        {
            string sql = @"
                SELECT Setting_Value
                FROM dbo.SystemSetting
                WHERE Setting_Key = 'CSV_EXPORT_PATH';
            ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            object? result =
                command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                throw new Exception("CSV_EXPORT_PATH 설정값이 없습니다.");
            }

            string path =
                result.ToString()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new Exception("CSV_EXPORT_PATH 설정값이 비어 있습니다.");
            }

            return path;
        }

        // SqlDataReader의 컬럼명과 모든 행 데이터를 CSV 형식으로 파일에 기록합니다.
        private void WriteReaderToCsv(SqlDataReader reader, string filePath)
        {
            using StreamWriter writer =
                new StreamWriter(filePath, false, new UTF8Encoding(true));

            // 첫 줄에는 조회 결과의 컬럼명을 CSV 헤더로 기록합니다.
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                writer.Write(EscapeCsvValue(reader.GetName(i)));
            }

            writer.WriteLine();

            // 두 번째 줄부터는 실제 조회 데이터를 한 행씩 기록합니다.
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(",");
                    }

                    string columnName = reader.GetName(i);

                    object? rawValue = null;

                    if (!reader.IsDBNull(i))
                    {
                        rawValue = reader.GetValue(i);
                    }

                    writer.Write(FormatCsvValue(columnName, rawValue));
                }

                writer.WriteLine();
            }
        }

        // 날짜와 전화번호처럼 CSV에서 깨지기 쉬운 값을 저장용 문자열로 변환합니다.
        private string FormatCsvValue(string columnName, object? rawValue)
        {
            if (rawValue == null)
            {
                return "";
            }

            if (rawValue is DateTime dateTime)
            {
                return EscapeCsvValue(dateTime.ToString("yyyy-MM-dd"));
            }

            string value = rawValue.ToString() ?? "";

            if (IsExcelTextColumn(columnName))
            {
                // Excel이 전화번호 앞자리 0을 지우지 않도록 수식 형태의 텍스트로 감쌉니다.
                return "=\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return EscapeCsvValue(value);
        }

        // Excel에서 숫자로 오인하기 쉬운 전화번호 계열 컬럼인지 확인합니다.
        private bool IsExcelTextColumn(string columnName)
        {
            string normalizedColumnName =
                columnName.Replace("_", "").ToLower();

            return normalizedColumnName.Contains("phonenumber")
                || normalizedColumnName.Contains("phone")
                || normalizedColumnName.Contains("tel")
                || normalizedColumnName.Contains("mobile");
        }

        // 쉼표, 따옴표, 줄바꿈이 들어간 값은 CSV 규칙에 맞게 따옴표로 감쌉니다.
        private string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return "\"" + value + "\"";
            }

            return value;
        }

        // 창을 닫습니다.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
