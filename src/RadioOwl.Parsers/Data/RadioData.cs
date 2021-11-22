﻿using Dtc.Common.Extensions;
using RadioOwl.Parsers.Data.Factory;
using RadioOwl.Parsers.Data.PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Windows.Media;

namespace RadioOwl.Parsers.Data
{
    /// <summary>
    /// Řádek pro data o pořadu - má pod sebou jeden nebo více částí viz PartSet
    /// </summary>
    public class RadioData :  PropertyChangedBase
    {
        /// <summary>
        /// Části - jendotlivé díly pořadu a pod
        /// </summary>
        public ObservableCollection<RadioDataPart> PartSet { get; set; } = new ObservableCollection<RadioDataPart>();

        /// <summary>
        /// Seznam log hlášení - z jednotlivých částí přidávám hlášení i sem, takže by to měl být komplet seznam
        /// </summary>
        public List<RadioLog> LogSet { get; set; } = new List<RadioLog>();

        private int _logSetSelectedIndex;
        /// <summary>
        /// SelectedIndex z seznamu logů - jen pro pěkné zobrazování v lookupu logu v datagridu
        /// </summary>
        public int LogSetSelectedIndex
        {
            get { return _logSetSelectedIndex; }
            set
            {
                _logSetSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        private string _url;
        /// <summary>
        /// Hlavní url (to co bylo dotaženo přes drag-drop)
        /// </summary>
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Celková velikost všech částí
        /// </summary>
        public long TotalBytesToReceive { get { return PartSet.Sum(p => p.TotalBytesToReceive); } }

        /// <summary>
        /// Celková velikost všech částí - jako text (123MB a pod)
        /// </summary>
        public string TotalBytesToReceiveAsText { get { return TotalBytesToReceive.ToFileSize(); } }

        /// <summary>
        /// Celková velikost ze všech stažených částí
        /// </summary>
        public long BytesReceived { get { return PartSet.Sum(p => p.BytesReceived); } }

        /// <summary>
        /// Celková velikost ze všech stažených částí
        /// </summary>
        public string BytesReceivedAsText { get { return BytesReceived.ToFileSize(); } }

        /// <summary>
        /// Celkový total progres ze všech částí v %
        /// </summary>
        public string Progress { get { return BytesReceived.ToPercentFromTotal(TotalBytesToReceive); } }

        public string ProgressAsText { get { return string.Format("{0}%  {1}", Progress, BytesReceived.ToFileSize()); } }
























        /// <summary>
        /// Html kod pro url
        /// </summary>
        public string HtmlPage;





        ///// <summary>
        ///// 
        ///// </summary>
        //public List<RadioLog> LogWithPartsSet { get; set; } = new List<RadioLog>();






        private string _siteEntityBundle;
        /// <summary>
        /// 
        /// </summary>
        public string SiteEntityBundle
        {
            get { return _siteEntityBundle; }
            set
            {
                _siteEntityBundle = value;
                OnPropertyChanged();
            }
        }
        private string _siteEntityLabel;
        /// <summary>
        /// 
        /// </summary>
        public string SiteEntityLabel
        {
            get { return _siteEntityLabel; }
            set
            {
                _siteEntityLabel = value;
                OnPropertyChanged();
            }
        }
        private string _siteDocumentPath;
        /// <summary>
        /// 
        /// </summary>
        public string SiteDocumentPath
        {
            get { return _siteDocumentPath; }
            set
            {
                _siteDocumentPath = value;
                OnPropertyChanged();
            }
        }


        private int? _contentSerialAllParts;
        /// <summary>
        /// Počet dílů zjištěný parsováním
        /// </summary>
        public int? ContentSerialAllParts
        {
            get { return _contentSerialAllParts; }
            set
            {
                _contentSerialAllParts = value;
                OnPropertyChanged();
            }
        }








        private string _contentId;
        /// <summary>
        /// 
        /// </summary>
        public string ContentId
        {
            get { return _contentId; }
            set
            {
                _contentId = value;
                OnPropertyChanged();
            }
        }

















      

        /// <summary>
        /// TODO nepujde volat rovnou z jednotliv7ych casti?
        /// </summary>
        public void UpdateTotalProgress()
        {
            OnPropertyChanged(() => TotalBytesToReceive);
            OnPropertyChanged(() => BytesReceived);
            OnPropertyChanged(() => ProgressAsText);

            //   TotalBytesToReceive = PartSet.Sum(p => p.TotalBytesToReceive);
        }


     

































        private string _urlMp3Download;
        /// <summary>
        /// url for mp3 file
        /// </summary>
        public string UrlMp3Download
        {
            get { return _urlMp3Download; }
            set
            {
                _urlMp3Download = value;
                OnPropertyChanged();
            }
        }

        //private int? _urlMp3DownloadNo = null;
        //public int? UrlMp3DownloadNo
        //{
        //    get { return _urlMp3DownloadNo; }
        //    set
        //    {
        //        _urlMp3DownloadNo = value;
        //        OnPropertyChanged();
        //    }
        //}

        private string _id;
        // TODO delete?
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }


        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }


        private string _readMeFileName;
        public string ReadMeFileName
        {
            get { return _readMeFileName; }
            set
            {
                _readMeFileName = value;
                OnPropertyChanged();
            }
        }


