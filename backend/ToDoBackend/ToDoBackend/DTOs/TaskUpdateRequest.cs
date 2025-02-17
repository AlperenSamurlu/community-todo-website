namespace ToDoBackend.DTOs
{
    public class TaskUpdateRequest
    {
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsIndividual { get; set; }
        public bool IsCompleted { get; set; }
        public int UpdatedByUserId { get; set; } // Güncelleme yapan kullanıcı
    }

}
