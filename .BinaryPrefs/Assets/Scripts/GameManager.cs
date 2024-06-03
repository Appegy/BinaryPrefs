using System.IO;
using UnityEngine;

namespace Appegy.BinaryStorage.Example
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("Game started");

            using (var storage = BinaryPrefs.Get(Path.Combine(Application.persistentDataPath, "prefs.bin")))
            {
                storage.Set("key_s", "str");
                storage.Set("key_i", 10);

                Debug.Log(storage.Has("key_s"));
                Debug.Log(storage.Has("key_i"));

                Debug.Log(storage.Get<string>("key_s"));
                Debug.Log(storage.Get<int>("key_i"));
            }
        }
    }
}