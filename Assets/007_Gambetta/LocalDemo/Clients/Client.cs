using System.Collections.Generic;

namespace Demo
{
    /// <summary> 客戶端的演算法處理 /// </summary>
    public class Client
    {
        /// <summary> 實體清單 </summary>
        public List<Entity> entities = new List<Entity>();
        /// <summary> 模擬網路連接 </summary>
        public LagNetwork network = new LagNetwork();
        /// <summary> 待處理清單 </summary>
        public List<Input> pendingInputs = new List<Input>();

        /// <summary> 唯一ID , 由服務氣連接時分配 </summary>
        public int? entityID = null;

        // Input state.
        /// <summary> 輸入狀態 (左) </summary>
        public bool keyLeft = false;
        /// <summary> 輸入狀態 (右) </summary>
        public bool keyRight = false;


    #region ========== 三大重點處理 ==========

        /// <summary> 客戶端預測 Client-side prediction </summary>
        public bool client_side_prediction = false;
        /// <summary> 服務氣對帳 Server reconciliation </summary>
        public bool server_reconciliation = false;
        /// <summary> 實體插值 Entity Interpolation </summary>
        public bool entity_interpolation = true;

    #endregion

        //TODO 問題:不懂為什麼要用 ulong ?
        /// <summary> 更新率 </summary>
        public ulong updateRate;
        /// <summary> 上一次的時間點 </summary>
        public Date lastDate;
        /// <summary> For Additions </summary>
        private ulong nextUpdate;

        public Server server                = null;
        public int    lag                   = 0;
        public int    input_sequence_number = 0;

    #region ========== Constructor ==========

        // Default update rate.
        public Client() => SetUpdateRate(50);

    #endregion

    #region ========== Public Methods ==========

        /// <summary> For Additions Tick </summary>
        public void AdditionTick()
        {
            var now = new Date();
            if (now < nextUpdate)
            {
                return;
            }

            nextUpdate = now + updateRate;
            Update();
        }

    #endregion


    #region ========== Privates Methods ==========

        private void SetUpdateRate(ulong rate)
        {
            updateRate = rate;
        }

        // Update Client state.
        private void Update()
        {
            ProcessServerMessages();      // 收聽伺服器 Listen to the server.
            if (entityID == null) return; // 尚未連接 Not connected yet.
            ProcessInputs();              // 過程輸入 Process inputs.
            if (entity_interpolation)     // 插入其他實體 Interpolate other entities.
                InterpolateEntities();
            RenderWorld(); // 渲染世界 Render the World.
        }

        // Show some info.
        public string ShowSomeInfo()
        {
            return "Non-acknowledged inputs: " + pendingInputs.Count;
        }

        // Get inputs and send them to the server.
        // If enabled, do client-side prediction.
        private void ProcessInputs()
        {
            // 計算自上次更新以來的增量時間 (Compute delta time since last update.)
            var nowDate    = new Date();
            var lastDate   = this.lastDate ?? nowDate;
            var betweenSec = (nowDate - lastDate) / 1000d;
            this.lastDate = nowDate;

            // 打包 player's input.
            Input input = null;

            if (keyRight) input     = new Input { press_time = betweenSec };
            else if (keyLeft) input = new Input { press_time = -betweenSec };
            else return; // 沒有發生任何有趣的事情

            // Send the input to the server.
            input.input_sequence_number = input_sequence_number++;
            input.entity_id             = entityID.Value;
            server.network.Send(lag, input);

            // Do client-side prediction.
            if (client_side_prediction)
            {
                entities[entityID.Value].ApplyInput(input);
            }

            // Save this input for later reconciliation.
            pendingInputs.Add(input);
        }

        private void ProcessServerMessages()
        {
            while (true)
            {
                var msg = network.Receive();
                if (msg == null) break;

                // :: Addition (cast to list of entity states)
                var message = (List<EntityState>)msg;

                // World state is a list of entity states.
                for (var i = 0; i < message.Count; i++)
                {
                    var state = message[i];

                    // If this is the first time we see this entity, create a local representation.
                    if (entities.Count <= state.entity_id)
                    {
                        // :: Addition (changed name entity => newEntity to avoid compile error)
                        var newEntity = new Entity();
                        newEntity.ID = state.entity_id;

                        if (entities.Count <= state.entity_id)
                            entities.Add(newEntity);
                        else
                            entities[state.entity_id] = newEntity;
                    }

                    var entity = entities[state.entity_id];

                    if (state.entity_id == entityID)
                    {
                        // Received the authoritative position of this client's entity.
                        entity.X = state.position;

                        if (server_reconciliation)
                        {
                            // Server Reconciliation. Re-apply all the inputs not yet processed by
                            // the server.
                            var j = 0;
                            while (j < pendingInputs.Count)
                            {
                                var input = pendingInputs[j];
                                if (input.input_sequence_number <= state.last_processed_input)
                                {
                                    // Already processed. Its effect is already taken into account into the world update
                                    // we just got, so we can drop it.
                                    pendingInputs.RemoveAt(j);
                                }
                                else
                                {
                                    // Not processed by the server yet. Re-apply it.
                                    entity.ApplyInput(input);
                                    j++;
                                }
                            }
                        }
                        else
                        {
                            // Reconciliation is disabled, so drop all the saved inputs.
                            pendingInputs.Clear();
                        }
                    }
                    else
                    {
                        // Received the position of an entity other than this client's.

                        if (!entity_interpolation)
                        {
                            // Entity interpolation is disabled - just accept the server's position.
                            entity.X = state.position;
                        }
                        else
                        {
                            // Add it to the position buffer.
                            var timestamp = new Date();
                            entity.PositionBuffer.Add(new TimedPosition()
                            {
                                timestamp = timestamp,
                                position  = state.position
                            });
                        }
                    }
                }
            }
        }

        private void InterpolateEntities()
        {
            // Compute render timestamp.
            var now              = new Date();
            var render_timestamp = now - (1000d / server.update_rate);

            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                // No point in interpolating this client's entity.
                if (entity.ID == entityID)
                {
                    continue;
                }

                // Find the two authoritative positions surrounding the rendering timestamp.
                var buffer = entity.PositionBuffer;

                // Drop older positions.
                while (buffer.Count >= 2 && buffer[1].timestamp <= render_timestamp)
                {
                    buffer.RemoveAt(0);
                }

                // Interpolate between the two surrounding authoritative positions.
                if (buffer.Count >= 2 && buffer[0].timestamp <= render_timestamp &&
                    render_timestamp <=
                    buffer[1].timestamp) // :: Addition (instead of nested array, use struct to access named fields)
                {
                    var x0 = buffer[0].position;
                    var x1 = buffer[1].position;
                    var t0 = buffer[0].timestamp;
                    var t1 = buffer[1].timestamp;

                    entity.X = (float)(x0 + (x1 - x0) * (render_timestamp - t0) / (t1 - t0));
                }
            }
        }

        private void RenderWorld()
        {
            // TODO ?
            LocalWorldCanvas.instance.RenderWorld(entityID.Value + 1, entities);
        }

    #endregion
    }
}