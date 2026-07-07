/* =========================================================
   1. 실행 환경 및 데이터베이스 선택
   ========================================================= */
-- 이 파일은 기존 개발 DB의 구조 문서화용이며 전체 재실행하지 않는다.
CREATE DATABASE CINECODEDB
GO
USE CINECODEDB
GO
/* =========================================================
   2. 테이블 생성문
   ========================================================= */
/* =========================================================
   테이블: Content
   용도: 영화/TV 콘텐츠의 기본 정보와 TMDB 정보를 저장한다.
   주요 사용 화면: ContentManagement, WatchHistory, UserRating, Statistics
   ========================================================= */
CREATE TABLE Content
(
    Content_ID int IDENTITY(1,1) not null, -- 콘텐츠를 구분하는 기본키
    TMDB_ID int null,                      -- TMDB API에서 제공하는 콘텐츠 ID
    Content_Type varchar(10) not null,     -- 콘텐츠 타입 : MOVIE 또는 TV
    Title NVARCHAR(50) not null,           -- 콘텐츠 제목
    Release_Date date null,                -- 개봉일 또는 방영일
    TMDB_Average_Rating DECIMAL(3,1) null, -- TMDB 평균 평점
    OverView NVARCHAR(Max) null,           -- 콘텐츠 줄거리 또는 설명
    Poster_URL NVARCHAR(500) null,         -- 콘텐츠 포스터 이미지 경로
    Running_Time int null,                 -- 콘텐츠 러닝 타임
    Age_Rating nVarchar(5) null,           -- 콘텐츠 관람 등급
    CONSTRAINT PK_Content PRIMARY KEY (Content_ID), -- 콘텐츠 기본키 제약조건
    CONSTRAINT CK_Content_Type CHECK (Content_Type IN ('MOVIE', 'TV')), -- 콘텐츠 타입 제한
    CONSTRAINT Ck_Content_TMDB_Average_Rating CHECK (Tmdb_average_rating is null or TMDB_Average_Rating BETWEEN 0 AND 10), -- 평점 범위 제한
    CONSTRAINT CK_Content_Running_Time CHECK (Running_Time > 0 or Running_Time is null) -- 러닝 타임은 0보다 크거나 NULL 허용
)

/* =========================================================
   테이블: Genre
   용도: 콘텐츠 유형별 장르 기준 정보를 저장한다.
   주요 사용 화면: ContentMapping, ContentManagement, Statistics
   ========================================================= */
CREATE TABLE Genre
(
    Genre_ID int IDENTITY(1,1) not null, -- 장르를 구분하는 기본키
    TMDB_Genre_ID int null,              -- TMDB API에서 제공하는 장르 ID
    Content_Type varchar(10) not null,   -- 장르가 적용되는 콘텐츠 타입
    Genre_Name NVARCHAR(50) not null,    -- 장르 이름
    CONSTRAINT PK_Genre PRIMARY KEY (Genre_ID), -- 장르 기본키 제약조건
    CONSTRAINT CK_Genre_Content_Type CHECK (Content_Type IN ('MOVIE', 'TV')) -- 콘텐츠 타입 제한
)

/* =========================================================
   테이블: AppUser
   용도: 서비스 사용자 기본 정보와 연령 확인 정보를 저장한다.
   주요 사용 화면: UserManagement, WatchHistory, SubscriptionManagement
   ========================================================= */
CREATE TABLE AppUser
(
    User_ID int IDENTITY(1,1) not null,     --사용자를 구분하는 기본키
    USER_NAME NVARCHAR(10) NOT NULL,        -- 사용자 이름
    User_BirthDate date NOT NULL,           -- 사용자 생년월일
    User_PhoneNumber NVARCHAR(20) NOT NULL, -- 사용자 휴대전화번호
    User_Sex NCHAR(2) NOT NULL,             -- 사용자 성별
    CONSTRAINT PK_AppUser PRIMARY KEY (User_ID),    -- 사용자 기본키 제약조건
    CONSTRAINT CK_AppUser_User_Sex CHECK (User_Sex IN (N'남성', N'여성')),  --성별 입력값 제한
    CONSTRAINT UQ_APPUSER_User_Phone_BirthDate UNIQUE (User_PhoneNumber, User_BirthDate)    -- 휴대전화와 생년월일 조합 중복 방지
)

/* =========================================================
   테이블: OTTProvider
   용도: OTT 제공자 이름과 월 구독료를 저장한다.
   주요 사용 화면: OTTManagement, SubscriptionManagement, ContentMapping
   ========================================================= */
CREATE TABLE OTTProvider
(
    OTT_ID int IDENTITY(1,1) not null,      -- OTT 제공자를 구분하는 기본키
    OTT_NAME NVARCHAR(20) NOT NULL,         -- OTT 제공자 이름
    monthly_price int NOT NULL,             -- 월 구독료
    CONSTRAINT PK_OTTProvider PRIMARY KEY (OTT_ID), -- OTT 제공자 기본키 제약조건
    CONSTRAINT UQ_OTTProvider_OTT_NAME UNIQUE (OTT_NAME),   -- OTT 이름 중복 방지
    CONSTRAINT CK_OTTProvider_monthly_price CHECK (monthly_price >= 0)  -- 월 구독료는 0 이상
)

/* =========================================================
   테이블: ContentGenre
   용도: 콘텐츠와 장르의 다대다 연결 관계를 저장한다.
   주요 사용 화면: ContentMapping, Statistics
   ========================================================= */
CREATE TABLE ContentGenre
(
    Content_ID int not null, -- 콘텐츠 ID, Content 테이블 참조
    Genre_ID int not null,   -- 장르 ID, Genre 테이블 참조
    CONSTRAINT PK_ContentGenre PRIMARY KEY (Content_ID, Genre_ID),  -- 콘텐츠와 장르의 중복 연결 방지
    CONSTRAINT FK_ContentGenre_Content FOREIGN KEY (Content_ID) REFERENCES Content(Content_ID), -- Content 테이블 외래키
    CONSTRAINT FK_ContentGenre_Genre FOREIGN KEY (Genre_ID) REFERENCES Genre(Genre_ID)  -- Genre 테이블 외래키
)

/* =========================================================
   테이블: ContentOTTProvider
   용도: 콘텐츠와 OTT 제공자의 다대다 연결 관계를 저장한다.
   주요 사용 화면: ContentMapping, WatchHistory
   ========================================================= */
CREATE TABLE ContentOTTProvider
(
    Content_ID int not null,    -- 콘텐츠 ID, Content 테이블 참조
    OTT_ID int not null,        -- OTT ID , OTTProvider 테이블 참조
    CONSTRAINT PK_ContentOTTProvider PRIMARY KEY (Content_ID, OTT_ID),  --콘텐츠와 OTT의 중복 연결 방지
    CONSTRAINT FK_ContentOTTProvider_Content FOREIGN KEY (Content_ID) REFERENCES Content(Content_ID),   -- Content 테이블 외래키
    CONSTRAINT FK_ContentOTTProvider_OTTProvider FOREIGN KEY (OTT_ID) REFERENCES OTTProvider(OTT_ID)    -- OTTProvider 테이블 외래키
)

/* =========================================================
   테이블: Usersubscription
   용도: 사용자의 OTT 구독 기간과 결제 금액을 저장한다.
   주요 사용 화면: SubscriptionManagement, WatchHistory, Dashboard
   ========================================================= */
CREATE TABLE Usersubscription
(
    Subscription_ID int IDENTITY(1,1) not null,   -- 구독 정보를 구분하는 기본키
    User_ID int not null,                         -- 사용자 ID, Appuser 테이블 참조
    OTT_ID int not null,                          -- OTT ID, OTTProvider 테이블 참조
    started_at date not null,                     -- 구독 시작일
    expired_at date not null,                     -- 구독 만료일
    paid_amount int not null,                     -- 결제 금액
    CONSTRAINT PK_Usersubscription PRIMARY KEY (Subscription_ID),   -- 구독 기본키 제약조건
    CONSTRAINT FK_Usersubscription_AppUser FOREIGN KEY (User_ID) REFERENCES AppUser(User_ID),   -- AppUser 테이블 외래키
    CONSTRAINT FK_Usersubscription_OTTProvider FOREIGN KEY (OTT_ID) REFERENCES OTTProvider(OTT_ID), --OTTProvider 테이블 외래키
    CONSTRAINT CK_Usersubscription_paid_amount CHECK (paid_amount >= 0),    -- 결제 금액은 0원 이상
    CONSTRAINT CK_Usersubscription_expired_at CHECK (expired_at >= started_at) -- 만료일은 시작일 보다 같거나 이후여야함
)

/* =========================================================
   테이블: WatchHistory
   용도: 사용자의 콘텐츠 시청 시작·종료와 진행률을 저장한다.
   주요 사용 화면: WatchHistory, UserRating, Statistics
   ========================================================= */
CREATE TABLE WatchHistory
(
    Watch_ID int IDENTITY(1,1) not null, -- 시청 기록을 구분하는 기본키
    User_ID int not null,                -- 사용자 ID, AppUser 테이블 참조
    Content_ID int not null,             -- 콘텐츠 ID
    Ott_ID int NOT NULL,                 -- OTT ID
    Watch_Started_at datetime2 not null, -- 시청 시작 시간
    Watch_Ended_at datetime2 null,       -- 시청 종료 시간
    Watch_Minutes int null,              -- 시청 시간
    progress_percentage DECIMAL(5,2) null, -- 콘텐츠 시청 진행률
    is_completed bit not null default 0, -- 시청 완료 여부, 기본값은 미완료
    CONSTRAINT PK_WatchHistory PRIMARY KEY (Watch_ID), -- 시청 기록 기본키 제약조건
    CONSTRAINT FK_WatchHistory_AppUser FOREIGN KEY (User_ID) REFERENCES AppUser(User_ID), -- AppUser 테이블 외래키
    CONSTRAINT FK_WatchHistory_ContentOTTProvider FOREIGN KEY (Content_ID, Ott_ID) REFERENCES ContentOTTProvider(Content_ID, Ott_ID),
    -- 콘텐츠가 해당 OTT에서 제공되는지 확인하기 위한 외래키
    CONSTRAINT CK_WatchHistory_Watch_Ended_at CHECK (Watch_Ended_at > Watch_Started_at or Watch_Ended_at is null),  -- 종료 시간은 시작 시간보다 이후이거나 NULL
    CONSTRAINT CK_WatchHistory_Watch_Minutes CHECK (Watch_Minutes >= 0 or Watch_Minutes is null), -- 시청 시간은 0 이상 또는 NULL
    CONSTRAINT CK_WatchHistory_progress_percentage CHECK (progress_percentage >= 0 and progress_percentage <= 100 or progress_percentage is null)
    -- 진행률은 0~100 범위 또는 NULL
)

/* =========================================================
   테이블: UserRating
   용도: 사용자가 콘텐츠에 등록한 평점과 등록 시점을 저장한다.
   주요 사용 화면: UserRating, Statistics
   ========================================================= */
CREATE TABLE UserRating
(
    user_id int not null,       -- 사용자 ID, AppUser 테이블 참조
    content_id int not null,    -- 콘텐츠 ID, Content 테이블 참조
    rating_score DECIMAL(3,1) not null, -- 사용자가 등록한 평점
    rated_at datetime2 not null         -- 평점 등록 시간
        CONSTRAINT DF_UserRating_Rated_At DEFAULT SYSDATETIME() , -- 기본값은 현재 시스템 시간
    CONSTRAINT PK_UserRating PRIMARY KEY (user_id, content_id), -- 한 사용자가 같은 콘텐츠를 한 번만 평가하도록 복합 기본키 설정
    CONSTRAINT FK_UserRating_AppUser FOREIGN KEY (user_id) REFERENCES AppUser(User_ID), -- AppUser 테이블 외래키
    CONSTRAINT FK_UserRating_Content FOREIGN KEY (content_id) REFERENCES Content(Content_ID), -- Content 테이블 외래키
    CONSTRAINT CK_UserRating_rating_score CHECK (rating_score >= 0 AND rating_score <= 5) -- 평점은 0점 이상 5점 이하
)

/* =========================================================
   테이블: SystemSetting
   용도: 평점 가능 진행률 등 시스템 설정값을 저장한다.
   주요 사용 화면: SystemSetting, UserRating
   ========================================================= */
CREATE TABLE SystemSetting
(
    Setting_Key VARCHAR(50) NOT NULL, -- 설정 항목 이름
    Setting_Value NVARCHAR(200) NOT NULL, -- 설정값
    CONSTRAINT PK_SystemSetting PRIMARY KEY (Setting_Key) -- 설정 항목 이름을 기본키로 사용
)

GO
/* =========================================================
   3. 기본 데이터 또는 초기 설정 데이터
   ========================================================= */
-- 현재 파일에는 별도의 기본 데이터 또는 초기 설정 INSERT 구문이 없다.

/* =========================================================
   4. 인덱스 생성문
   ========================================================= */
