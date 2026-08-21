# Top of Slayers 체인 전투 시스템 리팩터링

## 포트폴리오 한 줄 요약

Unity 체인 전투의 규칙·타깃 탐색·피해 적용이 한 런타임 흐름에 강하게 결합되어 있던 구조를 순수 도메인 규칙과 캐시 기반 타깃 레지스트리로 분리하고, 반복 컬렉션 할당과 계층 탐색을 제거해 테스트 가능성과 프레임 안정성을 개선했다.

## 이력서/노션용 요약

- `AutoSlash → Targeting → SlashDash → Damage → Chain` 전투 파이프라인을 분석하고 체인 규칙을 순수 C# 객체로 분리해 Unity 생명주기 없이 단위 테스트할 수 있도록 개선
- 타깃 등록 시 `IDamageable`과 동일 적 판별용 Identity를 캐시해 매 쿼리의 반복 `GetComponent`/계층 탐색을 제거
- 라인 타깃 탐색을 caller-owned buffer 기반 Non-Alloc API로 변경해 활성 체인 탐색 경로의 `List` 생성을 쿼리당 1회에서 워밍업 후 0회로 축소
- 관통 공격 중복 판정을 선형 중첩 탐색에서 재사용 `HashSet`으로 변경해 평균 시간 복잡도를 O(k²)에서 O(k)로 개선
- 후보별 `Vector3.Angle`의 `acos` 연산을 방향 내적 점수 비교로 변경하고, 기존 API를 유지해 호출부 마이그레이션 위험을 최소화

## 시스템 흐름

```text
플레이어 입력
    ↓
AutoSlashController ── Non-Alloc 타깃 쿼리 ──→ TargetingSystem
    ↓                                              │
SlashDashController ←── 안정된 Target Identity ───┘
    ↓
DamageSystem ── DamageResult(Target + Identity) ──→ ChainCombatController
                                                   ↓
                                              ChainRules
                                      (순수 체인 증가/배율 규칙)
```

`TargetingSystem`이 전투 타깃 메타데이터의 단일 소유자가 되고, `DamageSystem`, `AutoSlashController`, `SlashDashController`, `ChainCombatController`가 동일한 Identity를 사용하도록 맞췄다. 이로써 한 적의 자식 히트 포인트를 공격하더라도 서로 다른 적으로 오판하지 않는다.

## 리팩터링 전 문제

### 1. 체인 규칙과 Unity 런타임 제어의 결합

`ChainCombatController`가 다음 책임을 동시에 갖고 있었다.

- 현재 체인과 직전 타깃 계산
- 다음 공격의 피해 배율 계산
- 전역 `Time.timeScale` 제어
- 코루틴 수명주기와 정지/재개 처리
- 씬 참조 탐색
- 마일스톤 이벤트 발행

핵심 규칙을 검증하려면 `MonoBehaviour`, `Transform`, 코루틴이 모두 필요했기 때문에 빠른 단위 테스트가 어려웠다. 연출이나 시간 제어를 수정할 때 체인 수치 규칙까지 함께 회귀할 위험도 있었다.

### 2. 타깃 쿼리마다 같은 메타데이터를 다시 탐색

기존 `CleanupTargets()`는 조회할 때마다 각 타깃의 `IDamageable`을 다시 찾았다. 컴포넌트가 직접 붙은 일반 경로에서도 Unity native 경계를 반복 호출하고, 루트 아래에서 찾아야 하는 경로에서는 `GetComponentsInChildren<MonoBehaviour>(true)` 배열까지 생성했다.

타깃의 피해 컴포넌트와 동일 적 판별 기준은 등록부터 해제까지 거의 변하지 않으므로, 프레임 쿼리에서 반복 계산할 이유가 없었다.

### 3. 핫 패스의 명시적 managed allocation

- `GetTargetsInLine()`은 호출할 때마다 `new List<Transform>()` 실행
- `ApplyPendingDamage()`는 공격을 확정할 때마다 `new HashSet<Transform>()` 실행
- 관통 타깃 중복 검사 중 같은 타깃의 피해 컴포넌트를 반복 탐색

`detectInterval` 기본값이 0이므로 활성 체인에서 라인 후보를 매 프레임 조회하면, 60 FPS 기준 `List` 객체만 최대 60개/초 생성될 수 있었다. 객체 하나의 크기보다도 모바일 환경에서 작은 할당이 누적되어 GC 스파이크를 만들 수 있다는 점이 문제였다.

