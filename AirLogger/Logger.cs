using System;
using System.Collections.Generic;
using System.Text;
using Sulakore.Network;
using System.Threading.Tasks;
using Sulakore.Crypto;
using Sulakore.Network.Protocol;
using System.Diagnostics;

namespace AirLogger
{
    class Logger
    {
        private string _host;
        private short _port;
        private HConnection _connection;

        public HNode Local => _connection.Local;
        public HNode Remote => _connection.Remote;

        public const int FAKE_EXPONENT = 3;
        public const string FAKE_MODULUS = "86851dd364d5c5cece3c883171cc6ddc5760779b992482bd1e20dd296888df91b33b936a7b93f06d29e8870f703a216257dec7c81de0058fea4cc5116f75e6efc4e9113513e45357dc3fd43d4efab5963ef178b78bd61e81a14c603b24c8bcce0a12230b320045498edc29282ff0603bc7b7dae8fc1b05b52b2f301a9dc783b7";
        public const string FAKE_PRIVATE_EXPONENT = "59ae13e243392e89ded305764bdd9e92e4eafa67bb6dac7e1415e8c645b0950bccd26246fd0d4af37145af5fa026c0ec3a94853013eaae5ff1888360f4f9449ee023762ec195dff3f30ca0b08b8c947e3859877b5d7dced5c8715c58b53740b84e11fbc71349a27c31745fcefeeea57cff291099205e230e0c7c27e8e1c0512b";

        public const int REAL_EXPONENT = 65537;
        public const string REAL_MODULUS = "e052808c1abef69a1a62c396396b85955e2ff522f5157639fa6a19a98b54e0e4d6e44f44c4c0390fee8ccf642a22b6d46d7228b10e34ae6fffb61a35c11333780af6dd1aaafa7388fa6c65b51e8225c6b57cf5fbac30856e896229512e1f9af034895937b2cb6637eb6edf768c10189df30c10d8a3ec20488a198063599ca6ad";

        private HKeyExchange _remoteKeys;
        private HKeyExchange _localKeys;
        private int _incomingOffset;
        private byte[] _localSharedKey;
        private byte[] _remoteSharedKey;

        public bool IsReceiving { get; private set; }
        public bool IsLogging { get; internal set; }

        public Logger(string host, short port)
        {
            _host = host;
            _port = port;
        }

        public async Task Start()
        {
            _connection = new HConnection();
            _connection.Connected += OnConnect;
            _connection.DataIncoming += OnIncomingData;
            _connection.DataOutgoing += OnOutgoingData;
            await _connection.InterceptAsync(HotelEndPoint.Parse("34.232.39.163", 30000));
        }

        private void OnOutgoingData(object sender, DataInterceptedEventArgs e)
        {
            Console.WriteLine($"[C>S][{e.Packet.Id}] {e.Packet.ToString()}");
            try
            {
                switch (e.Packet.Id)
                {
                    case 1767:
                        {
                            ReplaceLocalPublicKey(e);
                            break;
                        }
                }
            }
            catch { CancelHandshake(e); }
            finally { e.IsBlocked = false; }
        }

        private void OnIncomingData(object sender, DataInterceptedEventArgs e)
        {
            Console.WriteLine($"[S>C][{e.Packet.Id}] {e.Packet.ToString()}");

            try
            {
                if (e.Step < 3 && e.Packet.BodyLength == 2)
                {
                    _incomingOffset++;
                    return;
                }
                switch (e.Packet.Id)
                {
                    case 884:
                        {
                            InitializeKeys();
                            ReplaceRemoteSignedPrimes(e);
                            break;
                        }
                    case 1838:
                        {
                            ReplaceRemotePublicKey(e);
                            break;
                        }
                }
            }
            catch { CancelHandshake(e); }
            finally { e.IsBlocked = false; }
        }
        private void InitializeKeys()
        {
            _remoteKeys = new HKeyExchange(REAL_EXPONENT, REAL_MODULUS);
            _localKeys = new HKeyExchange(FAKE_EXPONENT, FAKE_MODULUS, FAKE_PRIVATE_EXPONENT);
        }
        private void FinalizeHandshake()
        {
            _incomingOffset = 0;
            IsReceiving = false;
        }

        private void ReplaceLocalPublicKey(DataInterceptedEventArgs e)
        {
            string localPublicKey = e.Packet.ReadUTF8();
            _localSharedKey = _localKeys.GetSharedKey(localPublicKey);

            // Use the same padding the client used when encrypting our public key.
            _remoteKeys.Padding = _localKeys.Padding;

            string remotePublicKey = _remoteKeys.GetPublicKey();
            e.Packet = new EvaWirePacket(e.Packet.Id, remotePublicKey);
        }
        private void ReplaceRemotePublicKey(DataInterceptedEventArgs e)
        {
            string remotePublicKey = e.Packet.ReadUTF8();
            _remoteSharedKey = _remoteKeys.GetSharedKey(remotePublicKey);

            // Use the same padding the server used when signing our public key.
            _localKeys.Padding = _remoteKeys.Padding;

            string localPublicKey = _localKeys.GetPublicKey();
            e.Packet = new EvaWirePacket(e.Packet.Id, localPublicKey);

            _localKeys.Dispose();
            _remoteKeys.Dispose();

            Local.Decrypter = new RC4(_localSharedKey);
            Remote.Encrypter = new RC4(_remoteSharedKey);
            Local.IsDecrypting = true;
            Remote.IsEncrypting = true;
        }
        private void ReplaceRemoteSignedPrimes(DataInterceptedEventArgs e)
        {
            string remoteP = e.Packet.ReadUTF8();
            string remoteG = e.Packet.ReadUTF8();
            _remoteKeys.VerifyDHPrimes(remoteP, remoteG);

            // Use the same padding the server used when signing our generated primes.
            _localKeys.Padding = _remoteKeys.Padding;

            string localP = _localKeys.GetSignedP();
            string localG = _localKeys.GetSignedG();
            e.Packet = new EvaWirePacket(e.Packet.Id, localP, localG);
        }

        private void CancelHandshake(DataInterceptedEventArgs e)
        {
            e.Restore();
            FinalizeHandshake();

            Local.Decrypter = null;
            Remote.Encrypter = null;

            _localKeys.Dispose();
            _remoteKeys.Dispose();
        }
        private void OnConnect(object sender, EventArgs e)
        {
            Console.WriteLine("Connected.");
        }
    }
}
