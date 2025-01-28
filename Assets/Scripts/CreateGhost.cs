using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Oculus.Interaction
{
    public class CreateGhost : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="IInteractableView"/> (Interactable) component to wrap.
        /// </summary>
        [Tooltip("The IInteractableView (Interactable) component to wrap.")]
        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object _interactableView;
        private IInteractableView InteractableView;

        /// <summary>
        /// Raised when an Interactor hovers over the Interactable.
        /// </summary>
        [Tooltip("Raised when an Interactor hovers over the Interactable.")]
        [SerializeField]
        private UnityEvent _whenHover;

        /// <summary>
        /// Raised when the Interactable was being hovered but now it isn't.
        /// </summary>
        [Tooltip("Raised when the Interactable was being hovered but now it isn't.")]
        [SerializeField]
        private UnityEvent _whenUnhover;

        /// <summary>
        /// Raised when an Interactor selects the Interactable.
        /// </summary>
        [Tooltip("Raised when an Interactor selects the Interactable.")]
        [SerializeField]
        private UnityEvent _whenSelect;

        /// <summary>
        /// Raised when the Interactable was being selected but now it isn't.
        /// </summary>
        [Tooltip("Raised when the Interactable was being selected but now it isn't.")]
        [SerializeField]
        private UnityEvent _whenUnselect;

        /// <summary>
        /// Raised each time an Interactor hovers over the Interactable, even if the Interactable is already being hovered by a different Interactor.
        /// </summary>
        [Tooltip("Raised each time an Interactor hovers over the Interactable, even if the Interactable is already being hovered by a different Interactor.")]
        [SerializeField]
        private UnityEvent _whenInteractorViewAdded;

        /// <summary>
        /// Raised each time an Interactor stops hovering over the Interactable, even if the Interactable is still being hovered by a different Interactor.
        /// </summary>
        [Tooltip("Raised each time an Interactor stops hovering over the Interactable, even if the Interactable is still being hovered by a different Interactor.")]
        [SerializeField]
        private UnityEvent _whenInteractorViewRemoved;

        /// <summary>
        /// Raised each time an Interactor selects the Interactable, even if the Interactable is already being selected by a different Interactor.
        /// </summary>
        [Tooltip("Raised each time an Interactor selects the Interactable, even if the Interactable is already being selected by a different Interactor.")]
        [SerializeField]
        private UnityEvent _whenSelectingInteractorViewAdded;

        /// <summary>
        /// Raised each time an Interactor stops selecting the Interactable, even if the Interactable is still being selected by a different Interactor.
        /// </summary>
        [Tooltip("Raised each time an Interactor stops selecting the Interactable, even if the Interactable is still being selected by a different Interactor.")]
        [SerializeField]
        private UnityEvent _whenSelectingInteractorViewRemoved;

        #region Properties

        public UnityEvent WhenHover => _whenHover;
        public UnityEvent WhenUnhover => _whenUnhover;
        public UnityEvent WhenSelect => _whenSelect;
        public UnityEvent WhenUnselect => _whenUnselect;
        public UnityEvent WhenInteractorViewAdded => _whenInteractorViewAdded;
        public UnityEvent WhenInteractorViewRemoved => _whenInteractorViewRemoved;
        public UnityEvent WhenSelectingInteractorViewAdded => _whenSelectingInteractorViewAdded;
        public UnityEvent WhenSelectingInteractorViewRemoved => _whenSelectingInteractorViewRemoved;

        public GameObject ghostPrefab;
        public GameObject TrajectoryPrefab;
        public GameObject twinLinePrefab;
        public float snapDistance = 0.1f;
        public float defaultDistance = 0.1f;
        public string snapDest = null;
        private GameObject currentGhost;
        private GameObject ghostObject;
        private GameObject memTrajectory;
        private LineRenderer memTrajectoryLine;
        private GameObject minDiffTrajectory;
        private LineRenderer minDiffTrajectoryLine;
        private int lineResolution = 30;
        private Vector3[] memTrajectoryPoints;
        private Vector3[] minDiffTrajectoryPoints;
        private GameObject twinConnector;
        private LineRenderer twinConnectorLine;
        private bool defaultState = false;
        private GameObject memSnapPoint;
        private GameObject minDiffSnapPoint;

        #endregion

        protected bool _started = false;

        protected virtual void Awake()
        {
            InteractableView = _interactableView as IInteractableView;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(InteractableView, nameof(InteractableView));
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged += HandleStateChanged;
                InteractableView.WhenInteractorViewAdded += HandleInteractorViewAdded;
                InteractableView.WhenInteractorViewRemoved += HandleInteractorViewRemoved;
                InteractableView.WhenSelectingInteractorViewAdded += HandleSelectingInteractorViewAdded;
                InteractableView.WhenSelectingInteractorViewRemoved += HandleSelectingInteractorViewRemoved;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged -= HandleStateChanged;
                InteractableView.WhenInteractorViewAdded -= HandleInteractorViewAdded;
                InteractableView.WhenInteractorViewRemoved -= HandleInteractorViewRemoved;
                InteractableView.WhenSelectingInteractorViewAdded -= HandleSelectingInteractorViewAdded;
                InteractableView.WhenSelectingInteractorViewRemoved -= HandleSelectingInteractorViewRemoved;
            }
        }

        // comment out this part if use TrajectorySnap script
        void Update()
        {
            if (currentGhost != null)
            {
                CheckNearOriginalPosition(ghostObject.transform);
                DrawTwinLine(twinConnectorLine, transform, ghostObject.transform);
                if (defaultState)
                {
                    memTrajectoryLine.enabled = true;
                    minDiffTrajectoryLine.enabled = true;
                    isNearTrajectory(ghostObject.transform);
                }
                else
                {
                    memTrajectoryLine.enabled = false;
                    minDiffTrajectoryLine.enabled = false;
                    snapDest = null;
                }
            }
        }

        private void HandleStateChanged(InteractableStateChangeArgs args)
        {
            switch (args.NewState)
            {
                case InteractableState.Normal:
                    if (args.PreviousState == InteractableState.Hover)
                    {
                        _whenUnhover.Invoke();
                    }

                    break;
                case InteractableState.Hover:
                    if (args.PreviousState == InteractableState.Normal)
                    {
                        _whenHover.Invoke();
                    }
                    else if (args.PreviousState == InteractableState.Select)
                    {
                        _whenUnselect.Invoke();

                        defaultState = false;
                    }

                    break;
                case InteractableState.Select:
                    if (args.PreviousState == InteractableState.Hover)
                    {
                        _whenSelect.Invoke();

                        // Initialize
                        if (currentGhost == null)
                        {
                            // Create Ghost
                            currentGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                            currentGhost.transform.SetParent(transform, true);
                            currentGhost.name = "currentGhost";

                            // uniform scale ghost 
                            Vector3 anchorScale = transform.lossyScale;
                            var anchorSmallestAxis = Mathf.Max(anchorScale.x < anchorScale.y ? anchorScale.y : anchorScale.x, anchorScale.z);
                            Mesh mesh = currentGhost.GetComponentInChildren<MeshFilter>().mesh;
                            Vector3 ghostScale = mesh.bounds.size;
                            var ghostSmallestAxis = Mathf.Max(ghostScale.x < ghostScale.y ? ghostScale.y : ghostScale.x, ghostScale.z);
                            float referenceSize = anchorSmallestAxis / ghostSmallestAxis;
                            currentGhost.transform.localScale = new Vector3(currentGhost.transform.localScale.x * referenceSize, currentGhost.transform.localScale.y * referenceSize, currentGhost.transform.localScale.z * referenceSize);

                            ghostObject = currentGhost.transform.Find("Ray Cube").gameObject;
                            memSnapPoint = currentGhost.transform.Find("memSnapPoint").gameObject;
                            minDiffSnapPoint = currentGhost.transform.Find("minDiffSnapPoint").gameObject;

                            // Create mem trajectory
                            // Vector3 offset = new Vector3(0.2f, 0f, -0.2f);
                            memTrajectory = Instantiate(TrajectoryPrefab, memSnapPoint.transform.position, transform.rotation);
                            memTrajectory.transform.SetParent(transform, true);
                            memTrajectory.name = "memTrajectory";
                            memTrajectoryLine = memTrajectory.GetComponent<LineRenderer>();
                            memTrajectoryLine.material.SetColor("_Color", Color.grey);
                            lineResolution = memTrajectoryLine.positionCount;
                            memTrajectoryPoints = new Vector3[lineResolution];
                            DrawParabolicTrajectory(memTrajectoryLine, memTrajectoryPoints, transform, memTrajectory.transform);

                            // Create minDiff trajectory
                            minDiffTrajectory = Instantiate(TrajectoryPrefab, minDiffSnapPoint.transform.position, transform.rotation);
                            minDiffTrajectory.transform.SetParent(transform, true);
                            minDiffTrajectory.name = "minDiffTrajectory";
                            minDiffTrajectoryLine = minDiffTrajectory.GetComponent<LineRenderer>();
                            minDiffTrajectoryLine.material.SetColor("_Color", Color.grey);
                            minDiffTrajectoryPoints = new Vector3[lineResolution];
                            DrawParabolicTrajectory(minDiffTrajectoryLine, minDiffTrajectoryPoints, transform, minDiffTrajectoryLine.transform);

                            // Create twin line
                            twinConnector = Instantiate(twinLinePrefab, transform.position, transform.rotation);
                            twinConnector.transform.SetParent(transform, true);
                            twinConnector.name = "twinConnector";
                            twinConnectorLine = twinConnector.GetComponent<LineRenderer>();
                            DrawTwinLine(twinConnectorLine, transform, currentGhost.transform);

                            
                        }
                        defaultState = true;
                    }

                    break;
            }
        }

        private void HandleInteractorViewAdded(IInteractorView interactorView)
        {
            WhenInteractorViewAdded.Invoke();
        }

        private void HandleInteractorViewRemoved(IInteractorView interactorView)
        {
            WhenInteractorViewRemoved.Invoke();
        }

        private void HandleSelectingInteractorViewAdded(IInteractorView interactorView)
        {
            WhenSelectingInteractorViewAdded.Invoke();
        }

        private void HandleSelectingInteractorViewRemoved(IInteractorView interactorView)
        {
            WhenSelectingInteractorViewRemoved.Invoke();
        }

        private void DrawTwinLine(LineRenderer line, Transform objectTransform, Transform targetTransform)
        {
            Vector3 startPos = objectTransform.position;
            Vector3 endPos = targetTransform.position;

            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
        }

        private void DrawParabolicTrajectory(LineRenderer line, Vector3[] trajectoryPoints, Transform objectTransform, Transform targetTransform)
        {
            Vector3 startPos = objectTransform.position;
            Vector3 endPos = targetTransform.position;
            float trajectoryHeight = 0.1f;

            for (int i = 0; i < lineResolution; i++)
            {
                float t = i / (float)(lineResolution - 1);

                Vector3 point = CalculateParabola(startPos, endPos, trajectoryHeight, t);

                line.SetPosition(i, point);
                trajectoryPoints[i] = point;
            }
        }

        Vector3 CalculateParabola(Vector3 start, Vector3 end, float height, float t)
        {
            Vector3 midPoint = Vector3.Lerp(start, end, t);
            midPoint.y += Mathf.Sin(t * Mathf.PI) * height;

            return midPoint;
        }

        private void isNearTrajectory(Transform objectTransform)
        {
            bool isNearMem = false;
            bool isNearMinDiff = false;
            float shortestMemDist = snapDistance;
            float shortestMinDiffDist = snapDistance;
            float dist;
            if (memTrajectoryPoints != null)
            {
                foreach (Vector3 point in memTrajectoryPoints)
                {
                    dist = Vector3.Distance(objectTransform.position, point);
                    if (dist <= snapDistance)
                    {
                        isNearMem = true;
                        shortestMemDist = Mathf.Min(dist, shortestMemDist);
                    }
                }
            }
            if (minDiffTrajectoryPoints != null)
            {
                foreach (Vector3 point in minDiffTrajectoryPoints)
                {
                    dist = Vector3.Distance(objectTransform.position, point);
                    if (dist <= snapDistance)
                    {
                        isNearMinDiff = true;
                        shortestMinDiffDist = Mathf.Min(dist, shortestMinDiffDist);
                    }
                }
            }

            if (isNearMem && isNearMinDiff) 
            {
                memTrajectoryLine.material.SetColor("_Color", Color.white);
                minDiffTrajectoryLine.material.SetColor("_Color", Color.white);
                snapDest = shortestMemDist < shortestMinDiffDist ? "memory" : "minDiff";
            }
            else if (isNearMem)
            {
                memTrajectoryLine.material.SetColor("_Color", Color.white);
                minDiffTrajectoryLine.material.SetColor("_Color", Color.grey);
                snapDest = "memory";
            }
            else if (isNearMinDiff)
            {
                memTrajectoryLine.material.SetColor("_Color", Color.grey);
                minDiffTrajectoryLine.material.SetColor("_Color", Color.white);
                snapDest = "minDiff";
            }
            else
            {
                memTrajectoryLine.material.SetColor("_Color", Color.grey);
                minDiffTrajectoryLine.material.SetColor("_Color", Color.grey);
                snapDest = null;
            }
        }

        private void CheckNearOriginalPosition(Transform objectTransform)
        {
            if (Vector3.Distance(objectTransform.position, transform.position) <= defaultDistance)
            {
                defaultState = true;
                return;
            }
            defaultState = false;
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
