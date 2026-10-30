# CineCode
**OTT 콘텐츠·구독·시청 기록 통합 관리 시스템**

CineCode는 **OTT 서비스 운영자를 위한 Windows 관리자 프로그램**입니다. 영화·TV 콘텐츠를 등록하고, 플랫폼별 제공 정보와 사용자 구독을 관리하며, 시청 기록·평점을 바탕으로 운영 현황을 분석합니다.

동영상 재생 앱이 아니라 **콘텐츠와 OTT 운영 데이터를 관리하는 WPF 데스크톱 애플리케이션**입니다.

**기술 스택** — C# · WPF(.NET 10, XAML/Code-behind) · Microsoft SQL Server(T-SQL) · TMDB API

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png" alt="CineCode 대시보드" width="660"></a>
</p>

## 1. 프로그램 사용 흐름

콘텐츠와 사용자를 준비하고, 구독 여부를 확인해 시청을 기록한 뒤 평점·통계에 활용하는 것이 대표적인 작업 흐름입니다.

```mermaid
flowchart LR
    A["콘텐츠 등록 · 장르/OTT 연결"] --> B["사용자 등록 · OTT 구독"]
    B --> C{"시청 가능한가?"}
    C -->|"예"| D["시청 시작·종료 및 진행률 기록"]
    C -->|"아니오"| R["시청 등록 제한"]
    D --> E{"평점 조건 충족?"}
    E -->|"예"| F["사용자 평점 등록"]
    E -->|"아니오"| N["평점 등록 제한"]
    D --> G["대시보드 · 통계 · CSV"]
    F --> G
```

각 화면은 위 순서대로만 사용해야 하는 것은 아니며, 기능별로 독립적인 조회·관리도 가능합니다.

## 2. 핵심 기능

### 01. 콘텐츠 등록 및 장르·OTT 매핑

- **콘텐츠 등록** — TMDB에서 영화·TV를 검색하여 제목, 공개일, 줄거리, 포스터, 장르, 러닝타임, 관람 등급 등을 입력 화면으로 가져옵니다.
- **콘텐츠 관리** — 등록한 콘텐츠를 검색·조회하고 수정·삭제합니다.
- **제공 관계 관리** — 장르와 OTT 제공자를 등록·관리하고, 콘텐츠별 장르·OTT 연결을 추가하거나 해제합니다.

**사용 예시** — 콘텐츠를 등록한 뒤 제공 OTT를 연결하면 시청 기록 등록 시 그 플랫폼에서 실제로 제공하도록 설정했는지 검증할 수 있습니다.

**관련 코드** — [콘텐츠 등록 화면](CineCode/CineCode/Views/ContentManagement/ContentAddView.xaml.cs) · [매핑 서비스](CineCode/CineCode/Services/ContentMappingService.cs)

### 02. 사용자·구독 관리

- **사용자 관리** — 사용자 목록 검색·조회, 이름·생년월일·연락처 등의 신규 정보 등록
- **구독 등록** — 사용자와 OTT를 선택해 구독 시작일·만료일·결제 금액 저장
- **구독 운영** — 상태·만료 예정 내역 조회, 구독 한 달 연장 및 해지
- **중복 검증** — 동일 사용자의 동일 OTT 구독 기간이 겹치는 등록 요청 제한

**현재 구현 범위** — 사용자 수정·삭제 *화면은 존재하지만*, 해당 버튼의 실제 DB 수정·삭제 기능은 아직 연결되지 않았습니다.

**관련 코드** — [구독 관리 서비스](CineCode/CineCode/Services/SubscriptionManagementService.cs)

### 03. 시청 기록 관리

- **시청 시작** — 사용자·콘텐츠·OTT를 선택하여 시작 기록 등록
- **등록 조건** — 해당 OTT의 콘텐츠 제공 여부, 시청일의 활성 구독, 사용자 관람 연령 확인
- **시청 종료** — 입력한 시청 시간으로 진행률 및 완료 여부 계산
- **기록 관리** — 검색·조회, 종료된 기록의 시청 시간 수정, 삭제 및 CSV 추출

