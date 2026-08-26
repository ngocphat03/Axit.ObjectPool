# Axit Object Pool

A minimal static object pool for Unity.

- No asset loading.
- No installer or scene setup.
- The caller always supplies the prefab.

## Installation

### Unity Package Manager

1. Open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/ngocphat03/Axit.ObjectPool.git#release
```

### manifest.json

Add the package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.axit.objectpool": "https://github.com/ngocphat03/Axit.ObjectPool.git#release"
  }
}
```

## Usage

Import the namespace:

```csharp
using Axit.ObjectPool;
```

### Spawn a GameObject

The prefab is required. A pool is created automatically the first time that prefab is used.

```csharp
GameObject enemy = ObjectPool.Spawn(
    enemyPrefab,
    parent,
    position,
    rotation
);
```

Optional parameters can be omitted:

```csharp
GameObject enemy = ObjectPool.Spawn(enemyPrefab);
```

### Spawn a Component

When a component prefab is supplied, the same component type is returned:

```csharp
Enemy enemy = ObjectPool.Spawn(enemyPrefabComponent);
```

### Recycle

Return an instance to its original pool:

```csharp
ObjectPool.Recycle(enemy);
```

Both `GameObject` and `Component` are supported.

### Prewarm

Create inactive instances before gameplay to avoid runtime Instantiate spikes:

```csharp
ObjectPool.Prewarm(enemyPrefab, 20);
```

Without `Prewarm`, the first `Spawn` creates a default capacity of 10 instances.

### Clear

Destroy the pool associated with one prefab:

```csharp
ObjectPool.Clear(enemyPrefab);
```

Destroy all pools:

```csharp
ObjectPool.ClearAll();
```

## API

| Method | Description |
| --- | --- |
| `Spawn(prefab, parent, position, rotation)` | Gets an instance and activates it. |
| `Recycle(instance)` | Deactivates and returns an instance to its pool. |
| `Prewarm(prefab, count)` | Ensures that a pool has the requested capacity. |
| `Clear(prefab)` | Destroys the pool associated with a prefab. |
| `ClearAll()` | Destroys all pools. |

## Requirements

- Unity 2019.1 or newer.
