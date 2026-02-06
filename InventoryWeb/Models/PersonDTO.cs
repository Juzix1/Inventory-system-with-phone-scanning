using System;

namespace InventoryWeb.Models;

public class PersonDto
    {
        public int Id { get; set; }
        public string IndexNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }