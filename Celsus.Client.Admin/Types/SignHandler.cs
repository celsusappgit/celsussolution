using Celsus.Types.NonDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Celsus.Client.Admin.Types
{
    public class SignHandler
    {
        public static string GenerateSignedSerial(LicenseData license, byte[] certPrivateKeyData)
        {
            SecureString certFilePwd = new SecureString();

            certFilePwd.AppendChar('O');
            certFilePwd.AppendChar('s');
            certFilePwd.AppendChar('m');
            certFilePwd.AppendChar('a');
            certFilePwd.AppendChar('n');
            certFilePwd.AppendChar('T');
            certFilePwd.AppendChar('a');
            certFilePwd.AppendChar('m');
            certFilePwd.AppendChar('b');
            certFilePwd.AppendChar('u');
            certFilePwd.AppendChar('r');
            certFilePwd.AppendChar('a');
            certFilePwd.AppendChar('c');
            certFilePwd.AppendChar('i');
            certFilePwd.AppendChar('1');
            certFilePwd.AppendChar('2');

            X509Certificate2 certificate = new X509Certificate2(certPrivateKeyData, certFilePwd);
            RSACryptoServiceProvider privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;

            XmlDocument licenseXml = new XmlDocument();
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LicenseData), new Type[] { license.GetType() });
                xmlSerializer.Serialize(stringWriter, license);
                licenseXml.LoadXml(stringWriter.ToString());
            }

            SignXML(licenseXml, privateKey);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(licenseXml.OuterXml));
        }

        //public static object VerifyLicense(byte[] certPubKeyData, string licenseKey)
        //{

        //    string licenseString;
        //    object licenseInformation = null;
        //    try
        //    {
        //        X509Certificate2 certificate = new X509Certificate2(certPubKeyData);
        //        RSACryptoServiceProvider rsaCryptoServiceProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
        //        XmlDocument xmlDocument = new XmlDocument
        //        {
        //            PreserveWhitespace = true
        //        };
        //        xmlDocument.LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(licenseKey)));
        //        if (VerifyXml(xmlDocument, rsaCryptoServiceProvider))
        //        {
        //            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
        //            xmlDocument.DocumentElement.RemoveChild(nodeList[0]);
        //            licenseString = xmlDocument.OuterXml;
        //            XmlSerializer xmlSerializer = new XmlSerializer(typeof(object), new Type[] { typeof(object) });
        //            using (StringReader stringReader = new StringReader(licenseString))
        //            {
        //                licenseInformation = (object)xmlSerializer.Deserialize(stringReader);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return licenseInformation;
        //}

        //private static Boolean VerifyXml(XmlDocument Doc, RSA Key)
        //{
        //    // Check arguments.
        //    if (Doc == null)
        //        throw new ArgumentException("Doc");
        //    if (Key == null)
        //        throw new ArgumentException("Key");

        //    // Create a new SignedXml object and pass it
        //    // the XML document class.
        //    SignedXml signedXml = new SignedXml(Doc);

        //    // Find the "Signature" node and create a new
        //    // XmlNodeList object.
        //    XmlNodeList nodeList = Doc.GetElementsByTagName("Signature");

        //    // Throw an exception if no signature was found.
        //    if (nodeList.Count <= 0)
        //    {
        //        throw new CryptographicException("Verification failed: No Signature was found in the document.");
        //    }

        //    // This example only supports one signature for
        //    // the entire XML document.  Throw an exception 
        //    // if more than one signature was found.
        //    if (nodeList.Count >= 2)
        //    {
        //        throw new CryptographicException("Verification failed: More that one signature was found for the document.");
        //    }

        //    // Load the first <signature> node.  
        //    signedXml.LoadXml((XmlElement)nodeList[0]);

        //    // Check the signature and return the result.
        //    return signedXml.CheckSignature(Key);
        //}

        private static void SignXML(XmlDocument xmlDoc, RSA Key)
        {
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");

            SignedXml signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = Key ?? throw new ArgumentException("Key")
            };

            Reference reference = new Reference
            {
                Uri = ""
            };

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            XmlElement xmlDigitalSignature = signedXml.GetXml();

            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

        }
    }
}