### 4. 타깃 Identity 규칙의 중복

동일 적 판별 로직이 `TargetingSystem`, `AutoSlashController`, `SlashDashController`, `DamageSystem`에 각각 구현되어 있었다. 한쪽만 수정하면 자식 Transform을 별도 적으로 보거나 관통 피해가 중복 적용되는 등 시스템 간 판정 불일치가 생길 수 있었다.

## 핵심 설계와 구현

### A. 순수 도메인 객체 `ChainRules`

체인 증가, 동일 타깃 재공격, 다음 공격 피해 배율, 리셋을 Unity API가 없는 `ChainRules`로 추출했다.

```csharp
var transition = chainRules.RegisterHit(targetIdentityId);
var multiplier = chainRules.GetDamageMultiplier(nextTargetIdentityId);
```

`ChainCombatController`는 Damage 이벤트와 코루틴/연출을 조정하는 애플리케이션 계층으로 남고, `ChainRules`는 결정적인 입력과 출력만 다루도록 했다.

적용 원칙:

- SRP: 체인 수치 규칙과 Unity 시간/연출 오케스트레이션 분리
- Pure Domain Logic: 프레임, 씬, GameObject 없이 규칙 검증
- Tell, Don't Ask: 컨트롤러가 내부 필드를 직접 계산하지 않고 `RegisterHit`과 `GetDamageMultiplier`에 의도를 전달

### B. 등록 시 계산하는 타깃 메타데이터 캐시

`TargetingSystem`은 `TargetEntry`에 다음 값을 저장한다.

- 원본 `Transform`
- `DamageSystem.IDamageable`
- 동일 적 판별용 `Identity Transform`
- Unity Instance ID

등록 중 한 번만 `CombatTargetResolver`를 호출하고, 이후 쿼리는 캐시된 값을 사용한다. 중복 등록 확인도 `List.Contains` 대신 `Dictionary<int, TargetEntry>`로 처리한다.

트레이드오프:

- 타깃당 `TargetEntry`와 Dictionary 엔트리의 상주 메모리가 추가된다.
- 대신 매 프레임 반복되는 컴포넌트 탐색 비용과 최악 경로의 임시 배열을 제거한다.
- 현재 게임처럼 동시 타깃이 수십 개 수준이고 조회 빈도가 등록/해제보다 훨씬 높은 구조에 적합하다.

### C. Caller-owned buffer 기반 Non-Alloc 쿼리

기존 반환형 API는 호환성을 위해 유지하고, 핫 패스에는 다음 API를 추가했다.

```csharp
targetingSystem.GetTargetsInLineNonAlloc(
    origin,
    direction,
    range,
    ignoreTarget,
    linePriorityTargets);
```

버퍼 소유권이 호출자에게 있어 수명과 재사용 시점을 명확히 알 수 있다. `AutoSlashController`는 용량 64의 버퍼를 한 번 생성한 뒤 계속 비워서 사용한다.

주의할 점은 버퍼를 지연 실행 객체가 계속 참조하면 다음 프레임 쿼리에 의해 내용이 바뀔 수 있다는 것이다. 이번 변경은 결과를 같은 호출 스택에서 즉시 소비하는 라인 우선 타깃 경로에만 적용했다. Ready Delay가 보관하는 관통 목록에는 무리하게 같은 버퍼를 재사용하지 않았다.

### D. Stable Identity와 중복 피해 방지

`DamageResult`에 `TargetIdentity`를 추가했다. 화면 조준점인 `Target`과 게임 규칙상 동일 적을 뜻하는 `TargetIdentity`를 구분해 다음 문제를 해결했다.

- 서로 다른 자식 히트 포인트를 다른 체인 타깃으로 계산하는 문제
- 한 적의 여러 Transform이 관통 목록에 들어왔을 때 피해가 중복 적용되는 문제
- 체인 배율 미리보기와 실제 피격 후 체인 증가가 서로 다른 기준을 사용하는 문제

기존 3-인자 `DamageResult` 생성자는 유지해 다른 호출부의 호환성도 보존했다.

### E. 알고리즘과 수학 연산 개선

- 관통 후보 중복 검사: 후보마다 기존 목록을 다시 순회하는 방식에서 Identity `HashSet`으로 변경
- 각도 우선 타깃: 후보마다 `Vector3.Angle`을 호출하는 방식에서 정규화된 내적 점수를 비교
- Forward Cone: 후보마다 `acos`로 각도를 구하지 않고, 쿼리당 한 번 계산한 `cos(halfAngle)`과 내적을 비교

