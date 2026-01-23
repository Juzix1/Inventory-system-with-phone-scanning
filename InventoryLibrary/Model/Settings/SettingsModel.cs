using System;

namespace InventoryWeb.Models;
 public class SettingsModel
    {
        public string FileStoragePath { get; set; } = "";
        public int MaxFileSizeMB { get; set; } = 10;
        public bool EnableNotifications { get; set; } = true;
        public string CompanyName { get; set; } = "";
        public int? ChosenDepartmentId { get; set; }
    }