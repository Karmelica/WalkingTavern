using System;
using Steamworks;
using UnityEngine;

namespace Managers.Network.Steam
{
    public class SteamBootstrapper : MonoBehaviour
    {
        private void Start()
        {
            if (!SteamClient.IsValid)
            {
                Debug.LogError("Nie udało się zainicjalizować Steam!");
                return;
            }

            try
            {
                SteamClient.Init( 480 );
                Debug.Log( "Steam initialized successfully." );
            }
            catch ( Exception e )
            {
                Debug.LogWarning( $"Steam initialization failed: {e.Message}" );
            }
            
        }

        private void Update()
        {
            SteamClient.RunCallbacks();
        }

        private void OnDisable()
        {
            SteamClient.Shutdown();
        }
    }
}