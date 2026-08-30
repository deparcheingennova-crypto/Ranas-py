using System.Collections;
using System.Collections.Generic;
using TotalCreations.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TotalCreations {
    public class Example : MonoBehaviour {
        public float startTimeDistance = 0.2f;
        public float pauseBeforeRestart = 1f;
        public JumpInJumpOut[] list;

        private void OnEnable() {
            list = gameObject.GetComponentsInChildren<JumpInJumpOut>();
            StartCoroutine(StartLoops());
        }

        private IEnumerator StartLoops() {
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < list.Length; i++) {
                JumpInJumpOut item = list[i];
                item.Show(() => {
                    StartCoroutine(OnShowFinished(item));
                });

                if (startTimeDistance > 0f) {
                    yield return new WaitForSeconds(startTimeDistance);
                }
            }
        }

        private IEnumerator OnShowFinished(JumpInJumpOut jumpInJumpOut) {
            yield return new WaitForSeconds(pauseBeforeRestart);
            jumpInJumpOut.Hide(() => {
                StartCoroutine(OnHideFinished(jumpInJumpOut));
            });
        }

        private IEnumerator OnHideFinished(JumpInJumpOut jumpInJumpOut) {
            yield return new WaitForSeconds(pauseBeforeRestart);
            jumpInJumpOut.Show(() => {
                StartCoroutine(OnShowFinished(jumpInJumpOut));
            });
        }
    }
}
