using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phoenix;
using Constants;
using Phoenix.WebSocketImpl;
using Packets;

public class SocketService : MonoBehaviour
{
    public static SocketService instance { get; private set; }
    public Socket socket;
    public Channel mainChannel;
    public Channel roomChannel;
    public bool connected;

    private Queue replyPackets = new Queue();
    private Queue messagePackets = new Queue();

    private string pausedChannel = null;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void OnDestroy()
    {
        this.Disconnect();
    }

    public bool isChannelAlive(Channel channel)
    {
        return channel.State == ChannelState.Joined || channel.State == ChannelState.Joining;
    }

    public void Connect(string token)
    {
        this.JoinSocket(token);

        if (this.socket.State == WebsocketState.Open)
        {
            this.JoinMain();
        }
        else
        {
            this.Disconnect();
        };

        this.connected = this.socket.State == WebsocketState.Open && (this.mainChannel.State == ChannelState.Joined || this.mainChannel.State == ChannelState.Joining);
    }

    public void Disconnect()
    {
        if (this.mainChannel != null)
        {
            this.mainChannel.Leave();
            this.mainChannel = null;
        }

        if (this.roomChannel != null)
        {
            this.roomChannel.Leave();
            this.roomChannel = null;
        }

        if (this.socket != null && (this.socket.State == WebsocketState.Open || this.socket.State == WebsocketState.Connecting))
        {
            this.socket.Disconnect();
        }

        this.mainChannel = null;
        this.roomChannel = null;
        this.socket = null;
        this.connected = false;
    }

    public void RoomPush(string packet, Dictionary<string, object> payload)
    {
        string channelName = ClientService.instance.room;
        if (channelName == null) return;
        if (this.roomChannel == null || this.roomChannel.State != ChannelState.Joined) return;

        this.Push(channelName, packet, payload);
    }

    public void Push(string channelName, string packet, Dictionary<string, object> payload)
    {
        Channel channel = channelName == "main" ? this.mainChannel : this.roomChannel;
        if (channel == null)
        {
            Debug.LogWarning("[ERROR] Tried pushing on null channel " + channelName + " -> " + packet);
            return;
        }

        channel.Push(packet, payload)
            .Receive(ReplyStatus.Ok, m => replyPackets.Enqueue(new ReplyQueue(roomChannel, packet, m)))
            .Receive(ReplyStatus.Error, m => replyPackets.Enqueue(new ReplyQueue(roomChannel, packet, m)))
            .Receive(ReplyStatus.Timeout, m => replyPackets.Enqueue(new ReplyQueue(roomChannel, packet, m)));
    }

    private void JoinSocket(string token)
    {
        Socket.Options options = new Socket.Options(new JsonMessageSerializer())
        {
            ReconnectAfter = null,
            RejoinAfter = null
        };

        this.socket = new Socket(EnvConstants.WEBSOCKET_ENDPOINT, Packets.Builder.SocketLogin(token), new WebsocketSharpFactory(), options);
        this.socket.Connect();
    }

    private void JoinMain()
    {
        this.mainChannel = socket.Channel("main");
        this.mainChannel.On(Message.InBoundEvent.Close, m => messagePackets.Enqueue(new MessageQueue(mainChannel, "main_dc", m)));

        foreach (string e in ServerConstants.MAIN_INBOUND_EVENTS)
        {
            this.mainChannel.On(e, m => messagePackets.Enqueue(new MessageQueue(mainChannel, e, m)));
        }

        this.mainChannel.Join()
            .Receive(ReplyStatus.Ok, r => replyPackets.Enqueue(new ReplyQueue(mainChannel, "main_join", r)))
            .Receive(ReplyStatus.Timeout, (r) =>
            {
                this.mainChannel = null;
                replyPackets.Enqueue(new ReplyQueue(mainChannel, "main_join", r));
            })
            .Receive(ReplyStatus.Error, (r) =>
            {
                this.mainChannel = null;
                replyPackets.Enqueue(new ReplyQueue(mainChannel, "main_join", r));
            });
    }
    public void JoinRoom(string uuid)
    {
        if (this.roomChannel != null)
        {
            if (this.roomChannel.State != ChannelState.Closed && this.roomChannel.State != ChannelState.Errored)
                this.roomChannel.Leave();
        }

        this.roomChannel = socket.Channel(string.Format("room:{0}", uuid));

        // Listening to room events
        this.roomChannel.On(Message.InBoundEvent.Close, m => messagePackets.Enqueue(new MessageQueue(roomChannel, "room_dc", m)));

        foreach (string e in ServerConstants.ROOM_INBOUND_EVENTS)
        {
            this.roomChannel.On(e, m => messagePackets.Enqueue(new MessageQueue(roomChannel, e, m)));
        }

        this.roomChannel.Join()
            .Receive(ReplyStatus.Ok, r => replyPackets.Enqueue(new ReplyQueue(roomChannel, "room_join", r)))
            .Receive(ReplyStatus.Timeout, (r) =>
            {
                this.roomChannel = null;
                replyPackets.Enqueue(new ReplyQueue(roomChannel, "room_join", r));
            })
            .Receive(ReplyStatus.Error, (r) =>
            {
                this.roomChannel = null;
                replyPackets.Enqueue(new ReplyQueue(roomChannel, "room_join", r));
            });
    }
    private void FixedUpdate()
    {
        /* Handle Reply Packets from Client Push */
        lock (this.replyPackets)
        {
            while (this.replyPackets.Count > 0)
            {
                ReplyQueue packet = (ReplyQueue)this.replyPackets.Dequeue();
                PayloadReader.ParseReply(packet);
            }
        }

        /* Handle Packets from Server */
        lock (this.messagePackets)
        {
            while (this.messagePackets.Count > 0)
            {
                MessageQueue packet = (MessageQueue)this.messagePackets.Dequeue();
                PayloadReader.ParseMessage(packet);
            }
        }

        /* Handle Main Channel disconnection */
        if (
            (this.connected && this.socket.State != WebsocketState.Open) ||
            (this.connected && this.mainChannel != null && !this.isChannelAlive(this.mainChannel))
        )
        {
            SocketService.instance.Disconnect();
            ClientService.instance.Logout();
        }

        /* Handle Room Channel disconnection */
        if (this.connected && roomChannel != null && !this.isChannelAlive(this.roomChannel))
        {
            this.roomChannel.Leave();
            this.roomChannel = null;
            ClientService.instance.LoadLobby();
        }
    }

    public void OnApplicationPause(bool paused)
    {
        if (paused && this.roomChannel != null)
        {
            this.pausedChannel = RoomController.instance.room.room_id;
            this.roomChannel.Leave();
            this.roomChannel = null;
        }
        else if (!paused && this.pausedChannel != null)
        {
            this.JoinRoom(this.pausedChannel);
            this.pausedChannel = null;
        }
    }
}
