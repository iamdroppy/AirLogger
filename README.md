# AirLogger
PacketLogger for HabboAir
http://forum.ragezone.com/f353/habboair-air63-201708251331-359388093-crack-1147564/


---

## How it works

The keys exchange is an essential part of the EvaWire protocol, which enables secure communication between nodes on the network. In this context, "keys" refer to RSA encryption keys used for secure data transmission. The local and remote nodes establish each other's public and private keys through a process called key exchange. This is done to ensure that both parties can encrypt and decrypt messages using their shared secret key.

The key exchange involves several steps. Initially, the local node generates a pair of public and private RSA keys, which are used for encryption and decryption, respectively. The remote node receives this information and responds with its own public key. The local node then verifies the remote node's public key to ensure it matches the expected value. If successful, the local node sends its private key to the remote node, which uses this information to compute a shared secret key.

The shared secret key is used for encrypting and decrypting messages between nodes. This key is derived from the RSA keys exchanged during the key exchange process using a mathematical formula called Diffie-Hellman key exchange. By sharing this key, both parties can ensure that all encrypted messages are secure and only accessible to authorized recipients. The key exchange process also enables the remote node to verify the local node's identity and authenticity, ensuring that data transmission is trustworthy and tamper-proof.


**Key exchange mechanism**

The code implements a key exchange mechanism between two parties, typically referred to as Alice and Bob. The key exchange is performed using RSA encryption keys. Alice generates a pair of RSA keys (public and private), while Bob already has a public key that he shares with Alice.

When Alice wants to communicate with Bob, she uses her private key to encrypt her message before sending it to Bob's server. Meanwhile, Bob sends his public key to Alice's server, which is used by Alice to decrypt the incoming messages. The key exchange process involves two rounds: 

1.  In the first round, Bob sends his public key to Alice's server along with the encrypted message.
2.  In the second round, Alice uses her private key to decrypt the received message and verifies it using Bob's shared public key.

This ensures that only authorized parties can intercept and read each other's messages, as they use their respective encryption keys to secure the communication.

**Use of cryptographic primitives**

The code uses various cryptographic primitives such as RSA encryption, RC4 stream cipher for data encryption/decryption. This enables secure communication between Alice and Bob by protecting against interception or eavesdropping attacks.

---

Mostly, this is a C# implementation of the Diffie-Hellman key exchange protocol for secure communication over an insecure channel. Here's a breakdown of the code:

* `_host`: The hostname or IP address of the remote server.
* `_port`: The port number used to connect to the remote server.
* `_connection`: An instance of `HConnection`, which is a custom class that represents the network connection.
* `_localKeys` and `_remoteKeys`: Instances of `HKeyExchange`, which are used for key exchange and encryption/decryption.
* `_incomingOffset`: The offset in the incoming data where the handshake begins.
* `_localSharedKey` and `_remoteSharedKey`: The shared keys used for encryption and decryption.
* `Start()`: Initializes the connection and starts the handshake process.
* `OnOutgoingData()`: Handles outgoing data from the client, including processing the packet's step number and replacing public keys or signed primes as necessary.
* `OnIncomingData()`: Handles incoming data from the server, including processing the packet's step number and replacing public keys or signed primes as necessary.
* `InitializeKeys()`: Initializes the local and remote key exchange objects using the Diffie-Hellman parameters.
* `FinalizeHandshake()`: Finalizes the handshake process by resetting the `_incomingOffset` and setting the local and remote decryption/encryption state.
* `ReplaceLocalPublicKey()` and `ReplaceRemotePublicKey()`: Replaces the public keys in the packet with the generated shared key.
* `ReplaceRemoteSignedPrimes()`: Replaces the signed primes in the packet with the generated DH prime values.
* `CancelHandshake()`: Cancels the handshake process by disposing of the local and remote key exchange objects and resetting the decryption/encryption state.

**Key Exchange**

The Diffie-Hellman key exchange is implemented using the `HKeyExchange` class, which takes three parameters:

* `expponent`: The public exponent used for encryption and decryption.
* `modulus`: The modulus value used for the DH algorithm.
* `privateExponent`: The private exponent used for signing.

The local and remote key exchange objects are initialized with the Diffie-Hellman parameters:

* `_localKeys = new HKeyExchange(REAL_EXPONENT, REAL_MODULUS)`: Initializes the local key exchange object using the real Diffie-Hellman parameters.
* `_remoteKeys = new HKeyExchange(FAKE_EXPONENT, FAKE_MODULUS, FAKE_PRIVATE_EXPONENT)`: Initializes the remote key exchange object using fake Diffie-Hellman parameters.

**Encryption and Decryption**

The encryption/decryption state is managed by the `Local` and `Remote` properties, which are instances of `HNode`. These objects represent the client and server nodes in the communication process. The decryption/encryption state is set when a packet is received from the other party:

* `Local.Decrypter = new RC4(_localSharedKey)`: Initializes the local decryption object using the shared key.
* `Remote.Encrypter = new RC4(_remoteSharedKey)`: Initializes the remote encryption object using the shared key.

The `ReplaceRemotePublicKey()` and `ReplaceLocalPublicKey()` methods replace the public keys in the packet with the generated shared key, allowing the client and server to authenticate each other's identities. The `ReplaceRemoteSignedPrimes()` method replaces the signed primes in the packet with the generated DH prime values, completing the Diffie-Hellman key exchange.

Overall, this code implements a secure communication protocol using Diffie-Hellman key exchange, encryption/decryption, and authentication mechanisms.
