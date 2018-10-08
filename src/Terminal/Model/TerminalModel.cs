using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Communication.Annotations;
using Communication.Settings;
using Communication.TcpIp;
using Library.Logs;
using Library.Xml;
using NLog;
using NLog.Config;
using NLog.Targets;
using Terminal.Infrastructure;
using Terminal.Service;
using Terminal.Settings;
using LogLevel = NLog.LogLevel;


namespace Terminal.Model
{
    public class TerminalModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Log _logger = new Log("Terminal.CommandAddItem");

        private readonly Logger _loggerRaw;//DEBUG


        public TerminalModel()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/logs/Log.txt",
                Layout = "${longdate} ${level} ${message} ${exception}"
            };
            config.AddTarget(fileTarget);

            config.AddRuleForAllLevels(fileTarget);
            LogManager.Configuration = config;

            _loggerRaw = LogManager.GetLogger("1111");
            _loggerRaw.Info("Create TerminalModel .........");
        }



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
            if (!IsConnectTcpIp)
            {
                _loggerRaw.Info("NotConnect");   //DEBUG
                return;
            }

            try
            {
                _loggerRaw.Info("ЗАПРОС О СОСТОЯНИИ ОЧЕРЕДИ");   //DEBUG

                //ЗАПРОС О СОСТОЯНИИ ОЧЕРЕДИ
                var provider = new Terminal2ServerExchangeDataProvider { InputData = new TerminalInData { NameQueue = nameQueue, PrefixQueue = prefixQueue, Action = TerminalAction.Info } };
                _loggerRaw.Info("RequestAndRespouneAsync");   //DEBUG
                await MasterTcpIp.RequestAndRespouneAsync(provider);

                //_logger.Info($"provider.IsOutDataValid=   {provider.IsOutDataValid}");
                _loggerRaw.Info($"provider.IsOutDataValid=  {provider.IsOutDataValid}");   //DEBUG

                if (provider.IsOutDataValid)
                {
                    var prefix = provider.OutputData.PrefixQueue;
                    var ticketName = prefix + provider.OutputData.NumberElement.ToString("000");
                    var countPeople = provider.OutputData.CountElement.ToString();

                    _loggerRaw.Info($"prefix= {prefix}  ticketName= {ticketName}  countPeople= {countPeople}");   //DEBUG

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

                            //_logger.Info($"PrintTicket: {provider.OutputData.AddedTime}     {ticketName}    nameQueue= {nameQueue}   descriptionQueue= {descriptionQueue}");
                            _loggerRaw.Info($"PrintTicket: {provider.OutputData.AddedTime}     {ticketName}    nameQueue= {nameQueue}   descriptionQueue= {descriptionQueue}");   //DEBUG
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
                //_logger.Error($"TerminalModel/QueueSelection()=   {ex}");
                _loggerRaw.Error($"provider.IsOutDataValid= {ex}");   //DEBUG
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