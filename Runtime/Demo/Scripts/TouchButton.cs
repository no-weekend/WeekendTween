using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using noWeekend;

namespace noWeekend
{
	public class TouchButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
	{
		private bool interactabe = true;

		[SerializeField] private WeekendTween buttonTween;
		[SerializeField] private UnityEvent onClickAction;

		public bool Interactable
		{
			set
			{
				interactabe = value;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!interactabe)
			{
				return;
			}
			onClickAction?.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			buttonTween.Activate();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			buttonTween.Deactivate();
		}
	}
}