**핵심 규칙** — 콘텐츠 제공·구독·관람 연령 조건을 충족하지 못하면 시청 시작 기록을 생성하지 않습니다.

**관련 코드** — [WatchHistoryService.cs](CineCode/CineCode/Services/WatchHistoryService.cs)

### 04. 사용자 평점 관리

- **평가 대상** — 사용자별 시청 기록을 확인하여 평가할 수 있는 콘텐츠 목록 제공
- **등록 조건** — 시청이 종료됐고 최소 진행률(기본 **50%**)을 충족해야 평점 등록 가능
- **평점 관리** — **0~5점, 0.5점 단위**로 등록·수정, 기존 평점 삭제 및 목록 검색·필터링

**핵심 규칙** — 실제 시청 이력과 진행률 조건을 확인한 후 평점을 저장합니다.

**관련 코드** — [UserRatingService.cs](CineCode/CineCode/Services/UserRatingService.cs)

### 05. 대시보드·통계·CSV 및 설정

- **대시보드** — 콘텐츠·사용자·OTT·활성 구독 현황과 장르·OTT 미연결 콘텐츠 수 표시
- **운영 통계** — 월별 시청 시간·횟수, 전월·전분기·전년 대비 변화, 플랫폼별 시청 점유율과 인기 콘텐츠·장르 확인
- **조회 필터** — 기간·콘텐츠 유형·OTT·장르·검색어 등 조건 선택
- **내보내기** — 사용자·콘텐츠·구독·시청·평점 등의 관리 데이터를 CSV 파일로 저장
- **환경 설정** — CSV 저장 경로와 평점 등록의 최소 시청 진행률 관리

**관련 코드** — [DashboardService.cs](CineCode/CineCode/Services/DashboardService.cs) · [StatisticsService.cs](CineCode/CineCode/Services/StatisticsService.cs)

## 3. 시스템 구조

```mermaid
flowchart LR
    Admin["운영자"] --> UI["WPF 화면 (XAML / Code-behind)"]
    subgraph App["CineCode · .NET 10"]
        UI --> S["C# Services"]
        UI --> T["TmdbService"]
        UI -.-> C["일부 CSV 직접 조회"]
        S --> D["DbConnectionFactory / SqlClient"]
        C --> D
    end
    T <-->|"HTTP · JSON"| API["TMDB API"]
    D <--> DB[("SQL Server · CINECODEDB")]
```

| 구성 | 역할 |
|---|---|
| **WPF 화면** | 입력·검색·조회·화면 전환 |
| **C# Service** | 기능별 데이터 처리와 DB 접근 |
| **SQL Server** | 데이터 저장, 제약조건과 저장 프로시저를 통한 업무 규칙 검증 |
| **TMDB API** | 영화·TV 외부 정보 검색 및 등록 지원 |

**구현 특징** — WPF Code-behind와 Service를 사용합니다. 일부 CSV 기능은 화면 코드에서 SQL을 직접 실행하며, 전체를 MVVM 구조로 소개하지 않습니다.

## 4. 데이터베이스 설계

- **테이블 구성** — 콘텐츠·장르·OTT·사용자·구독·시청·평점·설정을 총 **10개 테이블**로 관리합니다.
- **관계 설계** — `ContentGenre`, `ContentOTTProvider`로 콘텐츠와 장르·OTT의 다대다 관계를 분리했습니다.
- **무결성 관리** — PK·FK·UNIQUE·CHECK 제약조건 및 저장 프로시저를 적용했습니다.

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png" alt="CINECODEDB ERD" width="850"></a>
</p>

**관련 구현** — [ContentService.cs](CineCode/CineCode/Services/ContentService.cs) · [TmdbService.cs](CineCode/CineCode/Services/TmdbService.cs) · [테이블·저장 프로시저](First_Query.sql)

## 5. 문제 해결 경험

원래 포트폴리오에서 정리한 실제 개발 경험을 기준으로, **어떤 문제가 있었고 어느 단계에서 해결했는지**를 설명합니다.

### 01. TMDB 연결 실패 시 콘텐츠 등록 흐름 중단

