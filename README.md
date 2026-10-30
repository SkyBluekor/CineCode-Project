# CineCode
### OTT 콘텐츠 및 서비스 운영 관리 시스템

CineCode는 영화와 드라마 콘텐츠, OTT 플랫폼, 사용자 구독, 시청 기록과 평점 정보를 하나의 프로그램에서 관리하기 위해 개발한 **WPF 기반 데스크톱 관리자 프로그램**입니다.

## 기획 배경

OTT 서비스를 운영하려면 콘텐츠 정보뿐 아니라 어떤 플랫폼에서 제공되는지, 사용자가 어떤 서비스를 구독하고 있는지, 콘텐츠를 얼마나 시청했는지 등 다양한 정보를 함께 관리해야 합니다.

이 데이터들은 서로 연결되어 있기 때문에 단순히 정보를 저장하는 것만으로는 충분하지 않습니다.

예를 들어 사용자가 콘텐츠를 시청하려면 해당 OTT를 구독 중이어야 하고, 콘텐츠가 그 플랫폼에서 제공되어야 합니다. 평점을 등록할 때도 실제 시청 기록을 확인할 필요가 있습니다.

CineCode는 이러한 **데이터 간 관계와 업무 규칙을 체계적으로 관리하는 프로그램**을 만드는 것을 목표로 개발했습니다.

## 개발 목표

- **통합 관리:** 콘텐츠, 사용자, 구독, 시청 기록, 평점을 하나의 프로그램에서 관리
- **데이터 신뢰성:** 구독 기간 중복, 시청 가능 여부, 평점 등록 조건 등을 데이터베이스에서 검증
- **관리 업무 편의성:** 대시보드, 통계, 검색, CSV 내보내기를 통해 운영 정보 확인
- **외부 데이터 연동:** TMDB API를 활용해 영화·드라마 정보를 검색하고 콘텐츠 등록 과정에 활용

## 기술 스택

- **Language:** C#, SQL (T-SQL)
- **Framework:** WPF, .NET 10
- **Database:** Microsoft SQL Server
- **External API:** TMDB API

## 실제 프로그램 화면

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png"><img src="CineCode/CineCode/Images/ViewScreenImages/DashboardView.png" alt="CineCode 대시보드" width="760"></a>
</p>

## 주요 기능

- 대시보드: 콘텐츠, 사용자, 구독, 매핑 현황 요약
- 콘텐츠 관리: 영화/TV 콘텐츠 등록, 조회, TMDB 기반 정보 보조 입력
- 사용자 관리: 사용자 목록, 신규 사용자 등록, 연령/구독 상태 관리
- 구독 관리: OTT 플랫폼별 구독 내역, 만료 예정 구독 조회
- 시청 기록: 시청 시작/완료 기록과 진행률 관리
- 평점 관리: 최소 시청률 조건 기반 사용자 평점 등록
- 콘텐츠 매핑: 장르, OTT 제공자, 콘텐츠 연결 관계 관리
- 통계: 인기 콘텐츠, 플랫폼 점유율, 월별 시청 활동 분석

## 시스템 구조

CineCode는 **WPF 화면, C# 서비스 계층, Microsoft SQL Server 데이터베이스**를 연결한 데스크톱 관리 프로그램입니다.

사용자가 화면에서 콘텐츠를 등록하거나 구독·시청 정보를 관리하면, C# 서비스가 SQL Server에 데이터를 요청하고 처리 결과를 화면에 반영합니다.

**주요 데이터 처리 흐름**

```text
WPF 화면 (XAML / Code-behind)
    ↓
C# Service 계층
    ↓
Microsoft.Data.SqlClient
    ↓
Microsoft SQL Server (저장 프로시저 및 테이블)
```

콘텐츠 검색 시에는 `TmdbService`를 통해 TMDB API에서 영화·드라마 정보를 가져와 등록 과정에 활용합니다.

### 주요 구성 요소

| 구성 | 역할 |
|---|---|
| **WPF View** | 관리자 화면 구성, 사용자 입력 및 이벤트 처리 |
| **Service** | 콘텐츠·구독·시청·평점 등 기능별 데이터 처리 |
| **Model** | 조회 결과와 화면 표시 데이터 관리 |
| **DbConnectionFactory** | SQL Server 연결 생성 및 연결 설정 관리 |
| **SQL Server** | 관계형 데이터 저장, 제약조건 및 저장 프로시저를 통한 업무 규칙 처리 |
| **TMDB API** | 영화·TV 콘텐츠 검색 및 상세 정보 조회 |

