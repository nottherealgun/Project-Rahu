using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMap : SerializedMonoBehaviour
{

    public static PlayerInputMap Instance { get; private set; }
    [OdinSerialize] private PlayerInput playerInput;
    void Awake()
    {

        if (Instance != null && Instance != this){

            Destroy(this.gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(this.gameObject);
        playerInput = GetComponent<PlayerInput>();
    }

    #region Player Actions
    void OnMove(InputValue value) {}




    void OnLook(InputValue value) {}




    void OnInteract(InputValue value) {}





    void OnExitInteraction(InputValue value) {}





    void OnMenu(InputValue value) {}




    void OnReset(InputValue value) {}





    void OnSprint(InputValue value) {}



    #endregion

    #region UI Actions
    void OnNavigate(InputValue value) {}




    void OnScrollWheel(InputValue value) {}




    void OnNext(InputValue value) {}





    void OnBack(InputValue value) {}




    void OnRotate(InputValue value) {}





    void OnZoom(InputValue value) {}




    void QTE1(InputValue value) {}




    void QTE2(InputValue value) {}




    void QTE3(InputValue value) {}




    void QTE4(InputValue value) {}




    void OnLeft(InputValue value) {}




    void OnRight(InputValue value) {}




    void OnBoost(InputValue value) {}




    void OnEject(InputValue value) {}




    void OnClick(InputValue value) {}




    void OnRightClick(InputValue value) {}




    void OnMiddleClick(InputValue value) {}



    #endregion
}
