//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TISS_JetLag.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AirportInfo
    {
        public int AirportID { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TimeZoneOffset { get; set; }
        public string Remark { get; set; }
    }
}
