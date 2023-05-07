using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace noWeekend
{
    public class WeekendTween : MonoBehaviour
    {
        public enum EaseActionType { Position, Rotation,Scale}
        public Transform targetTransform;

        [SerializeReference] public List<EaseAction> activateEaseActions = new List<EaseAction>();
        [SerializeReference] public List<EaseAction> deactivateEaseActions = new List<EaseAction>();

        public bool activateOnEnable;
        public bool useUnscaledTime;

        public UnityEvent onActivateCompleteAction,onDeactivateCompleteAction;
        private Coroutine onActivateCoroutine, onDeactivateCoroutine;

        public float LongestActivateTween => activateEaseActions.Max(item => item.Duration);
		public float LongestDeactivateTween => deactivateEaseActions.Max(item => item.Duration);

        private float timer;

        public float EditorTimer
        {
			get
			{
				return timer;
			}
		}

        public float ActivateEditorTimer
        {
            set
            {
                if (activateEaseActions.Count == 0) return;
                timer = value % LongestActivateTween;
            }
        }

		public float DeactivateEditorTimer
		{
			set
			{
				if (deactivateEaseActions.Count == 0) return;
				timer = value % LongestDeactivateTween;
			}
		}


		private void OnEnable()
        {
            if (activateOnEnable)
            {
                Activate();
            }
        }

        public void RemoveEaseAction(EaseAction easeAction, bool isActivateEase)
        {
			if (isActivateEase)
			{
				activateEaseActions.Remove(easeAction);
			}
			else
			{
				deactivateEaseActions.Remove(easeAction);
			}
		}

        public void OnAddPositionEaseButtonPress(bool isActivateEase)
        {
            if (isActivateEase)
            {
                activateEaseActions.Add(new PositionEaseAction(targetTransform.localPosition));
            }
            else
            {
                deactivateEaseActions.Add(new PositionEaseAction(targetTransform.localPosition));
            }
        }

        public void OnAddRotationEaseButtonPress(bool isActivateEase)
        {
            if (isActivateEase)
            {
                activateEaseActions.Add(new RotateEaseAction(targetTransform.localRotation));
            }
            else
            {
                deactivateEaseActions.Add(new RotateEaseAction(targetTransform.localRotation));
            }
            
        }

        public void OnAddScaleEaseButtonPress(bool isActivateEase)
        {
            if (isActivateEase)
            {
                activateEaseActions.Add(new ScaleEaseAction(targetTransform.localScale));
            }
            else
            {
                deactivateEaseActions.Add(new ScaleEaseAction(targetTransform.localScale));
            }
        }

        public void Activate()
        {
            Activate(null);
		}

        public void Activate(Action onComplete = null)
        {
			StopAllTweens();

			onActivateCoroutine = StartCoroutine(ActivateCoroutine(onComplete));
        }

		public IEnumerator ActivateCoroutine(Action onComplete = null)
		{
			//Get the length of the longest tween (including delay)
			float longestTween = LongestDeactivateTween;

			//Loop through each active tween 
			timer = 0;
			while (timer < longestTween)
			{
				foreach (EaseAction easeAction in activateEaseActions)
				{
					easeAction.Process(targetTransform, timer);
				}

				timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}

			//Finish all Tweens
			foreach (EaseAction easeAction in activateEaseActions)
			{
				easeAction.SetEnd(targetTransform);
			}

			//Run any on complete Actions
			onActivateCompleteAction?.Invoke();
			onComplete?.Invoke();
		}

        //Stop all tweens if they are currently happening
        private void StopAllTweens()
        {
			if (onDeactivateCoroutine != null) StopCoroutine(onDeactivateCoroutine);
			if (onActivateCoroutine != null) StopCoroutine(onActivateCoroutine);
		}


		public void Deactivate()
		{
			Deactivate(null);
		}

		public void Deactivate(Action onComplete = null)
        {
            StopAllTweens();
			StartCoroutine(DeactivateCoroutine(onComplete));
        }

		IEnumerator DeactivateCoroutine(Action onComplete = null)
		{
			//Get the length of the longest tween (including delay)
			float longestTween = deactivateEaseActions.Max(item => item.Duration);

			//Loop through each active tween 
			float timer = 0;
			while (timer < longestTween)
			{
				foreach (EaseAction easeAction in deactivateEaseActions)
				{
					easeAction.Process(targetTransform, timer);
				}

				timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}

			//Finish all Tweens
			foreach (EaseAction easeAction in deactivateEaseActions)
			{
				easeAction.SetEnd(targetTransform);
			}

			//Run any on complete Actions
			onDeactivateCompleteAction?.Invoke();
			onComplete?.Invoke();
		}

	}

    [System.Serializable]
    public abstract class EaseAction
    {
        public float duration = 1;
        public float delay = 0;
        public EaseType easeType = EaseType.SineInOut;
        public float Duration => delay + duration;
        public abstract string EaseName {get;}
        public abstract WeekendTween.EaseActionType EaseActionType { get; }

		public void Process(EaseRefererceTransform targetTransform, float time)
        {
            
            if(time < delay || time > delay + duration)
            {
                return;
            }

            float value = Mathf.InverseLerp(delay, duration + delay, time);
            ApplyEase(targetTransform, value);
        }

        public void Process(Transform targetTransform, float time)
        {
			if (time < delay || time > delay + duration)
			{
				return;
			}

			float value = Mathf.InverseLerp(delay, duration + delay, time);
            ApplyEase(targetTransform, value);
        }
        protected virtual void ApplyEase(Transform targetTransform, float value) { }
        protected virtual void ApplyEase(EaseRefererceTransform targetTransform, float value) { }
        public virtual void SetStart(Transform targetTransform){ }
        public virtual void SetStart(EaseRefererceTransform targetTransform) { }
        public virtual void SetEnd(Transform targetTransform){ }
        public virtual void SetEnd(EaseRefererceTransform targetTransform) { }
    }

    [System.Serializable]
    public class PositionEaseAction : EaseAction
    {
        public override string EaseName => "Ease Position";
		public Vector3 fromPosition;
        public Vector3 toPosition;
        public override WeekendTween.EaseActionType EaseActionType => WeekendTween.EaseActionType.Position;

		public PositionEaseAction(Vector3 positions)
        {
            this.fromPosition = positions;
            this.toPosition = positions;
        }

        public PositionEaseAction(Vector3 fromPosition, Vector3 toPosition)
        {
            this.fromPosition = fromPosition;
            this.toPosition = toPosition;
        }

        protected override void ApplyEase(Transform targetTransform, float value)
        {
            targetTransform.localPosition = Vector3.LerpUnclamped(fromPosition, toPosition, Ease.GetEase(easeType, value));
        }
        protected override void ApplyEase(EaseRefererceTransform targetTransform, float value)
        {
            targetTransform.localPosition = Vector3.LerpUnclamped(fromPosition, toPosition, Ease.GetEase(easeType, value));
        }

        public override void SetStart(Transform targetTransform)
        {
            targetTransform.localPosition = fromPosition;
        }

        public override void SetStart(EaseRefererceTransform targetTransform)
        {
            targetTransform.localPosition = fromPosition;
        }

        public override void SetEnd(Transform targetTransform)
		{
            targetTransform.localPosition = toPosition;
        }
        public override void SetEnd(EaseRefererceTransform targetTransform)
        {
            targetTransform.localPosition = toPosition;
        }

        public void SetFromPositionToCurrent(Transform targetTransform)
        {
            fromPosition = targetTransform.localPosition;

		}

        public void SetToPositionToCurrent(Transform targetTransform)
        {
			toPosition = targetTransform.localPosition;
		}
    }

    [System.Serializable]
    public class RotateEaseAction : EaseAction
    {

		public override string EaseName => "Ease Rotation";
		public Quaternion toRotation;
        public Quaternion fromRotation;
		public override WeekendTween.EaseActionType EaseActionType => WeekendTween.EaseActionType.Rotation;

		public RotateEaseAction(Quaternion rotations)
        {
            this.toRotation = rotations;
            this.fromRotation = rotations;
        }

        public RotateEaseAction(Quaternion toRotation, Quaternion fromRotation)
        {
            this.toRotation = toRotation;
            this.fromRotation = fromRotation;
        }

        protected override void ApplyEase(Transform targetTransform, float value)
        {
            targetTransform.localRotation = Quaternion.LerpUnclamped(fromRotation, toRotation, Ease.GetEase(easeType, value));
        }
        protected override void ApplyEase(EaseRefererceTransform targetTransform, float value)
        {
            targetTransform.localRotation = Quaternion.LerpUnclamped(fromRotation, toRotation, Ease.GetEase(easeType, value));
        }

        public override void SetStart(Transform targetTransform)
        {
            targetTransform.localRotation = fromRotation;
        }
        public override void SetEnd(Transform targetTransform)
        {
            targetTransform.localRotation = toRotation;
        }
        public override void SetStart(EaseRefererceTransform targetTransform)
        {
            targetTransform.localRotation = fromRotation;
        }
        public override void SetEnd(EaseRefererceTransform targetTransform)
        {
            targetTransform.localRotation = toRotation;
        }

		public void SetFromRotationToCurrent(Transform targetTransform)
		{
			toRotation = targetTransform.localRotation;

		}

		public void SetToRotationToCurrent(Transform targetTransform)
		{
			toRotation = targetTransform.localRotation;
		}
	}

    [System.Serializable]
    public class ScaleEaseAction : EaseAction
    {
		public override string EaseName => "Ease Scale";
		public Vector3 fromScale;
        public Vector3 toScale;
		public override WeekendTween.EaseActionType EaseActionType => WeekendTween.EaseActionType.Scale;

		public ScaleEaseAction(Vector3 scales)
        {
            this.fromScale = scales;
            this.toScale = scales;
        }


        public ScaleEaseAction(Vector3 fromScale, Vector3 toScale)
        {
            this.fromScale = fromScale;
            this.toScale = toScale;
        }

        protected override void ApplyEase(Transform targetTransform, float value)
        {
            targetTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, Ease.GetEase(easeType, value));
        }
        protected override void ApplyEase(EaseRefererceTransform targetTransform, float value)
        {
            targetTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, Ease.GetEase(easeType, value));
        }

        public override void SetStart(Transform targetTransform)
        {
            targetTransform.localScale = fromScale;
        }
        public override void SetStart(EaseRefererceTransform targetTransform)
        {
            targetTransform.localScale = fromScale;
        }

        public override void SetEnd(Transform targetTransform)
        {
            targetTransform.localScale = toScale;
        }
        public override void SetEnd(EaseRefererceTransform targetTransform)
        {
            targetTransform.localScale = toScale;
        }

		public void SetFromScaleToCurrent(Transform targetTransform)
		{
			toScale = targetTransform.localScale;

		}

		public void SetToScaleToCurrent(Transform targetTransform)
		{
			toScale = targetTransform.localScale;
		}
	}

    public class EaseRefererceTransform
    {
        public Transform transform;
        public Vector3 localPosition;
        public Vector3 position;
        public Vector3 localScale;
        public Quaternion localRotation;
        public Rect rect;
        public Bounds bounds;
        public bool isRectTransform;

        public Vector3 lossyScale; 

		public EaseRefererceTransform(Transform referenceTransform)
		{
            this.transform = referenceTransform;
            this.localPosition = referenceTransform.localPosition;
			this.localScale = referenceTransform.localScale;
            this.localRotation = referenceTransform.localRotation;
            this.lossyScale = referenceTransform.lossyScale;
            if (referenceTransform.GetType() == typeof(RectTransform))
            {
                rect = ((RectTransform)referenceTransform).rect;
                isRectTransform = true;
            }
            else
            {
                isRectTransform = false;
            }

        }
	}

}