-- ContentManagement 화면의 제목 검색을 지원한다.
-- WHERE Title LIKE ... 조건을 고려한 단일 컬럼 인덱스다.
CREATE NONCLUSTERED INDEX IX_Content_Title
ON dbo.Content(Title)

-- 콘텐츠 유형별 TMDB 평점 정렬·조회를 지원한다.
-- Content_Type로 범위를 좁힌 뒤 TMDB_Average_Rating을 사용하는 순서다.
CREATE NONCLUSTERED INDEX IX_Content_Type_TMDBRating
ON dbo.Content(Content_Type, TMDB_Average_Rating)

-- 콘텐츠 유형별 최신 콘텐츠 조회를 지원한다.
-- Content_Type 조건 후 Release_Date 정렬·범위 조회를 고려한 순서다.
CREATE NONCLUSTERED INDEX IX_Content_Type_ReleaseDate
ON dbo.Content(Content_Type, Release_Date)

-- ContentMapping과 Statistics의 장르별 콘텐츠 조회를 지원한다.
-- Genre_ID로 찾은 뒤 Content_ID로 조인하기 위한 역방향 복합 인덱스다.
CREATE NONCLUSTERED INDEX IX_ContentGenre_GenreID_ContentID
ON dbo.ContentGenre(Genre_ID, Content_ID)

-- ContentMapping의 OTT별 콘텐츠 조회를 지원한다.
-- OTT_ID로 찾은 뒤 Content_ID로 조인하기 위한 역방향 복합 인덱스다.
CREATE NONCLUSTERED INDEX IX_ContentOTTProvider_OTTID_ContentID
ON dbo.ContentOTTProvider(OTT_ID, Content_ID)

-- 구독 등록과 시청 시작 시 활성 구독 검사를 지원한다.
-- User_ID·OTT_ID로 대상을 좁힌 뒤 started_at·expired_at 기간을 검사하는 순서다.
CREATE NONCLUSTERED INDEX IX_Usersubscription_UserID_OTTID_Date
ON dbo.Usersubscription(User_ID,OTT_ID, started_at, expired_at)

-- UserRating의 평점 가능 시청 기록 조회를 지원한다.
-- User_ID·Content_ID로 좁힌 뒤 progress_percentage가 높은 기록을 찾는 순서다.
CREATE NONCLUSTERED INDEX IX_WatchHistory_UserID_ContentID_Progress
ON dbo.WatchHistory(User_ID,Content_ID, progress_percentage DESC)

GO
/* =========================================================
   5. 구독 / 시청 / 평점 관련 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_AddUserSubscription
   용도:
   - SubscriptionManagement에서 사용자 OTT 구독을 등록한다.

   입력:
   - @user_id 사용자, @ott_id OTT, @started_at·@expired_at 구독 기간, @paid_amount 결제 금액

   반환:
   - Subscription_ID와 구독 입력값, Message

   주요 로직:
   - 사용자·OTT·날짜·금액의 유효성을 검사한다.
   - 잠금을 사용해 겹치는 활성 구독을 확인한 뒤 구독을 추가한다.

   주의:
   - 종료일은 시작일보다 늦어야 하며 동일 사용자·OTT의 기간 중복을 허용하지 않는다.
   ========================================================= */
create or ALTER proc dbo.Usp_AddUserSubscription
    @user_id int,
    @ott_id int,
    @started_at date,
    @expired_at date,
    @paid_amount int
as
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    -- 트랜잭션 중 오류 발생 시 자동으로 롤백

    BEGIN TRY
        BEGIN TRANSACTION;

    IF @user_id IS NULL
        BEGIN
            THROW 50000, N'회원 ID는 필수입니다.', 1;
        END;

    IF @ott_id IS NULL
        BEGIN
            THROW 50000, N'OTT ID는 필수입니다.', 1;
        END;

    IF @started_at IS NULL OR @expired_at IS NULL
    BEGIN
    THROW 50000, N'구독 시작일과 만료일은 필수입니다.', 1;
END;

IF @paid_amount IS NULL
    BEGIN
THROW 50000, N'결제 금액은 필수입니다.', 1;
END;
-- 회원 존재 확인
if not exists (
        select 1
from dbo.AppUser
where User_ID = @user_id
    )
    BEGIN
THROW 50001, N'사용자가 존재하지 않습니다.', 1;
END;

-- OTT 제공자 존재 확인
if not exists (
        select 1
from dbo.OTTProvider
where OTT_ID = @ott_id
    )
    BEGIN
THROW 50002, N'OTT 제공자가 존재하지 않습니다.', 1;
END;

-- 날짜 유효성
if @expired_at <= @started_at
    BEGIN
THROW 50003, N'만료 날짜는 시작 날짜보다 이후여야 합니다.', 1;
END;

-- 결제 금액 검사
if @paid_amount < 0
    BEGIN
THROW 50004, N'결제 금액은 음수일 수 없습니다.', 1;
END;

-- 활성 구독 중복 검사
IF EXISTS (
        SELECT 1
FROM dbo.Usersubscription with (UPDLOCK,HOLDLOCK)
-- UPDLOCK 읽을 때 부터 수정 예정 잠금 사용 , HOLDLOCK 그 잠금을 트랜잭션 끝까지 유지
WHERE User_ID = @user_id
    AND OTT_ID = @ott_id
    and @started_at <= expired_at -- 시작 날짜가 기존 구독의 만료 날짜보다 이전인 경우
    and @expired_at >= started_at -- 만료 날짜가 기존 구독의 시작 날짜보다 이후인 경우
    )
    BEGIN
THROW 50005, N'이미 해당 기간 내 같은 OTT 구독이 존재합니다.', 1;
END;

-- 구독 정보 삽입
INSERT Into dbo.Usersubscription
    (
    User_ID,
    OTT_ID,
    started_at,
    expired_at,
    paid_amount
    )
VALUES
    (
        @user_id,
        @ott_id,
        @started_at,
        @expired_at,
        @paid_amount
    );

-- 새로 생성된 구독 ID 반환
SELECT
    Cast(SCOPE_IDENTITY() AS int) AS New_Subscription_ID,
    N'구독 등록이 완료되었습니다.' as Message;

COMMIT TRANSACTION;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
end;
GO

