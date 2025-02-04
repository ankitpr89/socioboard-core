﻿using Api.Socioboard.Model;
using GlobusInstagramLib.Authentication;
using GlobusLinkedinLib.Authentication;
using GlobusTwitterLib.Authentication;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;
using Api.Socioboard.Helper;
using System.Text.RegularExpressions;
using System.Threading;

namespace Api.Socioboard.Services
{
    /// <summary>
    /// Summary description for Companypage
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Companypage : System.Web.Services.WebService
    {
        ILog logger = LogManager.GetLogger(typeof(Companypage));
        private InstagramAccountRepository instagramRepo = new InstagramAccountRepository();
        private TumblrAccountRepository tmblrRepo = new TumblrAccountRepository();
        private LinkedInAccountRepository objLinkedinrepo = new LinkedInAccountRepository();
        private FacebookAccountRepository objfbaccountrepo = new FacebookAccountRepository();
        private BoardRepository boardrepo = new BoardRepository();
        //private facebookPageInfoRepository ObjfacebookPageInfoRepository = new facebookPageInfoRepository();
        #region FacebookLogic
        public bool CheckFacebookToken(string fbtoken, string txtvalue)
        {
            bool checkFacebookToken = false;
            try
            {
                string facebookSearchUrl = "https://graph.facebook.com/search?q=" + txtvalue + " &type=post&access_token=" + fbtoken;
                var facerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
                facerequest.Method = "GET";
                string outputface = string.Empty;
                using (var response = facerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
                checkFacebookToken = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return checkFacebookToken;
        }


        public string getfacebookActiveAceessTokenFromDb()
        {
            FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
            ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccounts();
            string accesstoken = string.Empty;
            foreach (Domain.Socioboard.Domain.FacebookAccount item in asltFbAccount)
            {
                accesstoken = item.AccessToken;
                if (this.CheckFacebookToken(accesstoken, "globussoft"))
                {
                    break;
                }
            }
            return accesstoken;
        }

        //Start Facebook Search Logic
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string SearchFacebookPage(string Keyword)
        {
            Domain.Socioboard.Domain.FacebookAccount fbaccount = null;
            string facebookResultPage = string.Empty;
            int likes = 0;
            facebookResultPage = this.getFacebookPage(Keyword.Replace(" ", string.Empty));
            string error = string.Empty;
            try
            {
                JObject checkpage = JObject.Parse(facebookResultPage);
                error = checkpage["error"].ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(facebookResultPage))
            {
                string fbpagelist = this.getFacebookkPageList(Keyword);
                if (!fbpagelist.StartsWith("["))
                    fbpagelist = "[" + fbpagelist + "]";
                JArray fbpageArray = JArray.Parse(fbpagelist);
                foreach (var item in fbpageArray)
                {
                    var data = item["data"];

                    foreach (var page in data)
                    {
                        try
                        {
                            string fbpage = this.getFacebookPage(page["id"].ToString());
                            JObject pageresult = JObject.Parse(fbpage);
                            if (Convert.ToInt32(pageresult["likes"].ToString()) > likes)
                            {
                                facebookResultPage = fbpage;
                                likes = Convert.ToInt32(pageresult["likes"].ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.StackTrace);
                        }
                    }
                }

            }
            //Domain.Socioboard.Domain.facebookpageinfo fbPageInfo = new Domain.Socioboard.Domain.facebookpageinfo();
            //JObject fb = JObject.Parse(facebookResultPage);
            //try
            //{
            //    fbPageInfo.companyname = fb["name"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.about = fb["about"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.founded = fb["founded"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.description = fb["description"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.country = fb["location"]["country"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.city = fb["location"]["city"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.pageid = fb["id"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.pagelikes = fb["likes"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.phone = fb["phone"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.username = fb["username"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.link = fb["link"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.mission = fb["mission"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.talkingabout = fb["talking_about_count"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}
            //try
            //{
            //    fbPageInfo.website = fb["website"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace);
            //}                         

            return facebookResultPage;
        }

        public string getFacebookPageFeeds(string PageId)
        {
            //FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
            //ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccounts();
            string accesstoken = "CAAKYvwDVmnUBAJtKlQvRNTf3ACcN2loYYae5jhBuR6xwQeD6YY6psMoRKo6HBChX1O3Ec5Hf6hI9Hsn4PwGefwZCbbQmVbHX7JlspLYs6eSQfFU9rx7kOOlXdcvSOnx65TjwRCKPTElgov8Hv90wQu83OivXc9bbqDii4u583mKZA59runloi16ZBqF82W1pT7vnjbb8ZB0g7Bt6AXm2";
            //string accesstoken = "CAACZB5L4uuV8BACXwWhgpnE6lrSuIz0vdr6HtMQM8rUEKFPBVfhuYr56OCvPmRqsWPoYaMtYmaRGPZCqRqa562eaoSXaa1xScB5zKtE5jHFw07wI0GENjFOnluGrduNhHRqJT1iNUCFnTh5GXmZAtc4AiZAPMvVXS9EidsDo9PNVQwd262eSFapVZCFvxJpIZD";
            //foreach (Domain.Socioboard.Domain.FacebookAccount item in asltFbAccount)
            //{
            //    accesstoken = item.AccessToken;
            //    if (this.CheckFacebookToken(accesstoken, PageId))
            //    {
            //        break;
            //    }
            //}
            string facebookSearchUrl = "https://graph.facebook.com/v1.0/" + PageId + "/posts?limit=20&access_token=" + accesstoken;
            var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
            facebooklistpagerequest.Method = "GET";
            facebooklistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            facebooklistpagerequest.AllowWriteStreamBuffering = true;
            facebooklistpagerequest.ServicePoint.Expect100Continue = false;
            facebooklistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = facebooklistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;

        }


        public string getFacebookTagFeeds(string HashTag, Guid BoardfbPageId)
        {
            HashTag = Uri.EscapeUriString(HashTag);
            HashTag = HashTag.Replace("%20%E2%80%8E", string.Empty);
            //string accesstoken = "CAAKYvwDVmnUBAJtKlQvRNTf3ACcN2loYYae5jhBuR6xwQeD6YY6psMoRKo6HBChX1O3Ec5Hf6hI9Hsn4PwGefwZCbbQmVbHX7JlspLYs6eSQfFU9rx7kOOlXdcvSOnx65TjwRCKPTElgov8Hv90wQu83OivXc9bbqDii4u583mKZA59runloi16ZBqF82W1pT7vnjbb8ZB0g7Bt6AXm2";
            //string facebookSearchUrl = "https://graph.facebook.com/search?type=post&q=%23" + HashTag + "&access_token=" + accesstoken;
            //var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
            //facebooklistpagerequest.Method = "GET";
            //facebooklistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            //facebooklistpagerequest.AllowWriteStreamBuffering = true;
            //facebooklistpagerequest.ServicePoint.Expect100Continue = false;
            //facebooklistpagerequest.PreAuthenticate = false;
            //string outputface = string.Empty;
            //try
            //{
            //    using (var response = facebooklistpagerequest.GetResponse())
            //    {
            //        using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
            //        {
            //            outputface = stream.ReadToEnd();
            //        }
            //    }
            //}
            //catch (Exception e) { }
            //return outputface;
            //GlobusHttpHelper objGlobusHttpHelper = new GlobusHttpHelper();
            //FacebookUser fbUser = new FacebookUser();
            //fbUser.username = ConfigurationManager.AppSettings["fb_userid"];
            //fbUser.password = ConfigurationManager.AppSettings["fb_pass"];


            //fbUser.globusHttpHelper = objGlobusHttpHelper;
            //LoginUsingGlobusHttp(ref fbUser);

            //GlobusHttpHelper HttpHelper = fbUser.globusHttpHelper;

            //ScraperHasTage(ref fbUser, HashTag, BoardfbPageId);
            ScraperHasTage(HashTag, BoardfbPageId);

            return "";

        }

        //search facebook for pages and return page list
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getFacebookkPageList(string Keyword)
        {
            //getting active token
            //FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
            //ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccounts();
            string accesstoken = getfacebookActiveAceessTokenFromDb();
            //foreach (Domain.Socioboard.Domain.FacebookAccount item in asltFbAccount)
            //{
            //    accesstoken = item.AccessToken;
            //    if (this.CheckFacebookToken(accesstoken, Keyword))
            //    {

            //        break;
            //    }
            //}

            // getting search results
            string facebookSearchUrl = "https://graph.facebook.com/v1.0/search?q=" + Keyword + "&type=page&access_token=" + accesstoken + "&limit=10";
            var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
            facebooklistpagerequest.Method = "GET";
            facebooklistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            facebooklistpagerequest.AllowWriteStreamBuffering = true;
            facebooklistpagerequest.ServicePoint.Expect100Continue = false;
            facebooklistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = facebooklistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }

        //Takes pageId as input and return fb page details
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getFacebookPage(string PageId)
        {
            string pageUrl = "http://graph.facebook.com/" + PageId.ToString();
            var fbpage = (HttpWebRequest)WebRequest.Create(pageUrl);
            fbpage.Method = "GET";
            string Outputpage = string.Empty;
            try
            {
                using (var response = fbpage.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        Outputpage = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return Outputpage;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getFacebookPageNotes(string PageId)
        {
            //FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
            //ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccounts();
            string accesstoken = getfacebookActiveAceessTokenFromDb();
            //foreach (Domain.Socioboard.Domain.FacebookAccount item in asltFbAccount)
            //{
            //    accesstoken = item.AccessToken;
            //    if (this.CheckFacebookToken(accesstoken, PageId))
            //    {
            //        break;
            //    }
            //}
            string facebookSearchUrl = "https://graph.facebook.com/v1.0/" + PageId + "/notes?access_token=" + accesstoken;
            var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
            facebooklistpagerequest.Method = "GET";
            facebooklistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            facebooklistpagerequest.AllowWriteStreamBuffering = true;
            facebooklistpagerequest.ServicePoint.Expect100Continue = false;
            facebooklistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = facebooklistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;

        }



        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public bool IsfacebookAccountVerified(string fbAccountId)
        {
            bool Isverified = false;
            string AccessToken = getfacebookActiveAceessTokenFromDb();
            string Url = "https://graph.facebook.com//v2.1/20528438720?fields=is_verified&access_token=" + AccessToken;
            var fbpage = (HttpWebRequest)WebRequest.Create(Url);
            fbpage.Method = "GET";
            string Outputpage = string.Empty;
            try
            {
                using (var response = fbpage.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        Outputpage = stream.ReadToEnd();
                    }
                }
                JObject JobjResult = JObject.Parse(Outputpage);
                if (JobjResult["is_verified"].ToString().Equals("true"))
                {
                    Isverified = true;
                }
            }
            catch (Exception e) { }
            return Isverified;
        }

        # endregion

        # region twitter Logic
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string TwitterSearch(string keyword)
        {
            string SingleTwitterPageResult = string.Empty;
            try
            {
                SingleTwitterPageResult = TwitterAccountPageWithoutLogin("", keyword);
                if (!string.IsNullOrEmpty(SingleTwitterPageResult))
                {
                    return SingleTwitterPageResult;
                }
            }
            catch (Exception eee) { }
            //int Followers = 0;
            bool ischanged = false;
            string TwitterResutPage = string.Empty;
            string TwitterResutPageid = string.Empty;
            string ScreenName = string.Empty;
            List<Domain.Socioboard.Domain.DiscoverySearch> lstDiscoverySearch = new List<Domain.Socioboard.Domain.DiscoverySearch>();
            string profileid = string.Empty;
            try
            {
                oAuthTwitter oauth = new oAuthTwitter();
                Twitter obj = new Twitter();
                TwitterAccountRepository twtAccRepo = new TwitterAccountRepository();
                //ArrayList alst = twtAccRepo.getAllTwitterAccounts();
                oauth.AccessToken = Twitterapponlykey();
                //oauth.AccessTokenSecret = "beScSFa1uI7MttvgjoDPjxYMKgC0Mq2EUYzYewbbNvobO";
                //oauth.ConsumerKey = "LvHB4sHi0RWcQF7MmrstXhEJE";
                //oauth.ConsumerKeySecret = "vd5cdLeje1sThW4cYonIhqWuvKkGk1mZLDu1j1IAbSkLLqp5Kd";
                //oauth.ConsumerKey = ConfigurationManager.AppSettings["consumerKey"];
                //oauth.ConsumerKeySecret = ConfigurationManager.AppSettings["consumerSecret"];
                string twitterSearchResult = Get_Search_Users(oauth, keyword);
                JArray twitterpageArray = JArray.Parse(twitterSearchResult);
                foreach (var item in twitterpageArray)
                {
                    if (item["verified"].ToString().Equals("True"))
                    {
                        TwitterResutPageid = item["id"].ToString();
                        ScreenName = item["screen_name"].ToString();
                        ischanged = true;
                    }
                }
                if (ischanged)
                {
                    TwitterResutPage = Get_Search_SingleUser(oauth, TwitterResutPageid, ScreenName);
                }
                return TwitterResutPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return "";
            }
        }

        public string Twitterapponlykey()
        {
            string retvalu = string.Empty;
            var oauth_consumer_key = "yNt3tISGJji5poVSgSO1Og";
            var oauth_consumer_secret = "BvJQlpnjBxN7rtiGF6fIbcGlTgmRac8O3cOamwmr8X4";
            //Token URL
            var oauth_url = "https://api.twitter.com/oauth2/token";
            var headerFormat = "Basic {0}";
            var authHeader = string.Format(headerFormat,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oauth_consumer_key) + ":" +
            Uri.EscapeDataString((oauth_consumer_secret)))
            ));

            var postBody = "grant_type=client_credentials";

            ServicePointManager.Expect100Continue = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            request.Headers.Add("Accept-Encoding", "gzip");
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
            using (var reader = new StreamReader(responseStream))
            {

                JavaScriptSerializer js = new JavaScriptSerializer();
                var objText = reader.ReadToEnd();
                JObject o = JObject.Parse(objText);
                retvalu = o["access_token"].ToString();

            }
            return retvalu;

        }
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string TwitterAccountPageWithoutLogin(string UserId, string ScreenName)
        {
            JObject output = new JObject();
            try
            {
                SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
                requestParameters.Add("user_id", UserId);
                requestParameters.Add("screen_name", ScreenName);
                //Token URL
                var oauth_url = "https://api.twitter.com/1.1/users/show.json";
                var headerFormat = "Bearer {0}";
                var authHeader = string.Format(headerFormat, "AAAAAAAAAAAAAAAAAAAAAOZyVwAAAAAAgI0VcykgJ600le2YdR4uhKgjaMs%3D0MYOt4LpwCTAIi46HYWa85ZcJ81qi0D9sh8avr1Zwf7BDzgdHT");

                var postBody = requestParameters.ToWebString();
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url + "?"
                       + requestParameters.ToWebString());

                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip");
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using (var reader = new StreamReader(responseStream))
                {

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    output = JObject.Parse(objText);


                }
            }
            catch (Exception e) { }

            return output.ToString();
        }

        public string TwitterTweetDetailsByAppOnlyKey(string TweetId)
        {
            JObject output = new JObject();
            try
            {
                SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
                //requestParameters.Add("user_id", UserId);
                requestParameters.Add("id", TweetId);
                // Token URL
                var oauth_url = "https://api.twitter.com/1.1/statuses/show.json";
                var headerFormat = "Bearer {0}";
                var authHeader = string.Format(headerFormat, "AAAAAAAAAAAAAAAAAAAAAOZyVwAAAAAAgI0VcykgJ600le2YdR4uhKgjaMs%3D0MYOt4LpwCTAIi46HYWa85ZcJ81qi0D9sh8avr1Zwf7BDzgdHT");

                var postBody = requestParameters.ToWebString();
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url + "?"
                       + requestParameters.ToWebString());

                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip");
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using (var reader = new StreamReader(responseStream))
                {

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    output = JObject.Parse(objText);

                }
            }
            catch (Exception ee) { }

            return output.ToString();
        }


        public string TwitterTweetDetails(string TweetId, string Accesstoken, string AccesstokenSecret)
        {
            oAuthTwitter oauth = new oAuthTwitter();

            oauth.AccessToken = Accesstoken;
            oauth.AccessTokenSecret = AccesstokenSecret;
            oauth.ConsumerKey = ConfigurationManager.AppSettings["consumerKey"];
            oauth.ConsumerKeySecret = ConfigurationManager.AppSettings["consumerSecret"];
            string RequestUrl = "https://api.twitter.com/1.1/statuses/show.json";
            SortedDictionary<string, string> strdic = new SortedDictionary<string, string>();
            strdic.Add("id", TweetId);

            string response = oauth.oAuthWebRequest(oAuthTwitter.Method.GET, RequestUrl, strdic);

            return response;
        }



        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string TwitterUserTimeLine(string ScreenName, string LastTweetId)
        {
            JArray output = new JArray();
            try
            {
                SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
                //requestParameters.Add("user_id", UserId);
                requestParameters.Add("screen_name", ScreenName);
                if (!string.IsNullOrEmpty(LastTweetId))
                {
                    requestParameters.Add("since_id", LastTweetId);
                }
                //Token URL
                var oauth_url = "https://api.twitter.com/1.1/statuses/user_timeline.json";
                var headerFormat = "Bearer {0}";
                var authHeader = string.Format(headerFormat, "AAAAAAAAAAAAAAAAAAAAAOZyVwAAAAAAgI0VcykgJ600le2YdR4uhKgjaMs%3D0MYOt4LpwCTAIi46HYWa85ZcJ81qi0D9sh8avr1Zwf7BDzgdHT");

                var postBody = requestParameters.ToWebString();
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url + "?"
                       + requestParameters.ToWebString());

                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip");
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using (var reader = new StreamReader(responseStream))
                {

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    output = JArray.Parse(objText);

                }
            }
            catch (Exception ee) { }

            return output.ToString();
        }

