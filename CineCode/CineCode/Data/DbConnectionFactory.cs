using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CineCode.Data
{
    
    /// 애플리케이션에서 사용할 SQL Server 연결을 생성합니다.    
    public class DbConnectionFactory
    {
        private const string DatabaseName = "CINECODEDB";
        private const string ServerEnvironmentVariable = "CINECODE_DB_SERVER";

        private static string? _cachedConnectionString;
        
        /// 설정된 연결 문자열을 사용해 새로운 SQL 연결 객체를 반환합니다.        
        public static SqlConnection CreateConnection()
        {
            _cachedConnectionString ??= FindWorkingConnectionString();

            return new SqlConnection(_cachedConnectionString);
        }
        
        /// CINECODEDB에 실제로 연결 가능한 ConnectionString을 찾습니다.        
        private static string FindWorkingConnectionString()
        {
            List<string> candidates = GetCandidateServerNames();

            List<string> failedMessages = new List<string>();

            foreach (string serverName in candidates)
            {
                string connectionString = BuildConnectionString(serverName);

                if (CanConnect(connectionString, out string errorMessage))
                {
                    return connectionString;
                }

                failedMessages.Add($"{serverName} 실패: {errorMessage}");
            }

            string triedServers = string.Join("\n", failedMessages);

            throw new InvalidOperationException(
                "CINECODEDB에 연결할 수 없습니다.\n\n" +
                "확인할 항목:\n" +
                "1. SQL Server Express가 실행 중인지 확인하세요.\n" +
                "2. SSMS에서 .\\SQLEXPRESS 또는 localhost\\SQLEXPRESS 접속이 되는지 확인하세요.\n" +
                "3. CINECODEDB 데이터베이스가 복원되어 있는지 확인하세요.\n" +
                "4. 복원된 DB 이름이 정확히 CINECODEDB인지 확인하세요.\n\n" +
                "시도한 연결 정보:\n" +
                triedServers);
        }
        
        /// 연결 문자열을 생성합니다.        
        private static string BuildConnectionString(string serverName)
        {
            return
                $"Data Source={serverName};" +
                $"Initial Catalog={DatabaseName};" +
                "Integrated Security=True;" +
                "Encrypt=False;" +
                "TrustServerCertificate=True;" +
                "Connection Timeout=5;";
        }
        
        /// 실제 DB 연결 가능 여부를 확인합니다.       
        private static bool CanConnect(string connectionString, out string errorMessage)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
        
        /// 접속 후보 SQL Server 서버 이름 목록을 만듭니다.        
        private static List<string> GetCandidateServerNames()
        {
            List<string> candidates = new List<string>();

            string? configuredServerName =
                Environment.GetEnvironmentVariable(ServerEnvironmentVariable);

            if (!string.IsNullOrWhiteSpace(configuredServerName))
            {
                candidates.Add(configuredServerName.Trim());
            }

            // 현재 PC 기준 기본 후보
            candidates.Add(@".\SQLEXPRESS");
            candidates.Add(@"localhost\SQLEXPRESS");
            candidates.Add($@"{Environment.MachineName}\SQLEXPRESS");

            // 레지스트리에서 설치된 SQL Server 인스턴스 검색
            List<string> installedInstances = GetInstalledSqlInstanceNames();

            foreach (string instanceName in installedInstances)
            {
                candidates.Add($@".\{instanceName}");
                candidates.Add($@"localhost\{instanceName}");
                candidates.Add($@"{Environment.MachineName}\{instanceName}");
            }

            return candidates
                .Where(serverName => !string.IsNullOrWhiteSpace(serverName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        
        /// 현재 PC에 설치된 SQL Server 인스턴스 이름을 레지스트리에서 읽습니다.        
        private static List<string> GetInstalledSqlInstanceNames()
        {
            List<string> result = new List<string>();

            ReadInstancesFromRegistry(RegistryView.Registry64, result);
            ReadInstancesFromRegistry(RegistryView.Registry32, result);

            return result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


        /// SQL Server 인스턴스 레지스트리를 읽습니다.
        
        private static void ReadInstancesFromRegistry(
            RegistryView registryView,
            List<string> result)
        {
            try
            {
                using RegistryKey baseKey =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);

                using RegistryKey? key = baseKey.OpenSubKey(
                    @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");

                if (key == null)
                {
                    return;
                }

                foreach (string instanceName in key.GetValueNames())
                {
                    result.Add(instanceName);
                }
            }
            catch
            {
                // 레지스트리 조회 실패 시 무시합니다.
                // 기본 후보 서버 목록은 이미 시도합니다.
            }
        }
    }
}