주요 업무 기능은 Service 클래스로 분리했으며, 일부 화면의 CSV 내보내기 기능은 화면 코드에서 데이터베이스를 직접 조회합니다.

### 실제 처리 예시 — 시청 기록 등록

1. 관리자가 WPF 화면에서 사용자, 콘텐츠, OTT 플랫폼을 선택합니다.
2. `WatchHistoryService`에서 `Usp_StartWatchHistory` 저장 프로시저를 호출합니다.
3. SQL Server에서 콘텐츠 제공 여부, 사용자 구독 상태, 관람 연령 조건을 검증합니다.
4. 모든 조건을 만족하면 시청 기록을 저장하고 결과를 화면에 반영합니다.

이처럼 화면 입력뿐 아니라 **데이터베이스에서도 업무 규칙을 검증**하도록 구성했습니다.

## 화면

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

## 데이터베이스 설계 (ERD)

CineCode는 Microsoft SQL Server에 **10개 테이블**을 구성하여 콘텐츠, OTT 플랫폼, 사용자와 시청 데이터를 관계형 구조로 관리합니다.

<p align="center">
  <a href="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png"><img src="CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png" alt="CINECODEDB ERD" width="850"></a>
</p>

### 주요 테이블 구성

| 분류 | 테이블 | 역할 |
|---|---|---|
| 콘텐츠 | `Content`, `Genre` | 영화·TV 콘텐츠 및 장르 정보 |
| 콘텐츠 연결 | `ContentGenre`, `ContentOTTProvider` | 콘텐츠와 장르·OTT의 다대다 관계 관리 |
| 사용자·구독 | `AppUser`, `OTTProvider`, `Usersubscription` | 사용자, OTT 제공자, 구독 기간 및 결제 정보 |
| 시청·평점 | `WatchHistory`, `UserRating` | 시청 기록·진행률·사용자 평가 |
| 시스템 설정 | `SystemSetting` | CSV 저장 경로, 최소 평점 등록 진행률 등 |

### 핵심 설계 특징

**1. 다대다 관계 분리**

하나의 콘텐츠는 여러 장르에 속하거나 여러 OTT 플랫폼에서 제공될 수 있습니다. 이를 `ContentGenre`, `ContentOTTProvider` 중간 테이블로 분리해 중복 데이터와 관계를 관리했습니다.

**2. 데이터 무결성 확보**

기본키(PK), 외래키(FK), UNIQUE·CHECK 제약조건을 활용해 잘못된 참조나 중복 등록, 유효하지 않은 데이터 저장을 방지하도록 설계했습니다.

**3. 시청 기록의 관계 검증**

`WatchHistory`는 사용자와 콘텐츠·OTT 조합을 참조합니다. 시청 시작 시에는 저장 프로시저에서 실제 콘텐츠 제공 여부와 활성 구독 상태를 추가로 확인합니다.

**4. 업무 규칙과 데이터 접근**

구독 등록, 시청 시작·종료, 평점 처리, 콘텐츠 관리 및 통계 조회에 저장 프로시저를 사용했습니다. 주요 조회 조건에 맞춰 인덱스도 구성했습니다.

자세한 데이터베이스 정의와 저장 프로시저는 저장소의 [`First_Query.sql`](First_Query.sql)에서 확인할 수 있습니다.

## 핵심 기술 구현

CineCode는 데이터를 저장하는 것뿐 아니라 **잘못된 정보가 등록되지 않도록 관리하는 것**에 중점을 두었습니다.

이를 위해 C#과 Microsoft SQL Server를 연동하고, 트랜잭션과 저장 프로시저를 활용해 주요 기능을 구현했습니다.

### 1. 콘텐츠 등록 — 데이터가 일부만 저장되는 문제 방지

영화나 드라마를 등록할 때는 콘텐츠 정보뿐 아니라 장르와 OTT 플랫폼 연결 정보도 함께 저장해야 합니다.

예를 들어 영화 정보는 저장됐지만 장르 연결에 실패하면 데이터가 불완전해질 수 있습니다.

이를 해결하기 위해 C#의 **`SqlTransaction`(트랜잭션)**을 사용했습니다.

콘텐츠 등록, 장르 연결, OTT 연결을 하나의 작업으로 묶고, 모든 작업이 성공하면 `Commit`으로 저장합니다. 도중에 오류가 발생하면 `Rollback`으로 이전 작업까지 취소합니다.

**구현 효과:** 콘텐츠와 연결 정보가 일부만 저장되는 상황을 방지합니다.

관련 코드: [ContentService.cs](CineCode/CineCode/Services/ContentService.cs)

