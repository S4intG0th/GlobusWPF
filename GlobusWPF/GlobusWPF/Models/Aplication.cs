using System;

public class Aplication
{
    public int AplicationId { get; set; }
    public int TourId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } // Добавляем это свойство
    public string TourName { get; set; }
    public DateTime AplicationDate { get; set; }
    public string Status { get; set; }
    public int PeopleCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Comment { get; set; }

    public Aplication()
    {
        ClientName = string.Empty;
        TourName = string.Empty;
        Status = string.Empty;
        Comment = string.Empty;
        AplicationDate = DateTime.Now;
    }
}