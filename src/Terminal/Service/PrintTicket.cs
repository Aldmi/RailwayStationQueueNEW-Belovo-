﻿using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using Terminal.Settings;

namespace Terminal.Service
{
    public enum PrinterStatus
    {
        Ok,                                // Все хорошо
        QueueContainsElements,             // Очередь содержит элементы
        IsInError,                         // Ошибка принтера
        IsOutOfPaper,                      // Отсутсвует бумага
        IsPaperJammed                      // Замята бумага
    }


    public class PrintTicket : IDisposable
    {
        #region Field

        private readonly PrintDocument _printDocument;

        private string _ticketName;
        private string _countPeople;
        private DateTime _dateAdded;

        private readonly PrintServer _printServer;
        private readonly PrintQueue _printQueue;

        #endregion



        #region ctor

        public PrintTicket(string printerName)
        {
            var printersNames = PrinterSettings.InstalledPrinters;

           if(printersNames == null || printersNames.Count == 0)
               throw new Exception("ПРИНТЕРЫ НЕ НАЙДЕННЫ В СИСТЕМЕ");

            bool isFind = false;
            for (int i = 0; i < printersNames.Count; i++)
            {
                if (printersNames[i] == printerName)
                {
                    isFind = true;
                    break;
                }
            }
            if(!isFind)
                throw new Exception($"ПРИНТЕРА С ИМЕНЕМ {printerName} НЕ НАЙДЕННО В СИСТЕМЕ");

            PrinterSettings ps = new PrinterSettings {PrinterName = printerName};
            _printDocument = new PrintDocument {PrinterSettings = ps};
            _printDocument.PrintPage += Pd_PrintPage;

            _printServer = new PrintServer();
            _printQueue = _printServer.GetPrintQueues().FirstOrDefault(printer => printer.FullName == printerName);
            if(_printQueue == null)
                throw new Exception($"ПРИНТЕРА С ИМЕНЕМ {printerName} НЕ НАЙДЕННО В СИСТЕМЕ");
        }

        public PrintTicket(XmlPrinterSettings settings) : this(settings.PrinterName)
        {

        }

        #endregion




        #region Event

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            //ПЕЧАТЬ ЛОГОТИПА
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Picture", "RZD_logo.jpg");
            if (File.Exists(filePath))
                e.Graphics.DrawImage(Image.FromFile(filePath), 5, 5);

            e.Graphics.DrawLine(new Pen(Color.Black), 5, 130, 245, 130);

            //ПЕЧАТЬ ТЕКСТА
            string printText = $"{_ticketName}\r\n";
            Font printFont = new Font("Times New Roman", 20, FontStyle.Regular, GraphicsUnit.Millimeter);
            e.Graphics.DrawString(printText, printFont, Brushes.Black, 9, 150);

            printText = $"перед вами {_countPeople} чел.\r\n";
            printFont = new Font("Times New Roman", 7, FontStyle.Regular, GraphicsUnit.Millimeter);
            e.Graphics.DrawString(printText, printFont, Brushes.Black, 9, 260);

            printText = "\r\n \r\n ";
            printText += $"{_dateAdded.ToString("T")}         {_dateAdded.ToString("d")}";
            printFont = new Font("Times New Roman", 5, FontStyle.Regular, GraphicsUnit.Millimeter);
            e.Graphics.DrawString(printText, printFont, Brushes.Black, 5, 300);
        }

        #endregion




        #region Methode

        public PrinterStatus GetPrinterStatus()
        {
            var queue= _printQueue?.GetPrintJobInfoCollection();
            var count= queue?.Count();
            if (count > 0)
                return PrinterStatus.QueueContainsElements;

            if (_printQueue.IsInError)
                return PrinterStatus.IsInError;

            if (_printQueue.IsOutOfPaper)
                return PrinterStatus.IsOutOfPaper;

            if (_printQueue.IsPaperJammed)
                return PrinterStatus.IsOutOfPaper;

            return PrinterStatus.Ok;
        }


        public void Print(string ticketName, string countPeople, DateTime dateAdded)
        {
            _ticketName = ticketName;
            _countPeople = countPeople;
            _dateAdded = dateAdded;

            _printDocument.Print();
        }

        #endregion




        #region IDisposable

        public void Dispose()
        {
            _printDocument?.Dispose();
            _printServer?.Dispose();
            _printQueue?.Dispose();
        }

        #endregion
    }
}