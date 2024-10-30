using UnityEngine;

namespace SCC
{
    //!<===============================================================================
    //PreLoadedInternalResourcesData
    //!<===============================================================================
    [CreateAssetMenu(fileName = "PreLoadedResourcesData", menuName = "SCC/Resources")]
    public class PreLoadedInternalResourcesData : ScriptableObject
    {
        //!<============================================================================

        [field: SerializeField] public Sprite PossessionGold            { get; protected set; }
        [field: SerializeField] public Sprite ErrorIcon                 { get; protected set; }
        [field: SerializeField] public Material UIMaterialGray          { get; protected set; }
        [field: SerializeField] public Material UIMaterialColorFill     { get; protected set; }

        //!<============================================================================
    }
}
