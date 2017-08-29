using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

/// <summary>
/// Summary description for saml
/// </summary>

namespace OneLogin
{
    public class AppSettings
    {
        public string idp_sso_url = "https://login.rmuti.ac.th/idp/saml2/SSOService.php";
        public string singleLogoutService = "https://login.rmuti.ac.th/idp/saml2/SingleLogoutService.php";
        public string certificate = "-----BEGIN CERTIFICATE-----\nMIIFvTCCBKWgAwIBAgIJAJVWKDnYZN16MA0GCSqGSIb3DQEBBQUAMIHuMQswCQYD\nVQQGEwJUSDEaMBgGA1UECBMRTmFraG9uIFJhdGNoYXNpbWExDjAMBgNVBAcTBU11\nYW5nMTIwMAYDVQQKEylSYWphbWFuZ2FsYSBVbml2ZXJzaXR5IG9mIFRlY2hub2xv\nZ3kgSXNhbjFAMD4GA1UECxM3T2ZmaWNlIG9mIEFjYWRlbWljIFJlc291cmNlcyBh\nbmQgSW5mb3JtYXRpb24gVGVjaG5vbG9neTEdMBsGA1UEAxMUUk1VVGkgU2luZ2xl\nIHNpZ24tb24xHjAcBgkqhkiG9w0BCQEWD25vY0BybXV0aS5hYy50aDAeFw0xNTA5\nMjIxODE2NTFaFw0yNTA5MjExODE2NTFaMIHuMQswCQYDVQQGEwJUSDEaMBgGA1UE\nCBMRTmFraG9uIFJhdGNoYXNpbWExDjAMBgNVBAcTBU11YW5nMTIwMAYDVQQKEylS\nYWphbWFuZ2FsYSBVbml2ZXJzaXR5IG9mIFRlY2hub2xvZ3kgSXNhbjFAMD4GA1UE\nCxM3T2ZmaWNlIG9mIEFjYWRlbWljIFJlc291cmNlcyBhbmQgSW5mb3JtYXRpb24g\nVGVjaG5vbG9neTEdMBsGA1UEAxMUUk1VVGkgU2luZ2xlIHNpZ24tb24xHjAcBgkq\nhkiG9w0BCQEWD25vY0BybXV0aS5hYy50aDCCASIwDQYJKoZIhvcNAQEBBQADggEP\nADCCAQoCggEBAJilVrEQJ2xdAfrsdHyevNThzAjbXpiX4KvetI6TYyq/9jfSCYqm\nJqTHD9yRvtzEUUOySR7KTi9K2ATUD1R9w8DLaDHjEwJWpfwFGoidNjZfhm7IJtHF\nh7G2Z4Y7QpkBqhqTk8WKDwygGooQB7yDPMCUw+Ld/Z0wxFgGmrb6IHJyDQAplNpz\n8jND65mq3Sir/dcVBuiXF9Uj75a1uvyECPWGJpyVwSlyZKlZuGJP//ZrHBLvWvT2\no3bMTtaelKXNPjYHV2kuQmvi3QlOcSd3c0GhnwNmc6n9ady0Vskt8SIMORqFM2eZ\n4Jzb1JXobRNYuf7R8WWew8gQ1kbU/jM6yZECAwEAAaOCAVowggFWMB0GA1UdDgQW\nBBTmfvFkLPIo0dUjZbRMrPylrmkTrDCCASUGA1UdIwSCARwwggEYgBTmfvFkLPIo\n0dUjZbRMrPylrmkTrKGB9KSB8TCB7jELMAkGA1UEBhMCVEgxGjAYBgNVBAgTEU5h\na2hvbiBSYXRjaGFzaW1hMQ4wDAYDVQQHEwVNdWFuZzEyMDAGA1UEChMpUmFqYW1h\nbmdhbGEgVW5pdmVyc2l0eSBvZiBUZWNobm9sb2d5IElzYW4xQDA+BgNVBAsTN09m\nZmljZSBvZiBBY2FkZW1pYyBSZXNvdXJjZXMgYW5kIEluZm9ybWF0aW9uIFRlY2hu\nb2xvZ3kxHTAbBgNVBAMTFFJNVVRpIFNpbmdsZSBzaWduLW9uMR4wHAYJKoZIhvcN\nAQkBFg9ub2NAcm11dGkuYWMudGiCCQCVVig52GTdejAMBgNVHRMEBTADAQH/MA0G\nCSqGSIb3DQEBBQUAA4IBAQAZjxqYuPyRaUKO728ZEHpRfW2nEX5jvAXV97cST7Bw\nikJhmDtp5bVy4O7KaxiyRn0HMbFWjI/TKa2aWq5c1eL0HEgmgtPOu4GOMgQ7vZ08\nLDv3vTxmiNa5J719r3IjFrYyxlV2JONqFvLM2eRIbAiezaS6Ssqn5RRQRCqpN9yU\nOUOlasihH1PApBoTEWW2/JoDFtpDDhsMB61k78l5EDyMZglueWPfW4ZLbFp24Xv7\nkNwg823o0gTRyP7qvY/3nhneRPQ9Og7UKgY/IlU6NlK5h/9aIZvlPcK6AJ9/RMtQ\nNsgT9WzXnYzqyx0/2BhTcWnkFVuzatN4bowMTWhO7ID6\n-----END CERTIFICATE-----";

