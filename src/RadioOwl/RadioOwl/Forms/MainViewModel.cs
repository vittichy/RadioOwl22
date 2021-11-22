using Caliburn.Micro;
using Dtc.Http;
using RadioOwl.Helpers;
using RadioOwl.Parsers;
using RadioOwl.Parsers.Data;
using RadioOwl.Parsers.Data.Factory;
using RadioOwl.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RadioOwl.Forms
{
    /// <summary>
    /// Hlavni okno aplikace
    /// </summary>
    public class MainViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Seznam dostupných parserů
        /// </summary>
        private readonly ParserCollection _parsers = new ParserCollection();




        /// <summary>
        /// Hlavní set s pořady (master)
        /// </summary>
        public ObservableCollection<RadioData> RadioDataSet { get; set; } = new ObservableCollection<RadioData>();

        private RadioData _radioDataSelected;
        /// <summary>
        /// Selected pořad (v master datagridu)
        /// </summary>
        public RadioData RadioDataSelected
        {
            get { return _radioDataSelected; }
            set
            {
                _radioDataSelected = value;
                NotifyOfPropertyChange();
// TODO RefreshLogSet(value);
            }
        }
















        //public MainViewModel()
        //{


        //}


        /// <summary>
        /// Drag-drop preview event
        /// </summary>
        public void EventPreviewDragOver(DragEventArgs e)
        {
            var url = e.GetUnicodeData();
            var parserExists = _parsers.ExistsParser(url);
            e.Effects = parserExists ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Drag-drop event
        /// </summary>
        public void EventDrop(DragEventArgs e)
        {
            // nefunguje-li drag-drop nezoufej a pust visual studio jako uzivatel a ne pod adminem!
            var url = e.GetUnicodeData();
            ProcessUrl(url);
        }

        /// <summary>
        /// Event obdržení focusu na master datagridu --------- TODO nevypada, ze by event prichazel?
        /// </summary>
        public void DataGridMainGotFocus()
        {
            //            RefreshLogSet(RadioDataSelected);
        }


        public void DataGridMainMouseDoubleClick()
        {
            //            RefreshLogSet(RadioDataSelected);
        }

        




        /// <summary>
        /// Zahaji zpracovani url
        /// </summary>
        private void ProcessUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            // založení nového stahování
            var radioData = new RadioDataFactory().Create(url);
            RadioDataSet.Insert(0, radioData);
            RadioDataSelected = radioData;
            DataGridMainGotFocus();

            ProcessRadioData(radioData);
        }

        /// <summary>
        /// Zahájení zpracování RadioData řádku
        /// </summary>
        private async void ProcessRadioData(RadioData radioData)
        {
            if (radioData == null)
                return;

            // dohledání vhodného parseru stránky
            var parser = _parsers.FindParser(radioData.Url);
            if (parser == null)
            {
                radioData.AddLogError($"Nepodařilo se dohledat parser pro url: {radioData.Url}.");
                return;
            }

            // zkusím použít parser a rozpasovat
            radioData.State = FileRowState.Parse;
            var parseOk = await parser.ParseAsync(radioData);
            if (parseOk)
            {
                radioData.AddLog("Parser ok.");
                DownloadRadioData(radioData);
            }
            else
            {
                radioData.AddLogError("Parser error.");
            }
        }

        /// <summary>
        /// Zpracovani rozparsovanych dat - tj zde bych jiz mel znat odkazy na finalni mp3 a ty stahnu, ulozim, porezim filename, mp3id tagy atd
        /// </summary>
        private void DownloadRadioData(RadioData radioData)
        {
            radioData.State = FileRowState.Started;

            radioData.PartSet
                        .ToList()
                            .ForEach(p => new FileHelper().GenerateFilename(p));

            radioData.PartSet
                            .Where(p => p.UrlExists)
                                .ToList()
                                    .ForEach(async p => await ProcessDataPartAsync(p));
        }


        private async Task ProcessDataPartAsync(RadioDataPart radioDataPart)
        {
            if (File.Exists(radioDataPart.FileName))
            {
                radioDataPart.AddLog($"Soubor již existuje: {radioDataPart.FileName}.");
            }
            await DownloadPartAsync(radioDataPart);
        }



        private async Task DownloadPartAsync(RadioDataPart radioDataPart)
        {
            radioDataPart.AddLog($"Zahájení stahování: '{radioDataPart.Url}'");

            var asyncDownloader = new AsyncDownloader();
            var output = await asyncDownloader.GetData(radioDataPart.Url,
                                                       p =>
                                                       {

                                                           radioDataPart.Progress = p.ProgressPercentage;
                                                           radioDataPart.BytesReceived = p.BytesReceived;
                                                           radioDataPart.TotalBytesToReceive = p.TotalBytesToReceive;


                                                               ///??????       TotalProgress.UpdateProgress(RadioDataSet);
                                                           });
            if (output.DownloadOk)
            {
                Save(radioDataPart, output.Output);
            }
            else
            {
                radioDataPart.AddLog(string.Format("Chyba při stahování streamu: {0}.", output.Exception?.Message)); /////////, FileRowState.Error);
            }
        }



        private void Save(RadioDataPart radioDataPart, byte[] data)
        {
            var path = Path.GetDirectoryName(radioDataPart.FileName);
            Directory.CreateDirectory(path);

            using (var file = new FileStream(radioDataPart.FileName, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
            radioDataPart.Saved = true;
            radioDataPart.AddLog($"Uložen soubor: {radioDataPart.FileName}");


            // TODO add readme file?
            //if (!string.IsNullOrEmpty(fileRow.ReadMeText) && !string.IsNullOrEmpty(fileRow.ReadMeFileName))
            //    File.WriteAllText(fileRow.ReadMeFileName, fileRow.ReadMeText);
        }














        /// <summary>
        /// akce Načíst z URL
        /// </summary>
        public void OpenUrl()
        {
            // TODO zatim jen test stazeni - nahrada dropu
            var url = @"https://www.mujrozhlas.cz/cetba-na-pokracovani/alena-mornstajnova-listopad-ctou-jana-strykova-igor-bares-simona-postlerova";
            ProcessUrl(url);


            //var url = InputBoxViewModel.ExecuteModal("Načíst z URL", "URL:");
            //ProcessUrl(url);
        }









    }
}