/* =========================================================
   프로시저: dbo.Usp_StartWatchHistory
   용도:
   - WatchHistory에서 콘텐츠 시청 시작 기록을 생성한다.

   입력:
   - @user_id 사용자, @content_id 콘텐츠, @ott_id OTT, @watch_started_at 시작 시각

   반환:
   - Watch_ID와 사용자·콘텐츠·OTT·시작 시각, Message

   주요 로직:
   - 사용자 연령과 콘텐츠 관람 등급을 비교한다.
   - 콘텐츠-OTT 매핑과 시작일 기준 활성 구독을 확인한 뒤 시청 기록을 추가한다.

   주의:
   - 연령 제한, OTT 제공 매핑, 구독 기간 조건을 모두 충족해야 한다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_StartWatchHistory
    @user_id int,
    @content_id int,
    @ott_id int,
    @watch_started_at datetime2
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    -- 트랜잭션 중 오류 발생 시 자동으로 롤백
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @UserBirthDate DATE;
        DECLARE @AgeRating NVARCHAR(50);
        DECLARE @MinimumAge INT = 0;
    -- 필수값 검사
    IF @user_id IS NULL
        BEGIN
            THROW 50000, N'회원 ID는 필수입니다.', 1;
        END;
    IF @content_id IS NULL
        BEGIN
            THROW 50001, N'콘텐츠 ID는 필수입니다.', 1;
        END;
    IF @ott_id IS NULL
    BEGIN
    THROW 50002, N'OTT ID는 필수입니다.', 1;
END;

IF @watch_started_at IS NULL
            BEGIN
THROW 50003, N'시청 시작 시간은 필수입니다.', 1;
END;
-- 사용자 존재 확인
if not exists (
            select 1
from dbo.AppUser
where User_ID = @user_id
        )
        BEGIN
THROW 50004, N'사용자가 존재하지 않습니다.', 1;
END;

-- 연령 제한 검사
SELECT
    @UserBirthDate = User_BirthDate
FROM dbo.AppUser
WHERE User_ID = @user_id;

SELECT
    @AgeRating = Age_Rating
FROM dbo.Content
WHERE Content_ID = @content_id;

SET @AgeRating = UPPER(LTRIM(RTRIM(ISNULL(@AgeRating, N''))));

SET @MinimumAge =
    CASE
        WHEN @AgeRating LIKE N'%청소년%' THEN 19
        WHEN @AgeRating LIKE N'%19%' THEN 19
        WHEN @AgeRating LIKE N'%18%' THEN 18
        WHEN @AgeRating LIKE N'%15%' THEN 15
        WHEN @AgeRating LIKE N'%12%' THEN 12
        WHEN @AgeRating LIKE N'%ALL%' THEN 0
        WHEN @AgeRating LIKE N'%전체%' THEN 0
        ELSE 0
    END;

IF @MinimumAge > 0 AND @UserBirthDate IS NULL
BEGIN
    THROW 50007, N'사용자의 생년월일 정보가 없어 연령 제한 콘텐츠를 시청할 수 없습니다.', 1;
END;

IF @MinimumAge > 0
   AND DATEADD(YEAR, @MinimumAge, @UserBirthDate) > CAST(@watch_started_at AS DATE)
BEGIN
    THROW 50008, N'사용자의 나이가 콘텐츠 관람 등급을 만족하지 않습니다.', 1;
END;

-- 콘텐츠 + OTT 조합이 ContentOTTProvider 테이블에 존재하는지 확인
if not exists (
            select 1
from dbo.ContentOTTProvider
where Content_ID = @content_id
    and Ott_ID = @ott_id
        )
        BEGIN
THROW 50005, N'해당 콘텐츠는 지정된 OTT에서 제공되지 않습니다.', 1;
END;

-- 시청 시작일 기준 회원이 해당 OTT를 구독 중인지 확인
if not exists (
            select 1
from dbo.Usersubscription with (UPDLOCK,HOLDLOCK)
where User_ID = @user_id
    and OTT_ID = @ott_id
    and CAST(@watch_started_at AS DATE) BETWEEN started_at AND expired_at
        )
        BEGIN
THROW 50006, N'해당 시간 동안 회원은 해당 OTT를 구독 중이 아닙니다.', 1;
END;

-- 시청 기록 삽입
INSERT INTO dbo.WatchHistory
    (
    User_ID,
    Content_ID,
    OTT_ID,
    Watch_Started_At
    )
VALUES
    (
        @user_id,
        @content_id,
        @ott_id,
        @watch_started_at
    );

SELECT Cast(SCOPE_IDENTITY() AS int) AS New_Watch_ID,
    N'시청 기록이 시작되었습니다.' as Message;

COMMIT TRANSACTION;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_EndWatchHistory
   용도:
   - WatchHistory에서 진행 중인 시청 기록을 종료한다.

   입력:
   - @Watch_ID 시청 기록, @Watch_Minutes 시청 분량

   반환:
   - Watch_ID, Watch_Minutes, Running_Time, Progress_Percentage, Is_Completed, Watch_Ended_at, Message

   주요 로직:
   - 종료되지 않은 시청 기록과 콘텐츠 러닝 타임을 잠금 상태로 조회한다.
   - 시청 진행률과 완료 여부를 계산해 종료 시각과 함께 갱신한다.

   주의:
   - 이미 종료된 기록은 처리하지 않으며 진행률 계산은 Running_Time에 의존한다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_EndWatchHistory
    @Watch_ID int,
    @Watch_Minutes int
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    -- 트랜잭션 중 오류 발생 시 자동으로 롤백
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Watch_Ended_at datetime2 = SYSDATETIME(); -- 시청 종료 시간은 현재 시간으로 설정
        DECLARE @Watch_Started_at datetime2;
        DECLARE @Running_Time int;
        DECLARE @Progress_Percentage DECIMAL(5,2);
        DECLARE @Is_Completed bit;
    -- 필수값 검사
    IF @Watch_ID IS NULL
    BEGIN
        THROW 50000, N'시청 기록 ID는 필수입니다.', 1;
    END;

    IF @Watch_Minutes IS NULL
    BEGIN
        THROW 50001, N'시청 분은 필수입니다.', 1;
    END;

    -- 시청 시간 유효성 검사
    if @Watch_Minutes < 0
    BEGIN
    THROW 50002, N'시청 분은 음수일 수 없습니다.', 1;
END;

-- 시청 기록 + 콘텐츠 러닝 타임 조회
SELECT
    @Watch_Started_at = WH.Watch_Started_at,
    @Running_Time = C.Running_Time
FROM dbo.WatchHistory WH WITH (UPDLOCK, HOLDLOCK) -- 레코드 잠금으로 동시성 제어
    JOIN dbo.Content C ON WH.Content_ID = C.Content_ID
WHERE WH.Watch_ID = @Watch_ID;

-- 시청 기록 존재 여부 확인
IF @Watch_Started_at IS NULL
    BEGIN
THROW 50003, N'해당 시청 기록이 존재하지 않습니다.', 1;
END;

-- 이미 종료된 기록인지 확인
IF EXISTS (
        SELECT 1
FROM dbo.WatchHistory
WHERE Watch_ID = @Watch_ID
    AND Watch_Ended_at IS NOT NULL
    )
    BEGIN
THROW 50004, N'이미 종료된 시청 기록입니다.', 1;
END;

-- 러닝 타임 확인
if @Running_Time IS NULL or @Running_Time <= 0
    BEGIN
THROW 50005, N'콘텐츠의 러닝 타임 정보가 유효하지 않습니다.', 1;
END;

if @Watch_Ended_at <= @Watch_Started_at
    BEGIN
THROW 50006, N'시청 종료 시간은 시작 시간보다 이후여야 합니다.', 1;
END;

-- 시청률 계산
Set @Progress_Percentage =
        CASE
            When @Watch_Minutes >= @Running_Time Then 100
            Else CAST(@Watch_Minutes *100.0 / @Running_Time AS DECIMAL(5,2))
        END;

-- 완료 여부 계산
set @Is_Completed =
        CASE
            When @Watch_Minutes >= @Running_Time Then 1
            Else 0
        END;

-- 시청 기록 종료 처리
UPDATE dbo.WatchHistory
    SET
        Watch_Ended_at = @Watch_Ended_at,
        Watch_Minutes = @Watch_Minutes,
        progress_percentage = @Progress_Percentage,
        is_completed = @Is_Completed
    WHERE Watch_ID = @Watch_ID;

SELECT
    @Watch_id as Watch_ID,
    @Watch_Minutes AS Watch_Minutes,
    @Running_Time as Running_Time,
    @Progress_Percentage AS Progress_Percentage,
    @Is_Completed AS Is_Completed,
    @Watch_Ended_at AS Watch_Ended_at,
    N'시청 기록이 종료되었습니다.' AS Message;

COMMIT TRANSACTION;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_AddOrUpdateUserRating
   용도:
   - UserRating에서 시청 조건을 충족한 콘텐츠의 평점을 등록하거나 수정한다.

   입력:
   - @user_id 사용자, @content_id 콘텐츠, @rating_score 평점, @rated_at 평가일

   반환:
   - user_id, content_id, rating_score, rated_at, min_rating_progress, Message

   주요 로직:
   - 시스템 설정의 최소 진행률과 완료된 시청 기록을 기준으로 평점 가능 여부를 판단한다.
   - 기존 평점이 있으면 수정하고 없으면 새 평점을 추가한다.

   주의:
   - 평가일·점수 범위·0.5 단위 조건과 최소 시청 진행률을 모두 검사한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_AddOrUpdateUserRating
@user_id INT,
@content_id INT,
@rating_score DECIMAL(3,1),
@rated_at DATE
AS
BEGIN
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Existing_Rating_Score DECIMAL(3,1);
    DECLARE @Min_Rating_Progress DECIMAL(5,2);
    DECLARE @Matched_Watch_ID INT;

    SELECT
        @Min_Rating_Progress = TRY_CONVERT(DECIMAL(5,2), Setting_Value)
    FROM dbo.SystemSetting
    WHERE Setting_Key = 'MIN_RATING_PROGRESS';

    IF @Min_Rating_Progress IS NULL
    BEGIN
        THROW 50000, N'평점 등록을 위한 최소 시청률 설정이 없거나 숫자로 변환할 수 없습니다.', 1;
    END;

    IF @Min_Rating_Progress < 0 OR @Min_Rating_Progress > 100
    BEGIN
        THROW 50001, N'평점 등록을 위한 최소 시청률 설정이 0에서 100 사이여야 합니다.', 1;
    END;

    IF @user_id IS NULL
    BEGIN
        THROW 50002, N'회원 ID는 필수입니다.', 1;
    END;

    IF @content_id IS NULL
    BEGIN
        THROW 50003, N'콘텐츠 ID는 필수입니다.', 1;
    END;

    IF @rating_score IS NULL
    BEGIN
        THROW 50004, N'평점 점수는 필수입니다.', 1;
    END;

    IF @rated_at IS NULL
    BEGIN
        THROW 50005, N'평가일은 필수입니다.', 1;
    END;

    IF @rated_at > CAST(SYSDATETIME() AS DATE)
    BEGIN
        THROW 50006, N'평가일은 오늘 이후 날짜로 등록할 수 없습니다.', 1;
    END;

    IF @rating_score < 0 OR @rating_score > 5
    BEGIN
        THROW 50007, N'평점 점수는 0에서 5 사이여야 합니다.', 1;
    END;

    IF (@rating_score * 10) % 5 <> 0
    BEGIN
        THROW 50008, N'평점은 0.5점 단위로만 등록할 수 있습니다.', 1;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.AppUser
        WHERE User_ID = @user_id
    )
    BEGIN
        THROW 50009, N'사용자가 존재하지 않습니다.', 1;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Content
        WHERE Content_ID = @content_id
    )
    BEGIN
        THROW 50010, N'콘텐츠가 존재하지 않습니다.', 1;
    END;

    -- 완료된 시청 중 진행률과 종료 시각이 가장 높은 기록으로 평점 가능 여부를 판단한다.
    SELECT TOP 1
        @Matched_Watch_ID = WH.Watch_ID
    FROM dbo.WatchHistory WH
    WHERE WH.User_ID = @user_id
      AND WH.Content_ID = @content_id
      AND WH.Watch_Ended_At IS NOT NULL
      AND WH.Progress_Percentage >= @Min_Rating_Progress
      AND CAST(@rated_at AS DATE) >= CAST(WH.Watch_Ended_At AS DATE)
    ORDER BY
        WH.Progress_Percentage DESC,
        WH.Watch_Ended_At DESC,
        WH.Watch_ID DESC;

    IF @Matched_Watch_ID IS NULL
    BEGIN
        THROW 50011, N'평가 가능한 시청 기록이 없습니다. 최소 시청률 또는 평가일을 확인하세요. 평가일은 시청 종료일 이후여야 합니다.', 1;
    END;

    SELECT
        @Existing_Rating_Score = Rating_Score
    FROM dbo.UserRating WITH (UPDLOCK, HOLDLOCK)
    WHERE User_ID = @user_id
      AND Content_ID = @content_id;

    IF @Existing_Rating_Score IS NOT NULL
    BEGIN
        UPDATE dbo.UserRating
        SET
            Rating_Score = @rating_score,
            Rated_At = @rated_at
        WHERE User_ID = @user_id
          AND Content_ID = @content_id;

        SELECT
            @user_id AS user_id,
            @content_id AS content_id,
            @rating_score AS rating_score,
            @rated_at AS rated_at,
            @Min_Rating_Progress AS min_rating_progress,
            N'평점이 업데이트되었습니다.' AS Message;
    END;
    ELSE
    BEGIN
        INSERT INTO dbo.UserRating
        (
            User_ID,
            Content_ID,
            Rating_Score,
            Rated_At
        )
        VALUES
        (
            @user_id,
            @content_id,
            @rating_score,
            @rated_at
        );

        SELECT
            @user_id AS user_id,
            @content_id AS content_id,
            @rating_score AS rating_score,
            @rated_at AS rated_at,
            @Min_Rating_Progress AS min_rating_progress,
            N'평점이 등록되었습니다.' AS Message;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

END;
GO

/* =========================================================
   프로시저: dbo.Usp_GetRateableContentsForRating
   용도:
   - UserRating 화면에 사용자가 평가할 수 있는 콘텐츠 목록을 제공한다.

   입력:
   - @User_ID 조회 사용자

   반환:
   - Content_ID, Title, Content_Type, Progress_Percentage, Watch_Started_At, Watch_Ended_At

   주요 로직:
   - 최소 진행률 이상이며 종료된 시청 기록 중 미평가 콘텐츠를 찾는다.
   - ROW_NUMBER로 콘텐츠별 대표 시청 기록 한 건만 반환한다.

   주의:
   - SystemSetting 값이 없으면 최소 진행률 50을 사용한다.
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.Usp_GetRateableContentsForRating
@User_ID INT
AS
BEGIN
SET NOCOUNT ON;

DECLARE @MinRatingProgress DECIMAL(5,2);

SELECT
    @MinRatingProgress = TRY_CONVERT(DECIMAL(5,2), Setting_Value)
FROM dbo.SystemSetting
WHERE Setting_Key = 'MIN_RATING_PROGRESS';

IF @MinRatingProgress IS NULL
BEGIN
    SET @MinRatingProgress = 50;
END;

-- 콘텐츠별 여러 시청 기록 중 평점 기준에 가장 적합한 한 건을 선택한다.
;WITH RateableWatch AS
(
    SELECT
        WH.Watch_ID,
        C.Content_ID,
        C.Title,
        C.Content_Type,
        WH.Progress_Percentage,
        WH.Watch_Started_At,
        WH.Watch_Ended_At,
        ROW_NUMBER() OVER (
            PARTITION BY C.Content_ID
            ORDER BY
                WH.Progress_Percentage DESC,
                WH.Watch_Ended_At DESC,
                WH.Watch_ID DESC
        ) AS RowNumber
    FROM dbo.WatchHistory WH
    INNER JOIN dbo.Content C
        ON WH.Content_ID = C.Content_ID
    WHERE WH.User_ID = @User_ID
      AND WH.Watch_Ended_At IS NOT NULL
      AND WH.Progress_Percentage >= @MinRatingProgress
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.UserRating UR
          WHERE UR.User_ID = WH.User_ID
            AND UR.Content_ID = WH.Content_ID
      )
)
SELECT
    Content_ID,
    Title,
    Content_Type,
    Progress_Percentage,
    Watch_Started_At,
    Watch_Ended_At
FROM RateableWatch
WHERE RowNumber = 1
ORDER BY Title;

END;
GO

/* =========================================================
   프로시저: dbo.Usp_GetPendingUserRatingCount
   용도:
   - 평가 대기 중인 사용자-콘텐츠 조합 수를 조회한다.

   입력:
   - 입력 파라미터 없음

   반환:
   - PendingRatingCount

   주요 로직:
   - 최소 진행률 이상으로 종료된 시청 기록에서 미평가 조합을 찾는다.
   - 사용자와 콘텐츠로 그룹화한 뒤 대기 건수를 집계한다.

   주의:
   - SystemSetting 값이 없으면 최소 진행률 50을 사용한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetPendingUserRatingCount
AS
BEGIN
SET NOCOUNT ON;

DECLARE @Min_Rating_Progress DECIMAL(5,2);

SELECT
    @Min_Rating_Progress = TRY_CONVERT(DECIMAL(5,2), Setting_Value)
FROM dbo.SystemSetting
WHERE Setting_Key = 'MIN_RATING_PROGRESS';

IF @Min_Rating_Progress IS NULL
BEGIN
    SET @Min_Rating_Progress = 50;
END;

-- 동일 사용자-콘텐츠의 여러 시청 기록을 한 건으로 묶어 평가 대기 수를 계산한다.
SELECT
    COUNT(*) AS PendingRatingCount
FROM
(
    SELECT
        WH.User_ID,
        WH.Content_ID
    FROM dbo.WatchHistory WH
    WHERE WH.Watch_Ended_At IS NOT NULL
      AND WH.Progress_Percentage >= @Min_Rating_Progress
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.UserRating UR
          WHERE UR.User_ID = WH.User_ID
            AND UR.Content_ID = WH.Content_ID
      )
    GROUP BY
        WH.User_ID,
        WH.Content_ID
) PendingRatings;

END;
GO

/* =========================================================
   6. 콘텐츠 / 장르 / OTT 관리 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_AddContent
   용도:
   - ContentManagement에서 콘텐츠 기본 정보를 등록한다.

   입력:
   - @TMDB_ID, @Content_Type, @Title 및 콘텐츠 상세 정보

   반환:
   - New_Content_ID와 등록된 콘텐츠 입력값, Message

   주요 로직:
   - 필수값과 콘텐츠 유형, 평점, 러닝 타임의 유효성을 검사한다.
   - TMDB_ID와 콘텐츠 유형 조합의 중복을 확인한 뒤 콘텐츠를 추가한다.

   주의:
   - TMDB_ID가 있는 경우 동일 유형 내 중복 등록을 허용하지 않는다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_AddContent
    @TMDB_ID int = NULL,
    @Content_Type varchar(10),
    @Title NVARCHAR(50),
    @Release_Date DATE =NULL,
    @TMDB_Average_Rating DECIMAL(3,1) = NULL,
    @OverView NVARCHAR(Max) = NULL,
    @Poster_URL NVARCHAR(500) = NULL,
    @Running_Time int =NULL,
    @Age_Rating nVarchar(5) = NULL
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    -- 트랜잭션 중 오류 발생 시 자동으로 롤백
    BEGIN TRY
        BEGIN TRANSACTION;
    DECLARE @New_Content_ID int;

    set @Content_Type = UPPER(LTRIM(RTRIM(@Content_Type)))
    set @Title = LTRIM(RTRIM(@Title))

    -- 기본값 검사
    if @Content_Type is NULL or @Content_Type  = ''
    BEGIN
        THROW 50000, N'콘텐츠 타입은 필수입니다.', 1;
    END
    if @Content_Type NOT IN ('MOVIE', 'TV')
    BEGIN
        THROW 50001, N'콘텐츠 타입은 MOVIE 또는 TV여야 합니다.', 1;
    END
    if @Title is NULL or @Title = N''
    BEGIN
    THROW 50002, N'콘텐츠 제목은 필수입니다.', 1;
END

if @TMDB_Average_Rating IS NOT NULL AND (@TMDB_Average_Rating < 0 OR @TMDB_Average_Rating > 10)
        BEGIN
THROW 50003, N'TMDB 평점은 0에서 10 사이여야 합니다.', 1;
END;

if @Running_Time IS NOT NULL AND @Running_Time <= 0
        BEGIN
THROW 50004, N'러닝 타임은 NULL 이 아니면서 0 보다 커야 합니다',1;
END;

if @TMDB_ID is NOT NULL
    BEGIN
    if EXISTS(
            SELECT 1
    FROM dbo.Content with(UPDLOCK, HOLDLOCK)
    WHERE TMDB_ID = @TMDB_ID
        and Content_Type = @Content_Type
        )
    BEGIN
    THROW 50005, N'이미 동일한 TMDB_ID와 콘텐츠 타입을 가진 콘텐츠가 존재합니다.', 1;
END;
END;

INSERT INTO dbo.Content
    (
    TMDB_ID,
    Content_Type,
    Title,
    Release_Date,
    TMDB_Average_Rating,
    OverView,
    Poster_URL,
    Running_Time,
    Age_Rating
    )
VALUES
    (
        @TMDB_ID,
        @Content_Type,
        @Title,
        @Release_Date,
        @TMDB_Average_Rating,
        @OverView,
        @Poster_URL,
        @Running_Time,
        @Age_Rating
    );
set @New_Content_ID = CAST(SCOPE_IDENTITY() as int);
COMMIT TRANSACTION;

SELECT
    @New_Content_ID AS New_Content_ID,
    @TMDB_ID AS TMDB_ID,
    @Content_Type AS Content_Type,
    @Title AS Title,
    @Release_Date AS Release_Date,
    @TMDB_Average_Rating AS TMDB_Average_Rating,
    @OverView AS OverView,
    @Poster_URL AS Poster_URL,
    @Running_Time AS Running_Time,
    @Age_Rating AS Age_Rating,
    N'콘텐츠가 등록되었습니다.' as MESSAGE;
END TRY
    BEGIN CATCH
if @@TRANCOUNT > 0
    BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_UpdateContent
   용도:
   - ContentManagement에서 기존 콘텐츠 정보를 수정한다.

   입력:
   - @Content_ID와 제목·개봉일·개요·포스터·러닝 타임

   반환:
   - Content_ID와 수정된 콘텐츠 값, Message

   주요 로직:
   - 콘텐츠 ID와 제목, 러닝 타임을 검증하고 수정 대상을 잠금 조회한다.
   - 대상 콘텐츠의 관리 가능 필드를 입력값으로 갱신한다.

   주의:
   - 존재하지 않는 콘텐츠는 수정하지 않는다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_UpdateContent
    @Content_ID INT,
    @Title NVARCHAR(50),
    @Release_Date DATE = NULL,
    @OverView NVARCHAR(MAX) = NULL,
    @Poster_URL NVARCHAR(500) = NULL,
    @Running_Time INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @Title = LTRIM(RTRIM(@Title));

        IF @Content_ID IS NULL
        BEGIN
            THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
    FROM dbo.Content WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_ID = @Content_ID
        )
        BEGIN
            THROW 50001, N'콘텐츠가 존재하지 않습니다.', 1;
        END;

    IF @Title IS NULL OR @Title = N''
        BEGIN
    THROW 50002, N'콘텐츠 제목은 필수입니다.', 1;
END;

IF @Running_Time IS NOT NULL AND @Running_Time <= 0
        BEGIN
THROW 50003, N'러닝 타임은 0보다 커야 합니다.', 1;
END;

UPDATE dbo.Content
        SET
            Title = @Title,
            Release_Date = @Release_Date,
            OverView = @OverView,
            Poster_URL = @Poster_URL,
            Running_Time = @Running_Time
        WHERE Content_ID = @Content_ID;

COMMIT TRANSACTION;

SELECT
    @Content_ID AS Content_ID,
    @Title AS Title,
    @Release_Date AS Release_Date,
    @OverView AS OverView,
    @Poster_URL AS Poster_URL,
    @Running_Time AS Running_Time,
    N'콘텐츠 정보가 수정되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;

THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_DeleteContentWithRelation
   용도:
   - ContentManagement에서 콘텐츠와 관련 데이터를 함께 삭제한다.

   입력:
   - @Content_ID 삭제할 콘텐츠

   반환:
   - Content_ID와 각 테이블 삭제 건수, Message

   주요 로직:
   - 평점과 시청 기록을 먼저 삭제한다.
   - 장르·OTT 연결 관계를 삭제한 뒤 콘텐츠를 삭제한다.

   주의:
   - 여러 관계 테이블의 데이터가 함께 삭제되므로 영향 범위를 확인해야 한다.
   ========================================================= */
