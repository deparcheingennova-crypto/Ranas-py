// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : Hermann Fischer
// Created          : 07-07-2019
//
// Last Modified By : Hermann Fischer
// Last Modified On : 07-09-2019
// ***********************************************************************
// <copyright file="JumpInJumpOut.cs" company="Total Creations">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary>The main part of the JumpInJumpOut package</summary>
// ***********************************************************************

using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Events;
using System;

namespace TotalCreations {
    namespace UI {
        ///<remarks>
        ///Drives shake, scale, rotation and blending animations for menus and buttons
        ///</remarks>
        [RequireComponent(typeof(CanvasGroup))]
        public class JumpInJumpOut : MonoBehaviour {
            public CanvasGroup canvasGroup;
            [Tooltip("Recommendation to turn this off if you use it for buttons. Actively drives the Blocks Raycast toggle of the Canvas Group")]
            ///<summary>
            ///Recommendation to turn this off if you use it for buttons. Actively
            ///drives the Blocks Raycast toggle of the Canvas Group
            ///</summary>
            public bool controlUIRaycast = false;
            [Tooltip("Start state will get applied without animation")]
            ///<summary>
            ///Start state will get applied without animation
            ///</summary>
            public bool startWithoutAnimation = false;
            [Tooltip("Starts with the Hide State without animation")]
            ///<summary>
            ///Starts with the Hide State without animation
            ///</summary>
            public bool hideOnStart = false;
            [Tooltip("Starts animation when OnEnable gets called")]
            ///<summary>
            ///Show animation plays when OnEnable gets called
            ///</summary>
            public bool activateOnEnabled = false;
            [Tooltip("Calling the Show Method will result in hiding first before showing again")]
            ///<summary>
            ///Calling the Show Method will result in hiding first before showing again
            ///</summary>
            public bool hideBeforeShow = false;
            [Tooltip("Autohide in seconds, 0 will never autohide")]
            ///<summary>
            ///Autohide in seconds, 0 will never autohide
            ///</summary>
            public float displayTime = 0f;
            public float showDuration = 0.5f;
            public float hideDuration = 0.5f;
            [Header("Blend")]
            [Tooltip("Toggles the blend-effect")]
            ///<summary>
            ///Toggles the blend-effect
            ///</summary>
            public bool blendActive = true;
            public Ease showBlendEase = Ease.InOutQuad;
            public Ease hideBlendEase = Ease.InOutQuad;
            [Header("Scale")]
            [Tooltip("Toggles the scale-effect")]
            ///<summary>
            ///Toggles the scale-effect
            ///</summary>
            public bool scaleActive = true;
            public Ease showScaleEase = Ease.InOutQuad;
            public Ease hideScaleEase = Ease.InOutQuad;
            [Tooltip("Min represents the scale factor of the hide-state and max of the show-state")]
            ///<summary>
            ///Min represents the scale factor of the hide-state and max of the
            ///show-state
            ///</summary>
            public MinMaxHelper scaleMinMax = new MinMaxHelper(0f, 1f);
            [Header("Shake")]
            [Tooltip("Toggles the shake-effect. NOTE: If also the scale-effect is activated, only the scale effect will be used")]
            ///<summary>
            ///Toggles the shake-effect. NOTE: If also the scale-effect is activated,
            ///only the scale effect will be used
            ///</summary>
            public bool shakeActive = true;
            public float showShakeStrength = 0.1f;
            public int showShakeVibrato = 10;
            public float showShakeRandomness = 90;
            public Ease showShakeEase = Ease.InOutQuad;
            public Ease hideShakeEase = Ease.InOutQuad;
            public float hideShakeStrength = 0.1f;
            public int hideShakeVibrato = 10;
            public float hideShakeRandomness = 90;
            [Header("Rotation")]
            [Tooltip("Toggles the rotation-effect")]
            ///<summary>
            ///Toggles the rotation-effect
            ///</summary>
            public bool rotationActive = true;
            public Vector3 rotationAxis = Vector3.forward;
            public Ease showRotationEase = Ease.InOutQuad;
            public Ease hideRotationEase = Ease.InOutQuad;
            [Tooltip("The initial rotation when showing")]
            ///<summary>
            ///The initial rotation when showing
            ///</summary>
            public MinMaxHelper showStartMinMaxRotation = new MinMaxHelper(-15, 15);
            [Tooltip("The rotation when showing is completed")]
            public MinMaxHelper showMinMaxRotation = new MinMaxHelper(-15, 15);
            [Tooltip("The rotation after hiding")]
            ///<summary>
            ///The rotation after hiding
            ///</summary>
            public MinMaxHelper hideMinMaxRotation = new MinMaxHelper(-15, 15);
            public UnityEvent onShowFinished;
            public UnityEvent onHideStarted;
            public UnityEvent onHideFinished;
            private State mState = State.Invisible;

