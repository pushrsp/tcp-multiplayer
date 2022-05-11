using ServerCore;

namespace Dummy
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        public Session Generate()
        {
            return new ServerSession();
        }
    }
}