        public string assertionConsumerServiceUrl = "http://cpe.rmuti.ac.th/project/StudentAttendance/Login/acs.cshtml"; // แก้ตรงนี้ url หน้าแรกที่ลอกอินได้
        public string identityId = "http://cpe.rmuti.ac.th/project/StudentAttendance"; //ตรงนี้url project ตรงนี้มันถูกใช่ไหม
    }

    public enum RequestFormat
    {
        Base64 = 1
    }

    /*
        Compression: Compress and Decompress stream
    */
    public class Compression
    {
        public static byte[] Compress(string str)
        {
            return Compress(System.Text.Encoding.UTF8.GetBytes(str));
        }
        public static byte[] Compress(byte[] data)
        {
            byte[] compressArray = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                    {
                        deflateStream.Write(data, 0, data.Length);
                    }
                    compressArray = memoryStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }
            return compressArray;
        }

        public static byte[] Decompress(string str)
        {
            return Decompress(System.Text.Encoding.UTF8.GetBytes(str));
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            try
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    using (MemoryStream compressStream = new MemoryStream(data))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(decompressedStream);
                        }
                    }
                    decompressedArray = decompressedStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }

            return decompressedArray;
        }
    }

    public class Certificate
    {
        public X509Certificate2 cert;

        public void LoadCertificate(string certificate)
        {
            cert = new X509Certificate2();
            cert.Import(StringToByteArray(certificate));
        }

        public void LoadCertificate(byte[] certificate)
        {
            cert = new X509Certificate2();
            cert.Import(certificate);
        }

        private byte[] StringToByteArray(string st)
        {
            byte[] bytes = new byte[st.Length];
            for (int i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }
            return bytes;
        }
    }

    /*
        SAML Response: Process SAML response token from IdP
    */
    public class SamlResponse
    {
        private XmlDocument responseXmlDoc;
        private AppSettings appSettings;
        private Certificate certificate;
        private string responseText;

        public SamlResponse(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            certificate = new Certificate();
            certificate.LoadCertificate(appSettings.certificate);
        }

        public void LoadXml(string xml)
        {
            responseXmlDoc = new XmlDocument();
            responseXmlDoc.PreserveWhitespace = true;
            responseXmlDoc.XmlResolver = null;
            responseXmlDoc.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            var enc = Convert.FromBase64String(response);
            try
            {
                var dec = Compression.Decompress(enc);
                responseText = System.Text.Encoding.UTF8.GetString(dec);
            }
            catch (ArgumentNullException e)
            {
                responseText = System.Text.Encoding.UTF8.GetString(enc);
            }
            LoadXml(responseText);
        }

        public bool IsValid()
        {
            if (responseXmlDoc == null)
            {
                return false;
            }

            if (responseXmlDoc.FirstChild.Name == "samlp:Response")
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                XmlNodeList nodeList = responseXmlDoc.SelectNodes("//ds:Signature", manager);

                SignedXml signedXml = new SignedXml(responseXmlDoc);
                signedXml.LoadXml((XmlElement)nodeList[0]);
                return signedXml.CheckSignature(certificate.cert, true);
            }
            else
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

                XmlNode node = responseXmlDoc.SelectSingleNode("/samlp:LogoutResponse/samlp:Status/samlp:StatusCode", manager);
                return (node.Attributes["Value"].Value == "urn:oasis:names:tc:SAML:2.0:status:Success");
            }
        }

        public bool IsAuthenticated()
        {
            if (responseXmlDoc == null)
            {
                return false;
            }

            if (responseXmlDoc.FirstChild.Name == "samlp:Response")
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

                XmlNode node = responseXmlDoc.SelectSingleNode("/samlp:Response/samlp:Status/samlp:StatusCode", manager);
                return (node.Attributes["Value"].Value == "urn:oasis:names:tc:SAML:2.0:status:Success");
            }
            return false;
        }

        public string GetResponseText()
        {
            return responseText;
        }

        public XmlDocument GetResponseXml()
        {
            return responseXmlDoc;
        }

        public string GetNameID()
        {
            if (responseXmlDoc == null)
            {
                return null;
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node = responseXmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", manager);
            return node.InnerText;
        }

        public string GetSessionIndex()
        {
            if (responseXmlDoc == null)
            {
                return null;
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node = responseXmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AuthnStatement", manager);
            return node.Attributes["SessionIndex"].Value;
        }

        public string GetAttributes()
        {
            if (responseXmlDoc == null)
            {
                return null;
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(responseXmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node = responseXmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement", manager);
            return "<saml:AttributeStatement xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\">" + node.InnerXml + "</saml:AttributeStatement>";
        }
    }

    /*
        SAML Request: Generate SAML request token for sending to IdP
    */
    public class SamlRequest
    {
        public string id;
        private string issue_instant;
        private AppSettings appSettings;

        public SamlRequest(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            id = "_" + System.Guid.NewGuid().ToString();
            issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetAuthRequest()
        {
            return GetAuthRequest(RequestFormat.Base64);
        }

        public string GetAuthRequest(RequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    xw.WriteAttributeString("AssertionConsumerServiceURL", appSettings.assertionConsumerServiceUrl);

                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(appSettings.identityId);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient");
                    xw.WriteAttributeString("AllowCreate", "true");
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Comparison", "exact");
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                if (format == RequestFormat.Base64)
                {
                    byte[] toEncodeAsBytes = Compression.Compress(sw.ToString());
                    return System.Convert.ToBase64String(toEncodeAsBytes);
                }

                return null;
            }
        }

        public string GetLogoutRequest()
        {
            return GetLogoutRequest("");
        }

        public string GetLogoutRequest(string nameId)
        {
            return GetLogoutRequest(nameId, "");
        }

        public string GetLogoutRequest(string nameId, string sessionIndex)
        {
            return GetLogoutRequest(nameId, sessionIndex, RequestFormat.Base64);
        }

        public string GetLogoutRequest(string nameId, string sessionIndex, RequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "LogoutRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("xmlns", "saml", null, "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("Destination", appSettings.singleLogoutService);

                    xw.WriteStartElement("saml", "Issuer", null);
                    xw.WriteString(appSettings.identityId);
                    xw.WriteEndElement();

                    if (nameId != "")
                    {
                        xw.WriteStartElement("saml", "NameID", null);
                        xw.WriteAttributeString("SPNameQualifier", appSettings.identityId);
                        xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient");
                        xw.WriteString(nameId);
                        xw.WriteEndElement();
                    }

                    if (sessionIndex != "")
                    {
                        xw.WriteStartElement("samlp", "SessionIndex", null);
                        xw.WriteString(sessionIndex);
                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                }

                if (format == RequestFormat.Base64)
                {
                    byte[] toEncodeAsBytes = Compression.Compress(sw.ToString());
                    return System.Convert.ToBase64String(toEncodeAsBytes);
                }

                return null;
            }
        }
    }

    /*
        Main Class to provide for access SAML Authentication
    */
    public class Auth
    {
        public AppSettings Settings;
        public SamlRequest Request;
        public SamlResponse Response;
        private System.Web.HttpContext Context = System.Web.HttpContext.Current;

        public Auth(AppSettings appSettings)
        {
            this.Settings = appSettings;
            this.Request = new SamlRequest(this.Settings);
            this.Response = new SamlResponse(this.Settings);
        }

        public void Login()
        {
            Login("");
        }

        public string Login(String returnTo)
        {
            String Url;
            if (returnTo == "")
            {
                Url = this.Settings.idp_sso_url + "?SAMLRequest=" + this.Context.Server.UrlEncode(this.Request.GetAuthRequest());
            }
            else
            {
                Url = this.Settings.idp_sso_url + "?SAMLRequest=" + this.Context.Server.UrlEncode(this.Request.GetAuthRequest()) + "&RelayState=" + returnTo;
            }
            //this.Context.Response.Redirect(Url);
            return Url;
        }

        public void ProcessResponse()
        {
            if (this.Context.Request.Form["SAMLResponse"] != null)
            {
                //System.Web.HttpContext.Current.Response.Write(this.Context.Request.Form["SAMLResponse"]);
                this.Response.LoadXmlFromBase64(this.Context.Request.Form["SAMLResponse"]);
            }
            else if (this.Context.Request.QueryString["SAMLResponse"] != null)
            {
                //System.Web.HttpContext.Current.Response.Write(this.Context.Request.QueryString["SAMLResponse"]);
                this.Response.LoadXmlFromBase64(this.Context.Request.QueryString["SAMLResponse"]);
            }
            else if (this.Context.Request.QueryString["SAMLRequest"] != null)
            {
                //System.Web.HttpContext.Current.Response.Write(this.Context.Request.QueryString["SAMLRequest"]);
                this.Response.LoadXmlFromBase64(this.Context.Request.QueryString["SAMLRequest"]);
            }
        }

        public bool IsAuthenticated()
        {
            return this.Response.IsAuthenticated();
        }

        public void Logout()
        {
            Logout("");
        }

        public void Logout(String returnTo)
        {
            Logout(returnTo, "");
        }

        public void Logout(String returnTo, string nameId)
        {
            Logout(returnTo, nameId, "");
        }

        public string Logout(String returnTo, string nameId, string sessionIndex)
        {
            String Url;
            if (returnTo == "")
            {
                Url = this.Settings.singleLogoutService + "?SAMLRequest=" + this.Context.Server.UrlEncode(this.Request.GetLogoutRequest(nameId, sessionIndex, RequestFormat.Base64));
            }
            else
            {
                Url = this.Settings.singleLogoutService + "?SAMLRequest=" + this.Context.Server.UrlEncode(this.Request.GetLogoutRequest(nameId, sessionIndex, RequestFormat.Base64)) + "&RelayState=" + returnTo;
            }
            //this.Context.Response.Redirect(Url);
            return Url;
        }
    }
}

