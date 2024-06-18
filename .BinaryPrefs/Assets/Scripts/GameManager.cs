using System.IO;
using UnityEngine;

namespace Appegy.Storage.Example
{
    public class GameManager : MonoBehaviour
    {
        private BinaryStorage _storage;

        private void Awake()
        {
            Debug.Log("Game started");
            _storage = BinaryStorage
                .Construct(Path.Combine(Application.persistentDataPath, "PlayerPrefs.bin"))
                .AddPrimitiveTypes()
                .SupportListsOf<int>()
                .SupportListsOf<string>()
                .EnableAutoSaveOnChange()
                .Build();

            _storage.GetListOf<int>("ints");
            _storage.Remove("ints");

            using (_storage.MultipleChangeScope())
            {
                var value = _storage.Get<int>("int_val", 0);
                _storage.Set("int_val", value + 1);

                using (_storage.MultipleChangeScope())
                {
                    value = _storage.Get<int>("int_val", 0);
                    _storage.Set("int_val", value + 1);

                    using (_storage.MultipleChangeScope())
                    {
                        value = _storage.Get<int>("int_val", 0);
                        _storage.Set("int_val", value + 1);
                    }
                }
            }
        }

        private void OnGUI()
        {
            var value = _storage.Get<int>("int_val", 0);
            if (GUILayout.Button($"INT={value}"))
            {
                _storage.Set("int_val", value + 1);
            }
        }

        private void OnDestroy()
        {
            _storage?.Dispose();
            _storage = null;
        }
    }
}