using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class NetworkObjectPool : Fusion.Behaviour, INetworkObjectProvider
{

    private Dictionary<NetworkPrefabId, Stack<NetworkObject>> _cached = new(32);
    private Dictionary<NetworkObject, NetworkPrefabId> _borrowed = new();

    NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        if (_cached.TryGetValue(context.PrefabId, out var objects) == false)
        {
            objects = _cached[context.PrefabId] = new Stack<NetworkObject>();
        }

        if (objects.Count > 0)
        {
            var oldInstance = objects.Pop();
            _borrowed[oldInstance] = context.PrefabId;

#if UNITY_EDITOR
            var originalPrefab = runner.Config.PrefabTable.Load(context.PrefabId, true);
            oldInstance.name = originalPrefab.name;
#endif

            oldInstance.gameObject.SetActive(true);

            result = oldInstance;
            return NetworkObjectAcquireResult.Success;
        }

        var original = runner.Config.PrefabTable.Load(context.PrefabId, true);
        if (original == null)
        {
            result = default;
            return NetworkObjectAcquireResult.Failed;
        }

        var instance = Instantiate(original);
        runner.MoveToRunnerScene(instance.gameObject);

#if UNITY_EDITOR
        instance.name = original.name;
#endif

        _borrowed[instance] = context.PrefabId;

        AssignContext(instance);

        for (int i = 0; i < instance.NestedObjects.Length; i++)
        {
            AssignContext(instance.NestedObjects[i]);
        }

        result = instance;
        return NetworkObjectAcquireResult.Success;
    }

    void INetworkObjectProvider.ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        if (context.IsNestedObject == true)
            return;

        NetworkObject instance = context.Object;
        if (instance == null)
            return;

        if (instance.NetworkTypeId.IsSceneObject == false && runner.IsShutdown == false)
        {
            if (_borrowed.TryGetValue(instance, out var prefabID) == true)
            {
                _borrowed.Remove(instance);
                _cached[prefabID].Push(instance);

                instance.gameObject.SetActive(false);
                instance.transform.parent = null;
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
                instance.name = $"(Cached) {instance.name}";
#endif
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }

    private void AssignContext(NetworkObject instance)
    {
        // for (int i = 0, count = instance.NetworkedBehaviours.Length; i < count; i++)
        // {
        //     if (instance.NetworkedBehaviours[i] is IContextBehaviour cachedBehaviour)
        //     {
        //         cachedBehaviour.Context = Context;
        //     }
        // }
    }
}