CREATE OR ALTER proc dbo.Usp_DeleteContentWithRelation
    @Content_ID int
AS
BEGIN
    set NOCOUNT on;
    set XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
    DECLARE @Delete_Rating_Count int = 0;
    DECLARE @Delete_WatchHistory_Count int = 0;
    DECLARE @Delete_Content_Count int = 0;
    DECLARE @Delete_ContentGenre_Count int = 0;
    DECLARE @Delete_ContentOTTProvider_Count int = 0;

    -- 기본값 검사
    if @Content_ID IS NULL
    BEGIN
        THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
    END;

    if not exists (
        select 1
    from dbo.Content WITH (UPDLOCK, HOLDLOCK)
    where Content_ID = @Content_ID
    )
    BEGIN
        THROW 50001, N'콘텐츠가 존재하지 않습니다.', 1;
    END;
    -- 평점 삭제
    DELETE FROM dbo.UserRating
        WHERE content_id = @Content_ID;

    Set @Delete_Rating_Count = @@ROWCOUNT;

    -- 시청 기록 삭제
    DELETE FROM dbo.WatchHistory
        WHERE Content_ID = @Content_ID;

    Set @Delete_WatchHistory_Count = @@ROWCOUNT;

    -- 콘텐츠 장르 연결 삭제
    DELETE FROM dbo.ContentGenre
        WHERE Content_ID = @Content_ID;

    SET @Delete_ContentGenre_Count = @@ROWCOUNT;

    -- 콘텐츠-OTT 연결 삭제
    DELETE FROM dbo.ContentOTTProvider
        WHERE Content_ID = @Content_ID;

    SET @Delete_ContentOTTProvider_Count = @@ROWCOUNT;

    -- 콘텐츠
    DELETE FROM dbo.Content
        WHERE Content_ID = @Content_ID;

    SET @Delete_Content_Count = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @Content_ID AS Content_ID,
        @Delete_Rating_Count AS Delete_Rating_Count,
        @Delete_WatchHistory_Count AS Delete_WatchHistory_Count,
        @Delete_Content_Count AS Delete_Content_Count,
        @Delete_ContentGenre_Count AS Delete_ContentGenre_Count,
        @Delete_ContentOTTProvider_Count AS Delete_ContentOTTProvider_Count,
        N'콘텐츠 및 연결된 기록이 삭제되었습니다.' AS Message;
    END TRY
    BEGIN CATCH
    IF @@TRANCOUNT > 0
            BEGIN
        ROLLBACK TRANSACTION;
    END;
    THROW;
    END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_AddOrUpdateGenre
   용도:
   - 장르 관리에서 장르를 추가하거나 수정한다.

   입력:
   - @Genre_ID, @TMDB_Genre_ID, @Content_Type, @Genre_Name

   반환:
   - Genre_ID와 장르 정보, Message

   주요 로직:
   - Genre_ID 유무에 따라 추가와 수정 흐름을 나눈다.
   - 이름·TMDB ID 중복과 연결 콘텐츠 유형의 일치 여부를 검사한다.

   주의:
   - 연결 콘텐츠가 있으면 장르 Content_Type 변경이 제한될 수 있다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_AddOrUpdateGenre
    @Genre_ID INT = NULL,
    @TMDB_Genre_ID INT = NULL,
    @Content_Type VARCHAR(10),
    @Genre_Name NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @Content_Type = UPPER(LTRIM(RTRIM(@Content_Type)));
        SET @Genre_Name = LTRIM(RTRIM(@Genre_Name));

        -- 필수값 검사
        IF @Content_Type IS NULL OR @Content_Type = ''
        BEGIN
            THROW 50000, N'콘텐츠 타입은 필수입니다.', 1;
        END;

        IF @Content_Type NOT IN ('MOVIE', 'TV')
        BEGIN
            THROW 50001, N'콘텐츠 타입은 MOVIE 또는 TV여야 합니다.', 1;
        END;

    IF @Genre_Name IS NULL OR @Genre_Name = N''
        BEGIN
    THROW 50002, N'장르 이름은 필수입니다.', 1;
END;

IF @TMDB_Genre_ID IS NOT NULL AND @TMDB_Genre_ID <= 0
        BEGIN
THROW 50003, N'TMDB 장르 ID는 NULL이 아니면 0보다 커야 합니다.', 1;
END;

-- 추가
IF @Genre_ID IS NULL OR @Genre_ID <= 0
        BEGIN
    -- 같은 타입 안에서 같은 장르명 중복 방지
    IF EXISTS (
                SELECT 1
    FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_Type = @Content_Type
        AND Genre_Name = @Genre_Name
            )
            BEGIN
    THROW 50004, N'이미 동일한 콘텐츠 타입의 장르 이름이 존재합니다.', 1;
END;

-- 같은 타입 안에서 같은 TMDB_Genre_ID 중복 방지
IF @TMDB_Genre_ID IS NOT NULL
    AND EXISTS (
                    SELECT 1
    FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_Type = @Content_Type
        AND TMDB_Genre_ID = @TMDB_Genre_ID
               )
            BEGIN
THROW 50005, N'이미 동일한 콘텐츠 타입의 TMDB 장르 ID가 존재합니다.', 1;
END;

INSERT INTO dbo.Genre
    (
    TMDB_Genre_ID,
    Content_Type,
    Genre_Name
    )
VALUES
    (
        @TMDB_Genre_ID,
        @Content_Type,
        @Genre_Name
            );

SET @Genre_ID = CAST(SCOPE_IDENTITY() AS INT);

COMMIT TRANSACTION;

SELECT
    @Genre_ID AS Genre_ID,
    @TMDB_Genre_ID AS TMDB_Genre_ID,
    @Content_Type AS Content_Type,
    @Genre_Name AS Genre_Name,
    N'장르가 추가되었습니다.' AS Message;

RETURN;
END;

-- 수정 대상 존재 확인
IF NOT EXISTS (
            SELECT 1
FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
WHERE Genre_ID = @Genre_ID
        )
        BEGIN
THROW 50006, N'수정할 장르가 존재하지 않습니다.', 1;
END;

-- 이미 연결된 콘텐츠가 있을 경우, 변경하려는 Content_Type과 연결 콘텐츠 타입이 맞는지 검사
IF EXISTS (
            SELECT 1
FROM dbo.ContentGenre CG
    INNER JOIN dbo.Content C
    ON CG.Content_ID = C.Content_ID
WHERE CG.Genre_ID = @Genre_ID
    AND C.Content_Type <> @Content_Type
        )
        BEGIN
THROW 50007, N'이미 연결된 콘텐츠의 타입과 변경하려는 장르 타입이 일치하지 않습니다.', 1;
END;

-- 다른 장르와 이름 중복 방지
IF EXISTS (
            SELECT 1
FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
WHERE Content_Type = @Content_Type
    AND Genre_Name = @Genre_Name
    AND Genre_ID <> @Genre_ID
        )
        BEGIN
THROW 50008, N'이미 동일한 콘텐츠 타입의 장르 이름이 존재합니다.', 1;
END;

-- 다른 장르와 TMDB_Genre_ID 중복 방지
IF @TMDB_Genre_ID IS NOT NULL
    AND EXISTS (
                SELECT 1
    FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_Type = @Content_Type
        AND TMDB_Genre_ID = @TMDB_Genre_ID
        AND Genre_ID <> @Genre_ID
           )
        BEGIN
THROW 50009, N'이미 동일한 콘텐츠 타입의 TMDB 장르 ID가 존재합니다.', 1;
END;

UPDATE dbo.Genre
        SET
            TMDB_Genre_ID = @TMDB_Genre_ID,
            Content_Type = @Content_Type,
            Genre_Name = @Genre_Name
        WHERE Genre_ID = @Genre_ID;

COMMIT TRANSACTION;

