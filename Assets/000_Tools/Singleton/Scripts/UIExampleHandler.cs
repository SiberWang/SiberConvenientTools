using Cysharp.Threading.Tasks;
using rStarUtility.Util.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SingletonExample
{
    // View UI 相關的東西放在這
    public class UIExampleHandler : MonoBehaviour
    {
    #region ========== Private Variables ==========

        [SerializeField]
        private TMP_Text textMessage;

        [SerializeField]
        private Button buttonStart;

        [SerializeField]
        private Button buttonStop;

    #endregion

    #region ========== Untiy Events ==========

        private void Start()
        {
            // TODO: 先暫時使用，這個其實是很糟糕的設計~~
            GameMgr.Instance.UpdateUIAction += OnUpdateUITextEvent;
            GameMgr.Instance.FinishedAction += OnFinishedAction;
            GameMgr.Instance.StopAction     += OnStopEvent;

            buttonStart.BindClick(() => GameMgr.Instance.CallUserDatas().Forget());
            buttonStop.BindClick(() => GameMgr.Instance.StopCall());
        }

    #endregion

    #region ========== Events ==========

        private void OnUpdateUITextEvent(string message)
        {
            textMessage.text = message;
        }

        private void OnFinishedAction()
        {
            InitButton();
        }
        
        private void OnStopEvent()
        {
            InitButton();
        }

    #endregion

    #region ========== Private Methods ==========

        private void InitButton()
        {
            buttonStart.interactable = true;
            buttonStop.interactable  = false;
        }

    #endregion
    }
}