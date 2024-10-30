using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SCC
{

    //!<================================================================================
    //AppContextBehaviour
    public class AppContextBehaviour : MonoBehaviour
    {
        public virtual void OnCreate() { }
        public virtual void OnStartup(IAppContext app) { }
        public virtual void CheckInit() { }
        public virtual void OnGameStart(IAppContext app) { }
        public virtual void DoCleanUp() { }
        public virtual void OnBeforeApplicationPause(bool pause) { }
        public virtual void OnBeforeAplicationQuit() { }
        public virtual void OnBeforeAplicationRestart() { }
    }
    //!<================================================================================
}