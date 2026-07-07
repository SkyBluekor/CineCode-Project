using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CINECODE
{
    public class Program
    {
        static async Task Main()
        {
            string apiKey = "9c0e4e583a4f265cff7fb3629dc413fe";
            string query = Uri.EscapeDataString("기생충");

            string searchUrl =
                $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={query}&include_adult=true&language=ko-KR";

            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(searchUrl);
            string json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Code: {(int)response.StatusCode}");
            Console.WriteLine(json);

            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("API 요청 실패");
                return;
            }

            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("results", out JsonElement results))
            {
                Console.WriteLine("results 필드가 없습니다.");
                return;
            }

            if (results.GetArrayLength() == 0)
            {
                Console.WriteLine("검색 결과가 없습니다.");
                return;
            }

            JsonElement movie = results[0];

            int movieId = movie.GetProperty("id").GetInt32();
            string title = movie.GetProperty("title").GetString() ?? "제목 없음";
            bool adult = movie.GetProperty("adult").GetBoolean();

            Console.WriteLine("==== 검색 결과 ====");
            Console.WriteLine($"영화 ID: {movieId}");
            Console.WriteLine($"제목: {title}");
            Console.WriteLine($"adult: {adult}");

            await GetReleaseCertification(client, apiKey, movieId);            
        }

        static async Task GetReleaseCertification(HttpClient client, string apiKey, int movieId)
        {
            string releaseUrl =
                $"https://api.themoviedb.org/3/movie/{movieId}/release_dates?api_key={apiKey}";

            HttpResponseMessage response = await client.GetAsync(releaseUrl);
            string json = await response.Content.ReadAsStringAsync();

            Console.WriteLine();
            Console.WriteLine($"Release Status Code: {(int)response.StatusCode}");
            Console.WriteLine(json);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("관람등급 요청 실패");
                return;
            }

            using JsonDocument doc = JsonDocument.Parse(json);

            JsonElement results = doc.RootElement.GetProperty("results");

            string krCert = "등급 정보 없음";

            foreach (JsonElement country in results.EnumerateArray())
            {
                string iso = country.GetProperty("iso_3166_1").GetString() ?? "";

                if (iso == "KR")
                {
                    JsonElement releaseDates = country.GetProperty("release_dates");

                    foreach (JsonElement release in releaseDates.EnumerateArray())
                    {
                        string cert = release.GetProperty("certification").GetString() ?? "";

                        if (!string.IsNullOrWhiteSpace(cert))
                        {
                            krCert = cert;
                            break;
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"한국 관람등급: {krCert}");
        }
    }
}
