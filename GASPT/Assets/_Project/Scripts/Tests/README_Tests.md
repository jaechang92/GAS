# 스컬 시스템 테스트 가이드

## 📋 개요
이 문서는 GASPT 프로젝트의 스컬 시스템 테스트 코드에 대한 가이드입니다.

## 🗂️ 테스트 구조

```
Assets/_Project/Scripts/Tests/
├── Unit/                          # 단위 테스트
│   └── SkullManagerTests.cs       # SkullManager 단위 테스트
├── Integration/                   # 통합 테스트
│   └── SkullSystemIntegrationTests.cs # 시스템 통합 테스트
├── Performance/                   # 성능 테스트
│   └── SkullSystemPerformanceTests.cs # 성능 및 스트레스 테스트
├── Mocks/                         # 모킹 객체
│   └── MockSkullController.cs     # 테스트용 스컬 컨트롤러
├── TestRunner.cs                  # 통합 테스트 러너
├── TestConfiguration.cs           # 테스트 설정
└── README_Tests.md               # 이 문서
```

## 🧪 테스트 종류

### 1. 단위 테스트 (Unit Tests)
- **목적**: 개별 클래스와 메서드의 기능 검증
- **대상**: SkullManager, BaseSkull, SkullData 등
- **실행 방법**: Unity Test Runner 또는 F1 키

**주요 테스트 항목:**
- SkullManager 초기화 검증
- 스컬 추가/제거 기능
- 슬롯 관리 기능
- 스컬 검색 기능
- 상태 관리 기능

### 2. 통합 테스트 (Integration Tests)
- **목적**: 시스템 간 상호작용 검증
- **대상**: SkullSystem + GAS_Core + FSM_Core 연동
- **실행 방법**: F2 키

**주요 테스트 항목:**
- 스컬 교체 시 GAS 리소스 동기화
- 이벤트 발생 순서 검증
- 어빌리티 시스템 연동
- 동시성 및 안정성
- 오류 처리 및 복구

### 3. 성능 테스트 (Performance Tests)
- **목적**: 시스템 성능 측정 및 최적화 검증
- **실행 방법**: F3 키

**주요 테스트 항목:**
- 스컬 교체 실행 시간 (목표: < 16ms)
- 어빌리티 실행 시간 (목표: < 8ms)
- 메모리 할당 및 누수 검사
- FPS 영향도 측정 (목표: < 20%)
- 스트레스 테스트 (1000회 연속 교체)

## 🎮 테스트 실행 방법

### Unity Editor에서 실행

1. **테스트 러너 사용**
   - TestRunner 컴포넌트를 씬에 추가
   - 플레이 모드에서 키보드로 테스트 실행
   - GUI를 통한 시각적 결과 확인

2. **Unity Test Runner 사용**
   - Window > General > Test Runner
   - PlayMode 탭에서 테스트 선택
   - Run All 또는 개별 테스트 실행

3. **키보드 단축키**
   ```
   F1: 단위 테스트
   F2: 통합 테스트
   F3: 성능 테스트
   F9: 전체 테스트
   F10: 결과 저장
   F11: 결과 초기화
   F12: 상세/간단 보기 전환
   ```

### 빌드에서 실행

1. TestRunner가 포함된 씬을 빌드에 포함
2. 빌드 실행 후 키보드 단축키로 테스트
3. 결과는 Application.persistentDataPath에 JSON 파일로 저장

## 📊 성능 기준

### 실행 시간 기준
- **스컬 교체**: 평균 < 16ms (60FPS 기준 1프레임)
- **기본 공격**: 평균 < 8ms
- **보조 공격**: 평균 < 8ms
- **궁극기**: 평균 < 8ms
- **스컬 던지기**: 평균 < 8ms

### 메모리 기준
- **단일 교체 메모리 할당**: < 1MB
- **100회 교체 후 메모리 증가**: < 2MB
- **메모리 누수**: 감지되지 않아야 함

### FPS 영향도 기준
- **스컬 시스템 활성화 시 FPS 저하**: < 20%
- **스트레스 테스트 중 FPS 저하**: < 30%