            private enum State { Visible, Invisible };

            private Coroutine mCoroutine = null;
            private Tween shakeTween = null;
            private Tween scaleTween = null;
            private Tween blendTween = null;
            private Tween rotationTween = null;

            ///<summary>
            ///Applies CanvasGroup to the class and goes to initial state
            ///</summary>
            private void Awake() {
                if (canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();

                if (!Application.isPlaying)
                    return;

                if (!activateOnEnabled) {
                    SetToInitialState();
                }
            }

            ///<summary>
            ///Sets the display settings to the initial state
            ///</summary>
            public void SetToInitialState() {
                if (gameObject.activeInHierarchy) {
                    mState = State.Invisible;
                } else {
                    mState = State.Visible;
                }

                if (startWithoutAnimation) {
                    if (hideOnStart) {
                        canvasGroup.alpha = 0;
                        if (controlUIRaycast)
                            canvasGroup.blocksRaycasts = false;

                        if (scaleActive)
                            this.transform.localScale = scaleMinMax.min.FillVector3();
                        if (rotationActive) {
                            Vector3 endValue = rotationAxis * hideMinMaxRotation.Random();
                            this.transform.localRotation = Quaternion.Euler(endValue);
                        }
                        mState = State.Invisible;
                    } else {
                        canvasGroup.alpha = 1;
                        if (controlUIRaycast)
                            canvasGroup.blocksRaycasts = false;

                        if (scaleActive)
                            this.transform.localScale = scaleMinMax.max.FillVector3();
                        if (rotationActive) {
                            Vector3 endValue = rotationAxis * showMinMaxRotation.Random();
                            this.transform.localRotation = Quaternion.Euler(endValue);
                        }
                        mState = State.Visible;
                    }
                } else {

                }
            }

            ///<summary>
            ///Shows the menu/button/UI if showOnEnabled is activated
            ///</summary>
            private void OnEnable() {
                if (activateOnEnabled) {
                    if (startWithoutAnimation) {
                        SetToInitialState();
                    } else {
                        if (hideOnStart) {
                            Hide();
                        } else {
                            Show();
                        }
                    }
                }
            }

            ///<summary>
            ///Shows the menu/button/UI
            ///</summary>
            public void JumpIn() {
                Show();
            }

            ///<summary>
            ///Hides the menu/button/UI
            ///</summary>
            public void JumpOut() {
                Hide();
            }

            ///<summary>
            ///Shows the menu/button/UI
            ///</summary>
            ///<returns>the yield instruction to wait until the execution finishes
            ///    </returns>
            public YieldInstruction Show() {
                return Show(null);
            }

            ///<summary>
            ///Hides the menu/button/UI
            ///</summary>
            ///<returns>the yield instruction to wait until the execution finishes
            ///    </returns>
            public YieldInstruction Hide() {
                return Hide(null);
            }

            ///<summary>
            ///Shows the menu/button/UI and triggers the onFinished action after
            ///execution
            ///</summary>
            ///<returns>the yield instruction to wait until the execution finishes
            ///    </returns>
            public YieldInstruction Show(Action onFinished) {
                mCoroutine = StartCoroutine(ShowRoutine(onFinished));
                return mCoroutine;
            }

            ///<summary>
            ///Hides the menu/button/UI and triggers the onFinished action after
            ///execution
            ///</summary>
            ///<returns>the yield instruction to wait until the execution finishes
            ///    </returns>
            public YieldInstruction Hide(Action onFinished) {
                if (canvasGroup.alpha == 0f) {
                    if (onHideStarted != null)
                        onHideStarted.Invoke();

                    if (onFinished != null)
                        onFinished.Invoke();
                    return null;
                }
                if (mCoroutine != null)
                    StopCoroutine(mCoroutine);
                if (gameObject.activeInHierarchy) {
                    mCoroutine = StartCoroutine(HideRoutine(onFinished));
                    return mCoroutine;
                }
                return mCoroutine;
            }

            ///<summary>
            ///Kills all existent, active tweens
            ///</summary>
            private void KillAllTweens() {
                if (shakeTween != null)
                    shakeTween.Kill();
                if (scaleTween != null)
                    scaleTween.Kill();
                if (blendTween != null)
                    blendTween.Kill();
                if (rotationTween != null)
                    rotationTween.Kill();
            }

            ///<summary>
            ///Shows the menu/button/UI and triggers the onFinished action after
            ///execution
            ///</summary>
            ///<returns>the IEnumerator instruction to wait until the execution finishes
            ///    </returns>
            public IEnumerator ShowRoutine(Action onFinished = null) {
                if (mState == State.Invisible) {
                    KillAllTweens();
                    mState = State.Visible;

                    if (shakeActive && showDuration > 0f)
                        shakeTween = this.transform.DOShakeScale(showDuration, showShakeStrength, showShakeVibrato, showShakeRandomness).SetEase(showShakeEase);
                    if (scaleActive)
                        scaleTween = this.transform.DOScale(scaleMinMax.max, showDuration).SetEase(showScaleEase);
                    if (blendActive) {
                        if (controlUIRaycast)
                            canvasGroup.blocksRaycasts = true;
                        blendTween = canvasGroup.DOFade(1f, showDuration).SetEase(showBlendEase);
                    } else {
                        canvasGroup.alpha = 1f;
                        if (controlUIRaycast)
                            canvasGroup.blocksRaycasts = true;
                    }
                    if (rotationActive) {
                        this.transform.localRotation = Quaternion.Euler(rotationAxis * showStartMinMaxRotation.Random());

                        Vector3 endValue = rotationAxis * showMinMaxRotation.Random();
                        rotationTween = this.transform.DOLocalRotate(endValue, showDuration, RotateMode.Fast).SetEase(showRotationEase);
                    }
                    yield return new WaitForSeconds(showDuration);

                    onShowFinished.Invoke();
                    if (onFinished != null)
                        onFinished.Invoke();


                    if (displayTime > 0) {
                        yield return new WaitForSeconds(displayTime);
                        yield return HideRoutine(null);
                    }
                } else {
                    if (hideBeforeShow) {
                        yield return HideRoutine();
                        yield return ShowRoutine(onFinished);
                    } else {
                        if (onFinished != null)
                            onFinished.Invoke();
                    }
                }
            }

            ///<summary>
            ///Hides the menu/button/UI and triggers the onFinished action after
            ///execution
            ///</summary>
            ///<returns>the IEnumerator instruction to wait until the execution finishes
            ///    </returns>
            public IEnumerator HideRoutine(Action onFinished = null) {
                if (mState == State.Visible) {
                    if (onHideStarted != null)
                        onHideStarted.Invoke();
                    KillAllTweens();
                    mState = State.Invisible;

                    if (shakeActive && hideDuration > 0f)
                        shakeTween = this.transform.DOShakeScale(hideDuration, hideShakeStrength, hideShakeVibrato, hideShakeRandomness).SetEase(hideShakeEase);
                    if (scaleActive) {
                        scaleTween = this.transform.DOScale(scaleMinMax.min, hideDuration).SetEase(hideScaleEase);
                    }
                    if (blendActive) {
                        blendTween = canvasGroup.DOFade(0f, hideDuration).SetEase(hideBlendEase);
                    }

                    if (rotationActive) {
                        Vector3 endValue = rotationAxis * hideMinMaxRotation.Random();
                        rotationTween = this.transform.DOLocalRotate(endValue, hideDuration, RotateMode.Fast).SetEase(hideRotationEase);
                    }
                    yield return new WaitForSeconds(hideDuration);
                    if ((shakeActive && (this.transform.localScale.x == 0f || this.transform.localScale.y == 0f || this.transform.localScale.z == 0f)) || blendActive) {
                        canvasGroup.alpha = 0f;
                        if (controlUIRaycast)
                            canvasGroup.blocksRaycasts = false;
                    }
                    if (onFinished != null)
                        onFinished.Invoke();
                    onHideFinished.Invoke();

                } else {
                    if (controlUIRaycast)
                        canvasGroup.blocksRaycasts = false;
                    if (onHideStarted != null)
                        onHideStarted.Invoke();
                    if (onFinished != null)
                        onFinished.Invoke();
                }
            }
        }
    }
}
