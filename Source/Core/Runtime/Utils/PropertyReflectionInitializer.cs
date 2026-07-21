using System.Linq;
using UnityEngine;
using VRBuilder.Core.Utils;

namespace VRBuilder.Unity.Utils
{
    public static class PropertyReflectionInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PropertyReflectionHelper.ResolveRequiredComponents = type =>
            {
                return type.GetCustomAttributes(typeof(RequireComponent), false)
                    .Cast<RequireComponent>()
                    .SelectMany(rq => new[] { rq.m_Type0, rq.m_Type1, rq.m_Type2 })
                    .Where(t => t != null);
            };

            PropertyReflectionHelper.ShouldExcludeType = type =>
            {
                return type.Assembly.GetReferencedAssemblies().Any(a => a.Name == "UnityEditor");
            };
        }
    }
}