        public string Get_Search_Users(oAuthTwitter oAuth, string SearchKeyword)
        {

            string RequestUrl = "https://api.twitter.com/1.1/users/search.json";
            SortedDictionary<string, string> strdic = new SortedDictionary<string, string>();
            strdic.Add("q", SearchKeyword);
            string response = oAuth.oAuthWebRequest(oAuthTwitter.Method.GET, RequestUrl, strdic);
            if (!response.StartsWith("["))
                response = "[" + response + "]";
            return response;
        }

        public string Get_Search_SingleUser(oAuthTwitter oAuth, string SearchKeyword, string ScreenName)
        {

            string RequestUrl = "https://api.twitter.com/1.1/users/show.json";
            SortedDictionary<string, string> strdic = new SortedDictionary<string, string>();
            strdic.Add("user_id", SearchKeyword);
            strdic.Add("screen_name", ScreenName);
            string response = oAuth.oAuthWebRequest(oAuthTwitter.Method.GET, RequestUrl, strdic);

            return response;
        }


        public string TwitterHashTagSearch(string Hashtag, string LastTweetId)
        {
            Hashtag = Uri.EscapeUriString(Hashtag);
            Hashtag = Hashtag.Replace("%20%E2%80%8E", string.Empty);
            JArray output = new JArray();
            try
            {
                SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
                //requestParameters.Add("user_id", UserId);
                //requestParameters.Add("screen_name", Hashtag);
                //requestParameters.Add("count", "198");
                //Token URL
                var oauth_url = " https://api.twitter.com/1.1/search/tweets.json?q=%23" + Hashtag.TrimStart() + "&result_type=recent&count=30";
                if (!string.IsNullOrEmpty(LastTweetId))
                {
                    oauth_url = oauth_url + "&since_id=" + LastTweetId;
                }
                var headerFormat = "Bearer {0}";
                var authHeader = string.Format(headerFormat, "AAAAAAAAAAAAAAAAAAAAAOZyVwAAAAAAgI0VcykgJ600le2YdR4uhKgjaMs%3D0MYOt4LpwCTAIi46HYWa85ZcJ81qi0D9sh8avr1Zwf7BDzgdHT");

                var postBody = requestParameters.ToWebString();
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url + "?"
                       + requestParameters.ToWebString());

                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip");
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using (var reader = new StreamReader(responseStream))
                {

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    output = JArray.Parse(JObject.Parse(objText)["statuses"].ToString());

                }
            }
            catch (Exception ee) { }

