using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Core
{
    public class GameObjectPoolManager : MonoBehaviour
    {
        public GameObject TemplateGameObject;
        public List<GameObject> InactivePool = new ();

        private void Awake()
        {
            TemplateGameObject.SafeSetActive(false);
            TemplateGameObject.transform.SetParent(transform);
        }

        public void ReturnToPool(GameObject poolObject)
        {
            poolObject.transform.SetParent(transform);
            poolObject.gameObject.SafeSetActive(false);
            InactivePool.Add(poolObject);
        }

        public GameObject GetPooledObject()
        {
            GameObject result = null;

            if (InactivePool.Count > 0)
            {
                result = InactivePool[0];
                InactivePool.RemoveAt(0);
            }
            else
            {
                result = Instantiate<GameObject>(TemplateGameObject, transform);
            }
            return result;
        }

        public T GetPooledObject<T>()
        {
            var pooledObject = GetPooledObject();
            if (!pooledObject.TryGetComponent<T>(out T typedPooledObject))
            {
                Debug.LogError($"GameObjectPoolManager [{gameObject.name}] not set up correctly, missing {typeof(T)} in its template");
            }
            return typedPooledObject;
        }
    }
}