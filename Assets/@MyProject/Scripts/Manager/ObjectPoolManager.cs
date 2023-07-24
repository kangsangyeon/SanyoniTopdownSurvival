using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    private Dictionary<GameObject, ObjectPool<GameObject>> m_ObjectPoolDict = new Dictionary<GameObject, ObjectPool<GameObject>>();

    public void Register(
        GameObject _go, int _defaultCapacity,
        System.Func<GameObject> _onCreate, System.Action<GameObject> _onDestroy = null,
        System.Action<GameObject> _onGet = null, System.Action<GameObject> _onRelease = null,
        bool _collectionCheck = true)
    {
        if (m_ObjectPoolDict.ContainsKey(_go))
            return;

        m_ObjectPoolDict.Add(_go,
            new ObjectPool<GameObject>(
                _onCreate, _onGet, _onRelease, _onDestroy
                , _collectionCheck, _defaultCapacity));
    }

    public GameObject Get(GameObject _prefab)
    {
        if (m_ObjectPoolDict.ContainsKey(_prefab) == false)
        {
            Debug.LogWarning($"등록되지 않은 프리팹입니다! {_prefab.name}");
            return null;
        }

        return m_ObjectPoolDict[_prefab].Get();
    }

    public void Release(GameObject _prefab, GameObject _instance, float _delay = 0.0f)
    {
        if (m_ObjectPoolDict.ContainsKey(_prefab) == false)
        {
            Debug.LogWarning($"등록되지 않은 프리팹입니다! {_prefab.name}");
            return;
        }

        if (_delay == 0.0f)
            m_ObjectPoolDict[_prefab].Release(_instance);
        else
            this.Invoke(() => { m_ObjectPoolDict[_prefab].Release(_instance); }, _delay);
    }

    private void OnDestroy()
    {
        foreach (var _objectPool in m_ObjectPoolDict.Values)
            _objectPool.Dispose();
    }
}