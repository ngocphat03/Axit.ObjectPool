# Axit Object Pool

A minimal static object pool for Unity.
## Usage

```csharp
using Axit.ObjectPool;

// A prefab is always required.
GameObject enemy = ObjectPool.Spawn(enemyPrefab, parent, position, rotation);

// Component overload keeps the component type.
Enemy enemyComponent = ObjectPool.Spawn(enemyPrefabComponent, parent, position, rotation);

ObjectPool.Recycle(enemy);
```

Optional helpers:

```csharp
ObjectPool.Prewarm(enemyPrefab, 20);
ObjectPool.Clear(enemyPrefab);
ObjectPool.ClearAll();
```