SELECT
    @Genre_ID AS Genre_ID,
    @TMDB_Genre_ID AS TMDB_Genre_ID,
    @Content_Type AS Content_Type,
    @Genre_Name AS Genre_Name,
    N'장르가 수정되었습니다.' AS Message;

END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;

THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_DeleteGenreWithRelations
   용도:
   - 장르 관리에서 장르와 콘텐츠 연결 관계를 삭제한다.

   입력:
   - @Genre_ID, @ForceDeleteRelations 강제 삭제 여부

   반환:
   - Genre_ID와 연결·장르 삭제 건수, Message

   주요 로직:
   - 연결된 콘텐츠 수를 집계하고 강제 삭제 옵션을 확인한다.
   - 허용된 경우 ContentGenre 관계를 먼저 지운 뒤 Genre를 삭제한다.

   주의:
   - 연결 관계가 있으면 @ForceDeleteRelations가 1일 때만 삭제한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_DeleteGenreWithRelations
    @Genre_ID INT,
    @ForceDeleteRelations BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Relation_Count INT = 0;
        DECLARE @Delete_ContentGenre_Count INT = 0;
        DECLARE @Delete_Genre_Count INT = 0;
        DECLARE @ErrorMessage NVARCHAR(2048);

        -- 필수값 검사
        IF @Genre_ID IS NULL
        BEGIN
            THROW 50000, N'장르 ID는 필수입니다.', 1;
        END;

        -- 장르 존재 확인
        IF NOT EXISTS (
            SELECT 1
    FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK)
    WHERE Genre_ID = @Genre_ID
        )
        BEGIN
            THROW 50001, N'삭제할 장르가 존재하지 않습니다.', 1;
        END;

    -- 연결 콘텐츠 수 확인
    SELECT @Relation_Count = COUNT(*)
    FROM dbo.ContentGenre WITH (UPDLOCK, HOLDLOCK)
    WHERE Genre_ID = @Genre_ID;

    -- 연결이 있는데 강제 삭제 옵션이 꺼져 있으면 중단
    IF @Relation_Count > 0 AND @ForceDeleteRelations = 0
        BEGIN
        SET @ErrorMessage =
                N'이 장르는 '
                + CAST(@Relation_Count AS NVARCHAR(20))
                + N'개의 콘텐츠와 연결되어 있습니다. 연결 정보까지 삭제할지 확인이 필요합니다.';

        THROW 50002, @ErrorMessage, 1;
    END;

    -- 연결 관계 삭제
    IF @ForceDeleteRelations = 1
        BEGIN
        DELETE FROM dbo.ContentGenre
            WHERE Genre_ID = @Genre_ID;

        SET @Delete_ContentGenre_Count = @@ROWCOUNT;
    END;

    -- 장르 삭제
    DELETE FROM dbo.Genre
        WHERE Genre_ID = @Genre_ID;

    SET @Delete_Genre_Count = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @Genre_ID AS Genre_ID,
        @Delete_ContentGenre_Count AS Delete_ContentGenre_Count,
        @Delete_Genre_Count AS Delete_Genre_Count,
        N'장르 및 연결 정보가 삭제되었습니다.' AS Message;

    END TRY
    BEGIN CATCH
    IF @@TRANCOUNT > 0
        BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
    END CATCH;
END;
GO

GO
/* =========================================================
   프로시저: dbo.Usp_AddOrUpdateOTTProvider
   용도:
   - OTT 관리에서 제공자를 추가하거나 수정한다.

   입력:
   - @OTT_ID, @OTT_NAME, @monthly_price

   반환:
   - OTT_ID, OTT_NAME, monthly_price, Message

   주요 로직:
   - OTT_ID 유무에 따라 추가와 수정 흐름을 나눈다.
   - 이름 중복과 월 구독료 유효성을 검사한 뒤 저장한다.

   주의:
   - 다른 OTT 제공자와 동일한 이름을 허용하지 않는다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_AddOrUpdateOTTProvider
    @OTT_ID INT = NULL,
    @OTT_NAME NVARCHAR(20),
    @monthly_price INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @OTT_NAME = LTRIM(RTRIM(@OTT_NAME));

        -- 필수값 검사
        IF @OTT_NAME IS NULL OR @OTT_NAME = N''
        BEGIN
            THROW 50000, N'OTT 이름은 필수입니다.', 1;
        END;

        IF LEN(@OTT_NAME) > 20
        BEGIN
            THROW 50001, N'OTT 이름은 20자를 초과할 수 없습니다.', 1;
        END;

    IF @monthly_price IS NULL
        BEGIN
    THROW 50002, N'월 구독료는 필수입니다.', 1;
END;

IF @monthly_price < 0
        BEGIN
THROW 50003, N'월 구독료는 음수일 수 없습니다.', 1;
END;

-- 추가
IF @OTT_ID IS NULL OR @OTT_ID <= 0
        BEGIN
    -- OTT 이름 중복 방지
    IF EXISTS (
                SELECT 1
    FROM dbo.OTTProvider WITH (UPDLOCK, HOLDLOCK)
    WHERE OTT_NAME = @OTT_NAME
            )
            BEGIN
    THROW 50004, N'이미 동일한 OTT 제공자가 존재합니다.', 1;
END;

INSERT INTO dbo.OTTProvider
    (
    OTT_NAME,
    monthly_price
    )
VALUES
    (
        @OTT_NAME,
        @monthly_price
            );

SET @OTT_ID = CAST(SCOPE_IDENTITY() AS INT);

COMMIT TRANSACTION;

SELECT
    @OTT_ID AS OTT_ID,
    @OTT_NAME AS OTT_NAME,
    @monthly_price AS monthly_price,
    N'OTT 제공자가 추가되었습니다.' AS Message;

RETURN;
END;

-- 수정 대상 존재 확인
IF NOT EXISTS (
            SELECT 1
FROM dbo.OTTProvider WITH (UPDLOCK, HOLDLOCK)
WHERE OTT_ID = @OTT_ID
        )
        BEGIN
THROW 50005, N'수정할 OTT 제공자가 존재하지 않습니다.', 1;
END;

-- 다른 OTT와 이름 중복 방지
IF EXISTS (
            SELECT 1
FROM dbo.OTTProvider WITH (UPDLOCK, HOLDLOCK)
WHERE OTT_NAME = @OTT_NAME
    AND OTT_ID <> @OTT_ID
        )
        BEGIN
THROW 50006, N'이미 동일한 OTT 제공자가 존재합니다.', 1;
END;

UPDATE dbo.OTTProvider
        SET
            OTT_NAME = @OTT_NAME,
            monthly_price = @monthly_price
        WHERE OTT_ID = @OTT_ID;

COMMIT TRANSACTION;

SELECT
    @OTT_ID AS OTT_ID,
    @OTT_NAME AS OTT_NAME,
    @monthly_price AS monthly_price,
    N'OTT 제공자가 수정되었습니다.' AS Message;

END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;

THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_DeleteOTTProviderWithRelations
   용도:
   - OTT 관리에서 제공자와 관련 데이터를 함께 삭제한다.

   입력:
   - @OTT_ID, @ForceDeleteRelations 강제 삭제 여부

   반환:
   - OTT_ID와 시청·구독·매핑·제공자 삭제 건수, Message

   주요 로직:
   - 콘텐츠 매핑·구독·시청 기록 수를 집계해 삭제 가능 여부를 판단한다.
   - WatchHistory, Usersubscription, ContentOTTProvider 순으로 지운 뒤 OTTProvider를 삭제한다.

   주의:
   - 연결 데이터가 있으면 강제 삭제 옵션이 필요하며 시청 기록도 삭제된다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_DeleteOTTProviderWithRelations
    @OTT_ID INT,
    @ForceDeleteRelations BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ContentOTTProvider_Count INT = 0;
        DECLARE @UserSubscription_Count INT = 0;
        DECLARE @WatchHistory_Count INT = 0;

        DECLARE @Delete_WatchHistory_Count INT = 0;
        DECLARE @Delete_UserSubscription_Count INT = 0;
        DECLARE @Delete_ContentOTTProvider_Count INT = 0;
        DECLARE @Delete_OTTProvider_Count INT = 0;

        DECLARE @Total_Relation_Count INT = 0;
        DECLARE @ErrorMessage NVARCHAR(2048);

        -- 필수값 검사
        IF @OTT_ID IS NULL
        BEGIN
            THROW 50000, N'OTT ID는 필수입니다.', 1;
        END;

        -- OTT 존재 확인
        IF NOT EXISTS (
            SELECT 1
    FROM dbo.OTTProvider WITH (UPDLOCK, HOLDLOCK)
    WHERE OTT_ID = @OTT_ID
        )
        BEGIN
            THROW 50001, N'삭제할 OTT 제공자가 존재하지 않습니다.', 1;
        END;

    -- 콘텐츠-OTT 연결 수
    SELECT @ContentOTTProvider_Count = COUNT(*)
    FROM dbo.ContentOTTProvider WITH (UPDLOCK, HOLDLOCK)
    WHERE OTT_ID = @OTT_ID;

    -- 사용자 구독 수
    SELECT @UserSubscription_Count = COUNT(*)
    FROM dbo.Usersubscription WITH (UPDLOCK, HOLDLOCK)
    WHERE OTT_ID = @OTT_ID;

    -- 시청 기록 수
    SELECT @WatchHistory_Count = COUNT(*)
    FROM dbo.WatchHistory WITH (UPDLOCK, HOLDLOCK)
    WHERE OTT_ID = @OTT_ID;

    SET @Total_Relation_Count =
            @ContentOTTProvider_Count
            + @UserSubscription_Count
            + @WatchHistory_Count;

    -- 연결 데이터가 있는데 강제 삭제 옵션이 꺼져 있으면 중단
    IF @Total_Relation_Count > 0 AND @ForceDeleteRelations = 0
        BEGIN
        SET @ErrorMessage =
                N'이 OTT 제공자는 관련 데이터가 있습니다. '
                + N'콘텐츠 연결 '
                + CAST(@ContentOTTProvider_Count AS NVARCHAR(20))
                + N'개, 구독 '
                + CAST(@UserSubscription_Count AS NVARCHAR(20))
                + N'개, 시청 기록 '
                + CAST(@WatchHistory_Count AS NVARCHAR(20))
                + N'개가 연결되어 있습니다. 관련 정보까지 삭제할지 확인이 필요합니다.';

        THROW 50002, @ErrorMessage, 1;
    END;

    IF @ForceDeleteRelations = 1
        BEGIN
        -- WatchHistory는 ContentOTTProvider 조합을 참조하므로 먼저 삭제
        DELETE FROM dbo.WatchHistory
            WHERE OTT_ID = @OTT_ID;

        SET @Delete_WatchHistory_Count = @@ROWCOUNT;

        -- 사용자 구독 삭제
        DELETE FROM dbo.Usersubscription
            WHERE OTT_ID = @OTT_ID;

        SET @Delete_UserSubscription_Count = @@ROWCOUNT;

        -- 콘텐츠-OTT 연결 삭제
        DELETE FROM dbo.ContentOTTProvider
            WHERE OTT_ID = @OTT_ID;

        SET @Delete_ContentOTTProvider_Count = @@ROWCOUNT;
    END;

    -- OTT 제공자 삭제
    DELETE FROM dbo.OTTProvider
        WHERE OTT_ID = @OTT_ID;

    SET @Delete_OTTProvider_Count = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @OTT_ID AS OTT_ID,
        @Delete_WatchHistory_Count AS Delete_WatchHistory_Count,
        @Delete_UserSubscription_Count AS Delete_UserSubscription_Count,
        @Delete_ContentOTTProvider_Count AS Delete_ContentOTTProvider_Count,
        @Delete_OTTProvider_Count AS Delete_OTTProvider_Count,
        N'OTT 제공자 및 관련 정보가 삭제되었습니다.' AS Message;

    END TRY
    BEGIN CATCH
    IF @@TRANCOUNT > 0
        BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
    END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_ImportTmdbGenre
   용도:
   - TMDB 장르 정보를 로컬 Genre 테이블에 조건부로 추가한다.

   입력:
   - @TMDB_Genre_ID, @Content_Type, @Genre_Name, @Inserted OUTPUT

   반환:
   - @Inserted 출력 파라미터

   주요 로직:
   - TMDB 장르 ID와 콘텐츠 유형 조합의 존재 여부를 확인한다.
   - 존재하지 않을 때만 장르를 추가하고 @Inserted를 1로 설정한다.

   주의:
   - 기존 조합이 있으면 삽입하지 않고 @Inserted는 0을 유지한다.
   ========================================================= */
Create or alter PROC dbo.Usp_ImportTmdbGenre
    @TMDB_Genre_ID int,
    @Content_Type varchar(10),
    @Genre_Name nvarchar(100),
    @Inserted bit output