내적 기반 비교는 “정확한 각도 값”이 아니라 “어느 방향이 더 가까운가”만 필요한 문제에 역삼각함수를 계산하지 않는 선택이다.

## 성능 변화

아래 수치는 FPS를 추정한 값이 아니라 코드 경로에서 직접 확인 가능한 연산/할당 변화다.

| 구간 | Before | After | 효과 |
|---|---:|---:|---|
| 활성 체인 라인 조회 | 쿼리당 새 `List` 1개 | 워밍업 후 0개 | 프레임 반복 GC 압력 제거 |
| 타깃 상태 확인 | 조회마다 `GetComponent` 계열 호출 | 등록 시 1회 후 캐시 | Unity native 경계 호출 감소 |
| 계층 fallback 탐색 | 최악의 경우 타깃당 새 `MonoBehaviour[]` | 재사용 `List`로 등록 시 검색 | 임시 배열 제거 |
| 공격 피해 확정 | 공격당 새 `HashSet` 1개 | 재사용 Set | 반복 managed allocation 제거 |
| 관통 중복 등록 | O(k²) + 반복 resolver | 평균 O(k) | 관통 타깃 증가 시 확장성 개선 |
| 중복 등록 검사 | O(n) `List.Contains` | 평균 O(1) Dictionary | 풀링 객체 활성화 비용 감소 |
| Cone/Angle 후보 비교 | 후보당 `acos` | 내적 + 제곱근 | 고비용 역삼각 연산 제거 |

공간 탐색 자체는 여전히 O(n)이다. 현재 규모에서는 캐시와 Non-Alloc 전환이 복잡도 대비 효과가 가장 크다. 동시 활성 적이 수백~수천 단위로 증가한다면 그때 Uniform Grid, Quadtree, Physics NonAlloc 쿼리 등을 검토하는 것이 합리적이다.

## 검증

추가한 EditMode 테스트:

- 새 타깃에서만 체인이 증가하고 동일 타깃 재공격은 증가하지 않는지
- 공격 전에 계산하는 다음 타깃 배율이 정확한지
- 체인 리셋과 음수 증가율 방어가 동작하는지
- 자식 히트 포인트가 부모 `IDamageable` Identity로 해석되는지
- 사망 타깃이 다음 쿼리에서 제거되는지
- Non-Alloc 라인 쿼리가 호출자 버퍼를 비우고 올바른 후보만 반환하는지

현재 작업 환경에서 수행한 검증:

- 변경된 모든 KTJ C# 파일 Roslyn 구문 검사 통과
- `ChainRules`, `CombatTargetResolver`, `DamageSystem`, `TargetingSystem`, `ChainCombatController`와 테스트 코드 별도 컴파일: 오류 0건
- 순수 `ChainRules` 스모크 테스트 통과
- `git diff --check` 통과

전체 Unity Test Runner와 플레이 모드 회귀 테스트는 프로젝트 버전인 Unity `2022.3.43f1`에서 수행한다. 현재 머신에는 Unity `6000.3.7f1`만 있어 프로젝트 자동 업그레이드로 원본을 오염시키지 않기 위해 전체 에디터 실행은 하지 않았다.

### Unity Profiler 측정 체크리스트

포트폴리오에 실제 ms/GC 수치를 넣을 때는 같은 기기, 같은 씬, 같은 적 배치에서 Before/After를 각각 300프레임 이상 기록한다.

1. Unity 2022.3.43f1에서 동일 전투 씬 실행
2. 적 10/30/50마리 케이스를 각각 측정
3. CPU Usage의 `AutoSlashController`, `TargetingSystem` 호출 시간 비교
4. GC.Alloc의 프레임당 bytes와 spike 빈도 비교
5. Deep Profile은 원인 확인에만 사용하고 최종 수치는 일반 Profile로 기록

결과 기록 템플릿:

| 적 수 | 지표 | Before | After | 변화율 |
|---:|---|---:|---:|---:|
| 10 | Targeting CPU ms |  |  |  |
| 30 | Targeting CPU ms |  |  |  |
| 50 | Targeting CPU ms |  |  |  |
| 50 | GC.Alloc bytes/frame |  |  |  |

## 왜 더 큰 기술을 쓰지 않았는가

### ECS/Jobs/Burst를 사용하지 않은 이유

