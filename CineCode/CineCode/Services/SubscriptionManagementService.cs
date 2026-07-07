using System.Data;
using CineCode.Data;
using CineCode.Models.Subscription;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    // 사용자 구독 등록, 조회, 연장, 해지를 담당하는 서비스
    public class SubscriptionManagementService
    {
        // 구독 관리 화면에 표시할 최신 구독 목록을 조회
        public List<SubscriptionListItemModel> GetSubscriptionList()
        {
            List<SubscriptionListItemModel> Subscriptions =
                new List<SubscriptionListItemModel>();

            string sql = @"
                WITH LatestSubscription AS
                (
                    SELECT
                        US.Subscription_ID,
                        US.User_ID,
                        US.OTT_ID,
                        US.started_at,
                        US.expired_at,
                        US.paid_amount,

                        ROW_NUMBER() OVER (
                            PARTITION BY US.User_ID, US.OTT_ID
                            ORDER BY
                                US.expired_at DESC,
                                US.started_at DESC,
                                US.Subscription_ID DESC
                        ) AS RowNumber
                    FROM dbo.UserSubscription US
                )
                SELECT
                    LS.Subscription_ID,
                    LS.User_ID,
                    AU.USER_NAME AS User_Name,
                    LS.OTT_ID,
                    OP.OTT_NAME AS OTT_Name,
                    OP.monthly_price AS Monthly_Price,
                    LS.started_at AS Started_At,
                    LS.expired_at AS Expired_At,
                    LS.paid_amount AS Paid_Amount
                FROM LatestSubscription LS
                INNER JOIN dbo.AppUser AU
                    ON LS.User_ID = AU.User_ID
                INNER JOIN dbo.OTTProvider OP
                    ON LS.OTT_ID = OP.OTT_ID
                WHERE LS.RowNumber = 1
                ORDER BY LS.Subscription_ID DESC;
            ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                SubscriptionListItemModel Subscription =
                    new SubscriptionListItemModel
                    {
                        Subscription_ID = DbReaderHelper.GetInt(reader, "Subscription_ID"),
                        User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                        User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                        OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),
                        OTT_Name = DbReaderHelper.GetString(reader, "OTT_Name"),
                        Monthly_Price = DbReaderHelper.GetInt(reader, "Monthly_Price"),
                        Started_At = DbReaderHelper.GetDateTime(reader, "Started_At"),
                        Expired_At = DbReaderHelper.GetDateTime(reader, "Expired_At"),
                        Paid_Amount = DbReaderHelper.GetInt(reader, "Paid_Amount")
                    };

                Subscriptions.Add(Subscription);
            }

            return Subscriptions;
        }

        // 구독 등록 화면에서 선택할 전체 사용자 목록을 조회
        public List<SubscriptionUserSelectItemModel> GetUserSelectedList()
        {
            List<SubscriptionUserSelectItemModel> users = new List<SubscriptionUserSelectItemModel>();

            string sql = @"
                select
                    User_ID,
                    User_Name,
                    User_BirthDate,
                    User_PhoneNumber,
                    User_Sex
                from dbo.AppUSer
                order by User_Name;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                SubscriptionUserSelectItemModel user = new SubscriptionUserSelectItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_BirthDate = DbReaderHelper.GetDateTime(reader, "User_BirthDate"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber"),
                    User_Sex = DbReaderHelper.GetString(reader, "User_Sex").Trim()
                };
                users.Add(user);
            }
            return users;
        }

        // 구독 등록 화면에서 사용자 검색 결과를 조회
        public List<SubscriptionUserSelectItemModel> SearchUsersForSubscription(string keyword)
        {
            List<SubscriptionUserSelectItemModel> users = new List<SubscriptionUserSelectItemModel>();

            string sql = @"
                SELECT TOP 30
                    User_ID,
                    USER_NAME,
                    User_BirthDate,
                    User_PhoneNumber,
                    User_Sex
                FROM dbo.AppUser
                WHERE
                    CAST(User_ID AS NVARCHAR(20)) LIKE @Keyword
                    OR USER_NAME LIKE @Keyword
                    OR User_PhoneNumber LIKE @Keyword
                ORDER BY USER_NAME;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 50).Value = "%" + keyword.Trim() + "%";

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                SubscriptionUserSelectItemModel user = new SubscriptionUserSelectItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_BirthDate = DbReaderHelper.GetDateTime(reader, "User_BirthDate"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber"),
                    User_Sex = DbReaderHelper.GetString(reader, "User_Sex").Trim()
                };

                users.Add(user);
            }

            return users;
        }

        // 구독 등록 화면에서 선택할 OTT 플랫폼 목록을 조회
        public List<SubscriptionOttPorviderSelectedItemModel> GetOTTProviderListForSubscription()
        {
            List<SubscriptionOttPorviderSelectedItemModel> OttProviders = new List<SubscriptionOttPorviderSelectedItemModel>();

            string sql = @"
                select
                    OTT_ID,
                    OTT_Name,
                    Monthly_Price
                from dbo.OTTProvider
                Order by OTT_Name;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                SubscriptionOttPorviderSelectedItemModel provider = new SubscriptionOttPorviderSelectedItemModel
                {
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),
                    OTT_Name = DbReaderHelper.GetString(reader, "OTT_Name"),
                    Monthly_Price = DbReaderHelper.GetInt(reader, "Monthly_Price")
                };
                OttProviders.Add(provider);
            }

            return OttProviders;
        }

        // 선택한 사용자와 OTT 플랫폼으로 새 구독을 등록
        public int AddUserSubscription(
            int UserID,
            int OTTID,
            DateTime StartedAt,
            DateTime ExpoiredAt,
            int PaidAmount
        )
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddUserSubscription", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@user_id", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@ott_id", SqlDbType.Int).Value = OTTID;
            command.Parameters.Add("@started_at", SqlDbType.Date).Value = StartedAt.Date;
            command.Parameters.Add("@expired_at", SqlDbType.Date).Value = ExpoiredAt.Date;
            command.Parameters.Add("@paid_amount", SqlDbType.Int).Value = PaidAmount;

            connection.Open();

            object? result = command.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToInt32(result);
        }

        // 활성 구독을 한 달 연장하고 결제 금액을 월 요금만큼 추가
        public void RenewActiveSubscriptionOneMonth(int subscriptionID)
        {
            string sql = @"
                Update US
                SET
                    US.expired_at = DATEADD(MONTH,1,US.expired_at),
                    US.paid_amount = US.paid_amount + OP.monthly_price
                From Usersubscription US
                inner join dbo.OTTProvider OP on US.OTT_ID =OP.OTT_ID
                where US.Subscription_ID = @Subscription_ID;";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Subscription_ID", SqlDbType.Int).Value = subscriptionID;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 구독을 오늘 날짜로 해지하고 사용 기간 기준 결제 금액을 재계산
        public void CancelSubscription(int subscriptionID)
        {
            string sql = @"
                UPDATE US
                SET
                    US.expired_at = CAST(GETDATE() AS DATE),
                    US.paid_amount =
                        CASE
                            WHEN CEILING((DATEDIFF(DAY, US.started_at, CAST(GETDATE() AS DATE)) + 1) / 30.0) < 1
                            THEN OP.monthly_price
                            ELSE CAST(CEILING((DATEDIFF(DAY, US.started_at, CAST(GETDATE() AS DATE)) + 1) / 30.0) AS INT) * OP.monthly_price
                        END
                FROM dbo.Usersubscription US
                INNER JOIN dbo.OTTProvider OP
                    ON US.OTT_ID = OP.OTT_ID
                WHERE US.Subscription_ID = @Subscription_ID;
            ";

            SqlConnection connection = DbConnectionFactory.CreateConnection();
            SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Subscription_ID", SqlDbType.Int).Value = subscriptionID;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // CSV 내보내기 경로 설정값을 조회
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
