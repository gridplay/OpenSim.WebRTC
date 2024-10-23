namespace OpenSim.WebRTC
{
    public interface IWebRTC
    {
        Task<string> CreateOfferAsync();
        Task HandleAnswerAsync(string Sdp);
    }
}
