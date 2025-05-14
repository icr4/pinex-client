using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Phoenix;
using Serializers.Payload;
using Models;
using UnityEngine;
using Constants;
using Serializers.Api.Login;

namespace Packets
{
    public static class PayloadReader
    {
        public static void ParseMessage(MessageQueue pMessage)
        {
            JObject payload;

            try
            {
                payload = JObject.FromObject(pMessage.message.Payload);
            }
            catch
            {
                payload = new();
            }

            if (!ServerConstants.IGNORED_EVENTS.Contains(pMessage.topic) && pMessage != null)
            {
                Debug.Log("[INCOMING] [" + pMessage.channel.Topic + ":(" + pMessage.topic + ")] -> " + payload);
            }
            if (!SocketService.instance.connected) return;

            switch (pMessage.topic)
            {
                default:
                    break;
                case "lookup_last_room":
                    if (LobbyController.instance == null) return;
                    LobbyController.instance.ToggleRejoin((string)payload["uid"]);
                    break;
                case "room_start":
                    string room = (string)payload["room_uid"];
                    ClientService.instance.room = room;

                    LoadingService.instance.enable();
                    SocketService.instance.JoinRoom(room);
                    break;
                case "room_running":
                    RoomController.instance.OnRoomIsRunning(payload.ToObject<Room>());
                    break;
                case "send_matchmaking":
                    ClientService.instance.LoadInvite(payload["user"].ToObject<User>().username, (string)payload["pool_id"]);
                    break;
                case "update_user":
                    ClientService.instance.user.coins = (int)payload["coins"];

                    if (LobbyController.instance == null) return;
                    LobbyController.instance.LoadUser(() => { });
                    break;
                case "draw_card":
                    RoomController.instance.OnPlayerDraw(
                        (string)payload["player_id"],
                        payload["draw_cards"].ToObject<List<Card>>(),
                        (Nullable<int>)payload["grounded_index"]
                    );

                    RoomController.instance.OnTurnChange(
                        (string)payload["player_id"],
                        "drop"
                    );

                    InterfaceController.instance.UpdateScores(payload["scores"].ToObject<List<Score>>());
                    break;
                case "drop_card":
                    RoomController.instance.OnPlayerDrop(
                        (string)payload["player_id"],
                        payload["card"].ToObject<Card>()
                    );

                    if (!payload.ContainsKey("terminate"))
                    {
                        RoomController.instance.OnTurnChange(
                            (string)payload["turn"],
                            "draw",
                            true
                        );

                        InterfaceController.instance.UpdateScores(payload["scores"].ToObject<List<Score>>());
                    }
                    else
                    {
                        RoomController.instance.OnRoomTerminate(
                            (string)payload["terminate"],
                            payload["scores"].ToObject<List<Score>>()
                        );
                    }

                    break;
                case "put_set":
                    RoomController.instance.OnPlayerPutSet(
                        (string)payload["player_id"],
                        (string)payload["set_id"],
                        payload["cards"].ToObject<List<Card>>()
                    );

                    InterfaceController.instance.UpdateScores(payload["scores"].ToObject<List<Score>>());
                    break;
                case "put_card":
                    RoomController.instance.OnPlayerPutCard(
                        (string)payload["player_id"],
                        (string)payload["set_id"],
                        payload["cards"].ToObject<List<Card>>(),
                        Enum.Parse<Position>((string)payload["position"]),
                        payload["replace_card"].ToObject<Card>()
                    );

                    InterfaceController.instance.UpdateScores(payload["scores"].ToObject<List<Score>>());
                    break;
                case "chat":
                    ChatController.instance.AddMessage((string)payload["player"], (string)payload["message"]);
                    break;
            }
        }

        public static void ParseReply(ReplyQueue pReply)
        {
            string topic = pReply.channel != null ? pReply.channel.Topic : "Unknown";
            string payload = null;
            ChannelReply channelReply = null;

            if (!ServerConstants.IGNORED_EVENTS.Contains(pReply.topic))
            {
                Debug.Log("[REPLY] [" + topic + ":(" + pReply.topic + "): " + pReply.reply.Status + "] -> " + pReply.reply.Response);
            }

            payload = pReply.reply.Response.ToString();

            try
            {
                channelReply = JsonConvert.DeserializeObject<ChannelReply>(payload);
            }
            catch
            {
                channelReply = null;
            }

            switch (pReply.topic)
            {
                case "main_join":
                    switch (pReply.reply.ReplyStatus)
                    {
                        default:
                            LoginController.instance.SetError(new LoginResponse() { error = "default" });
                            break;
                        case ReplyStatus.Ok:
                            JoinMainPayload mainJoin = JsonConvert.DeserializeObject<JoinMainPayload>(payload);
                            ClientService.instance.pools = mainJoin.pools;
                            ClientService.instance.user = mainJoin.user;
                            ClientService.instance.LoadLobby();
                            break;
                    }
                    break;

                case "room_join":
                    switch (pReply.reply.ReplyStatus)
                    {
                        default:
                            SocketService.instance.roomChannel = null;
                            ClientService.instance.LoadLobby(() =>
                            {
                                string description = TranslateService.instance.Translate("lobby.games.rejoin.error.description");
                                ConfirmService.instance.Show("lobby.games.rejoin.error.title", description, () => { });
                            });
                            break;
                        case ReplyStatus.Ok:
                            JoinRoomPayload data = JsonConvert.DeserializeObject<JoinRoomPayload>(payload);
                            ClientService.instance.LoadRoom(data.room, data.player);
                            break;
                    }
                    break;

                case "start_matchmaking":
                    switch (pReply.reply.ReplyStatus)
                    {
                        default:
                            break;
                        case ReplyStatus.Ok:
                            LobbyController.instance.ToggleMatchmaking();
                            break;
                    }
                    break;

                case "put_card":
                case "put_set":
                    switch (pReply.reply.ReplyStatus)
                    {
                        default:
                            break;
                        case ReplyStatus.Error:
                            InterfaceController.instance.LoadTooltip("room.tooltip.invalid_move");
                            break;
                    }
                    break;
            }
        }
    }
}