as
begin
    set nocount on;
    set @Inserted = 0;

    if not exists
    (
        select 1
    from dbo.Genre
    where TMDB_Genre_ID = @TMDB_Genre_ID
        and Content_Type = @Content_Type
    )
    Begin
        Insert into dbo.Genre
            (
            TMDB_Genre_ID,
            Content_Type,
            Genre_Name
            )
        values
            (
                @TMDB_Genre_ID,
                @Content_Type,
                @Genre_Name
        );

        Set @Inserted = 1;
    END
END;
GO

/* =========================================================
   7. 콘텐츠 매핑 관련 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_LinkContentGenre
   용도:
   - ContentMapping에서 콘텐츠와 장르를 연결한다.

   입력:
   - @Content_ID, @Genre_ID

   반환:
   - Content_ID, Genre_ID, Content_Type, Message

   주요 로직:
   - 콘텐츠와 장르의 존재 여부 및 Content_Type 일치를 검사한다.
   - 중복 연결을 잠금 조회한 뒤 ContentGenre에 추가한다.

   주의:
   - 콘텐츠 유형과 장르 유형이 같아야 하며 중복 연결은 허용하지 않는다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_LinkContentGenre
    @Content_ID int,
    @Genre_ID int
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
    DECLARE @Content_Type VARCHAR(10);
    DECLARE @Genre_Content_Type VARCHAR(10);

    -- 필수값 검사
    if @Content_ID IS NULL
    BEGIN
        THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
    END
    if @Genre_ID IS NULL
    BEGIN
        THROW 50001, N'장르 ID는 필수입니다.', 1;
    END
    -- 콘텐츠 존재 확인 및 타입 조회
    SELECT @Content_Type = Content_Type
    FROM dbo.Content
    WHERE Content_ID = @Content_ID;

    if @Content_Type IS NULL
    BEGIN
    THROW 50002, N'콘텐츠가 존재하지 않습니다.', 1;
END;

-- 장르 타입 조회 / 확인
SELECT @Genre_Content_Type = Content_Type
FROM dbo.Genre
WHERE Genre_ID = @Genre_ID;

if @Genre_Content_Type IS NULL
    BEGIN
THROW 50003, N'장르가 존재하지 않습니다.', 1;
END;

-- 콘텐츠 타입과 장르 타입 일치 검사
if @Content_Type <> @Genre_Content_Type
    BEGIN
THROW 50004, N'콘텐츠와 장르의 타입이 일치하지 않습니다.', 1;
END;

-- 중복 연결 확인
if EXISTS
    (
        SELECT 1
from dbo.ContentGenre with (UPDLOCK, HOLDLOCK)
WHERE Content_ID = @Content_ID
    and Genre_ID = @Genre_ID
    )
    BEGIN
THROW 50005, N'이미 해당 콘텐츠와 장르의 연결이 존재합니다.',1;
END;

-- 콘텐츠 장르 연결
insert into dbo.ContentGenre
    (
    Content_ID,
    Genre_ID
    )
VALUES
    (
        @Content_ID,
        @Genre_ID
    );

COMMIT TRANSACTION;

SELECT
    @Content_ID AS Content_ID,
    @Genre_ID AS Genre_ID,
    @Content_Type AS Content_Type,
    N'콘텐츠와 장르의 연결이 설정되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
            BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_UnlinkContentGenre
   용도:
   - ContentMapping에서 콘텐츠와 장르 연결을 해제한다.

   입력:
   - @Content_ID, @Genre_ID

   반환:
   - Content_ID, Genre_ID, Message

   주요 로직:
   - 입력값과 기존 ContentGenre 연결의 존재 여부를 확인한다.
   - 확인된 콘텐츠-장르 연결 한 건을 삭제한다.

   주의:
   - 존재하지 않는 연결은 삭제하지 않는다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_UnlinkContentGenre
    @Content_ID INT,
    @Genre_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Content_ID IS NULL
        BEGIN
            THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
        END;

        IF @Genre_ID IS NULL
        BEGIN
            THROW 50001, N'장르 ID는 필수입니다.', 1;
        END;

    IF NOT EXISTS
        (
            SELECT 1
    FROM dbo.ContentGenre WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_ID = @Content_ID
        AND Genre_ID = @Genre_ID
        )
        BEGIN
    THROW 50002, N'연결된 콘텐츠-장르 정보가 없습니다.', 1;
END;

DELETE FROM dbo.ContentGenre
        WHERE Content_ID = @Content_ID
    AND Genre_ID = @Genre_ID;

COMMIT TRANSACTION;

SELECT
    @Content_ID AS Content_ID,
    @Genre_ID AS Genre_ID,
    N'콘텐츠와 장르의 연결이 해제되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;

THROW;
END CATCH;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_LinkContentOTTProvider
   용도:
   - ContentMapping에서 콘텐츠와 OTT 제공자를 연결한다.

   입력:
   - @Content_ID, @OTT_ID

   반환:
   - Content_ID, OTT_ID, Message

   주요 로직:
   - 콘텐츠와 OTT 제공자의 존재 여부를 확인한다.
   - 중복 연결을 잠금 조회한 뒤 ContentOTTProvider에 추가한다.

   주의:
   - 동일한 콘텐츠-OTT 조합의 중복 연결을 허용하지 않는다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_LinkContentOTTProvider
    @Content_ID int,
    @OTT_ID int
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

    IF @Content_ID IS NULL
    BEGIN
        THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
    END

    IF @OTT_ID IS NULL
    BEGIN
        THROW 50001, N'OTT ID는 필수입니다.', 1;
    END

    if not exists (
        select 1
    from dbo.Content
    where Content_ID = @Content_ID
    )
    BEGIN
    THROW 50002, N'콘텐츠가 존재하지 않습니다.', 1;
END;

if not exists (
        select 1
from dbo.OTTProvider
where OTT_ID = @OTT_ID
    )
    BEGIN
THROW 50003, N'OTT 제공자가 존재하지 않습니다.', 1;
END;
-- 중복 연결 확인
IF EXISTS
    (
        SELECT 1
FROM dbo.ContentOTTProvider with (UPDLOCK, HOLDLOCK)
WHERE Content_ID = @Content_ID
    AND OTT_ID = @OTT_ID
    )
    BEGIN
THROW 50004, N'이미 해당 콘텐츠와 OTT 제공자가 연결되어 있습니다.',1;
END;

-- 콘텐츠-OTT 연결
insert into dbo.ContentOTTProvider
    (
    Content_ID,
    OTT_ID
    )
VALUES(
        @Content_ID,
        @OTT_ID
    );

COMMIT TRANSACTION;

SELECT
    @Content_ID AS Content_ID,
    @OTT_ID AS OTT_ID,
    N'콘텐츠와 OTT 제공자가 연결되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
            BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO


GO
/* =========================================================
   프로시저: dbo.Usp_UnlinkContentOTTProvider
   용도:
   - ContentMapping에서 콘텐츠와 OTT 제공자 연결을 해제한다.

   입력:
   - @Content_ID, @OTT_ID

   반환:
   - Content_ID, OTT_ID, Message

   주요 로직:
   - 입력값과 기존 ContentOTTProvider 연결의 존재 여부를 확인한다.
   - 확인된 콘텐츠-OTT 연결 한 건을 삭제한다.

   주의:
   - WatchHistory가 해당 조합을 참조하는 경우 외래키 영향을 확인해야 한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_UnlinkContentOTTProvider
    @Content_ID INT,
    @OTT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 필수값 검사
        IF @Content_ID IS NULL
        BEGIN
            THROW 50000, N'콘텐츠 ID는 필수입니다.', 1;
        END;

        IF @OTT_ID IS NULL
        BEGIN
            THROW 50001, N'OTT ID는 필수입니다.', 1;
        END;

    -- 연결 존재 확인
    IF NOT EXISTS
        (
            SELECT 1
    FROM dbo.ContentOTTProvider WITH (UPDLOCK, HOLDLOCK)
    WHERE Content_ID = @Content_ID
        AND OTT_ID = @OTT_ID
        )
        BEGIN
    THROW 50002, N'연결된 콘텐츠-OTT 정보가 없습니다.', 1;
END;

-- 콘텐츠-OTT 연결 해제
DELETE FROM dbo.ContentOTTProvider
        WHERE Content_ID = @Content_ID
    AND OTT_ID = @OTT_ID;

COMMIT TRANSACTION;

SELECT
    @Content_ID AS Content_ID,
    @OTT_ID AS OTT_ID,
    N'콘텐츠와 OTT 제공자의 연결이 해제되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
        BEGIN
    ROLLBACK TRANSACTION;
END;

THROW;
END CATCH;
END;
GO


GO
/* =========================================================
   프로시저: dbo.Usp_GetContentMappingSummary
   용도:
   - ContentMapping 화면의 장르·OTT·미연결 콘텐츠 요약을 조회한다.

   입력:
   - 입력 파라미터 없음

   반환:
   - TotalGenreCount, TotalOttProviderCount, UnmappedContentCount

   주요 로직:
   - 장르와 OTT 제공자 수를 각각 집계한다.
   - 장르 또는 OTT 연결이 없는 콘텐츠 수를 NOT EXISTS로 계산한다.

   주의:
   - 미연결 콘텐츠는 장르나 OTT 중 하나라도 연결되지 않은 콘텐츠다.
   ========================================================= */
create or alter Proc dbo.Usp_GetContentMappingSummary
as
begin
    set nocount on;

    -- 기준 정보 수와 장르·OTT 미연결 콘텐츠 수를 한 번에 집계한다.
    select
        (
        Select count(*)
        from dbo.Genre
    ) as RegisteredGenreCount,

        (
        Select count(*)
        from dbo.OTTProvider
    ) As OttProviderCount,

        (
        Select count(*)
        from dbo.Content C
        where not exists
        (
            select 1
            from dbo.ContentGenre CG
            where CG.Content_ID = C.Content_ID
        )
            or not exists
        (
            select 1
            from dbo.ContentOTTProvider COP
            where COP.content_ID = C.Content_ID
        )
    ) AS UnmappedContentCount;
End;
GO

/* =========================================================
   프로시저: dbo.Usp_GetContentMappingContentList
   용도:
   - ContentMapping 화면의 콘텐츠별 연결 상태 목록을 조회한다.

   입력:
   - 입력 파라미터 없음

   반환:
   - 콘텐츠 정보, GenreCount, OttCount, MappingStatus, MappingSortOrder

   주요 로직:
   - CTE에서 콘텐츠별 장르와 OTT 연결 수를 각각 집계한다.
   - CASE로 매핑 상태와 정렬 순서를 계산해 목록을 반환한다.

   주의:
   - 연결 수가 0인 조합에 따라 상태 코드와 표시 문구가 달라진다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetContentMappingContentList
AS
BEGIN
    SET NOCOUNT ON;
    -- 장르와 OTT 연결 수를 각각 집계한 뒤 콘텐츠 목록에 결합한다.
    WITH
        GenreCountCTE
        AS
        (
            SELECT
                Content_ID,
                COUNT(*) AS GenreCount
            FROM dbo.ContentGenre
            GROUP BY Content_ID
        ),
        OttCountCTE
        AS
        (
            SELECT
                Content_ID,
                COUNT(*) AS OttCount
            FROM dbo.ContentOTTProvider
            GROUP BY Content_ID
        )
    SELECT
        C.Content_ID,
        C.Content_Type,
        C.Title,

        ISNULL(GC.GenreCount, 0) AS GenreCount,
        ISNULL(OC.OttCount, 0) AS OttCount,

        CASE
            WHEN ISNULL(GC.GenreCount, 0) = 0
            AND ISNULL(OC.OttCount, 0) = 0
                THEN N'전체 미연결'

            WHEN ISNULL(GC.GenreCount, 0) = 0
                THEN N'장르 없음'

            WHEN ISNULL(OC.OttCount, 0) = 0
                THEN N'OTT 없음'

            ELSE N'연결 완료'
        END AS MappingStatus,

        -- 연결 수 조합을 화면 표시용 매핑 상태로 변환한다.
        CASE
            WHEN ISNULL(GC.GenreCount, 0) = 0
            AND ISNULL(OC.OttCount, 0) = 0
                THEN 0

            WHEN ISNULL(GC.GenreCount, 0) = 0
                THEN 1

            WHEN ISNULL(OC.OttCount, 0) = 0
                THEN 2

            ELSE 3
        END AS MappingSortOrder

    FROM dbo.Content C
        LEFT JOIN GenreCountCTE GC
        ON C.Content_ID = GC.Content_ID
        LEFT JOIN OttCountCTE OC
        ON C.Content_ID = OC.Content_ID

    ORDER BY
        MappingSortOrder,
        C.Title;
END;
GO

/* =========================================================
   8. 사용자 관리 저장 프로시저
   ========================================================= */

GO
/* =========================================================
   프로시저: dbo.Usp_DeleteUserWithHistory
   용도:
   - 사용자 관리에서 회원과 관련 기록을 함께 삭제한다.

   입력:
   - @user_id 삭제할 사용자

   반환:
   - User_ID와 평점·시청·구독·사용자 삭제 건수, Message

   주요 로직:
   - UserRating, WatchHistory, Usersubscription 순으로 관련 데이터를 삭제한다.
   - 관계 데이터 삭제 후 AppUser를 삭제하고 건수를 반환한다.

   주의:
   - 사용자의 평점·시청·구독 기록이 모두 삭제된다.
   ========================================================= */
