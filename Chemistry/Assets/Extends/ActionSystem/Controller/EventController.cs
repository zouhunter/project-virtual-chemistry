using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
namespace WorldActionSystem
{
    /// <summary>
    /// Represents a pool of objects that we can pull from in order
    /// to prevent constantly reallocating new objects. This collection
    /// is meant to be fast, so we limit the "lock" that we use and do not
    /// track the instances that we hand out.
    /// </summary>
    /// <typeparam name="T">The type of object in the pool.</typeparam>
    internal sealed class ObjectPool<T>
        where T : new()
    {
        /// <summary>
        /// Number of items to grow the array by if needed
        /// </summary>
        private int mGrowSize = 20;

        /// <summary>
        /// Pool objects
        /// </summary>
        private T[] mPool;

        /// <summary>
        /// Index into the pool
        /// </summary>
        private int mNextIndex = 0;

        /// <summary>
        /// Initializes a new instance of the ObjectPool class.
        /// </summary>
        /// <param name="size">The size of the object pool.</param>
        public ObjectPool(int rSize)
        {
            // Initialize the pool
            Resize(rSize, false);
        }

        /// <summary>
        /// Initializes a new instance of the ObjectPool class.
        /// </summary>
        /// <param name="rSize">The initial size of the object pool.</param>
        /// <param name="rGrowize">Increment to grow the pool by when needed</param>
        public ObjectPool(int rSize, int rGrowSize)
        {
            mGrowSize = rGrowSize;

            // Initialize the pool
            Resize(rSize, false);
        }

        /// <summary>
        /// The total size of the pool
        /// </summary>
        /// <value>The length.</value>
        public int Length
        {
            get { return mPool.Length; }
        }

        /// <summary>
        /// The number of items available in the pool
        /// </summary>
        public int Available
        {
            get { return mPool.Length - mNextIndex; }
        }

        /// <summary>
        /// The number of items that have been allocated
        /// </summary>
        public int Allocated
        {
            get { return mNextIndex; }
        }

        /// <summary>
        /// Pulls an item from the object pool or creates more
        /// if needed.
        /// </summary>
        /// <returns>Object of the specified type</returns>
        public T Allocate()
        {
            T lItem = default(T);

            // Creates extra items if needed
            if (mNextIndex >= mPool.Length)
            {
                if (mGrowSize > 0)
                {
                    Resize(mPool.Length + mGrowSize, true);
                }
                else
                {
                    return lItem;
                }
            }

            // Returns the item. For performance, we'll use an if
            // statement instead of a try-catch block.
            if (mNextIndex >= 0 && mNextIndex < mPool.Length)
            {
                lItem = mPool[mNextIndex];
                mNextIndex++;
            }

            return lItem;
        }

        /// <summary>
        /// Sends an item back to the pool.
        /// </summary>
        /// <param name="rInstance">Object to return</param>
        public void Release(T rInstance)
        {
            if (mNextIndex > 0)
            {
                mNextIndex--;
                mPool[mNextIndex] = rInstance;
            }
        }

        /// <summary>
        /// Rebuilds the pool with new instances
        /// 
        /// Note:
        /// This is a fast pool so we don't track the instances
        /// that are handed out. Releasing an instance also overwrites
        /// what was there. That means we can't have a "ReleaseAll"
        /// function that allows the array to be used again. The best
        /// we can do is abandon what we have given out and rebuild all our instances.
        /// </summary>
        /// <param name="rInstance">Object to return</param>
        public void Reset()
        {
            // Determine the length to initialize
            int lLength = mGrowSize;
            if (mPool != null) { lLength = mPool.Length; }

            // Rebuild our elements
            Resize(lLength, false);

            // Reset the pool stats
            mNextIndex = 0;
        }

        /// <summary>
        /// Resize the pool array
        /// </summary>
        /// <param name="rSize">New size of the pool</param>
        /// <param name="rCopyExisting">Determines if we copy contents from the old pool</param>
        public void Resize(int rSize, bool rCopyExisting)
        {
            lock (this)
            {
                int lCount = 0;

                // Build the new array and copy the contents
                T[] lNewPool = new T[rSize];

                if (mPool != null && rCopyExisting)
                {
                    lCount = mPool.Length;
                    Array.Copy(mPool, lNewPool, Math.Min(lCount, rSize));
                }

                // Allocate items in the new array
                for (int i = lCount; i < rSize; i++)
                {
                    lNewPool[i] = new T();
                }

                // Replace the old array
                mPool = lNewPool;
            }
        }
    }
    internal interface IEventItem
    {
        object Action { get; }
        void Release();
    }

