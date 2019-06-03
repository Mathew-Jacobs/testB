using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portlets.MVC.Models
{
    public class Location
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public List<string> BuildingCodes { get; set; }
        public NorthWestCoordinate NorthWestCoordinate { get; set; }
        public SouthEastCoordinate SouthEastCoordinate { get; set; }
        public string CampusLocation { get; set; }
        public List<string> AddressLines { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool HideInSelfServiceCourseSearch { get; set; }
        public object SortOrder { get; set; }
    }

    public class NorthWestCoordinate
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool OfficeUseOnly { get; set; }
    }

    public class SouthEastCoordinate
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool OfficeUseOnly { get; set; }
    }
}