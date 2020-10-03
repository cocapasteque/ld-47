using UnityEngine;

namespace Utils
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
	
        #region Fields

        /// <summary>
        /// The instance.
        /// </summary>
        private static T s_instance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if ( s_instance == null )
                {
                    s_instance = FindObjectOfType<T> ();
                    if ( s_instance == null )
                    {
                        GameObject obj = new GameObject ();
                        obj.name = typeof ( T ).Name;
                        s_instance = obj.AddComponent<T> ();
                    }
                }
                return s_instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake ()
        {
            if ( s_instance == null )
            {
                s_instance = this as T;
                DontDestroyOnLoad ( gameObject );
            }
            else
            {
                Destroy ( gameObject );
            }
        }

        #endregion
	
    }
}