using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMService2.Model
{
    public class Project
    {
        public string ProjectId {  get; set; }
        public DateTime ChreatedOn { get; set; }
        public string ChreatedBy { get; set; }
        public string Title { get; set; }
        public string ProjectManager { get; set; }
        public List<string> Assignees { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }


    }
}