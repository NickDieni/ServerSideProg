using System.Security.Cryptography;
using System.Text;

namespace ServerSideProg.Codes
{
    public class AsymmetriskEncryption
    {
        private string _privateKey;
        private string _publicKey;

        public AsymmetriskEncryption()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
                _privateKey = "-----BEGIN PRIVATE KEY-----\n" + 
                    Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks) + 
                    "\n-----END PRIVATE KEY-----";

                byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
                _publicKey = "-----BEGIN PUBLIC KEY-----\n" + 
                    Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks) + 
                    "\n-----END PUBLIC KEY-----";

            }
        }

        public string EncryptAsymetrisk(string valueToEncrypt)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                string publicKey = _publicKey
                    .Replace("-----BEGIN PUBLIC KEY-----", "")
                    .Replace("-----END PUBLIC KEY-----", "")
                    .Replace("\n", "").Replace("\r", "").Trim();

                byte[] publicKeyBytes = Convert.FromBase64String(publicKey);

                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                byte[] valueToEncryptAsBytes = Encoding.UTF8.GetBytes(valueToEncrypt);
                byte[] encryptValueBytes = rsa.Encrypt(valueToEncryptAsBytes, true);
                return Convert.ToBase64String(encryptValueBytes);
            }

            //string result = call web api(publickey, valueToEncrypt)
        }

        public string DecryptAsymetrisk(string valueToDecrypt)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                string privateKey = _privateKey
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Replace("\n", "").Replace("\r", "").Trim();

                byte[] privateKeyBytes = Convert.FromBase64String(privateKey);

                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

                byte[] valueToDecryptAsBytes = Convert.FromBase64String(valueToDecrypt);
                byte[] decryptValueBytes = rsa.Decrypt(valueToDecryptAsBytes, true);
                string decryptedDataAsString = System.Text.Encoding.UTF8.GetString(decryptValueBytes);

                return decryptedDataAsString;
            }
        }
    }
}
