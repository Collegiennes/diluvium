using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    public delegate T ObjectConstructor();

    ObjectConstructor constructor;

    LinkedList<T> objects = new LinkedList<T>();

    public ObjectPool(ObjectConstructor constructor, int count)
    {
        this.constructor = constructor;

        for(int i = 0; i < count; i++)
            AddElement();
    }

    void AddElement()
    {
        objects.AddLast(constructor());
    }

    public void Return(T obj)
    {
        objects.AddFirst(obj);
    }

    public T Request()
    {
        LinkedListNode<T> obj = objects.First;
        if(obj != null)
        {
            objects.RemoveFirst();
            return obj.Value;
        }
        return null;
    }
}
