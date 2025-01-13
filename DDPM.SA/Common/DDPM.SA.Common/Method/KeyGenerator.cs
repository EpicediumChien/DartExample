using DDPM.SA.Obfuscation;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public class KeyGenerator
    {
        ILog local_log = null;

        byte[] _tKeyPair;
        byte[] _tPublicKey;
        byte[] _tPrivateKey;

        byte[] _skT1;
        byte[] _shrKey1;
        byte[] _nfw1;
        byte[] _nddpm1;
        byte[] _assd;

        //readonly byte[] cp1 = { 0x39, 0x65, 0x82, 0x37, 0xA0, 0xDC, 0x2E, 0x5F };
        //readonly byte[] cp1_keyseed = { 0x1F, 0x3D, 0x92, 0x3C };

        public KeyGenerator(ILog input_log = null)
        {
            _tKeyPair = new byte[0];
            _tPublicKey = new byte[0];
            _tPrivateKey = new byte[0];
            _skT1 = new byte[0];
            _shrKey1 = new byte[0];
            _nfw1 = new byte[0];
            _nddpm1 = new byte[0];
            _assd = new byte[0];

            local_log = input_log;
        }
        private bool GenerateRandomECCKeyPair()
        {
            try
            {
                using (ECDiffieHellman ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
                {
                    // store keypair in X.509 format
                    _tKeyPair = ecdh.ExportSubjectPublicKeyInfo();

                    // Extract ECParameters
                    ECParameters parameters = ecdh.ExportParameters(true);

                    if (parameters.Q.Y != null && parameters.Q.X != null && parameters.D != null)
                    {
                        // Export PublicKey in combining X and Y coordinates
                        _tPublicKey = new byte[parameters.Q.X.Length + parameters.Q.Y.Length];
                        Buffer.BlockCopy(parameters.Q.X, 0, _tPublicKey, 0, parameters.Q.X.Length);
                        Buffer.BlockCopy(parameters.Q.Y, 0, _tPublicKey, parameters.Q.X.Length, parameters.Q.Y.Length);

                        // Export PrivatecKey in combining X and Y coordinates
                        _tPrivateKey = new byte[parameters.D.Length];
                        Buffer.BlockCopy(parameters.D, 0, _tPrivateKey, 0, parameters.D.Length);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateRandomECCKeyPair error : {ex.Message}");
                local_log?.Error($"GenerateRandomECCKeyPair error : {ex.Message}");
            }
            return false;
        }


        /// <summary>
        /// 傳入 private key , public key.x, public key.y.傳回 ECC相乘後的(sharedSecret, agreedSecret)
        /// </summary>
        /// <param name="scalar">private key</param>
        /// <param name="pointX">public key.x</param>
        /// <param name="pointY">public key.y</param>
        /// <returns>(sharedSecret, agreedSecret)</returns>
        private (byte[], byte[]) MultiplyEccPoint(byte[] scalar, byte[] pointX, byte[] pointY)
        {
            ECCurve curve = ECCurve.NamedCurves.nistP256;

            // Import the scalar (private key)
            var privateKeyParams = new ECParameters
            {
                Curve = curve,
                D = scalar,
            };

            // Import the point (public key)
            var publicPointParams = new ECParameters
            {
                Curve = curve,
                Q = new ECPoint
                {
                    X = pointX,
                    Y = pointY
                }
            };

            // Create the local ECDH instance
            using (ECDiffieHellman ecdh = ECDiffieHellman.Create(privateKeyParams))
            //using (var ecdh = new ECDiffieHellmanCng())
            using (ECDiffieHellman peerEcdh = ECDiffieHellman.Create(publicPointParams))
            //using (var peerEcdh = new ECDiffieHellmanCng())
            {
                ecdh.ImportParameters(privateKeyParams);
                peerEcdh.ImportParameters(publicPointParams);

                // Derive the shared point (Q = d * P)
                byte[] sharedSecret = ecdh.DeriveKeyMaterial(peerEcdh.PublicKey); ;
                byte[] agreedSecret = ecdh.DeriveRawSecretAgreement(peerEcdh.PublicKey); ;

                return (sharedSecret, agreedSecret);
            }
        }
        private byte[] ProcessECDHKeyExchange(byte[] publicKeyData, byte[] privateKeyData)
        {
            byte[] sharedSecret;
            byte[] agreedSecret;
            byte[] agreeKeyPair;

            try
            {
                var parameters = new ECParameters
                {
                    Curve = ECCurve.NamedCurves.nistP256,
                    //D = privateKeyData,
                    Q = new ECPoint
                    {
                        X = publicKeyData.Skip(1).Take(32).ToArray(),
                        Y = publicKeyData.Skip(33).Take(32).ToArray()
                    }
                };

                using (ECDiffieHellman ecdh = ECDiffieHellman.Create(parameters))
                {
                    var peerParams = new ECParameters
                    {
                        Curve = ECCurve.NamedCurves.nistP256,
                        Q = new ECPoint
                        {
                            X = publicKeyData.Skip(1).Take(32).ToArray(),
                            Y = publicKeyData.Skip(33).Take(32).ToArray()
                        }
                    };
                    using (ECDiffieHellman peerECDH = ECDiffieHellman.Create(peerParams))
                    {
                        (sharedSecret, agreedSecret) = MultiplyEccPoint(privateKeyData, publicKeyData.Skip(1).Take(32).ToArray(), publicKeyData.Skip(33).Take(32).ToArray());
                        //sharedSecret = ecdh.DeriveKeyMaterial(peerECDH.PublicKey);
                        //agreedSecret = ecdh.DeriveRawSecretAgreement(peerECDH.PublicKey);
                        Console.WriteLine($"Derived sharedSecret: {BitConverter.ToString(sharedSecret)}");
                        Console.WriteLine($"Derived agreedSecret: {BitConverter.ToString(agreedSecret)}");
                        //agreedSecret = agreedSecret.Reverse().ToArray();

                        //Console.WriteLine($"Derived agreedSecret Reverse: {BitConverter.ToString(agreedSecret)}");
                        local_log?.Info("Derived sharedSecret and agreedSecret");
                    }
                }

                _skT1 = agreedSecret;
                Console.WriteLine($"Length: {_skT1.Length}, _skT1: {BitConverter.ToString(_skT1)}");
                local_log?.Info($"Info of _skT1: {_skT1.Length}");

                parameters.Q = new ECPoint { X = null, Y = null };
                parameters.D = agreedSecret;
                using (ECDiffieHellman agreeECDH = ECDiffieHellman.Create(parameters))
                {
                    ECParameters ecparameters = agreeECDH.ExportParameters(true);

                    // gneerate _skT1
                    _skT1 = new byte[ecparameters.Q.X.Length + ecparameters.Q.Y.Length];
                    Buffer.BlockCopy(ecparameters.Q.X, 0, _skT1, 0, ecparameters.Q.X.Length);
                    Buffer.BlockCopy(ecparameters.Q.Y, 0, _skT1, ecparameters.Q.X.Length, ecparameters.Q.Y.Length);
                    Console.WriteLine($"Length: {_skT1.Length}, _skT1.x: {BitConverter.ToString(_skT1.Skip(0).Take(32).ToArray())}");
                    Console.WriteLine($"Length: {_skT1.Length}, _skT1.y: {BitConverter.ToString(_skT1.Skip(32).Take(32).ToArray())}");
                    
                    // generate _shrKey1
                    byte[] _shrKey1data = new byte[_skT1.Length + SettingsAccess.cp1.Length];
                    Array.Copy(_skT1, 0, _shrKey1data, 0, _skT1.Length / 2);
                    Array.Copy(SettingsAccess.cp1, 0, _shrKey1data, _skT1.Length / 2, SettingsAccess.cp1.Length);
                    Array.Copy(_skT1, _skT1.Length / 2, _shrKey1data, _shrKey1data.Length - (_skT1.Length / 2), _skT1.Length / 2);
                    
                    Console.WriteLine($"Length: {_shrKey1data.Length}, _shrKey1data: {BitConverter.ToString(_shrKey1data)}");
                    local_log?.Info($"Info of _shrKey1data: {_shrKey1data.Length}");

                    _shrKey1 = HmacHash256(_skT1.Skip(0).Take(32).ToArray(), _shrKey1data.Skip(32).Take(_shrKey1data.Length - 32).ToArray());
                    Console.WriteLine($"Length: {_shrKey1.Length}, _shrKey1: {BitConverter.ToString(_shrKey1)}");
                    local_log?.Info($"Info of _shrKeyT1: {_shrKey1.Length}");

                    // generate ccmSeed
                    Console.WriteLine($"Length: {(publicKeyData.Length)}, publicKeyData: {BitConverter.ToString(publicKeyData)}");
                    byte[] ccmSeed = HmacHash256(_skT1.Skip(32).Take(32).ToArray(), publicKeyData);
                    Console.WriteLine($"Length: {ccmSeed.Length}, ccmSeed: {BitConverter.ToString(ccmSeed)}");
                    local_log?.Info($"Info of ccmSeed: {ccmSeed.Length}");

                    // generate _nfw1,_ndpm1,_assd
                    _nfw1 = new byte[12];
                    Array.Copy(ccmSeed, 0, _nfw1, 0, _nfw1.Length);
                    _nddpm1 = new byte[12];
                    Array.Copy(ccmSeed, 12, _nddpm1, 0, _nddpm1.Length);
                    _assd = new byte[8];
                    Array.Copy(ccmSeed, 24, _assd, 0, _assd.Length);
                    Console.WriteLine($"Length: {_nfw1.Length}, _nfw1: {BitConverter.ToString(_nfw1)}");
                    Console.WriteLine($"Length: {_nddpm1.Length}, _ndpm1: {BitConverter.ToString(_nddpm1)}");
                    Console.WriteLine($"Length: {_assd.Length}, _assd: {BitConverter.ToString(_assd)}");
                    local_log?.Info($"Info of _nfw1: {_nfw1.Length}, _nddpm1: {_nddpm1.Length}, _assd:{_assd.Length}");
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Cryptographic Error: {ex.Message}");
                local_log?.Error($"Cryptographic Error: {ex.Message}");
                throw;
            }
            //Console.WriteLine($"Derived Secret: {BitConverter.ToString(agreedSecret)}");
            return agreedSecret;
        }

        private byte[] EncKeySeed(byte[] comKey, byte[] inData, uint idx)
        {
            byte[] retVal = inData.ToArray(); // Clone input data
            byte[] kCom = comKey.Take(16).ToArray();
            byte[] KSD, stateM;
            (KSD, stateM) = calculateKSD(kCom, idx);
            //Console.WriteLine($"KSD: {BitConverter.ToString(KSD)} , stateM: {BitConverter.ToString(stateM)}");

            bxor(retVal, KSD, Math.Min(KSD.Length, inData.Length));

            return retVal;
        }

        private byte[] GenerateTKDeviceIDPair2(byte[] comKey, byte[] keySeed, uint idx, out byte[] deviceID)
        {
            deviceID = new byte[16];
            byte[] kCom = comKey.Take(16).ToArray();
            byte[] kCom2 = comKey.Skip(16).Take(16).ToArray();
            byte[] OOBKEY = new byte[16];
            byte[] KSD, stateM;
            (KSD, stateM) = calculateKSD(kCom, idx);
            Console.WriteLine($"Length: {KSD.Length}, KSD: {BitConverter.ToString(KSD)}");
            Console.WriteLine($"Length: {stateM.Length}, stateM: {BitConverter.ToString(stateM)}");            

            //bxor(keySeed, KSD, Math.Min(KSD.Length, keySeed.Length));
            Console.WriteLine($"keySeed: {BitConverter.ToString(keySeed)}");
            local_log?.Info($"Info of KSD: {KSD.Length}, stateM: {stateM.Length}, keySeed: {keySeed.Length}");

            // ----- HASHLOOP -----
            // Combine KeySeed and StateM
            byte[] opData;
            using (SHA256 sha256 = SHA256.Create())
            {
                opData = sha256.ComputeHash(keySeed.Concat(stateM).ToArray());
            }

            byte[] OP = HmacHash256(kCom2, opData);
            byte[] OPL = OP.Take(16).ToArray();
            byte[] OPH = OP.Skip(16).Take(16).ToArray();
            Console.WriteLine($"Length: {OP.Length}, OP: {BitConverter.ToString(OP)}");
            Console.WriteLine($"Length: {OPL.Length}, OPL: {BitConverter.ToString(OPL)}");
            Console.WriteLine($"Length: {OPH.Length}, OPH: {BitConverter.ToString(OPH)}");
            local_log?.Info($"Info of OP: {OP.Length}, OPL: {OPL.Length}, OPH: {OPH.Length}");

            byte[] a1 = EncryptAES(stateM.Skip(16).Take(16).ToArray(), OPL);
            byte[] a2 = EncryptAES(a1, OPH);
            Console.WriteLine($"Length: {a1.Length}, a1: {BitConverter.ToString(a1)}");
            Console.WriteLine($"Length: {a2.Length}, a2: {BitConverter.ToString(a2)}");
            local_log?.Info($"Info of a1: {a1.Length}, a2: {a2.Length}");

            uint opB = BitConverter.ToUInt32(OP.Take(4).Reverse().ToArray(), 0);
            uint N0 = 31 + opB % 19;
            uint TkipN = (uint)(N0 - (OP[21] % 11));
            uint DevID_N = (uint)(N0 - (OP[22] % 3));
            Console.WriteLine($"opB: {opB}");
            Console.WriteLine($"N0: {N0}");
            Console.WriteLine($"TkipN: {TkipN}");
            Console.WriteLine($"DevID_N: {DevID_N}");
            local_log?.Info($"Info of opB: {opB}, N0: {N0}, TkipN: {TkipN}, DevID_N: {DevID_N}");

            if (TkipN == N0) TkipN -= 11;
            if (DevID_N == N0) DevID_N -= 3;
            if (DevID_N == TkipN) TkipN -= 1;
            //Console.WriteLine($"DEBUG: Hash Loop Count: {N0}, TkipN: {TkipN}, DevID_N: {DevID_N}");

            uint HashCnt = N0 / 5;
            uint i = 1;
            bool DellOOBDevice = false;
            bool TKIPReady = false;

            Console.WriteLine($"HashCnt: {HashCnt}");

            while (i < N0 + 1)
            {
                if (i % HashCnt == 0)
                {
                    byte[] OPi = SHA256.Create().ComputeHash(
                        a1.Concat(SHA256.Create().ComputeHash(OP.Concat(a2).ToArray())).ToArray()
                    );
                    bxor(OP, OPi, Math.Min(OP.Length, OPi.Length));

                    OPL = OP.Take(16).ToArray();
                    OPH = OP.Skip(16).Take(16).ToArray();
                }

                byte[] x1 = a1.ToArray();
                bxor(x1, OPL, Math.Min(x1.Length, OPL.Length));
                a1 = EncryptAES(a2, x1);

                byte[] x2 = a2.ToArray();
                bxor(x2, OPH, Math.Min(x2.Length, OPH.Length));
                a2 = EncryptAES(a1, x2);

                if (i == TkipN && !TKIPReady)
                {
                    OOBKEY = SHA256.Create().ComputeHash(
                        OP.Concat(a1)
                        .Concat(BitConverter.GetBytes(TkipN).Take(1).Reverse())
                        .ToArray()).Take(16).ToArray();
                    TKIPReady = true;
                }

                if (i == DevID_N && !DellOOBDevice)
                {
                    deviceID = SHA256.Create().ComputeHash(
                        OP.Concat(a2)
                        .Concat(BitConverter.GetBytes(DevID_N).Take(1).Reverse())
                        .ToArray()).Take(16).ToArray();
                    DellOOBDevice = true;
                }

                if (TKIPReady && DellOOBDevice)
                    break;

                i++;
            }

            return OOBKEY;
        }

        // ----- utility function -----
        private (byte[] KSD, byte[] stateM) calculateKSD(byte[] kcom, uint idx)
        {
            // Step 1: Prepare Data for Hash
            byte[] cp1B = SettingsAccess.cp1_keyseed.ToArray();// cp1_keyseed.ToArray(); // Ensure Big-Endian
            byte[] idxBytes = BitConverter.GetBytes(idx).ToArray();    // Ensure Big-Endian

            byte[] bstr = kcom
                .Concat(cp1B)
                .Concat(idxBytes)
                .ToArray();

            Console.WriteLine($"DEBUG: Length: {bstr.Length}, bstr: {BitConverter.ToString(bstr)}");
            local_log?.Info($"Info of bstr: {bstr.Length}");

            // Step 2: SHA256 Hash
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(bstr);
            }

            // Step 3: HMAC-SHA256
            byte[] stateM;
            using (HMACSHA256 hmac = new HMACSHA256(kcom))
            {
                stateM = hmac.ComputeHash(hash);
            }

            //Console.WriteLine($"DEBUG: Length: {stateM.Length}, stateM: {BitConverter.ToString(stateM)}");

            // Step 4: AES Encryption
            byte[] es = EncryptAES(
                stateM.Skip(16).Take(16).ToArray(), // AES Key
                stateM.Take(16).ToArray()          // AES Data
            );

            //Console.WriteLine($"DEBUG: Length: {es.Length}, ES: {BitConverter.ToString(es)}");

            return (es, stateM);
        }

        private void bxor(byte[] a, byte[] b, int length)
        {
            for (int i = 0; i < length; i++)
            {
                a[i] ^= b[i];
            }
        }

        private byte[] EncryptAES(byte[] key, byte[] data)
        {
            byte[] retVal = new byte[16];

            // AES CBC mode
            //using (Aes aes = Aes.Create())
            //{
            //    aes.Key = key;
            //    aes.IV = iv;
            //    aes.Mode = CipherMode.CBC; // Assuming CBC as default mode
            //    aes.Padding = PaddingMode.PKCS7;

            //    using (ICryptoTransform encryptor = aes.CreateEncryptor())
            //    {
            //        byte[] plaintext = new byte[16]; // Empty plaintext (adjust as per requirements)
            //        return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
            //    }
            //}

            //AES ECB mode
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
                    Array.Copy(encryptedData, retVal, Math.Min(retVal.Length, encryptedData.Length));

                    return retVal;
                }
            }
        }

        private byte[] HmacHash256(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        private (byte[] ciphertext, byte[] tag) AES256CCMEncrypt(byte[] key, byte[] plaintext, byte[] nonce, byte[] associatedData)
        {
            using (var aesCcm = new AesCcm(key))
            {
                byte[] ciphertext = new byte[plaintext.Length];
                byte[] tag = new byte[4]; // Authentication tag (16 bytes for AES-CCM)

                aesCcm.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
                return (ciphertext, tag);
            }
        }

        private byte[]? AES256CCMDecrypt(byte[] key, byte[] ciphertext, byte[] nonce, byte[] associatedData, byte[] tag)
        {
            try
            {
                using (var aesCcm = new AesCcm(key))
                {
                    byte[] decryptedBytes = new byte[ciphertext.Length];

                    aesCcm.Decrypt(nonce, ciphertext, tag, decryptedBytes, associatedData);
                    return decryptedBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AES256CCMDecrypt error : {ex.Message}");
                local_log?.Error($"AES256CCMDecrypt error : {ex.Message}");
            }
            return null;
        }
        public string ProcessX0State(string tPubKeyDev1_str)
        {
            Console.WriteLine($"ProcessX0State start");
            Console.WriteLine($"Lenght : {tPubKeyDev1_str.Length} tPubKeyDev1_str: {tPubKeyDev1_str}");
            byte[] tPubKeyDev1 = Convert.FromHexString(tPubKeyDev1_str);
            Console.WriteLine($"Lenght : {tPubKeyDev1.Length} tPubKeyDev1: {BitConverter.ToString(tPubKeyDev1)}");
            local_log?.Info($"Info of tPubKeyDev1: {tPubKeyDev1.Length}");

            GenerateRandomECCKeyPair();
            if (_tKeyPair != null && _tPublicKey != null && _tPrivateKey != null)
            {
                byte[] tKeyPairPC1 = _tPublicKey;
                Console.WriteLine($"Lenght : {tKeyPairPC1.Length} tKeyPairPC1: {BitConverter.ToString(tKeyPairPC1)}");
                // Extract ECC public/priavate key
                byte[] tPubKeyPC1 = new byte[65];
                tPubKeyPC1[0] = 0x04;
                Array.Copy(tKeyPairPC1, 0, tPubKeyPC1, 1, tKeyPairPC1.Length);
                byte[] tPriKeyPC1 = _tPrivateKey;
                // process ProcessECDHKeyExchange
                byte[] tderivedPublicKey = ProcessECDHKeyExchange(tPubKeyDev1, tPriKeyPC1);
                Console.WriteLine($"Lenght : {tPubKeyPC1.Length} tPubKeyPC1: {BitConverter.ToString(tPubKeyPC1)}");
                Console.WriteLine($"Lenght : {tPriKeyPC1.Length} tPriKeyPC1: {BitConverter.ToString(tPriKeyPC1)}");
                Console.WriteLine($"ProcessX0State done");
                local_log?.Info($"Info of tPubKeyPC1: {tPubKeyPC1.Length}, tPriKeyPC1: {tPriKeyPC1.Length}");
                return BitConverter.ToString(tPubKeyPC1);
            }
            return string.Empty;
        }
        public string ProcessX2State(string fwEncBlock_str)
        {
            Console.WriteLine($"ProcessX2State start");
            byte[] fwEncBlock = Convert.FromHexString(fwEncBlock_str);
            byte[] fwEncBlock_WithOutTag = new byte[fwEncBlock.Length - 4];
            byte[] fwEncBlock_Tag = new byte[4];
            Array.Copy(fwEncBlock, 0, fwEncBlock_WithOutTag, 0, fwEncBlock_WithOutTag.Length);
            Array.Copy(fwEncBlock, fwEncBlock_WithOutTag.Length, fwEncBlock_Tag, 0, fwEncBlock_Tag.Length);
            Console.WriteLine($"Length: {fwEncBlock_WithOutTag.Length}, fwEncBlock_WithOutTag(): {BitConverter.ToString(fwEncBlock_WithOutTag)}");
            Console.WriteLine($"Length: {fwEncBlock_Tag.Length}, fwEncBlock_Tag(): {BitConverter.ToString(fwEncBlock_Tag)}");
            local_log?.Info($"Info of fwEncBlock_WithOutTag: {fwEncBlock_WithOutTag.Length}, fwEncBlock_Tag: {fwEncBlock_Tag.Length}");

            byte[]? fwEncBlockData = AES256CCMDecrypt(_shrKey1, fwEncBlock_WithOutTag, _nfw1, _assd, fwEncBlock_Tag);
            if (fwEncBlockData == null)
            {
                local_log?.Error("null fwEncBlockData");
                return string.Empty;
            }
            Console.WriteLine($"Length: {fwEncBlockData.Length}, fwEncBlockData: {BitConverter.ToString(fwEncBlockData)}");
            byte[] index = new byte[4];
            Array.Copy(fwEncBlockData, 0, index, 0, 4);
            Console.WriteLine($"Length: {index.Length}, index: {BitConverter.ToString(index)}");
            uint idxNumber = BitConverter.ToUInt32(index, 0);
            Console.WriteLine($"idxNumber: {idxNumber}");
            byte[] tkeyseed = new byte[8];
            Array.Copy(fwEncBlockData, 4, tkeyseed, 0, 8);
            Console.WriteLine($"Length: {tkeyseed.Length}, tkeyseed: {BitConverter.ToString(tkeyseed)}");
            byte[] authTag = new byte[16];
            Array.Copy(fwEncBlockData, 12, authTag, 0, 16);
            Console.WriteLine($"Length: {authTag.Length}, authTag: {BitConverter.ToString(authTag)}");
            local_log?.Info($"Info of index: {index.Length}, tkeyseed: {tkeyseed.Length}, authTag: {authTag.Length}");

            byte[] comkey = Convert.FromHexString(SettingsAccess.comKey);// "51f371b0181d7a9a7457e48ef639396d8cac1445a762cd012de42ab2a70aa9ab");
            Console.WriteLine($"Length: {comkey.Length}, comkey: {BitConverter.ToString(comkey)}");
            byte[] deviceID = new byte[16];
            byte[] OOBKEY = GenerateTKDeviceIDPair2(comkey, tkeyseed, idxNumber, out deviceID);
            Console.WriteLine($"deviceID: {BitConverter.ToString(deviceID)}");
            Console.WriteLine($"OOBKEY: {BitConverter.ToString(OOBKEY)}");
            local_log?.Info($"Info of deviceID: {deviceID.Length}, OOBKEY: {OOBKEY.Length}");

            if (deviceID.SequenceEqual(authTag))
            {
                Console.WriteLine($"ProcessX2State pass");
                byte[] ret = new byte[32];
                Array.Copy(OOBKEY, 0, ret, 0, OOBKEY.Length);
                (byte[] ciphertext, byte[] tag) = AES256CCMEncrypt(_shrKey1, ret, _nddpm1, _assd);
                Console.WriteLine($"Length: {ciphertext.Length}, ciphertext: {BitConverter.ToString(ciphertext)}");
                Console.WriteLine($"Length: {tag.Length}, tag: {BitConverter.ToString(tag)}");
                local_log?.Info($"Info of ciphertext: {ciphertext.Length}, tag: {tag.Length}, deviceID equal to authTag");

                return BitConverter.ToString(ciphertext) + BitConverter.ToString(tag);
            }
            else
            {
                Console.WriteLine($"ProcessX2State fail");
                local_log?.Error("deviceID not equal to authTag");
                return string.Empty;
            }
        }
    }
}
