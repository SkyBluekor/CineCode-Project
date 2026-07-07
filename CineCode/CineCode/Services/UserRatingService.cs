using System.Data;
using CineCode.Data;
using CineCode.Models.UserRating;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    public class UserRatingService
    {
        // 사용자 평점 목록과 시청 진행률 정보를 함께 조회
        public List<UserRatingListItemModel> GetUserRatingList()
        {
            List<UserRatingListItemModel> ratings = new List<UserRatingListItemModel>();

            string sql = @"
                SELECT
                    UR.User_ID,
                    UR.Content_ID,
                    AU.USER_NAME AS User_Name,
                    C.Title AS Content_Name,
                    C.Content_Type AS Content_Type,
                    UR.Rating_Score,
                    WH.Progress_Percentage,
                    UR.Rated_At
                FROM dbo.UserRating UR
                INNER JOIN dbo.AppUser AU
                    ON UR.User_ID = AU.User_ID
                INNER JOIN dbo.Content C
                    ON UR.Content_ID = C.Content_ID
                OUTER APPLY
                (
                    SELECT TOP 1
                        WH2.Progress_Percentage
                    FROM dbo.WatchHistory WH2
                    WHERE WH2.User_ID = UR.User_ID
                      AND WH2.Content_ID = UR.Content_ID
                      AND WH2.Watch_Ended_At IS NOT NULL
                    ORDER BY
                        CASE
                            WHEN CAST(WH2.Watch_Ended_At AS DATE) <= CAST(UR.Rated_At AS DATE)
                            THEN 0
                            ELSE 1
                        END,
                        WH2.Progress_Percentage DESC,
                        WH2.Watch_Ended_At DESC,
                        WH2.Watch_ID DESC
                ) WH
                ORDER BY UR.Rated_At DESC;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                UserRatingListItemModel rating = new UserRatingListItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    Content_ID = DbReaderHelper.GetInt(reader, "Content_ID"),

                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    Content_Name = DbReaderHelper.GetString(reader, "Content_Name"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),

                    Rating_Score =
                        DbReaderHelper.GetNullableDecimal(reader, "Rating_Score") ?? 0,

                    Progress_Percentage =
                        DbReaderHelper.GetNullableDecimal(reader, "Progress_Percentage"),

                    Rated_At = DbReaderHelper.GetDateTime(reader, "Rated_At")
                };

                ratings.Add(rating);
            }

            return ratings;
        }

        // 평점 등록 화면에서 선택할 전체 사용자 목록 조회
        public List<UserRatingUserSelectItemModel> GetUsersForUserRating()
        {
            List<UserRatingUserSelectItemModel> users = new List<UserRatingUserSelectItemModel>();

            string sql = @"
                SELECT
                    User_ID,
                    USER_NAME AS User_Name,
                    User_PhoneNumber
                FROM dbo.AppUser
                ORDER BY USER_NAME;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                UserRatingUserSelectItemModel user = new UserRatingUserSelectItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber")
                };

                users.Add(user);
            }

            return users;
        }

        // 키워드로 평점 등록 대상 사용자를 검색
        public List<UserRatingUserSelectItemModel> SearchUsersForUserRating(string keyword)
        {
            List<UserRatingUserSelectItemModel> users = new List<UserRatingUserSelectItemModel>();

            string sql = @"
                SELECT TOP 30
                    User_ID,
                    USER_NAME AS User_Name,
                    User_PhoneNumber
                FROM dbo.AppUser
                WHERE
                    CAST(User_ID AS NVARCHAR(20)) LIKE @Keyword
                    OR USER_NAME LIKE @Keyword
                    OR User_PhoneNumber LIKE @Keyword
                ORDER BY USER_NAME;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value =
                "%" + keyword.Trim() + "%";

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                UserRatingUserSelectItemModel user = new UserRatingUserSelectItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber")
                };

                users.Add(user);
            }

            return users;
        }

        // 선택한 사용자가 평점을 남길 수 있는 콘텐츠 목록 조회
        public List<UserRatingContentSelectItemModel> GetRateableContentsByUserId(int userId)
        {
            List<UserRatingContentSelectItemModel> contents = new List<UserRatingContentSelectItemModel>();

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetRateableContentsForRating", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                UserRatingContentSelectItemModel content = new UserRatingContentSelectItemModel
                {
                    Content_ID = DbReaderHelper.GetInt(reader, "Content_ID"),
                    Title = DbReaderHelper.GetString(reader, "Title"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    Progress_Percentage = DbReaderHelper.GetNullableDecimal(reader, "Progress_Percentage"),
                    WatchStartedAt = DbReaderHelper.GetNullableDateTime(reader, "Watch_Started_At"),
                    WatchEndedAt = DbReaderHelper.GetNullableDateTime(reader, "Watch_Ended_At")
                };

                contents.Add(content);
            }

            return contents;
        }

        // 사용자 평점을 새로 등록하거나 기존 평점을 수정
        public void AddOrUpdateUserRating(
            int userId,
            int contentId,
            decimal ratingScore,
            DateTime ratedAt)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddOrUpdateUserRating", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@content_id", SqlDbType.Int).Value = contentId;

            SqlParameter ratingScoreParameter =
                command.Parameters.Add("@rating_score", SqlDbType.Decimal);

            ratingScoreParameter.Precision = 3;
            ratingScoreParameter.Scale = 1;
            ratingScoreParameter.Value = ratingScore;

            command.Parameters.Add("@rated_at", SqlDbType.Date).Value = ratedAt.Date;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 선택한 사용자와 콘텐츠의 평점 삭제
        public void DeleteUserRating(int userId, int contentId)
        {
            string sql = @"
                DELETE FROM dbo.UserRating
                WHERE User_ID = @user_id
                    AND Content_ID = @content_id;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@content_id", SqlDbType.Int).Value = contentId;

            connection.Open();

            int affectedRows = command.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new Exception("삭제할 평점이 존재하지 않습니다.");
            }
        }

        // 아직 평점을 남길 수 있는 시청 기록 수 조회
        public int GetPendingUserRatingCount()
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetPendingUserRatingCount", connection);

            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(result);
        }

        // 평점 목록 CSV 저장에 사용할 폴더 경로 조회
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

            string path = result.ToString()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new Exception("CSV_EXPORT_PATH 설정값이 비어 있습니다.");
            }

            return path;
        }
    }
}