현재 병목은 데이터 병렬 계산 자체보다 반복 객체 탐색과 managed allocation에 있었다. 수십 개 타깃 규모에서 전체 전투 모델을 ECS로 옮기면 프리팹, 애니메이션, VFX, 팀 협업 비용이 크게 증가한다. 먼저 프로파일로 확인된 비용 구조를 캐시와 Non-Alloc API로 해결하는 편이 변경 위험과 효과의 균형이 좋다.

### Physics.OverlapSphereNonAlloc로 교체하지 않은 이유

현재 시스템은 등록/해제로 활성 타깃 집합을 이미 관리하며, 단순 거리뿐 아니라 Line, Cone, 동일 타깃 제외, 사망 상태, 체인 Identity가 필요하다. 물리 쿼리로 바꾸면 Collider 의존성과 다시 컴포넌트를 해석하는 비용이 생긴다. 레지스트리를 유지하면서 선택 전략만 계산하는 편이 의미 규칙을 한곳에 모을 수 있다.

### 전역 Object Pool을 새로 만들지 않은 이유

최적화 대상은 전투 도중 생기는 작은 컬렉션이었고, 이들은 컴포넌트 수명 동안 버퍼를 소유하면 충분하다. 범용 풀은 반납 누락과 소유권 문제를 추가한다. 가장 좁은 범위의 재사용 버퍼를 선택했다.

## 면접에서 설명할 핵심

### “성능이 얼마나 좋아졌나요?”

먼저 구조적으로 제거한 비용을 답한다. 활성 체인의 라인 탐색에서 쿼리당 `List` 1개, 피해 확정에서 공격당 `HashSet` 1개, 타깃 조회마다 반복되던 컴포넌트 해석을 제거했다. CPU ms/FPS는 기기와 적 수에 의존하므로 Unity Profiler의 동일 조건 Before/After 캡처로 제시하고, 측정하지 않은 수치를 만들지 않는다.

### “캐시가 오래된 값을 들고 있으면 어떻게 하나요?”

등록자는 `OnEnable`에 등록하고 `OnDisable`에 해제한다. 쿼리 전 `CleanupTargets`가 비활성, 파괴, 사망 상태를 검사하고 레지스트리와 Dictionary에서 함께 제거한다. Unity Object가 파괴된 경우도 `CombatTargetResolver.IsAlive`로 확인한다.

### “왜 반환 List 대신 버퍼를 받나요?”

Unity의 프레임 반복 경로에서는 반환 컬렉션의 소유권이 모호하면 매번 새로 만들거나 전역 공유 버퍼를 쓰게 된다. 호출자가 버퍼를 소유하면 할당 시점과 수명이 명확하고, 필요한 호출부만 안전하게 Non-Alloc으로 전환할 수 있다.

### “가장 중요한 설계 개선은 무엇인가요?”

체인 카운트 계산 자체보다 모든 전투 단계가 같은 Target Identity를 사용하도록 만든 것이다. 조준용 Transform과 규칙상 적의 Identity를 분리함으로써 체인 증가, 중복 피해, 사망 판정이 같은 기준으로 동작한다.

## 주요 변경 파일

- `ChainRules.cs`: 순수 체인 진행/피해 배율 규칙
- `CombatTargetResolver.cs`: 배열을 만들지 않는 피해 컴포넌트/Identity 해석
- `TargetingSystem.cs`: 캐시 기반 타깃 레지스트리
- `TargetingSystem.Query.cs`: Non-Alloc 라인 쿼리와 내적 기반 방향 점수
- `TargetingSystem.Strategies.cs`: 캐시 엔트리 기반 전략과 `acos` 제거
- `DamageSystem.cs`: 안정된 `TargetIdentity`를 포함하는 피해 이벤트
- `AutoSlashController.Chain.cs`: 프레임 반복 라인 쿼리의 버퍼 재사용
- `SlashDashController.Damage.cs`: 재사용 HashSet 기반 중복 피해 방지
- `Tests/Editor`: 체인 규칙과 타깃 레지스트리 EditMode 테스트

## 실행 메모

저장소의 대용량 파일은 Git LFS를 사용한다. 전체 Unity 프로젝트를 열기 전 다음 명령으로 실제 에셋을 받는다.

```powershell
git lfs pull
```

그다음 Unity 2022.3.43f1에서 프로젝트를 열고 Test Runner의 EditMode 테스트를 실행한다.