## 🔧 테스트 설정

### TestConfiguration.asset 설정
- **성능 임계값**: 위 기준에 따라 설정
- **스트레스 테스트**: 1000회 반복, 10개 동시 스컬
- **결과 저장**: JSON 형식으로 자동 저장
- **로깅**: 상세 로그 활성화 가능

### 모킹 객체 사용
- **MockSkullController**: 실제 스컬 없이 테스트 가능
- **호출 추적**: 메서드 호출 횟수 및 파라미터 기록
- **예외 시뮬레이션**: 오류 상황 테스트 가능
- **지연 시뮬레이션**: 비동기 작업 테스트 가능

## 📈 테스트 결과 분석

### 자동 생성되는 리포트
1. **실행 시간 통계**: 평균, 최대, 95퍼센타일
2. **성공률**: 카테고리별 성공/실패 비율
3. **성능 메트릭**: 메모리 사용량, FPS 영향도
4. **오류 상세**: 실패한 테스트의 상세 정보

### 리포트 파일 위치
- **Windows**: `%USERPROFILE%/AppData/LocalLow/[CompanyName]/[ProductName]/`
- **Mac**: `~/Library/Application Support/[CompanyName]/[ProductName]/`
- **Linux**: `~/.config/unity3d/[CompanyName]/[ProductName]/`

## 🛠️ 트러블슈팅

### 일반적인 문제들

1. **테스트가 실행되지 않음**
   - Unity Test Framework 패키지 설치 확인
   - Assembly Definition 파일 설정 확인
   - 테스트 어셈블리 참조 확인

2. **성능 테스트 실패**
   - 다른 프로세스로 인한 시스템 부하 확인
   - Unity Editor에서는 빌드보다 느릴 수 있음
   - 프로파일러로 병목 지점 확인

3. **메모리 누수 감지**
   - 이벤트 구독 해제 확인
   - 생성된 객체의 적절한 정리 확인
   - WeakReference 사용 고려

4. **통합 테스트 불안정**
   - 비동기 작업의 완료 대기 시간 조정
   - 테스트 순서에 따른 상태 의존성 제거
   - 각 테스트 간 상태 초기화

### 디버깅 팁

1. **로그 활성화**
   ```csharp
   TestConfiguration.enableDetailedLogging = true;
   ```

2. **개별 테스트 실행**
   ```csharp
   // 특정 테스트만 실행
   [Test, Category("Debug")]
   public void 디버깅용_테스트() { ... }
   ```

3. **성능 프로파일링**
   - Unity Profiler 사용
   - Deep Profile 모드 활성화
   - Memory Profiler 패키지 사용

## 🎯 베스트 프랙티스

### 테스트 작성 시
1. **AAA 패턴 준수**: Arrange, Act, Assert
2. **의미 있는 테스트명**: 기능과 예상 결과를 명확히 표현
3. **독립적인 테스트**: 다른 테스트에 의존하지 않아야 함
4. **적절한 범위**: 한 번에 하나의 기능만 테스트

### 성능 테스트 시
1. **워밍업 수행**: 초기 JIT 컴파일 등의 영향 제거
2. **여러 번 측정**: 평균값으로 안정적인 결과 확보
3. **가비지 컬렉션 고려**: 테스트 전후 GC 실행
4. **환경 일관성**: 동일한 조건에서 반복 측정

### CI/CD 통합 시
1. **자동 실행**: 빌드 파이프라인에 테스트 단계 포함
2. **결과 저장**: 테스트 결과를 아티팩트로 보관
3. **실패 시 중단**: 중요한 테스트 실패 시 배포 중단
4. **성능 회귀 감지**: 이전 빌드 대비 성능 저하 알림

## 📚 참고 자료

- [Unity Test Framework 공식 문서](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [Unity Performance Testing 가이드](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@latest)
- [NUnit 테스트 프레임워크](https://nunit.org/)
- [Unity Memory Profiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@latest)

---

**작성일**: 2025-09-29
**버전**: 1.0
**작성자**: Claude Code Assistant