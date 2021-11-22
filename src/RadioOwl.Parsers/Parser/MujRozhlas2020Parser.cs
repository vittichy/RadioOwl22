using Dtc.Common.Extensions;
using Dtc.Html.Html;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RadioOwl.Parsers.Data;
using RadioOwl.Parsers.Data.Factory;
using RadioOwl.Parsers.Parser.Base;
using RadioOwl.Parsers.Parser.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RadioOwl.Parsers.Parser
{
    /// <summary>
    /// Parser pro stránku https://www.mujrozhlas.cz/
    /// 
    /// Postup:
    /// 1) stahnout html
    /// 2) dohledat 
    /// </summary>
    public class MujRozhlas2020Parser : PageParserBase
    {

        //public MujRozhlas2020Parser() //: base(nextParser)
        //{
        //}

        public override bool CanParse(string url)
        {
            return (url ?? string.Empty).ToLower().Contains("mujrozhlas.cz");
        }

        

        public override async Task<bool> ParseAsync(RadioData radioData)
        {
            //await base.ParseAsync(radioData);

            await DownloadHtmlAsync(radioData);

            //   var parserResult = new ParserResult();
            try
            {
                // html nemusi byt validni xml, takze je potreba pro parsovani pouzit Html Agility Pack
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(radioData.HtmlPage);

                //// hlavni title cele stranky - tj je-li vice dilu, tak spolecny nazev - nasledne bych ho mel pouzit pro folder kam budu ukladat
                //// mozna by slo pouzit i <h1>, ale zatim beru tohle:
                //// <meta property="og:title" content="Steinar Bragi: Planina" />
                //var metaTitle = htmlDoc.DocumentNode.SelectSingleNode(@"//meta[@property='og:title']");
                //if (metaTitle != null)
                //{
                //    parserResult.MetaTitle = metaTitle.Attributes.FirstOrDefault(p => p.Name == "content")?.Value?.Trim();
                //}
                //if (string.IsNullOrEmpty(parserResult.MetaTitle))
                //{
                //    parserResult.MetaTitle = "DESCRIPTION_NOT_FOUND";
                //}

                //// hlavni description cele stranky - asi nebude potreba? 
                //// <meta property="og:description" content="Sugestivní a výborně napsaný příběh o vině a nevinnosti, hranicích lidskosti a střetnutí s chladnou krutostí islandské přírody i vlastním nitrem – dílo, které v sobě spojuje prvky psychologického thrilleru a islandské lidové pověsti. Četbu na pokračování z románu současného islandského spisovatele poslouchejte on-line po dobu jednoho týdne po odvysílání." />
                //parserResult.MetaDescription = htmlDoc.DocumentNode.SelectSingleNode(@"//meta[@property='og:description']")
                //                                        ?.Attributes
                //                                            ?.FirstOrDefault(p => p.Name == "content")
                //                                                ?.Value
                //                                                    ?.Trim();

                //// <meta property="og:site_name" content="Radiožurnál" />
                //var siteNameItem = htmlDoc.DocumentNode.SelectSingleNode(@"//meta[@property='og:site_name']");
                //if (siteNameItem != null)
                //{
                //    parserResult.MetaSiteName = siteNameItem.Attributes.FirstOrDefault(p => p.Name == "content")?.Value?.Trim();
                //}
                //else
                //{
                //    parserResult.MetaSiteName = "UNKNOWN_SITE_NAME";
                //}

                //// readme o poradu
                //var divFieldBody = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@class='field body']");
                //if (divFieldBody != null)
                //{
                //    var pTextSet = divFieldBody.ChildNodes
                //                    .Where(p => p.Name?.Trim().ToUpper() == "P")
                //                        .Select(p => p.InnerText?.Trim())
                //                            .Where(p => !string.IsNullOrEmpty(p))
                //                                .ToList();
                //    parserResult.ReadMeText = string.Join(Environment.NewLine, pTextSet);
                //}

                //    / ajax / player / play / 1241302

                //   var partUrl22 = $@"https://www.mujrozhlas.cz//ajax/player/play/1241302";

                //var ss22 = DownloadHtml(partUrl22).Result;

                //var xxx22 = System.Net.WebUtility.HtmlDecode(ss22);

                // RID se zjisti je u linku ke konkretnimu poradu, napr url 'https://www.mujrozhlas.cz/lide/martin-c-putna' zadne RID nevraci!
                // rid=1239859"
                var rid = GetRID(radioData.HtmlPage);
                if (string.IsNullOrEmpty(rid))
                    throw new Exception("Nepodařilo se dohledat RID.");
                radioData.AddLog($"Dohledáno RID: {rid}");


                // zde asi jedine misto, kde zjistim pocet epizod?
                // dohledat <script> pod <div> kde je to jako kus JS zdrojaku s definovanou JSON promennou, kterou z toho zkusim vykousnou

                var mujRozhlas2020SiteInfo = GetContentSerialAllParts(htmlDoc);

                if (string.IsNullOrEmpty(mujRozhlas2020SiteInfo?.SiteEntityBundle))
                {
                    radioData.AddLog($"Nedohledáno SiteEntityBundle:'{mujRozhlas2020SiteInfo.ContentId}");
                }
                radioData.AddLog($"Dohledáno SiteEntityBundle:'{mujRozhlas2020SiteInfo.SiteEntityBundle}', ContentSerialAllParts:'{mujRozhlas2020SiteInfo.ContentSerialAllParts}'");

                SiteInfoToRadioData(radioData, mujRozhlas2020SiteInfo);

                switch (mujRozhlas2020SiteInfo.SiteEntityBundle)
                {
                    case "episode":
                        // neni serial, jen jednodilny porad, ContentSerialAllParts je null
                        await ParseEpisodeAsync(radioData, mujRozhlas2020SiteInfo);
                        break;
                    case "show":
                        // napr Spirituala https://www.mujrozhlas.cz/spirituala - stranka obsahuje jednotlive dily 
                        await ParseShowBundleAsync(radioData, mujRozhlas2020SiteInfo, rid);
                        break;
                    case "serial":
                        await ParseSerialAsync(radioData, rid, mujRozhlas2020SiteInfo);
                        break;
                    default:
                        throw new Exception($"Unknown SiteEntityBundle:'{mujRozhlas2020SiteInfo.SiteEntityBundle}'");
                }
                return true;
            }
            catch (Exception ex)
            {
                radioData.AddLogError($"ParseAsync error: {ex.Message}.");
                return false;
            }

            ///return parserResult;
        }

        private async Task ParseShowBundleAsync(RadioData radioData, MujRozhlas2020SiteInfo mujRozhlas2020SiteInfo, string rid)
        {

            // ziskat data pro seznam dalsich epizod poradu - stejny postup jako kdyz dam nacist dalsi na strance poradu
            // https://www.mujrozhlas.cz/ajax/ajax_list/show?page=1&size=9&id=show-577096&rid=577096
            var pageUrl = $@"https://www.mujrozhlas.cz/ajax/ajax_list/show?page=1&size=9&id=show-{rid}&rid={rid}";
            var jsonDataForPart = await DownloadHtmlAsync(pageUrl);
            var jsonDataForPartDecoded = System.Net.WebUtility.HtmlDecode(jsonDataForPart);
            var JsonForPart = JObject.Parse(jsonDataForPartDecoded);
            var jsonContetnForPart = JsonForPart.SelectToken($"$.snippets.show-{rid}.content");
            // zde mam html data pro nove nactene porady
            var jsonContetnForPartX = jsonContetnForPart?.Value<string>();
            // z nich musim vykousnout jednotlive uuid pro jednotlive mp3 dily
            var partHtml = new HtmlDocument();
            partHtml.LoadHtml(jsonContetnForPartX);
            // jedna se o seznam <article> tagu
            var articlePartSet = partHtml.DocumentNode.SelectNodes(@"//article[@class='b-episode']")?.ToList();
            if (articlePartSet == null)
                throw new Exception($"Article set  - nepodařilo se dohledat //article[@class='b-episode'].");
            if (articlePartSet == null)
                throw new Exception($"Article set - je prázdný.");
            // zajima mne data-entry atribut u jednotlivych <article>
            foreach (var articleTag in articlePartSet )
            {
                var dataEntryValue = articleTag
                                        .Attributes
                                            ?.FirstOrDefault(p => p.Name == "data-entry")
                                                ?.Value;

                var dataEntryValueEnc = System.Net.WebUtility.HtmlDecode(dataEntryValue);
                if (string.IsNullOrEmpty(dataEntryValueEnc))
                    throw new Exception($"Part  - hodnota dataEntryValueEnc je prázdná.");

                var jsonDataEntry = JObject.Parse(dataEntryValueEnc);

                // hlavni ID dílu seriálu
                var partUuid = jsonDataEntry.SelectToken("$.uuid")?.Value<string>();

            }




        }

        private async Task ParseSerialAsync(RadioData radioData, string rid, MujRozhlas2020SiteInfo mujRozhlas2020SiteInfo)
        {
            if (!mujRozhlas2020SiteInfo.ContentSerialAllParts.HasValue)
                throw new Exception($"Nepodařilo se dohledat ContentSerialAllParts, pro SiteEntityBundle:'{mujRozhlas2020SiteInfo.SiteEntityBundle}'.");

         

            for (var i = 0; i < mujRozhlas2020SiteInfo.ContentSerialAllParts.Value; i++)
            {
                radioData.AddLog($"Hledám link k části: {i}/{mujRozhlas2020SiteInfo.ContentSerialAllParts - 1}...");

                var partUrl = $@"https://www.mujrozhlas.cz/ajax/ajax_list/serial?page={i}&size=1&id=serial-{rid}&rid={rid}";

                // z api dostanu json s html daty pro konkrétní část pořadu
                var jsonDataForPart = await DownloadHtmlAsync(partUrl);
                var jsonDataForPartDecoded = System.Net.WebUtility.HtmlDecode(jsonDataForPart);




                //        snippets / serial{ }/content

                //JToken acme = JsonForPart.SelectToken("$.snippets");
                //JToken acme2 = JsonForPart.SelectToken($"$.snippets.serial-{rid}");

                // api vraci json, kde musim najit element na ceste snippets.serial-{rid}.content a v nem je kus html, kde by melo vy dohledat finalni mp3 atd

                // parsovani JSONu xpath like: https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm

                var JsonForPart = JObject.Parse(jsonDataForPartDecoded);
                var jsonContetnForPart = JsonForPart.SelectToken($"$.snippets.serial-{rid}.content");

                var jsonContetnForPartX = jsonContetnForPart?.Value<string>();

                // v tomto html kousku najit <article class="b-episode" data-entry='

                var partHtml = new HtmlDocument();
                partHtml.LoadHtml(jsonContetnForPartX);


                var liForPart = partHtml.DocumentNode.SelectNodes(@"//article[@class='b-episode']")?.FirstOrDefault();
                if (liForPart == null)
                    throw new Exception($"Part {i} - nepodařilo se dohledat //article[@class='b-episode'].");
                var dataEntryValue = liForPart
                                                     .Attributes
                                                         ?.FirstOrDefault(p => p.Name == "data-entry")
                                                             ?.Value;

                var dataEntryValueEnc = System.Net.WebUtility.HtmlDecode(dataEntryValue);
                if (string.IsNullOrEmpty(dataEntryValueEnc))
                    throw new Exception($"Part {i} - hodnota dataEntryValueEnc je prázdná.");

                var jsonDataEntry = JObject.Parse(dataEntryValueEnc);

                // hlavni ID dílu seriálu
                var partUuid = jsonDataEntry.SelectToken("$.uuid")?.Value<string>();

                // ted mam UUID casti poradu - pres API se dostanu k mp3 ... snad :-)

                //https://api.mujrozhlas.cz/episodes/6d34cb00-fb37-3aaf-8037-6c2c5ba0deb0

                var urlPart = $@"https://api.mujrozhlas.cz/episodes/{partUuid}";
                var urlPartInfo = await DownloadHtmlAsync(urlPart);

                //       var urlPartInfo2 = System.Net.WebUtility.HtmlDecode(urlPartInfo);

                // unescapovat unicode znaky typu "\u003Cp\u003E\u010cetbu na pokra\u010dov\u00e1n\u00ed ze..."
                // viz https://stackoverflow.com/questions/9303257/how-to-decode-a-unicode-character-in-a-string
                var urlPartInfoUnescaped = System.Text.RegularExpressions.Regex.Unescape(urlPartInfo);



                // ZDE UZ MAM VSECHNY INFORMACE ... mp3 link, nazvy atd!


                // faq dotazy na json: https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm

                var partJson = JObject.Parse(urlPartInfoUnescaped);
                // guid
                var partDataId = partJson.SelectToken("$.data.id")?.Value<string>();
                // melo bybyt episode?
                var partDataType = partJson.SelectToken("$.data.type")?.Value<string>();

                // 
                var partNo = partJson.SelectToken("$.data.attributes.part")?.Value<int>();
                // 
                var partTitle = partJson.SelectToken("$.data.attributes.title")?.Value<string>();
                //
                var partShortTitle = partJson.SelectToken("$.data.attributes.shortTitle")?.Value<string>();
                // 
                var partDescriptionHtml = partJson.SelectToken("$.data.attributes.description")?.Value<string>();
                var partDescription = new HtmlHelper().StripHtmlTags(partDescriptionHtml);

                // samotne url k mp3
                var partAudioLink = partJson.SelectToken("$.data.attributes.audioLinks[0].url")?.Value<string>();


                new RadioDataPartFactory().Create(radioData, partNo, partTitle, partDescription, partAudioLink);


                //radioData.AddLog($"Ok.");
            }
        }

        private static void SiteInfoToRadioData(RadioData radioData, MujRozhlas2020SiteInfo mujRozhlas2020SiteInfo)
        {
            radioData.SiteDocumentPath = mujRozhlas2020SiteInfo.SiteDocumentPath;
            radioData.SiteEntityBundle = mujRozhlas2020SiteInfo.SiteEntityBundle;
            radioData.SiteEntityLabel = mujRozhlas2020SiteInfo.SiteEntityLabel;
            radioData.ContentSerialAllParts = mujRozhlas2020SiteInfo.ContentSerialAllParts;
            radioData.ContentId = mujRozhlas2020SiteInfo.ContentId;
        }

        private async Task ParseEpisodeAsync(RadioData radioData, MujRozhlas2020SiteInfo mujRozhlas2020SiteInfo)
        {
            if (radioData is null) throw new ArgumentNullException(nameof(radioData));
            if (mujRozhlas2020SiteInfo is null) throw new ArgumentNullException(nameof(mujRozhlas2020SiteInfo));

            if (mujRozhlas2020SiteInfo.SiteEntityBundle != "episode")
                throw new NotSupportedException("Pouze pro typ pořadu bundle=episode!");

            var episodeJsonUrl = $@"https://api.mujrozhlas.cz/episodes/{mujRozhlas2020SiteInfo.ContentId}";


            var episodeJsonCode = await DownloadHtmlAsync(episodeJsonUrl);

            // viz https://stackoverflow.com/questions/9303257/how-to-decode-a-unicode-character-in-a-string
           // var urlPartInfoUnescaped = System.Text.RegularExpressions.Regex.Unescape(urlPartInfo);

            var episodeJson = JObject.Parse(episodeJsonCode);

            if(episodeJson != null)
            {
              //  var episodeMp3 = episodeJson.SelectToken("$.data.audioLinks[0].url")?.Value<string>();

                var partNo = 0; // jen jedn epizoda  
                // 
                var partTitle = episodeJson.SelectToken("$.data.attributes.title")?.Value<string>();
                //
                var partShortTitle = episodeJson.SelectToken("$.data.attributes.shortTitle")?.Value<string>();
                // 
                var partDescriptionHtml = episodeJson.SelectToken("$.data.attributes.description")?.Value<string>();
                var partDescription = new HtmlHelper().StripHtmlTags(partDescriptionHtml);
                // samotne url k mp3
                var partAudioLink = episodeJson.SelectToken("$.data.attributes.audioLinks[0].url")?.Value<string>();


                new RadioDataPartFactory().Create(radioData, partNo, partTitle, partDescription, partAudioLink);

            }

        }


        /// <summary>
        /// zde asi jedine misto, kde zjistim pocet epizod?
        /// dohledat <script> pod <div> kde je to jako kus JS zdrojaku s definovanou JSON promennou, kterou z toho zkusim vykousnou
        /// </summary>
        private MujRozhlas2020SiteInfo GetContentSerialAllParts(HtmlDocument htmlDoc)
        {
        //    "siteEntityBundle":"show"


            var divPageMain = htmlDoc.DocumentNode.SelectNodes(@".//div[@class='page__main']//script");
            if (divPageMain == null)
                throw new Exception("Chyba při parsování html - nepodařilo se dohledat seznam 'div[@class='page__main']'.");
            if (!divPageMain.Any())
                throw new Exception("Chyba při parsování html - podařilo se dohledat seznam 'article[@class='b-episode']' - seznam je však prázdný.");

            var pageMainScript = divPageMain.FirstOrDefault().InnerHtml?.SubstrFromToChar('{', '}');

            var mainJson = System.Net.WebUtility.HtmlDecode(pageMainScript);
            var jMainJson = JObject.Parse(mainJson);

            //            var contentSerialAllParts = jMainJson.SelectToken("contentSerialAllParts")?.Value<int>();
            //[JSON].siteEntityBundle
            
            
            var mujRozhlas2020SiteInfo = new MujRozhlas2020SiteInfo
            {
                ContentSerialAllParts = jMainJson.SelectToken("contentSerialAllParts")?.Value<int>(),
                SiteEntityBundle = jMainJson.SelectToken("siteEntityBundle")?.Value<string>(),
                SiteEntityLabel = jMainJson.SelectToken("siteEntityLabel")?.Value<string>(),
                SiteDocumentPath = jMainJson.SelectToken("siteDocumentPath")?.Value<string>(),
                ContentId = jMainJson.SelectToken("contentId")?.Value<string>(),
            };



            return mujRozhlas2020SiteInfo;
        }


        /// <summary>
        /// Potrebuju zjistit RID identifaktor poradu 
        /// - bohuzel nejde vykousnout z nejakeho jsonu atd, musim dohledat v parametru ajax volani z cele html stranky
        /// 
        /// napr:
        /// <input type="checkbox" class="checkbox__control" id="" name="" checked="checked" data-ajax="/ajax/ajax_list_redraw/serial?size=9&amp;id=serial-1239859&amp;rid=1239859">
        /// <a href="https://www.mujrozhlas.cz/ajax/ajax_list/serial?page=1&amp;size=9&amp;id=serial-1239859&amp;rid=1239859" class="more-link__link ajax">
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetRID(string html)
        {
            if (string.IsNullOrEmpty(html)) 
                return null;
            // někde je problém s crlf a pod
            html = html.Replace("\n", "").Replace("\r", "");
            var htmlParts = html.Split(new string[] { "rid=" }, StringSplitOptions.RemoveEmptyEntries);
            if (htmlParts.Length > 1)
            {
                // prvni token je zacatek html, ten mne nezajima
                for (int i = 1; i < htmlParts.Length; i++)
                {
                    // var rid = htmlParts[i]?.Trim()?.SubstrTo('"')?.Trim();

                    var rid = ReadNumber(htmlParts[i]);
                    if (!string.IsNullOrEmpty(rid))
                        return rid;
                }
            }
            return null;
        }


        private string ReadNumber(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            var result = string.Empty;
            for(var i = 0; i < s.Length; i++)
            {
                if (!char.IsDigit(s[i]))
                    break;
                result += s[i];
            }
            return result;
        }

        //private void ParseXXX(HtmlNode liNode)
        //{
        //    // <article class="b-episode" data-entry="

        //    var a1 = liNode.SelectNodes(@".//article[@class='b-episode']");

        //    var a2 = liNode.SelectNodes(@".////article[@class='b-episode']");

        //    var a3 = liNode.SelectNodes(@".////article[@class='b-episode']");

        //    var a4 = liNode.SelectNodes(@".////article[@class='b-episode']");

        //    var a5 = liNode.SelectNodes(@".////article[@class='b-episode']");

        //    var a6 = liNode.SelectNodes(@".////article[@class='b-episode']");



        //}
    }
}
