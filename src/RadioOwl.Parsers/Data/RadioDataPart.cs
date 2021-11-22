using Dtc.Common.Extensions;
using RadioOwl.Parsers.Data.Factory;
using RadioOwl.Parsers.Data.PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace RadioOwl.Parsers.Data
{
    /// <summary>
    /// Jedna část 
    /// </summary>
    public class RadioDataPart : PropertyChangedBase
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly RadioData RadioData;




        /// <summary>
        /// 
        /// </summary>
        public List<RadioLog> LogSet { get; set; } = new List<RadioLog>();

        private int _logSetSelectedIndex;
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
        /// Url části pořadu
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

        public bool UrlExists { get { return !string.IsNullOrEmpty(Url); } }


        private int? _partNo;
        /// <summary>
        ///
        /// </summary>
        public int? PartNo
        {
            get { return _partNo; }
            set
            {
                _partNo = value;
                OnPropertyChanged();
            }
        }


        private string _title;
        /// <summary>
        ///
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }


        private string _description;
        /// <summary>
        ///
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(() => Description, () => DescriptionSingleLine);
            }
        }


      //  private string _description;
        /// <summary>
        ///
        /// </summary>
        public string DescriptionSingleLine
        {
            get { return Description?.RemoveWhitespace(); }
            //set
            //{
            //    _description = value;
            //    OnPropertyChanged();
            //}
        }


















        private int _progress;
        /// <summary>
        /// Progres stahování
        /// </summary>
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }


        private long _bytesReceived;
        /// <summary>
        /// Stažená část souboru
        /// </summary>
        public long BytesReceived
        {
            get { return _bytesReceived; }
            set
            {
                _bytesReceived = value;
                OnPropertyChanged();
                RadioData.UpdateTotalProgress();
            }
        }

        private long _totalBytesToReceive;
        /// <summary>
        /// Velikost souboru ke stažení
        /// </summary>
        public long TotalBytesToReceive
        {
            get { return _totalBytesToReceive; }
            set
            {
                _totalBytesToReceive = value;
                OnPropertyChanged();
                RadioData.UpdateTotalProgress();
            }
        }

        


        public string ProgressPercent
        {
            get { return string.Format("{0}%  {1}", Progress, BytesReceived.ToFileSize()); }
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










        /// <summary>
        /// Kostruktor
        /// </summary>
        /// <param name="radioData"></param>
        public RadioDataPart(RadioData radioData)
        {
            RadioData = radioData ?? throw new ArgumentNullException(nameof(radioData));
        }



        public void AddLog(string msg, EventLevel eventLevel = EventLevel.Informational)
        {
            var radioLog = new RadioLogFactory().Create(msg, PartNo, eventLevel);
            LogSet.Add(radioLog);
            RadioData.LogSet.Add(radioLog); // pridavam i na master!
            LogSetSelectedIndex = LogSet.Count() - 1;
        }

        public void AddLogError(string msg)
        {
            AddLog(msg, EventLevel.Error);
        }
    }
}
