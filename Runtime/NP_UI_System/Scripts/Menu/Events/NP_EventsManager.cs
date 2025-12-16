using UnityEngine.Events;

namespace NP_UI
{
    /// Static class for common use of events.
    public static class NP_EventsManager
    {
        public static UnityEvent<NpGenericMenu> CloseMenuEvent = new UnityEvent<NpGenericMenu>();
        public static UnityEvent<NpGenericMenu> OpenMenuEvent = new UnityEvent<NpGenericMenu>();
        public static UnityEvent CloseAllMenus = new UnityEvent();
        public static UnityEvent<NpGenericMenu> OnMenuCreated = new UnityEvent<NpGenericMenu>();
    }
}
