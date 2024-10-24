using log4net;
using Mono.Addins;
using Nini.Config;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using SIPSorcery.Net;
using System.Reflection;
using WebSocketSharp;

[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]
namespace OpenSim.WebRTC
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "WebRTC")]
    public class WebRTC : ISharedRegionModule
    {
        static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static int SIP_LISTEN_PORT = 5060;
        private const int WEBSOCKET_PORT = 8081;
        private RTCPeerConnection _peerConnection;
        private WebSocket _webSocket;
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
            RTCConfiguration config = new RTCConfiguration
            {
                iceServers = new List<RTCIceServer>
               {
                   new RTCIceServer { urls = "stun:stun.l.canadiangrid.ca:8081" }
               }
            };
            _peerConnection = new RTCPeerConnection(config);
            _peerConnection.onicecandidate += (candidate) =>
            {
                m_log.Info($"ICE Candidate: {candidate}");
            };

            // Establish WebSocket connection for signaling
            _webSocket = new WebSocket("wss://canadiangrid.ca");
            _webSocket.OnMessage += (sender, e) =>
            {
                var message = e.Data;
                m_log.Info($"Signaling Message: {message}");

                if (message.Contains("offer"))
                {
                    var offer = new RTCSessionDescriptionInit
                    {
                        type = RTCSdpType.offer,
                        sdp = message
                    };
                    _peerConnection.setRemoteDescription(offer);
                    var answer = _peerConnection.CreateAnswer(null);
                    //_peerConnection.setLocalDescription(answer);
                    _webSocket.Send(answer.URI);
                }
                else if (message.Contains("answer"))
                {
                    var answer = new RTCSessionDescriptionInit
                    {
                        type = RTCSdpType.answer,
                        sdp = message
                    };
                    _peerConnection.setRemoteDescription(answer);
                }
                else if (message.Contains("candidate"))
                {
                    var candidate = new RTCIceCandidateInit
                    {
                        candidate = message
                    };
                    _peerConnection.addIceCandidate(candidate);
                }
            };

            _webSocket.Connect();
            m_log.Info("WebSocket Signaling Connected.");
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
       
    }
}