CREATE or ALTER proc dbo.Usp_DeleteUserWithHistory
    @user_id INT
as
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

    DECLARE @Delete_Rating_Count int = 0;
    DECLARE @Delete_WatchHistory_Count int = 0;
    DECLARE @Delete_Subscription_Count int = 0;
    DECLARE @Delete_User_Count int = 0;

    -- 기본값 검사
    IF @user_id IS NULL
    BEGIN
        THROW 50000, N'회원 ID는 필수입니다.', 1;
    END;

    if not EXISTS
    (
        SELECT 1
    FROM dbo.AppUser With (UPDLOCK, HOLDLOCK)
    WHERE User_ID = @user_id
    )
    BEGIN
        THROW 50001, N'사용자가 존재하지 않습니다.', 1;
    END;

    -- 평점 삭제
    DELETE FROM dbo.UserRating
        WHERE user_id = @user_id;

    Set @Delete_Rating_Count = @@ROWCOUNT;

    -- 시청 기록 삭제
    DELETE FROM dbo.WatchHistory
        WHERE User_ID = @user_id;

    Set @Delete_WatchHistory_Count = @@ROWCOUNT;

    -- 구독 삭제
    DELETE FROM dbo.Usersubscription
        WHERE User_ID = @user_id;

    Set @Delete_Subscription_Count = @@ROWCOUNT;

    -- 회원 삭제
    DELETE FROM dbo.AppUser
        WHERE User_ID = @user_id;

    Set @Delete_User_Count = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @user_id AS User_ID,
        @Delete_Rating_Count AS Delete_Rating_Count,
        @Delete_WatchHistory_Count AS Delete_WatchHistory_Count,
        @Delete_Subscription_Count AS Delete_Subscription_Count,
        @Delete_User_Count AS Delete_User_Count,
        N'회원 및 관련 기록이 삭제되었습니다.' AS Message;

    END TRY
    BEGIN CATCH
    IF @@TRANCOUNT > 0
            BEGIN
        ROLLBACK TRANSACTION;
    END;
    THROW;
    END CATCH;
END;
GO

/* =========================================================
   9. 대시보드 관련 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_GetDashboardSummary
   용도:
   - Dashboard 상단에 표시할 전체 현황 요약을 조회한다.

   입력:
   - 입력 파라미터 없음

   반환:
   - 콘텐츠·사용자·활성 구독·OTT·미매핑·만료 예정 건수

   주요 로직:
   - 각 기준 테이블과 관계 테이블을 스칼라 집계로 요약한다.
   - 현재 활성 구독과 7일 이내 만료 예정 구독을 날짜 조건으로 계산한다.

   주의:
   - 현재 서버 날짜 GETDATE를 기준으로 구독 상태를 판단한다.
   ========================================================= */
create or alter PROC dbo.Usp_GetDashboardSummary
as
begin
    set nocount on;

    -- 대시보드 카드에 필요한 독립 집계값을 한 결과 행으로 반환한다.
    Select
        (Select count(*)
        from dbo.Content) as TotalContentCount,

        (Select count(*)
        From dbo.AppUser) as TotalUserCount,

        (Select count(*)
        from dbo.Usersubscription
        where Cast(GetDate() AS date) between started_at and expired_at) AS ActiveSubscriptionCount,

        (Select count(*)
        From dbo.OTTProvider) as OttProviderCount,

        (Select Count(*)
        from dbo.Content C
        Where not Exists
        (
            select 1
        from dbo.ContentGenre CG
        where CG.Content_ID = C.Content_ID
        )) as UnmappedGenreContentCount,

        (select Count(*)
        from dbo.Content C
        where not exists
        (
            select 1
        from dbo.ContentOTTProvider COP
        where COP.Content_ID = C.Content_ID
        )) as UnmappedOttContentCount,


        (select count(*)
        from dbo.Usersubscription
        where expired_at >= CAST(Getdate() As date)
            and expired_at <= DATEADD(DAY, 7, CAST(GETDATE() AS date))) AS ExpiringSubscriptionCount;
end;
GO

/* =========================================================
   10. 통계 관련 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_GetStatisticsSummaryCards
   용도:
   - Statistics 화면 상단의 요약 카드 값을 조회한다.

   입력:
   - @StartDate, @EndDate, @ContentType

   반환:
   - TotalUserCount, TotalWatchMinutes, AverageRatingScore, ActiveSubscriptionRate

   주요 로직:
   - 기간과 콘텐츠 유형을 기준으로 사용자·시청 시간·평균 평점을 집계한다.
   - 전체 사용자 대비 기간 내 활성 구독 사용자 비율을 계산한다.

   주의:
   - 기간 조건은 시작 이상, 종료 미만이며 ContentType ALL은 전체 유형을 뜻한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetStatisticsSummaryCards
    @StartDate   DATETIME2,
    @EndDate     DATETIME2,
    @ContentType VARCHAR(10) = 'ALL'
AS
BEGIN
    SET NOCOUNT ON;

    -- 동일 기간·콘텐츠 유형 조건으로 사용자, 시청 시간, 평점, 구독률을 각각 집계한다.
    SELECT
        (
            SELECT COUNT(DISTINCT WH.User_ID)
            FROM dbo.WatchHistory WH
            INNER JOIN dbo.Content C
                ON WH.Content_ID = C.Content_ID
            WHERE WH.Watch_Ended_At IS NOT NULL
              AND WH.Watch_Ended_At >= @StartDate
              AND WH.Watch_Ended_At < @EndDate
              AND (
                    @ContentType = 'ALL'
                    OR C.Content_Type = @ContentType
                  )
        ) AS TotalUserCount,

        (
            SELECT ISNULL(SUM(CAST(WH.Watch_Minutes AS DECIMAL(18,2))), 0)
            FROM dbo.WatchHistory WH
            INNER JOIN dbo.Content C
                ON WH.Content_ID = C.Content_ID
            WHERE WH.Watch_Ended_At IS NOT NULL
              AND WH.Watch_Ended_At >= @StartDate
              AND WH.Watch_Ended_At < @EndDate
              AND (
                    @ContentType = 'ALL'
                    OR C.Content_Type = @ContentType
                  )
        ) AS TotalWatchMinutes,

        (
            SELECT ISNULL(AVG(CAST(UR.Rating_Score AS DECIMAL(10,2))), 0)
            FROM dbo.UserRating UR
            INNER JOIN dbo.Content C
                ON UR.Content_ID = C.Content_ID
            WHERE UR.Rated_At >= @StartDate
              AND UR.Rated_At < @EndDate
              AND (
                    @ContentType = 'ALL'
                    OR C.Content_Type = @ContentType
                  )
        ) AS AverageRatingScore,

        (
            SELECT
                CASE
                    WHEN COUNT(DISTINCT AU.User_ID) = 0 THEN 0
                    ELSE
                        CAST(COUNT(DISTINCT US.User_ID) AS DECIMAL(10,2))
                        / COUNT(DISTINCT AU.User_ID) * 100
                END
            FROM dbo.AppUser AU
            LEFT JOIN dbo.UserSubscription US
                ON AU.User_ID = US.User_ID
               AND US.started_at < @EndDate
               AND US.expired_at >= @StartDate
        ) AS ActiveSubscriptionRate;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_GetTopGenreWatchStatistics
   용도:
   - Statistics 화면의 장르별 시청 TOP N을 조회한다.

   입력:
   - @StartDate, @EndDate, @ContentType, @TopCount

   반환:
   - GenreRank, GenreName, WatchCount, ShareRate

   주요 로직:
   - CTE에서 기간 내 장르별 시청 횟수와 전체 횟수를 집계한다.
   - ROW_NUMBER로 순위를 정하고 전체 대비 점유율을 계산한다.

   주의:
   - 하나의 콘텐츠가 여러 장르에 연결되면 각 장르 집계에 포함된다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetTopGenreWatchStatistics
    @StartDate   DATETIME2,
    @EndDate     DATETIME2,
    @ContentType VARCHAR(10) = 'ALL',
    @TopCount    INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    SET @ContentType = UPPER(LTRIM(RTRIM(@ContentType)));

    IF @TopCount IS NULL OR @TopCount <= 0
        SET @TopCount = 5;

    -- 장르별 시청 횟수와 전체 시청 횟수를 분리 집계해 점유율 계산에 사용한다.
    ;WITH GenreWatch AS
    (
        SELECT
            G.Genre_ID,
            G.Genre_Name,
            COUNT(*) AS WatchCount
        FROM dbo.WatchHistory WH
        INNER JOIN dbo.Content C
            ON WH.Content_ID = C.Content_ID
        INNER JOIN dbo.ContentGenre CG
            ON C.Content_ID = CG.Content_ID
        INNER JOIN dbo.Genre G
            ON CG.Genre_ID = G.Genre_ID
        WHERE WH.Watch_Ended_At IS NOT NULL
          AND WH.Watch_Ended_At >= @StartDate
          AND WH.Watch_Ended_At < @EndDate
          AND (
                @ContentType = 'ALL'
                OR C.Content_Type = @ContentType
              )
        GROUP BY
            G.Genre_ID,
            G.Genre_Name
    ),
    GenreTotal AS
    (
        SELECT
            ISNULL(SUM(WatchCount), 0) AS TotalWatchCount
        FROM GenreWatch
    )
    SELECT TOP (@TopCount)
        CAST(
            ROW_NUMBER() OVER (
                ORDER BY GW.WatchCount DESC, GW.Genre_Name
            ) AS INT
        ) AS GenreRank,

        GW.Genre_Name AS GenreName,
        GW.WatchCount,

        CASE
            WHEN GT.TotalWatchCount = 0 THEN 0
            ELSE
                CAST(GW.WatchCount AS DECIMAL(10,2))
                / GT.TotalWatchCount * 100
        END AS ShareRate

    FROM GenreWatch GW
    CROSS JOIN GenreTotal GT
    ORDER BY
        GW.WatchCount DESC,
        GW.Genre_Name;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_GetTopContentStatistics
   용도:
   - Statistics 화면의 상위 콘텐츠 요약 목록을 조회한다.

   입력:
   - @StartDate, @EndDate, @TopCount

   반환:
   - 순위, 콘텐츠, 시청 집계, 대표 OTT, 장르 목록, 평균 평점

   주요 로직:
   - CTE에서 콘텐츠별 시청 횟수와 총 시청 시간을 집계한다.
   - OUTER APPLY로 대표 OTT·장르 목록·평균 평점을 결합해 순위를 반환한다.

   주의:
   - 대표 OTT는 기간 내 시청 횟수가 많은 순서로 선택한다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetTopContentStatistics
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @TopCount INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    IF @TopCount IS NULL OR @TopCount <= 0
    BEGIN
        SET @TopCount = 5;
    END;

    -- 기간 내 콘텐츠별 시청 횟수와 총 시청 시간을 먼저 집계한다.
    ;WITH ContentWatch AS
    (
        SELECT
            C.Content_ID,
            C.Title,
            COUNT(*) AS WatchCount,
            ISNULL(SUM(CAST(WH.Watch_Minutes AS DECIMAL(18,2))), 0) AS TotalWatchMinutes
        FROM dbo.WatchHistory WH
        INNER JOIN dbo.Content C
            ON WH.Content_ID = C.Content_ID
        WHERE WH.Watch_Ended_At IS NOT NULL
          AND WH.Watch_Ended_At >= @StartDate
          AND WH.Watch_Ended_At < @EndDate
        GROUP BY
            C.Content_ID,
            C.Title
    )
    SELECT TOP (@TopCount)
        CAST(
            ROW_NUMBER() OVER (
                ORDER BY
                    CW.WatchCount DESC,
                    CW.TotalWatchMinutes DESC,
                    ISNULL(RT.AverageRatingScore, 0) DESC,
                    CW.Title
            ) AS INT
        ) AS ContentRank,

        CW.Content_ID,
        CW.Title,
        CW.WatchCount,
        CW.TotalWatchMinutes,

        ISNULL(OT.OttName, '-') AS OttName,
        ISNULL(GN.GenreNames, '-') AS GenreNames,
        ISNULL(RT.AverageRatingScore, 0) AS AverageRatingScore

    FROM ContentWatch CW

    -- 콘텐츠별 대표 OTT, 장르 목록, 평균 평점을 개별 집계로 결합한다.
    OUTER APPLY
    (
        SELECT TOP 1
            OP.OTT_Name AS OttName
        FROM dbo.WatchHistory WH2
        INNER JOIN dbo.OTTProvider OP
            ON WH2.OTT_ID = OP.OTT_ID
        WHERE WH2.Content_ID = CW.Content_ID
          AND WH2.Watch_Ended_At IS NOT NULL
          AND WH2.Watch_Ended_At >= @StartDate
          AND WH2.Watch_Ended_At < @EndDate
        GROUP BY
            OP.OTT_Name
        ORDER BY
            COUNT(*) DESC,
            OP.OTT_Name
    ) OT

    OUTER APPLY
    (
        SELECT
            STUFF(
                (
                    SELECT ', ' + G.Genre_Name
                    FROM dbo.ContentGenre CG
                    INNER JOIN dbo.Genre G
                        ON CG.Genre_ID = G.Genre_ID
                    WHERE CG.Content_ID = CW.Content_ID
                    ORDER BY G.Genre_Name
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'),
                1,
                2,
                ''
            ) AS GenreNames
    ) GN

    OUTER APPLY
    (
        SELECT
            AVG(CAST(UR.Rating_Score AS DECIMAL(10,2))) AS AverageRatingScore
        FROM dbo.UserRating UR
        WHERE UR.Content_ID = CW.Content_ID
    ) RT

    ORDER BY
        CW.WatchCount DESC,
        CW.TotalWatchMinutes DESC,
        ISNULL(RT.AverageRatingScore, 0) DESC,
        CW.Title;
END;
GO

/* =========================================================
   프로시저: dbo.Usp_GetTopContentRanking
   용도:
   - Statistics 화면의 필터 가능한 전체 콘텐츠 순위를 조회한다.

   입력:
   - @StartDate, @EndDate, @Keyword, @ContentType, @OttId, @GenreId

   반환:
   - 순위, 콘텐츠, OTT·장르 목록, 시청·사용자·평점·완료율 집계

   주요 로직:
   - FilteredWatch CTE에서 기간·검색어·유형·OTT·장르 조건을 적용한다.
   - ContentWatch CTE와 APPLY 집계로 순위와 표시 정보를 구성한다.

   주의:
   - 기간은 종료 시각 기준 시작 이상·종료 미만이며 여러 선택 필터가 함께 적용된다.
   ========================================================= */
