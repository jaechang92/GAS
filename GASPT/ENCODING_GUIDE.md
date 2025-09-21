# 한글 인코딩 문제 해결 및 방지 가이드

## 🚨 문제 개요

Unity 프로젝트에서 한글 주석이 깨지는 문제는 다음과 같은 원인들로 발생합니다:

1. **파일 인코딩 불일치**: ISO-8859-1 vs UTF-8
2. **Git 설정 문제**: 줄 끝 처리 및 인코딩 설정 부재
3. **에디터 설정 부재**: EditorConfig 없음
4. **시스템 로케일 차이**: 개발 환경별 인코딩 차이

## 🔍 원인 분석

### 1. 파일 인코딩 확인 방법
```bash
# 파일 인코딩 확인
file Assets/Scripts/GameFlow/GameState.cs
# 결과: "ISO-8859 text" -> 문제 있음
# 정상: "UTF-8 Unicode text" -> 정상
```

### 2. 깨지는 패턴
```csharp
// 원본 한글
// 메인 메뉴 UI 활성화

// 깨진 텍스트
// ¸ÞÀÎ ¸Þ´º UI È°¼ºÈ­
```

## ✅ 해결책 적용

### 1. Git 설정 (.gitattributes)
```gitattributes
# C# 소스 파일들
*.cs text eol=crlf encoding=utf-8
*.asmdef text eol=crlf encoding=utf-8

# Unity 설정 파일들
*.unity text eol=crlf encoding=utf-8
*.prefab text eol=crlf encoding=utf-8
*.asset text eol=crlf encoding=utf-8
*.meta text eol=crlf encoding=utf-8
```

### 2. 에디터 설정 (.editorconfig)
```ini
# C# files
[*.cs]
charset = utf-8
end_of_line = crlf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true
```

### 3. Git 전역 설정
```bash
git config core.autocrlf true
git config core.quotepath false
git config gui.encoding utf-8
git config i18n.commit.encoding utf-8
git config i18n.logoutputencoding utf-8
```

## 🛠️ 기존 파일 복구 방법

### 방법 1: 전체 파일 재작성 (권장)
```csharp
// Write 도구를 사용하여 올바른 한글로 전체 파일 재작성
Write 도구로 UTF-8 인코딩으로 새로 작성
```

### 방법 2: 명령어로 인코딩 변환
```bash
# iconv를 사용한 인코딩 변환 (주의: 한글이 이미 깨진 경우 복구 불가)
iconv -f iso8859-1 -t utf-8 파일명.cs > 파일명_utf8.cs
mv 파일명_utf8.cs 파일명.cs
```

### 방법 3: MultiEdit으로 개별 수정
```csharp
// 개별 라인만 수정 (토큰 효율적)
MultiEdit 도구로 깨진 주석만 개별 복구
```

## 🚫 방지 방법

### 1. 개발 환경 설정 체크리스트

#### ✅ 프로젝트 루트에 필수 파일들
- [ ] `.gitattributes` 파일 존재
- [ ] `.editorconfig` 파일 존재
- [ ] Git 설정 완료

#### ✅ 개발 도구 설정
- [ ] Visual Studio: UTF-8 with BOM 없이 저장
- [ ] VS Code: `"files.encoding": "utf8"`
- [ ] Rider: UTF-8 설정

### 2. 코딩 규칙

#### ✅ 한글 주석 작성 시
```csharp
// ✅ 좋은 예
/// <summary>
/// 게임 상태 기본 클래스
/// </summary>
public class GameState

// ✅ 좋은 예
// 메인 메뉴 UI 활성화
gameFlowManager?.ShowMainMenu();

// ❌ 피해야 할 것
// BOM이 포함된 파일 저장
// 다른 인코딩으로 저장
```

### 3. 팀 개발 시 주의사항

#### ✅ 새 팀원 온보딩
1. `.gitattributes`와 `.editorconfig` 설명
2. Git 설정 확인
3. 에디터 인코딩 설정 확인

#### ✅ 코드 리뷰 시 체크
- 한글 주석이 깨져 있는지 확인
- 새 파일의 인코딩 확인

## 🔧 인코딩 문제 진단 도구

### 1. 파일 인코딩 일괄 검사
```bash
# 모든 C# 파일의 인코딩 확인
find . -name "*.cs" -exec file {} \; | grep -v "UTF-8"
```

### 2. 한글 주석 검증
```bash
# 한글이 포함된 파일 찾기
find . -name "*.cs" -exec grep -l "[가-힣]" {} \;
```

### 3. Git 상태 확인
```bash
# Git 인코딩 설정 확인
git config --list | grep -E "(encoding|autocrlf|quotepath)"
```

## 📋 문제 발생 시 대응 절차

### 1. 긴급 대응 (한글이 깨진 경우)
1. **즉시**: 해당 파일을 Write 도구로 전체 재작성
2. **확인**: 다른 파일들도 동일한 문제가 있는지 검사
3. **커밋**: 수정된 파일들을 즉시 커밋

### 2. 근본 해결
1. **설정 파일 확인**: `.gitattributes`, `.editorconfig`
2. **Git 설정 확인**: 인코딩 관련 설정들
3. **팀 공유**: 설정 변경 사항을 팀에 공유

### 3. 예방 조치
1. **자동화**: pre-commit hook으로 인코딩 검사
2. **문서화**: 팀 위키에 인코딩 가이드 추가
3. **교육**: 정기적인 인코딩 관련 교육

## 🎯 최종 권장사항

### ✅ DO (권장사항)
- 프로젝트 시작 시 `.gitattributes`와 `.editorconfig` 설정
- UTF-8 (BOM 없이) 인코딩 사용
- 한글 주석 적극 활용 (올바른 설정 하에서)
- 정기적인 인코딩 상태 검사

### ❌ DON'T (피해야 할 것)
- 인코딩 설정 없이 한글 주석 사용
- 서로 다른 인코딩으로 파일 저장
- 깨진 주석을 그대로 방치
- BOM이 포함된 UTF-8 사용

## 🚀 자동화 스크립트

### 인코딩 문제 일괄 검사 스크립트
```bash
#!/bin/bash
# encoding_check.sh

echo "=== Unity 프로젝트 인코딩 검사 ==="

# 1. .gitattributes 확인
if [ ! -f ".gitattributes" ]; then
    echo "❌ .gitattributes 파일이 없습니다."
else
    echo "✅ .gitattributes 파일 존재"
fi

# 2. .editorconfig 확인
if [ ! -f ".editorconfig" ]; then
    echo "❌ .editorconfig 파일이 없습니다."
else
    echo "✅ .editorconfig 파일 존재"
fi

# 3. Git 설정 확인
echo "=== Git 인코딩 설정 ==="
git config --list | grep -E "(encoding|autocrlf|quotepath)" || echo "❌ Git 인코딩 설정 부족"

# 4. UTF-8이 아닌 C# 파일 찾기
echo "=== 인코딩 문제 파일 검사 ==="
find . -name "*.cs" -exec file {} \; | grep -v "UTF-8" | head -10

echo "=== 검사 완료 ==="
```

이 가이드를 따르면 한글 인코딩 문제를 근본적으로 해결하고 앞으로 발생하지 않도록 방지할 수 있습니다.