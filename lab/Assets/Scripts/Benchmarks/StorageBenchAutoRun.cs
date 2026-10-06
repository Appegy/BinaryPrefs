#if BINARYPREFS_BENCH
using System.Collections;
using UnityEngine;

namespace Appegy.Storage.Benchmarks
{
    public class StorageBenchAutoRun : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var host = new GameObject(nameof(StorageBenchAutoRun));
            DontDestroyOnLoad(host);
            host.AddComponent<StorageBenchAutoRun>();
        }

        private IEnumerator Start()
        {
            Debug.Log($"{StorageBench.Tag} device={SystemInfo.deviceModel} os={SystemInfo.operatingSystem} graphics={SystemInfo.graphicsDeviceName}");

            var directory = StorageBench.Prepare(Application.persistentDataPath);
            yield return null;

            var control = StorageBench.RunControl(directory);
            yield return null;

            foreach (var scenario in StorageBenchScenario.All)
            {
                StorageBench.RunScenario(directory, scenario, control);
                yield return null;
            }

            Debug.Log($"{StorageBench.Tag} end");
        }
    }
}
#endif
