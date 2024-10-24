using log4net;
using Mono.Addins;
using Nini.Config;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using SIPSorcery.Net;
using SIPSorcery.SIP.App;
using SIPSorcery.SIP;
using SIPSorceryMedia.Abstractions;
using System.Net;
using System.Reflection;
using System.Text;
using WebSocketSharp.Server;

[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]
namespace OpenSim.WebRTC
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "WebRTC")]
    public class WebRTC : ISharedRegionModule
    {
        static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static int SIP_LISTEN_PORT = 5060;
        private const int WEBSOCKET_PORT = 8081;
        private static RTCPeerConnection _peerConnection;
        private static RTPSession _rtpSession;
        public Type? ReplaceableInterface
        {
            get { return null; }
        }

        public string Name
        {
            get { return "WebRTC"; }
        }
        public WebRTC()
        {
            /*
            Console.WriteLine("SIPSorcery SIP to WebRTC example.");
            Console.WriteLine("Press ctrl-c to exit.");

            // Plumbing code to facilitate a graceful exit.
            CancellationTokenSource exitCts = new CancellationTokenSource(); // Cancellation token to stop the SIP transport and RTP stream.

            //EnableTraceLogs(sipTransport);

            // Start web socket.
            Console.WriteLine("Starting web socket server...");
            var webSocketServer = new WebSocketServer(IPAddress.Any, WEBSOCKET_PORT);
            webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/", (peer) => peer.CreatePeerConnection = CreatePeerConnection);
            webSocketServer.Start();

            // Set up a default SIP transport.
            var sipTransport = new SIPTransport();
            sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, SIP_LISTEN_PORT)));

            // Create a SIP user agent to receive a call from a remote SIP client.
            // Wire up event handlers for the different stages of the call.
            var userAgent = new SIPUserAgent(sipTransport, null, true);

            // We're only answering SIP calls, not placing them.
            userAgent.OnCallHungup += (dialog) =>
            {
                m_log.Info($"Call hungup by remote party.");
                exitCts.Cancel();
            };
            //userAgent.ServerCallCancelled += (uas) => m_log.Info("Incoming call cancelled by caller.");
            userAgent.OnIncomingCall += async (ua, req) =>
            {
                m_log.Info($"Incoming call request from {req.RemoteSIPEndPoint}: {req.StatusLine}.");
                var incomingCall = userAgent.AcceptCall(req);

                var rtpSession = new RTPSession(false, false, false);
                rtpSession.AcceptRtpFromAny = true;
                MediaStreamTrack audioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                    new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) });
                rtpSession.addTrack(audioTrack);

                await userAgent.Answer(incomingCall, rtpSession);
                rtpSession.OnRtpPacketReceived += ForwardMediaToPeerConnection;

                m_log.Info($"Answered incoming call from {req.Header.From.FriendlyDescription()} at {req.RemoteSIPEndPoint}.");

                _rtpSession = rtpSession;
            };

            Console.WriteLine($"Waiting for browser web socket connection to {webSocketServer.Address}:{webSocketServer.Port}...");
            var contactURI = new SIPURI(SIPSchemesEnum.sip, sipTransport.GetSIPChannels().First().ListeningSIPEndPoint);
            Console.WriteLine($"Waiting for incoming SIP call to {contactURI}.");

            // Ctrl-c will gracefully exit the call at any point.
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                exitCts.Cancel();
            };

            // Wait for a signal saying the call failed, was cancelled with ctrl-c or completed.
            exitCts.Token.WaitHandle.WaitOne();

            #region Cleanup.

            m_log.Info("Exiting...");

            _rtpSession?.Close("app exit");

            if (userAgent != null)
            {
                if (userAgent.IsCallActive)
                {
                    m_log.Info($"Hanging up call to {userAgent?.CallDescriptor?.To}.");
                    userAgent.Hangup();
                }

                // Give the BYE or CANCEL request time to be transmitted.
                m_log.Info("Waiting 1s for call to clean up...");
                Task.Delay(1000).Wait();
            }

            if (sipTransport != null)
            {
                m_log.Info("Shutting down SIP transport...");
                sipTransport.Shutdown();
            }

            #endregion
            */
        }
        public void AddRegion(Scene scene)
        {
            //
        }

        public void Close()
        {
            //
        }

        public void Initialise(IConfigSource source)
        {
            m_log.Info("[WebRTC]: Starting WebRTC");
        }

        public void PostInitialise()
        {
            //
        }

        public void RegionLoaded(Scene scene)
        {
            //
        }

        public void RemoveRegion(Scene scene)
        {
            //
        }
        private static Task<RTCPeerConnection> CreatePeerConnection()
        {
            var pc = new RTCPeerConnection(null);

            MediaStreamTrack track = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) });
            pc.addTrack(track);
            pc.onconnectionstatechange += (state) => m_log.Debug($"Peer connection state change to {state}.");
            pc.OnRtpPacketReceived += ForwardMediaToSIP;
            _peerConnection = pc;

            return Task.FromResult(pc);
        }
        private static void ForwardMediaToSIP(IPEndPoint remote, SDPMediaTypesEnum mediaType, RTPPacket rtpPacket)
        {
            if (_rtpSession != null && mediaType == SDPMediaTypesEnum.audio)
            {
                _rtpSession.SendAudio((uint)rtpPacket.Payload.Length, rtpPacket.Payload);
            }
        }
        private static void ForwardMediaToPeerConnection(IPEndPoint remote, SDPMediaTypesEnum mediaType, RTPPacket rtpPacket)
        {
            if (_peerConnection != null && mediaType == SDPMediaTypesEnum.audio)
            {
                _peerConnection.SendAudio((uint)rtpPacket.Payload.Length, rtpPacket.Payload);
            }
        }
    }
}
