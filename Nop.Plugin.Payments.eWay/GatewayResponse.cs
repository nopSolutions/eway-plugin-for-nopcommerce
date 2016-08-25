using System.IO;
using System.Xml;
using Nop.Core;

namespace Nop.Plugin.Payments.eWay
{
    /// <summary>
    /// Summary description for GatewayResponse.
    /// Copyright Web Active Corporation Pty Ltd  - All rights reserved. 1998-2004
    /// This code is for exclusive use with the eWAY payment gateway
    /// </summary>
    public class GatewayResponse
    {
        /// <summary>
        /// Creates a new instance of the GatewayResponse class from xml
        /// </summary>
        /// <param name="xml">Xml string</param>
        public GatewayResponse(string xml)
        {
            var _sr = new StringReader(xml);
            var _xtr = new XmlTextReader(_sr)
            {
                XmlResolver = null,
                WhitespaceHandling = WhitespaceHandling.None
            };

            // get the root node
            _xtr.Read();

            if ((_xtr.NodeType != XmlNodeType.Element) || (_xtr.Name != "ewayResponse")) return;

            while (_xtr.Read())
            {
                if ((_xtr.NodeType != XmlNodeType.Element) || _xtr.IsEmptyElement) continue;

                var _currentField = _xtr.Name;
                _xtr.Read();
                if (_xtr.NodeType != XmlNodeType.Text) continue;

                switch (_currentField)
                {
                    case "ewayTrxnError":
                        Error = _xtr.Value;
                        break;

                    case "ewayTrxnStatus":
                        if (_xtr.Value.ToLower().IndexOf("true") != -1)
                            Status = true;
                        break;

                    case "ewayTrxnNumber":
                        TransactionNumber = _xtr.Value;
                        break;

                    case "ewayTrxnOption1":
                        Option1 = _xtr.Value;
                        break;

                    case "ewayTrxnOption2":
                        Option2 = _xtr.Value;
                        break;

                    case "ewayTrxnOption3":
                        Option3 = _xtr.Value;
                        break;

                    case "ewayReturnAmount":
                        Amount = int.Parse(_xtr.Value);
                        break;

                    case "ewayAuthCode":
                        AuthorisationCode = _xtr.Value;
                        break;

                    case "ewayTrxnReference":
                        InvoiceReference = _xtr.Value;
                        break;

                    default:
                        // unknown field
                        throw new NopException("Unknown field in response.");
                }
            }
        }

        /// <summary>
        /// Gets a transaction number
        /// </summary>
        public string TransactionNumber { get; private set; }

        /// <summary>
        /// Gets an invoice reference
        /// </summary>
        public string InvoiceReference { get; private set; }

        /// <summary>
        /// Gets an option 1
        /// </summary>
        public string Option1 { get; private set; }

        /// <summary>
        /// Gets an option 2
        /// </summary>
        public string Option2 { get; private set; }

        /// <summary>
        /// Gets an option 3
        /// </summary>
        public string Option3 { get; private set; }

        /// <summary>
        /// Gets an authorisatio code
        /// </summary>
        public string AuthorisationCode { get; private set; }

        /// <summary>
        /// Gets an error 
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Gets an amount
        /// </summary>
        public int Amount { get; private set; }

        /// <summary>
        /// Gets a status
        /// </summary>
        public bool Status { get; private set; }
    }
}
