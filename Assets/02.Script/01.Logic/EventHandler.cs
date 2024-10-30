using System.Collections.Generic;
using UnityEngine;

namespace SCC.UTIL
{
    //!<==========================================================================================
    public class EventHandler<T>
    {
        //!<===================================================================================

        private List<System.Action<T>> Receiver = new List<System.Action<T>>();

        //!<===================================================================================

        public int DirtyCount { get; private set; }
        public bool IsNeedUpdate => this.DirtyCount > 0;

        //!<===================================================================================

        public event System.Action<T> Handler
        {
            add
            {
                if (this.Receiver.Contains(value) == false)
                {
                    this.Receiver.Add(value);
                }
            }
            remove
            {
                if (this.Receiver.Count > 0)
                {
                    this.Receiver.Remove(value);
                }
            }
        }

        public EventHandler<T> OnNeedUpdate()
        {
            this.DirtyCount++;
            return this;
        }

        public void OnFire(T value)
        {
            this.DirtyCount = 0;

            if (this.Receiver.Count > 0)
            {
                for (var i = 0; i < this.Receiver.Count; ++i)
                {
                    var c = this.Receiver[i];
                    if (c != null)
                    {
                        c.Invoke(value);
                    }
                }
            }
        }
        public void OnClear()
        {
            this.DirtyCount = 0;
            this.Receiver.Clear();
        }
    }
    //!<==========================================================================================
    public class EventHandler
    {
        //!<===================================================================================

        private List<System.Action> Receiver = new List<System.Action>();

        //!<===================================================================================

        public int DirtyCount { get; private set; }
        public bool IsNeedUpdate => this.DirtyCount > 0;
        //!<===================================================================================

        public event System.Action Handler
        {
            add
            {
                if (this.Receiver.Contains(value) == false)
                {
                    this.Receiver.Add(value);
                }
            }
            remove
            {
                if (this.Receiver.Count > 0)
                {
                    this.Receiver.Remove(value);
                }
            }
        }
        public EventHandler OnNeedUpdate()
        {
            this.DirtyCount++;
            return this;
        }

        public void OnFire()
        {
            this.DirtyCount = 0;

            if (this.Receiver.Count > 0)
            {
                for (var i = 0; i < this.Receiver.Count; ++i)
                {
                    var c = this.Receiver[i];
                    if (c != null)
                    {
                        c.Invoke();
                    }
                }
            }
        }
        public void OnClear()
        {
            this.DirtyCount = 0;
            this.Receiver.Clear();
        }
    }

    //!<==========================================================================================
    //IEventHandlTrigger
    //!<==========================================================================================
    public interface IEventHandlDispatcher<T> where T : System.IConvertible
    {
        IEventHandlDispatcher<T> OnNeedUpdate(T type);
    }
    //!<==========================================================================================
    //VoidEventHandlerGroup
    //!<==========================================================================================
    public class VoidEventHandlerGroup<T> : IEventHandlDispatcher<T> where T : System.IConvertible
    {
        //!<=======================================================================================

        private Dictionary<T, EventHandler> _handler = new Dictionary<T, EventHandler>();

        //!<=======================================================================================

        public EventHandler GetHandler(T type)
        {
            if (this._handler.TryGetValue(type, out var handler) == false)
            {
                //!< log
                Debug.LogError($"ERROR= EventHandler Not Contain {type}");
            }
            return handler;
        }
        public EventHandler AddHandler(T type)
        {
            if (this._handler.ContainsKey(type) == false)
            {
                var handler = new EventHandler();

                this._handler.Add(type, handler);

                return handler;
            }

            return null;
        }
        public EventHandler OnNeedUpdate(T type)
        {
            var hdl = this.GetHandler(type);
            if (hdl != null)
            {
                hdl.OnNeedUpdate();
                return hdl;
            }
            return null;
        }
        public VoidEventHandlerGroup<T> OnAddNeedUpdate(T type)
        {
            var hdl = this.GetHandler(type);
            if (hdl != null)
            {
                hdl.OnNeedUpdate();
            }
            return this;
        }
        public void OnFireNotify()
        {
            foreach (var hdl in this._handler)
            {
                if (hdl.Value.DirtyCount > 0)
                {
                    hdl.Value.OnFire();
                }
            }
        }

        IEventHandlDispatcher<T> IEventHandlDispatcher<T>.OnNeedUpdate(T type)
        {
            this.OnNeedUpdate(type);
            return this;
        }
    }

    //!<==========================================================================================
}
