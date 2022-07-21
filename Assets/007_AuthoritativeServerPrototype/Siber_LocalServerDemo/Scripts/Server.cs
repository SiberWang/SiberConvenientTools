using System.Collections.Generic;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class Server
    {
        private List<string> clientIDList = new List<string>();
        
        public void Tick()
        {
            throw new System.NotImplementedException();
        }
        
        public void TakeInput(InputAction inputAction)
        {
            // Debug.Log($"Cache inputAction from Client:{} , action:{}");
        }

        public void ThrowResult()
        {
            
        }

        private void Calculation()
        {
            
        }

        public void AddClient(string clientID)
        {
            clientIDList.Add(clientID);
        }
    }
}