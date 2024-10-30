using System.Collections.Generic;
using System.Linq;

namespace SCC
{

    //!<============================================================================================
    //NetClientLogic
    //!<============================================================================================
    public partial class NetClientLogic : AppContextBehaviour
    {
        //!<=========================================================================================

        public readonly static string UserDbSetFileName = "DbSet";

        //!<=========================================================================================

        protected bool _IsActiveLogic = false;
        protected bool _IsInitialized = false;

        //!<=========================================================================================

        public bool IsActiveLogic
        {
            get => this != null &&
                    this.gameObject != null &&
                    this.gameObject.activeInHierarchy == true &&
                    this._IsActiveLogic == true;
            protected set => this._IsActiveLogic = value;
        }

        //!<=========================================================================================
        public override void OnCreate() 
        {

        }
        public override void OnStartup(IAppContext app)
        {
            
        }
        public override void CheckInit()
        {
            if(this._IsInitialized == false)
            {
                this.Initialized();
            }
        }
        public override void OnGameStart(IAppContext app)
        {

        }
        public override void DoCleanUp() 
        {

        }
        public override void OnBeforeApplicationPause(bool pause) 
        {

        }
        public override void OnBeforeAplicationQuit() 
        {
            if (this.IsActiveLogic == true)
            {
                this.IsActiveLogic = false;
            }
        }
        public override void OnBeforeAplicationRestart() 
        {

        }
      
        //!<=========================================================================================
        //Initialized
        //!<=========================================================================================
        protected void Initialized()
        {
            if (this._IsInitialized == false)
            {
                this._IsInitialized = true;

                this.IsActiveLogic = true;
            }
        }

        //!<=========================================================================================
        //
        //!<=========================================================================================
    }

    //!<=============================================================================================
}