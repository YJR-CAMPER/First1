using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 대화 시작/종료 시 플레이어 이동을 제어
/// 
/// 설정 방법:
/// 1. Inspector에서 movementControllerComponent에 IMovementController 구현체를 할당
/// 2. DialogueManager의 OnDialogueStart 이벤트에 이 컴포넌트의 HandleDialogueStart를 연결
/// 3. DialogueManager의 OnDialogueEnd 이벤트에 이 컴포넌트의 HandleDialogueEnd를 연결
/// </summary>
public class DialoguePlayerControl : MonoBehaviour
{
    [Header("Movement Controller")]
    [SerializeField] private MonoBehaviour movementControllerComponent;

    [Header("Events (Optional)")]
    [SerializeField] private UnityEvent onDialogueStart;
    [SerializeField] private UnityEvent onDialogueEnd;

    private IMovementController movementController;

    private void Awake()
    {
        ResolveMovementController();
    }

    /// <summary>
    /// DialogueManager.OnDialogueStart 이벤트에 연결
    /// Inspector에서 UnityEvent 리스너로 할당
    /// </summary>
    public void HandleDialogueStart()
    {
        if (movementController != null)
        {
            movementController.EnableMovement(false);
        }

        onDialogueStart?.Invoke();
    }

    /// <summary>
    /// DialogueManager.OnDialogueEnd 이벤트에 연결
    /// Inspector에서 UnityEvent 리스너로 할당
    /// </summary>
    public void HandleDialogueEnd()
    {
        if (movementController != null)
        {
            movementController.EnableMovement(true);
        }

        onDialogueEnd?.Invoke();
    }

    public void SetMovementController(IMovementController controller)
    {
        if (controller == null)
        {
            Debug.LogError("IMovementController is null.");
            return;
        }

        movementController = controller;
    }

    private void ResolveMovementController()
    {
        if (movementControllerComponent != null)
        {
            movementController = movementControllerComponent as IMovementController;

            if (movementController == null)
            {
                Debug.LogError(
                    $"{movementControllerComponent.GetType().Name} does not implement IMovementController."
                );
            }

            return;
        }

        movementController = GetComponent<IMovementController>();
    }
}
