using Unity.Netcode.Components;

namespace VRBuilder.Networking
{
    public class ClientNewtorkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
