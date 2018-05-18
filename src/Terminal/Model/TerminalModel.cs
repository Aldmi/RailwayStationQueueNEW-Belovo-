﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Communication.Annotations;
using Communication.Settings;
using Communication.TcpIp;
using Library.Logs;
using Library.Xml;
using Terminal.Infrastructure;
using Terminal.Service;
using Terminal.Settings;


namespace Terminal.Model
{
    public class TerminalModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Log _logger = new Log("Terminal.CommandAddItem");



        #region prop

        public MasterTcpIp MasterTcpIp { get; set; }
        public PrintTicket PrintTicket { get; set; }

        public bool IsConnectTcpIp => (MasterTcpIp != null && MasterTcpIp.IsConnect);


        private string _errorString;
        public string ErrorString
        {
            get { return _errorString; }
            set
            {
                if (value == _errorString) return;
                _errorString = value;
                OnPropertyChanged();
            }
        }

        #endregion




        #region Events

        public event Func<string, string, string, bool> ConfirmationAdded;
        private bool OnConfirmationAdded(string arg1, string arg2, string arg3)
        {
            var res = ConfirmationAdded?.Invoke(arg1, arg2, arg3);
            return res != null && res.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion





        #region Methods

        public void LoadSetting()
        {
            XmlMasterSettings xmlTerminal;
            XmlPrinterSettings xmlPrinter;
            try
            {
                var xmlFile = XmlWorker.LoadXmlFile("Settings", "Setting.xml");
                if (xmlFile == null)
                    return;

                xmlTerminal = XmlMasterSettings.LoadXmlSetting(xmlFile);
                xmlPrinter = XmlPrinterSettings.LoadXmlSetting(xmlFile);
            }
            catch (FileNotFoundException ex)
            {
                ErrorString = ex.ToString();
                return;
            }
            catch (Exception ex)
            {
                ErrorString = "ОШИБКА в узлах дерева XML файла настроек:  " + ex;
                return;
            }

            
            try
            {
                MasterTcpIp = new MasterTcpIp(xmlTerminal);
                PrintTicket = new PrintTicket(xmlPrinter); 
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
            }
        }


        public async Task Start()
        {
            if (MasterTcpIp != null)
            {
                  await MasterTcpIp.ReConnect();
            }
        }


        public async Task QueueSelection(string nameQueue, string prefixQueue, string descriptionQueue)
        {
            if(!IsConnectTcpIp)
                return;

            try
            {
                //ЗАПРОС О СОСТОЯНИИ ОЧЕРЕДИ
                var provider = new Terminal2ServerExchangeDataProvider { InputData = new TerminalInData { NameQueue = nameQueue, PrefixQueue = prefixQueue, Action = TerminalAction.Info } };
                await MasterTcpIp.RequestAndRespouneAsync(provider);

                if (provider.IsOutDataValid)
                {
                    var prefix = provider.OutputData.PrefixQueue;
                    var ticketName = prefix + provider.OutputData.NumberElement.ToString("000");
                    var countPeople = provider.OutputData.CountElement.ToString();

                    var isAdded = OnConfirmationAdded(ticketName, countPeople, descriptionQueue);
                    if (isAdded)
                    {
                        //ЗАПРОС О ДОБАВЛЕНИИ ЭЛЕМЕНТА В ОЧЕРЕДЬ
                        provider = new Terminal2ServerExchangeDataProvider { InputData = new TerminalInData { NameQueue = nameQueue, PrefixQueue = prefixQueue, Action = TerminalAction.Add } };
                        await MasterTcpIp.RequestAndRespouneAsync(provider);

                        if (provider.IsOutDataValid)
                        {
                            prefix = provider.OutputData.PrefixQueue;
                            ticketName = prefix + provider.OutputData.NumberElement.ToString("000");
                            countPeople = provider.OutputData.CountElement.ToString();

                            PrintTicket.Print(ticketName, countPeople, provider.OutputData.AddedTime);

                            _logger.Info($"PrintTicket: {provider.OutputData.AddedTime}     {ticketName}    nameQueue= {nameQueue}   descriptionQueue= {descriptionQueue}");
                        }
                    }
                    else
                    {
                        // "НЕ добавлять"
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"TerminalModel/QueueSelection()=   {ex}");
            }
        }

        #endregion




        #region IDisposable

        public void Dispose()
        {
            //MasterTcpIp?.Dispose();
            PrintTicket?.Dispose();
        }

        #endregion
    }
}