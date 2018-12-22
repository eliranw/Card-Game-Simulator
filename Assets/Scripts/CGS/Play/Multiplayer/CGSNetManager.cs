﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using UnityEngine;
using Mirror;

using CardGameView;

namespace CGS.Play.Multiplayer
{
    public class CGSNetManager : NetworkManager
    {
        public const string PlayerCountMessage = "Number of connected players: ";
        public const string ConnectionIdMessage = "Connection Id: ";

        public static CGSNetManager Instance => (CGSNetManager)singleton;
        public CGSNetPlayer LocalPlayer { get; set; }
        public CGSNetData Data { get; set; }

        public GameObject cardModelPrefab;
        public PlayMode playController;
        public PointsCounter pointsDisplay;

        public override void OnStartServer()
        {
            base.OnStartServer();
            // TODO: CardGameManager.Instance.Discovery.StartAsHost();
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);
            if (Data == null)
            {
                Data = Instantiate(spawnPrefabs[0]).GetOrAddComponent<CGSNetData>();
                NetworkServer.Spawn(Data.gameObject);
            }
            Data.RegisterScore(conn.playerControllers[playerControllerId].gameObject, CardGameManager.Current.GameStartPointsCount);
            playController.netText.text = PlayerCountMessage + NetworkServer.connections.Count.ToString();
        }

        public override void OnStartClient(NetworkClient netClient)
        {
            base.OnStartClient(netClient);
            ClientScene.RegisterSpawnHandler(cardModelPrefab.GetComponent<NetworkIdentity>().assetId, SpawnCard, UnSpawnCard);
        }

        public override void OnClientConnect(NetworkConnection connection)
        {
            base.OnClientConnect(connection);
            playController.netText.text = ConnectionIdMessage + connection.connectionId;
        }

        public GameObject SpawnCard(Vector3 position, NetworkHash128 assetId)
        {
            GameObject newCardGO = Instantiate(cardModelPrefab, playController.playAreaContent);
            playController.SetPlayActions(playController.playAreaContent.GetComponent<CardStack>(), newCardGO.GetComponent<CardModel>());
            return newCardGO;
        }

        public void UnSpawnCard(GameObject spawned)
        {
            Destroy(spawned);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            // TODO: CardGameManager.Instance.Discovery.sTOP();
            Debug.Log("Server stopped");
        }

        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            base.OnServerError(conn, errorCode);
            Debug.LogError("Server error:" + errorCode);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("Server disconnected");
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            base.OnClientError(conn, errorCode);
            Debug.LogError("Client error:" + errorCode);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            Debug.Log("Client Disconnected");
        }
    }
}
