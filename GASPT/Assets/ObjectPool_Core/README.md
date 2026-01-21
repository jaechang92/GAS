# ObjectPool_Core

Unity용 범용 오브젝트 풀링 시스템

## 특징

- **제네릭 설계**: 모든 Component 타입 풀링 가능
- **자동 확장**: 풀 부족 시 자동 생성 옵션
- **IPoolable 인터페이스**: 풀링 이벤트 콜백
- **중앙 관리**: PoolManager로 여러 풀 관리

## 구조

```
ObjectPool_Core/
├── ObjectPool.cs       # 제네릭 오브젝트 풀
├── PoolManager.cs      # 중앙 풀 매니저
└── README.md
```

## 사용 방법

### 1. 직접 풀 사용

```csharp
// 풀 생성
var bulletPool = new ObjectPool<Bullet>(bulletPrefab, transform, 20, true);

// 오브젝트 가져오기
Bullet bullet = bulletPool.Get(spawnPosition, Quaternion.identity);

// 오브젝트 반환
bulletPool.Release(bullet);

// 모든 오브젝트 반환
bulletPool.ReleaseAll();
```

### 2. PoolManager 사용

```csharp
// 풀에서 가져오기 (자동 생성)
var bullet = PoolManager.Instance.Get(bulletPrefab, spawnPosition, rotation);

// 풀로 반환
PoolManager.Instance.Release(bulletPrefab, bullet);
```

### 3. IPoolable 구현

```csharp
public class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawn()
    {
        // 풀에서 꺼날 때 호출 (초기화)
        damage = baseDamage;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public void OnDespawn()
    {
        // 풀로 반환될 때 호출 (정리)
        StopAllCoroutines();
    }
}
```

### 4. 풀 정보

```csharp
PoolInfo info = bulletPool.GetInfo();
Debug.Log($"활성: {info.activeCount}, 대기: {info.availableCount}");
```

## 의존성

없음 (독립 패키지)

## 다른 프로젝트에서 사용

1. `ObjectPool_Core` 폴더 복사
2. 풀링할 오브젝트에 `IPoolable` 구현 (선택)

## 라이선스

MIT License
