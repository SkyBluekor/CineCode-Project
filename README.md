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

![Dashboard](CineCode/CineCode/Images/ViewScreenImages/DashboardView.png)

## 주요 기능

- 대시보드: 콘텐츠, 사용자, 구독, 매핑 현황 요약
- 콘텐츠 관리: 영화/TV 콘텐츠 등록, 조회, TMDB 기반 정보 보조 입력
- 사용자 관리: 사용자 목록, 신규 사용자 등록, 연령/구독 상태 관리
- 구독 관리: OTT 플랫폼별 구독 내역, 만료 예정 구독 조회
- 시청 기록: 시청 시작/완료 기록과 진행률 관리
- 평점 관리: 최소 시청률 조건 기반 사용자 평점 등록
- 콘텐츠 매핑: 장르, OTT 제공자, 콘텐츠 연결 관계 관리
- 통계: 인기 콘텐츠, 플랫폼 점유율, 월별 시청 활동 분석

## 화면

### 콘텐츠 관리

![Content Management](CineCode/CineCode/Images/ViewScreenImages/ContentManagementListView.png)

### 콘텐츠 등록

![Content Add](CineCode/CineCode/Images/ViewScreenImages/ContentManagementAddView.png)

### 콘텐츠 매핑

![Content Mapping](CineCode/CineCode/Images/ViewScreenImages/ContentMappingConnectionsView.png)

### 구독 관리

![Subscriptions](CineCode/CineCode/Images/ViewScreenImages/SubscriptionsListView.png)

### 시청 기록

![Watch History](CineCode/CineCode/Images/ViewScreenImages/WatchHistoryStartView.png)

### 통계

![Statistics](CineCode/CineCode/Images/ViewScreenImages/StatisticsView.png)

## ERD

![ERD](CineCode/CineCode/Images/ViewScreenImages/ERD_DIagram.png)

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