### 2. 구독 관리 — 중복 구독과 동시 요청 처리

한 사용자가 동일한 OTT 플랫폼을 같은 기간에 중복 구독하지 않도록 검증했습니다.

SQL Server의 **`Usp_AddUserSubscription` 저장 프로시저**에서 구독 날짜가 기존 구독 기간과 겹치는지 확인합니다.

또한 여러 등록 요청이 동시에 들어오는 상황을 고려해 `UPDLOCK`, `HOLDLOCK`을 적용했습니다. 이는 데이터 확인과 등록 사이에 다른 요청이 끼어들어 중복 데이터가 발생할 위험을 줄이기 위한 잠금 처리입니다.

사용자와 OTT의 존재 여부, 날짜와 결제 금액도 함께 검증합니다.

**구현 효과:** 중복 구독을 방지하고 여러 요청이 동시에 처리될 때 데이터 일관성을 유지하도록 설계했습니다.

### 3. 시청 기록 — 실제 시청 가능한 콘텐츠인지 확인

사용자가 콘텐츠를 선택했다고 해서 무조건 시청 기록을 등록하지는 않습니다.

**`Usp_StartWatchHistory` 저장 프로시저**에서는 다음 조건을 확인합니다.

- 선택한 OTT에서 실제 제공하는 콘텐츠인가?
- 시청 시작일에 사용자가 해당 OTT를 구독 중인가?
- 사용자의 나이가 콘텐츠 관람 등급을 만족하는가?

조건을 만족해야 시청 기록이 생성됩니다.

시청을 종료할 때는 `Usp_EndWatchHistory`에서 콘텐츠의 전체 러닝타임과 입력된 시청 시간을 비교하여 **시청 진행률과 완료 여부를 자동 계산**합니다.

**구현 효과:** 시청할 수 없는 콘텐츠의 기록을 차단하고, 시청 상태를 일관된 기준으로 관리합니다.

관련 코드: [WatchHistoryService.cs](CineCode/CineCode/Services/WatchHistoryService.cs)

### 4. 사용자 평점 — 실제 시청 기록을 바탕으로 평가

콘텐츠를 시청하지 않은 사용자도 평점을 남길 수 있다면 평가 데이터의 신뢰성이 떨어질 수 있습니다.

이를 방지하기 위해 **`Usp_AddOrUpdateUserRating` 저장 프로시저**에서 평점 등록 전에 시청 기록을 확인합니다.

시청이 종료되었고, 설정된 최소 시청 진행률을 충족해야 평점을 등록할 수 있습니다.

최소 진행률은 `SystemSetting`의 `MIN_RATING_PROGRESS`에서 관리하며, 프로그램 기본값은 **50%**입니다.

평점은 **0~5점, 0.5점 단위**로 제한하고 평가일이 시청 종료일보다 빠르지 않은지도 확인합니다. 동일한 사용자와 콘텐츠의 평점이 이미 존재하면 새로 등록하는 대신 기존 평점을 수정합니다.

**구현 효과:** 실제 시청 기록에 근거한 평점만 등록되도록 하여 평가 데이터의 신뢰성을 높입니다.

관련 코드: [UserRatingService.cs](CineCode/CineCode/Services/UserRatingService.cs)

저장 프로시저 및 데이터베이스 구현: [First_Query.sql](First_Query.sql)

## 실행 준비

1. SQL Server Express를 설치하고 실행합니다.
2. `CINECODEDB` 데이터베이스를 생성합니다.
3. 저장소의 SQL 스크립트를 이용해 테이블과 기본 데이터를 준비합니다.
4. TMDB API 키를 Windows 환경 변수로 등록합니다.

```powershell
setx TMDB_API_KEY "본인의_TMDB_API_KEY"
```

기본 DB 서버 후보는 `.\SQLEXPRESS`, `localhost\SQLEXPRESS`, `{컴퓨터이름}\SQLEXPRESS`입니다.  
다른 SQL Server 인스턴스를 쓰는 경우 아래 환경 변수를 등록할 수 있습니다.

```powershell
setx CINECODE_DB_SERVER "localhost\SQLEXPRESS"
```

## 빌드

```powershell
dotnet build .\CineCode\CineCode.slnx
```

## 공개 저장소 관리 메모

API 키, DB 백업, 설치 파일, PDF/DOCX/HWP 포트폴리오 문서, 로컬 빌드 산출물은 저장소에 포함하지 않습니다.  
실행에 필요한 개인 설정은 환경 변수 또는 로컬 파일로만 관리합니다.