            return output.ToString();
        }

        # endregion

        # region Linkedin Logic

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string LinkedinSearch(string keyword)
        {
            string profileid = string.Empty;
            try
            {
                // ArrayList alstLIAccounts = objLinkedinrepo.getAllLinkedinAccounts();
                //Domain.Socioboard.Domain.LinkedInAccount linkacc = (Domain.Socioboard.Domain.LinkedInAccount)alstLIAccounts[0];
                oAuthLinkedIn oauth = new oAuthLinkedIn();
                oauth.ConsumerKey = ConfigurationManager.AppSettings["LiApiKey"];
                oauth.ConsumerSecret = ConfigurationManager.AppSettings["LiSecretKey"];
                //oauth.Token = "49c2202b-2cd4-4c74-b5db-ce8d7f5e029e";
                //oauth.TokenSecret = "a79cfbe5-d268-456e-8fdc-0d12869a1cf3";
                //oauth.Verifier = "52921";
                oauth.Token = "b82db6bb-21bb-44d2-a298-0b093708ddbf";
                oauth.TokenSecret = "f7c9b7b8-9295-46fe-8cb4-914c1c52820f";
                oauth.Verifier = "23836";
                //oauth.AccessTokenGet(linkacc.OAuthToken);
                //TODO: access Token Logic
                oauth.AccessTokenGet("b82db6bb-21bb-44d2-a298-0b093708ddbf");
                //oauth.AccessTokenGet();

                //https://api.linkedin.com/v1/people-search? keywords=[space delimited keywords]
                //oauth.AccessTokenGet(oauth.Token);
                // company.Get_CompanyProfileById(oauth, keyword);
                //string response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/companies/" + keyword + ":(id,name,email-domains,description,founded-year,end-year,locations,Specialties,website-url,status,employee-count-range,industries,company-type,logo-url,square-logo-url,blog-rss-url,num-followers,universal-name)", null);
                string response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/company-search" + ":(companies:(id,name,universal-name,website-url,industries,status,logo-url,blog-rss-url,twitter-id,employee-count-range,specialties,locations,description,stock-exchange,founded-year,end-year,num-followers))?keywords=" + keyword, null);
                XmlDocument XmlResult = new XmlDocument();
                XmlResult.Load(new StringReader(response));


                XmlNode ResultCompany = null;
                int followers = 0;
                string result = string.Empty;
                XmlNodeList Companies = XmlResult.SelectNodes("company-search/companies/company");
                foreach (XmlNode node in Companies)
                {
                    if (Convert.ToInt32(node.SelectSingleNode("num-followers").InnerText) > followers)
                    {
                        ResultCompany = node;
                        followers = Convert.ToInt32(node.SelectSingleNode("num-followers").InnerText);
                    }

                }



                return response;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return new JavaScriptSerializer().Serialize("Please try Again");
            }
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string LinkedinCompanyrecentActivites(string CompanyId)
        {
            string response = string.Empty;
            try
            {
                ArrayList alstLIAccounts = objLinkedinrepo.getAllLinkedinAccounts();
                Domain.Socioboard.Domain.LinkedInAccount linkacc = (Domain.Socioboard.Domain.LinkedInAccount)alstLIAccounts[0];
                oAuthLinkedIn oauth = new oAuthLinkedIn();
                oauth.ConsumerKey = ConfigurationManager.AppSettings["LiApiKey"];
                oauth.ConsumerSecret = ConfigurationManager.AppSettings["LiSecretKey"];
                oauth.Token = "49c2202b-2cd4-4c74-b5db-ce8d7f5e029e";
                oauth.TokenSecret = "a79cfbe5-d268-456e-8fdc-0d12869a1cf3";
                oauth.Verifier = "52921";
                oauth.AccessTokenGet(linkacc.OAuthToken);

                //oauth.AccessTokenGet();

                //https://api.linkedin.com/v1/people-search? keywords=[space delimited keywords]
                //oauth.AccessTokenGet(oauth.Token);
                // company.Get_CompanyProfileById(oauth, keyword);
                //string response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/companies/" + keyword + ":(id,name,email-domains,description,founded-year,end-year,locations,Specialties,website-url,status,employee-count-range,industries,company-type,logo-url,square-logo-url,blog-rss-url,num-followers,universal-name)", null);
                response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/companies/" + CompanyId + "/updates?start=0&count=10&event-type=status-update", null);
            }
            catch (Exception e) { }
            return response;

        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string LinkedinCompnayJobs(string CompanyId)
        {
            string response = string.Empty;
            try
            {
                ArrayList alstLIAccounts = objLinkedinrepo.getAllLinkedinAccounts();
                Domain.Socioboard.Domain.LinkedInAccount linkacc = (Domain.Socioboard.Domain.LinkedInAccount)alstLIAccounts[0];
                oAuthLinkedIn oauth = new oAuthLinkedIn();
                oauth.ConsumerKey = ConfigurationManager.AppSettings["LiApiKey"];
                oauth.ConsumerSecret = ConfigurationManager.AppSettings["LiSecretKey"];
                oauth.Token = "49c2202b-2cd4-4c74-b5db-ce8d7f5e029e";
                oauth.TokenSecret = "a79cfbe5-d268-456e-8fdc-0d12869a1cf3";
                oauth.Verifier = "52921";
                oauth.AccessTokenGet(linkacc.OAuthToken);

                //oauth.AccessTokenGet();

                //https://api.linkedin.com/v1/people-search? keywords=[space delimited keywords]
                //oauth.AccessTokenGet(oauth.Token);
                // company.Get_CompanyProfileById(oauth, keyword);
                //string response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/companies/" + keyword + ":(id,name,email-domains,description,founded-year,end-year,locations,Specialties,website-url,status,employee-count-range,industries,company-type,logo-url,square-logo-url,blog-rss-url,num-followers,universal-name)", null);
                response = oauth.APIWebRequest("GET", "https://api.linkedin.com/v1/jobs/" + CompanyId, null);
            }
            catch (Exception e) { }
            return response;

        }
        # endregion

        #region Instagram Logic
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string InstagramSearch(string keyword, string WebUrl)
        {
            string response = string.Empty;
            string resId = string.Empty;
            try
            {
                GlobusInstagramLib.Authentication.ConfigurationIns configi = new GlobusInstagramLib.Authentication.ConfigurationIns("https://api.instagram.com/oauth/authorize/", "f5f052ccbdf94df490020f852863141b", "6c8ac0efa42c4c918bf33835fc98a793", "http://localhost:9821/InstagramManager/Instagram", "http://api.instagram.com/oauth/access_token", "https://api.instagram.com/v1/", "");
                oAuthInstagram _api = new oAuthInstagram();
                _api = oAuthInstagram.GetInstance(configi);
                AccessToken access = new AccessToken();
                ArrayList arrList = instagramRepo.getAllInstagramAccounts();
                Domain.Socioboard.Domain.InstagramAccount instaacc = (Domain.Socioboard.Domain.InstagramAccount)arrList[0];
                string tk = instaacc.AccessToken;
                response = _api.WebRequest(oAuthInstagram.Method.GET, "https://api.instagram.com/v1/users/search?q=" + keyword + "&access_token=" + tk, null);
                if (!response.StartsWith("["))
                    response = "[" + response + "]";
                JArray Instagramaccarray = JArray.Parse(response);
                foreach (var acc in Instagramaccarray)
                {
                    var data = acc["data"];

                    foreach (var page in data)
                    {
                        try
                        {

                            if (page["website"].ToString().Equals(WebUrl))
                            {
                                resId = page["id"].ToString();
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            if (string.IsNullOrEmpty(resId))
            {
                return string.Empty;
            }
            return InstagramSingleUser(resId);
        }


        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string InstagramSingleUser(string UserId)
        {
            string response = string.Empty;
            try
            {
                GlobusInstagramLib.Authentication.ConfigurationIns configi = new GlobusInstagramLib.Authentication.ConfigurationIns("https://api.instagram.com/oauth/authorize/", "f5f052ccbdf94df490020f852863141b", "6c8ac0efa42c4c918bf33835fc98a793", "http://localhost:9821/InstagramManager/Instagram", "http://api.instagram.com/oauth/access_token", "https://api.instagram.com/v1/", "");
                oAuthInstagram _api = new oAuthInstagram();
                _api = oAuthInstagram.GetInstance(configi);
                AccessToken access = new AccessToken();
                ArrayList arrList = instagramRepo.getAllInstagramAccounts();
                Domain.Socioboard.Domain.InstagramAccount instaacc = (Domain.Socioboard.Domain.InstagramAccount)arrList[1];
                string tk = instaacc.AccessToken;
                response = _api.WebRequest(oAuthInstagram.Method.GET, "https://api.instagram.com/v1/users/" + UserId + "/?access_token=" + tk, null);


            }
            catch (Exception ex)
            {

            }

            return response;
        }


        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getInstagramCompanyPage(string Keyword)
        {
            int followers = 0;
            string ResultPage = string.Empty;
            try
            {
                string instagrampagelist = getInstagramList(Keyword);
                if (!instagrampagelist.StartsWith("["))
                    instagrampagelist = "[" + instagrampagelist + "]";
                JArray fbpageArray = JArray.Parse(instagrampagelist);
                foreach (var item in fbpageArray)
                {
                    var data = item["data"];

                    foreach (var page in data)
                    {
                        try
                        {
                            string instagrampage = this.getInstagramUserDetails(page["id"].ToString());
                            JObject pageresult = JObject.Parse(instagrampage);
                            if (Convert.ToInt32(pageresult["data"]["counts"]["followed_by"].ToString()) > followers)
                            {
                                ResultPage = instagrampage;
                                followers = Convert.ToInt32(pageresult["data"]["counts"]["followed_by"].ToString());
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            catch (Exception e) { }

            return ResultPage;
        }
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getInstagramList(string Keyword)
        {
            string InstagramUrl = "https://api.instagram.com/v1/users/search?q=" + Keyword + "&client_id=" + ConfigurationManager.AppSettings["InstagramClientKey"] + "&count=10";
            HttpWebRequest Instagramlistpagerequest = (HttpWebRequest)WebRequest.Create(InstagramUrl);
            Instagramlistpagerequest.Method = "GET";
            Instagramlistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            Instagramlistpagerequest.AllowWriteStreamBuffering = true;
            Instagramlistpagerequest.ServicePoint.Expect100Continue = false;
            Instagramlistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = Instagramlistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getInstagramUserRecentActivities(string UserId)
        {
            string InstagramUrl = "https://api.instagram.com/v1/users/" + UserId + "/media/recent/?client_id=" + ConfigurationManager.AppSettings["InstagramClientKey"];
            var Instagramlistpagerequest = (HttpWebRequest)WebRequest.Create(InstagramUrl);
            Instagramlistpagerequest.Method = "GET";
            Instagramlistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            Instagramlistpagerequest.AllowWriteStreamBuffering = true;
            Instagramlistpagerequest.ServicePoint.Expect100Continue = false;
            Instagramlistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = Instagramlistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string getInstagramUserDetails(string UserId)
        {
            string InstagramUrl = "https://api.instagram.com/v1/users/" + UserId + "/?client_id=" + ConfigurationManager.AppSettings["InstagramClientKey"];
            var Instagramlistpagerequest = (HttpWebRequest)WebRequest.Create(InstagramUrl);
            Instagramlistpagerequest.Method = "GET";
            Instagramlistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            Instagramlistpagerequest.AllowWriteStreamBuffering = true;
            Instagramlistpagerequest.ServicePoint.Expect100Continue = false;
            Instagramlistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = Instagramlistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }


        public string InstagramTagSearch(string TagName)
        {
            string accesstoken = "1045136815.0a58814.28e2a364a96544c4b40f6caa17672e89";
            TagName = Uri.EscapeUriString(TagName);
            TagName = TagName.Replace("%E2%80%AA%E2%80%8E", string.Empty);
            string instagrmHastagSearchURL = "https://api.instagram.com/v1/tags/" + TagName.TrimStart() + "/media/recent?access_token=" + accesstoken;
            var Instagramsearchreq = (HttpWebRequest)WebRequest.Create(instagrmHastagSearchURL);
            Instagramsearchreq.Method = "GET";
            Instagramsearchreq.Credentials = CredentialCache.DefaultCredentials;
            Instagramsearchreq.AllowWriteStreamBuffering = true;
            Instagramsearchreq.ServicePoint.Expect100Continue = false;
            Instagramsearchreq.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = Instagramsearchreq.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }
        #endregion

        #region tumblr Logic
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string TumblrSearch(string keyword)
        {
            string ret = string.Empty;
            string outputface = string.Empty;
            try
            {
                string key = ConfigurationManager.AppSettings["TumblrClientKey"];
                TumblrAccountRepository tumaccrepo = new TumblrAccountRepository();
                Domain.Socioboard.Domain.TumblrAccount TumComAcc = tumaccrepo.getTumblrAccountDetailsById(keyword);
                if (TumComAcc != null && !string.IsNullOrEmpty(TumComAcc.tblrUserName))
                {
                    string TumblrSearchUrl = "http://api.tumblr.com/v2/blog/" + TumComAcc.tblrUserName + ".tumblr.com/posts/text?api_key=" + key + "&limit=10";
                    var TumblrBlogpagerequest = (HttpWebRequest)WebRequest.Create(TumblrSearchUrl);
                    TumblrBlogpagerequest.Method = "GET";
                    try
                    {
                        using (var response = TumblrBlogpagerequest.GetResponse())
                        {
                            using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                            {
                                outputface = stream.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
                else
                {
                    string TumblrSearchUrl = "http://api.tumblr.com/v2/blog/" + keyword.Replace(" ", string.Empty) + ".tumblr.com/posts/text?api_key=" + key + "&limit=10";
                    var TumblrBlogpagerequest = (HttpWebRequest)WebRequest.Create(TumblrSearchUrl);
                    TumblrBlogpagerequest.Method = "GET";
                    try
                    {
                        using (var response = TumblrBlogpagerequest.GetResponse())
                        {
                            using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                            {
                                outputface = stream.ReadToEnd();
                            }
                        }
                        JObject outputJson = JObject.Parse(outputface);
                        Domain.Socioboard.Domain.TumblrAccount newtumbobj = new Domain.Socioboard.Domain.TumblrAccount();
                        newtumbobj.tblrUserName = outputJson["response"]["blog"]["name"].ToString();
                        TumblrAccountRepository.Add(newtumbobj);
                    }
                    catch (Exception ex) { }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            return outputface;
        }

        # endregion

        #region youtube Logic
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string YoutubeSearch(string keyword)
        {
            string response = string.Empty;
            string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
            // YoutubeAccountRepository ytAccrepo = new YoutubeAccountRepository();
            //Domain.Socioboard.Domain.YoutubeAccount ytAccount = ytAccrepo.getYoutubeAccountDetailsByUserName(keyword);
            //if (ytAccount != null && !string.IsNullOrEmpty(ytAccount.Ytusername))
            //{
            //    //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
            //    //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&maxResults=20&q=" + keyword + "&key=" + Key;
            //    string RequestUrl = "https://www.googleapis.com/youtube/v3/channels?part=id,snippet,contentDetails,statistics,topicDetails,invideoPromotion&forUsername=" + ytAccount.Ytusername + "&key=" + Key;
            //    //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&q=" + ytAccount.Ytusername + "&type=channel&key=" + Key;
            //    var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
            //    facebooklistpagerequest.Method = "GET";
            //    try
            //    {
            //        using (var youtuberesponse = facebooklistpagerequest.GetResponse())
            //        {
            //            using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
            //            {
            //                response = stream.ReadToEnd();
            //            }
            //        }
            //    }
            //    catch (Exception e) { }
            //}
            //else
            {
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&q=" + ytAccount.Ytusername + "&type=channel&key=" + Key;
                string SearchList = YoutubeSearchList(keyword);
                string RequestUrl = "https://www.googleapis.com/youtube/v3/channels?part=id,snippet,contentDetails,statistics,topicDetails,invideoPromotion&forUsername=" + keyword + "&key=" + Key;

                try
                {
                    JObject Listresult = JObject.Parse(SearchList);
                    keyword = Listresult["items"][0]["id"]["channelId"].ToString();
                    RequestUrl = "https://www.googleapis.com/youtube/v3/channels?part=id,snippet,contentDetails,statistics,topicDetails,invideoPromotion&id=" + keyword + "&key=" + Key;

                }
                catch (Exception eee) { }

                var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                facebooklistpagerequest.Method = "GET";
                try
                {
                    using (var youtuberesponse = facebooklistpagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();
                        }
                    }
                    if (!response.StartsWith("["))
                        response = "[" + response + "]";
                    //JArray youtubechannels = JArray.Parse(response);
                    //JObject resultPage = (JObject)youtubechannels[0];
                    //Domain.Socioboard.Domain.YoutubeAccount ytnewacc = new Domain.Socioboard.Domain.YoutubeAccount();
                    //ytnewacc.Ytusername = resultPage["items"][0]["snippet"]["title"].ToString();
                    //ytnewacc.Ytuserid = resultPage["items"][0]["id"].ToString();
                    //YoutubeAccountRepository.Add(ytnewacc);
                }
                catch (Exception e) { }
            }


            return response;

        }



        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string YoutubeSearchList(string keyword)
        {
            string response = string.Empty;
            string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";

            //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
            //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&maxResults=20&q=" + keyword + "&key=" + Key;
            //string RequestUrl = "https://www.googleapis.com/youtube/v3/channels?part=id,snippet,contentDetails,statistics,topicDetails,invideoPromotion&forUsername=" + ytAccount.Ytusername + "&key=" + Key;
            string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&q=" + keyword + "&type=channel&key=" + Key;
            var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
            facebooklistpagerequest.Method = "GET";
            try
            {
                using (var youtuberesponse = facebooklistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        response = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return response;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string YoutubeChannelPlayList(string ChannelId)
        {
            string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
            //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
            //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet,id&maxResults=20&q=" + keyword + "&key=" + Key;
            string RequestUrl = "https://www.googleapis.com/youtube/v3/playlists?part=+id,snippet,status,contentDetails&channelId=" + ChannelId + "&key=" + Key;

            var pagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
            pagerequest.Method = "GET";
            string response = string.Empty;
            try
            {
                using (var youtuberesponse = pagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        response = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return response;
        }
        #endregion

        #region gplus Loigic
        public string GooglePlus(string keyword)
        {
            string ret = string.Empty;
            try
            {
                string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
                //AIzaSyCvTBnEDnr5DEpvlVDCuxz9K9TK84rX0fE
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                string RequestUrl = "https://www.googleapis.com/plus/v1/people?query=" + keyword + "&key=" + Key;

                var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                facebooklistpagerequest.Method = "GET";
                string response = string.Empty;
                try
                {
                    using (var youtuberesponse = facebooklistpagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();
                        }
                    }
                }
                catch (Exception e) { }

                return response;
            }
            catch (Exception ex)
            {

                throw;
            }

            //return ret;
        }

        //Google Plus
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GooglePlusSearch(string keyword)
        {
            //GooglePlusAccountRepository gpaccrepo = new GooglePlusAccountRepository();
            //Domain.Socioboard.Domain.GooglePlusAccount gpacc = null;
            //try
            //{
            //    gpacc = gpaccrepo.getUserDetailsByUserName(keyword);

            //}
            //catch (Exception ee) { }
            //if (gpacc != null)
            //{
            //    return GooglePlusgetUser(gpacc.GpUserId);
            //}
            //else
            {
                string ResultPage = string.Empty;
                string FirstResultPage = string.Empty;
                //bool Isfirst = true;
                try
                {
                    JObject PageList = JObject.Parse(GooglePlusList(keyword));
                    foreach (JObject item in PageList["items"])
                    {
                        if (item["objectType"].ToString().Equals("page"))
                        {
                            try
                            {
                                FirstResultPage = GooglePlusgetUser(item["id"].ToString());
                                JObject jobjrpage = JObject.Parse(FirstResultPage);
                                if (jobjrpage["verified"].ToString().Equals("True"))
                                {
                                    ResultPage = GooglePlusgetUser(item["id"].ToString());
                                    break;
                                }
                            }
                            catch (Exception e) { }

                            //TODO: write exact page refine logic
                        }
                    }
                    //JObject jobjresult = JObject.Parse(ResultPage);
                    //if (gpacc == null)
                    //{
                    //    try
                    //    {
                    //        gpacc = gpaccrepo.getUserDetailsByUserName(jobjresult["displayName"].ToString());

                    //    }
                    //    catch (Exception eeee) { }
                    //}
                    //if (gpacc == null)
                    //{
                    //    Domain.Socioboard.Domain.GooglePlusAccount newgplusacc = new Domain.Socioboard.Domain.GooglePlusAccount();
                    //    newgplusacc.GpUserId = jobjresult["id"].ToString();
                    //    newgplusacc.GpUserName = jobjresult["displayName"].ToString();
                    //    newgplusacc.EntryDate = DateTime.Now;
                    //    gpaccrepo.addGooglePlusUser(newgplusacc);
                    //}
                }
                catch (Exception e) { }

                return ResultPage;
            }

        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GooglePlusList(string keyword)
        {
            string ret = string.Empty;
            try
            {
                string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
                //AIzaSyCvTBnEDnr5DEpvlVDCuxz9K9TK84rX0fE
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                string RequestUrl = "https://www.googleapis.com/plus/v1/people?query=" + keyword + "&key=" + Key;

                var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                facebooklistpagerequest.Method = "GET";
                string response = string.Empty;
                try
                {
                    using (var youtuberesponse = facebooklistpagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();

                        }
                    }
                }
                catch (Exception e) { }

                return response;
            }
            catch (Exception ex)
            {

                throw;
            }

            //return ret;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GooglePlusgetUser(string UserId)
        {
            string ret = string.Empty;
            string response = string.Empty;

            try
            {
                string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
                //AIzaSyCvTBnEDnr5DEpvlVDCuxz9K9TK84rX0fE
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                string RequestUrl = "https://www.googleapis.com/plus/v1/people/" + UserId + "?key=" + Key;

                var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                facebooklistpagerequest.Method = "GET";
                try
                {
                    using (var youtuberesponse = facebooklistpagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(youtuberesponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();

                        }
                    }
                }
                catch (Exception e) { }

            }
            catch (Exception ex)
            {
            }
            return response;

            //return ret;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GooglePlusgetUserRecentActivities(string UserId)
        {
            string ret = string.Empty;
            string response = string.Empty;

            try
            {
                string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
                //AIzaSyCvTBnEDnr5DEpvlVDCuxz9K9TK84rX0fE
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                string RequestUrl = "https://www.googleapis.com/plus/v1/people/" + UserId + "/activities/public/?key=" + Key;

                var gpluspagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                gpluspagerequest.Method = "GET";
                try
                {
                    using (var gplusresponse = gpluspagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(gplusresponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();

                        }
                    }
                }
                catch (Exception e) { }

            }
            catch (Exception ex)
            {
            }
            return response;


            //return ret;
        }



        public string GooglePlusgetUserRecentActivitiesByHashtag(string Hashtag)
        {
            string ret = string.Empty;
            string response = string.Empty;
            Hashtag = Uri.EscapeUriString(Hashtag);
            Hashtag = Hashtag.Replace("%E2%80%AA%E2%80%8E", string.Empty);
            try
            {
                string Key = "AIzaSyCISaVFe_TJknn92J7xw2diFEi6Z_aroYE";
                //AIzaSyCvTBnEDnr5DEpvlVDCuxz9K9TK84rX0fE
                //string RequestUrl = "https://www.googleapis.com/youtube/v3/search?part=" + part + "&maxResults=" + maxResults + "&q=" + keyword + "&key=" + accesstoken;
                //string RequestUrl = "https://www.googleapis.com/plus/v1/" + Hashtag + "/activities/public/?key=" + Key + "&maxResults=99";
                string RequestUrl = " https://www.googleapis.com/plus/v1/activities?orderBy=recent&query=%23" + Hashtag + "&alt=json&key=" + Key;

                var gpluspagerequest = (HttpWebRequest)WebRequest.Create(RequestUrl);
                gpluspagerequest.Method = "GET";
                try
                {
                    using (var gplusresponse = gpluspagerequest.GetResponse())
                    {
                        using (var stream = new StreamReader(gplusresponse.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            response = stream.ReadToEnd();
                        }
                    }
                }
                catch (Exception e) { }

            }
            catch (Exception ex)
            {
            }
            return response;


            //return ret;
        }
        #endregion

        #region facebook login

        public void LoginUsingGlobusHttp(ref FacebookUser facebookUser)
        {
            ///Sign In
            try
            {
                GlobusHttpHelper objGlobusHttpHelper = facebookUser.globusHttpHelper;
                #region Post variable

                string fbpage_id = string.Empty;
                string fb_dtsg = string.Empty;
                string __user = string.Empty;
                string xhpc_composerid = string.Empty;
                string xhpc_targetid = string.Empty;
                string xhpc_composerid12 = string.Empty;
                #endregion

                int intProxyPort = 80;

                if (!string.IsNullOrEmpty(facebookUser.proxyport) && Utils.IdCheck.IsMatch(facebookUser.proxyport))
                {
                    intProxyPort = int.Parse(facebookUser.proxyport);
                }
                Console.WriteLine("Logging in with " + facebookUser.username);
                Console.WriteLine("Logging in with " + facebookUser.username);
                string pageSource = string.Empty;
                try
                {
                    pageSource = objGlobusHttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com/login.php"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error : " + ex.StackTrace);
                }

                if (pageSource == null || string.IsNullOrEmpty(pageSource))
                {
                    pageSource = objGlobusHttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com/login.php"));
                }

                if (pageSource == null)
                {
                    pageSource = objGlobusHttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com/login.php"));
                    return;
                }

                string valueLSD = GlobusHttpHelper.GetParamValue(pageSource, "lsd");
                string ResponseLogin = string.Empty;
                try
                {
                    ResponseLogin = objGlobusHttpHelper.postFormData(new Uri("https://www.facebook.com/login.php?login_attempt=1"), "charset_test=%E2%82%AC%2C%C2%B4%2C%E2%82%AC%2C%C2%B4%2C%E6%B0%B4%2C%D0%94%2C%D0%84&lsd=" + valueLSD + "&locale=en_US&email=" + facebookUser.username.Split('@')[0].Replace("+", "%2B") + "%40" + facebookUser.username.Split('@')[1] + "&pass=" + Uri.EscapeDataString(facebookUser.password) + "&persistent=1&default_persistent=1&charset_test=%E2%82%AC%2C%C2%B4%2C%E2%82%AC%2C%C2%B4%2C%E6%B0%B4%2C%D0%94%2C%D0%84&lsd=" + valueLSD + "");           //"https://www.facebook.com/login.php?login_attempt=1"
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error : " + ex.StackTrace);
                }
                if (string.IsNullOrEmpty(ResponseLogin))
                {
                    ResponseLogin = objGlobusHttpHelper.postFormData(new Uri("https://www.facebook.com/login.php?login_attempt=1"), "charset_test=%E2%82%AC%2C%C2%B4%2C%E2%82%AC%2C%C2%B4%2C%E6%B0%B4%2C%D0%94%2C%D0%84&lsd=" + valueLSD + "&locale=en_US&email=" + facebookUser.username.Split('@')[0].Replace("+", "%2B") + "%40" + facebookUser.username.Split('@')[1] + "&pass=" + Uri.EscapeDataString(facebookUser.password) + "&persistent=1&default_persistent=1&charset_test=%E2%82%AC%2C%C2%B4%2C%E2%82%AC%2C%C2%B4%2C%E6%B0%B4%2C%D0%94%2C%D0%84&lsd=" + valueLSD + "");           //"https://www.facebook.com/login.php?login_attempt=1"
                }
                if (ResponseLogin == null)
                {
                    return;
                }

                string loginStatus = "";
                if (CheckLogin(ResponseLogin, facebookUser.username, facebookUser.password, facebookUser.proxyip, facebookUser.proxyport, facebookUser.proxyusername, facebookUser.proxypassword, ref loginStatus))
                {
                    Console.WriteLine("Logged in with Username : " + facebookUser.username);
                    Console.WriteLine("Logged in with Username : " + facebookUser.username);

                    facebookUser.isloggedin = true;

                }
                else
                {
                    Console.WriteLine("Couldn't login with Username : " + facebookUser.username);
                    facebookUser.isloggedin = false;


                    if (loginStatus == "account has been disabled")
                    {
                        // GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_DisabledAccount);
                    }

                    if (loginStatus == "Please complete a security check")
                    {
                        //GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_SecurityCheckAccounts);
                    }


                    if (loginStatus == "Your account is temporarily locked")
                    {
                        //GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_TemporarilyLockedAccount);

                    }
                    if (loginStatus == "have been blocked")
                    {
                        //GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_havebeenblocked);

                    }
                    if (loginStatus == "For security reasons your account is temporarily locked")
                    {
                        // GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_SecurityCheckAccountsforsecurityreason);
                    }

                    if (loginStatus == "Account Not Confirmed")
                    {
                        //GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_AccountNotConfirmed);
                    }
                    if (loginStatus == "Temporarily Blocked for 30 Days")
                    {
                        // GlobusFileHelper.AppendStringToTextfileNewLine(Username + ":" + Password + ":" + proxyAddress + ":" + proxyPort + ":" + proxyUsername + ":" + proxyPassword, Globals.path_30daysBlockedAccount);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error : " + ex.StackTrace);

            }
        }

        public bool CheckLogin(string response, string username, string password, string proxyAddress, string proxyPort, string proxyUsername, string proxyPassword, ref string loginStatus)
        {

            try
            {
                if (!string.IsNullOrEmpty(response))
                {


                    if (response.ToLower().Contains("unusual login activity"))
                    {
                        loginStatus = "unusual login activity";
                        Console.WriteLine("Unusual Login Activity: " + username);
                        return false;
                    }
                    if (response.ToLower().Contains("incorrect username"))
                    {
                        loginStatus = "incorrect username";
                        Console.WriteLine("Incorrect username: " + username);
                        return false;
                    }
                    if (response.ToLower().Contains("Choose a verification method".ToLower()))
                    {
                        loginStatus = "Choose a verification method";
                        Console.WriteLine("Choose a verification method: " + username);
                        return false;
                    }
                    if (response.ToLower().Contains("not logged in".ToLower()))
                    {
                        loginStatus = "not logged in";
                        Console.WriteLine("not logged in: " + username);
                        return false;
                    }
                    if (response.Contains("Please log in to continue".ToLower()))
                    {
                        loginStatus = "Please log in to continue";
                        Console.WriteLine("Please log in to continue: " + username);
                        return false;
                    }
                    if (response.Contains("re-enter your password"))
                    {
                        loginStatus = "re-enter your password";
                        Console.WriteLine("Wrong password for: " + username);
                        return false;
                    }
                    if (response.Contains("Incorrect Email"))
                    {
                        loginStatus = "Incorrect Email";
                        Console.WriteLine("Incorrect email: " + username);

                        try
                        {
                            ///Write Incorrect Emails in text file
                            //GlobusFileHelper.AppendStringToTextfileNewLine(username + ":" + password, incorrectEmailFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                        return false;
                    }
                    if (response.Contains("have been blocked"))
                    {
                        loginStatus = "have been blocked";
                        Console.WriteLine("you have been blocked: " + username);
                        return false;
                    }
                    if (response.Contains("account has been disabled"))
                    {
                        loginStatus = "account has been disabled";
                        Console.WriteLine("your account has been disabled: " + username);
                        return false;
                    }
                    if (response.Contains("Please complete a security check"))
                    {
                        loginStatus = "Please complete a security check";
                        Console.WriteLine("Please complete a security check: " + username);
                        return false;
                    }
                    if (response.Contains("Please complete a security check"))
                    {
                        loginStatus = "Please complete a security check";
                        Console.WriteLine("You must log in to see this page: " + username);
                        return false;
                    }
                    if (response.Contains("<input value=\"Sign Up\" onclick=\"RegistrationBootloader.bootloadAndValidate();"))
                    {
                        loginStatus = "RegistrationBootloader.bootloadAndValidate()";
                        Console.WriteLine("Not logged in with: " + username);
                        return false;
                    }
                    if (response.Contains("Account Not Confirmed"))
                    {
                        loginStatus = "Account Not Confirmed";
                        Console.WriteLine("Account Not Confirmed " + username);
                        return false;
                    }
                    if (response.Contains("Your account is temporarily locked"))
                    {
                        loginStatus = "Your account is temporarily locked";
                        Console.WriteLine("Your account is temporarily locked: " + username);
                        return false;
                    }
                    if (response.Contains("Your account has been temporarily suspended"))
                    {
                        loginStatus = "Your account has been temporarily suspended";
                        Console.WriteLine("Your account has been temporarily suspended: " + username);
                        return false;
                    }
                    if (response.Contains("You must log in to see this page"))
                    {
                        Console.WriteLine("You must log in to see this page: " + username);
                        return false;
                    }
                    if (response.ToLower().Contains("you must log in to see this page"))
                    {
                        Console.WriteLine("You must log in to see this page: " + username);
                        return false;
                    }
                    if (response.ToLower().Contains("you entered an old password"))
                    {
                        loginStatus = "you entered an old password";
                        Console.WriteLine("You Entered An Old Password: " + username);
                        return false;
                    }
                    if (response.Contains("For security reasons your account is temporarily locked"))
                    {
                        loginStatus = "For security reasons your account is temporarily locked";
                        Console.WriteLine("For security reasons your account is temporarily locked: " + username);
                        return false;
                    }
                    if (response.Contains("Please Verify Your Identity") || response.Contains("please Verify Your Identity"))
                    {
                        loginStatus = "Please Verify Your Identity";
                        Console.WriteLine("Please Verify Your Identity: " + username);
                        return false;
                    }
                    if (response.Contains("Temporarily Blocked for 30 Days"))
                    {
                        loginStatus = "Temporarily Blocked for 30 Days";
                        Console.WriteLine("You're Temporarily Blocked for 30 Days: " + username);
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.StackTrace);
            }
            return true;
        }

        #endregion
        #region facebook hashtag scraper

        public void Scraper(ref FacebookUser fbUser, string Hash)
        {


            GlobusHttpHelper HttpHelper = fbUser.globusHttpHelper;
            string pageSource_Home = HttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com"));


            string __user = string.Empty;
            string fb_dtsg = string.Empty;
            string KeyWord = string.Empty;

            __user = GlobusHttpHelper.GetParamValue(pageSource_Home, "user");
            if (string.IsNullOrEmpty(__user))
            {
                __user = GlobusHttpHelper.ParseJson(pageSource_Home, "user");
            }


            fb_dtsg = GlobusHttpHelper.Get_fb_dtsg(pageSource_Home);



            string PostData = "__user=" + __user + "&__a=1&__dyn=7nmanEyl2lm9o-t2u5bGya4Au74qbx2mbAKGiyEyut9LRwxBxem9V8CdwIhEyfyUnwPUS2O4K5e8GQ8GqcFoy8ACxtpm&__req=h&fb_dtsg=" + fb_dtsg + "&ttstamp=26581701181109510510483495368&__rev=1719057";
            string PostUrl = "https://www.facebook.com/pubcontent/trending/see_more/?topic_ids[0]=105471716153186&topic_ids[1]=109659645720566&topic_ids[2]=108332329190557&position=3";
            string Pageresponce = HttpHelper.postFormData(new Uri(PostUrl), PostData);
            Pageresponce = Pageresponce.Replace("\\u003C", "<");


            string[] trendingArr = System.Text.RegularExpressions.Regex.Split(Pageresponce, "li data-topicid=");
            trendingArr = trendingArr.Skip(1).ToArray();
            foreach (var item_trendingArr in trendingArr)
            {
                string trendingId = string.Empty;
                string trendingName = string.Empty;
                string trendingLinkUrl = string.Empty;
                try
                {
                    try
                    {
                        trendingId = Utils.getBetween(item_trendingArr, "\\\"", "\\\"");
                    }
                    catch { };
                    try
                    {
                        trendingName = Utils.getBetween(item_trendingArr, "_5v0s _5my8\\\">", "<");
                    }
                    catch { };

                    string LinkUrl = Utils.getBetween(item_trendingArr, "_4qzh _5v0t _7ge\\\" href=\\\"", "\" id=").Replace("\\", string.Empty).Replace("amp;", string.Empty);
                    if (!LinkUrl.Contains("www.facebook.com/"))
                    {
                        trendingLinkUrl = "https://www.facebook.com" + LinkUrl;
                    }
                    else
                    {
                        trendingLinkUrl = LinkUrl;
                    }

                    if (trendingName.Contains(KeyWord))
                    {
                        string FindPageSource = string.Empty;


                        FindPageSource = HttpHelper.getHtmlfromUrl(new Uri(trendingLinkUrl));

                        string postID = string.Empty;
                        string postName = string.Empty;
                        string postImage = string.Empty;
                        string postDescriptions = string.Empty;
                        string posttitle = string.Empty;

                        string[] ContentArr = System.Text.RegularExpressions.Regex.Split(FindPageSource, "userContentWrapper _5pcr _3ccb");
                        ContentArr = ContentArr.Skip(1).ToArray();
                        foreach (var ContentArr_item in ContentArr)
                        {
                            try
                            {
                                postName = trendingName;
                                postID = trendingId;
                                // class="_4-eo _2t9n"
                                postImage = Utils.getBetween(ContentArr_item, "scaledImageFitWidth img\" src=\"", "alt=").Replace("amp;", string.Empty).Replace("\"", string.Empty);

                            }
                            catch (Exception)
                            {

                            }
                            string profileName = posttitle;//2
                            string Message = Utils.getBetween(ContentArr_item, "<p>", "<").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%").Replace("&#039;", "'"); ;//7
                            string[] DetailedInfo = System.Text.RegularExpressions.Regex.Split(ContentArr_item, "<div class=\"_6m7\">");
                            string detail = "-" + DetailedInfo[1];//8
                            detail = Utils.getBetween(detail, "-", "</div>").Replace("&amp;", "&").Replace("u0025", "%").Replace("&#039;", "'");

                            if (detail.Contains("<a "))
                            {
                                string GetVisitUrl = Utils.getBetween(detail, "\">", "</a>");

                                detail = Utils.getBetween("$$$####" + detail, "$$$####", "<a href=") + "-" + GetVisitUrl;

                            }

                            string[] ArrDetail = System.Text.RegularExpressions.Regex.Split(ContentArr_item, "<div class=\"mbs _6m6\">");

                            string Titles = Utils.getBetween(ArrDetail[1], ">", "</a>").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%").Replace("&#039;", "'");
                            string SiteRedirectionUrl = Utils.getBetween(ArrDetail[1], "LinkshimAsyncLink.swap(this, &quot;", ");");
                            SiteRedirectionUrl = Uri.UnescapeDataString(SiteRedirectionUrl).Replace("\\u0025", "%").Replace("\\", "");//4
                            string websiteUrl = Utils.getBetween(SiteRedirectionUrl, "//", "/");

                            string[] adImg = System.Text.RegularExpressions.Regex.Split(ContentArr_item, "<img class=\"scaledImageFitWidth img\"");
                            string redirectionImg = Utils.getBetween(adImg[1], "src=\"", "\"").Replace("&amp;", "&");

                            string[] profImg = System.Text.RegularExpressions.Regex.Split(ContentArr_item, "<img class=\"_s0 5xib 5sq7 _rw img\"");
                            string profileImg = Utils.getBetween(profImg[0], "src=\"", "\"").Replace("&amp;", "&");

                        }

                    }

                }
                catch { };

            }


        }

        //public void ScraperHasTage(ref FacebookUser fbUser, string Hash, Guid BoardfbPageId)
        //{


        //    GlobusHttpHelper HttpHelper = fbUser.globusHttpHelper;
        //    string KeyWord = Hash;
        //    string pageSource_Home = HttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com/hashtag/" + KeyWord));
        //    List<string> pageSouceSplit = new List<string>();
        //    string[] trendingArr = System.Text.RegularExpressions.Regex.Split(pageSource_Home, "li data-topicid=");
        //    string[] PagesLink = System.Text.RegularExpressions.Regex.Split(pageSource_Home, "uiLikePageButton");
        //    foreach (var item in PagesLink)
        //    {
        //        pageSouceSplit.Add(item);
        //    }
        //    PagesLink = PagesLink.Skip(1).ToArray();

        //    foreach (var item_pageSouceSplit in pageSouceSplit)
        //    {
        //        try
        //        {
        //            if (item_pageSouceSplit.Contains("<!DOCTYPE html>"))
        //            {
        //                continue;
        //            }
        //            Dictionary<string, string> listContent = ScrapHasTagPages(item_pageSouceSplit);
        //            Domain.Socioboard.Domain.Boardfbfeeds fbfeed = new Domain.Socioboard.Domain.Boardfbfeeds();
        //            fbfeed.Id = Guid.NewGuid();
        //            fbfeed.Isvisible = true;
        //            fbfeed.Message = listContent["Message"];
        //            fbfeed.Image = listContent["PostImage"];
        //            fbfeed.Description = listContent["Title"];
        //            string[] splitdate = System.Text.RegularExpressions.Regex.Split(listContent["Time"], "at");
        //            string d = splitdate[0].Trim();
        //            string t = splitdate[1].Trim();
        //            fbfeed.Createddate = Convert.ToDateTime(d + " " + t);
        //            fbfeed.Feedid = listContent["PostId"];
        //            fbfeed.Type = listContent["Type"];
        //            fbfeed.Type = listContent["Link"];
        //            fbfeed.Fbpageprofileid = BoardfbPageId;
        //             if (!boardrepo.checkFacebookFeedExists(fbfeed.Feedid, BoardfbPageId))
        //             {
        //                 boardrepo.addBoardFbPageFeed(fbfeed);
        //             }

        //            // Please Write Code get Dictionary data 

        //            //


        //        }
        //        catch { };



        //    }


        //    try
        //    {


        //        string ajaxpipe_token = Utils.getBetween(pageSource_Home, "\"ajaxpipe_token\":\"", "\"");
        //        string[] data_c = System.Text.RegularExpressions.Regex.Split(pageSource_Home, "data-cursor=");
        //        string cursor = Utils.getBetween(data_c[4], "\"", "=");
        //        if (cursor.Contains("data-dedupekey"))
        //        {
        //            cursor = "-" + cursor;
        //            cursor = Utils.getBetween(cursor, "-", "\"");
        //        }
        //        string sectionid = Utils.getBetween(pageSource_Home, "section_id\\\":", ",");
        //        string userid = Utils.getBetween(pageSource_Home, "USER_ID\":\"", "\"");
        //        string feed_Id = "90842368";
        //        string pager_id = "u_ps_0_0_1n";
        //        for (int i = 2; i < 50; i++)
        //        {
        //            try
        //            {
        //                Thread.Sleep(30 * 1000);

        //                List<string> pageSouceSplitPagination = new List<string>();
        //                if (string.IsNullOrEmpty(fbUser.username))
        //                {
        //                    break;
        //                }
        //                //GlobusLogHelper.log.Info("Please wait... Searching for data from Page :" + i + "   with User Name : " + fbUser.username);


        //                string req = "https://www.facebook.com/ajax/pagelet/generic.php/LitestandMoreStoriesPagelet?ajaxpipe=1&ajaxpipe_token=" + ajaxpipe_token + "&no_script_path=1&data=%7B%22cursor%22%3A%22" + cursor + "%22%2C%22preload_next_cursor%22%3Anull%2C%22pager_config%22%3A%22%7B%5C%22edge%5C%22%3Anull%2C%5C%22source_id%5C%22%3Anull%2C%5C%22section_id%5C%22%3A" + sectionid + "%2C%5C%22pause_at%5C%22%3Anull%2C%5C%22stream_id%5C%22%3Anull%2C%5C%22section_type%5C%22%3A1%2C%5C%22sizes%5C%22%3Anull%2C%5C%22most_recent%5C%22%3Afalse%2C%5C%22unread_session%5C%22%3Afalse%2C%5C%22continue_top_news_feed%5C%22%3Afalse%2C%5C%22ranking_model%5C%22%3Anull%2C%5C%22unread_only%5C%22%3Afalse%7D%22%2C%22pager_id%22%3A%22" + pager_id + "%22%2C%22scroll_count%22%3A1%2C%22start_unread_session%22%3Afalse%2C%22start_continue_top_news_feed%22%3Afalse%2C%22feed_stream_id%22%3A" + feed_Id + "%2C%22snapshot_time%22%3Anull%7D&__user=" + userid + "&__a=1&__dyn=7nm8RW8BgCBynzpQ9UoHaEWCueyrhEK49oKiWFaaBGeqrYw8popyujhElx2ubhHximmey8szoyfwgo&__req=jsonp_2&__rev=1583304&__adt=" + i + "";      //
        //                string respReq = HttpHelper.getHtmlfromUrl(new Uri(req));

        //                respReq = respReq.Replace("\\", "").Replace("u003C", "<");
        //                string[] arrrespReq = System.Text.RegularExpressions.Regex.Split(respReq, "source_id");
        //                feed_Id = Utils.getBetween(respReq, "feed_stream_id", "snapshot_time");
        //                feed_Id = Utils.getBetween(feed_Id, "A", "u");

        //                string[] pager_id1 = System.Text.RegularExpressions.Regex.Split(respReq, "_4-u2 mbl ");
        //                pager_id = Utils.getBetween(pager_id1[2], "id=\"", "\"");
        //                data_c = System.Text.RegularExpressions.Regex.Split(respReq, "data-cursor=");
        //                if (data_c.Length < 8)
        //                {
        //                    cursor = Utils.getBetween(data_c[data_c.Length - 1], "\"", "=");
        //                }
        //                cursor = Utils.getBetween(data_c[8], "\"", "=");
        //                if (cursor.Contains("data-dedupekey"))
        //                {
        //                    cursor = "-" + cursor;
        //                    cursor = Utils.getBetween(cursor, "-", "\"");
        //                }

        //                string[] PagesLinkPagination = System.Text.RegularExpressions.Regex.Split(respReq, "<span>Suggested Post</span>");

        //                foreach (var item in PagesLinkPagination)
        //                {
        //                    pageSouceSplitPagination.Add(item);
        //                }

        //                PagesLink = System.Text.RegularExpressions.Regex.Split(respReq, "uiLikePageButton");
        //                foreach (var item in PagesLink)
        //                {
        //                    pageSouceSplitPagination.Add(item);
        //                }


        //                foreach (var item_pageSouceSplit in pageSouceSplit)
        //                {
        //                    try
        //                    {
        //                        if (item_pageSouceSplit.Contains("<!DOCTYPE html>"))
        //                        {
        //                            continue;
        //                        }
        //                        Dictionary<string, string> listContent = ScrapHasTagPages(item_pageSouceSplit);
        //                        Domain.Socioboard.Domain.Boardfbfeeds fbfeed = new Domain.Socioboard.Domain.Boardfbfeeds();
        //                        fbfeed.Id = Guid.NewGuid();
        //                        fbfeed.Isvisible = true;
        //                        fbfeed.Message = listContent["Message"];
        //                        fbfeed.Image = listContent["PostImage"];
        //                        fbfeed.Description = listContent["Title"];
        //                        string[] splitdate = System.Text.RegularExpressions.Regex.Split(listContent["Time"], "at");
        //                        string d = splitdate[0].Trim();
        //                        string t = splitdate[1].Trim();
        //                        fbfeed.Createddate = Convert.ToDateTime(d + " " + t);
        //                        fbfeed.Feedid = listContent["PostId"];
        //                        fbfeed.Type = listContent["Type"];
        //                        fbfeed.Type = listContent["Link"];
        //                        fbfeed.Fbpageprofileid = BoardfbPageId;
        //                        if (!boardrepo.checkFacebookFeedExists(fbfeed.Feedid, BoardfbPageId))
        //                        {
        //                            boardrepo.addBoardFbPageFeed(fbfeed);
        //                        }
        //                        // Please Write Code get Dictionary data 

        //                        //

        //                    }
        //                    catch { };



        //                }

        //            }
        //            catch (Exception ex)
        //            { //GlobusLogHelper.log.Error(ex.StackTrace); 
        //            }
        //        }
        //    }
        //    catch
        //    { }





        //}



        //public Dictionary<string, string> ScrapHasTagPages(string Value)
        //{
        //    string redirectionHref = string.Empty;
        //    string title = string.Empty;
        //    List<string[]> Likedata = new List<string[]>();
        //    Dictionary<string, string> HasTagData = new Dictionary<string, string>();
        //    //  foreach (var Value in Likepages)
        //    {
        //        try
        //        {

        //            redirectionHref = Utils.getBetween(Value, "href=\"", "\"");
        //            string profileUrl = redirectionHref;//1
        //            if (redirectionHref.Contains("https://www.facebook.com"))
        //            {

        //                string[] Arr_Title = System.Text.RegularExpressions.Regex.Split(Value, "<span class=\"fwb fcg\"");
        //                foreach (var valuetitle in Arr_Title)
        //                {
        //                    try
        //                    {


        //                        // title = Utils.getBetween(valuetitle, "<a", "/a>");
        //                        title = Utils.getBetween(valuetitle, "\">", "/a>");
        //                        if (!title.Equals(string.Empty))
        //                        {
        //                            title = Utils.getBetween(title, "\">", "<");
        //                            if (!string.IsNullOrEmpty(title))
        //                            {
        //                                break;
        //                            }

        //                        }
        //                    }
        //                    catch (Exception)
        //                    {

        //                    }
        //                }
        //                string profileName = title;//2
        //                string Message = Utils.getBetween(Value, "<p>", "<").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%");//7

        //                string[] timeDetails = Regex.Split(Value, "<abbr");
        //                string postedTime = string.Empty;
        //                try
        //                {
        //                    postedTime = Utils.getBetween(timeDetails[1], "=\"", "\"");
        //                }
        //                catch { };
        //                string postid = string.Empty;

        //                try
        //                {
        //                    postid = Utils.getBetween(timeDetails[0], "fbid=", "&amp;");
        //                    if (postid == "")
        //                    {
        //                        postid = Utils.getBetween(timeDetails[0], "/posts/", "\" target=");
        //                    }
        //                }
        //                catch
        //                {
        //                }

        //                string[] DetailedInfo = System.Text.RegularExpressions.Regex.Split(Value, "<div class=\"_6m7\">");
        //                string detail = string.Empty;
        //                try
        //                {
        //                    detail = "-" + DetailedInfo[1];//8
        //                    detail = Utils.getBetween(detail, "-", "</div>").Replace("&amp;", "&").Replace("u0025", "%");
        //                    if (detail.Contains("<a "))
        //                    {
        //                        string GetVisitUrl = Utils.getBetween(detail, "\">", "</a>");
        //                        detail = Utils.getBetween("$$$####" + detail, "$$$####", "<a href=") + "-" + GetVisitUrl;
        //                    }
        //                }
        //                catch
        //                { };

        //                string[] ArrDetail = System.Text.RegularExpressions.Regex.Split(Value, "<div class=\"mbs _6m6\">");
        //                string Titles = string.Empty;
        //                // string Url = Utils.getBetween(ArrDetail[0], "", "");
        //                try
        //                {
        //                    Titles = Utils.getBetween(ArrDetail[1], ">", "</a>").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%");//6
        //                    if (Titles.Contains("Sachin Tendulkar"))
        //                    {

        //                    }
        //                }
        //                catch { };
        //                string SiteRedirectionUrl = string.Empty;
        //                try
        //                {
        //                    SiteRedirectionUrl = Utils.getBetween(ArrDetail[1], "LinkshimAsyncLink.swap(this, &quot;", ");");
        //                }
        //                catch { };
        //                try
        //                {
        //                    SiteRedirectionUrl = Uri.UnescapeDataString(SiteRedirectionUrl).Replace("\\u0025", "%").Replace("\\", "");//4
        //                }
        //                catch { };
        //                string websiteUrl = string.Empty;
        //                try
        //                {
        //                    websiteUrl = Utils.getBetween(SiteRedirectionUrl, "//", "/");
        //                }
        //                catch { };
        //                string redirectionImg = string.Empty;
        //                try
        //                {
        //                    string[] adImg = System.Text.RegularExpressions.Regex.Split(Value, "<img class=\"scaledImageFitWidth img\"");
        //                       redirectionImg = Utils.getBetween(adImg[1], "src=\"", "\"").Replace("&amp;", "&");
        //                }
        //                catch { };


        //                string[] profImg = System.Text.RegularExpressions.Regex.Split(Value, "<img class=\"_s0 5xib 5sq7 _rw img\"");
        //                string profileImg = string.Empty;
        //                try
        //                {
        //                    profileImg = Utils.getBetween(profImg[0], "src=\"", "\"").Replace("&amp;", "&");
        //                }
        //                catch { };
        //                HasTagData.Add("Title", title);
        //                HasTagData.Add("Time", postedTime);
        //                HasTagData.Add("Type", "link");
        //                HasTagData.Add("Message", Message);
        //                HasTagData.Add("Image", profileImg);
        //                HasTagData.Add("PostImage", redirectionImg);
        //                HasTagData.Add("PostId", postid);
        //                HasTagData.Add("Link", SiteRedirectionUrl);
        //            }
        //        }
        //        catch { };
        //    }

        //    return HasTagData;
        //}

        public void ScraperHasTage(string Hash, Guid BoardfbPageId)
        {


            GlobusHttpHelper HttpHelper = new GlobusHttpHelper();
            string KeyWord = Hash;
            string pageSource_Home = HttpHelper.getHtmlfromUrl(new Uri("https://www.facebook.com/hashtag/" + KeyWord));
            List<string> pageSouceSplit = new List<string>();
            string[] PagesLink = System.Text.RegularExpressions.Regex.Split(pageSource_Home, "_4-u2 mbm _5jmm _5pat _5v3q _4-u8");
            foreach (var item in PagesLink)
            {
                pageSouceSplit.Add(item);
            }
            PagesLink = PagesLink.Skip(1).ToArray();

            foreach (var item_pageSouceSplit in pageSouceSplit)
            {
                try
                {
                    if (item_pageSouceSplit.Contains("<!DOCTYPE html>"))
                    {
                        continue;
                    }
                    Dictionary<string, string> listContent = ScrapHasTagPages(item_pageSouceSplit);
                    Domain.Socioboard.Domain.Boardfbfeeds fbfeed = new Domain.Socioboard.Domain.Boardfbfeeds();
                    fbfeed.Id = Guid.NewGuid();
                    fbfeed.Isvisible = true;
                    fbfeed.Message = listContent["Message"];
                    fbfeed.Image = listContent["PostImage"];
                    fbfeed.Description = listContent["Title"];
                    string[] splitdate = System.Text.RegularExpressions.Regex.Split(listContent["Time"], "at");
                    string d = splitdate[0].Trim();
                    string t = splitdate[1].Trim();
                    fbfeed.Createddate = Convert.ToDateTime(d + " " + t);
                    fbfeed.Feedid = listContent["PostId"];
                    fbfeed.Type = listContent["Type"];
                    fbfeed.Link = listContent["Link"];
                    fbfeed.FromId = listContent["FromId"];
                    fbfeed.FromName = listContent["FromName"];
                    fbfeed.Fbpageprofileid = BoardfbPageId;
                    if (!boardrepo.checkFacebookFeedExists(fbfeed.Feedid, BoardfbPageId))
                    {
                        boardrepo.addBoardFbPageFeed(fbfeed);
                    }

                    // Please Write Code get Dictionary data 

                    //


                }
                catch { };



            }

        }
        public Dictionary<string, string> ScrapHasTagPages(string Value)
        {
            string redirectionHref = string.Empty;
            string title = string.Empty;
            string profileid = string.Empty;
            List<string[]> Likedata = new List<string[]>();
            Dictionary<string, string> HasTagData = new Dictionary<string, string>();
            //  foreach (var Value in Likepages)
            {
                try
                {
                    profileid = Utils.getBetween(Value, "entity_id&quot;:&quot;", "&quot;,&quot;entity_path").Trim();
                    if (profileid == "")
                    {
                        profileid = Utils.getBetween(Value, "target_profile_id&quot;:&quot;", "&quot;,&quot;type_id").Trim();
                    }
                    redirectionHref = Utils.getBetween(Value, "href=\"", "\"");
                    string profileUrl = redirectionHref;//1
                    if (redirectionHref.Contains("https://www.facebook.com"))
                    {

                        string[] Arr_Title = System.Text.RegularExpressions.Regex.Split(Value, "<span class=\"fwb fcg\"");
                        foreach (var valuetitle in Arr_Title)
                        {
                            try
                            {


                                // title = Utils.getBetween(valuetitle, "<a", "/a>");
                                title = Utils.getBetween(valuetitle, "\">", "/a>");
                                if (!title.Equals(string.Empty))
                                {
                                    title = Utils.getBetween(title, "\">", "<");
                                    if (!string.IsNullOrEmpty(title))
                                    {
                                        break;
                                    }

                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        try
                        {
                            if (title.Trim() == "")
                            {

                                title = Utils.getBetween(Arr_Title[0], "data-ft=\"&#123;&quot;tn&quot;:&quot;k&quot;&#125;\">", "</a></span>");

                            }
                        }
                        catch { }
                        string profileName = title;//2
                        string Message = Utils.getBetween(Value, "<p>", "<").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%");//7

                        string[] timeDetails = Regex.Split(Value, "<abbr");
                        string postedTime = string.Empty;
                        try
                        {
                            postedTime = Utils.getBetween(timeDetails[1], "=\"", "\"");
                        }
                        catch { };


                        string[] DetailedInfo = System.Text.RegularExpressions.Regex.Split(Value, "<div class=\"_6m7\">");
                        string detail = string.Empty;
                        try
                        {
                            detail = "-" + DetailedInfo[1];//8
                            detail = Utils.getBetween(detail, "-", "</div>").Replace("&amp;", "&").Replace("u0025", "%");

                            if (detail.Contains("<a "))
                            {
                                string GetVisitUrl = Utils.getBetween(detail, "\">", "</a>");

                                detail = Utils.getBetween("$$$####" + detail, "$$$####", "<a href=") + "-" + GetVisitUrl;

                            }

                        }
                        catch
                        { };

                        string[] ArrDetail = System.Text.RegularExpressions.Regex.Split(Value, "<div class=\"mbs _6m6\">");
                        string Titles = string.Empty;
                        // string Url = Utils.getBetween(ArrDetail[0], "", "");
                        try
                        {
                            Titles = Utils.getBetween(ArrDetail[1], ">", "</a>").Replace("&#064;", "@").Replace("&amp;", "&").Replace("u0025", "%");//6
                            if (Titles.Contains("Sachin Tendulkar"))
                            {

                            }
                        }
                        catch { };
                        string SiteRedirectionUrl = string.Empty;
                        try
                        {
                            SiteRedirectionUrl = Utils.getBetween(ArrDetail[1], "LinkshimAsyncLink.swap(this, &quot;", ");");
                        }
                        catch { };
                        try
                        {
                            SiteRedirectionUrl = Uri.UnescapeDataString(SiteRedirectionUrl).Replace("\\u0025", "%").Replace("\\", "");//4
                        }
                        catch { };
                        string websiteUrl = string.Empty;
                        try
                        {
                            websiteUrl = Utils.getBetween(SiteRedirectionUrl, "//", "/");
                        }
                        catch { };

                        string[] adImg = System.Text.RegularExpressions.Regex.Split(Value, "<img class=\"scaledImageFitWidth img\"");
                        string redirectionImg = string.Empty;
                        try
                        {
                            redirectionImg = Utils.getBetween(adImg[1], "src=\"", "\"").Replace("&amp;", "&");
                        }
                        catch { };

                        string[] profImg = System.Text.RegularExpressions.Regex.Split(Value, "<img class=\"_s0 5xib 5sq7 _rw img\"");
                        string profileImg = string.Empty;
                        try
                        {
                            profileImg = Utils.getBetween(profImg[0], "src=\"", "\"").Replace("&amp;", "&");
                        }
                        catch { };

                        string PostId = string.Empty;
                        try
                        {

                            PostId = Utils.getBetween(Value, "story_id=", "data-ft=").Replace("\"", string.Empty);
                        }
                        catch { };

                        HasTagData.Add("Title", Titles);
                        HasTagData.Add("Time", postedTime);

                        HasTagData.Add("Message", Message);

                        HasTagData.Add("Image", profileImg);

                        try
                        {
                            if (redirectionImg.Contains("/video/"))
                            {
                                HasTagData.Add("Type", "video");
                            }
                            else if (redirectionImg.Contains("/photo/"))
                            {
                                HasTagData.Add("Type", "image");
                            }
                            else
                            {
                                HasTagData.Add("Type", "link");
                            }
                        }
                        catch { };

                        HasTagData.Add("PostImage", redirectionImg);
                        HasTagData.Add("PostId", PostId);
                        HasTagData.Add("Link", redirectionHref);
                        HasTagData.Add("FromId", profileid);
                        HasTagData.Add("FromName", profileName);



                    }
                }
                catch { };
            }

            return HasTagData;
        }

        #endregion

    }
}
