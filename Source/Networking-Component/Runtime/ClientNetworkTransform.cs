using Unity.Netcode.Components;

namespace VRBuilder.Networking
{
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