CREATE OR ALTER PROC dbo.Usp_GetTopContentRanking
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @Keyword NVARCHAR(100) = NULL,
    @ContentType VARCHAR(10) = 'ALL',
    @OttId INT = NULL,
    @GenreId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Keyword = NULLIF(LTRIM(RTRIM(@Keyword)), '');
    SET @ContentType = UPPER(LTRIM(RTRIM(ISNULL(@ContentType, 'ALL'))));

    IF @ContentType NOT IN ('ALL', 'MOVIE', 'TV')
    BEGIN
        SET @ContentType = 'ALL';
    END;

    -- 선택된 기간과 검색 필터를 먼저 적용해 이후 집계 대상을 제한한다.
    ;WITH FilteredWatch AS
    (
        SELECT
            WH.Watch_ID,
            WH.User_ID,
            WH.Content_ID,
            WH.OTT_ID,
            ISNULL(WH.Watch_Minutes, 0) AS Watch_Minutes,
            ISNULL(WH.Progress_Percentage, 0) AS Progress_Percentage,
            ISNULL(WH.Is_Completed, 0) AS Is_Completed
        FROM dbo.WatchHistory WH
        INNER JOIN dbo.Content C
            ON WH.Content_ID = C.Content_ID
        WHERE WH.Watch_Ended_At IS NOT NULL
          AND WH.Watch_Ended_At >= @StartDate
          AND WH.Watch_Ended_At < @EndDate
          AND (
                @Keyword IS NULL
                OR C.Title LIKE '%' + @Keyword + '%'
              )
          AND (
                @ContentType = 'ALL'
                OR C.Content_Type = @ContentType
              )
          AND (
                @OttId IS NULL
                OR WH.OTT_ID = @OttId
              )
          AND (
                @GenreId IS NULL
                OR EXISTS
                (
                    SELECT 1
                    FROM dbo.ContentGenre CG
                    WHERE CG.Content_ID = C.Content_ID
                      AND CG.Genre_ID = @GenreId
                )
              )
    ),
    ContentWatch AS
    (
        SELECT
            C.Content_ID,
            C.Title,
            C.Content_Type,
            C.Poster_URL,

            COUNT(*) AS WatchCount,
            COUNT(DISTINCT FW.User_ID) AS ViewerCount,

            SUM(CAST(FW.Watch_Minutes AS DECIMAL(18,2))) AS TotalWatchMinutes,

            CAST(
                AVG(
                    CASE
                        WHEN FW.Is_Completed = 1
                          OR FW.Progress_Percentage >= 100
                        THEN 100.0
                        ELSE 0.0
                    END
                ) AS DECIMAL(10,2)
            ) AS CompletionRate
        FROM FilteredWatch FW
        INNER JOIN dbo.Content C
            ON FW.Content_ID = C.Content_ID
        GROUP BY
            C.Content_ID,
            C.Title,
            C.Content_Type,
            C.Poster_URL
    )
    SELECT
        CAST(
            ROW_NUMBER() OVER (
                ORDER BY
                    CW.TotalWatchMinutes DESC,
                    CW.WatchCount DESC,
                    ISNULL(RT.AverageRatingScore, 0) DESC,
                    CW.Title
            ) AS INT
        ) AS ContentRank,

        CW.Content_ID,
        CW.Title,
        CW.Content_Type AS ContentType,
        CW.Poster_URL AS PosterUrl,

        ISNULL(OT.OttNames, '-') AS OttNames,
        ISNULL(GN.GenreNames, '-') AS GenreNames,

        CW.WatchCount,
        CW.ViewerCount,
        CW.TotalWatchMinutes,

        ISNULL(RT.AverageRatingScore, 0) AS AverageRatingScore,
        CW.CompletionRate
    FROM ContentWatch CW

    -- 필터 결과에 OTT·장르 표시값과 평균 평점을 콘텐츠별로 결합한다.
    OUTER APPLY
    (
        SELECT
            STUFF(
                (
                    SELECT ', ' + X.OTT_Name
                    FROM
                    (
                        SELECT DISTINCT
                            OP.OTT_Name
                        FROM FilteredWatch FW2
                        INNER JOIN dbo.OTTProvider OP
                            ON FW2.OTT_ID = OP.OTT_ID
                        WHERE FW2.Content_ID = CW.Content_ID
                    ) X
                    ORDER BY X.OTT_Name
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'),
                1,
                2,
                ''
            ) AS OttNames
    ) OT

    OUTER APPLY
    (
        SELECT
            STUFF(
                (
                    SELECT ', ' + X.Genre_Name
                    FROM
                    (
                        SELECT DISTINCT
                            G.Genre_Name
                        FROM dbo.ContentGenre CG
                        INNER JOIN dbo.Genre G
                            ON CG.Genre_ID = G.Genre_ID
                        WHERE CG.Content_ID = CW.Content_ID
                    ) X
                    ORDER BY X.Genre_Name
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'),
                1,
                2,
                ''
            ) AS GenreNames
    ) GN

    OUTER APPLY
    (
        SELECT
            AVG(CAST(UR.Rating_Score AS DECIMAL(10,2))) AS AverageRatingScore
        FROM dbo.UserRating UR
        WHERE UR.Content_ID = CW.Content_ID
    ) RT

    ORDER BY
        ContentRank;
END;
GO

/* =========================================================
   11. 시스템 설정 관련 저장 프로시저
   ========================================================= */
/* =========================================================
   프로시저: dbo.Usp_UpdateSystemSetting
   용도:
   - 시스템 설정 화면에서 기존 설정값을 수정한다.

   입력:
   - @Setting_Key, @Setting_Value

   반환:
   - Setting_Key, Setting_Value, Message

   주요 로직:
   - 필수값과 설정 키의 존재 여부를 확인한다.
   - 확인된 SystemSetting 행의 값을 갱신한다.

   주의:
   - 새 설정 키를 추가하지 않고 기존 키만 수정한다.
   ========================================================= */
CREATE OR ALTER proc dbo.Usp_UpdateSystemSetting
    @Setting_Key VARCHAR(50),
    @Setting_Value NVARCHAR(200)
AS
BEGIN
    set NOCOUNT ON;
    set XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

    DECLARE @Min_Rating_Progress DECIMAL(5,2);
    SET @Setting_Key = UPPER(LTRIM(RTRIM(@Setting_Key)))
    SET @Setting_Value = LTRIM(RTRIM(@Setting_Value))

    IF @Setting_Key IS NULL or @Setting_Key = ''
    BEGIN
        THROW 50000, N'설정 키는 필수입니다.', 1;
    END;

    IF @Setting_Value IS NULL or @Setting_Value = N''
    BEGIN
        THROW 50001, N'설정 값은 필수입니다.', 1;
    END;

    IF @Setting_Key NOT IN ('MIN_RATING_PROGRESS','CSV_EXPORT_PATH')
    BEGIN
    THROW 50002, N'수정할수 없는 설정 키입니다.', 1;
END;

-- 설정키 존재 확인
IF NOT EXISTS(
        SELECT 1
FROM dbo.SystemSetting WITH (UPDLOCK, HOLDLOCK)
WHERE Setting_Key = @Setting_Key
    )
    BEGIN
THROW 50003, N'설정 키가 존재하지 않습니다.', 1;
END;

IF @Setting_Key = 'MIN_RATING_PROGRESS'
    BEGIN
    SET @Min_Rating_Progress = TRY_CONVERT(DECIMAL(5,2), @Setting_Value)
    IF @Min_Rating_Progress IS NULL
        BEGIN
    THROW 50004, N'설정 값이 숫자가 아닙니다.', 1;
END;
IF @Min_Rating_Progress < 0 OR @Min_Rating_Progress > 100
        BEGIN
THROW 50005, N'설정 값은 0에서 100 사이여야 합니다.', 1;
END;
END;

IF @Setting_Key = 'CSV_EXPORT_PATH'
    BEGIN
    IF LEN(@Setting_Value) < 3
        BEGIN
    THROW 50006, N'설정 값이 너무 짧습니다.', 1;
END;
END;

UPDATE dbo.SystemSetting
        SET Setting_Value = @Setting_Value
        WHERE Setting_Key = @Setting_Key;

COMMIT TRANSACTION;

SELECT
    @Setting_Key AS Setting_Key,
    @Setting_Value AS Setting_Value,
    N'설정이 업데이트되었습니다.' AS Message;
END TRY
    BEGIN CATCH
IF @@TRANCOUNT > 0
            BEGIN
    ROLLBACK TRANSACTION;
END;
THROW;
END CATCH;
END;
GO

/* =========================================================
   12. 개발 / 검증용 쿼리
   - 설치 스크립트 실행 대상 아님
   - 필요할 때 개별 주석 해제 후 실행
   ========================================================= */
/*
-- 아래 쿼리는 개발 중 확인용이므로 설치 스크립트에서는 개별 실행한다.
EXEC sp_help SystemSetting;
EXEC sp_help Usersubscription;


-- ContentMapping 화면 Genre 관리 확인용
SELECT
    G.Genre_ID,
    G.TMDB_Genre_ID,
    G.Content_Type,
    G.Genre_Name,
    COUNT(CG.Content_ID) AS LinkedContentCount
FROM Genre G
    LEFT JOIN dbo.ContentGenre CG
        ON G.Genre_ID = CG.Genre_ID
GROUP BY
    G.Genre_ID,
    G.TMDB_Genre_ID,
    G.Content_Type,
    G.Genre_Name
ORDER BY
    G.Content_Type,
    G.Genre_Name;

-- OTT Provider 탭 Datagrid 리스트 확인용
SELECT
    OP.OTT_ID,
    OP.OTT_NAME,
    OP.monthly_price,
    COUNT(DISTINCT COP.Content_ID) AS LinkedContentCount,
    COUNT(DISTINCT US.Subscription_ID) AS SubscriptionCount,
    COUNT(DISTINCT WH.Watch_ID) AS WatchHistoryCount
FROM dbo.OTTProvider OP
    LEFT JOIN dbo.ContentOTTProvider COP
        ON OP.OTT_ID = COP.OTT_ID
    LEFT JOIN dbo.Usersubscription US
        ON OP.OTT_ID = US.OTT_ID
    LEFT JOIN dbo.WatchHistory WH
        ON OP.OTT_ID = WH.OTT_ID
GROUP BY
    OP.OTT_ID,
    OP.OTT_NAME,
    OP.monthly_price
ORDER BY
    OP.OTT_NAME;

-- 유저 목록 확인용: User_ID 역순
SELECT
    User_ID,
    User_NAME,
    User_BirthDate,
    User_PhoneNumber,
    User_Sex
FROM dbo.AppUser
ORDER BY User_ID DESC;

-- 유저 목록 확인용: User_Name 순
SELECT
    User_ID,
    User_Name,
    User_BirthDate,
    User_PhoneNumber,
    User_Sex
FROM dbo.AppUSer
ORDER BY User_Name;

-- OTT Provider 목록 확인용
SELECT
    OTT_ID,
    OTT_Name,
    Monthly_Price
FROM dbo.OTTProvider
ORDER BY OTT_Name;

-- 미완성 구문: 조회 컬럼이 정의되지 않아 실행하지 않는다.
SELECT

FROM dbo.UserSubscription;
*/
