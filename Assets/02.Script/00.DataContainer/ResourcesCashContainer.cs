using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SCC.UTIL;
using UnityEngine;

namespace SCC.Foundation
{
    //!<=================================================================================
    //ResourcesCashContainer
    //!<=================================================================================
    public class ResourcesCashContainer<OBJECT_TYPE> where OBJECT_TYPE : UnityEngine.Object
    {
        //!<==============================================================================
        public enum ResourcesLocation
        {
            Internal = 0,
            Addressble,
        }

        protected IResources Context;
        protected Dictionary<string, OBJECT_TYPE> Resources = new Dictionary<string, OBJECT_TYPE>();
        protected HashSet<string> NullResources = new HashSet<string>();
        protected HashSet<string> ExtraResources = new HashSet<string>();
        //!<==============================================================================

        public ResourcesCashContainer(IResources resources)
        {
            this.Context = resources;
        }

        //!<==============================================================================
        //DoCleanUp
        //!<==============================================================================
        public void DoCleanUp()
        {
            this.Resources?.Clear();
            this.NullResources?.Clear();

            if (this.Context != null)
            {
                if (this.ExtraResources.Count > 0)
                {
                    foreach (var address in this.ExtraResources)
                    {
                        this.Context.ReleaseExtraAsset(address);
                    }
                }
            }

            this.ExtraResources.Clear();
        }
        //!<==============================================================================
        //AddResources
        //!<==============================================================================
        public OBJECT_TYPE Add(string name, OBJECT_TYPE obj)
        {
            if (this.Resources.ContainsKey(name) == false)
            {
                this.Resources.Add(name, obj);

                if (this.NullResources.Contains(name) == true)
                {
                    this.NullResources.Remove(name);
                }
                return obj;
            }
            else
            {
                return null;
            }
        }
        //!<==============================================================================
        //TryGetInternalResources
        //!<==============================================================================
        public OBJECT_TYPE TryGetInternal(string name, bool errormsg = true)
        {
            if (this.Resources.TryGetValue(name, out var res) == false)
            {
                if (this.IsNullResources(name) == false)
                {
                    if (res = this.Context.LoadInternalResource<OBJECT_TYPE>(name))
                    {
                        this.Resources.Add(name, res);
                    }
                    else
                    {
                        this.NullResources ??= new HashSet<string>();
                        this.NullResources.Add(name);
                    }
                }
            }

            if (res == null && errormsg == true)
            {
                Debug.LogErrorFormat("ResourcesCashUtil : {0} is not exist ", name);
            }

            return res;
        }
        //!<==============================================================================
        //TryGetExtraAsync
        //!<==============================================================================

        public async UniTask<OBJECT_TYPE> TryGetExtraAsync(string name, bool errormsg = true)
        {
            if (this.Resources.TryGetValue(name, out var res) == false)
            {
                if (this.IsNullResources(name) == false)
                {
                    var locker = AsyncLock.Get(name);

                    using (await locker.LockAsync())
                    {
                        if (this.Resources.ContainsKey(name) == true)
                        {
                            return await this.TryGetExtraAsync(name, errormsg);
                        }

                        var loadres = await this.Context
                            .LoadExtraResourceAsync<OBJECT_TYPE>(name, false);
                        if (loadres != null)
                        {
                            if (this.ExtraResources.Contains(name) == false)
                            {
                                this.ExtraResources.Add(name);
                            }

                            this.Resources.Add(name, loadres);
                            res = loadres;
                        }
                        else
                        {
                            this.NullResources ??= new HashSet<string>();
                            this.NullResources.Add(name);
                        }
                    }
                }
            }

            if (res == null && errormsg == true)
            {
                Debug.LogErrorFormat("ResourcesCashUtil : {0} is not exist ", name);
            }
            return res;
        }
        //!<==============================================================================
        //IsNullResources
        //!<==============================================================================
        public bool IsNullResources(string name)
        {
            return this.NullResources != null ?
                this.NullResources.Contains(name) : false;
        }

        //!<==============================================================================
    }
}
