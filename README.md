# CineCode
### OTT 콘텐츠 운영 관리 시스템

영화·TV 콘텐츠, OTT 플랫폼, 사용자 구독과 시청·평점 기록을 통합 관리하는 **WPF 데스크톱 프로그램**입니다. 콘텐츠 등록부터 시청·평점 처리까지의 업무 규칙을 SQL Server에서 검증하고, 운영 데이터를 통계로 제공합니다.

**기술 스택:** C# · WPF (.NET 10) · Microsoft SQL Server (T-SQL) · TMDB API

## 주요 기능

- **콘텐츠 관리:** 영화·TV 등록, 장르·OTT 연결, TMDB 검색을 통한 정보 입력
- **사용자·구독 관리:** 사용자 등록·조회, OTT 구독 및 만료 정보 관리
- **시청·평점 관리:** 시청 가능 조건 확인, 진행률·완료 처리, 시청 기록 기반 평점 등록
- **운영 분석:** 월별 시청 활동, OTT 점유율, 인기 콘텐츠·장르 통계와 CSV 내보내기

## 핵심 구현

- **트랜잭션으로 데이터 일관성 유지:** 콘텐츠·장르·OTT 연결을 `SqlTransaction`으로 묶어 모두 성공하면 저장(`Commit`), 중간에 실패하면 취소(`Rollback`)합니다. [ContentService.cs](CineCode/CineCode/Services/ContentService.cs)
- **중복 구독 및 동시 요청 제어:** 구독 기간 겹침을 검사하고 SQL Server의 `UPDLOCK`, `HOLDLOCK`을 활용해 동시에 등록할 때 중복이 발생할 위험을 줄였습니다.
- **시청·평점 유효성 검증:** OTT 콘텐츠 제공 여부, 활성 구독, 관람 연령을 확인해 시청을 등록합니다. 시청 종료 시 진행률을 계산하며, 평점은 종료된 기록이 최소 진행률(기본 50%)을 충족할 때만 0~5점(0.5점 단위)으로 등록·수정합니다. [WatchHistoryService.cs](CineCode/CineCode/Services/WatchHistoryService.cs) · [UserRatingService.cs](CineCode/CineCode/Services/UserRatingService.cs)
- **TMDB API 데이터 통합:** `HttpClient`와 `async/await`로 영화·TV 데이터를 비동기 조회하고, 서로 다른 제목·공개일 필드를 공통 모델로 변환합니다. 러닝타임과 관람 등급도 콘텐츠 유형에 따라 조회합니다. [TmdbService.cs](CineCode/CineCode/Services/TmdbService.cs)
- **SQL 운영 통계:** `JOIN`, `GROUP BY`, 저장 프로시저를 이용해 시청·구독·평점 데이터를 집계하고, 기간 비교·플랫폼 점유율·인기 콘텐츠 순위를 제공합니다. [StatisticsService.cs](CineCode/CineCode/Services/StatisticsService.cs)

## 시스템 구조 및 데이터베이스

**처리 흐름:** WPF(XAML / Code-behind) → C# Service → `Microsoft.Data.SqlClient` → SQL Server

SQL Server의 **10개 테이블**을 관계형으로 설계했습니다. `ContentGenre`와 `ContentOTTProvider`로 다대다 관계를 분리하고, PK·FK 및 제약조건과 저장 프로시저로 데이터 무결성과 업무 규칙을 관리합니다.

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png" alt="CINECODEDB ERD" width="850"></a>
</p>

[데이터베이스 정의 및 저장 프로시저](First_Query.sql)

## 프로그램 화면

**대시보드**

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png" alt="CineCode 대시보드" width="760"></a>
</p>

**주요 화면**

<table>
  <tr>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/ContentManagementListView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ContentManagementListView.png" width="350" alt="콘텐츠 관리 화면"></a><br>
      <sub><strong>콘텐츠 관리</strong></sub>
    </td>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/ContentManagementAddView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ContentManagementAddView.png" width="350" alt="콘텐츠 등록 화면"></a><br>
      <sub><strong>콘텐츠 등록</strong></sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/ContentMappingConnectionsView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ContentMappingConnectionsView.png" width="350" alt="콘텐츠 매핑 화면"></a><br>
      <sub><strong>콘텐츠 매핑</strong></sub>
    </td>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/SubscriptionsListView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/SubscriptionsListView.png" width="350" alt="구독 관리 화면"></a><br>
      <sub><strong>구독 관리</strong></sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/WatchHistoryStartView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/WatchHistoryStartView.png" width="350" alt="시청 기록 화면"></a><br>
      <sub><strong>시청 기록</strong></sub>
    </td>
    <td align="center" width="50%">
      <a href="CineCode/CineCode/Images/ViewScreenImages/StatisticsView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/StatisticsView.png" width="350" alt="통계 화면"></a><br>
      <sub><strong>통계</strong></sub>
    </td>
  </tr>
</table>

## 실행 및 빌드

Windows, **.NET 10 SDK**, SQL Server와 사전에 구성된 `CINECODEDB`가 필요합니다. 공개 저장소에 DB 백업은 포함하지 않으며, [`First_Query.sql`](First_Query.sql)은 전체를 재실행하는 초기화 스크립트가 아닌 구조 참고 자료입니다.

**Windows PowerShell**에서 TMDB API 키를 등록합니다.

```powershell
setx TMDB_API_KEY "본인의_TMDB_API_KEY"
```

SQL Server 주소를 직접 지정하려면 다음 명령어를 사용합니다(선택 사항).

```powershell
setx CINECODE_DB_SERVER "localhost\SQLEXPRESS"
```

환경 변수 등록 후 PowerShell을 새로 열고, **저장소 최상위 폴더**에서 빌드합니다.

```powershell
dotnet build .\CineCode\CineCode\CineCode.csproj
```
