using System;
using System.Collections.Generic;
using System.Text;

namespace Inframonitor.Api.Models;

public class LogEvento()
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    public string Servidor { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
