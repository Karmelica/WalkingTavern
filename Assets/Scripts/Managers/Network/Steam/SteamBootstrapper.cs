using System;
using Steamworks;
using UnityEngine;

namespace Managers.Network.Steam
{
    public class SteamBootstrapper : MonoBehaviour
    {
//#if !UNITY_EDITOR
        private void Start()
        {
            if (!SteamClient.IsValid)
            {
                Debug.LogError("Nie udało się zainicjalizować Steam!");
                return;
            }
            Debug.Log($"Zalogowano jako {SteamClient.Name} ({SteamClient.SteamId})");

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
//#endif
    }
}