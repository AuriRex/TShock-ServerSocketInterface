using System;
using System.Net.Sockets;
using System.Threading;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace ServerSocketInterface {
    /// <summary>
    /// The main plugin class should always be decorated with an ApiVersion attribute. The current API Version is 1.25
    /// </summary>
    [ApiVersion(2, 1)]
    public class ServerSocketInterface : TerrariaPlugin
    {

        public static ServerSocketInterface instance;

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public override string Name => "Server Socket Interface";

        /// <summary>
        /// The version of the plugin in its current state.
        /// </summary>
        public override Version Version => new Version(1, 2, 0);

        /// <summary>
        /// The author(s) of the plugin.
        /// </summary>
        public override string Author => "AuriRex";

        /// <summary>
        /// A short, one-line, description of the plugin's purpose.
        /// </summary>
        public override string Description => "Creates a Socket to receive and send data to other local programms.";

        /// <summary>
        /// The plugin's constructor
        /// Set your plugin's order (optional) and any other constructor logic here
        /// </summary>
        public ServerSocketInterface(Main game) : base(game)
        {
            Order = 100;
        }

        Thread socketThread = null;
        Thread senderThread = null;

        /// <summary>
        /// Performs plugin initialization logic.
        /// Add your hooks, config file read/writes, etc here
        /// </summary>
        public override void Initialize() //First method TShock runs when the plugin is loaded
        {
            instance = this;
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);

            //TShockAPI.Utils.Instance.
            socketThread = new Thread(AsynchronousSocketListener.StartListening);
            socketThread.Start();
            senderThread = new Thread(AsynchronousSocketSender.StartLoop);
            senderThread.Start();
            AsynchronousSocketSender.Send("ServerInit: true");
            // Console.WriteLine("I'm not stuck :3");
        }

        private void OnServerBroadcast(ServerBroadcastEventArgs args) {
            if(args != null) {
                if (args.Message == null) return;
                if (args.Color == null) return;
                AsynchronousSocketSender.Send($"ServerBroadcast: {args.Message}\n{args.Color.ToString()}");
            }
            
        }

        private void OnGamePostInitialize(EventArgs args) {
            AsynchronousSocketSender.Send("GamePostInitialize: true");
        }

        public void SendMsg(String msg) {
            if (TShockAPI.TShock.Players != null)
                foreach (TShockAPI.TSPlayer ply in TShockAPI.TShock.Players) {
                    if(ply != null)
                        //ply.SendSuccessMessage(msg);
                        ply.SendMessage(msg, 255, 255, 255);
                }
        }

        public void Restart(String reason) {
            TShockAPI.Utils.Instance.RestartServer(true, reason);
        }

        public String GetOnlinePlayers() {
            String ret = "";
            if(TShockAPI.TShock.Players != null)
                foreach (TShockAPI.TSPlayer ply in TShockAPI.TShock.Players) {
                    if(ply != null)
                        ret += ply.Name + "\n" + ply.IP + "\n";
                }
            if (ret.Equals(""))
                ret = "None";
            return ret;
        }

        public String getItemName(int id) {
            String ret = "";

            try {
                ret = TShockAPI.Utils.Instance.GetItemById(id).Name;
                
            } catch (Exception ex) {
                ret = "Exception";
            }

            if (ret.Equals("")) ret = "Error";

            return ret;
        }

        public String getPrefixName(int id) {
            String ret = "";

            try {
                ret = TShockAPI.Utils.Instance.GetPrefixById(id);
            } catch (Exception ex) {
                ret = "Exception";
            }

            if (ret.Equals("")) ret = "Error";

            return ret;
        }

        void OnServerJoin(JoinEventArgs args) {
            if (args != null) {
                if (TShockAPI.TShock.Players[args.Who] != null) {
                    Console.WriteLine($"ServerJoin: {TShockAPI.TShock.Players[args.Who].Name}");
                    AsynchronousSocketSender.Send($"ServerJoin: {TShockAPI.TShock.Players[args.Who].Name}\n{TShockAPI.TShock.Players[args.Who].IP}");
                }
            }
        }
        
        void OnServerLeave(LeaveEventArgs args) {
            if(args != null) {
                if(TShockAPI.TShock.Players[args.Who] != null) {
                    AsynchronousSocketSender.Send($"ServerLeave: {TShockAPI.TShock.Players[args.Who].Name}\n{TShockAPI.TShock.Players[args.Who].IP}");
                }
            }
        }

        void OnServerChat(ServerChatEventArgs args) {
            if (args != null) {
                if (TShockAPI.TShock.Players[args.Who] != null) {
                    AsynchronousSocketSender.Send($"ServerChat: {TShockAPI.TShock.Players[args.Who].Name}\n{args.Text}");
                }
            }
        }

        /// <summary>
        /// Performs plugin cleanup logic
        /// Remove your hooks and perform general cleanup here
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AsynchronousSocketSender.Send("ServerShutdown: true");
                //unhook
                //dispose child objects
                //set large objects to null
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.ServerBroadcast.Deregister(this, OnServerBroadcast);


                try {
                    if (socketThread != null)
                        socketThread.Abort();
                } catch(Exception ex) {

                }
                AsynchronousSocketSender.Stop();

            }
            base.Dispose(disposing);
        }

    }
}
