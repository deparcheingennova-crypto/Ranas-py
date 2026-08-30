using System.Collections.Generic;
using UnityEngine;

namespace TotalCreations {
    ///<remarks>
    ///The example switcher class, switches between different samples
    ///</remarks>
    public class ExampleSwitcher : MonoBehaviour {
        public List<GameObject> examples;
        private int currentExampleIndex = 0;

        ///<summary>
        ///Switches to the next sample
        ///</summary>
        public void NextSample() {
            if (examples != null && examples.Count > 0) {
                examples[currentExampleIndex].gameObject.SetActive(false);

                if (currentExampleIndex + 1 < examples.Count) {
                    currentExampleIndex++;
                } else {
                    currentExampleIndex = 0;
                }
                examples[currentExampleIndex].gameObject.SetActive(true);
            }
        }
    }
}
