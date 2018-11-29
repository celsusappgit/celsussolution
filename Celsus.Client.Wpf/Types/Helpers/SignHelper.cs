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

namespace Celsus.Client.Wpf.Types
{
    public class SignHelper
    {

        public static LicenseData VerifyLicense(byte[] certPubKeyData, string licenseKey)
        {

            string licenseString;
            LicenseData licenseInformation = null;
            try
            {
                X509Certificate2 certificate = new X509Certificate2(certPubKeyData);
                RSACryptoServiceProvider rsaCryptoServiceProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                XmlDocument xmlDocument = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                xmlDocument.LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(licenseKey)));
                if (VerifyXml(xmlDocument, rsaCryptoServiceProvider))
                {
                    XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
                    xmlDocument.DocumentElement.RemoveChild(nodeList[0]);
                    licenseString = xmlDocument.OuterXml;
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(LicenseData), new Type[] { typeof(LicenseData) });
                    using (StringReader stringReader = new StringReader(licenseString))
                    {
                        licenseInformation = (LicenseData)xmlSerializer.Deserialize(stringReader);
                    }
                }
            }
            catch (Exception)
            {
            }
            return licenseInformation;
        }

        private static Boolean VerifyXml(XmlDocument Doc, RSA Key)
        {
            // Check arguments.
            if (Doc == null)
                throw new ArgumentException("Doc");
            if (Key == null)
                throw new ArgumentException("Key");

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(Doc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = Doc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            }

            // This example only supports one signature for
            // the entire XML document.  Throw an exception 
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");
            }

            // Load the first <signature> node.  
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature(Key);
        }


    }
}