    internal class EventItem : IEventItem
    {
        public UnityAction action;
        public object Action
        {
            get { return action; }
        }
        private static ObjectPool<EventItem> sPool = new ObjectPool<EventItem>(1, 1);

        public static EventItem Allocate(UnityAction action)
        {
            var item = sPool.Allocate();
            item.action = action;
            return item;
        }
        public void Release()
        {
            sPool.Release(this);
        }
    }

    internal class EventItem<T> : IEventItem
    {
        public UnityAction<T> action;
        public object Action
        {
            get { return action; }
        }
        private static ObjectPool<EventItem<T>> sPool = new ObjectPool<EventItem<T>>(1, 1);

        public static EventItem<T> Allocate(UnityAction<T> action)
        {
            var item = sPool.Allocate();
            item.action = action;
            return item;
        }
        public void Release()
        {
            sPool.Release(this);
        }
    }

    public class EventController
    {
        private IDictionary<string, List<IEventItem>> m_observerMap;

        public UnityAction<string> MessageNotHandled { get; set; }

        public EventController()
        {
            m_observerMap = new Dictionary<string, List<IEventItem>>();
        }
        public void Clean()
        {
            m_observerMap.Clear();
        }
        public void NoMessageHandle(string rMessage)
        {
            if (MessageNotHandled == null)
            {
                Debug.LogWarning("MessageDispatcher: Unhandled Message of type " + rMessage);
            }
            else
            {
                MessageNotHandled(rMessage);
            }
        }

        #region 注册注销事件
        public void AddDelegate<T>(string key, UnityAction<T> handle)
        {
            if (handle == null) return;
            EventItem<T> observer = EventItem<T>.Allocate(handle);
            RegisterObserver(key, observer);
        }
        public void AddDelegate(string key, UnityAction handle)
        {
            if (handle == null) return;
            EventItem observer = EventItem.Allocate(handle);
            RegisterObserver(key, observer);
        }

        public void RemoveDelegate<T>(string key, UnityAction<T> handle)
        {
            ReMoveObserver(key, handle);
        }
        public void RemoveDelegate(string key, UnityAction handle)
        {
            ReMoveObserver(key, handle);
        }
        public void RemoveDelegates(string key)
        {
            if (m_observerMap.ContainsKey(key))
            {
                m_observerMap.Remove(key);
            }
        }
        #endregion

        #region 触发事件
        public void NotifyObserver(string key)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                foreach (var item in list)
                {
                    if (item is EventItem)
                    {
                        (item as EventItem).action.Invoke();
                    }
                    else
                    {
                        NoMessageHandle(key);
                    }
                }

            }
            else
            {
                NoMessageHandle(key);
            }
        }
        public void NotifyObserver<T>(string key, T value)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                var actions = list.FindAll(x => x is EventItem<T>);
                foreach (var item in actions)
                {
                    (item as EventItem<T>).action.Invoke(value);
                }
            }
            else
            {
                NoMessageHandle(key);
            }
        }
        #endregion

        private void RegisterObserver(string key, IEventItem observer)
        {
            if (m_observerMap.ContainsKey(key))
            {
                if (!m_observerMap[key].Contains(observer))
                {
                    m_observerMap[key].Add(observer);
                }
            }
            else
            {
                m_observerMap.Add(key, new List<IEventItem>() { observer });
            }
        }

        private bool ReMoveObserver(string key, object handle)
        {
            if (handle == null) return false;
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                var item = list.Find(x => object.Equals(x.Action, handle));
                if (item != null)
                {
                    item.Release();
                    list.Remove(item);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }

}