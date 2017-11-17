using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Threading;

namespace csCryptopiaManager
{
    class API
    {
        private string burl = "https://blockchain.info/ticker";
        private string curl = "https://www.cryptopia.co.nz/api/";
        // Although I'd prefer the following to be private, I need to serialize them for saving and loading.
        // On the plus side, I may as well make the lot public to pad the encrypted data.
        public string APIkey { get; set; }
        public string APIsecret { get; set; }
        public bool keysProvided;
        public int nonce = -1;
        // These are known numerical string names for currencyKeyer
        public List<int> numNames { get; set; }
        private static Object fileLocker = new Object();
        public bool logEnabled = false;

        //Empty Constructor for Public queries
        public API()
        {
            APIkey = "";
            APIsecret = "";
            keysProvided = false;
            numNames = new List<int>(new int[] { 1337, 300, 42, 808, 888, 611 });
        }

        // 2 Param constructor for Private queries
        public API(string key, string secret)
        {
            setKeys(key, secret);
        }

        // ############################################## PUBLIC CLASS FUNCTIONS ################################################
        // setKeys
        public void setKeys(string key, string secret)
        {
            APIkey = key;
            APIsecret = secret;
            keysProvided = true;
            numNames = new List<int>(new int[] { 1337, 300, 42, 808, 888, 611 });
        }

        // Serialize blank copy of self.
        public string serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        // ############################################### EXTERNAL API FUNCTIONS ###############################################
        // ##### PUBLIC BTC API ##### - GetBTCPrice - ##### PUBLIC BTC API #####
        public string getFiatPrice()
        {
            return makeBTCQuery();
        }
        // ##### PUBLIC API ##### - GetCurrencies - ##### PUBLIC API #####
        public string getCurrencies()
        {
            return makePubQuery("GetCurrencies/");
        }

        // ##### PUBLIC API ##### - GetTradePairs - ##### PUBLIC API #####
        public string getTradePairs()
        {
            return makePubQuery("GetTradePairs/");
        }

        // ##### PUBLIC API ##### - GetMarkets, basecurrency overriden to "All", provide "BTC/" or similar, Hours. - ##### PUBLIC API #####
        public string getMarkets(string basecurrency = "", string hours = "24/")
        {
            return makePubQuery("GetMarkets/" + basecurrency + hours);
        }

        // ##### PUBLIC API ##### - GetMarket, TradePairID or underscore market name eg. "100" or "DOT_BTC/" (Required), Hours. - ##### PUBLIC API #####
        public string getMarket(string market, string hours = "24/")
        {
            return makePubQuery("GetMarket/" + market + hours);
        }

        // ##### PUBLIC API ##### - GetMarketHistory, TradePairID or underscore market name eg. "100" or "DOT_BTC/" (Required), Hours. - ##### PUBLIC API #####
        public string getMarketHistory(string market, string hours = "24/")
        {
            return makePubQuery("GetMarketHistory/" + market + hours);
        }

        // ##### PUBLIC API ##### - GetMarketOrders, TradePairID or underscore market name eg. "100" or "DOT_BTC/" (Required), Count. - ##### PUBLIC API #####
        public string getMarketOrders(string market, string count = "100/")
        {
            return makePubQuery("GetMarketOrders/" + market + count);
        }

        // ##### PUBLIC API ##### - GetMarketOrderGroups, TradePairID or Market names eg. "100" or "DOT_BTC/" (Required), Count. - ##### PUBLIC API #####
        public string getMarketOrderGroups(string marketgroupstring, string count = "100/")
        {
            return makePubQuery("GetMarketOrderGroups/" + marketgroupstring + count);
        }

        // ##### PRIVATE API ##### - GetBalances, Currency Symbol or ID, empty=All. - ##### PRIVATE API #####
        public string getBalance(string currency = "")
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = currencyKeyer(currency);
            postParams.Add(paramKey, currency);
            return makePrivQuery("GetBalance/", postParams);
        }

