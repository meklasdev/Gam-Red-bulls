using System;

// Minimalne atrapy typów UnityEngine potrzebne do kompilacji poza silnikiem.
namespace UnityEngine
{
    // Pusta baza zachowująca referencję do GameObjectu.
    public class MonoBehaviour
    {
        public GameObject gameObject { get; } = new GameObject();
        public T GetComponent<T>() where T : new() => new T();
    }

    public class GameObject
    {
        public bool activeSelf;
        public void SetActive(bool active) => activeSelf = active;
    }

    public class Transform { }

    public class Collider
    {
        public bool isTrigger;
        public Transform transform = new Transform();
    }

    public static class Time
    {
        public static float time;
    }

    // Atrybuty używane w skryptach.
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequireComponent : Attribute
    {
        public RequireComponent(Type type) { }
    }
}
