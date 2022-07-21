using TMPro;
using UnityEngine.UI;

namespace Demo
{
    using System.Collections.Generic;
    using UnityEngine;

    internal sealed class LocalWorldCanvas : SingletonMonoBehaviour<LocalWorldCanvas>
    {
    #pragma warning disable 0649

        [Header("Player 1")]
        public Transform[] clients_1;

        [Space]
        public int lag_ms_1 = 250;

        public bool prediction_1;
        public bool reconciliation_1;
        public bool interpolation_1;

        private Client _player1;

        [Header("Server")]
        public Transform[] server;

        [Space]
        public int updateTimesPerSecond = 3;

        private Server _server;

        [Header("Player 2")]
        public Transform[] clients_2;

        [Space]
        public int lag_ms_2 = 250;

        public bool prediction_2;
        public bool reconciliation_2;
        public bool interpolation_2;

        private Client _player2;

        public Text textPlayer1Content;
        public Text textPlayer2Content;
    #pragma warning restore 0649

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (_server == null)
                {
                    return;
                }

                UpdateParameters();
            }
        }

        private void Awake()
        {
            _server  = new Server();
            _player1 = new Client();
            _player2 = new Client();

            _server.Connect(_player1);
            _server.Connect(_player2);

            UpdateParameters();
        }

        private void UpdateParameters()
        {
            UpdatePlayerParameters(_player1);
            UpdatePlayerParameters(_player2);
            _server.SetUpdateRate((ulong)updateTimesPerSecond);
        }

        private void UpdatePlayerParameters(Client client)
        {
            client.lag = client == _player1 ? lag_ms_1 : lag_ms_2;

            var cb_prediction     = client == _player1 ? prediction_1 : prediction_2;
            var cb_reconciliation = client == _player1 ? reconciliation_1 : reconciliation_2;

            // Client Side Prediction disabled => disable Server Reconciliation.
            if (client.client_side_prediction && !cb_prediction)
            {
                cb_reconciliation = false;
            }

            // Server Reconciliation enabled => enable Client Side Prediction.
            if (!client.server_reconciliation && !cb_prediction)
            {
                cb_reconciliation = false;
            }

            client.client_side_prediction = cb_prediction;
            client.server_reconciliation  = cb_reconciliation;

            client.entity_interpolation = client == _player1 ? interpolation_1 : interpolation_2;
        }

        private void Update()
        {
            if (_player1 == null || _player2 == null || _server == null)
            {
                return;
            }

            _player1.keyLeft  = UnityEngine.Input.GetKey(KeyCode.LeftArrow);
            _player1.keyRight = UnityEngine.Input.GetKey(KeyCode.RightArrow);

            _player2.keyLeft  = UnityEngine.Input.GetKey(KeyCode.A);
            _player2.keyRight = UnityEngine.Input.GetKey(KeyCode.D);

            _server.Tick();
            _player1.AdditionTick();
            _player2.AdditionTick();

            textPlayer1Content.text = _player1.ShowSomeInfo();
            textPlayer2Content.text = _player2.ShowSomeInfo();
        }

        // Caller id 0 = server, id 1 = client 1, id 2 = client 2
        public void RenderWorld(int callerId, List<Entity> entities)
        {
            Transform[] transforms = null;
            if (callerId == 0)
            {
                transforms = server;
            }
            else if (callerId == 1)
            {
                // client 1
                transforms = clients_1;
            }
            else if (callerId == 2)
            {
                transforms = clients_2;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException();
            }

            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                var tr = transforms[entity.ID];
                tr.position = new Vector3(entity.X, tr.position.y, tr.position.z);
            }
        }
    }
}