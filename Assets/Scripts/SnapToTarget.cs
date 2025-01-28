using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Oculus.Interaction
{
    public class SnapToTarget : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object _interactableView;
        private IInteractableView InteractableView;
        
        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object _interactableViewHand;
        private IInteractableView InteractableViewHand;
        
        private Transform memSnapPos;
        private Transform minDiffSnapPos;
        private string snapDest = null;
        private bool isReleased = false;

        protected bool _started = false;

        protected virtual void Awake()
        {
            InteractableView = _interactableView as IInteractableView;
            InteractableViewHand = _interactableViewHand as IInteractableView;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(InteractableView, nameof(InteractableView));
            this.AssertField(InteractableViewHand, nameof(InteractableViewHand));
            this.EndStart(ref _started);
            memSnapPos = gameObject.transform.Find("memSnapPoint");
            minDiffSnapPos = gameObject.transform.Find("minDiffSnapPoint");
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged += HandleStateChanged;
                InteractableViewHand.WhenStateChanged += HandleStateChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged -= HandleStateChanged;
                InteractableViewHand.WhenStateChanged -= HandleStateChanged;
            }
        }

        void Update()
        {
            snapDest = gameObject.GetComponentInParent<CreateGhost>().snapDest;
            if (snapDest == "memory" && isReleased)
            {
                transform.position = memSnapPos.position;
                transform.rotation = Quaternion.identity;
            }
            else if (snapDest == "minDiff" && isReleased)
            {
                transform.position = minDiffSnapPos.position;
                transform.rotation = Quaternion.identity;
            }
        }

        private void HandleStateChanged(InteractableStateChangeArgs args)
        {
            if (args.NewState == InteractableState.Hover && args.PreviousState == InteractableState.Select)
            {
                isReleased = true;
            }
            else
            {
                isReleased = false;
            }
        }

        #region Inject

        public void InjectAllInteractableUnityEventWrapper(IInteractableView interactableView)
        {
            InjectInteractableView(interactableView);
        }

        public void InjectInteractableView(IInteractableView interactableView)
        {
            _interactableView = interactableView as UnityEngine.Object;
            InteractableView = interactableView;
        }

        #endregion
    }
}
