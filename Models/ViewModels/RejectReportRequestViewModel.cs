namespace medicalapp.Models.ViewModels
{
    public class RejectReportRequestViewModel
    {
        public int RequestId { get; set; }
        public string PatientName { get; set; }
        public string Reason { get; set; }
    }
}