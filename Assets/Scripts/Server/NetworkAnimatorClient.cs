using Unity.Netcode.Components;

namespace Server
{
    public class NetworkAnimatorClient : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