- **문제** — TMDB 서버 응답이 없거나 네트워크 오류가 발생할 때 예외가 사용자 화면까지 전달되어 프로그램 흐름이 중단될 수 있었습니다.
- **해결** — 콘텐츠 등록 화면에서 TMDB 검색·상세 정보 조회의 예외를 처리하고 오류 메시지를 표시했습니다. TMDB 검색이 실패해도 콘텐츠 정보를 직접 입력할 수 있도록 했습니다.
- **결과** — 외부 API 오류가 전체 콘텐츠 등록 작업의 중단으로 이어지지 않도록 구성했습니다.

**관련 코드** — [ContentAddView.xaml.cs](CineCode/CineCode/Views/ContentManagement/ContentAddView.xaml.cs) · [TmdbService.cs](CineCode/CineCode/Services/TmdbService.cs)

### 02. 아동 사용자의 성인 콘텐츠 시청 등록

- **문제** — OTT 구독 여부만 확인하면 연령 제한 콘텐츠도 시청 기록으로 남을 수 있었습니다.
- **해결** — `Usp_StartWatchHistory`에서 사용자 생년월일(`User_BirthDate`)과 콘텐츠 관람 등급(`Age_Rating`)을 비교하도록 검증을 추가했습니다.
- **결과** — 관람 연령 조건에 맞지 않는 시청 기록의 등록을 차단했습니다.

**관련 코드** — [First_Query.sql](First_Query.sql)

### 03. 제공하지 않는 OTT로 시청 기록 생성

- **문제** — 콘텐츠와 OTT를 각각 선택하는 구조에서 실제 제공되지 않는 조합으로 시청 기록이 저장될 수 있었습니다.
- **해결** — `WatchHistory`의 `(Content_ID, Ott_ID)`가 `ContentOTTProvider`의 동일한 조합을 참조하도록 **복합 외래키(FK)**를 설계했습니다. 시청 시작 저장 프로시저에서도 제공 관계를 확인합니다.
- **결과** — 실제 연결된 콘텐츠·OTT 조합만 시청 기록으로 저장할 수 있도록 했습니다.

**관련 코드** — [First_Query.sql](First_Query.sql)

### 04. 시청률 미달 콘텐츠의 평점 등록

- **문제** — 시청하지 않았거나 충분히 시청하지 않은 콘텐츠에도 평점을 등록하면 평가 데이터의 신뢰성이 떨어집니다.
- **해결** — `SystemSetting.MIN_RATING_PROGRESS`(기본 **50%**)와 종료된 시청 기록을 바탕으로 `Usp_AddOrUpdateUserRating`에서 평가 자격을 검증했습니다.
- **결과** — 시청 조건을 만족한 콘텐츠만 평점을 등록·수정할 수 있도록 제한했습니다.

**관련 코드** — [UserRatingService.cs](CineCode/CineCode/Services/UserRatingService.cs) · [First_Query.sql](First_Query.sql)

**화면 개선 경험** — DataGrid에서 글자가 가려지거나 정렬이 어긋나는 문제도 있었으며, 열 너비와 배치 기준을 조정해 화면 가독성을 보완했습니다.

## 6. 실제 프로그램 화면

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

## 7. 실행 및 빌드

**필요 환경** — Windows · .NET 10 SDK · SQL Server · 사전에 구성된 `CINECODEDB` · TMDB API 키

DB 백업 파일은 공개 저장소에 포함되지 않습니다. [`First_Query.sql`](First_Query.sql)은 전체 재실행용 초기화 스크립트가 아니라 DB 구조 참고 자료입니다.

<details>
<summary><strong>Windows PowerShell 환경 변수 설정 및 빌드 명령어</strong></summary>

**PowerShell에서 TMDB API 키 등록**

```powershell
setx TMDB_API_KEY "본인의_TMDB_API_KEY"
```

**다른 SQL Server 주소를 사용할 때(선택 사항)**

```powershell
setx CINECODE_DB_SERVER "localhost\SQLEXPRESS"
```

환경 변수 등록 후 PowerShell을 새로 열고, **저장소 최상위 폴더**에서 빌드합니다.

```powershell
dotnet build .\CineCode\CineCode\CineCode.csproj
```

</details>
