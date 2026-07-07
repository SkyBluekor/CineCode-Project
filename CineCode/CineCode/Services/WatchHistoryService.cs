using CineCode.Data;
using CineCode.Models.WatchHistory;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CineCode.Services
{
    public class WatchHistoryService
    {
        // 전체 시청 기록 목록을 사용자, 콘텐츠, OTT 정보와 함께 조회
        public List<WatchHistoryListItemModel> GetWatchHistoryList()
        {
            List<WatchHistoryListItemModel> WatchHistories = new List<WatchHistoryListItemModel>();

            string sql = @"
                Select 
                    WH.Watch_ID,
                    WH.User_ID,
                    WH.Content_ID,
                    WH.Ott_ID,
                    AU.User_Name AS User_Name,
                    C.Title AS Content_Name,
                    C.Content_Type AS Content_Type,
                    C.Running_Time AS Running_Time,
                    OP.OTT_NAME AS OTT_Name,
                    WH.Watch_Started_at,
                    WH.Watch_Ended_at,
                    WH.Watch_Minutes,
                    Wh.progress_percentage,
                    Wh.is_completed
                from dbo.WatchHistory WH
                INNER JOIN dbo.AppUser AU
                    ON WH.User_ID = AU.User_ID
                INNER JOIN dbo.Content C
                    ON WH.Content_ID = C.Content_ID
                INNER JOIN dbo.OTTProvider OP
                    ON WH.Ott_ID = OP.OTT_ID
                ORDER BY WH.Watch_ID DESC;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                WatchHistoryListItemModel watchHistory = new WatchHistoryListItemModel
                {
                    Watch_ID = DbReaderHelper.GetInt(reader, "Watch_ID"),
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    Content_ID = DbReaderHelper.GetInt(reader, "Content_ID"),
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),

                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    Content_Name = DbReaderHelper.GetString(reader, "Content_Name"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    Running_Time = DbReaderHelper.GetNullableInt(reader, "Running_TIme"),
                    OTT_Name = DbReaderHelper.GetString(reader, "OTT_Name"),

                    WatchStartedAt = DbReaderHelper.GetDateTime(reader, "Watch_Started_At"),
                    WatchEndedAt = DbReaderHelper.GetNullableDateTime(reader, "Watch_Ended_At"),

                    WatchMinutes = DbReaderHelper.GetNullableInt(reader, "Watch_Minutes"),
                    ProgressPercentage = DbReaderHelper.GetNullableDecimal(reader, "Progress_Percentage"),

                    IsCompleted = DbReaderHelper.GetNullableBool(reader, "Is_Completed")
                };

                WatchHistories.Add(watchHistory);
            }

            return WatchHistories;
        }

        // 시청 기록 등록 화면에서 사용자를 키워드로 검색
        public List<WatchHistoryUserSelectItemModel> SearchUsersForWatchHistory(string keyword)
        {
            List<WatchHistoryUserSelectItemModel> users = new List<WatchHistoryUserSelectItemModel>();

            string sql = @"
                select top 30
                    User_ID,
                    USer_Name,
                    User_PhoneNumber
                From dbo.AppUser
                where 
                    Cast(User_ID as nvarchar(20)) Like @Keyword 
                    OR User_Name Like @Keyword
                    OR User_PhoneNumber LIke @Keyword
                Order by USER_NAME;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = "%" + keyword.Trim() + "%";

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                WatchHistoryUserSelectItemModel user = new WatchHistoryUserSelectItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber")
                };

                users.Add(user);
            }

            return users;
        }

        // 시청 기록 등록 화면에서 콘텐츠를 키워드로 검색
        public List<WatchHistoryContentSelectItemModel> SearchContentsForWatchHistory(string keyword)
        {
            List<WatchHistoryContentSelectItemModel> contents = new List<WatchHistoryContentSelectItemModel>();

            string sql = @"
                select TOP 30
                    Content_ID,
                    Title,
                    Content_Type,
                    Running_TIme
                From dbo.Content
                Where
                    Cast(Content_ID as nvarchar(20)) Like @Keyword
                    OR Title like @Keyword
                    OR Content_Type like @Keyword
                Order by Title;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = "%" + keyword.Trim() + "%";

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                WatchHistoryContentSelectItemModel content = new WatchHistoryContentSelectItemModel
                {
                    Content_ID = DbReaderHelper.GetInt(reader, "Content_ID"),
                    Title = DbReaderHelper.GetString(reader, "Title"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    Running_Time = DbReaderHelper.GetNullableInt(reader, "Running_Time")
                };

                contents.Add(content);
            }

            return contents;
        }

        // 선택한 콘텐츠를 제공하는 OTT 목록 조회
        public List<WatchHistoryOttProviderSelectItemModel> GetOttProvidersByContentId(int contentId)
        {
            List<WatchHistoryOttProviderSelectItemModel> ottProviders = new List<WatchHistoryOttProviderSelectItemModel>();

            string sql = @"
                SELECT 
                    OP.OTT_ID,
                    OP.OTT_Name 
                FROM dbo.ContentOTTProvider COP
                INNER JOIN dbo.OTTProvider OP
                    ON COP.OTT_ID = OP.OTT_ID
                WHERE COP.Content_ID = @Content_ID
                order by OP.OTT_NAME;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                WatchHistoryOttProviderSelectItemModel ottprovider = new WatchHistoryOttProviderSelectItemModel
                {
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),
                    OTT_Name = DbReaderHelper.GetString(reader, "OTT_Name")
                };

                ottProviders.Add(ottprovider);
            }

            return ottProviders;
        }

        // 선택한 사용자, 콘텐츠, OTT로 시청 시작 기록을 등록
        public int StartWatchHistory(
            int UserID,
            int ContentID,
            int OTTID,
            DateTime WatchStartedAt)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_StartWatchHistory", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@user_id", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@content_id", SqlDbType.Int).Value = ContentID;
            command.Parameters.Add("@ott_id", SqlDbType.Int).Value = OTTID;
            command.Parameters.Add("@watch_started_at", SqlDbType.DateTime2).Value = WatchStartedAt;

            connection.Open();

            Object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                throw new Exception("시청 시작 등록 결과를 가져오지 못했습니다.");
            }

            return Convert.ToInt32(result);
        }

        // 진행 중인 시청 기록을 종료 처리
        public void EndWatchHistory(int watchId, int watchMinutes)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_EndWatchHistory", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Watch_ID", SqlDbType.Int).Value = watchId;
            command.Parameters.Add("@Watch_Minutes", SqlDbType.Int).Value = watchMinutes;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 종료된 시청 기록의 시청 시간과 진행률을 수정
        public void UpdateWatchMinutes(int watchId, int watchMinutes)
        {
            string sql = @"
                UPDATE WH
                SET WH.Watch_Minutes = @Watch_Minutes,
                    WH.Progress_Percentage =
                        CASE 
                            WHEN @Watch_Minutes >= C.Running_Time THEN 100
                            ELSE CAST(@Watch_Minutes * 100.0 / C.Running_Time as DECIMAL(5,2))
                        END,
                    WH.Is_Completed =
                        CASE 
                            WHEN @Watch_Minutes >= C.Running_Time THEN 1
                            ELSE 0
                        END                    
                FROM dbo.WatchHistory WH
                INNER JOIN dbo.Content C
                    ON WH.Content_ID = C.Content_ID
                WHERE WH.Watch_ID = @Watch_ID
                    AND WH.Watch_Ended_At IS NOT NULL
                    AND C.Running_TIme IS not null
                    AND @Watch_Minutes >= 0
                    AND @Watch_Minutes <= C.Running_Time
                    AND @Watch_Minutes <= DATEDIFF(MINUTE, WH.Watch_Started_At, WH.Watch_Ended_At);
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Watch_Minutes", SqlDbType.Int).Value = watchMinutes;
            command.Parameters.Add("@Watch_ID", SqlDbType.Int).Value = watchId;

            connection.Open();

            int affectedRows = command.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new Exception(
                    "시청 시간 수정에 실패했습니다."
                );
            }
        }

        // 선택한 시청 기록 삭제
        public void DeleteWatchHistory(int watchId)
        {
            string sql = @"
                DELETE FROM dbo.WatchHistory
                WHERE Watch_ID = @Watch_ID;            
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Watch_ID", SqlDbType.Int).Value = watchId;

            connection.Open();

            int affectedRows = command.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new Exception(
                    "시청 기록 삭제에 실패했습니다."
                );
            }
        }

        // 시청 기록 CSV 저장에 사용할 폴더 경로 조회
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

            if (result == null || result == DBNull.Value)
            {
                throw new Exception("CSV_EXPORT_PATH 설정값이 없습니다.");
            }

            string path = result.ToString().Trim() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new Exception("CSV_EXPORT_PATH 설정값이 비어 있습니다");
            }

            return path;
        }
    }
}
