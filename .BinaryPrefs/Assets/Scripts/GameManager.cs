using System.IO;
using UnityEngine;

namespace Appegy.BinaryStorage.Example
{
    public class GameManager : MonoBehaviour
    {
        private BinaryPrefs _prefs;

        private void Awake()
        {
            Debug.Log("Game started");
            _prefs = BinaryPrefs.Get(Path.Combine(Application.persistentDataPath, "PlayerPrefs.bin"));
        }

        private void OnGUI()
        {
            var value = _prefs.Get<int>("int_val", 0);
            if (GUILayout.Button($"INT={value}"))
            {
                _prefs.Set("int_val", value + 1);
            }
        }

        private void OnDestroy()
        {
            _prefs?.Dispose();
            _prefs = null;
        }
    }
}