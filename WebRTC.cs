using log4net;
using Mono.Addins;
using Nini.Config;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using SIPSorcery.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenSim.WebRTC
{
    [Extension(Path = "/OpenSim/WebRTC", NodeName = "WebRTC", Id = "WebRTC")]
    public class WebRTC : WebRTCBase, IWebRTC, ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private RTCPeerConnection _peerConnection;

        public WebRTC(IConfigSource config) : base(config)
        {
            _peerConnection = new RTCPeerConnection();
        }
        public Type? ReplaceableInterface
        {
            get { return null; }
        }

        public string Name
        {
            get { return "WebRTC voice"; }
        }

        public void AddRegion(Scene scene)
        {
            //
        }

        public void Close()
        {
            _peerConnection.Close("Simulator shutting down");
        }

        public async Task<string> CreateOfferAsync()
        {
            var offer = _peerConnection.createOffer();
            await _peerConnection.setLocalDescription(offer);
            return offer.sdp;
        }

        public async Task HandleAnswerAsync(string sdp)
        {
            var remoteDescription = new RTCSessionDescriptionInit
            {
                sdp = sdp,
                type = RTCSdpType.answer
            };
            var tcs = new TaskCompletionSource<bool>();
            /*_peerConnection.onSetRemoteDescriptionComplete += (succeeded, message) =>
            {
                if (succeeded) tcs.SetResult(true);
                else tcs.SetException(new Exception(message));
            };*/
            _peerConnection.setRemoteDescription(remoteDescription);
            await tcs.Task;
        }

        public void Initialise(IConfigSource source)
        {
            _peerConnection = new RTCPeerConnection();
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
