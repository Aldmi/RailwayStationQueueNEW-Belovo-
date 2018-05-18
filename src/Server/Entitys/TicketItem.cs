﻿using System;

namespace Server.Entitys
{
    [Serializable]
    public class TicketItem
    {
        public string Prefix { get; set; }          // строковый префикс элемента
        public uint NumberElement { get; set; }     // номер в очереди на момент добавления
        public ushort CountElement { get; set; }    // кол-во клиентов в очереди на момент добавления
        public DateTime AddedTime{ get; set; }      // дата добавления
        public int? Сashbox { get; set; }           // номер кассира
        public byte CountTryHandling { get; set; }  // количество попыток обработки этого билета кассиром
        public int Priority { get; set; }           // приоритет билета в очереди

        public override string ToString()
        {
            var ticketName = Prefix + NumberElement.ToString("000");
            return $" Дата добавления в очередь: {AddedTime}      Дата поступления в обработку: {DateTime.Now}      Номер билета: {ticketName}      Номер кассира:   {  Сashbox?.ToString() ?? "неизвестный кассир" } ";
        }
    }
}