        //private string _id3Name;
        //public string Id3Name
        //{
        //    get { return _id3Name; }
        //    set
        //    {
        //        _id3Name = value;
        //        OnPropertyChanged();
        //    }
        //}


        //private string _id3NamePart;
        //public string Id3NamePart
        //{
        //    get { return _id3NamePart; }
        //    set
        //    {
        //        _id3NamePart = value;
        //        OnPropertyChanged();
        //    }
        //}


        //private string _id3NameSite;
        //public string Id3NameSite
        //{
        //    get { return _id3NameSite; }
        //    set
        //    {
        //        _id3NameSite = value;
        //        OnPropertyChanged();
        //    }
        //}


        private FileRowState _state;
        public FileRowState State
        {
            get { return _state; }
            set
            {
                _state = value;
               // StateColor = new SolidColorBrush(SetStateColor());
                OnPropertyChanged();
                OnPropertyChanged("StateColor");
            }
        }


        private Brush _stateColor;
        /// <summary>
        /// barva se ve WPF binduje na Brush!
        /// </summary>
        public Brush StateColor
        {
            get
            {
                Color color;
                switch (State)
                {
                    case FileRowState.Parse:
                        color = Colors.Yellow;
                        break;
                    case FileRowState.Started:
                        color = Colors.Orange;
                        break;
                    case FileRowState.Finnished:
                        color = Colors.LightGreen;
                        break;
                    case FileRowState.Error:
                        color = Colors.Red;
                        break;
                    case FileRowState.AlreadyExists:
                        color = Colors.YellowGreen;
                        break;
                    default:
                        color = Colors.Blue;
                        break;
                }
                return new SolidColorBrush(color);




                //return _state;
            }
            // get { return _stateColor; }
            //set
            //{
            //    _stateColor = value;
            //    OnPropertyChanged();
            //}
        }


        //private int _progress;
        //public int Progress
        //{
        //    get { return _progress; }
        //    set
        //    {
        //        _progress = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged("ProgressPercent");
        //    }
        //}


        //private long _bytesReceived;
        //public long BytesReceived
        //{
        //    get { return _bytesReceived; }
        //    set
        //    {
        //        _bytesReceived = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged("ProgressPercent");
        //    }
        //}


        //public string ProgressPercent
        //{
        //    get { return string.Format("{0}%  {1}", Progress, BytesReceived.ToFileSize()); }
        //}


        //private List<string> _logList;
        //public List<string> LogList
        //{
        //    get { return _logList; }
        //    set
        //    {
        //        _logList = value;
        //        OnPropertyChanged();
        //    }
        //}


        //private int _logListIndex;
        //public int LogListIndex
        //{
        //    get { return _logListIndex; }
        //    set
        //    {
        //        _logListIndex = value;
        //        OnPropertyChanged();
        //    }
        //}

        private bool _saved;
        public bool Saved
        {
            get { return _saved; }
            set
            {
                _saved = value;
                OnPropertyChanged();
            }
        }






        private string _metaSiteName;
        public string MetaSiteName
        {
            get { return _metaSiteName; }
            set
            {
                _metaSiteName = value;
                OnPropertyChanged();
            }
        }

        private string _metaTitle;
        public string MetaTitle
        {
            get { return _metaTitle; }
            set
            {
                _metaTitle = value;
                OnPropertyChanged();
            }
        }

        private string _metaDescription;
        public string MetaDescription
        {
            get { return _metaDescription; }
            set
            {
                _metaDescription = value;
                OnPropertyChanged();
            }
        }

        private string _metaSubTitle;
        public string MetaSubTitle
        {
            get { return _metaSubTitle; }
            set
            {
                _metaSubTitle = value;
                OnPropertyChanged();
            }
        }

        private string _readMeText;
        public string ReadMeText
        {
            get { return _readMeText; }
            set
            {
                _readMeText = value;
                OnPropertyChanged();
            }
        }












     //   public RadioData(/*IList<FileRow> parentList,*/ string url)
     //   {
     // //      ParentList = parentList;
     //       State = FileRowState.Started;
     ////       LogList = new List<string>();
     //       Url = url;
     //   }

        //public FileRow(IList<FileRow> parentList, StreamUrlRow streamUrlRow) : this(parentList, streamUrlRow?.Url)
        //{
        //    Id3NamePart = streamUrlRow?.Title;
        //}


        /// <summary>
        /// Přidá záznam do logu
        /// </summary>
        public void AddLog(string msg, EventLevel eventLevel = EventLevel.Informational)
        {
            var radioLog = new RadioLogFactory().Create(msg, eventLevel);
            LogSet.Add(radioLog);
            LogSetSelectedIndex = LogSet.Count() - 1;
        }

        public void AddLogError(string msg)
        {
            AddLog(msg, EventLevel.Error);
        }














        public void AddLog(string log, FileRowState fileRowState)
        {
            AddLog(log);
            State = fileRowState;
        }


        
        private Color SetStateColor()
        {
            switch (State)   
            {
                case FileRowState.Parse:
                    return Colors.Yellow;
                case FileRowState.Started:
                    return Colors.Orange;
                case FileRowState.Finnished:
                    return Colors.LightGreen;
                case FileRowState.Error:
                    return Colors.Red;
                case FileRowState.AlreadyExists:
                    return Colors.YellowGreen;
                    //return Colors.SlateGray;
                default:
                    return Colors.Blue;
            }
        }










    }
}