        // ##### PRIVATE API ##### - GetDepositAddress, Currency Symbol or ID, Required. - ##### PRIVATE API #####
        public string getDepositAddress(string currency)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = currencyKeyer(currency);
            postParams.Add(paramKey, currency);
            return makePrivQuery("GetDepositAddress/", postParams);
        }

        // ##### PRIVATE API ##### - GetOpenOrders, Market Symbol or ID, empty=All, Count - ##### PRIVATE API #####
        public string getOpenOrders(string market = "", string count = "100")
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = marketKeyer(market);
            postParams.Add(paramKey, market);
            postParams.Add("Count", count);
            return makePrivQuery("GetOpenOrders/", postParams);
        }

        // ##### PRIVATE API ##### - GetTradeHistory, Market Name or ID, empty=All, Count - ##### PRIVATE API #####
        public string getTradeHistory(string market = "", string count = "100")
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = marketKeyer(market);
            postParams.Add(paramKey, market);
            postParams.Add("Count", count);
            return makePrivQuery("GetTradeHistory/", postParams);
        }

        // ##### PRIVATE API ##### - GetTransactions, Type "Deposit" or "Withdraw", Count - ##### PRIVATE API #####
        public string getTransactions(string type, string count = "100")
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("Type", type);
            postParams.Add("Count", count);
            return makePrivQuery("GetTransactions", postParams);
        }

        // ##### PRIVATE API ##### - SubmitTrade, Market Name or ID, Type (Buy/Sell), Rate, Amount - ##### PRIVATE API #####
        public string submitTrade(string market, string type, string rate, string amount)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = marketKeyer(market);
            postParams.Add(paramKey, market);
            postParams.Add("Type", type);
            postParams.Add("Rate", rate);
            postParams.Add("Amount", amount);
            return makePrivQuery("SubmitTrade/", postParams);
        }

        // ##### PRIVATE API ##### - CancelTrade, Type "All", "Trade" or "TradePair", [OrderID, TradePairID] Exclusive but optional - ##### PRIVATE API #####
        public string cancelTrade(string type, string cancelid = "")
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("Type", type);
            switch (type)
            {
                case "Trade":
                    postParams.Add("OrderID", cancelid);
                    break;
                case "TradePair":
                    postParams.Add("TradePairID", cancelid);  // We won't be sensing the key here, it's ALWAYS TradePairId.
                    break;
                    // No Case default, if it's All then no ID is required.
            }
            return makePrivQuery("CancelTrade/", postParams);
        }

        // ##### PRIVATE API ##### - SubmitTip, Currency Name or ID, No of active users, Total Amount - ##### PRIVATE API #####
        public string submitTip(string currency, string users, string amount)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = currencyKeyer(currency);
            postParams.Add(paramKey, currency);
            postParams.Add("ActiveUsers", users);
            postParams.Add("Amount", amount);
            return makePrivQuery("SubmitTip/", postParams);

        }

        // ##### PRIVATE API ##### - SubmitWithdraw, Currency Name or ID, Address, Amount, PaymentID (Not Required). - ##### PRIVATE API #####
        public string submitWithdraw(string currency, string address, string amount, string payid = null)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = currencyKeyer(currency);
            postParams.Add(paramKey, currency);
            postParams.Add("Address", address);
            postParams.Add("Amount", amount);
            if (payid != null || payid != "")
            {
                postParams.Add("PaymentId", payid);
            }
            return makePrivQuery("SubmitWithdraw/", postParams);
        }

        // ##### PRIVATE API ##### - SubmitTransfer, Currency Name or ID, Username, Amount - ##### PRIVATE API #####
        public string submitTransfer(string currency, string username, string amount)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string paramKey = currencyKeyer(currency);
            postParams.Add(paramKey, currency);
            postParams.Add("Username", username);
            postParams.Add("Amount", amount);
            return makePrivQuery("SubmitTransfer/", postParams);
        }

        // ############################################### INTERNAL API FUNCTIONS ###############################################
        // Noncifier™ - Increment the nonce and attach a timestamp.
        private string noncifier()
        {
            nonce += 1;
            return nonce.ToString() + "." + DateTime.Now.Ticks.ToString();
        }

        // Currency Keyer™ - Returns Currency if name is a string or CurrencyID if it looks to be numerical
        private string currencyKeyer(string cur)
        {
            int iCur;
            if (int.TryParse(cur, out iCur)) // It's numeric at least
            {
                if (numNames.Contains(iCur)) // It's a known numeric name
                {
                    return "Currency";
                }
                else // Definitely a CurrencyId
                {
                    return "CurrencyId";
                }
            }
            else // Whatever it is, it doesn't contain numbers!
            {
                return "Currency";
            }
        }

        // Market Keyer™ - Returns Market if name is a string or MarketID if it's numerical.
        private string marketKeyer(string mark)
        {
            int iMark;
            if (int.TryParse(mark, out iMark)) // It's numeric
            {
                return "TradePairId";
            }
            else // String
            {
                return "Market";
            }
        }

        // Returns All Fiat->BTC Prices.
        private string makeBTCQuery()
        {
            string tmpnonce = noncifier();
            logger("Making BTC API Query: " + burl);
            string httpdata = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(burl);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    httpdata = reader.ReadToEnd();
                }
                logger("BTC API Returned Payload");
                return httpdata;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    logger("Timeout on BTC API Query: " + burl);
                    return "API ERROR! : Timeout from: " + burl;
                }
                else throw;
            }
        }

        // Takes subdirectory parameters and makes the HTTP request, returning either data or error.
        private string makePubQuery(string urlParams)
        {
            string tmpnonce = noncifier();
            string fullurl = curl + urlParams;
            logger("Making Public API Query: " + fullurl);
            string httpdata = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullurl);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    httpdata = reader.ReadToEnd();
                }
                APIResponse apiresponse = JsonConvert.DeserializeObject<APIResponse>(httpdata);
                if (apiresponse.Success && apiresponse.Error == null) // Apparently Success is true even in the event of an error!
                {
                    logger("Public API Returned Data payload");  // Public data sets are far too large to store here.
                    return apiresponse.Data.ToString();
                }
                else
                {
                    logger("API Gave Error : " + apiresponse.Error);
                    return "API ERROR! : " + apiresponse.Error;
                }
            }
            catch (WebException e)
            {
                switch (e.Status)
                {
                    case WebExceptionStatus.Timeout:
                        logger("Timeout on Public API Query: " + fullurl);
                        return "API ERROR! : Timeout on Public API Call : " + fullurl;
                    case WebExceptionStatus.ProtocolError:
                        logger("HTTP Protocol Error in API Query: " + fullurl);
                        return "API ERROR! : Protocol Error in API Query: " + fullurl;
                    default:
                        logger("HTTP Error " + e.Message + " from: " + fullurl);
                        return "API ERROR! : Uncaught HTTP error : " + e.Message + " from: " + fullurl;
                }
            }
        }

        // Takes Json params, serializes, hashes, signs, securely submits request and returns response.
        private string makePrivQuery(string command, Dictionary<string, string> apiParams)
        {
            if (keysProvided)
            {
                string uri = curl + command;
                string serialParams = JsonConvert.SerializeObject(apiParams);
                string reqStr;
                string tmpNonce = noncifier();
                string strB64EncParams;
                string strSignedParams;
                string htmlResult;
                logger("Making Public API Query: " + uri);
                // MD5 Hash the Serialized Parameters
                using (MD5 md5 = MD5.Create())
                {
                    strB64EncParams = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(serialParams)));
                }
                // Compose request string and attempt to sign with HMAC SHA256
                reqStr = APIkey + "POST" + Uri.EscapeDataString(uri).ToLower() + tmpNonce + strB64EncParams;
                try
                {
                    using (HMACSHA256 hmacsha256 = new HMACSHA256(Convert.FromBase64String(APIsecret)))
                    {
                        strSignedParams = Convert.ToBase64String(hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(reqStr)));
                    }
                }
                catch
                {
                    return "API ERROR! : Could not sign parameters, please check API keys and try again.";
                }
                // Attempt to perform HTTP Post
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Set(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                        wc.Headers.Set(HttpRequestHeader.Authorization, "amx " + APIkey + ":" + strSignedParams + ":" + tmpNonce);
                        htmlResult = wc.UploadString(uri, serialParams);
                    }
                    // Response is a JSON Object.
                    APIResponse apiresponse = JsonConvert.DeserializeObject<APIResponse>(htmlResult);
                    if (apiresponse.Success && apiresponse.Error == null) // Apparently Success is true even in the event of an error!
                    {
                        // The API *can* return a null data object, namely on the TIP submit.
                        if (apiresponse.Data != null)
                        {
                            logger("Private API Returned Data payload");  // Public data sets are far too large to store here.
                            return apiresponse.Data.ToString();
                        }
                        else
                        {
                            logger("Private API Returned NULL Data payload");  // Public data sets are far too large to store here.
                            return "API RESPONDS SUCCESS: Returned Data is NULL";
                        }
                    }
                    else
                    {
                        logger("API Gave Error : " + apiresponse.Error);
                        return "API ERROR! : " + apiresponse.Error;
                    }
                }
                catch (WebException e)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            logger("Timeout on Private API Query: " + uri + " |Params| " + serialParams);
                            return "API ERROR! : Timeout from " + uri;
                        case WebExceptionStatus.ProtocolError:
                            logger("Protocol Error on Private API Query: " + uri + " |Params| " + serialParams);
                            return "API ERROR! : Protocol Error from " + uri;
                        default:
                            logger("HTTP Error " + e.Message + " from: " + uri);
                            return "API ERROR! : Uncaught HTTP error : " + e.Message + " from: " + uri;
                    }
                }
            }
            else
            {
                return "API ERROR! : API Keys Not Provided! Quite Frankly, I'm amazed you got this far!";
            }
        }

        // Log a line to a text file.
        private void logger(string logLine)
        {
            if (logEnabled)
            {
                // Timestamp then thread up the filewriter. (Actual API data uses Time epoch instead of date)
                string log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + logLine + "\r\n";
                Thread logThrd = new Thread(() => writeLog(log));
                logThrd.Start();
            }
        }

        // Write the log line, THREADED
        private void writeLog(string logLine)
        {
            lock (fileLocker)
            {
                if (!Directory.Exists(@"Captains Log\"))
                {
                    Directory.CreateDirectory(@"Captains Log\");
                }
                using (FileStream file = new FileStream(@"Captains Log\API.log", FileMode.Append, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(file, Encoding.Unicode))
                {
                    writer.Write(logLine.ToString());
                }
            }
        }
    }

    // Our API return is an object with one out of three fields being the payload.  This will temporarily store it
    // So we can return either the Error Message or the Payload.
    [JsonObject(MemberSerialization.OptOut)]
    class APIResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public Object Data { get; set; }
    }
}
