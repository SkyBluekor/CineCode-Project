## TMDB API 키 설정 방법

이 프로젝트는 콘텐츠 검색 기능을 위해 TMDB API를 사용합니다.

보안상 API 키는 코드나 저장소에 직접 저장하지 않고, Windows 환경 변수로 등록해서 사용합니다.

### 1. TMDB API 키 발급

TMDB 계정에서 API 키를 발급받습니다.

### 2. Windows 환경 변수 등록

명령 프롬프트 또는 PowerShell에서 아래 명령을 실행합니다.

```bash
setx TMDB_API_KEY "본인의_TMDB_API_KEY"
```

등록 후 새 터미널 또는 Visual Studio를 다시 열어 실행합니다.
